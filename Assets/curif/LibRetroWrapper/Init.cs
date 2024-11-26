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

// check Project settings -> Script execution order.
[DefaultExecutionOrder(-500)] // This will ensure that this script executes before others
public class Init : MonoBehaviour
{

//public class Init
//{
    public static bool PermissionGranted = false;

    //    void Start()

    //https://docs.unity3d.com/ScriptReference/RuntimeInitializeOnLoadMethodAttribute-ctor.html
    //[RuntimeInitializeOnLoadMethod]
    //static void OnRuntimeMethodLoad()

    private CabinetDBAdmin cabinetDBAdmin;

    void Awake()
    {
        ConfigManager.WriteConsole("[Init] +++++++++++++++++++++  Initialize  +++++++++++++++++++++");

        Application.lowMemory += OnLowMemory;
        Application.memoryUsageChanged += OnMemoryUsageChanged;

        cabinetDBAdmin = GetComponent<CabinetDBAdmin>();

        if (ConfigManager.ShouldUseInternalStorage())
        {
            ConfigManager.WriteConsole("[Init] init folders names (private)");
            PermissionGranted = true;
            ConfigManager.InitFolders(false);
            loadOperations();
        }
        else
        {
            ConfigManager.WriteConsole("[Init] ShouldUseInternalStorage");
            if (havePublicStorageAccess())
            {
                ConfigManager.WriteConsole("[Init] Already authorized, init public folders.");
                PermissionGranted = true;
                ConfigManager.InitFolders(true);
                loadOperations();
            }
            else
            {
                ConfigManager.WriteConsole("[Init] Async ask for permissions.");
                askForPublicStoragePermissions();
            }
        }

        ConfigManager.WriteConsole("+++++++++++++++++++++ initialization ends");
    }

    private void OnMemoryUsageChanged(in ApplicationMemoryUsageChange usage)
    {
        ConfigManager.WriteConsole("+++++++++++++++++++++ OnMemoryUsageChanged");
        ConfigManager.WriteConsole($"+++++++++++++++++++++++ {usage.memoryUsage}");
    }

    private void OnLowMemory()
    {
        ConfigManager.WriteConsole("+++++++++++++++++++++ OnLowMemory START");
        ResourceCacheManager.FreeResources();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        ConfigManager.WriteConsole("+++++++++++++++++++++ OnLowMemory END");
    }

    void loadOperations()
    {
        ConfigManager.WriteConsole("[Init] Loading cabinets");
        cabinetDBAdmin.loadCabinets();
    }
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

}
