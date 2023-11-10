/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

//#define _debug_fps_
//#define _debug_audio_
#define _debug_
//#define _serialize_

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/*
this class have a lot of static properties, and because of that we only have one game runing at a time.
you can manage callbacks from the libretro api in methods not static.
there are ways to do it, but there are obscure.
*/
public static unsafe class LibretroMameCore
{
    //IL2CPP does not support marshaling delegates that point to instance methods to native code
    //NotSupportedException: To marshal a managed method, please add an attribute named 'MonoPInvokeCallback' to the method definition. 

    #region INPUT
    public const int RETRO_DEVICE_TYPE_SHIFT = 8;
    public const int RETRO_DEVICE_MASK = (1 << RETRO_DEVICE_TYPE_SHIFT) - 1;
    public const uint RETRO_DEVICE_NONE = 0;
    public const uint RETRO_DEVICE_JOYPAD = 1;
    public const uint RETRO_DEVICE_MOUSE = 2;
    public const uint RETRO_DEVICE_LIGHTGUN = 4;
    public const uint RETRO_DEVICE_ANALOG = 5;

    public const uint RETRO_DEVICE_ID_JOYPAD_B = 0;
    public const uint RETRO_DEVICE_ID_JOYPAD_Y = 1;
    public const uint RETRO_DEVICE_ID_JOYPAD_SELECT = 2;
    public const uint RETRO_DEVICE_ID_JOYPAD_START = 3;
    public const uint RETRO_DEVICE_ID_JOYPAD_UP = 4;
    public const uint RETRO_DEVICE_ID_JOYPAD_DOWN = 5;
    public const uint RETRO_DEVICE_ID_JOYPAD_LEFT = 6;
    public const uint RETRO_DEVICE_ID_JOYPAD_RIGHT = 7;
    public const uint RETRO_DEVICE_ID_JOYPAD_A = 8;
    public const uint RETRO_DEVICE_ID_JOYPAD_X = 9;
    public const uint RETRO_DEVICE_ID_JOYPAD_L = 10;
    public const uint RETRO_DEVICE_ID_JOYPAD_R = 11;
    public const uint RETRO_DEVICE_ID_JOYPAD_L2 = 12;
    public const uint RETRO_DEVICE_ID_JOYPAD_R2 = 13;
    public const uint RETRO_DEVICE_ID_JOYPAD_L3 = 14;
    public const uint RETRO_DEVICE_ID_JOYPAD_R3 = 15;
    public const uint RETRO_DEVICE_ID_JOYPAD_MASK = 256;

    public const uint RETRO_DEVICE_ID_ANALOG_X = 0;
    public const uint RETRO_DEVICE_ID_ANALOG_Y = 1;

    public const uint RETRO_DEVICE_ID_MOUSE_X = 0;
    public const uint RETRO_DEVICE_ID_MOUSE_Y = 1;
    public const uint RETRO_DEVICE_ID_MOUSE_LEFT = 2;
    public const uint RETRO_DEVICE_ID_MOUSE_RIGHT = 3;
    public const uint RETRO_DEVICE_ID_MOUSE_WHEELUP = 4;
    public const uint RETRO_DEVICE_ID_MOUSE_WHEELDOWN = 5;
    public const uint RETRO_DEVICE_ID_MOUSE_MIDDLE = 6;
    public const uint RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP = 7;
    public const uint RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN = 8;
    public const uint RETRO_DEVICE_ID_MOUSE_BUTTON_4 = 9;
    public const uint RETRO_DEVICE_ID_MOUSE_BUTTON_5 = 10;

    /* Id values for LIGHTGUN. */
    public const uint RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X = 13;     /* Absolute Position */
    public const uint RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y = 14;     /* Absolute */
    public const uint RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN = 15; /* Status Check */
    public const uint RETRO_DEVICE_ID_LIGHTGUN_TRIGGER = 2;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_RELOAD = 16;       /* Forced off-screen shot */
    public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_A = 3;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_B = 4;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_START = 6;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_SELECT = 7;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_AUX_C = 8;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP = 9;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN = 10;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT = 11;
    public const uint RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT = 12;

    private static mameControls deviceIdsJoypad = null;
    private static mameControls deviceIdsMouse = null;
    private static mameControls deviceIdsAnalog = null;
    private static mameControls deviceIdsLightGun = null;
    public static List<string> deviceIdsCombined = null;

    private delegate void inputPollHander();
    private delegate Int16 inputStateHandler(uint port, uint device, uint index, uint id);
    static Waiter coinSlotWaiter = new(2);
    public static LibretroControlMap ControlMap;

    #endregion

    #region LOG
    // LibRetro log callback implementation ----------------------------------------
    public enum retro_log_level
    {
        RETRO_LOG_DEBUG = 0,
        RETRO_LOG_INFO = 1,
        RETRO_LOG_WARN = 2,
        RETRO_LOG_ERROR = 3
    }
    static retro_log_level MinLogLevel = retro_log_level.RETRO_LOG_INFO;

