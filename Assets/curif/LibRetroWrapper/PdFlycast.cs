using System;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

// PdFlycast — DEBUG driver for stage 2b: bring up Flycast on its own Vulkan device via libpdlr, run
// it, and show its emulated frame on this object's material (CPU read-back path). Proves the full
// pipeline end-to-end (stock core → in-process Vulkan → quad). Zero-copy AHB is the later upgrade.
//
// Attach to a debug QUAD (e.g. the "FrameBuffer" object). On-device
// only (no-op in editor). Watch logcat "pdlr"/"flycast". Remove before release.
//
// Setup (push BIOS + game once) — same layout as the shipping cabinets:
//   adb push dc_boot.bin dc_flash.bin /sdcard/Android/data/com.curif.AgeOfJoy/system/dc/
//   adb push vf3.cdi               /sdcard/Android/data/com.curif.AgeOfJoy/downloads/dc/
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(AudioSource))]   // authored (NOT runtime-added) — runtime-added sources never
                                          // get pulled into the DSP graph on Quest/Meta XR (callbacks fire 0×)
public class PdFlycast : MonoBehaviour
{
    [Tooltip("Deprecated / unused: BIOS + games now come from the shared production locations " +
             "(system/dc and downloads/dc), same as LibretroFlycastCore. Kept only so existing scene " +
             "serialization doesn't warn.")]
    public string dcSubDir = "dc";
    [Tooltip("Game file to load from downloads/dc/ (overridable at runtime via that folder's game.txt).")]
    public string gameFile = "vf3.cdi";
    public string coreFileName = "libflycast_libretro_android.so";

    [Tooltip("Tick the emulator at the core's own reported rate (Dreamcast NTSC ≈ 59.94 Hz) rather " +
             "than targetHz. Keeps emulated time correct — matters for audio sync. If off, or the " +
             "core reports no rate, targetHz is used.")]
    public bool useCoreFrameRate = true;

    [Tooltip("Fallback/override retro_run ticks per second, used when useCoreFrameRate is off or the " +
             "core doesn't report a rate.")]
    public float targetHz = 60f;

    [Tooltip("VR display refresh (Hz). The native pump phase-locks the emulator to this cadence so " +
             "≈60 Hz content on the 72 Hz display shows a steady 5:6 pulldown instead of a drifting, " +
             "juddery beat. Quest rooms present at 72; set 90 if a room runs at 90.")]
    public float displayHz = 72f;

    [Tooltip("Flip vertically. CPU path: software row-flip. Zero-copy path: UV transform on the " +
             "sampled texture. Device-tested false here (Flycast already matches Unity's orientation).")]
    public bool flipY = false;

    [Tooltip("Flip horizontally (zero-copy path only — applied as a UV transform on the sampled AHB " +
             "texture). Default true: the AHB-sampled frame comes out horizontally mirrored vs the " +
             "CPU path, so this corrects it. Harmless on the CPU path (not applied there).")]
    public bool flipX = true;

    [Tooltip("Zero-copy AHB transport (no per-frame CPU copy — the 72 fps form). Falls back to the " +
             "CPU read-back path automatically if the core's device can't enable the AHB extensions. " +
             "Set before entering play.")]
    public bool zeroCopy = true;

    [Tooltip("Play emulated audio through a 2D (non-spatial) AudioSource on this object. Debug.")]
    public bool audioEnabled = true;

    [Tooltip("DEBUG: record the exact PCM Unity pulls (post zero-fill — i.e. what would be heard) to a " +
             "16-bit WAV under persistentDataPath (pdflycast_audio.wav). Captures dumpSeconds then stops; " +
             "a partial file is flushed on scene exit. adb pull it and play on PC to confirm audio exists.")]
    public bool dumpAudioWav = true;   // debug driver: on by default so the capture needs no Inspector step
    [Tooltip("Seconds of audio to capture when dumpAudioWav is on. Buffer is preallocated (≈8 MB / 20 s).")]
    public float dumpSeconds = 60f;

