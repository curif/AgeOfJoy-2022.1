/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using UnityEngine;
using LC = LibretroControlMapDictionnary;

// FlycastCore — cabinet-lifecycle driver for the Flycast core on its own Vulkan device (libpdlr),
// the hardware-rendered counterpart of LibretroMameCore. A cabinet selects it with `core: flycast`
// in description.yaml; LibretroScreenController branches to this class instead of LibretroMameCore.
//
// The emulated frame reaches the cabinet CRT zero-copy: the core renders on its own VkDevice, the
// frame is blitted into AHardwareBuffer-backed images, imported once onto Unity's device on the
// render thread, and wrapped in external Texture2Ds that are swapped into the screen shader's
// material each frame (no per-frame CPU copy). Falls back automatically to a CPU read-back
// Texture2D when the AHB extensions are unavailable. Ported from the device-proven PdFlycast
// debug driver (which stays as-is for quad testing).
//
// One game at a time, same as LibretroMameCore (libpdlr hosts a single core instance).
public static class FlycastCore
{
    public const string CoreName = "flycast";
    public const string CoreLibFileName = "libflycast_libretro_android.so";
    // On-device content subfolder for this core's games + BIOS. Named "dc" to match Flycast's own
    // forced <systemDir>/dc BIOS folder — the RetroArch-standard flycast layout — so the naming is
    // consistent across both roots: BIOS in system/dc/, games in downloads/dc/.
    public const string ContentDirName = "dc";

    // Set by LibretroScreenController before Start(), mirroring LibretroMameCore statics.
    public static ShaderScreenBase Shader;
    public static LibretroControlMap ControlMap;
    public static CoinSlotController CoinSlot;
    public static LightGunTarget lightGunTarget;   // null unless the cabinet declares light-gun
    public static bool AnalogStick;                // route thumbstick → DC analog stick + triggers (racing cabinets)
    public static CoreEnvironment CabEnvironment;  // per-cabinet core-option overrides (description.yaml `environment:`)

    // VR display refresh the native pump phase-locks to (rooms present at 72 Hz).
    public static float DisplayHz = 72f;

    public static bool GameLoaded { get; private set; }
    public static string GameFileName { get; private set; } = "";
    public static string ScreenName { get; private set; } = "";

    static bool zeroCopy;             // transport actually in use this run
    static float tickHz;              // fallback retro_run rate (core fps; native pump normally drives)
    static float tickAccum;
    static Texture2D tex;             // CPU-path frame texture
    static Texture2D[] zcTexes;       // zero-copy: one external texture per AHB buffer
    static bool importIssued;
    static bool extTexReady;
    static int bufW, bufH;            // fixed AHB/external-texture size (the ceiling)
    static int cropActiveW = -1, cropActiveH = -1;
    static int lastBoundIdx = -1;
    static int coinFrames;            // frames left to hold the SELECT (coin) bit
    static float nextStatusAt;

    // Resolve the game file the same way LibretroMameCore.getPath does: per-core dir first
    // (downloads/dc/), then the downloads root as a fallback.
    public static string getPath(string gameFileName)
    {
        string path = ConfigManager.RomsDir + "/" + ContentDirName + "/" + gameFileName;
        if (!File.Exists(path))
            path = ConfigManager.RomsDir + "/" + gameFileName;
        if (!File.Exists(path))
        {
            ConfigManager.WriteConsoleError($"[FlycastCore] game not found: {ConfigManager.RomsDir}/{ContentDirName}/{gameFileName}");
            return null;
        }
        return path;
    }

