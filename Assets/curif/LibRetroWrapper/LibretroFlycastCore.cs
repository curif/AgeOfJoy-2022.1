/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using CM = ControlMapPathDictionary;
using LC = LibretroControlMapDictionnary;

// LibretroFlycastCore — cabinet-lifecycle driver for the Flycast core on its own Vulkan device (libpdlr),
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
public static class LibretroFlycastCore
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

    // Boot watchdog for the tester-facing flycast.log (see FlycastLog): time of the successful Start,
    // plus one-shot latches for "first frame rendered" and the "loaded but no video" warning.
    static float loadedAt;
    static bool firstFrameLogged;
    static bool noFrameWarned;

    // Tee AoJ's normal console logging (logcat / in-app bug report) to the always-on, tester-facing
    // flycast.log. File logging is Flycast-only; every WriteConsole call in this driver that matters to
    // diagnosing a boot goes through these so testers see the same story without adb or debug mode.
    static void Trace(string m)    { ConfigManager.WriteConsole(m);      FlycastLog.Line(m); }
    static void TraceErr(string m) { ConfigManager.WriteConsoleError(m); FlycastLog.Err(m); }

    // Flycast's phrasing when it can't find an arcade BIOS. What follows is the BIOS *set* name.
    const string CannotLoadBios = "cannot load BIOS ";

    // A failed load that blames a BIOS earns one extra line naming the file to install and where.
    //
    // A hint on failure, not a pre-flight check, and deliberately so. Each arcade game declares which
    // BIOS set it needs (naomi, hod2bios, awbios, f355bios, …), and Flycast's loadBios() hunts for
    // that set's ROM blobs in the game's own zip, then a parent romset zip beside it, and only then
    // <system>/dc/<set>.zip. So a merged romset boots with no separate BIOS file at all, and one
    // naomi.zip does not cover every NAOMI cabinet. Nothing outside the core can predict either fact
    // — but the core says exactly which set it wanted, so we repeat that.
    static void TraceBiosHint(string nativeReason, string sysDir)
    {
        if (string.IsNullOrEmpty(nativeReason)) return;
        int at = nativeReason.IndexOf(CannotLoadBios, StringComparison.OrdinalIgnoreCase);
        if (at < 0) return;

        string bios = nativeReason.Substring(at + CannotLoadBios.Length).Trim();
        int end = bios.IndexOfAny(new[] { ' ', '\t' });
        if (end > 0) bios = bios.Substring(0, end);
        if (bios.Length == 0) return;
        if (!bios.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) bios += ".zip";

        string wanted = Path.Combine(Path.Combine(sysDir, ContentDirName), bios);
        Trace($"[LibretroFlycastCore] hint: this game needs the '{bios}' arcade BIOS. Flycast looks for its ROMs " +
              $"inside the game's own zip, then a parent romset zip beside it, then {wanted}. Install {bios} there, " +
              $"or use a romset that carries its own BIOS. Each arcade game names its own BIOS set, so naomi.zip " +
              $"alone does not cover every cabinet.");
    }

    // Written to flycast.log whenever a boot fails: what the core could actually see on disk. A
    // directory listing settles "but the ROM is right there" in one line.
    static void TraceBootEnvironment(string gamePath, string sysDir)
    {
        const int maxListed = 40;
        try
        {
            var game = new FileInfo(gamePath);
            Trace($"[LibretroFlycastCore] game file {gamePath} — {(game.Exists ? $"{game.Length:N0} bytes" : "MISSING")}");

            string dcDir = Path.Combine(sysDir, ContentDirName);
            if (!Directory.Exists(dcDir))
            {
                Trace($"[LibretroFlycastCore] BIOS directory {dcDir} does not exist");
                return;
            }

            string[] files = Directory.GetFiles(dcDir);
            Trace($"[LibretroFlycastCore] BIOS directory {dcDir} holds {files.Length} file(s):");
            for (int i = 0; i < files.Length && i < maxListed; i++)
                Trace($"    {Path.GetFileName(files[i])} — {new FileInfo(files[i]).Length:N0} bytes");
            if (files.Length > maxListed)
                Trace($"    … and {files.Length - maxListed} more");
        }
        catch (Exception e)
        {
            TraceErr($"[LibretroFlycastCore] could not inspect the boot environment: {e.Message}");
        }
    }

    // Resolve the game file the same way LibretroMameCore.getPath does: per-core dir first
    // (downloads/dc/), then the downloads root as a fallback.
    public static string getPath(string gameFileName)
    {
        string path = ConfigManager.RomsDir + "/" + ContentDirName + "/" + gameFileName;
        if (!File.Exists(path))
            path = ConfigManager.RomsDir + "/" + gameFileName;
        if (!File.Exists(path))
        {
            TraceErr($"[LibretroFlycastCore] game not found: {ConfigManager.RomsDir}/{ContentDirName}/{gameFileName}");
            return null;
        }
        return path;
    }

    public static bool Start(string screenName, string gameFileName)
    {
        FlycastLog.Session($"Flycast boot: game='{gameFileName}' screen='{screenName}'");

        if (GameLoaded || !string.IsNullOrEmpty(GameFileName))
        {
            TraceErr($"[LibretroFlycastCore.Start] a game is already loaded ({GameFileName} in {ScreenName}); End() first");
            return false;
        }

        string gamePath = getPath(gameFileName);
        if (gamePath == null)
            return false;

        string corePath = Path.Combine(LibretroHWBridge.NativeLibraryDir(), CoreLibFileName);

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

        Trace($"[LibretroFlycastCore.Start] core='{corePath}' sys='{sysDir}' game='{gamePath}'");

        // In debug mode capture the core's INFO chatter too, not just WARN and above — the whole boot
        // trace lands in flycast.log. A verbose.txt next to the game overrides this on-device.
        if (ConfigManager.DebugActive)
            LibretroHWBridge.SetLogVerbosity(1);   // RETRO_LOG_INFO

        // Gun cabinet: declare port 0 LIGHTGUN before Start() — Flycast builds its maple bus at
        // load. Ports 1-3 stay JOYPAD (the 4-pad maple parity that gates NAOMI audio init).
        if (lightGunTarget != null && lightGunTarget.Initialized())
            LibretroHWBridge.SetPortDevice(0, LibretroHWBridge.DEVICE_LIGHTGUN);

        // The arcade TEST + SERVICE buttons are always reachable in AoJ via the double chord —
        // both stick clicks (L3+R3) held, plus left trigger = TEST / right trigger = SERVICE — on
        // every cabinet, pad and gun alike (a gun cabinet's one-time gun calibration lives in the
        // TEST menu). So the core option is
        // enabled for every flycast cabinet; the `environment:` loop below can still override it
        // per cabinet. Dreamcast games have no service buttons and ignore L3/R3 entirely. Note:
        // on NAOMI pad games this repurposes R3 from the core's "Button 9" fallback to real
        // SERVICE.
        LibretroHWBridge.SetOption("reicast_allow_service_buttons", "enabled");

        // Per-cabinet core-option overrides (description.yaml `environment:`) — must be pushed before
        // Start()/retro_load_game. Layered on top of the global Flycast.opt safe defaults inside
        // libpdlr: YAML wins, any option it doesn't name keeps its default. Full key = prefix + "_" +
        // property name (matching the software core), e.g. prefix "reicast" + "broadcast" → reicast_broadcast.
        if (CabEnvironment?.properties != null)
        {
            foreach (var kv in CabEnvironment.properties)
            {
                string key = string.IsNullOrEmpty(CabEnvironment.prefix) ? kv.Key : $"{CabEnvironment.prefix}_{kv.Key}";
                Trace($"[LibretroFlycastCore.Start] core-option override: {key} = {kv.Value}");
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
            TraceErr($"[LibretroFlycastCore.Start] pdlr_start FAILED — {why}");

            string[] captured = LibretroHWBridge.RecentLog();
            if (captured.Length > 0)
            {
                Trace($"[LibretroFlycastCore.Start] last {captured.Length} line(s) of the native boot trace:");
                foreach (string line in captured)
                    Trace($"    {line}");
            }
            TraceBiosHint(why, sysDir);
            TraceBootEnvironment(gamePath, sysDir);
            return false;
        }
        zeroCopy = LibretroHWBridge.ZeroCopyActive;
        if (!zeroCopy)
            Trace("[LibretroFlycastCore.Start] zero-copy unavailable on the core's device — using CPU read-back path");

        // Phase-lock the native pump to the VR display cadence; set after Start().
        // Pull the live headset refresh rather than trusting the 72 default — a Quest 3 (or a
        // future model) may present at 90/120, and the pump's emulated:display pulldown ratio must
        // match the real rate or ~60 Hz DC content judders. Getter returns 0 on failure → keep default.
        float liveHz = OVRPlugin.systemDisplayFrequency;
        if (liveHz > 1f)
            DisplayHz = liveHz;
        Trace($"[LibretroFlycastCore.Start] display refresh = {DisplayHz:F1} Hz (live={liveHz:F1})");
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
        Trace($"[LibretroFlycastCore.Start] pdlr_start OK — running {gameFileName} in {screenName} zeroCopy={zeroCopy} coreFps={coreFps:F3} sampleRate={LibretroHWBridge.SampleRate:F0}");
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

        // Boot watchdog for the tester log: confirm the first rendered frame, and flag a core that
        // loaded but never produced video (the "coin went in, screen stays black" case).
        if (!firstFrameLogged)
        {
            if (LibretroHWBridge.FrameCount > 0)
            {
                firstFrameLogged = true;
                FlycastLog.Line($"first frame rendered ({LibretroHWBridge.FrameCount} frames) — game is running");
            }
            else if (!noFrameWarned && Time.unscaledTime - loadedAt > 8f)
            {
                noFrameWarned = true;
                FlycastLog.Err($"core loaded but produced NO frames after 8s — check BIOS in system/{ContentDirName}/ (dc_boot.bin, dc_flash.bin) and that '{GameFileName}' is a valid set");
            }
        }

        if (Time.unscaledTime >= nextStatusAt)
        {
            nextStatusAt = Time.unscaledTime + 5f;
            ConfigManager.WriteConsole($"[LibretroFlycastCore] frames={LibretroHWBridge.FrameCount} zc={zeroCopy} extTex={extTexReady} readyIdx={(zeroCopy ? LibretroHWBridge.ReadyBufferIndex : -1)}");
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
                    if (!importIssued) { importIssued = true; Trace("[LibretroFlycastCore] zero-copy: issued AHB import event"); }
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

        // The active frame can change mid-run (NAOMI/AW resolution swaps). Re-crop when it does —
        // the buffer size is fixed, so the external textures never need rebuilding.
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
                TraceErr($"[LibretroFlycastCore] zero-copy: buffer {i} image ptr null — abort");
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
        LibretroHWBridge.FrameSize(out aw, out ah);
        ApplyZeroCopyCrop(aw, ah);
        extTexReady = true;
        Trace($"[LibretroFlycastCore] zero-copy: {n} external textures bound buffer={bw}x{bh} active={aw}x{ah}");
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
        ConfigManager.WriteConsole($"[LibretroFlycastCore] zero-copy crop: active={activeW}x{activeH} buffer={bufW}x{bufH}");
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

    // Flycast-only adjustments to the merged control map, applied by LibretroScreenController just
    // before the map is instantiated (never to DefaultControlMap itself — MAME/FBNeo cabinets keep
    // today's bindings):
    //  - Bluetooth/USB gamepads are polled directly in PollGamepad with the fixed standard flycast
    //    layout, so every gamepad-* binding is stripped from the JOYPAD_* ids to avoid double
    //    delivery with conflicting semantics (e.g. Xbox A firing both DC A and DC B). LIGHTGUN_*,
    //    INSERT/EXIT/MODIFIER and the keyboard/mouse bindings stay.
    //  - Gun cabinets get Dreamcast-gun-shaped defaults: the gun's B button (LIGHTGUN_AUX_A) on the
    //    Quest B button, and the dedicated reload off START — sharing START meant every game start
    //    also fired a forced offscreen shot — onto the free left trigger. Each rebind only applies
    //    while the merged map still equals the stock default, so `controllers:` YAML remaps of
    //    LIGHTGUN_* ids win.
    public static void AdjustControlMap(ControlMapConfiguration conf, bool lightGun)
    {
        if (conf == null) return;

        conf.RemoveControlsByPrefix("JOYPAD_", "gamepad-");
        if (!lightGun)
            return;

        if (conf.MapEquals(LC.LIGHTGUN_AUX_A, 0, new[] { CM.VR_CONTROLLER_A, CM.GAMEPAD_A }))
            conf.ReplaceMap(LC.LIGHTGUN_AUX_A, 0, new[] { CM.VR_CONTROLLER_B, CM.GAMEPAD_A });
        // quest-b must then leave AUX_B, or one press would hit NAOMI BTN1+BTN2 at once.
        if (conf.MapEquals(LC.LIGHTGUN_AUX_B, 0, new[] { CM.VR_CONTROLLER_B, CM.GAMEPAD_B, CM.VR_CONTROLLER_RIGHT_GRIP, CM.KEYBOARD_ENTER }))
            conf.RemoveControl(LC.LIGHTGUN_AUX_B, 0, CM.VR_CONTROLLER_B);
        if (conf.MapEquals(LC.LIGHTGUN_RELOAD, 0, new[] { CM.GAMEPAD_START, CM.VR_CONTROLLER_START }))
            conf.ReplaceMap(LC.LIGHTGUN_RELOAD, 0, new[] { CM.VR_CONTROLLER_LEFT_TRIGGER, CM.GAMEPAD_START });
    }

    // Push the cabinet's mapped controls to the core before its next retro_run: the RetroPad bitmask
    // (bit N == RETRO_DEVICE_ID_JOYPAD_N) plus the analog stick/trigger values. Two sources merge
    // here every frame:
    //  - the Quest controllers (and keyboard), via the cabinet's LibretroControlMap — the cabinet's
    //    `analog-stick` flag decides which thumbstick is the DC d-pad and which the DC analog stick;
    //  - a physical Bluetooth/USB gamepad, polled directly (PollGamepad) with the fixed standard
    //    flycast layout — a real pad has all the DC controls at once, so the flag never applies.
    // Coin: a taken coin (or the INSERT control) holds the SELECT bit a few frames — flycast
    // maps SELECT to coin-insert on NAOMI/Atomiswave; Dreamcast pads have no SELECT, so it's inert.
    static void PollInput()
    {
        if (ControlMap == null) return;

        uint b = 0;
        // Face buttons: label-matched to the Dreamcast, so Quest A→DC A, B→DC B, X→DC X, Y→DC Y.
        // flycast's dc_joymap swaps RetroPad A/B and X/Y (RetroPad B→DC_BTN_A, A→DC_BTN_B, Y→DC_BTN_X,
        // X→DC_BTN_Y), so we cross the Quest buttons onto the opposite RetroPad bit to undo that:
        // Quest A→RetroPad B (bit 0), Quest B→RetroPad A (bit 8), Quest X→RetroPad Y (bit 1),
        // Quest Y→RetroPad X (bit 9). (PollGamepad is already positional and needs no crossing.)
        if (ControlMap.isActive(LC.JOYPAD_A)) b |= 1u << 0;   // Quest A → RetroPad B → DC A
        if (ControlMap.isActive(LC.JOYPAD_X)) b |= 1u << 1;   // Quest X → RetroPad Y → DC X
        if (ControlMap.isActive(LC.JOYPAD_SELECT)) b |= 1u << 2;
        if (ControlMap.isActive(LC.JOYPAD_START)) b |= 1u << 3;
        if (ControlMap.isActive(LC.JOYPAD_B)) b |= 1u << 8;   // Quest B → RetroPad A → DC B
        if (ControlMap.isActive(LC.JOYPAD_Y)) b |= 1u << 9;   // Quest Y → RetroPad X → DC Y
        // DC L/R triggers (DC_BTN_L2/R2 = bits 12/13) are driven by the Quest TRIGGERS, set in the
        // analog/default branch below — analog value in analog-stick mode, digital full-press
        // otherwise. The Quest GRIPS stay unmapped here (reserved for the cabinet-exit gesture), and
        // DC_BTN_C/Z (bits 10/11) go unused — no retail DC pad has them; a `controllers:` block can
        // bind them if a game needs it.

        bool coinNow = (CoinSlot != null && CoinSlot.takeCoin()) || ControlMap.isActive(LC.INSERT);
        if (coinNow && coinFrames == 0)
            FlycastLog.Line("coin inserted (SELECT held to core)");
        if (coinNow)
            coinFrames = 6;
        if (coinFrames > 0)
        {
            b |= 1u << 2;   // SELECT
            coinFrames--;
        }

        // Arcade TEST/SERVICE double chord — universal (pad and gun). Deliberately awkward so it
        // can't fire by accident: hold BOTH stick clicks (L3 + R3) together, then squeeze a
        // trigger. L3+R3 + LEFT trigger = TEST, L3+R3 + RIGHT trigger = SERVICE. The stick clicks
        // are only the modifier — they never reach the game as L3/R3 on their own (nothing else
        // sets bits 14/15) — and the consumed trigger is kept off DC L2/R2 below (chordTrig) so a
        // pad game doesn't see it underneath. Both triggers at once fires nothing. On a gun cabinet
        // the trigger also fires/reloads via the LIGHTGUN_* path, which is harmless while opening the
        // menu. Dreamcast games never read L3/R3, so the chord is inert on a DC cabinet.
        int chordTrig = 0;   // trigger consumed by the chord: 1 = left, 2 = right (kept off DC L2/R2)
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


        // The standard DC pad exposes the analog stick and the d-pad separately, so each Quest
        // thumbstick drives exactly one of them and the roles swap with the cabinet's flag:
        //  - default: LEFT stick = DC d-pad (fighting titles), RIGHT stick = DC analog stick;
        //  - analog-stick true: LEFT stick = DC analog stick + L/R triggers = DC analog triggers
        //    (racing), and the d-pad moves to the RIGHT stick. One physical stick must never fire
        //    both the analog axis and d-pad-UP (e.g. Daytona's change-view).
        // The RIGHT stick is read through port 1, where the default map binds it.
        short lx, ly;
        short lt = 0, rt = 0;
        if (AnalogStick)
        {
            ControlMap.ReadStick(out lx, out ly);
            lt = chordTrig == 1 ? (short)0 : ControlMap.ReadTrigger(LC.JOYPAD_L);   // left trigger  → DC L2 (brake)
            rt = chordTrig == 2 ? (short)0 : ControlMap.ReadTrigger(LC.JOYPAD_R);   // right trigger → DC R2 (accelerate)
            if (ControlMap.isActive(LC.JOYPAD_UP, 1)) b |= 1u << 4;
            if (ControlMap.isActive(LC.JOYPAD_DOWN, 1)) b |= 1u << 5;
            if (ControlMap.isActive(LC.JOYPAD_LEFT, 1)) b |= 1u << 6;
            if (ControlMap.isActive(LC.JOYPAD_RIGHT, 1)) b |= 1u << 7;
        }
        else
        {
            if (ControlMap.isActive(LC.JOYPAD_UP)) b |= 1u << 4;
            if (ControlMap.isActive(LC.JOYPAD_DOWN)) b |= 1u << 5;
            if (ControlMap.isActive(LC.JOYPAD_LEFT)) b |= 1u << 6;
            if (ControlMap.isActive(LC.JOYPAD_RIGHT)) b |= 1u << 7;
            ControlMap.ReadStick(out lx, out ly, 1);
            // Quest triggers → DC L/R triggers as a digital full-press (analog value only in
            // analog-stick mode above). The chord-consumed trigger is held off.
            if (chordTrig != 1 && ControlMap.isActive(LC.JOYPAD_L)) b |= 1u << 12;   // left  trigger → DC L2
            if (chordTrig != 2 && ControlMap.isActive(LC.JOYPAD_R)) b |= 1u << 13;   // right trigger → DC R2
        }

        PollGamepad(ref b, ref lx, ref ly, ref lt, ref rt);

        LibretroHWBridge.SetInput(b, lx, ly, lt, rt);

        // Gun cabinet: push the VR raycast hit + the lightgun-mapped controls. In LIGHTGUN mode
        // flycast reads ONLY lightgun ids on that port, so the coin must ride SELECT here too.
        if (lightGunTarget != null)
        {
            uint gb = 0;
            if (ControlMap.isActive(LC.LIGHTGUN_TRIGGER)) gb |= 1u << LibretroHWBridge.Lightgun.TRIGGER;
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

    // A physical gamepad maps to the Dreamcast exactly as standalone flycast maps a RetroPad —
    // positional face buttons (south→DC A, east→DC B, west→DC X, north→DC Y through the core's
    // dc_joymap), d-pad→d-pad, left stick→analog stick, triggers→analog L2/R2, shoulders→C/Z,
    // L3+R3+trigger→arcade TEST (left) / SERVICE (right), matching the Quest chord — in both
    // cabinet modes. Merges with the Quest-derived state: bits OR, stick sums clamp, triggers take
    // the max. AdjustControlMap stripped gamepad-* from the JOYPAD_* action maps, so this is the
    // only path a pad reaches the joypad state through. Reads are allocation-free.
    static void PollGamepad(ref uint b, ref short lx, ref short ly, ref short lt, ref short rt)
    {
        Gamepad pad = Gamepad.current;
        if (pad == null) return;

        if (pad.buttonSouth.isPressed) b |= 1u << 0;        // retropad B → DC A
        if (pad.buttonWest.isPressed) b |= 1u << 1;         // retropad Y → DC X
        if (pad.selectButton.isPressed) b |= 1u << 2;       // coin on NAOMI/Atomiswave
        if (pad.startButton.isPressed) b |= 1u << 3;
        if (pad.dpad.up.isPressed) b |= 1u << 4;
        if (pad.dpad.down.isPressed) b |= 1u << 5;
        if (pad.dpad.left.isPressed) b |= 1u << 6;
        if (pad.dpad.right.isPressed) b |= 1u << 7;
        if (pad.buttonEast.isPressed) b |= 1u << 8;         // retropad A → DC B
        if (pad.buttonNorth.isPressed) b |= 1u << 9;        // retropad X → DC Y
        if (pad.leftShoulder.isPressed) b |= 1u << 10;      // retropad L → DC C
        if (pad.rightShoulder.isPressed) b |= 1u << 11;     // retropad R → DC Z
        Vector2 stick = pad.leftStick.ReadValue();
        lx = ClampAxis(lx + Mathf.RoundToInt(stick.x * 0x7fff));
        ly = ClampAxis(ly + Mathf.RoundToInt(-stick.y * 0x7fff));   // libretro/DC analog-up is -y

        float l = Mathf.Clamp01(pad.leftTrigger.ReadValue());
        float r = Mathf.Clamp01(pad.rightTrigger.ReadValue());

        // Arcade TEST/SERVICE double chord — the pad counterpart of the PollInput chord. Hold both
        // stick clicks (L3 + R3), then a trigger: L3+R3+LEFT = TEST, L3+R3+RIGHT = SERVICE; both
        // triggers at once fires nothing. Zero the squeezed trigger so a pad game sees neither its
        // analog value nor its L2/R2 shadow (below) underneath.
        bool lTrig = l > 0.5f, rTrig = r > 0.5f;
        if (pad.leftStickButton.isPressed && pad.rightStickButton.isPressed && (lTrig ^ rTrig))
        {
            b |= lTrig ? (1u << 14) : (1u << 15);   // TEST : SERVICE
            if (lTrig) l = 0f; else r = 0f;         // swallow the squeezed trigger (analog + shadow)
        }

        lt = (short)Mathf.Max(lt, (short)Mathf.RoundToInt(l * 0x7fff));
        rt = (short)Mathf.Max(rt, (short)Mathf.RoundToInt(r * 0x7fff));
        if (l > 0.5f) b |= 1u << 12;   // digital L2/R2 shadow — keeps reicast_digital_triggers usable
        if (r > 0.5f) b |= 1u << 13;
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

        Trace($"[LibretroFlycastCore.End] {gameFileName} in {screenName} (rendered {LibretroHWBridge.FrameCount} frames)");
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