    // MarshalDirectiveException: Cannot marshal type 'System.Object[]'
    //public delegate void logHandler(retro_log_level level, string format, object[] args);
    public delegate void logHandler(retro_log_level level, [In, MarshalAs(UnmanagedType.LPStr)] string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    public delegate void wrapperLogHandler(retro_log_level level, string value);

    static int bufLogSize = 2 * 1024;
    static IntPtr buf = Marshal.AllocHGlobal(bufLogSize); //there is a risk here.
    // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute.callingconvention?view=net-6.0
    //https://www.codeproject.com/Articles/19274/A-printf-implementation-in-C
    // public static extern int sprintf(IntPtr buffer, string format, __arglist); __arglist fails
    // based on @asimonf implementation
    //https://github.com/asimonf/RetroLite/blob/4a8acd5a1db353bfa76e6af238523260483e0b89/LibRetro/Native/LinuxHelper.cs
    [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
    private static extern int snprintf(IntPtr buffer, int maxSize, string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6,
                                        IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

    [AOT.MonoPInvokeCallback(typeof(logHandler))]
    public static void MamePrintf(retro_log_level level, string format,
                                  IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6,
                                  IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
    {
        if (level >= MinLogLevel)
        {
            if (arg1 != IntPtr.Zero)
            {
                snprintf(buf, bufLogSize, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
                string str = Marshal.PtrToStringAnsi(buf);
                WriteConsole($"{level}: {str}");
            }
            else
            {
                WriteConsole($"{level}: {format}");
            }
            // Marshal.FreeHGlobal(buf); //the pointer dies with the program, no memleak here.
        }
    }
    [AOT.MonoPInvokeCallback(typeof(wrapperLogHandler))]
    public static void WrapperPrintf(retro_log_level level, string value)
    {
        WriteConsole($"{level}: {value}");
    }

    #endregion

    #region AUDIO

    // audio ===============
    private delegate void AudioLockHandler();
    private delegate void AudioUnlockHandler();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_audio_init(AudioLockHandler AudioLock,
                                                    AudioUnlockHandler AudioUnlock
                                                    );

    // Declare the C functions using P/Invoke
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr wrapper_audio_get_audio_buffer_pointer();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_audio_get_audio_buffer_occupancy();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_audio_consume_buffer(int consumeSize);


    static object AudioBufferLock = new();
    static int QuestAudioFrequency = 48000; //Quest 2 standar, can change at start

    #endregion

#if _serialize_
    // serialization -------------------
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint retro_serialize_size();
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool retro_serialize(IntPtr info, uint size);
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool retro_unserialize(IntPtr info, uint size);
#endif

    // user control
    //games have to initialize and then they can accept controls.
    private static Waiter WaitToFinishedGameLoad = null;

#if _serialize_
    //serialization control
    private static Waiter WaitToSerialize = null;
    public enum SerializationState
    {
      None,
      Analize,
      Load,
      Serialize,
      Done
    }
    private static SerializationState SerializationStatus = SerializationState.Analize;
    public static bool EnableSaveState = true;
    public static string StateFile = "state.nv";
#endif

    //image ===========    
    static FpsControlNoUnity FPSControlNoUnity;
    public static Texture2D GameTexture = null;
    public static uint TextureWidth = 0, TextureHeight = 0;
    static bool RecreateTexture = true;
    static object GameTextureLock = new();
    static object LightGunLock = new();

    static ManualResetEventSlim GameTextureBufferSem = new ManualResetEventSlim(false);

    //parameters ================

    //components parameters
    public static AudioSource Speaker;
    public static CoinSlotController CoinSlot;
    public static int SecondsToWaitToFinishLoad = 2;

    static Task retroRunTask;
    static CancellationTokenSource retroRunTaskCancellationToken;

    //game info and storage.
    static string GameFileName = "";
    static string ScreenName = ""; //name of the screen of the cabinet where is running the game

    //Status flags
    public static bool GameLoaded = false;
    static bool InteractionAvailable = false;

    public static LightGunTarget lightGunTarget;

#if _debug_fps_
    //Profiling
    static StopWatches Profiling;
#endif

    // C Wrappers 

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_run();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_retro_deinit();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_load_game(string path, string _gamma,
                                                        string _brightness, int xy_device);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_unload_game();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_system_info_need_full_path();

    //image
    private delegate void CreateTextureHandler(uint width, uint height);
    private delegate void TextureLockHandler();
    private delegate void TextureUnlockHandler();
    private delegate void TextureBufferSemAvailableHandler();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_image_init(CreateTextureHandler CreateTexture,
                                                    TextureLockHandler TextureLock,
                                                    TextureUnlockHandler TextureUnlock,
                                                    TextureBufferSemAvailableHandler TextureBufferSemAvailable
                                                    );

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr wrapper_image_get_buffer();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_image_get_buffer_size();

    //environment.
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_environment_open(wrapperLogHandler lg,
                                                        retro_log_level _minLogLevel,
                                                        string _save_directory,
                                                        string _system_directory,
                                                        string _sample_rate,
                                                        inputStateHandler _input_state_handler_cb);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_environment_init();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern double wrapper_environment_get_fps();

    public static string[] GammaOptionsList = new string[] { "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0" };
    public static string[] BrightnessOptionsList = new string[] { "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0" };
    public static readonly string DefaultGamma = "0.5"; //tested feb/2023
    public static readonly string DefaultBrightness = "1.0";
    public static Func<string, bool> IsBrightnessValid = (input) => BrightnessOptionsList.Any(x => x.Contains(input)); //, StringComparison.OrdinalIgnoreCase
    public static Func<string, bool> IsGammaValid = (input) => GammaOptionsList.Any(x => x.Contains(input)); //, StringComparison.OrdinalIgnoreCase
    //parameters gama and brightness
    public static string Gamma = DefaultGamma;
    public static string Brightness = DefaultBrightness;

    //one time initialization
    static LibretroMameCore()
    {
        deviceIdsMouse = new();
        deviceIdsJoypad = new();
        deviceIdsAnalog = new();
        deviceIdsLightGun = new();

        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_X, "MOUSE_X");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_Y, "MOUSE_Y");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_LEFT, "MOUSE_LEFT");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_RIGHT, "MOUSE_RIGHT");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_WHEELUP, "MOUSE_WHEELUP");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_WHEELDOWN, "MOUSE_WHEELDOWN");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_MIDDLE, "MOUSE_MIDDLE");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP, "MOUSE_HORIZ_WHEELUP");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN, "MOUSE_HORIZ_WHEELDOWN");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_BUTTON_4, "MOUSE_BUTTON_4");
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_BUTTON_5, "MOUSE_BUTTON_5");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_B, "JOYPAD_B");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_A, "JOYPAD_A");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_X, "JOYPAD_X");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_Y, "JOYPAD_Y");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_SELECT, "JOYPAD_SELECT");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_START, "JOYPAD_START");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_UP, "JOYPAD_UP");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_DOWN, "JOYPAD_DOWN");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_LEFT, "JOYPAD_LEFT");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_RIGHT, "JOYPAD_RIGHT");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L, "JOYPAD_L");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R, "JOYPAD_R");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R2, "JOYPAD_R2");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L2, "JOYPAD_L2");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R3, "JOYPAD_R3");
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L3, "JOYPAD_L3");

        deviceIdsAnalog.addMap(RETRO_DEVICE_ID_ANALOG_X, "ANALOG_X");
        deviceIdsAnalog.addMap(RETRO_DEVICE_ID_ANALOG_Y, "ANALOG_Y");

        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_A, "LIGHTGUN_AUX_A");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_B, "LIGHTGUN_AUX_B");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_C, "LIGHTGUN_AUX_C");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN, "LIGHTGUN_DPAD_DOWN");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT, "LIGHTGUN_DPAD_LEFT");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT, "LIGHTGUN_DPAD_RIGHT");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP, "LIGHTGUN_DPAD_UP");
        //deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN, "LIGHTGUN_AUX_A");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_RELOAD, "LIGHTGUN_RELOAD");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_SELECT, "LIGHTGUN_SELECT");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_START, "LIGHTGUN_START");
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_TRIGGER, "LIGHTGUN_TRIGGER");

        List<string> joy = deviceIdsJoypad.ControlsList();
        List<string> mouse = deviceIdsMouse.ControlsList();
        List<string> light = deviceIdsLightGun.ControlsList();

        //analog isn't ready
        // List<string> analog = deviceIdsAnalog.ControlsList();
        // deviceIdsCombined = mouse.Concat(joy).Concat(analog).ToList();

        deviceIdsCombined = mouse.Concat(joy).Concat(light).ToList();

        GameTexture = new Texture2D(200, 200, TextureFormat.RGB565, false);
        GameTexture.filterMode = FilterMode.Point;
    }

    static void assignControls()
    {
        if (ControlMap == null)
        {
            throw new Exception("[LibretroMameCore.initializeControls] the ControlMap should be assigned previous to the start of the game.");
        }

        ConfigManager.WriteConsole($"[initializeControls] MOUSE: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsMouse.controlMap = ControlMap;
        ConfigManager.WriteConsole($"[initializeControls] JOYPAD: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsJoypad.controlMap = ControlMap;
        ConfigManager.WriteConsole($"[initializeControls] LIGTHGUN: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsLightGun.controlMap = ControlMap;
    }

    public static bool Start(string screenName, string gameFileName)
    {
        string path = ConfigManager.RomsDir + "/" + gameFileName;
        if (GameLoaded)
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR a game was loaded previously ({GameFileName}), it's neccesary to call End() before the Start()");
            return false;
        }
        if (!String.IsNullOrEmpty(GameFileName) || !String.IsNullOrEmpty(ScreenName))
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR: MAME previously initalized with [{GameFileName} in {ScreenName}], End() is needed");
            return false;
        }

        if (!File.Exists(path))
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} not found.");
            return false;
        }

        WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");
        WriteConsole("[LibRetroMameCore.Start] ------------------- LIBRETRO INIT -----------------------");
        WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");

        //Audio configuration
        var audioConfig = AudioSettings.GetConfiguration();
        QuestAudioFrequency = audioConfig.sampleRate;
        WriteConsole($"[LibRetroMameCore.Start] AUDIO Quest Sample Rate:{QuestAudioFrequency} dspBufferSize: {audioConfig.dspBufferSize}");

        WriteConsole("[LibRetroMameCore.Start] Init environmnet and call retro_init()");
        int result = wrapper_environment_open(new wrapperLogHandler(WrapperPrintf),
                                                MinLogLevel,
                                                ConfigManager.GameSaveDir,
                                                ConfigManager.SystemDir,
                                                QuestAudioFrequency.ToString(),
                                                new inputStateHandler(inputStateCB)
                                                );
        if (result != 0)
        {
            ConfigManager.WriteConsoleError("[LibRetroMameCore.Start] wrapper_environment_init failed");
            return false;
        }

        WriteConsole("[LibRetroMameCore.Start] image callbacks");
        wrapper_image_init(new CreateTextureHandler(CreateTextureCB),
                            new TextureLockHandler(TextureLockCB),
                            new TextureUnlockHandler(TextureUnlockCB),
                            new TextureBufferSemAvailableHandler(TextureBufferSemAvailable));
        wrapper_audio_init(new AudioLockHandler(AudioLockCB),
                            new AudioUnlockHandler(AudioUnlockCB));
        wrapper_environment_init();

        if (wrapper_system_info_need_full_path() == 0)
        {
            ClearAll();
            WriteConsole("[LibRetroMameCore.Start] ERROR only implemented MAME full path");
            return false;
        }

        WriteConsole("[LibRetroMameCore.Start] Libretro initialized.");
        GameFileName = gameFileName;
        ScreenName = screenName;

        //controls
        assignControls();

        //ligthgun
        int xy_device = (lightGunTarget?.lightGunInformation != null &&
                            lightGunTarget.lightGunInformation.active) ? 1 : 0;

        WriteConsole($"[LibRetroMameCore.Start] wrapper_load_game {GameFileName} in {ScreenName}");
        GameLoaded = wrapper_load_game(path, Gamma, Brightness, xy_device) == 1;
        if (!GameLoaded)
        {
            ClearAll();
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} libretro can't start the game, please check if it is the correct version and is supported in MAME2003+ in https://buildbot.libretro.com/compatibility_lists/cores/mame2003-plus/mame2003-plus.html.");
            return false;
        }

        /* It's impossible to change the Sample Rate, fixed in 48000
        audioConfig.sampleRate = sampleRate;
        AudioSettings.Reset(audioConfig);
        audioConfig = AudioSettings.GetConfiguration();
        WriteConsole($"[LibRetroMameCore.Start] New audio Sample Rate:{audioConfig.sampleRate}");
        */
        Speaker.Play();

        WriteConsole($"[LibRetroMameCore.Start] Game Loaded: {GameLoaded} in {GameFileName} in {ScreenName} ");

        return true;
    }

    public static bool isRunning(string screenName, string gameFileName)
    {
        return GameLoaded && GameFileName == gameFileName && screenName == ScreenName;
    }


    [AOT.MonoPInvokeCallback(typeof(CreateTextureHandler))]
    static void CreateTextureCB(uint width, uint height)
    {
        WriteConsole($"[CreateTextureCB] to be in the main thread: {width}, {height}");
        TextureWidth = width;
        TextureHeight = height;
        RecreateTexture = true;
    }
    [AOT.MonoPInvokeCallback(typeof(TextureLockHandler))]
    public static void TextureLockCB()
    {
        // ConfigManager.WriteConsole($"[TextureLockCB]");
        Monitor.Enter(GameTextureLock);
    }
    [AOT.MonoPInvokeCallback(typeof(TextureUnlockHandler))]
    public static void TextureUnlockCB()
    {
        // ConfigManager.WriteConsole($"[TextureUnlockCB]");
        Monitor.Exit(GameTextureLock);
    }
    [AOT.MonoPInvokeCallback(typeof(TextureBufferSemAvailableHandler))]
    public static void TextureBufferSemAvailable()
    {
        // ConfigManager.WriteConsole($"[TextureBufferSemAvailable]");
        GameTextureBufferSem.Set();
    }


    public static bool InitializeTexture()
    {
        if (TextureWidth != 0 && RecreateTexture)
        {
            GameTexture.Reinitialize((int)TextureWidth, (int)TextureHeight);
            WriteConsole($"[InitializeTexture] {GameTexture.width}, {GameTexture.height}- {GameTexture.format}");
            RecreateTexture = false;
            return true;
        }
        return false;
    }
    public static void LoadTextureData()
    {
        lock (GameTextureLock)
        {
            if (GameTextureBufferSem.Wait(0))
            {
                IntPtr data = wrapper_image_get_buffer();
                int size = wrapper_image_get_buffer_size();
                if (data != IntPtr.Zero)
                {
                    InitializeTexture();

                    // // GameTexture.LoadRawTextureData(data, size);
                    // NativeArray<byte> textureData = GameTexture.GetRawTextureData<byte>();
                    // // Marshal.Copy(data, textureData, 0, textureData.Length);
                    // WriteConsole($"[LoadTextureData] LoadRawTextureData size: {size} textureData:{textureData.Length} pointer: {data}");
                    // if (textureData.Length < size)
                    //     WriteConsole($"[LoadTextureData] ERROR size: {size} > texture: {textureData.Length}");
                    // else

                    GameTexture.LoadRawTextureData(data, size);
                    GameTexture.Apply(false, false);
                }
                GameTextureBufferSem.Reset();
            }
        }
        // WriteConsole($"[LoadTextureData] END");
    }

    public static void StartRunThread()
    {
#if _serialize_
            if (SerializationStatus == SerializationState.Serialize) {
              if (WaitToSerialize.Finished()) {
                Serialize();
                SerializationStatus = SerializationState.Done;
              }
            }
            else if (SerializationStatus == SerializationState.Load)
            {
              UnSerialize();
              WaitToFinishedGameLoad = new Waiter(1); //for first coin check
              SerializationStatus = SerializationState.Done;
            }
#endif

        ConfigManager.WriteConsole($"[StartRunThread] -------------------------");
        FPSControlNoUnity = new((float)wrapper_environment_get_fps());
        retroRunTaskCancellationToken = new();
        retroRunTask = Task.Run(() =>
        {
            ConfigManager.WriteConsole($"[StartRunThread.retroRunTask]task start running IsCancellationRequested: {retroRunTaskCancellationToken.IsCancellationRequested} status: {retroRunTask.Status}");
            while (!retroRunTaskCancellationToken.IsCancellationRequested)
            {
                FPSControlNoUnity.CountTimeFrame();
                if (FPSControlNoUnity.isTime())
                {
                    // ConfigManager.WriteConsole($"[StartRunThread.retroRunTask] wrapper_run -------------------------");
                    wrapper_run();
                    // ConfigManager.WriteConsole($"[retroRunTask] wrapper_run end IsCancellationRequested: {retroRunTaskCancellationToken.IsCancellationRequested} status: {retroRunTask.Status} -------------------------");
                }
            }
        }
        );
    }

    public static void StartInteractions()
    {

#if _serialize_
        if (EnableSaveState)
        {
          if (AlreadySerialized())
          {
            WaitToSerialize = new Waiter(3);
            SerializationStatus = SerializationState.Load;
          }
          else {
            WaitToFinishedGameLoad = new Waiter(SecondsToWaitToFinishLoad + 3); //for first coin check
            WaitToSerialize = new Waiter(SecondsToWaitToFinishLoad);
            SerializationStatus = SerializationState.Serialize;
          }
        }
        else
        {
          WaitToFinishedGameLoad = new Waiter(SecondsToWaitToFinishLoad); //for first coin check
          SerializationStatus = SerializationState.None;
        }
#else
        WaitToFinishedGameLoad = new Waiter(SecondsToWaitToFinishLoad); //for first coin check
#endif

        InteractionAvailable = true;
    }

    public static void StopRunThread()
    {
        ConfigManager.WriteConsole($"[StopThread] stopping task - status: {retroRunTask.Status}");

        // Check the task status
        if (retroRunTask.Status == TaskStatus.Faulted)
        {
            ConfigManager.WriteConsoleException($"Task has thrown an exception", retroRunTask.Exception);
        }
        else
        {
            Waiter stopThreadWaiter = new(2);
            retroRunTaskCancellationToken.Cancel();
            while (retroRunTask.Status == TaskStatus.Running && !stopThreadWaiter.Finished())
            {
                ConfigManager.WriteConsole($"[StopThread] stopping task, status: {retroRunTask.Status}");
                Task.Delay(100).Wait(); //can't await in unsafe, this block the thread.
            }
            if (retroRunTask.Status == TaskStatus.Running)
            {
                WriteConsole("[StopRunThread] ERROR ------");
                WriteConsole("[StopRunThread] Game thread can't finish");
                WriteConsole("[StopRunThread] ERROR ------");
                ConfigManager.WriteConsoleError("[StopRunThread] Game thread continues running. can't stop it.");
            }
        }
        ConfigManager.WriteConsole($"[StopThread] stopped, status: {retroRunTask.Status}");
    }

    public static void End(string screenName, string gameFileName)
    {
        if (gameFileName != GameFileName || screenName != ScreenName)
            return;

        WriteConsole($"[LibRetroMameCore.End] Unload game: {GameFileName}");

        StopRunThread();

        //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
        if (GameLoaded)
            wrapper_unload_game();

        wrapper_retro_deinit();

        ClearAll();

        WriteConsole("[LibRetroMameCore.End] END  *************************************************");
    }

    private static void ClearAll()
    {
        WriteConsole("[LibRetroMameCore.ClearAll]");

        InteractionAvailable = false;
        FPSControlNoUnity = null;

        LightGunLock = new();


        GameTextureLock = new();
        GameTextureBufferSem = new ManualResetEventSlim(false);
        TextureWidth = 0;
        TextureHeight = 0;
        RecreateTexture = true;

        AudioBufferLock = new();

        if (Speaker != null && Speaker.isPlaying)
        {
            WriteConsole("[LibRetroMameCore.ClearAll] Pause Speaker");
            Speaker.Pause();
        }
        Speaker = null;

        GameFileName = "";
        ScreenName = "";
        GameLoaded = false;

        CoinSlot?.clean();
        CoinSlot = null;

        WriteConsole("[LibRetroMameCore.ClearAll] Unloaded and clear  *************************************************");
    }

    static public void InputControlDebug(UInt32 device)
    {
        if (device == RETRO_DEVICE_JOYPAD)
        {
            foreach (uint id in new uint[] {
                        RETRO_DEVICE_ID_JOYPAD_B,
                        RETRO_DEVICE_ID_JOYPAD_A,
                        RETRO_DEVICE_ID_JOYPAD_X,
                        RETRO_DEVICE_ID_JOYPAD_Y,
                        RETRO_DEVICE_ID_JOYPAD_SELECT,
                        RETRO_DEVICE_ID_JOYPAD_START,
                        RETRO_DEVICE_ID_JOYPAD_UP,
                        RETRO_DEVICE_ID_JOYPAD_DOWN,
                        RETRO_DEVICE_ID_JOYPAD_LEFT,
                        RETRO_DEVICE_ID_JOYPAD_RIGHT,
                        RETRO_DEVICE_ID_JOYPAD_L,
                        RETRO_DEVICE_ID_JOYPAD_R,
                        RETRO_DEVICE_ID_JOYPAD_R2,
                        RETRO_DEVICE_ID_JOYPAD_L2,
                        RETRO_DEVICE_ID_JOYPAD_R3
                        //RETRO_DEVICE_ID_JOYPAD_L3 not used in controlMap, mapped in inputcall
                      })
            {
                int ret = deviceIdsJoypad.Active(id);
                if (ret != 0)
                    ConfigManager.WriteConsole($"[InputControlDebug] id:{id} name:{deviceIdsJoypad.Id(id)} ret:{ret}");
            }
        }
        else if (device == RETRO_DEVICE_MOUSE)
        {
            foreach (uint id in new uint[] {
                          RETRO_DEVICE_ID_MOUSE_X,
                          RETRO_DEVICE_ID_MOUSE_Y,
                          RETRO_DEVICE_ID_MOUSE_LEFT,
                          RETRO_DEVICE_ID_MOUSE_RIGHT,
                          RETRO_DEVICE_ID_MOUSE_WHEELUP,
                          RETRO_DEVICE_ID_MOUSE_WHEELDOWN,
                          RETRO_DEVICE_ID_MOUSE_MIDDLE,
                          RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP,
                          RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN,
                          RETRO_DEVICE_ID_MOUSE_BUTTON_4,
                          RETRO_DEVICE_ID_MOUSE_BUTTON_5
                        })
            {
                int ret = deviceIdsMouse.Active(id);
                if (ret != 0)
                    ConfigManager.WriteConsole($"[InputControlDebug] id:{id} name:{deviceIdsJoypad.Id(id)} ret:{ret}");
            }
            //ConfigManager.WriteConsole($"[InputControlDebugJoystick] --------------------------------");
        }
    }

    [AOT.MonoPInvokeCallback(typeof(inputPollHander))]
    static void inputPollCB()
    {
        //WriteConsole("[inputPollCB] ");
        return;
    }

    static Int16 checkForCoins()
    {

        if ((CoinSlot != null && CoinSlot.takeCoin()) ||
                ControlMap.Active("INSERT") != 0)
        {
            //hack for pacman and others.
            coinSlotWaiter = new(0.1); //respond 1 during the next 0.n of second.
            WriteConsole($"[insertCoins] starting coinSlotWaiter, returns 1");
            return (Int16)1;
        }

        if (!coinSlotWaiter.Finished())
        {
            WriteConsole($"[insertCoins] coinSlotWaiter not Finished, returns 1");
            return (Int16)1;
        }

        return (Int16)0;
    }

    public static void CalculateLightGunPosition()
    {
        if (lightGunTarget?.lightGunInformation != null &&
            lightGunTarget.lightGunInformation.active)
        {
            lock (LightGunLock)
            {
                lightGunTarget.Shoot();
            }
        }
    }

    // https://github.com/RetroPie/RetroPie-Docs/blob/219c93ca6a81309eed937bb5b7a79b8c71add41b/docs/RetroArch-Configuration.md
    // https://docs.libretro.com/library/mame2003_plus/#default-retropad-layouts
    [AOT.MonoPInvokeCallback(typeof(inputStateHandler))]
    static Int16 inputStateCB(uint port, uint device, uint index, uint id)
    {
        Int16 ret = 0;

        if (!InteractionAvailable)
            return 0;

        if (WaitToFinishedGameLoad != null && !WaitToFinishedGameLoad.Finished())
            return 0;

        // WriteConsole($"[inputStateCB] dev {device} port {port} index:{index} id: {id}");

#if _debug_fps_
      Profiling.input.Start();
#endif

        if (device == RETRO_DEVICE_JOYPAD)
        {
            //InputControlDebug(RETRO_DEVICE_JOYPAD);
            switch (id)
            {
                case RETRO_DEVICE_ID_JOYPAD_SELECT:
                    if (port == 0)
                    {
                        // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_JOYPAD_SELECT: {CoinSlot.ToString()}");
                        ret = checkForCoins();
                    }
                    break;

                case RETRO_DEVICE_ID_JOYPAD_L3:
                    //mame menu: joystick right button press and right grip
                    ret = (ControlMap.Active("JOYPAD_R3") != 0 &&
                            ControlMap.Active("JOYPAD_R") != 0) ?
                            (Int16)1 : (Int16)0;
                    break;

                default:
                    ret = (Int16)deviceIdsJoypad.Active(id, (int)port);
                    // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_JOYPAD_???: id: {id} active: {ret} - port: {port}");
                    break;
            }
            // if (ret != 0)
            // ConfigManager.WriteConsole($"[inputStateCB] JOYPAD id: {id} name: {deviceIdsJoypad.Id(id)} ret: {ret}");
        }

        else if (device == RETRO_DEVICE_MOUSE)
        {
            //InputControlDebug(RETRO_DEVICE_MOUSE);
            ret = (Int16)deviceIdsMouse.Active(id, (int)port);
            // WriteConsole($"[inputStateCB] RETRO_DEVICE_MOUSE_???: id: {id} active: {ret} - port: {port}");
        }

        else if (device == RETRO_DEVICE_LIGHTGUN &&
                    lightGunTarget?.lightGunInformation != null &&
                    lightGunTarget.lightGunInformation.active)
        {
            //WriteConsole($"[inputStateCB] RETRO_DEVICE_LIGHTGUN port {port} index:{index}");

            switch (id)
            {
                case RETRO_DEVICE_ID_LIGHTGUN_SELECT:
                    if (port == 0)
                    {
                        // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_LIGHTGUN_SELECT: {CoinSlot.ToString()}");
                        ret = checkForCoins();
                    }
                    break;
                case RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN:
                    if (port != 0)
                    {
                        ret = 1;
                    }
                    else
                    {
                        lock (LightGunLock)
                        {
                            // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN: {!lightGunTarget.OnScreen()} ({lightGunTarget.HitX}, {lightGunTarget.HitY}) - port: {port}");
                            ret = lightGunTarget.OnScreen() ? (Int16)0 : (Int16)1;
                        }
                    }
                    break;
                case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X:
                    lock (LightGunLock)
                    {
                        // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X - port: {port} - HitX,Y: ({lightGunTarget.HitX}, {lightGunTarget.HitX})");
                        ret = (Int16)lightGunTarget.HitX;
                    }
                    break;
                case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y:
                    lock (LightGunLock)
                    {
                        // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y - port: {port} - HitX,Y: ({lightGunTarget.HitX}, {lightGunTarget.HitY})");
                        ret = (Int16)lightGunTarget.HitY;
                    }
                    break;
                default:
                    // WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_LIGHTGUN_???: id: {id} - port: {port}");
                    ret = (Int16)deviceIdsLightGun.Active(id, (int)port);
                    // WriteConsole($"[inputStateCB] active: {ret}");
                    break;
            }
        }
#if _debug_fps_
      Profiling.input.Stop();
#endif

        return ret;
    }

    [AOT.MonoPInvokeCallback(typeof(AudioLockHandler))]
    public static void AudioLockCB()
    {
        Monitor.Enter(AudioBufferLock);
    }
    [AOT.MonoPInvokeCallback(typeof(AudioUnlockHandler))]
    public static void AudioUnlockCB()
    {
        // ConfigManager.WriteConsole($"[AudioUnlockCB]");
        Monitor.Exit(AudioBufferLock);
    }
    public static void MoveAudioStreamTo(string gameFileName, float[] audioData)
    {
        if (!GameLoaded || GameFileName != gameFileName)
            return;

        lock (AudioBufferLock)
        {
            // Call the C functions to access the audio data
            IntPtr audioBufferPtr = wrapper_audio_get_audio_buffer_pointer();
            int audioBufferOccupancy = wrapper_audio_get_audio_buffer_occupancy();
            int toCopy = audioBufferOccupancy >= audioData.Length ? audioData.Length : audioBufferOccupancy;
            // WriteConsole($"[MoveAudioStreamTo] toCopy: {toCopy}");

            if (toCopy > 0)
            {
                // Convert the IntPtr to a float array
                Marshal.Copy(audioBufferPtr, audioData, 0, toCopy);

                // Consume the data in the C buffer
                wrapper_audio_consume_buffer(toCopy);
            }
        }
    }