    [Tooltip("Also append every status line to a file under persistentDataPath (pdflycast_status.txt). " +
             "Off by default — it's a per-second disk write that's only useful for offline diagnosis.")]
    public bool verboseStatus = false;

    [Header("Status (read-only)")]
    public bool started;
    public int  frameCount;
    public string res;

    Renderer    _renderer;
    Material    _material;
    Texture2D   _tex;            // CPU-path frame texture
    Texture2D[] _zcTexes;        // zero-copy: one external texture per AHB buffer
    byte[]    _flipBuf;
    float     _lastRunAt;
    float     _tickAccum;      // time accumulator to drive retro_run at _tickHz, display-rate-agnostic
    float     _tickHz;         // effective retro_run rate (core fps or targetHz)
    float     _nextStatusAt;
    string    _statusPath;
    bool      _importIssued;   // zero-copy: GL.IssuePluginEvent(IMPORT_AHB) sent
    bool      _extTexReady;    // zero-copy: external Texture2D created + bound
    int       _bufW, _bufH;    // zero-copy: fixed AHB/external-texture size (the ceiling)
    int       _cropActiveW = -1, _cropActiveH = -1;  // last active size the UV crop was applied for
    AudioSource _audio;        // 2D authored source; OnAudioFilterRead (this GO) generates its output
    AudioClip   _keepAlive;    // short silent looping clip — keeps the source "playing" so the DSP
                               // filter chain (OnAudioFilterRead) is pulled; we overwrite its output
    float[]   _dumpBuf;        // WAV capture: preallocated, filled on the audio thread
    int       _dumpPos;        // floats captured so far
    int       _dumpRate;       // capture sample rate (= Unity output rate)
    bool      _dumpWritten;    // file flushed (full or on disable) — write once

    void OnEnable()
    {
        _statusPath = Path.Combine(Application.persistentDataPath, "pdflycast_status.txt");
        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;   // instance, safe to mutate

        // Pull BIOS and games from the SAME on-device locations the production LibretroFlycastCore uses, so
        // the debug quad and the real cabinets share one content layout (see LibretroFlycastCore.Start):
        //   BIOS + nvmem : <SystemDir>/dc/     (Flycast forces the /dc subdir on its system dir)
        //   games        : <RomsDir>/dc/<file> (downloads/dc/), falling back to downloads/
        string sysDir   = ConfigManager.SystemDir;
        string saveDir  = Path.Combine(sysDir, LibretroFlycastCore.ContentDirName, "saves");
        string romsDir  = Path.Combine(ConfigManager.RomsDir, LibretroFlycastCore.ContentDirName);

        // Per-device override: first non-empty line of <roms>/game.txt names the game file to load
        // (relative to the flycast roms dir), replacing the scene-serialized gameFile. Lets us swap
        // games (DC disc images, NAOMI/Atomiswave romsets — flycast auto-detects the platform from
        // the content) with an adb push instead of a Unity rebuild.
        string overridePath = Path.Combine(romsDir, "game.txt");
        try
        {
            if (File.Exists(overridePath))
            {
                foreach (string line in File.ReadAllLines(overridePath))
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;
                    Status($"game.txt override: '{gameFile}' → '{trimmed}'");
                    gameFile = trimmed;
                    break;
                }
            }
        }
        catch (Exception e) { Status("game.txt read failed (using default game): " + e.Message); }

        // Resolve the game the same way LibretroFlycastCore.getPath does: downloads/dc first, then downloads/.
        string gamePath = Path.Combine(romsDir, gameFile);
        if (!File.Exists(gamePath))
            gamePath = Path.Combine(ConfigManager.RomsDir, gameFile);
        string corePath = Path.Combine(LibretroHWBridge.NativeLibraryDir(), coreFileName);
        try { Directory.CreateDirectory(saveDir); } catch { }

        Status($"start core='{corePath}' sys='{sysDir}' roms='{romsDir}' game='{gamePath}' zeroCopy={zeroCopy}");
        if (!File.Exists(gamePath)) Status($"WARNING game not found at '{gamePath}' — push it to downloads/dc/ first");

