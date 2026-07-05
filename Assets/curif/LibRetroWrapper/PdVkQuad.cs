using System.IO;
using UnityEngine;

// PdVkQuad — drives the isolated Vulkan context (libpdvk.so via PdVk) onto this object's material.
//
// Two display paths:
//  • ZERO-COPY (Step B): import the plugin's AHardwareBuffer onto Unity's VkDevice (via a one-time
//    GL.IssuePluginEvent), wrap it with Texture2D.CreateExternalTexture, and let the quad sample the
//    shared GPU image directly — no CPU copy. This is the target path.
//  • CPU FALLBACK (Milestone 1): pdvk_get_pixels → LoadRawTextureData each frame. Used if the
//    zero-copy import fails (e.g. Unity's device lacks the AHB extension — see the native
//    [unity-import] log), so we still get a picture and a clear diagnostic.
//
// See claudedocs/geometrizer_vulkan_cores.md. On-device only: in the Windows editor PdVk.Available
// is false and this no-ops. DEBUG-ONLY test rig — attach to a debug quad ("FrameBuffer" in
// IntroGalleryExterior) and remove before release. Success = the quad shows a checkerboard that SPINS.
[RequireComponent(typeof(Renderer))]
public class PdVkQuad : MonoBehaviour
{
    enum Mode { WaitingImport, ZeroCopy, CpuFallback }

    [Header("Render target")]
    [Tooltip("Offscreen checkerboard resolution. Square looks best; any size works.")]
    public int width  = 512;
    public int height = 512;

    [Tooltip("How often to render (and, in CPU fallback, read back), in Hz. 0 = every Unity frame. " +
             "Rotation speed is unaffected (Render() advances by real elapsed time).")]
    public float targetHz = 30f;

    [Header("Step B")]
    [Tooltip("Try the zero-copy AHB import into Unity's device. If it fails within the timeout, " +
             "fall back to the CPU read-back path.")]
    public bool useZeroCopy = true;
    public float importTimeoutSec = 4f;

    [Header("Status (read-only)")]
    public string mode;
    public bool   running;
    public uint   framesBlitted;

    Renderer  _renderer;
    Material  _material;     // instanced material we own
    Texture2D _tex;          // CPU-fallback texture (we own + upload)
    Texture2D _extTex;       // zero-copy external texture (wraps the shared VkImage)
    Mode      _mode;
    float     _importIssuedAt;
    bool      _initFailed;
    string    _statusPath;
    float     _nextStatusAt;
    float     _lastRenderAt;

    void OnEnable()
    {
        _statusPath = Path.Combine(Application.persistentDataPath, "pdvk_status.txt");
        Status($"OnEnable on '{name}'. persistentDataPath={Application.persistentDataPath}");

        _renderer = GetComponent<Renderer>();
        _material = _renderer.material; // instance, not sharedMaterial — safe to mutate

        if (!PdVk.Init(width, height))
        {
            _initFailed = true;
            Status($"PdVk.Init({width}x{height}) failed — Available={PdVk.Available} " +
                   $"preload='{PdVk.PreloadInfo}' lastError='{PdVk.LastError}'");
            return;
        }

        _lastRenderAt = Time.unscaledTime;   // anchor the throttle so the first dt isn't huge
        running = true;

        if (useZeroCopy && PdVk.GetRenderEventFunc() != System.IntPtr.Zero)
        {
            // Ask the plugin to import its AHB onto Unity's device, on the render thread.
            GL.IssuePluginEvent(PdVk.GetRenderEventFunc(), PdVk.EVENT_IMPORT_AHB);
            _mode = Mode.WaitingImport;
            _importIssuedAt = Time.unscaledTime;
            Status($"PdVk.Init OK ({width}x{height}); issued IMPORT_AHB — waiting for zero-copy import");
        }
        else
        {
            EnsureCpuTexture(width, height);
            _mode = Mode.CpuFallback;
            Status($"PdVk.Init OK ({width}x{height}); zero-copy disabled — CPU read-back path");
        }
        mode = _mode.ToString();
    }

