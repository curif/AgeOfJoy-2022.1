/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using LC = LibretroControlMapDictionnary;

// LibretroModelizerCore — cabinet-lifecycle driver for the Modelizer core (our Vulkan MAME fork:
// Sega Model 1/2 + Namco System 21/22/23) on its own Vulkan device via libpdlr, the hardware-rendered
// counterpart of LibretroMameCore. A cabinet selects it with `core: modelizer` in description.yaml;
// LibretroScreenController branches here instead of LibretroMameCore.
//
// Cloned from LibretroFlycastCore (the first cabinet HW core). The frame transport, audio pull and
// UV-crop are core-agnostic and identical; the one substantive divergence is input. Modelizer reads a
// plain, POSITIONAL RetroPad and does all per-game button assignment inside the core (a generated
// per-game layout table), so PollInput must send honest positional bits — it must NOT cross A/B and
// X/Y the way the Flycast/Dreamcast driver does to pre-undo the DC joymap. A verbatim copy of that
// crossing would make every face button wrong.
//
// The emulated frame reaches the cabinet CRT zero-copy: the core renders on its own VkDevice, the
// frame is blitted into AHardwareBuffer-backed images, imported once onto Unity's device on the
// render thread, and wrapped in external Texture2Ds swapped into the screen shader's material each
// frame (no per-frame CPU copy). Falls back to a CPU read-back Texture2D when the AHB extensions are
// unavailable. Games + BIOS + I/O board sets all live flat in downloads/modelizer/, resolved via
// MAME's content-directory rompath, so there is no Dreamcast-style separate-BIOS hunt.
//
// One game at a time, same as LibretroMameCore (libpdlr hosts a single core instance).
public static class LibretroModelizerCore
{
    public const string CoreName = "modelizer";
    public const string CoreLibFileName = "libmodelizer_libretro_android.so";
    // On-device content subfolder for this core's games + BIOS + I/O board sets: downloads/modelizer/.
    // All flat in one dir — MAME resolves parent/BIOS/IO sets relative to the loaded content's own
    // directory, so nothing extra lives in the system dir.
    public const string ContentDirName = "modelizer";

    // Set by LibretroScreenController before Start(), mirroring LibretroMameCore statics.
    public static ShaderScreenBase Shader;
    public static LibretroControlMap ControlMap;
    public static CoinSlotController CoinSlot;
    public static LightGunTarget lightGunTarget;   // null unless the cabinet declares light-gun
    public static bool AnalogStick;                // route left thumbstick → analog steering + triggers (racing cabinets)
    public static bool TwinStick;                  // twin-stick cabinets (Virtual-On): both thumbsticks drive the two digital sticks
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

    // Boot watchdog: time of the successful Start, plus one-shot latches for "first frame rendered"
    // and the "loaded but no video" warning.
    static float loadedAt;
    static bool firstFrameLogged;
    static bool noFrameWarned;

    // Thin wrappers so the driver body stays diff-close to LibretroFlycastCore; a future tester-facing
    // file log (as FlycastLog does for Flycast) can tee in one place. For now every breadcrumb goes to
    // the normal console (logcat / in-app bug report), which is enough for the bring-up phase.
    static void Trace(string m)    { ConfigManager.WriteConsole(m); }
    static void TraceErr(string m) { ConfigManager.WriteConsoleError(m); }

    // Written whenever a boot fails: what the core could actually see on disk. A directory listing
    // settles "but the ROM is right there" in one line. Simpler than the Flycast version — there is no
    // separate arcade-BIOS set to name; the game set plus its parent/BIOS/IO sets all sit here.
    static void TraceBootEnvironment(string gamePath)
    {
        const int maxListed = 40;
        try
        {
            var game = new FileInfo(gamePath);
            Trace($"[LibretroModelizerCore] game file {gamePath} — {(game.Exists ? $"{game.Length:N0} bytes" : "MISSING")}");

            string romDir = Path.Combine(ConfigManager.RomsDir, ContentDirName);
            if (!Directory.Exists(romDir))
            {
                Trace($"[LibretroModelizerCore] content directory {romDir} does not exist");
                return;
            }

            string[] files = Directory.GetFiles(romDir);
            Trace($"[LibretroModelizerCore] content directory {romDir} holds {files.Length} file(s):");
            for (int i = 0; i < files.Length && i < maxListed; i++)
                Trace($"    {Path.GetFileName(files[i])} — {new FileInfo(files[i]).Length:N0} bytes");
            if (files.Length > maxListed)
                Trace($"    … and {files.Length - maxListed} more");
        }
        catch (Exception e)
        {
            TraceErr($"[LibretroModelizerCore] could not inspect the boot environment: {e.Message}");
        }
    }

