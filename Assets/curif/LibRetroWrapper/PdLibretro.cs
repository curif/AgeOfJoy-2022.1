using System;
using System.Runtime.InteropServices;
using UnityEngine;

// PdLibretro — managed binding for the embedded libretro frontend (libpdlr.so).
//
// Stage 2a-lite: probe a libretro core (load + bind + init + system info). Later stages add the
// Vulkan run loop and the AHB→Unity frame handoff. On-device only; no-op in the Windows editor.
// See claudedocs/geometrizer_vulkan_cores.md.
public static class PdLibretro
{
    const string LIB = "pdlr";

    [DllImport(LIB, EntryPoint = "pdlr_probe", CharSet = CharSet.Ansi)]
    static extern int _pdlr_probe([MarshalAs(UnmanagedType.LPStr)] string corePath);

    [DllImport(LIB, EntryPoint = "pdlr_core_name")]
    static extern IntPtr _pdlr_core_name();

    [DllImport(LIB, EntryPoint = "pdlr_core_version")]
    static extern IntPtr _pdlr_core_version();

    [DllImport(LIB, EntryPoint = "pdlr_start", CharSet = CharSet.Ansi)]
    static extern int _pdlr_start([MarshalAs(UnmanagedType.LPStr)] string corePath,
                                  [MarshalAs(UnmanagedType.LPStr)] string systemDir,
                                  [MarshalAs(UnmanagedType.LPStr)] string saveDir,
                                  [MarshalAs(UnmanagedType.LPStr)] string gamePath);

    [DllImport(LIB, EntryPoint = "pdlr_run")]
    static extern int _pdlr_run();

    [DllImport(LIB, EntryPoint = "pdlr_set_paused")]
    static extern void _pdlr_set_paused(int paused);

    [DllImport(LIB, EntryPoint = "pdlr_get_frame")]
    static extern int _pdlr_get_frame(out IntPtr pixels, out int width, out int height);

    [DllImport(LIB, EntryPoint = "pdlr_frame_count")]
    static extern int _pdlr_frame_count();

    [DllImport(LIB, EntryPoint = "pdlr_set_input")]
    static extern void _pdlr_set_input(uint buttons, short lx, short ly);

    // Stage 2c — zero-copy AHB handoff to Unity's device.
    [DllImport(LIB, EntryPoint = "pdlr_set_zero_copy")]
    static extern void _pdlr_set_zero_copy(int enabled);

    [DllImport(LIB, EntryPoint = "pdlr_zero_copy_active")]
    static extern int _pdlr_zero_copy_active();

    [DllImport(LIB, EntryPoint = "pdlr_frame_size")]
    static extern int _pdlr_frame_size(out int width, out int height);

    [DllImport(LIB, EntryPoint = "pdlr_frame_fps")]
    static extern double _pdlr_frame_fps();

    [DllImport(LIB, EntryPoint = "pdlr_sample_rate")]
    static extern double _pdlr_sample_rate();

    [DllImport(LIB, EntryPoint = "pdlr_audio_set_output_rate")]
    static extern void _pdlr_audio_set_output_rate(int hz);

    [DllImport(LIB, EntryPoint = "pdlr_audio_read")]
    static extern int _pdlr_audio_read([In, Out] float[] dst, int count);

    [DllImport(LIB, EntryPoint = "pdlr_GetRenderEventFunc")]
    static extern IntPtr _pdlr_GetRenderEventFunc();

    [DllImport(LIB, EntryPoint = "pdlr_unity_image_ready")]
    static extern int _pdlr_unity_image_ready();

    [DllImport(LIB, EntryPoint = "pdlr_buffer_count")]
    static extern int _pdlr_buffer_count();

    [DllImport(LIB, EntryPoint = "pdlr_ready_buffer_index")]
    static extern int _pdlr_ready_buffer_index();

    [DllImport(LIB, EntryPoint = "pdlr_get_unity_image_ptr_at")]
    static extern IntPtr _pdlr_get_unity_image_ptr_at(int idx);

    [DllImport(LIB, EntryPoint = "pdlr_shutdown")]
    static extern void _pdlr_shutdown();

    public static bool   Available { get; private set; }
    public static string LastError { get; private set; } = "";
    public static string PreloadInfo { get; private set; } = "(not run)";

    static bool _probed;

    static void Preload()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var sys = new AndroidJavaClass("java.lang.System"))
                sys.CallStatic("loadLibrary", "pdlr");
            PreloadInfo = "System.loadLibrary('pdlr') OK";
        }
        catch (Exception e) { PreloadInfo = "System.loadLibrary failed: " + e.Message; }
#else
        PreloadInfo = "(not Android — skipped)";
#endif
        ConfigManager.WriteConsole($"[PdLibretro.Preload] {PreloadInfo}");
    }

    // The app's native library dir, where Unity packages plugin .so files (so a bundled Flycast
    // core can be dlopen'd by absolute path). Empty off Android.
    public static string NativeLibraryDir()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var appInfo = activity.Call<AndroidJavaObject>("getApplicationInfo"))
                return appInfo.Get<string>("nativeLibraryDir");
        }
        catch (Exception e) { ConfigManager.WriteConsole($"[PdLibretro] nativeLibraryDir failed: {e.Message}"); }
