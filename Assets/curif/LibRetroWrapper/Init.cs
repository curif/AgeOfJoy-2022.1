/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Android;
using System.Diagnostics;
using System;
using System.Text;
using UnityEngine.SceneManagement;

// check Project settings -> Script execution order.
[DefaultExecutionOrder(-500)] // This will ensure that this script executes before others
public class Init : MonoBehaviour
{

    //public static bool PermissionGranted = false;

    private CabinetDBAdmin cabinetDBAdmin;

    /*
    static Init()
    {
        start();
    }
    */
    void Awake()
    {
        /*
        if (!PermissionGranted)
        {
            ConfigManager.WriteConsole("[Init] Async ask for permissions.");
            askForPublicStoragePermissions();
        }
        */
        cabinetDBAdmin = GetComponent<CabinetDBAdmin>();
        Application.lowMemory += OnLowMemory;
        Application.memoryUsageChanged += OnMemoryUsageChanged;
        ConfigManager.WriteConsole($"[Init.Memory] Device reference | SystemInfo.graphicsMemorySize: {SystemInfo.graphicsMemorySize}MB | SystemInfo.systemMemorySize: {SystemInfo.systemMemorySize}MB");
        ConfigManager.InitFolders();
        loadOperations();

        // Periodic memory/cache snapshot so real usage trends over a play session
        // can be observed in the logs, to make an informed decision on cache budgets.
        InvokeRepeating(nameof(LogPeriodicMemorySnapshot), 10f, 15f);
    }

    // InvokeRepeating requires a parameterless method.
    private void LogPeriodicMemorySnapshot()
    {
        LogMemorySnapshot("Periodic");
    }
    /*
    private static void start()
    {
        ConfigManager.WriteConsole("[Init] +++++++++++++++++++++  Initialize  +++++++++++++++++++++");

        if (ConfigManager.ShouldUseInternalStorage())
        {
            ConfigManager.WriteConsole("[Init] init folders names (private)");
            PermissionGranted = true;
        }
        else
        {
            ConfigManager.WriteConsole("[Init] ShouldUseInternalStorage");
            if (havePublicStorageAccess())
            {
                ConfigManager.WriteConsole("[Init] Already authorized, init public folders.");
                PermissionGranted = true;
            }
        }

        ConfigManager.WriteConsole("+++++++++++++++++++++ initialization ends");
    }
    */

    private void OnMemoryUsageChanged(in ApplicationMemoryUsageChange usage)
    {
        LogMemorySnapshot($"OnMemoryUsageChanged -> {usage.memoryUsage}");
    }

    private void OnLowMemory()
    {
        LogMemorySnapshot("OnLowMemory (CRITICAL)");
        StartCoroutine(ResourceCacheManager.FreeResourcesAsync());
    }

    // Logs both the app-level cache accounting (per ResourceCache budget/usage/pin
    // count) and the actual engine memory counters, so the two can be compared
    // when deciding whether cache budgets need to change.
    private void LogMemorySnapshot(string context)
    {
        long allocated = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
        long reserved = UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
        long unusedReserved = UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong();
        long monoUsed = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
        long monoHeap = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
        long graphicsDriver = UnityEngine.Profiling.Profiler.GetAllocatedMemoryForGraphicsDriver();

        // "reserved" tracks Unity's native + managed heaps; it does NOT include
        // GPU-only allocations (e.g. textures uploaded via Apply(.., makeNoLongerReadable:
        // true), which is how every cabinet texture ends up). graphicsDriver is the
        // separate counter for that, so comparing it against our own cache size tells
        // us whether the "missing" memory (reserved minus our tracked cache size) is
        // GPU/texture-driven or something else (managed heap, native plugins, audio...).
        ConfigManager.WriteConsole($"[Init.Memory] {context} | allocated: {allocated / (1024f * 1024f):F1}MB | reserved: {reserved / (1024f * 1024f):F1}MB | unusedReserved: {unusedReserved / (1024f * 1024f):F1}MB | mono: {monoUsed / (1024f * 1024f):F1}/{monoHeap / (1024f * 1024f):F1}MB | graphicsDriver: {graphicsDriver / (1024f * 1024f):F1}MB | originalTextures: {DeviceController.originalTextures}");
        ResourceCacheManager.LogAllCacheStatus(context);

        // Loaded (additive) scenes drive which cabinets are instantiated and pinning
        // their textures - if rooms accumulate here instead of unloading via gates,
        // that's the working-set growth, not the cache budget.
        int sceneCount = SceneManager.sceneCount;
        var sb = new StringBuilder();
        sb.Append($"[Init.Memory] {context} | loaded scenes ({sceneCount}): ");
        for (int i = 0; i < sceneCount; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(SceneManager.GetSceneAt(i).name);
        }
        ConfigManager.WriteConsole(sb.ToString());
    }

    void loadOperations()
    {
        ConfigManager.WriteConsole("[Init] Loading cabinets");
        cabinetDBAdmin.loadCabinets();
    }

    /***
     * permission manager deprecated
    internal void onPermissionDenied(string permissionName)
    {
        ConfigManager.WriteConsole($"[Init.onPermissionDenied] DENIED");
        ConfigManager.WriteConsole($"[Init.onPermissionDenied] Can't continue.");
    }
    internal void onPermissionGranted(string permissionName)
    {
        ConfigManager.WriteConsole($"[Init.onPermissionDenied] GRANTED");
        Init.PermissionGranted = true;
        ConfigManager.InitFolders(true);
        loadOperations();
    }
    internal void onPermissionGrantedDontAsk(string permissionName)
    {
        ConfigManager.WriteConsole($"[Init.onPermissionDenied] DENIED AND DON'T ASK ANYMORE");
        ConfigManager.WriteConsole($"[Init.onPermissionDenied] Can't continue.");
    }

    public static bool havePublicStorageAccess()
    {
        bool writeExternal = Permission.HasUserAuthorizedPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        bool readExternal = Permission.HasUserAuthorizedPermission("android.permission.READ_EXTERNAL_STORAGE");
        bool manageExternal = Permission.HasUserAuthorizedPermission("android.permission.MANAGE_EXTERNAL_STORAGE");

        ConfigManager.WriteConsole($"[Init.haveStorageAccess] premission has granted? {writeExternal}");
        return writeExternal && readExternal && manageExternal;
    }

    private void askForPublicStoragePermissions()
    {
        ConfigManager.WriteConsole($"[Init.askForInternalStoragePermissions] asking for permissions to the user");
        //Permission.RequestUserPermission("android.permission.MANAGE_EXTERNAL_STORAGE");
        //Permission.RequestUserPermission("android.permission.READ_EXTERNAL_STORAGE");

        var callbacks = new PermissionCallbacks();
        callbacks.PermissionDenied += onPermissionDenied;
        callbacks.PermissionDeniedAndDontAskAgain += onPermissionGrantedDontAsk;
        callbacks.PermissionGranted += onPermissionGranted;

        //Permission.RequestUserPermission("android.permission.WRITE_EXTERNAL_STORAGE", callbacks);

        string[] permissions = {
            "android.permission.MANAGE_EXTERNAL_STORAGE",
            "android.permission.READ_EXTERNAL_STORAGE",
            "android.permission.WRITE_EXTERNAL_STORAGE"
        };

        //Permission.RequestUserPermissions(permissions, new PermissionHandler());
        Permission.RequestUserPermissions(permissions, callbacks);
        
    }
    */

}