    // Resolve the game file the same way LibretroMameCore.getPath does: per-core dir first
    // (downloads/modelizer/), then the downloads root as a fallback.
    public static string getPath(string gameFileName)
    {
        string path = ConfigManager.RomsDir + "/" + ContentDirName + "/" + gameFileName;
        if (!File.Exists(path))
            path = ConfigManager.RomsDir + "/" + gameFileName;
        if (!File.Exists(path))
        {
            TraceErr($"[LibretroModelizerCore] game not found: {ConfigManager.RomsDir}/{ContentDirName}/{gameFileName}");
            return null;
        }
        return path;
    }

    public static bool Start(string screenName, string gameFileName)
    {
        if (GameLoaded || !string.IsNullOrEmpty(GameFileName))
        {
            TraceErr($"[LibretroModelizerCore.Start] a game is already loaded ({GameFileName} in {ScreenName}); End() first");
            return false;
        }

        string gamePath = getPath(gameFileName);
        if (gamePath == null)
            return false;

        string corePath = Path.Combine(LibretroHWBridge.NativeLibraryDir(), CoreLibFileName);

        // The core resolves its games + parent/BIOS/I/O board sets from the loaded content's own
        // directory (downloads/modelizer/), so the system dir only needs to be a valid writable root.
        // Point it at AoJ's shared system folder (the same root the software cores use); NVRAM +
        // savestates go to the save dir we pass. Net on-device layout:
        //   games + BIOS + I/O sets : <RomsDir>/modelizer/
        //   NVRAM + savestates      : <SystemDir>/modelizer/saves/
        string sysDir = ConfigManager.SystemDir;
        string saveDir = Path.Combine(sysDir, ContentDirName, "saves");
        try { Directory.CreateDirectory(saveDir); } catch { }

        Trace($"[LibretroModelizerCore.Start] core='{corePath}' sys='{sysDir}' game='{gamePath}'");

        // Twin-stick and analog-stick are mutually exclusive input modes; twin-stick wins in PollInput.
        if (TwinStick && AnalogStick)
            TraceErr("[LibretroModelizerCore.Start] both input.twin-stick and input.analog-stick set — twin-stick wins, analog-stick ignored");

        // In debug mode capture the core's INFO chatter too, not just WARN and above.
        if (ConfigManager.DebugActive)
            LibretroHWBridge.SetLogVerbosity(1);   // RETRO_LOG_INFO

        // Gun cabinet: declare port 0 LIGHTGUN before Start() — the core builds its input at load.
        // Other ports stay JOYPAD.
        if (lightGunTarget != null && lightGunTarget.Initialized())
            LibretroHWBridge.SetPortDevice(0, LibretroHWBridge.DEVICE_LIGHTGUN);

        // Test/Service: PollInput/PollGamepad cook the L3+R3+trigger double chord in C# and emit
        // bit 14 (L3) = TEST / bit 15 (R3) = SERVICE. The m2-vk core binds those straight to
        // IPT_SERVICE / IPT_SERVICE1 with no combo of its own, so no core option is needed here.

        // Per-cabinet core-option overrides (description.yaml `environment:`) — must be pushed before
        // Start()/retro_load_game. Layered on top of the core's own defaults inside libpdlr: YAML wins,
        // any option it doesn't name keeps its default. Full key = prefix + "_" + property name
        // (matching the software core).
        if (CabEnvironment?.properties != null)
        {
            foreach (var kv in CabEnvironment.properties)
            {
                string key = string.IsNullOrEmpty(CabEnvironment.prefix) ? kv.Key : $"{CabEnvironment.prefix}_{kv.Key}";
                Trace($"[LibretroModelizerCore.Start] core-option override: {key} = {kv.Value}");
                LibretroHWBridge.SetOption(key, kv.Value);
            }
        }

        // Must be set before Start() — it decides which device extensions the core is asked to enable.
        LibretroHWBridge.SetZeroCopy(true);
        if (!LibretroHWBridge.Start(corePath, sysDir, saveDir, gamePath))
        {
            // The reason comes from libpdlr, which captures it off the core's own log callback and
            // SET_MESSAGE. Everything else here is context for a tester who can't run adb logcat.
            string why = LibretroHWBridge.NativeLastError;
            if (string.IsNullOrEmpty(why))
                why = LibretroHWBridge.Available
                    ? "(libpdlr gave no reason)"
                    : $"libpdlr.so unavailable: {LibretroHWBridge.LastError} preload='{LibretroHWBridge.PreloadInfo}'";
            TraceErr($"[LibretroModelizerCore.Start] pdlr_start FAILED — {why}");

            string[] captured = LibretroHWBridge.RecentLog();
            if (captured.Length > 0)
            {
                Trace($"[LibretroModelizerCore.Start] last {captured.Length} line(s) of the native boot trace:");
                foreach (string line in captured)
                    Trace($"    {line}");
            }
            TraceBootEnvironment(gamePath);
            return false;
        }
        zeroCopy = LibretroHWBridge.ZeroCopyActive;
        if (!zeroCopy)
            Trace("[LibretroModelizerCore.Start] zero-copy unavailable on the core's device — using CPU read-back path");

        // Phase-lock the native pump to the VR display cadence; set after Start(). Pull the live
        // headset refresh rather than trusting the 72 default — a Quest 3 (or a future model) may
        // present at 90/120, and the pump's emulated:display pulldown ratio must match the real rate
        // or ~60 Hz content judders. Getter returns 0 on failure → keep default.
        float liveHz = OVRPlugin.systemDisplayFrequency;
        if (liveHz > 1f)
            DisplayHz = liveHz;
        Trace($"[LibretroModelizerCore.Start] display refresh = {DisplayHz:F1} Hz (live={liveHz:F1})");
        LibretroHWBridge.SetDisplayHz(DisplayHz);
        LibretroHWBridge.SetAudioOutputRate(AudioSettings.outputSampleRate);

        double coreFps = LibretroHWBridge.FrameFps;
        tickHz = coreFps > 1.0 ? (float)coreFps : 60f;

        GameLoaded = true;
        GameFileName = gameFileName;
        ScreenName = screenName;
        loadedAt = Time.unscaledTime;
        firstFrameLogged = false;
        noFrameWarned = false;
        Trace($"[LibretroModelizerCore.Start] pdlr_start OK — running {gameFileName} in {screenName} zeroCopy={zeroCopy} coreFps={coreFps:F3} sampleRate={LibretroHWBridge.SampleRate:F0}");
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
            LibretroHWBridge.SetPaused(paused);
    }

