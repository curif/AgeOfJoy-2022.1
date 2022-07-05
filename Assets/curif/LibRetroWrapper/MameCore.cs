using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class retro_system_info {
    public string library_name;    
    public string library_version;  
    public string valid_extensions; 
    public bool need_fullpath;
    public bool block_extract;

    public override string ToString() {
        return String.Format("Library Name: {0} \n Library Version: {1}\n valid_extensions : {2} need_fullpath: {3}\n",
        library_name, library_version, valid_extensions, need_fullpath, block_extract);
    }
}

public sealed class MameCore {
    private readonly static MameCore _instance = new MameCore();
 
    private MameCore() {}
 
    public static MameCore Instance {
        get {
            return _instance;
        }
    }

    private IntPtr _libPt = IntPtr.Zero;

    [DllImport("dl", EntryPoint = "dlopen")]
    private static extern IntPtr dlopen (String fileName, int flags);

    [DllImport("dl", EntryPoint = "dlsym")]
    private static extern IntPtr dlsym (IntPtr handle, String symbol);

    [DllImport("dl", EntryPoint = "dlclose")]
    private static extern int dlclose (IntPtr handle);

    [DllImport("dl", EntryPoint = "dlerror")]
    private static extern IntPtr dlerror ();

    public void Init(string core) {
        Debug.Log(String.Format("Init core {0} ", core));
        dlerror(); //clean unread errors
        _libPt = dlopen(core, 2);
        if (_libPt == IntPtr.Zero) {
            IntPtr errStrPt = dlerror();
            if (errStrPt != IntPtr.Zero) {
                throw new ArgumentException(String.Format("can't open core {0}, exists the library file? last registered error: [{1}]", core, Marshal.PtrToStringAnsi(errStrPt)));
            }
            else  {
                throw new ArgumentException(String.Format("can't open core {0}, exists the library file?, undefined error", core));
            }
        }
        initDelegates();
    }

    public void DeInit() {
        if (_libPt == IntPtr.Zero) {
            return;
        }
        Debug.Log("Close Mame");
        dlerror(); //clean unread errors
        dlclose(_libPt);
        IntPtr errStrPt = dlerror();
        if (errStrPt != IntPtr.Zero) {
            throw new ArgumentException(String.Format("Error close Mame Core Library, {0}", Marshal.PtrToStringAnsi (errStrPt)));
        }
        _libPt = IntPtr.Zero;
    }


    private IntPtr getFunctionPointer(string funct) {
        Debug.Log(String.Format("Init function pointer for {0} ", funct));
        dlerror(); //clean unread errors
        IntPtr fnctPt = dlsym(_libPt, funct);
        if (fnctPt == IntPtr.Zero) {
            IntPtr errStrPt = dlerror();
            string strErr = "undefined error";
            if (errStrPt != IntPtr.Zero) {
                strErr = Marshal.PtrToStringAnsi(errStrPt);
            }
            throw new ArgumentException(String.Format("Error finding function in core library {0} - {1}", funct, strErr));
        }
        return fnctPt;
    }

    public delegate void getSystemInfoDelegate(retro_system_info i);
    public getSystemInfoDelegate retro_get_system_info;
    
    public delegate uint APIVersionSignature();
    public APIVersionSignature retro_api_version;

    // retro_set_environment ------------------
    public enum EnvironmentCommands {
        RETRO_ENVIRONMENT_SET_MESSAGE = 6,
        RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY = 9,
        RETRO_ENVIRONMENT_SET_PIXEL_FORMAT = 10,
        RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS = 11,
        RETRO_ENVIRONMENT_GET_VARIABLE = 15,
        RETRO_ENVIRONMENT_SET_VARIABLES = 16,
        RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE = 17,
        RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY = 31,
        RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK = 62,
        RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY = 63
    }
    public delegate void SetEnvironmentHandler(EnvironmentHandlerCB env);
    public delegate bool EnvironmentHandlerCB(uint cmd, IntPtr data); //callback
    public SetEnvironmentHandler retro_set_environment;
    private void initDelegates() {
        retro_get_system_info = (getSystemInfoDelegate)Marshal.GetDelegateForFunctionPointer(getFunctionPointer("retro_get_system_info"), typeof(getSystemInfoDelegate));
        retro_api_version = (APIVersionSignature)Marshal.GetDelegateForFunctionPointer(getFunctionPointer("retro_api_version"), typeof(APIVersionSignature));
        retro_set_environment = (SetEnvironmentHandler)Marshal.GetDelegateForFunctionPointer(getFunctionPointer("retro_set_environment"), typeof(SetEnvironmentHandler));
    }
    
}