    public static bool Start(string screenName, string gameFileName)
    {
        if (GameLoaded || !string.IsNullOrEmpty(GameFileName))
        {
            ConfigManager.WriteConsoleError($"[FlycastCore.Start] a game is already loaded ({GameFileName} in {ScreenName}); End() first");
            return false;
        }

        string gamePath = getPath(gameFileName);
        if (gamePath == null)
            return false;

        string corePath = Path.Combine(PdLibretro.NativeLibraryDir(), CoreLibFileName);

        // BIOS lives in AoJ's shared system folder (ConfigManager.SystemDir, the same root the
        // software cores use). Flycast forces a "/dc" subdir on its system dir and reads BIOS + nvmem
        // there, so pointing it at SystemDir yields the RetroArch-standard system/dc/ layout. VMU +
        // savestates go to the save dir we pass. Net on-device layout:
        //   BIOS + nvmem     : <BaseDir>/system/dc/     (dc_boot.bin, dc_flash.bin,
        //                                                naomi.zip/awbios.zip/…, dc_nvmem.bin)
        //   VMU + savestates : <BaseDir>/system/dc/saves/
        string sysDir = ConfigManager.SystemDir;
        string saveDir = Path.Combine(sysDir, ContentDirName, "saves");
        try { Directory.CreateDirectory(saveDir); } catch { }

        ConfigManager.WriteConsole($"[FlycastCore.Start] core='{corePath}' sys='{sysDir}' game='{gamePath}'");

        // Gun cabinet: declare port 0 LIGHTGUN before Start() — Flycast builds its maple bus at
        // load. Ports 1-3 stay JOYPAD (the 4-pad maple parity that gates NAOMI audio init).
        if (lightGunTarget != null && lightGunTarget.Initialized())
            PdLibretro.SetPortDevice(0, PdLibretro.DEVICE_LIGHTGUN);

        // Per-cabinet core-option overrides (description.yaml `environment:`) — must be pushed before
        // Start()/retro_load_game. Layered on top of the global Flycast.opt safe defaults inside
        // libpdlr: YAML wins, any option it doesn't name keeps its default. Full key = prefix + "_" +
        // property name (matching the software core), e.g. prefix "reicast" + "broadcast" → reicast_broadcast.
        if (CabEnvironment?.properties != null)
        {
            foreach (var kv in CabEnvironment.properties)
            {
                string key = string.IsNullOrEmpty(CabEnvironment.prefix) ? kv.Key : $"{CabEnvironment.prefix}_{kv.Key}";
                ConfigManager.WriteConsole($"[FlycastCore.Start] core-option override: {key} = {kv.Value}");
                PdLibretro.SetOption(key, kv.Value);
            }
        }

        // Must be set before Start() — it decides which device extensions the core is asked to enable.
        PdLibretro.SetZeroCopy(true);
        if (!PdLibretro.Start(corePath, sysDir, saveDir, gamePath))
        {
            ConfigManager.WriteConsoleError($"[FlycastCore.Start] pdlr_start FAILED — Available={PdLibretro.Available} preload='{PdLibretro.PreloadInfo}' lastError='{PdLibretro.LastError}'");
            return false;
        }
        zeroCopy = PdLibretro.ZeroCopyActive;
        if (!zeroCopy)
            ConfigManager.WriteConsole("[FlycastCore.Start] zero-copy unavailable on the core's device — using CPU read-back path");

        // Phase-lock the native pump to the VR display cadence; set after Start().
        // Pull the live headset refresh rather than trusting the 72 default — a Quest 3 (or a
        // future model) may present at 90/120, and the pump's emulated:display pulldown ratio must
        // match the real rate or ~60 Hz DC content judders. Getter returns 0 on failure → keep default.
        float liveHz = OVRPlugin.systemDisplayFrequency;
        if (liveHz > 1f)
            DisplayHz = liveHz;
        ConfigManager.WriteConsole($"[FlycastCore.Start] display refresh = {DisplayHz:F1} Hz (live={liveHz:F1})");
        PdLibretro.SetDisplayHz(DisplayHz);
        PdLibretro.SetAudioOutputRate(AudioSettings.outputSampleRate);

        double coreFps = PdLibretro.FrameFps;
        tickHz = coreFps > 1.0 ? (float)coreFps : 60f;

        GameLoaded = true;
        GameFileName = gameFileName;
        ScreenName = screenName;
        ConfigManager.WriteConsole($"[FlycastCore.Start] running {gameFileName} in {screenName} zeroCopy={zeroCopy} coreFps={coreFps:F3} sampleRate={PdLibretro.SampleRate:F0}");
        return true;
    }

    public static bool isRunning(string screenName, string gameFileName)
    {
        return GameLoaded && ScreenName == screenName && GameFileName == gameFileName;
    }

    // Suspend/resume the native emu pump (headset off / system overlay).
    public static void SetPaused(bool paused)
    {
        if (GameLoaded)
            PdLibretro.SetPaused(paused);
    }