    // Called once per rendered frame from LibretroScreenController.Update while running.
    public static void Update()
    {
        if (!GameLoaded)
            return;

        // One tick per rendered VR frame: the pump paces retro_run off this (display-locked cadence).
        LibretroHWBridge.NotifyDisplayFrame();

        PollInput();

        // Fallback driver: a NO-OP while the native pump thread is active. Only ticks the core if the
        // pump's pthread_create failed at start. Accumulate elapsed time, capped (no catch-up spiral).
        float period = tickHz > 0f ? 1f / tickHz : 1f / 60f;
        tickAccum += Time.unscaledDeltaTime;
        int ticks = 0;
        while (tickAccum >= period && ticks < 4) { tickAccum -= period; ticks++; }
        if (tickAccum > period) tickAccum = 0f;
        for (int i = 0; i < ticks; i++)
            LibretroHWBridge.Run();

        if (zeroCopy)
            UpdateZeroCopy();
        else if (LibretroHWBridge.GetFrame(out IntPtr pixels, out int w, out int h) && pixels != IntPtr.Zero && w > 0 && h > 0)
            BlitCpu(pixels, w, h);

        // Boot watchdog: confirm the first rendered frame, and flag a core that loaded but never
        // produced video (the "coin went in, screen stays black" case).
        if (!firstFrameLogged)
        {
            if (LibretroHWBridge.FrameCount > 0)
            {
                firstFrameLogged = true;
                Trace($"[LibretroModelizerCore] first frame rendered ({LibretroHWBridge.FrameCount} frames) — game is running");
            }
            else if (!noFrameWarned && Time.unscaledTime - loadedAt > 8f)
            {
                noFrameWarned = true;
                TraceErr($"[LibretroModelizerCore] core loaded but produced NO frames after 8s — check that '{GameFileName}' and its parent/BIOS/I/O board sets are present in downloads/{ContentDirName}/");
            }
        }

        if (Time.unscaledTime >= nextStatusAt)
        {
            nextStatusAt = Time.unscaledTime + 5f;
            ConfigManager.WriteConsole($"[LibretroModelizerCore] frames={LibretroHWBridge.FrameCount} zc={zeroCopy} extTex={extTexReady} readyIdx={(zeroCopy ? LibretroHWBridge.ReadyBufferIndex : -1)}");
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
            if (LibretroHWBridge.ReadyBufferIndex >= 0 && !LibretroHWBridge.UnityImagesReady)
            {
                IntPtr fn = LibretroHWBridge.GetRenderEventFunc();
                if (fn != IntPtr.Zero)
                {
                    GL.IssuePluginEvent(fn, LibretroHWBridge.EVENT_IMPORT_AHB);
                    if (!importIssued) { importIssued = true; Trace("[LibretroModelizerCore] zero-copy: issued AHB import event"); }
                }
            }
            if (LibretroHWBridge.UnityImagesReady)
                CreateExternalTextures();
            return;
        }

        int idx = LibretroHWBridge.ReadyBufferIndex;
        if (idx >= 0 && idx < zcTexes.Length && zcTexes[idx] != null && idx != lastBoundIdx)
        {
            BindTexture(zcTexes[idx], false);
            lastBoundIdx = idx;
        }

        // The active frame can change mid-run (resolution swaps). Re-crop when it does — the buffer
        // size is fixed, so the external textures never need rebuilding.
        if (bufW > 0 && LibretroHWBridge.FrameSize(out int aw, out int ah) && aw > 0 && ah > 0 &&
            (aw != cropActiveW || ah != cropActiveH))
            ApplyZeroCopyCrop(aw, ah);
    }