#if _serialize_
    private static string SerializedFileName()
    {
      return PathBase + "/" + StateFile;
    }

    private static bool AlreadySerialized()
    {
      return File.Exists(SerializedFileName());
    }

    private static void UnSerialize()
    {
      int size = (int)retro_serialize_size();
      WriteConsole($"[LibRetroMameCore.serialize] Unserialize {size} bytes");

      byte[] buffer = new byte[size];
      using (var file = File.OpenRead(SerializedFileName()))
        file.Read(buffer, 0, size);
      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
      if (!retro_unserialize(ptr, (uint)size))
        WriteConsole($"[LibRetroMameCore.serialize] ERROR Libretro can't unserialize game memory of {size} bytes");

      return;
    }

    private static void Serialize()
    {
      int size = (int)retro_serialize_size();

      WriteConsole($"[LibRetroMameCore.serialize] serialize {size} bytes");
      byte[] buffer = new byte[size];
      var ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
      if (retro_serialize(ptr, (uint)size))
      {
        using (var file = File.OpenWrite(SerializedFileName()))
          file.Write(buffer, 0, size);
      }
      else
        WriteConsole($"[LibRetroMameCore.serialize] Libretro can't serialize game memory");

      return;
    }

#endif

    public class FpsControlNoUnity
    {
        private float timeBalance = 0;
        private float timePerFrame = 0;
        private uint frameCount = 0;
        private float acumTime = 0;

        private DateTime lastFrameTime;

        public FpsControlNoUnity(float FPSExpected)
        {
            timeBalance = 0;
            frameCount = 0;
            timePerFrame = 1f / FPSExpected;
            lastFrameTime = DateTime.Now;
        }

        public void CountTimeFrame()
        {
            DateTime currentFrameTime = DateTime.Now;
            TimeSpan deltaTime = currentFrameTime - lastFrameTime;
            lastFrameTime = currentFrameTime;

            timeBalance += (float)deltaTime.TotalSeconds;
            acumTime += (float)deltaTime.TotalSeconds;
            frameCount++;
        }

        public void Reset()
        {
            timeBalance = 0;
            frameCount = 0;
            acumTime = 0;
            lastFrameTime = DateTime.Now;
        }

        public float fps()
        {
            return frameCount / acumTime;
        }

        public bool isTime()
        {
            // deltaTime: time from the last call
            if (timeBalance >= timePerFrame)
            {
                timeBalance -= timePerFrame;
                return true;
            }
            return false;
        }

        public float DelayedFrames()
        {
            return timeBalance / timePerFrame;
        }

        public override string ToString()
        {
            return $"timePerFrame: {timePerFrame} delayed frames: {DelayedFrames()} fps: {fps()} frames total: {frameCount}";
        }
    }


    [Conditional("_debug_")]
    public static void WriteConsole(string st)
    {
        ConfigManager.WriteConsole($"({GameFileName}) - {st}");
    }

    public class Waiter
    {
        bool finished = false;
        DateTime started = DateTime.MaxValue;
        double waitSecs = 0;

        public Waiter(double _pwaitSecs)
        {
            waitSecs = _pwaitSecs;
        }

        public double WaitSecs
        {
            get
            {
                return waitSecs;
            }
        }

        public void Reset()
        {
            started = DateTime.MaxValue;
            finished = false;
        }

        public bool Finished()
        {
            if (!finished)
            {
                if (started == DateTime.MaxValue)
                    started = DateTime.Now;

                TimeSpan elapsedTime = DateTime.Now - started;
                finished = elapsedTime.TotalSeconds >= waitSecs;
            }
            return finished;
        }
    }

    private class mameControls
    {
        private Dictionary<uint, string> ids = new();
        public LibretroControlMap controlMap;

        public mameControls(LibretroControlMap ctrl = null)
        {
            controlMap = ctrl;
        }

        public void addMap(uint mameId, string gameId)
        {
            ids[mameId] = gameId;
        }

        public string Id(uint mameId)
        {
            if (ids.ContainsKey(mameId))
                return ids[mameId];
            return "";
        }

        public string InputActionMapName(string gameId, int port = 0)
        {
            return $"{gameId}_{port}";
        }

        public Int16 Active(uint mameId, int port = 0)
        {
            string gameId = Id(mameId);
            if (gameId == "")
            {
                ConfigManager.WriteConsoleError($"[mameControls] libretro is asking for a control id that is not mapped: {mameId}");
                return 0;
            }
            return (Int16)controlMap.Active(gameId, port);
        }

        public List<string> ControlsList()
        {
            return ids.Values.Distinct().ToList();
        }
    }


#if _debug_fps_
    public class StopWatches {
        public Stopwatch audio = new Stopwatch();
        public Stopwatch video = new Stopwatch();
        public Stopwatch input = new Stopwatch();
        public Stopwatch retroRun = new Stopwatch();

        public StopWatches() {
            audio = new Stopwatch();
            video = new Stopwatch();
            input = new Stopwatch();
            retroRun = new Stopwatch();
        }

        public double RetroRunReal() {
            return retroRun.Elapsed.TotalMilliseconds - audio.Elapsed.TotalMilliseconds - video.Elapsed.TotalMilliseconds - input.Elapsed.TotalMilliseconds;
        }

        public override string ToString() {
            return $"audio: {audio.Elapsed.TotalMilliseconds} video: {video.Elapsed.TotalMilliseconds} input: {input.Elapsed.TotalMilliseconds} retro_run: {RetroRunReal()}";
        }
    }
#endif
}
