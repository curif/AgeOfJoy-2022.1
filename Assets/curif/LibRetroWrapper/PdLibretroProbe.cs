using UnityEngine;

// PdLibretroProbe — DEBUG-ONLY: on enable, dlopen + initialize a libretro core via libpdlr and log
// the result. Stage 2a-lite of the Flycast bring-up (claudedocs/geometrizer_vulkan_cores.md): proves
// a stock core loads and initializes in-process under AoJ before any Vulkan/video work.
//
// Attach to any debug GameObject. On-device only (no-op in editor). Watch logcat tag "pdlr" (and
// "flycast" for the core's own logs). Remove before release.
public class PdLibretroProbe : MonoBehaviour
{
    [Tooltip("Core .so filename. If bundled as an AoJ Android plugin it lives in the app's native " +
             "library dir, found automatically. Set an absolute path in coreAbsolutePath to override.")]
    public string coreFileName = "libflycast_libretro_android.so";

    [Tooltip("Optional absolute path override (e.g. a core pushed to the app's files dir).")]
    public string coreAbsolutePath = "";

    [Header("Status (read-only)")]
    public bool   probed;
    public string coreName;
    public string coreVersion;

    void OnEnable()
    {
        string path = !string.IsNullOrEmpty(coreAbsolutePath)
            ? coreAbsolutePath
            : System.IO.Path.Combine(PdLibretro.NativeLibraryDir(), coreFileName);

        ConfigManager.WriteConsole($"[PdLibretroProbe] probing core at '{path}'");
        probed = PdLibretro.Probe(path);

        if (probed)
        {
            coreName    = PdLibretro.CoreName;
            coreVersion = PdLibretro.CoreVersion;
            ConfigManager.WriteConsole($"[PdLibretroProbe] OK — core='{coreName}' version='{coreVersion}'");
        }
        else
        {
            ConfigManager.WriteConsole($"[PdLibretroProbe] FAILED — Available={PdLibretro.Available} " +
                                       $"preload='{PdLibretro.PreloadInfo}' lastError='{PdLibretro.LastError}'");
        }
    }

    void OnDisable()
    {
        PdLibretro.Shutdown();
    }
}