    // Build one external Texture2D per AHB buffer, sized to the FIXED buffer (ceiling) — the active
    // frame is a sub-rect handled by the UV crop (the AHB import is one-shot).
    static void CreateExternalTextures()
    {
        if (!LibretroHWBridge.BufferSize(out int bw, out int bh) || bw <= 0 || bh <= 0) return;
        int n = LibretroHWBridge.BufferCount;
        if (n <= 0) return;

        bufW = bw; bufH = bh;
        zcTexes = new Texture2D[n];
        for (int i = 0; i < n; i++)
        {
            IntPtr img = LibretroHWBridge.GetUnityImagePtr(i);
            if (img == IntPtr.Zero)
            {
                TraceErr($"[LibretroModelizerCore] zero-copy: buffer {i} image ptr null — abort");
                zcTexes = null;
                return;
            }
            // linear:true (no hardware sRGB view): the AoJ screen shaders decode gamma themselves
            // (pow 2.0 in SHA_CRT_01) because the core's frame samples raw. An sRGB view here would
            // decode a second time → dark, crushed colors.
            var t = Texture2D.CreateExternalTexture(bw, bh, TextureFormat.RGBA32, false, true, img);
            // Repeat, NOT Clamp: the screen shaders (e.g. SHA_CRT_01) invert via negative _CRTTiling
            // with no offset, relying on wrap-around. Clamp collapses the flipped axis to a single
            // edge row stretched across the screen.
            t.wrapMode = TextureWrapMode.Repeat;
            t.filterMode = FilterMode.Bilinear;
            t.name = $"ModelizerAHB{i}";
            zcTexes[i] = t;
        }
        BindTexture(zcTexes[0], true);
        lastBoundIdx = 0;
        int aw = bw, ah = bh;
        LibretroHWBridge.FrameSize(out aw, out ah);
        ApplyZeroCopyCrop(aw, ah);
        extTexReady = true;
        Trace($"[LibretroModelizerCore] zero-copy: {n} external textures bound buffer={bw}x{bh} active={aw}x{ah}");
    }