    // Called once per rendered frame from LibretroScreenController.Update while running.
    public static void Update()
    {
        if (!GameLoaded)
            return;

        // One tick per rendered VR frame: the pump paces retro_run off this (display-locked cadence).
        PdLibretro.NotifyDisplayFrame();

        PollInput();

        // Fallback driver: a NO-OP while the native pump thread is active. Only ticks the core if the
        // pump's pthread_create failed at start. Accumulate elapsed time, capped (no catch-up spiral).
        float period = tickHz > 0f ? 1f / tickHz : 1f / 60f;
        tickAccum += Time.unscaledDeltaTime;
        int ticks = 0;
        while (tickAccum >= period && ticks < 4) { tickAccum -= period; ticks++; }
        if (tickAccum > period) tickAccum = 0f;
        for (int i = 0; i < ticks; i++)
            PdLibretro.Run();

        if (zeroCopy)
            UpdateZeroCopy();
        else if (PdLibretro.GetFrame(out IntPtr pixels, out int w, out int h) && pixels != IntPtr.Zero && w > 0 && h > 0)
            BlitCpu(pixels, w, h);

        if (Time.unscaledTime >= nextStatusAt)
        {
            nextStatusAt = Time.unscaledTime + 5f;
            ConfigManager.WriteConsole($"[FlycastCore] frames={PdLibretro.FrameCount} zc={zeroCopy} extTex={extTexReady} readyIdx={(zeroCopy ? PdLibretro.ReadyBufferIndex : -1)}");
        }
    }

    // Zero-copy: once the first blit completed (ReadyBufferIndex >= 0 guarantees the AHB buffers
    // exist), issue the render-thread import event each frame until it succeeds (idempotent
    // native-side; a one-shot latch races the pump). Then wrap each imported VkImage in an external
    // Texture2D and per-frame just bind the buffer the native side most recently blitted.
    static void UpdateZeroCopy()
    {
        if (!extTexReady)
        {
            if (PdLibretro.ReadyBufferIndex >= 0 && !PdLibretro.UnityImagesReady)
            {
                IntPtr fn = PdLibretro.GetRenderEventFunc();
                if (fn != IntPtr.Zero)
                {
                    GL.IssuePluginEvent(fn, PdLibretro.EVENT_IMPORT_AHB);
                    if (!importIssued) { importIssued = true; ConfigManager.WriteConsole("[FlycastCore] zero-copy: issued AHB import event"); }
                }
            }
            if (PdLibretro.UnityImagesReady)
                CreateExternalTextures();
            return;
        }

        int idx = PdLibretro.ReadyBufferIndex;
        if (idx >= 0 && idx < zcTexes.Length && zcTexes[idx] != null && idx != lastBoundIdx)
        {
            BindTexture(zcTexes[idx], false);
            lastBoundIdx = idx;
        }

        // The active frame can change mid-run (NAOMI/AW resolution swaps). Re-crop when it does —
        // the buffer size is fixed, so the external textures never need rebuilding.
        if (bufW > 0 && PdLibretro.FrameSize(out int aw, out int ah) && aw > 0 && ah > 0 &&
            (aw != cropActiveW || ah != cropActiveH))
            ApplyZeroCopyCrop(aw, ah);
    }

    // Build one external Texture2D per AHB buffer, sized to the FIXED buffer (ceiling) — the active
    // frame is a sub-rect handled by the UV crop (the AHB import is one-shot).
    static void CreateExternalTextures()
    {
        if (!PdLibretro.BufferSize(out int bw, out int bh) || bw <= 0 || bh <= 0) return;
        int n = PdLibretro.BufferCount;
        if (n <= 0) return;

        bufW = bw; bufH = bh;
        zcTexes = new Texture2D[n];
        for (int i = 0; i < n; i++)
        {
            IntPtr img = PdLibretro.GetUnityImagePtr(i);
            if (img == IntPtr.Zero)
            {
                ConfigManager.WriteConsoleError($"[FlycastCore] zero-copy: buffer {i} image ptr null — abort");
                zcTexes = null;
                return;
            }
            // linear:true (no hardware sRGB view): the AoJ screen shaders decode gamma themselves
            // (pow 2.0 in SHA_CRT_01) because MAME's RGB565 texture samples raw. An sRGB view here
            // decodes a second time → dark, crushed colors.
            var t = Texture2D.CreateExternalTexture(bw, bh, TextureFormat.RGBA32, false, true, img);
            // Repeat, NOT Clamp: the screen shaders (e.g. SHA_CRT_01) invert via negative _CRTTiling
            // with no offset, relying on wrap-around — MAME's GameTexture defaults to Repeat. Clamp
            // collapses the flipped axis to a single edge row stretched across the screen.
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            t.name = $"FlycastAHB{i}";
            zcTexes[i] = t;
        }
        BindTexture(zcTexes[0], true);
        lastBoundIdx = 0;
        int aw = bw, ah = bh;
        PdLibretro.FrameSize(out aw, out ah);
        ApplyZeroCopyCrop(aw, ah);
        extTexReady = true;
        ConfigManager.WriteConsole($"[FlycastCore] zero-copy: {n} external textures bound buffer={bw}x{bh} active={aw}x{ah}");
    }

