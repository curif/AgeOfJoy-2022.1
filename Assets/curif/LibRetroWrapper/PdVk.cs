using System;
using System.Runtime.InteropServices;
using UnityEngine;

// PdVk — managed binding for the isolated Vulkan context plugin (libpdvk.so).
//
// Unity side of the C ABI in nativebridge/vulkan_plugin/pdvk.h. The
// native lib owns its OWN VkInstance/VkDevice (Option B — a separate Vulkan context, NOT Unity's
// device), renders a rotating checkerboard offscreen, and reads it back to a CPU-visible buffer
// we copy onto a quad. This is the Milestone-1 / CPU-blit bring-up path; the zero-copy
// AHardwareBuffer import is a later upgrade.
//
// On-device (Quest) only: there is no Vulkan plugin path in the Windows editor, so Available is
// false in-editor and every call no-ops rather than throwing DllNotFoundException.
// See claudedocs/geometrizer_vulkan_cores.md.
public static class PdVk
{
    // On Android Unity strips "lib"/".so": resolves libpdvk.so from the app's native lib dir.
    const string LIB = "pdvk";

    [DllImport(LIB, EntryPoint = "pdvk_init")]
    static extern int _pdvk_init(int width, int height);

    [DllImport(LIB, EntryPoint = "pdvk_render")]
    static extern int _pdvk_render(float dtSeconds);

    [DllImport(LIB, EntryPoint = "pdvk_get_pixels")]
    static extern int _pdvk_get_pixels(out IntPtr pixels, out int width, out int height);

    [DllImport(LIB, EntryPoint = "pdvk_shutdown")]
    static extern void _pdvk_shutdown();

    // Step B — zero-copy handoff to Unity's Vulkan device.
    [DllImport(LIB, EntryPoint = "pdvk_GetRenderEventFunc")]
    static extern IntPtr _pdvk_GetRenderEventFunc();

    [DllImport(LIB, EntryPoint = "pdvk_unity_image_ready")]
    static extern int _pdvk_unity_image_ready();

    [DllImport(LIB, EntryPoint = "pdvk_get_unity_image_ptr")]
    static extern IntPtr _pdvk_get_unity_image_ptr();

    // Event id to pass to GL.IssuePluginEvent (must match PDVK_EVENT_IMPORT_AHB in pdvk.h).
    public const int EVENT_IMPORT_AHB = 1;

    // True once the native lib resolves on this platform. Probed lazily on the first call.
    public static bool   Available { get; private set; }
    public static string LastError { get; private set; } = "";
    public static string PreloadInfo { get; private set; } = "(not run)";

    static bool _probed;
    const int  DLL_PROBE_ATTEMPTS = 24;   // transient-DllNotFound budget for cold lib-load latch
    static int _dllMisses;

    // Bring the .so into the process via the Java loader before the first P/Invoke (IL2CPP's
    // resolver won't expand "pdvk"→libpdvk.so on its own). No-op off Android. Self-contained
    // lib (static libc++) → no extra runtime dependency to preload.
    static void Preload()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var sys = new AndroidJavaClass("java.lang.System"))
                sys.CallStatic("loadLibrary", "pdvk");
            PreloadInfo = "System.loadLibrary('pdvk') OK";
        }
        catch (Exception e) { PreloadInfo = "System.loadLibrary failed: " + e.Message; }
#else
        PreloadInfo = "(not Android — skipped)";
#endif
        ConfigManager.WriteConsole($"[PdVk.Preload] {PreloadInfo}");
    }

    // Probe via pdvk_get_pixels (harmless before init — returns -1, doesn't touch Vulkan).
    // DllNotFound is treated as transient (cold-boot lib-load latch) up to a bounded budget;
    // EntryPointNotFound = stale .so → terminal.
    static bool Probe()
    {
        if (_probed) return Available;
        Preload();
        try
        {
            _pdvk_get_pixels(out _, out _, out _);
            Available = true;
            LastError = "";
            _probed   = true;
        }
        catch (EntryPointNotFoundException e)
        {
            Available = false;
            LastError = "EntryPointNotFound (stale libpdvk.so, rebuild): " + e.Message;
            _probed   = true;
        }
        catch (DllNotFoundException e)
        {
            Available = false;
            _dllMisses++;
            LastError = $"DllNotFound (attempt {_dllMisses}/{DLL_PROBE_ATTEMPTS}): " + e.Message;
            if (_dllMisses >= DLL_PROBE_ATTEMPTS) _probed = true;
        }
        ConfigManager.WriteConsole($"[PdVk.Probe] Available={Available} lastError='{LastError}'");
        return Available;
    }

    // Build the isolated context + an w×h offscreen target. Returns true on success.
    public static bool Init(int width, int height)
    {
        if (!Probe()) return false;
        return _pdvk_init(width, height) == 0;
    }

    // Render one checkerboard frame, advancing rotation by dt seconds.
    public static bool Render(float dtSeconds)
    {
        if (!Available) return false;
        return _pdvk_render(dtSeconds) == 0;
    }

    // Latest frame's pixels (tightly-packed RGBA8, row-major top-to-bottom) + dimensions.
    // The pointer is stable for the context's life; copy after a successful Render().
    public static bool GetPixels(out IntPtr pixels, out int width, out int height)
    {
        pixels = IntPtr.Zero; width = 0; height = 0;
        if (!Available) return false;
        return _pdvk_get_pixels(out pixels, out width, out height) == 0;
    }

    // Tear down the context. Idempotent and safe when nothing is running.
    public static void Shutdown()
    {
        if (!Available) return;
        _pdvk_shutdown();
    }

    // --- Step B: zero-copy handoff to Unity's device ---

    // Native render-event callback for GL.IssuePluginEvent. Issue EVENT_IMPORT_AHB once (after
    // Init) to import the AHB onto Unity's VkDevice on the render thread.
    public static IntPtr GetRenderEventFunc() => Available ? _pdvk_GetRenderEventFunc() : IntPtr.Zero;

    // True once the import event has produced a Unity-side VkImage.
    public static bool UnityImageReady => Available && _pdvk_unity_image_ready() != 0;

    // Pointer to the Unity-device VkImage handle (a VkImage*) for Texture2D.CreateExternalTexture.
    // IntPtr.Zero until UnityImageReady. Stays valid until Shutdown().
    public static IntPtr GetUnityImagePtr() => Available ? _pdvk_get_unity_image_ptr() : IntPtr.Zero;
}