        // Must be set before Start() — it decides which device extensions the core is asked to enable.
        LibretroHWBridge.SetZeroCopy(zeroCopy);
        started = LibretroHWBridge.Start(corePath, sysDir, saveDir, gamePath);
        if (started && zeroCopy && !LibretroHWBridge.ZeroCopyActive)
        {
            // The core's device couldn't get the AHB extensions; native fell back to CPU readback.
            // Follow it so we actually display (otherwise we'd wait forever for an import that never comes).
            zeroCopy = false;
            Status("zero-copy unavailable on the core's device — using CPU read-back path");
        }
        // Phase-lock the native pump to the VR display cadence (removes the 60→72 beat/judder). Must
        // be set after Start(); the per-frame NotifyDisplayFrame() in Update drives the lock.
        LibretroHWBridge.SetDisplayHz(displayHz);
        double coreFps = LibretroHWBridge.FrameFps;
        _tickHz = (useCoreFrameRate && coreFps > 1.0) ? (float)coreFps : targetHz;
        Status(started
            ? $"pdlr_start OK — running (zeroCopy={zeroCopy}, tick={_tickHz:F3}Hz, coreFps={coreFps:F3}, sampleRate={LibretroHWBridge.SampleRate:F0})"
            : $"pdlr_start FAILED — Available={LibretroHWBridge.Available} preload='{LibretroHWBridge.PreloadInfo}' lastError='{LibretroHWBridge.LastError}'");