    // Map the screen's UV range onto the active sub-rect of the fixed AHB buffer via the material's
    // texture ST. Device-verified convention (PdFlycast): the Vulkan external texture samples with V
    // flipped (content at top-left), so the active band is U∈[0,au], V∈[1-av,1]. Orientation flips
    // stay in the screen shader's own invert path (YAML crt.screen.invertx/inverty). Identity when
    // active == buffer (e.g. VF3 at 640x480), so full-frame titles are unaffected even on shaders
    // that ignore the ST transform.
    static void ApplyZeroCopyCrop(int activeW, int activeH)
    {
        Material mat = Shader?.ScreenMaterial;
        if (mat == null) return;
        float au = bufW > 0 ? Mathf.Clamp01((float)activeW / bufW) : 1f;
        float av = bufH > 0 ? Mathf.Clamp01((float)activeH / bufH) : 1f;
        if (au <= 0f) au = 1f;
        if (av <= 0f) av = 1f;
        mat.SetTextureScale(Shader.TargetMaterialProperty, new Vector2(au, av));
        mat.SetTextureOffset(Shader.TargetMaterialProperty, new Vector2(0f, 1f - av));
        cropActiveW = activeW; cropActiveH = activeH;
        ConfigManager.WriteConsole($"[FlycastCore] zero-copy crop: active={activeW}x{activeH} buffer={bufW}x{bufH}");
    }