    // Map the screen's UV range onto the active sub-rect of the fixed AHB buffer via the material's
    // texture ST. The Vulkan external texture samples with V flipped (content at top-left), so the
    // active band is U∈[0,au], V∈[1-av,1]. Orientation flips stay in the screen shader's own invert
    // path (YAML crt.screen.invertx/inverty) and, natively, the core's 180° framebuffer is corrected
    // in the libpdlr blit (s_flip180). Identity when active == buffer, so full-frame titles are
    // unaffected even on shaders that ignore the ST transform.
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
        ConfigManager.WriteConsole($"[LibretroModelizerCore] zero-copy crop: active={activeW}x{activeH} buffer={bufW}x{bufH}");
    }

    static void BlitCpu(IntPtr pixels, int w, int h)
    {
        if (tex == null || tex.width != w || tex.height != h)
        {
            if (tex != null) UnityEngine.Object.Destroy(tex);
            // set_image is R8G8B8A8_UNORM → RGBA32. Repeat wrap + linear: see CreateExternalTextures.
            tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,
                name = "ModelizerFrame",
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

    // Modelizer-only adjustments to the merged control map, applied by LibretroScreenController just
    // before the map is instantiated (never to DefaultControlMap itself — MAME/FBNeo cabinets keep
    // today's bindings):
    //  - Bluetooth/USB gamepads are polled directly in PollGamepad with a fixed positional RetroPad
    //    layout, so every gamepad-* binding is stripped from the JOYPAD_* ids to avoid double delivery
    //    (e.g. an Xbox A firing both its positional bit here and again through the map). LIGHTGUN_*,
    //    INSERT/EXIT/MODIFIER and the keyboard/mouse bindings stay.
    //  - No gun-specific default rebinds: modelizer gun cabinets use the stock LIGHTGUN_* map. A
    //    `controllers:` YAML block can still remap those ids per cabinet.
    // `lightGun` is accepted for call-site parity with the Flycast driver; modelizer needs no
    // gun-shaped defaults, so it is unused.
    public static void AdjustControlMap(ControlMapConfiguration conf, bool lightGun)
    {
        if (conf == null) return;
        conf.RemoveControlsByPrefix("JOYPAD_", "gamepad-");
    }

    // Push the cabinet's mapped controls to the core before its next retro_run: the RetroPad bitmask
    // (bit N == RETRO_DEVICE_ID_JOYPAD_N) plus the analog stick/trigger values. Two sources merge here
    // every frame:
    //  - the Quest controllers (and keyboard), via the cabinet's LibretroControlMap — the cabinet's
    //    `analog-stick` / `twin-stick` flags decide how the thumbsticks are routed (see the three-mode
    //    block below); twin-stick wins if both are set;
    //  - a physical Bluetooth/USB gamepad, polled directly (PollGamepad) with a fixed positional
    //    layout — a real pad has every control at once, so the flag never applies.
    //
    // Positional throughout: Quest A→RetroPad A, B→B, X→X, Y→Y (bits 8/0/9/1). The core reads a plain
    // RetroPad and maps each positional control to a per-game MAME button via its own generated layout
    // table — so C# must send honest positional bits and NOT cross them (the Flycast driver crosses
    // A/B and X/Y to pre-undo a Dreamcast joymap that modelizer does not have).
    static void PollInput()
    {
        if (ControlMap == null) return;

        uint b = 0;
        // Face buttons. Skipped in twin-stick mode: von makes no use of B/Y/A/X (bits 0/1/8/9), and the
        // core's face-diamond fallback that would otherwise read them is suppressed for von, so sending
        // them would be inert at best — and keeping them off avoids any re-collision with the right
        // stick if the core's diamond ever comes back. The right stick rides ANALOG_RIGHT (rx/ry).
        if (!TwinStick)
        {
            if (ControlMap.isActive(LC.JOYPAD_A)) b |= 1u << 8;    // Quest A → RetroPad A
            if (ControlMap.isActive(LC.JOYPAD_B)) b |= 1u << 0;    // Quest B → RetroPad B
            if (ControlMap.isActive(LC.JOYPAD_X)) b |= 1u << 9;    // Quest X → RetroPad X
            if (ControlMap.isActive(LC.JOYPAD_Y)) b |= 1u << 1;    // Quest Y → RetroPad Y
        }
        if (ControlMap.isActive(LC.JOYPAD_START)) b |= 1u << 3;

        // Coin: a taken coin (or the INSERT control) holds SELECT a few frames — the core maps
        // IPT_SELECT → COIN1.
        bool coinNow = (CoinSlot != null && CoinSlot.takeCoin()) || ControlMap.isActive(LC.INSERT);
        if (coinNow && coinFrames == 0)
            Trace("[LibretroModelizerCore] coin inserted (SELECT held to core)");
        if (coinNow)
            coinFrames = 6;
        if (coinFrames > 0)
        {
            b |= 1u << 2;   // SELECT
            coinFrames--;
        }

        // Arcade TEST/SERVICE double chord — the same gesture as the Flycast core. Deliberately
        // awkward so it can't fire by accident: hold BOTH stick clicks (L3 + R3) together, then
        // squeeze a trigger. L3+R3 + LEFT trigger = TEST, L3+R3 + RIGHT trigger = SERVICE. The stick
        // clicks are only the modifier — they never reach the game as bare L3/R3 (nothing else sets
        // bits 14/15) — and the consumed trigger is kept off the game's L2/R2 below (chordTrig) so it
        // isn't seen underneath. Both triggers at once fires nothing. The m2-vk core binds bit 14 (L3)
        // → IPT_SERVICE (TEST switch) and bit 15 (R3) → IPT_SERVICE1 (SERVICE button) directly, with
        // no combo of its own, so this C# gate is the only gate.
        int chordTrig = 0;   // trigger consumed by the chord: 1 = left, 2 = right (kept off L2/R2)
        if (ControlMap.isActive(LC.JOYPAD_L3) && ControlMap.isActive(LC.JOYPAD_R3))
        {
            bool leftTrig = ControlMap.isActive(LC.JOYPAD_L);    // left  trigger
            bool rightTrig = ControlMap.isActive(LC.JOYPAD_R);   // right trigger
            if (leftTrig ^ rightTrig)
            {
                b |= leftTrig ? (1u << 14) : (1u << 15);          // TEST : SERVICE
                chordTrig = leftTrig ? 1 : 2;
            }
        }

        // The core serves a single analog channel (ANALOG_LEFT) plus the two analog triggers on
        // port 0. Three input modes, chosen by the cabinet flags (twin-stick wins over analog-stick):
        //  - twin-stick true (Virtual-On): LEFT stick = d-pad, RIGHT stick = ANALOG_RIGHT (rx/ry) —
        //    the two drive the game's two digital sticks; triggers = Shots, grips = Dashes. See below.
        //  - analog-stick true (racing): LEFT stick = steering (ANALOG_LEFT), triggers = pedals, and
        //    the d-pad moves to the RIGHT stick (menus/view). One physical stick must never drive both
        //    an analog axis and a d-pad direction (e.g. Daytona's change-view).
        //  - default (fighting / joystick): LEFT stick = digital joystick (d-pad); the RIGHT stick
        //    feeds the analog channel for the rare title that reads it; triggers are digital L2/R2.
        // The RIGHT stick is read through port 1, where the default map binds it.
        short lx, ly;
        short rx = 0, ry = 0;   // right analog stick — only used in twin-stick mode
        short lt = 0, rt = 0;
        if (TwinStick)
        {
            // Twin-stick cabinets (Cyber Troopers Virtual-On). BOTH thumbsticks are sent as ANALOG: the
            // core binds von's two digital sticks (IPT_JOYSTICKLEFT/RIGHT) to ANALOG_LEFT / ANALOG_RIGHT
            // and threshold-converts each axis to the digital directions. The core's digital d-pad /
            // face-diamond fallback is deliberately removed for von (the right diamond shared MAME-button
            // slots with the Shot/Dash buttons, so a button press also threw the stick), which is why the
            // LEFT stick must be analog here too — its d-pad fallback went with it. Buttons come from the
            // `von` layout row: Shots on the shoulders (bits 10/11), Dashes on L2/R2 (bits 12/13, read as
            // the core's L2/R2 axis via the digital-bit fallback). ReadStick returns libretro convention
            // (up = negative Y), which the ANALOG channels expect.
            ControlMap.ReadStick(out lx, out ly);       // LEFT  thumbstick → ANALOG_LEFT
            ControlMap.ReadStick(out rx, out ry, 1);    // RIGHT thumbstick → ANALOG_RIGHT (rx/ry, port 1)

            // Shots on the triggers (RetroPad L/R shoulders); Dashes on the grips (L2/R2). The
            // chord-consumed trigger is held off, exactly as L2/R2 are in the default branch.
            if (chordTrig != 1 && ControlMap.isActive(LC.JOYPAD_L)) b |= 1u << 10;   // left  trigger → Left Shot
            if (chordTrig != 2 && ControlMap.isActive(LC.JOYPAD_R)) b |= 1u << 11;   // right trigger → Right Shot
            if (ControlMap.isActive(LC.JOYPAD_L2)) b |= 1u << 12;                    // left  grip → Left Dash
            if (ControlMap.isActive(LC.JOYPAD_R2)) b |= 1u << 13;                    // right grip → Right Dash
        }
        else if (AnalogStick)
        {
            ControlMap.ReadStick(out lx, out ly);
            lt = chordTrig == 1 ? (short)0 : ControlMap.ReadTrigger(LC.JOYPAD_L);   // left  trigger → analog brake (core: IPT_PEDAL2)
            rt = chordTrig == 2 ? (short)0 : ControlMap.ReadTrigger(LC.JOYPAD_R);   // right trigger → analog accel (core: IPT_PEDAL)
            if (ControlMap.isActive(LC.JOYPAD_UP, 1)) b |= 1u << 4;
            if (ControlMap.isActive(LC.JOYPAD_DOWN, 1)) b |= 1u << 5;
            if (ControlMap.isActive(LC.JOYPAD_LEFT, 1)) b |= 1u << 6;
            if (ControlMap.isActive(LC.JOYPAD_RIGHT, 1)) b |= 1u << 7;
            // Grips → RetroPad L/R shoulders (bits 10/11). Free in racing mode — the Quest triggers are
            // the analog pedals here — so a driving cabinet can put extra buttons on the grips (e.g.
            // Tokyo Wars' two cannon triggers). NOT routed to L2/R2 (bits 12/13): those are the pedal
            // axes, and a digital L2/R2 bit reads as a full pedal press.
            if (ControlMap.isActive(LC.JOYPAD_L2)) b |= 1u << 10;   // left  grip → RetroPad L
            if (ControlMap.isActive(LC.JOYPAD_R2)) b |= 1u << 11;   // right grip → RetroPad R
        }
        else
        {
            if (ControlMap.isActive(LC.JOYPAD_UP)) b |= 1u << 4;
            if (ControlMap.isActive(LC.JOYPAD_DOWN)) b |= 1u << 5;
            if (ControlMap.isActive(LC.JOYPAD_LEFT)) b |= 1u << 6;
            if (ControlMap.isActive(LC.JOYPAD_RIGHT)) b |= 1u << 7;
            ControlMap.ReadStick(out lx, out ly, 1);
            // Quest triggers → digital L2/R2 (analog value only in analog-stick mode above). The
            // chord-consumed trigger is held off.
            if (chordTrig != 1 && ControlMap.isActive(LC.JOYPAD_L)) b |= 1u << 12;   // left  trigger → L2
            if (chordTrig != 2 && ControlMap.isActive(LC.JOYPAD_R)) b |= 1u << 13;   // right trigger → R2
        }

        PollGamepad(ref b, ref lx, ref ly, ref rx, ref ry, ref lt, ref rt);

        LibretroHWBridge.SetInput(b, lx, ly, rx, ry, lt, rt);

        // Gun cabinet: push the VR raycast hit + the lightgun-mapped controls, same path as Flycast.
        // The coin rides SELECT here too.
        //
        // ⚠ Unlike flycast, m2-vk keeps the port's PAD device live alongside the gun device — that is
        // deliberate, it is what gives a gun game any route to the test menu — so a stray press on
        // this channel is NOT invisible: it reaches the machine as a real switch and the Model 2
        // INPUT TEST screen draws it. LIGHTGUN_TRIGGER is on the right trigger, which is also the
        // SERVICE half of the TEST/SERVICE chord, so the chord's consumed trigger is held off here
        // (chordTrig) exactly as it is held off L2/R2 above. Without the gate, asking for SERVICE
        // lit the gun trigger at the same time — the whole reason this bug looked like two switches
        // firing at once, and why it never showed on a pad cabinet or in RetroArch (where the
        // switches are pressed bare, with no trigger involved). Nothing else in the stock gun map
        // sits on a trigger: RELOAD/START are on start, AUX_A/B/C on A/B/X, SELECT on select, and
        // AdjustControlMap makes no gun-specific rebinds.
        if (lightGunTarget != null)
        {
            uint gb = 0;
            if (chordTrig != 2 && ControlMap.isActive(LC.LIGHTGUN_TRIGGER)) gb |= 1u << LibretroHWBridge.Lightgun.TRIGGER;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_A)) gb |= 1u << LibretroHWBridge.Lightgun.AUX_A;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_B)) gb |= 1u << LibretroHWBridge.Lightgun.AUX_B;
            if (ControlMap.isActive(LC.LIGHTGUN_AUX_C)) gb |= 1u << LibretroHWBridge.Lightgun.AUX_C;
            if (ControlMap.isActive(LC.LIGHTGUN_START)) gb |= 1u << LibretroHWBridge.Lightgun.START;
            if (ControlMap.isActive(LC.LIGHTGUN_SELECT)) gb |= 1u << LibretroHWBridge.Lightgun.SELECT;
            if (ControlMap.isActive(LC.LIGHTGUN_RELOAD)) gb |= 1u << LibretroHWBridge.Lightgun.RELOAD;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_UP)) gb |= 1u << LibretroHWBridge.Lightgun.DPAD_UP;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_DOWN)) gb |= 1u << LibretroHWBridge.Lightgun.DPAD_DOWN;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_LEFT)) gb |= 1u << LibretroHWBridge.Lightgun.DPAD_LEFT;
            if (ControlMap.isActive(LC.LIGHTGUN_DPAD_RIGHT)) gb |= 1u << LibretroHWBridge.Lightgun.DPAD_RIGHT;
            if (coinFrames > 0)
                gb |= 1u << LibretroHWBridge.Lightgun.SELECT;

            lightGunTarget.GetLastHit(out int hitX, out int hitY);
            bool offscreen = !lightGunTarget.PointingToTheScreen();
            LibretroHWBridge.SetLightgun((short)hitX, (short)hitY, offscreen, gb);
        }
    }

    // A physical gamepad maps to a positional RetroPad exactly as standalone RetroArch would — south→B
    // (bit 0), east→A (bit 8), west→Y (bit 1), north→X (bit 9), d-pad→d-pad, left stick→analog stick,
    // triggers→analog L2/R2, shoulders→L/R, L3+R3+trigger→arcade TEST (left) / SERVICE (right)
    // matching the Quest chord — in both cabinet modes. Merges with the Quest-derived state: bits OR,
    // stick sums clamp, triggers take the max. AdjustControlMap stripped gamepad-* from the JOYPAD_*
    // action maps, so this is the only path a pad reaches the joypad state through. Reads are
    // allocation-free.
    static void PollGamepad(ref uint b, ref short lx, ref short ly, ref short rx, ref short ry, ref short lt, ref short rt)
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        if (pad.buttonSouth.isPressed) b |= 1u << 0;        // RetroPad B
        if (pad.buttonWest.isPressed) b |= 1u << 1;         // RetroPad Y
        if (pad.selectButton.isPressed) b |= 1u << 2;       // coin
        if (pad.startButton.isPressed) b |= 1u << 3;
        if (pad.dpad.up.isPressed) b |= 1u << 4;
        if (pad.dpad.down.isPressed) b |= 1u << 5;
        if (pad.dpad.left.isPressed) b |= 1u << 6;
        if (pad.dpad.right.isPressed) b |= 1u << 7;
        if (pad.buttonEast.isPressed) b |= 1u << 8;         // RetroPad A
        if (pad.buttonNorth.isPressed) b |= 1u << 9;        // RetroPad X
        if (pad.leftShoulder.isPressed) b |= 1u << 10;      // RetroPad L
        if (pad.rightShoulder.isPressed) b |= 1u << 11;     // RetroPad R

        Vector2 stick = pad.leftStick.ReadValue();
        lx = ClampAxis(lx + Mathf.RoundToInt(stick.x * 0x7fff));
        ly = ClampAxis(ly + Mathf.RoundToInt(-stick.y * 0x7fff));   // libretro analog-up is -y

        // Right stick → ANALOG_RIGHT (twin-stick cores read it; harmless 0 for the others). Same
        // sum-and-clamp merge as the left stick so a pad and the Quest can coexist.
        Vector2 rstick = pad.rightStick.ReadValue();
        rx = ClampAxis(rx + Mathf.RoundToInt(rstick.x * 0x7fff));
        ry = ClampAxis(ry + Mathf.RoundToInt(-rstick.y * 0x7fff));

        float l = Mathf.Clamp01(pad.leftTrigger.ReadValue());
        float r = Mathf.Clamp01(pad.rightTrigger.ReadValue());

        // Arcade TEST/SERVICE double chord — the pad counterpart of the PollInput chord. Hold both
        // stick clicks (L3 + R3), then a trigger: L3+R3+LEFT = TEST (bit 14), L3+R3+RIGHT = SERVICE
        // (bit 15); both triggers at once fires nothing. Zero the squeezed trigger so a game sees
        // neither its analog value nor its L2/R2 shadow (below) underneath. The stick clicks are the
        // modifier only — nothing else sets bits 14/15, so a bare click never reaches the game.
        bool lTrig = l > 0.5f, rTrig = r > 0.5f;
        if (pad.leftStickButton.isPressed && pad.rightStickButton.isPressed && (lTrig ^ rTrig))
        {
            b |= lTrig ? (1u << 14) : (1u << 15);   // TEST : SERVICE
            if (lTrig) l = 0f; else r = 0f;         // swallow the squeezed trigger (analog + shadow)
        }

        lt = (short)Mathf.Max(lt, (short)Mathf.RoundToInt(l * 0x7fff));
        rt = (short)Mathf.Max(rt, (short)Mathf.RoundToInt(r * 0x7fff));
        if (l > 0.5f) b |= 1u << 12;   // digital L2 shadow
        if (r > 0.5f) b |= 1u << 13;   // digital R2 shadow
    }

    static short ClampAxis(int v)
    {
        return (short)Mathf.Clamp(v, -0x7fff, 0x7fff);
    }

    // Unity audio thread (OnAudioFilterRead on the screen's authored AudioSource): overwrite the
    // buffer with the core's resampled PCM; zero-fill underflow.
    public static void MoveAudioStreamTo(float[] audioData)
    {
        if (!GameLoaded) { Array.Clear(audioData, 0, audioData.Length); return; }
        int n = LibretroHWBridge.AudioRead(audioData);
        for (int i = n; i < audioData.Length; i++)
            audioData[i] = 0f;
    }

    public static void End(string screenName, string gameFileName)
    {
        if (!isRunning(screenName, gameFileName))
            return;

        Trace($"[LibretroModelizerCore.End] {gameFileName} in {screenName} (rendered {LibretroHWBridge.FrameCount} frames)");
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
        LibretroHWBridge.Shutdown();

        GameFileName = "";
        ScreenName = "";
        importIssued = false;
        extTexReady = false;
        bufW = bufH = 0;
        cropActiveW = cropActiveH = -1;
        lastBoundIdx = -1;
        coinFrames = 0;
        tickAccum = 0f;
        firstFrameLogged = false;
        noFrameWarned = false;
        Shader = null;
        ControlMap = null;
        CoinSlot = null;
        lightGunTarget = null;
    }
}