        if (started && audioEnabled) SetupAudio();
        _lastRunAt = Time.unscaledTime;
    }

    void Update()
    {
        if (!started) return;

        float now = Time.unscaledTime;

        // Phase-lock reference for the native pump: one tick per rendered VR frame. The pump paces
        // retro_run off this (display-locked 5:6 cadence) rather than a free-running wall clock, which
        // removes the 60→72 beat/judder. Cheap (atomic bump + condvar signal in native).
        LibretroHWBridge.NotifyDisplayFrame();

        PollInput();   // push input once per rendered frame

        // Fallback core driver: a NO-OP while the native pump thread is active (it drives retro_run and
        // paces itself). Only ticks the core if the pump's pthread_create failed at start. The old
        // skip-if-too-soon throttle aliased badly against 72Hz, so accumulate elapsed time and run the
        // ticks that are due, capped to avoid a spiral. retro_run is ~1ms and non-blocking here.
        float period = (_tickHz > 0f) ? 1f / _tickHz : 1f / 60f;
        _tickAccum += Time.unscaledDeltaTime;
        int ticks = 0;
        while (_tickAccum >= period && ticks < 4) { _tickAccum -= period; ticks++; }
        if (_tickAccum > period) _tickAccum = 0f;   // drop backlog after a hitch/pause (no catch-up spiral)

        for (int i = 0; i < ticks; i++)
            LibretroHWBridge.Run();
        if (audioEnabled) EnsureAudioPlaying();   // re-arm if a focus/HMD change stopped the source

        if (zeroCopy) UpdateZeroCopy();
        else if (LibretroHWBridge.GetFrame(out IntPtr pixels, out int w, out int h) && pixels != IntPtr.Zero && w > 0 && h > 0)
            Blit(pixels, w, h);

        if (now >= _nextStatusAt)
        {
            _nextStatusAt = now + 1f;
            frameCount = LibretroHWBridge.FrameCount;
            res = LibretroHWBridge.FrameSize(out int rw, out int rh) ? $"{rw}x{rh}" : "?";
            Status($"running — core frames={frameCount} tex={res} zc={zeroCopy} extTex={_extTexReady} readyIdx={(zeroCopy ? LibretroHWBridge.ReadyBufferIndex : -1)}");
        }

        if (_dumpBuf != null && !_dumpWritten && _dumpPos >= _dumpBuf.Length) WriteWavDump();
    }

    // Zero-copy path: once the FIRST BLIT has completed (ReadyBufferIndex >= 0 — that's the only
    // signal that guarantees the AHB buffers exist, now that retro_run+blit live on the native pump
    // thread), issue the import event; re-issue each frame until the render-thread import succeeds
    // (it's idempotent native-side, and a one-shot latch raced the pump: event fired before the first
    // blit → "buffers not allocated" → permanent black quad). Once Unity has imported all N buffers,
    // wrap each in an external Texture2D. Then each frame just bind the material to the "ready"
    // buffer the native side most recently blitted — triple-buffered, no per-frame copy.
    void UpdateZeroCopy()
    {
        if (!_extTexReady)
        {
            if (LibretroHWBridge.ReadyBufferIndex >= 0 && !LibretroHWBridge.UnityImagesReady)
            {
                IntPtr fn = LibretroHWBridge.GetRenderEventFunc();
                if (fn != IntPtr.Zero)
                {
                    GL.IssuePluginEvent(fn, LibretroHWBridge.EVENT_IMPORT_AHB);
                    if (!_importIssued) { _importIssued = true; Status("zero-copy: issued AHB import event"); }
                }
            }
            if (LibretroHWBridge.UnityImagesReady) CreateExternalTextures();
            return;
        }

        int idx = LibretroHWBridge.ReadyBufferIndex;
        if (idx >= 0 && idx < _zcTexes.Length && _zcTexes[idx] != null) ShowBuffer(idx);

        // The active frame can change mid-run (e.g. Metal Slug 6 boots 640x238 then switches to
        // 640x480). Re-crop when it does — cheap and usually a no-op. The buffer size is fixed, so
        // the external textures never need rebuilding.
        if (_bufW > 0 && LibretroHWBridge.FrameSize(out int aw, out int ah) && aw > 0 && ah > 0 &&
            (aw != _cropActiveW || ah != _cropActiveH))
            ApplyZeroCopyCrop(aw, ah, _bufW, _bufH);
    }

    // Build one external Texture2D per AHB buffer (each aliases that buffer's Unity-imported VkImage).
    void CreateExternalTextures()
    {
        // Size the external textures to the FIXED buffer (ceiling), not the active frame — the active
        // frame is a sub-rect we UV-crop, and it may change over the run (the AHB import is one-shot).
        if (!LibretroHWBridge.BufferSize(out int bw, out int bh) || bw <= 0 || bh <= 0) return;
        int n = LibretroHWBridge.BufferCount;
        if (n <= 0) return;

        _bufW = bw; _bufH = bh;
        _zcTexes = new Texture2D[n];
        for (int i = 0; i < n; i++)
        {
            IntPtr img = LibretroHWBridge.GetUnityImagePtr(i);
            if (img == IntPtr.Zero) { Status($"zero-copy: buffer {i} ptr null — abort"); return; }
            var t = Texture2D.CreateExternalTexture(bw, bh, TextureFormat.RGBA32, false, false, img);
            t.wrapMode = TextureWrapMode.Clamp; t.filterMode = FilterMode.Bilinear; t.name = $"PdFlycastAHB{i}";
            _zcTexes[i] = t;
        }
        BindTexture(_zcTexes[0]);   // sets emission keyword/color once; per-frame ShowBuffer just swaps textures
        int aw = bw, ah = bh;
        LibretroHWBridge.FrameSize(out aw, out ah);
        ApplyZeroCopyCrop(aw, ah, bw, bh);   // flip + crop; identity when active == buffer
        _extTexReady = true;
        Status($"zero-copy: {n} external textures bound buffer={bw}x{bh} active={aw}x{ah}");
    }

    // Per-frame: point the material at the ready buffer's texture (cheap — keyword/color already set).
    void ShowBuffer(int idx)
    {
        Texture t = _zcTexes[idx];
        _material.mainTexture = t;
        if (_material.HasProperty("_EmissionMap")) _material.SetTexture("_EmissionMap", t);
    }

    // DEBUG input: read the attached gamepad (Unity InputSystem) and push it to the core before the
    // next retro_run. Buttons ABXY + d-pad + START, plus the left analog stick. Single port (0).
    // This is throwaway test wiring — the shipping control layer lives elsewhere.
    void PollInput()
    {
        var gp = Gamepad.current;
        if (gp == null) { LibretroHWBridge.SetInput(0, 0, 0); return; }

        uint b = 0;
        // Physical A/B/X/Y → RetroPad A/B/X/Y directly. Flycast's internal DC order can make this
        // feel rotated on-device; remap here if so.
        if (gp.buttonEast.isPressed)  b |= 1u << LibretroHWBridge.Joypad.A;
        if (gp.buttonSouth.isPressed) b |= 1u << LibretroHWBridge.Joypad.B;
        if (gp.buttonWest.isPressed)  b |= 1u << LibretroHWBridge.Joypad.X;
        if (gp.buttonNorth.isPressed) b |= 1u << LibretroHWBridge.Joypad.Y;
        if (gp.startButton.isPressed) b |= 1u << LibretroHWBridge.Joypad.START;
        if (gp.dpad.up.isPressed)     b |= 1u << LibretroHWBridge.Joypad.UP;
        if (gp.dpad.down.isPressed)   b |= 1u << LibretroHWBridge.Joypad.DOWN;
        if (gp.dpad.left.isPressed)   b |= 1u << LibretroHWBridge.Joypad.LEFT;
        if (gp.dpad.right.isPressed)  b |= 1u << LibretroHWBridge.Joypad.RIGHT;

        Vector2 s = gp.leftStick.ReadValue();
        if (Mathf.Abs(s.x) < 0.2f) s.x = 0f;
        if (Mathf.Abs(s.y) < 0.2f) s.y = 0f;
        // libretro analog: +X right, +Y down — Unity's stick Y is up-positive, so negate Y.
        short lx = (short)Mathf.Clamp(Mathf.RoundToInt(s.x * 32767f), -32767, 32767);
        short ly = (short)Mathf.Clamp(Mathf.RoundToInt(-s.y * 32767f), -32767, 32767);
        LibretroHWBridge.SetInput(b, lx, ly);
    }

    // 2D audio mirroring AoJ's proven MAME path (LibretroScreenController): an AUTHORED AudioSource
    // (RequireComponent — never AddComponent at runtime) plays a silent looping keep-alive clip, and
    // OnAudioFilterRead on this same GameObject overwrites the source's output with the core's PCM.
    // Runtime-added sources and streaming-clip readers were both device-confirmed silent on Quest
    // (isPlaying=true but the callback fired 0×) — the source was never pulled into the DSP graph.
    void SetupAudio()
    {
        LibretroHWBridge.SetAudioOutputRate(AudioSettings.outputSampleRate);
        _audio = GetComponent<AudioSource>();   // RequireComponent guarantees an authored instance
        _audio.playOnAwake = false;
        _audio.loop        = true;

        int sr = AudioSettings.outputSampleRate;
        // Silent keep-alive: just enough to keep the source "playing" so the filter chain runs. Its
        // samples are ignored — OnAudioFilterRead replaces the whole buffer with our PCM.
        _keepAlive = AudioClip.Create("_PdFlycastKeepAlive", sr, 2, sr, false);
        _audio.clip = _keepAlive;
        EnsureAudioPlaying();
        Status($"audio: output rate {sr} Hz, core {LibretroHWBridge.SampleRate:F0} Hz, isPlaying={_audio.isPlaying}");

        if (dumpAudioWav)
        {
            _dumpRate = sr;
            int floats = Mathf.Max(1, Mathf.CeilToInt(sr * 2f * Mathf.Max(1f, dumpSeconds)));
            _dumpBuf = new float[floats]; _dumpPos = 0; _dumpWritten = false;
            Status($"audio WAV capture armed: up to {dumpSeconds:F0}s → pdflycast_audio.wav ({floats} floats)");
        }
    }

    // Re-arm the source whenever it isn't playing — the Meta XR spatializer or an HMD-unmount/focus
    // change can stop it. Called each frame.
    void EnsureAudioPlaying()
    {
        if (_audio == null || _audio.isPlaying) return;
        _audio.spatialize        = false;   // OVR spatializer would mangle the stream
        _audio.bypassReverbZones = true;
        _audio.reverbZoneMix     = 0f;
        _audio.spatialBlend      = 0f;       // pure 2D
        _audio.Play();
    }

    // Unity audio thread (DSP filter on this GO's AudioSource): overwrite `data` with the core's PCM;
    // zero-fill underflow. The native buffer is interleaved stereo, matching channels==2 (Quest default).
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!started || !audioEnabled) { System.Array.Clear(data, 0, data.Length); return; }
        int n = (channels == 2) ? LibretroHWBridge.AudioRead(data) : 0;   // only stereo layout matches the buffer
        for (int i = n; i < data.Length; i++) data[i] = 0f;

        // Capture what Unity actually plays (post zero-fill) into the WAV buffer — audio-thread safe
        // (just an array copy; no allocation, no I/O). Stops when the preallocated buffer is full.
        if (_dumpBuf != null && _dumpPos < _dumpBuf.Length)
        {
            int c = Math.Min(_dumpBuf.Length - _dumpPos, data.Length);
            Array.Copy(data, 0, _dumpBuf, _dumpPos, c);
            _dumpPos += c;
        }

        // Continuous (throttled) so logcat shows whether the pull is sustained, not just the first frames.
        if (_audioLogN < 3 || (_audioLogN % 500) == 0)
            ConfigManager.WriteConsole($"[PdFlycast] OnAudioFilterRead #{_audioLogN} len={data.Length} ch={channels} got={n}");
        _audioLogN++;
    }
    int _audioLogN;

    // Write the captured PCM as a 16-bit mono-interleaved-stereo WAV. Main thread only (called from
    // Update when full, or OnDisable for a partial capture). Writes once.
    void WriteWavDump()
    {
        if (_dumpBuf == null || _dumpWritten) return;
        _dumpWritten = true;
        int samples = Math.Min(_dumpPos, _dumpBuf.Length);   // total floats captured
        string path = Path.Combine(Application.persistentDataPath, "pdflycast_audio.wav");
        try
        {
            const int channels = 2, bits = 16;
            int rate = _dumpRate > 0 ? _dumpRate : 48000;
            int dataBytes = samples * (bits / 8);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var bw = new BinaryWriter(fs))
            {
                bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataBytes);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);                                   // PCM fmt chunk size
                bw.Write((short)1);                             // PCM
                bw.Write((short)channels);
                bw.Write(rate);
                bw.Write(rate * channels * (bits / 8));         // byte rate
                bw.Write((short)(channels * (bits / 8)));       // block align
                bw.Write((short)bits);
                bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                bw.Write(dataBytes);
                for (int i = 0; i < samples; i++)
                    bw.Write((short)Mathf.RoundToInt(Mathf.Clamp(_dumpBuf[i], -1f, 1f) * 32767f));
            }
            Status($"audio WAV dump written: {path} ({samples / (float)(channels * Mathf.Max(1, rate)):F1}s, {samples} floats)");
        }
        catch (Exception e) { Status("audio WAV dump FAILED: " + e.Message); }
    }

    void Blit(IntPtr pixels, int w, int h)
    {
        EnsureTexture(w, h);
        int bytes = w * h * 4;
        if (flipY)
        {
            if (_flipBuf == null || _flipBuf.Length != bytes) _flipBuf = new byte[bytes];
            int row = w * 4;
            // copy source row r into dest row (h-1-r) → vertical flip
            for (int r = 0; r < h; r++)
                System.Runtime.InteropServices.Marshal.Copy(IntPtr.Add(pixels, r * row), _flipBuf, (h - 1 - r) * row, row);
            _tex.LoadRawTextureData(_flipBuf);
        }
        else
        {
            _tex.LoadRawTextureData(pixels, bytes);
        }
        _tex.Apply(false, false);
    }

    void EnsureTexture(int w, int h)
    {
        if (_tex != null && _tex.width == w && _tex.height == h) return;
        if (_tex != null) Destroy(_tex);
        // Flycast set_image is R8G8B8A8_UNORM → RGBA32.
        _tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
        {
            wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "PdFlycastFrame",
        };
        BindTexture(_tex);
    }

    // Zero-copy: orient AND crop the sampled AHB texture via the material's UV transform — the
    // external texture can't be flipped/cropped in memory like the CPU path. The AHB buffer is the
    // fixed ceiling (640x480); the core may emit a smaller active frame (e.g. Metal Slug 6's 640x238
    // boot mode) that native blits into the top-left. This maps the quad's full UV range onto just
    // the active sub-rect, composed with flipX/flipY. When active == buffer (DOA2, in-game Metal
    // Slug) it reduces exactly to the old plain-flip transform — full-frame titles are unchanged.
    //
    // Convention (device-verified for the full frame at flipX=true, flipY=false): the Vulkan external
    // texture samples V flipped (V'=1 → image row 0 = frame top) and U unflipped (U'=0 → col 0 =
    // frame left); content sits at the top-left, so the active band is U'∈[0,au), V'∈[1-av,1].
    void ApplyZeroCopyCrop(int activeW, int activeH, int bufW, int bufH)
    {
        float au = (bufW > 0) ? Mathf.Clamp01((float)activeW / bufW) : 1f;
        float av = (bufH > 0) ? Mathf.Clamp01((float)activeH / bufH) : 1f;
        if (au <= 0f) au = 1f;
        if (av <= 0f) av = 1f;
        Vector2 scale  = new Vector2(flipX ? -au : au, flipY ? -av : av);
        Vector2 offset = new Vector2(flipX ?  au : 0f, flipY ?  1f  : 1f - av);
        _material.mainTextureScale  = scale;
        _material.mainTextureOffset = offset;
        if (_material.HasProperty("_EmissionMap"))
        {
            _material.SetTextureScale("_EmissionMap", scale);
            _material.SetTextureOffset("_EmissionMap", offset);
        }
        _cropActiveW = activeW; _cropActiveH = activeH;
        Status($"zero-copy crop: active={activeW}x{activeH} buffer={bufW}x{bufH} au={au:F3} av={av:F3}");
    }

    // Bind a texture as both albedo and emission so the screen is lit regardless of room lighting.
    void BindTexture(Texture tex)
    {
        _material.mainTexture = tex;
        if (_material.HasProperty("_EmissionMap"))
        {
            _material.EnableKeyword("_EMISSION");
            _material.SetTexture("_EmissionMap", tex);
            _material.SetColor("_EmissionColor", Color.white);
        }
    }

    void Status(string msg)
    {
        Debug.Log($"[PdFlycast] {msg}");
        ConfigManager.WriteConsole($"[PdFlycast] {msg}");
        if (!verboseStatus) return;
        try { File.AppendAllText(_statusPath, $"{Time.frameCount} {Time.unscaledTime:F2} {msg}\n"); }
        catch (Exception e) { Debug.LogWarning("[PdFlycast] status write failed: " + e.Message); }
    }

    // Pause policy (decided 2026-07-03): the emulator suspends with the app. Unity stops pulling
    // OnAudioFilterRead while paused, so running through it just dropped every sample and advanced
    // the game invisibly (the "heard nothing live / too fast" confusion) — RetroArch pauses too.
    void OnApplicationPause(bool paused)
    {
        if (!started) return;
        LibretroHWBridge.SetPaused(paused);
        Status(paused ? "app paused — emu pump suspended" : "app resumed — emu pump running");
    }

    void OnDisable()
    {
        // Stop the audio pull first so OnAudioFilterRead (audio thread) won't call into native
        // during/after shutdown.
        started = false;
        if (_audio != null) { _audio.Stop(); _audio.clip = null; }
        if (_keepAlive != null) { Destroy(_keepAlive); _keepAlive = null; }

        // Flush whatever was captured (partial is fine) now that the audio thread has stopped pulling.
        WriteWavDump();

        // Destroy our textures BEFORE Shutdown: the zero-copy textures are external, wrapping the
        // Unity-side VkImages that Shutdown tears down.
        if (_tex != null) { Destroy(_tex); _tex = null; }
        if (_zcTexes != null) { foreach (var t in _zcTexes) if (t != null) Destroy(t); _zcTexes = null; }
        LibretroHWBridge.Shutdown();
        _importIssued = false;
        _extTexReady = false;
    }
}