    static void BlitCpu(IntPtr pixels, int w, int h)
    {
        if (tex == null || tex.width != w || tex.height != h)
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
            // Flycast set_image is R8G8B8A8_UNORM → RGBA32. Repeat wrap + linear: see CreateExternalTextures.
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name = "FlycastFrame",
            };
            BindTexture(tex, true);
        }
        tex.LoadRawTextureData(pixels, w * h * 4);
        tex.Apply(false, false);
    }

    // firstBind goes through the shader's Texture setter (it logs, and children may do extra
    // bookkeeping); the per-frame zero-copy swap writes the material property directly.
    static void BindTexture(Texture t, bool firstBind)
    {
        if (Shader == null) return;
        if (firstBind)
            Shader.Texture = t;
        else
            Shader.ScreenMaterial?.SetTexture(Shader.TargetMaterialProperty, t);
    }

    // Push the cabinet's mapped controls to the core before its next retro_run: the RetroPad bitmask
    // (bit N == RETRO_DEVICE_ID_JOYPAD_N) read from the same LibretroControlMap the software path
    // uses. Coin: a taken coin (or the INSERT control) holds the SELECT bit a few frames — flycast
    // maps SELECT to coin-insert on NAOMI/Atomiswave; Dreamcast pads have no SELECT, so it's inert.
    static void PollInput()
    {
        if (ControlMap == null) return;

        uint b = 0;
        if (ControlMap.isActive(LC.JOYPAD_B)) b |= 1u << 0;
        if (ControlMap.isActive(LC.JOYPAD_Y)) b |= 1u << 1;
        if (ControlMap.isActive(LC.JOYPAD_SELECT)) b |= 1u << 2;
        if (ControlMap.isActive(LC.JOYPAD_START)) b |= 1u << 3;
        if (ControlMap.isActive(LC.JOYPAD_UP)) b |= 1u << 4;
        if (ControlMap.isActive(LC.JOYPAD_DOWN)) b |= 1u << 5;
        if (ControlMap.isActive(LC.JOYPAD_LEFT)) b |= 1u << 6;
        if (ControlMap.isActive(LC.JOYPAD_RIGHT)) b |= 1u << 7;
        if (ControlMap.isActive(LC.JOYPAD_A)) b |= 1u << 8;
        if (ControlMap.isActive(LC.JOYPAD_X)) b |= 1u << 9;
        if (ControlMap.isActive(LC.JOYPAD_L)) b |= 1u << 10;
        if (ControlMap.isActive(LC.JOYPAD_R)) b |= 1u << 11;
        if (ControlMap.isActive(LC.JOYPAD_L2)) b |= 1u << 12;
        if (ControlMap.isActive(LC.JOYPAD_R2)) b |= 1u << 13;
        if (ControlMap.isActive(LC.JOYPAD_L3)) b |= 1u << 14;
        if (ControlMap.isActive(LC.JOYPAD_R3)) b |= 1u << 15;

        if ((CoinSlot != null && CoinSlot.takeCoin()) || ControlMap.isActive(LC.INSERT))
            coinFrames = 6;
        if (coinFrames > 0)
        {
            b |= 1u << 2;   // SELECT
            coinFrames--;
        }

        // Analog cabinets (input: { analog-stick: true }) drive the DC analog stick + analog
        // triggers from the thumbstick and the L/R triggers (racing games). The digital d-pad bits
        // in `b` still ride along — harmless, the standard DC pad exposes both. Default cabinets
        // send a centered stick, so the game sees only the d-pad (fighting titles).
        if (AnalogStick)
        {
            ControlMap.ReadStick(out short lx, out short ly);
            short lt = ControlMap.ReadTrigger(LC.JOYPAD_L);   // left trigger  → DC L2 (brake)
            short rt = ControlMap.ReadTrigger(LC.JOYPAD_R);   // right trigger → DC R2 (accelerate)
            PdLibretro.SetInput(b, lx, ly, lt, rt);
        }
        else
        {
            PdLibretro.SetInput(b, 0, 0);
        }

        // Gun cabinet: push the VR raycast hit + the lightgun-mapped controls. In LIGHTGUN mode
        // flycast reads ONLY lightgun ids on that port, so the coin must ride SELECT here too.
        if (lightGunTarget != null)
        {
            uint gb = 0;
            if (ControlMap.isActive(LC.LIGHTGUN_TRIGGER)) gb |= 1u << PdLibretro.Lightgun.TRIGGER;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_A)) gb |= 1u << PdLibretro.Lightgun.AUX_A;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_B)) gb |= 1u << PdLibretro.Lightgun.AUX_B;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_C)) gb |= 1u << PdLibretro.Lightgun.AUX_C;
            if (ControlMap.isActive(LC.LIGHTGUN_START)) gb |= 1u << PdLibretro.Lightgun.START;
            if (ControlMap.isActive(LC.LIGHTGUN_SELECT)) gb |= 1u << PdLibretro.Lightgun.SELECT;
            if (ControlMap.isActive(LC.LIGHTGUN_RELOAD)) gb |= 1u << PdLibretro.Lightgun.RELOAD;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_UP)) gb |= 1u << PdLibretro.Lightgun.DPAD_UP;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_DOWN)) gb |= 1u << PdLibretro.Lightgun.DPAD_DOWN;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_LEFT)) gb |= 1u << PdLibretro.Lightgun.DPAD_LEFT;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_RIGHT)) gb |= 1u << PdLibretro.Lightgun.DPAD_RIGHT;
            if (coinFrames > 0)
                gb |= 1u << PdLibretro.Lightgun.SELECT;

            lightGunTarget.GetLastHit(out int hitX, out int hitY);
            bool offscreen = !lightGunTarget.PointingToTheScreen();
            PdLibretro.SetLightgun((short)hitX, (short)hitY, offscreen, gb);
        }
    }

    // Unity audio thread (OnAudioFilterRead on the screen's authored AudioSource): overwrite the
    // buffer with the core's resampled PCM; zero-fill underflow.
    public static void MoveAudioStreamTo(float[] audioData)
    {
        if (!GameLoaded) { Array.Clear(audioData, 0, audioData.Length); return; }
        int n = PdLibretro.AudioRead(audioData);
        for (int i = n; i < audioData.Length; i++)
            audioData[i] = 0f;
    }

    public static void End(string screenName, string gameFileName)
    {
        if (!isRunning(screenName, gameFileName))
            return;

        ConfigManager.WriteConsole($"[FlycastCore.End] {gameFileName} in {screenName}");
        // Stop the audio pull path first (GameLoaded gates MoveAudioStreamTo on the audio thread).
        GameLoaded = false;

        // Destroy our textures BEFORE Shutdown: the zero-copy ones are external, wrapping the
        // Unity-side VkImages that Shutdown tears down.
        if (tex != null) { UnityEngine.Object.Destroy(tex); tex = null; }
        if (zcTexes != null)
        {
            foreach (var t in zcTexes)
                if (t != null) UnityEngine.Object.Destroy(t);
            zcTexes = null;
        }
        PdLibretro.Shutdown();

        GameFileName = "";
        ScreenName = "";
        importIssued = false;
        extTexReady = false;
        bufW = bufH = 0;
        cropActiveW = cropActiveH = -1;
        lastBoundIdx = -1;
        coinFrames = 0;
        tickAccum = 0f;
        Shader = null;
        ControlMap = null;
        CoinSlot = null;
        lightGunTarget = null;
    }
}