#endif
        return "";
    }

    static bool EnsureAvailable()
    {
        if (_probed) return Available;
        Preload();
        try
        {
            // Harmless null probe: returns -1 without touching a core.
            _pdlr_probe(null);
            Available = true; LastError = "";
        }
        catch (DllNotFoundException e)   { Available = false; LastError = "DllNotFound: " + e.Message; }
        catch (EntryPointNotFoundException e) { Available = false; LastError = "EntryPointNotFound: " + e.Message; }
        _probed = true;
        ConfigManager.WriteConsole($"[PdLibretro] Available={Available} lastError='{LastError}'");
        return Available;
    }

    // Load + initialize the core at corePath. Returns true on success.
    public static bool Probe(string corePath)
    {
        if (!EnsureAvailable()) return false;
        return _pdlr_probe(corePath) == 0;
    }

    public static string CoreName    => Available ? Marshal.PtrToStringAnsi(_pdlr_core_name())    : null;
    public static string CoreVersion => Available ? Marshal.PtrToStringAnsi(_pdlr_core_version()) : null;

    // Stage 2b: load + negotiate Vulkan + load the game. Returns true on success.
    public static bool Start(string corePath, string systemDir, string saveDir, string gamePath)
    {
        if (!EnsureAvailable()) return false;
        return _pdlr_start(corePath, systemDir, saveDir, gamePath) == 0;
    }

    // One retro_run tick. Returns true on success.
    public static bool Run() => Available && _pdlr_run() == 0;

    // Suspend/resume the native emu pump (wire to OnApplicationPause — headset off / system
    // overlay). Paused = the guest stops advancing; native resyncs its schedule on resume.
    public static void SetPaused(bool paused) { if (Available) _pdlr_set_paused(paused ? 1 : 0); }

    // Latest CPU-read-back frame: tightly packed RGBA8 + dimensions. Valid after Run().
    public static bool GetFrame(out IntPtr pixels, out int width, out int height)
    {
        pixels = IntPtr.Zero; width = 0; height = 0;
        if (!Available) return false;
        return _pdlr_get_frame(out pixels, out width, out height) == 0;
    }

    // Frames the core has rendered so far (set_image calls).
    public static int FrameCount => Available ? _pdlr_frame_count() : 0;

    // RetroPad button bit indices (bit N == RETRO_DEVICE_ID_JOYPAD_N). DEBUG input only.
    public static class Joypad
    {
        public const int B = 0, Y = 1, SELECT = 2, START = 3;
        public const int UP = 4, DOWN = 5, LEFT = 6, RIGHT = 7;
        public const int A = 8, X = 9, L = 10, R = 11, L2 = 12, R2 = 13, L3 = 14, R3 = 15;
    }

    // DEBUG: push the current gamepad state (buttons bitmask + left analog stick) for the next Run().
    // No-op until the core is available. Call once per frame before Run().
    public static void SetInput(uint buttons, short lx, short ly)
    {
        if (!Available) return;
        _pdlr_set_input(buttons, lx, ly);
    }

    public static void Shutdown()
    {
        if (!Available) return;
        _pdlr_shutdown();
    }

    // --- Stage 2c: zero-copy AHB handoff to Unity's device ---

    // Event id for GL.IssuePluginEvent (must match PDLR_EVENT_IMPORT_AHB in libpdlr).
    public const int EVENT_IMPORT_AHB = 1;

    // Select the frame transport. Call BEFORE Start(): it decides which device extensions the core
    // is asked to enable. enabled=false → legacy CPU read-back (GetFrame); true → AHB zero-copy.
    public static void SetZeroCopy(bool enabled)
    {
        if (!EnsureAvailable()) return;
        _pdlr_set_zero_copy(enabled ? 1 : 0);
    }

    // Whether zero-copy is actually active. After Start() this reflects auto-fallback — false means
    // the core's device couldn't get the AHB extensions, so the caller should use the CPU path.
    public static bool ZeroCopyActive => Available && _pdlr_zero_copy_active() != 0;

    // The core's declared native video rate (Hz) — Dreamcast NTSC ≈ 59.94. 0 if unknown (valid after Start).
    public static double FrameFps => Available ? _pdlr_frame_fps() : 0.0;

    // The core's declared audio sample rate (Hz). 0 if unknown (valid after Start).
    public static double SampleRate => Available ? _pdlr_sample_rate() : 0.0;

    // Tell the native side Unity's output rate so it resamples the core's PCM to match. Call at start.
    public static void SetAudioOutputRate(int hz) { if (Available) _pdlr_audio_set_output_rate(hz); }

    // Pull buffered audio into dst (interleaved stereo). Returns floats written (< dst.Length on
    // underflow). Call from OnAudioFilterRead; zero-fill the remainder for silence.
    public static int AudioRead(float[] dst) => (Available && dst != null) ? _pdlr_audio_read(dst, dst.Length) : 0;

    // Emulated frame dimensions from the core's video_refresh. False until the first frame.
    public static bool FrameSize(out int width, out int height)
    {
        width = 0; height = 0;
        if (!Available) return false;
        return _pdlr_frame_size(out width, out height) == 0;
    }

    // Native render-event callback for GL.IssuePluginEvent. Issue EVENT_IMPORT_AHB once (after
    // frames flow) to import the AHB onto Unity's VkDevice on the render thread.
    public static IntPtr GetRenderEventFunc() => Available ? _pdlr_GetRenderEventFunc() : IntPtr.Zero;

    // True once the import event has produced all the Unity-side VkImages (triple-buffered).
    public static bool UnityImagesReady => Available && _pdlr_unity_image_ready() != 0;

    // Number of AHB buffers — create this many external textures.
    public static int BufferCount => Available ? _pdlr_buffer_count() : 0;

    // Buffer index to display this frame (most recent completed blit). -1 until the first frame.
    public static int ReadyBufferIndex => Available ? _pdlr_ready_buffer_index() : -1;

    // Pointer to buffer idx's Unity-device VkImage handle (a VkImage*) for Texture2D.CreateExternalTexture.
    // IntPtr.Zero until UnityImagesReady. Stays valid until Shutdown().
    public static IntPtr GetUnityImagePtr(int idx) => Available ? _pdlr_get_unity_image_ptr_at(idx) : IntPtr.Zero;
}