    void Update()
    {
        if (_initFailed || !PdVk.Available) return;

        // Resolve the zero-copy import (poll every frame, independent of the render throttle).
        if (_mode == Mode.WaitingImport)
        {
            if (PdVk.UnityImageReady)                                          CreateZeroCopyTexture();
            else if (Time.unscaledTime - _importIssuedAt > importTimeoutSec)   FallBackToCpu();
        }

        // Throttle the actual render (+ CPU read-back in the fallback path).
        float now = Time.unscaledTime;
        float dt  = now - _lastRenderAt;
        if (targetHz > 0f && dt < 1f / targetHz) return;
        _lastRenderAt = now;

        bool tick = now >= _nextStatusAt;

        if (!PdVk.Render(dt))   // draws into the AHB on the plugin's device — needed by BOTH paths
        {
            if (tick) { _nextStatusAt = now + 1f; Status("PdVk.Render returned false (see native 'pdvk' log)"); }
            return;
        }

        // Zero-copy: the quad samples the shared image directly, nothing to upload. CPU fallback:
        // pull the pixels and upload them.
        if (_mode == Mode.CpuFallback)
        {
            if (!PdVk.GetPixels(out System.IntPtr pixels, out int w, out int h) ||
                pixels == System.IntPtr.Zero || w <= 0 || h <= 0)
            {
                if (tick) { _nextStatusAt = now + 1f; Status($"PdVk.GetPixels bad (ptr={pixels} {w}x{h})"); }
                return;
            }
            EnsureCpuTexture(w, h);
            _tex.LoadRawTextureData(pixels, w * h * 4);   // tightly-packed RGBA8 from the plugin
            _tex.Apply(false, false);
        }

        framesBlitted++;
        if (tick)
        {
            _nextStatusAt = now + 1f;
            Status($"mode={_mode} running={running} blitted={framesBlitted} fb={width}x{height}");
        }
    }

    // Wrap the imported AHB (now a VkImage on Unity's device) as an external texture. The native
    // ptr is a VkImage* (a pointer to the handle) — passing the raw handle is the classic mistake
    // that yields "Graphics device is null".
    void CreateZeroCopyTexture()
    {
        System.IntPtr imgPtr = PdVk.GetUnityImagePtr();
        if (imgPtr == System.IntPtr.Zero)
        {
            Status("zero-copy: image ready but ptr null — falling back to CPU");
            FallBackToCpu();
            return;
        }
        if (_tex != null) { Destroy(_tex); _tex = null; }

        _extTex = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, imgPtr);
        _extTex.wrapMode   = TextureWrapMode.Clamp;
        _extTex.filterMode = FilterMode.Bilinear;
        BindTexture(_extTex);
        _mode = Mode.ZeroCopy;
        mode  = _mode.ToString();
        Status($"ZERO-COPY active: CreateExternalTexture {width}x{height} ptr={imgPtr}");
    }

    void FallBackToCpu()
    {
        Status("zero-copy import did not complete — using CPU read-back path");
        EnsureCpuTexture(width, height);
        _mode = Mode.CpuFallback;
        mode  = _mode.ToString();
    }

    void EnsureCpuTexture(int w, int h)
    {
        if (_tex != null && _tex.width == w && _tex.height == h) return;
        if (_tex != null) Destroy(_tex);

        // Plugin writes R8G8B8A8_UNORM (byte order R,G,B,A) → TextureFormat.RGBA32.
        _tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
        {
            wrapMode   = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            name       = "PdVkCheckerboard",
        };
        BindTexture(_tex);
    }

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

    // Echo to logcat (Unity tag) + AoJ's console + a device status file (survives logcat throttle).
    void Status(string msg)
    {
        Debug.Log($"[PdVkQuad] {msg}");
        ConfigManager.WriteConsole($"[PdVkQuad] {msg}");
        try { File.AppendAllText(_statusPath, $"{Time.frameCount} {Time.unscaledTime:F2} {msg}\n"); }
        catch (System.Exception e) { Debug.LogWarning("[PdVkQuad] status write failed: " + e.Message); }
    }

    void OnDisable()
    {
        PdVk.Shutdown();
        running = false;
        if (_extTex != null) { Destroy(_extTex); _extTex = null; }
        if (_tex    != null) { Destroy(_tex);    _tex    = null; }
    }
}
