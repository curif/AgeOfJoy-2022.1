/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

//#define _debug_fps_
//#define _debug_audio_
#define _debug_
//#define _serialize_

//#define INPUT_DEBUG

using Assets.curif.LibRetroWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using LC = LibretroControlMapDictionnary;

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

    /* Id values for POINTER. */
    public const uint RETRO_DEVICE_ID_POINTER_X = 0;
    public const uint RETRO_DEVICE_ID_POINTER_Y = 1;
    public const uint RETRO_DEVICE_ID_POINTER_PRESSED = 2;
    public const uint RETRO_DEVICE_ID_POINTER_COUNT = 3;

    public const uint RETRO_MEMORY_SAVE_RAM = 0;
    public const uint RETRO_MEMORY_RTC = 1;
    public const uint RETRO_MEMORY_SYSTEM_RAM = 2;
    public const uint RETRO_MEMORY_VIDEO_RAM = 3;

    private static mameControls deviceIdsJoypad = null;
    private static mameControls deviceIdsMouse = null;
    private static mameControls deviceIdsAnalog = null;
    private static mameControls deviceIdsLightGun = null;
    public static List<string> deviceIdsCombined = null;

    // 0 = Player 1, 1 = Player 2, etc.
    public static uint activePlayerSlot = 0;

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
    public static ShaderScreenBase Shader;


    static ManualResetEventSlim GameTextureBufferSem = new ManualResetEventSlim(false);

    //parameters ================

    //components parameters
    public static AudioSource Speaker;
    public static CoinSlotController CoinSlot;
    public static int SecondsToWaitToFinishLoad = 2;
    public static string Core;
    public static CoreEnvironment CabEnvironment;
    public static bool? Persistent;

    static Task retroRunTask;
    static CancellationTokenSource retroRunTaskCancellationToken;

    //game info and storage.
    static string GameFileName = "";

    // Playlist management
    static List<string> PlayList = new List<string>();
    static int PlayListIndex = 0;
    static string PreviousGameFileName = "";

    static string ScreenName = ""; //name of the screen of the cabinet where is running the game

    //Status flags
    public static bool GameLoaded = false;
    static bool InteractionAvailable = false;

    public static LightGunTarget lightGunTarget;
    public static Dictionary<uint, LibretroInputDevice> libretroInputDevices;

    // Aimed mouse state
    private static int lastCoordX;
    private static int lastCoordY;
    private static int lastMouseCoordX;
    private static int lastMouseCoordY;
    private static bool isMouseAim;

    //events
    public static UnityEvent OnPlayerStartPlaying = new();
    public static UnityEvent OnPlayerStopPlaying = new();

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
    private static extern int wrapper_load_game(string path, long size, byte[] data, string gamma, string brightness, uint xy_device);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_unload_game();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_reset();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_system_info_need_full_path();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint wrapper_get_memory_size(uint id);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern char* wrapper_get_memory_data(uint id);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_set_memory_value(uint id, uint value);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_get_memory_value(uint id);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_copy_memory_section(uint id, IntPtr src, uint value);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]

    private static extern uint wrapper_get_savestate_size();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool wrapper_set_savestate_data(void* data, uint size);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool wrapper_get_savestate_data(void* data, uint size);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_set_controller_port_device(uint port, uint device);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_input_init();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool wrapper_is_hardware_rendering();

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

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern float wrapper_image_get_light_red();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern float wrapper_image_get_light_green();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern float wrapper_image_get_light_blue();

    //environment

    private delegate string EnvironmentHandler(string key);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_environment_open(wrapperLogHandler lg,
                                                        retro_log_level _minLogLevel,
                                                        string _save_directory,
                                                        string _system_directory,
                                                        string _sample_rate,
                                                        inputStateHandler _input_state_handler_cb,
                                                        string _coreLibrary,
                                                        EnvironmentHandler _environmentHandler);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_environment_init();

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern double wrapper_environment_get_fps();

    // public static string[] GammaOptionsList = new string[] { "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0" };
    // public static string[] BrightnessOptionsList = new string[] { "0.2", "0.3", "0.4", "0.5", "0.6", "0.7", "0.8", "0.9", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7", "1.8", "1.9", "2.0" };
    public static readonly string DefaultGamma = "1.0"; //tested feb/2023
    public static readonly string DefaultBrightness = "1.0";
    // public static Func<string, bool> IsBrightnessValid = (input) => BrightnessOptionsList.Any(x => x.Contains(input)); //, StringComparison.OrdinalIgnoreCase
    // public static Func<string, bool> IsGammaValid = (input) => GammaOptionsList.Any(x => x.Contains(input)); //, StringComparison.OrdinalIgnoreCase
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

        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_X, LC.MOUSE_X);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_Y, LC.MOUSE_Y);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_LEFT, LC.MOUSE_LEFT);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_RIGHT, LC.MOUSE_RIGHT);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_WHEELUP, LC.MOUSE_WHEELUP);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_WHEELDOWN, LC.MOUSE_WHEELDOWN);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_MIDDLE, LC.MOUSE_MIDDLE);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELUP, LC.MOUSE_HORIZ_WHEELUP);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_HORIZ_WHEELDOWN, LC.MOUSE_HORIZ_WHEELDOWN);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_BUTTON_4, LC.MOUSE_BUTTON_4);
        deviceIdsMouse.addMap(RETRO_DEVICE_ID_MOUSE_BUTTON_5, LC.MOUSE_BUTTON_5);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_B, LC.JOYPAD_B);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_A, LC.JOYPAD_A);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_X, LC.JOYPAD_X);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_Y, LC.JOYPAD_Y);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_SELECT, LC.JOYPAD_SELECT);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_START, LC.JOYPAD_START);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_UP, LC.JOYPAD_UP);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_DOWN, LC.JOYPAD_DOWN);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_LEFT, LC.JOYPAD_LEFT);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_RIGHT, LC.JOYPAD_RIGHT);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L, LC.JOYPAD_L);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R, LC.JOYPAD_R);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L2, LC.JOYPAD_L2);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R2, LC.JOYPAD_R2);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_L3, LC.JOYPAD_L3);
        deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_R3, LC.JOYPAD_R3);

        deviceIdsAnalog.addMap(RETRO_DEVICE_ID_ANALOG_X, LC.MOUSE_X); // Assuming "ANALOG_X" and "ANALOG_Y" are defined similarly
        deviceIdsAnalog.addMap(RETRO_DEVICE_ID_ANALOG_Y, LC.MOUSE_Y); // Replace with appropriate constants if available

        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_A, LC.LIGHTGUN_AUX_A);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_B, LC.LIGHTGUN_AUX_B);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_AUX_C, LC.LIGHTGUN_AUX_C);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_DOWN, LC.LIGHTGUN_DPAD_DOWN);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_LEFT, LC.LIGHTGUN_DPAD_LEFT);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_RIGHT, LC.LIGHTGUN_DPAD_RIGHT);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_DPAD_UP, LC.LIGHTGUN_DPAD_UP);
        //deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN, LC.LIGHTGUN_AUX_A);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_RELOAD, LC.LIGHTGUN_RELOAD);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_SELECT, LC.LIGHTGUN_SELECT);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_START, LC.LIGHTGUN_START);
        deviceIdsLightGun.addMap(RETRO_DEVICE_ID_LIGHTGUN_TRIGGER, LC.LIGHTGUN_TRIGGER);

        List<string> joy = deviceIdsJoypad.ControlsList();
        List<string> mouse = deviceIdsMouse.ControlsList();
        List<string> light = deviceIdsLightGun.ControlsList();

        //analog isn't ready
        // List<string> analog = deviceIdsAnalog.ControlsList();
        // deviceIdsCombined = mouse.Concat(joy).Concat(analog).ToList();

        deviceIdsCombined = mouse.Concat(joy).Concat(light).ToList();

        /*GameTexture = new Texture2D(200, 200, TextureFormat.RGB565, false);
        GameTexture.filterMode = FilterMode.Bilinear;
        GameTexture.anisoLevel = 0;
        */
        GameTexture = new Texture2D(200, 200, TextureFormat.RGB565, false);
        GameTexture.filterMode = FilterMode.Bilinear;
        //GameTexture.anisoLevel = 0;
    }

    public static float getLightRed() { return wrapper_image_get_light_red(); }
    public static float getLightGreen() { return wrapper_image_get_light_green(); }
    public static float getLightBlue() { return wrapper_image_get_light_blue(); }

    public static void AssignControls()
    {
        if (ControlMap == null)
        {
            throw new Exception("[LibretroMameCore.initializeControls] the ControlMap should be assigned previous to the start of the game.");
        }

        ConfigManager.WriteConsole($"[initializeControls] MOUSE: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsMouse.controlMap = ControlMap;
        ConfigManager.WriteConsole($"[initializeControls] JOYPAD: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsJoypad.controlMap = ControlMap;
        ConfigManager.WriteConsole($"[initializeControls] LIGHTGUN: naming MAME controls (mapping libretro ids to control name)");
        deviceIdsLightGun.controlMap = ControlMap;
    }

    public static bool Start(string screenName, string gameFileName, List<string> playList)
    {
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

        if (getPath(gameFileName) == null)
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR {gameFileName} not found.");
            return false;
        }

        WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");
        WriteConsole("[LibRetroMameCore.Start] ------------------- LIBRETRO INIT -----------------------");
        WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");

        //Audio configuration
        var audioConfig = AudioSettings.GetConfiguration();
        QuestAudioFrequency = audioConfig.sampleRate;
        WriteConsole($"[LibRetroMameCore.Start] AUDIO Quest Sample Rate:{QuestAudioFrequency} dspBufferSize: {audioConfig.dspBufferSize}");
        WriteConsole("[LibRetroMameCore.Start] Init environment and call retro_init()");

        Core core = CoresController.GetCore(Core);
        ConfigManager.WriteConsoleError($"[LibRetroMameCore.Start] Using corelib {core.Library}");
        InitEnvironment(core);

        WriteConsole($"[LibRetroMameCore.Start] Using coreLib:{core.Library} for {Core}");

        string persistentSaveState = null;
        if (Persistent.HasValue && Persistent.Value)
        {
            persistentSaveState = $"{ConfigManager.GameSaveDir}/{gameFileName}.state";
        }

        WriteConsole($"[LibRetroMameCore.Start] Persistent:{Persistent}/{persistentSaveState}");

        int result = wrapper_environment_open(new wrapperLogHandler(WrapperPrintf),
                                                MinLogLevel,
                                                ConfigManager.GameSaveDir,
                                                ConfigManager.SystemDir,
                                                QuestAudioFrequency.ToString(),
                                                new inputStateHandler(inputStateCB),
                                                core.Library,
                                                new EnvironmentHandler(EnvironmentHandlerCB)
                                                );
        if (result != 0)
        {
            ConfigManager.WriteConsoleError("[LibRetroMameCore.Start] wrapper_environment_init failed");
            return false;
        }

        WriteConsole("[LibRetroMameCore.Start] image callbacks");

        // Calling wrapper_environment_init and therefore retro_init as soon as possible to benefit from logs
        wrapper_environment_init();

        // Let's do this as soon as possible, but after the environment is initialized we have logging
        LibretroVulkan.WrapperInit();

        int needFullPath = wrapper_system_info_need_full_path();
        WriteConsole("[LibRetroMameCore.Start] Libretro initialized.");
        GameFileName = gameFileName;
        PlayList = playList;
        PlayListIndex = 0;
        ScreenName = screenName;

        //controls
        AssignControls();

        bool loadSuccess = loadGame(gameFileName);
        if (!loadSuccess)
        {
            return false;
        }

        // set up input ports
        foreach (var device in libretroInputDevices)
        {
            uint port = device.Key;
            uint deviceId = device.Value.Id;
            string deviceName = device.Value.Name;
            WriteConsole($"[LibRetroMameCore.Start] Setting controller port device {port} to {deviceName}:{deviceId}");
            wrapper_set_controller_port_device(port, deviceId);
        }
        resetMouseAim();
        activePlayerSlot = 0;  // Default back to Player 1 on cab startup

        // Do all at the latest possible moment. The core may have had a change of heart and decide to change settings
        wrapper_image_init(new CreateTextureHandler(CreateTextureCB),
                            new TextureLockHandler(TextureLockCB),
                            new TextureUnlockHandler(TextureUnlockCB),
                            new TextureBufferSemAvailableHandler(TextureBufferSemAvailable));
        wrapper_audio_init(new AudioLockHandler(AudioLockCB),
                            new AudioUnlockHandler(AudioUnlockCB));
        wrapper_input_init();

        /* It's impossible to change the Sample Rate, fixed in 48000
        audioConfig.sampleRate = sampleRate;
        AudioSettings.Reset(audioConfig);
        audioConfig = AudioSettings.GetConfiguration();
        WriteConsole($"[LibRetroMameCore.Start] New audio Sample Rate:{audioConfig.sampleRate}");
        */
        Speaker.Play();

        WriteConsole($"[LibRetroMameCore.Start] Game Loaded: {GameLoaded} in {GameFileName} in {ScreenName} ");

        // Boost the resolution of the eye texture during gameplay
        DeviceController.ApplySettings(true);

        OnPlayerStartPlaying.Invoke();

        return true;
    }

    public static string getPath(string gameFileName)
    {
        string path = ConfigManager.RomsDir + "/" + Core + "/" + gameFileName;

        if (!File.Exists(path))
        {
            path = ConfigManager.RomsDir + "/" + gameFileName;
        }

        if (!File.Exists(path))
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} not found.");
            return null;
        }

        return path;
    }

    public static bool loadGame(string gameFileName)
    {
        string path = getPath(gameFileName);
        if (path == null)
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} not found.");
            return false;
        }

        int needFullPath = wrapper_system_info_need_full_path();

        //lightgun
        int xy_device = (lightGunTarget?.lightGunInformation != null && lightGunTarget.lightGunInformation.active) ? 1 : 0;

        WriteConsole($"[LibRetroMameCore.Start] wrapper_load_game {GameFileName} in {ScreenName}");

        if (GameLoaded)
        {
            // save state for previous game and unload it
            saveState(PreviousGameFileName);
            wrapper_unload_game();
        }

        byte[] data = null;
        long fileSizeInBytes = 0;
        if (needFullPath == 0)
        {
            data = File.ReadAllBytes(path);
            fileSizeInBytes = data.Length;
        }
        GameLoaded = wrapper_load_game(path, fileSizeInBytes, data, Gamma, Brightness, (uint)xy_device) == 1;
        if (!GameLoaded)
        {
            ClearAll();
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} libretro can't start the game, please check if it is the correct version and is supported by {Core}");
            return false;
        }

        // This is now the new active game. Load its state
        loadState(gameFileName);
        PreviousGameFileName = gameFileName;

        return true;
    }

    public static void loadState(string gameFileName)
    {
#if !UNITY_EDITOR
        loadSram(gameFileName);
        loadPersistentState(gameFileName);
#endif
    }

    public static void saveState(string gameFileName)
    {
#if !UNITY_EDITOR
        saveSram(gameFileName);
        savePersistentState(gameFileName);
#endif
    }

    public static void loadSram(string gameFileName)
    {
        if (!GameLoaded)
            throw new Exception($"Can't load the game state from the file if the game isn't loaded yet: {gameFileName}");

        string sramFileName = getSramFileName(gameFileName);
        if (File.Exists(sramFileName))
        {
            uint sramSize = wrapper_get_memory_size(RETRO_MEMORY_SAVE_RAM);
            if (sramSize > 0)
            {
                char* sramBuffer = wrapper_get_memory_data(RETRO_MEMORY_SAVE_RAM);
                byte[] sramData = File.ReadAllBytes(sramFileName);
                int bytesToCopy = Math.Min(sramData.Length, (int)sramSize);
                Marshal.Copy(sramData, 0, (IntPtr)sramBuffer, bytesToCopy);
                WriteConsole($"[LibRetroMameCore.loadSram] SRAM data loaded: {sramFileName}: {bytesToCopy} bytes");
            }
        }
    }

    public static void saveSram(string gameFileName)
    {
        if (!GameLoaded)
            throw new Exception($"Can't save the game state to a file if the game isn't loaded: {gameFileName}");

        string sramFileName = getSramFileName(gameFileName);
        uint sramSize = wrapper_get_memory_size(RETRO_MEMORY_SAVE_RAM);
        if (sramSize > 0)
        {
            char* sramBuffer = wrapper_get_memory_data(RETRO_MEMORY_SAVE_RAM);
            byte[] sramData = new byte[sramSize];
            Marshal.Copy((IntPtr)sramBuffer, sramData, 0, (int)sramSize);
            File.WriteAllBytes(sramFileName, sramData);
            WriteConsole($"[LibRetroMameCore.saveSram] SRAM data saved: {sramFileName}: {sramSize} bytes");
        }
    }

    public static void setSramBlock(uint dest_offset, string src)
    {
        if (!GameLoaded)
            throw new Exception($"[setSramBlock] Can't operate on memory of non loaded games.");
        int ret;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(src);
        uint length = (uint)bytes.Length;
        GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        IntPtr pointer = pinnedArray.AddrOfPinnedObject();
        try
        {
            ret = wrapper_copy_memory_section(dest_offset, pointer, length);
        }
        finally
        {
            pinnedArray.Free();
        }

        if (ret < 0)
            throw new Exception("Error copying memory block");
    }

    public static int getSram(int offset)
    {
        if (!GameLoaded)
            throw new Exception($"[getSram] Can't operate on memory of non loaded games.");
        int ret = wrapper_get_memory_value((uint)offset);
        if (ret < 0)
            throw new Exception("Error reading memory");
        return ret;
    }

    public static void setSram(uint offset, uint value)
    {
        if (!GameLoaded)
            throw new Exception($"[setSram] Can't operate on memory of non loaded games.");
        int ret = wrapper_set_memory_value(offset, value);
        if (ret < 0)
            throw new Exception("Error copying memory value");
    }

    public static string getSramFileName(string gameFileName)
    {
        return $"{ConfigManager.GameSaveDir}/{gameFileName}.srm";
    }

    public static void loadPersistentState(string gameFileName)
    {
        if (isPersistentEnabled())
        {
            loadGameState(getPersistentFileName(gameFileName));
        }
    }

    public static void savePersistentState(string gameFileName)
    {
        if (isPersistentEnabled())
        {
            saveGameState(getPersistentFileName(gameFileName));
        }
    }

    public static void loadGameState(string statefilename)
    {
        if (File.Exists(statefilename))
        {
            uint persistentSize = wrapper_get_savestate_size();
            if (persistentSize > 0)
            {
                byte[] persistentData = File.ReadAllBytes(statefilename);
                if (persistentData.Length == persistentSize)
                {
                    fixed (byte* persistentBuffer = persistentData)
                    {
                        wrapper_set_savestate_data(persistentBuffer, persistentSize);
                        WriteConsole($"[LibRetroMameCore.loadPersistentState] Persistent data loaded: {statefilename}: {persistentSize} bytes");
                    }
                }
                else
                {
                    WriteConsole($"[LibRetroMameCore.loadPersistentState] ERROR Persistent data size mismatch: {statefilename}: {persistentData.Length} != {persistentSize}");
                }
            }
        }
    }

    public static void saveGameState(string statefilename)
    {
        uint persistentSize = wrapper_get_savestate_size();
        if (persistentSize > 0)
        {
            byte[] persistentData = new byte[persistentSize];
            fixed (byte* persistentBuffer = persistentData)
            {
                wrapper_get_savestate_data(persistentBuffer, persistentSize);
                File.WriteAllBytes(statefilename, persistentData);
                WriteConsole($"[LibRetroMameCore.savePersistentState] Persistent data saved: {statefilename}: {persistentSize} bytes");
            }
        }
    }

    public static bool isPersistentEnabled()
    {
        return Persistent.HasValue && Persistent.Value;
    }

    public static string getPersistentFileName(string gameFileName)
    {
        return $"{ConfigManager.GameSaveDir}/{gameFileName}.state";
    }

#if UNITY_EDITOR

    public static void simulateInEditor(string screenName, string gameFileName)
    {
        WriteConsole("[LibRetroMameCore.simulateInEditor] Libretro simulated.");
        GameFileName = gameFileName;
        ScreenName = screenName;
        GameLoaded = true;
        /*var sourceTexture = Resources.Load<Texture2D>("Decoration/MoviePoster/Pictures/18/kingsOfDragons.png");
        TextureWidth = (uint)sourceTexture.width;
        TextureHeight = (uint)sourceTexture.height;
        GameTexture.Reinitialize((int)TextureWidth, (int)TextureHeight);
        GameTexture.SetPixels(sourceTexture.GetPixels());
        GameTexture.Apply();
        WriteConsole($"[simulateInEditor] {GameTexture.width}, {GameTexture.height}- {GameTexture.format}");
        */
    }
#endif


    public static bool isRunning(string screenName, string gameFileName)
    {
        return GameLoaded && GameFileName == gameFileName && screenName == ScreenName;
    }


    static Dictionary<string, string> currentEnvironment = new Dictionary<string, string>();
    static void InitEnvironment(Core core)
    {
        currentEnvironment.Clear();
        AddEnvironment(core.GlobalEnvironment);
        AddEnvironment(core.ReadCoreEnvironment());
        AddEnvironment(CabEnvironment);
    }

    static void AddEnvironment(CoreEnvironment environment)
    {
        if (environment?.properties != null)
        {
            foreach (KeyValuePair<string, string> setting in environment.properties)
            {
                string key = setting.Key;
                string value = setting.Value;
                if (environment.prefix != null)
                {
                    AddEnvironmentKey(environment.prefix + "_" + key, value);
                    AddEnvironmentKey(environment.prefix + "-" + key, value);
                }
                else
                {
                    AddEnvironmentKey(key, value);
                }
            }
        }
    }

    static void AddEnvironmentKey(string key, string value)
    {
        ConfigManager.WriteConsole($"[LibRetroMameCore.Start] Using configuration data: {key} = {value}");
        if (currentEnvironment.ContainsKey(key))
            currentEnvironment[key] = value;
        else
            currentEnvironment.Add(key, value);
    }

    [AOT.MonoPInvokeCallback(typeof(EnvironmentHandler))]
    static string EnvironmentHandlerCB(string key)
    {
        return currentEnvironment.ContainsKey(key) ? currentEnvironment[key] : null;
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
            ResetTextureData();
            //GameTexture.wrapMode = TextureWrapMode.Clamp;
            WriteConsole($"[InitializeTexture] {GameTexture.width}, {GameTexture.height}- {GameTexture.format}");
            RecreateTexture = false;
            //some shaders needs to refresh the texture once recreated.
            Shader.Refresh(GameTexture);
            Shader.ApplyConfiguration();
            return true;
        }
        return false;
    }

    public static void UpdateTexture()
    {
#if !UNITY_EDITOR
        if (wrapper_is_hardware_rendering())
        {
            LoadVulkanTextureData();
        }
        else
        {
            LoadTextureData();
        }
#endif

    }

    //static int frame = 0;
    public static void LoadVulkanTextureData()
    {
        WriteConsole($"[LoadVulkanTextureData]");
        if (LibretroVulkan.isVkImageReady())
        {
            IntPtr vkImage = LibretroVulkan.GetVkImage();
            WriteConsole($"[LoadVulkanTextureData] vulkan frame available {vkImage.ToString("x16")}");
            //Texture2D tex = Texture2D.CreateExternalTexture(640, 480, TextureFormat.RGBA32, false, false, vkImage);
            //WriteConsole($"[LoadVulkanTextureData] texture created");
            //byte[] png = tex.EncodeToPNG();
            //WriteConsole($"[LoadVulkanTextureData] png created");
            //string pngName = Path.Combine(ConfigManager.DebugDir, "test" + (frame++) + ".png");
            //File.WriteAllBytes(pngName, png);
            //WriteConsole($"[LoadVulkanTextureData] png saved {pngName}");
        }
        else
        {
            WriteConsole($"[LoadVulkanTextureData] no vulkan frame available");
        }
        WriteConsole($"[LoadVulkanTextureData] END");
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
        //WriteConsole($"[LoadTextureData] END");
    }

    public static void ResetTextureData()
    {
        WriteConsole($"[ResetTextureData]");
        if (GameTexture != null)
        {
            lock (GameTextureLock)
            {
                Color32 blackPixel = new Color32(0, 0, 0, 255);
                Color32[] pixels = GameTexture.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = blackPixel;
                }
                GameTexture.SetPixels32(pixels);
                GameTexture.Apply(false, false);
            }
        }
    }

    public static double GetFps()
    {
        /*
         * DirkSimple is claiming to be running at 23.976024 fps, but it's actually running at 30fps.
         * It's actually informing us of the 30 fps framerate on RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK but we don't handle this yet.
         * So we add this dirty hack for now
         * 
         * BE SURE TO STILL CALL wrapper_environment_get_fps() OR BAD SIDE EFFECTS HAPPEN !
         */

        double fps = wrapper_environment_get_fps();
        if (Core.Equals("dirksimple"))
        {
            WriteConsole($"[GetFps] Ajusting framerate for lying DirkSimple core -> 30");
            fps = 30;
        }
        return fps;
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

        FPSControlNoUnity = new((float)GetFps());

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
                    handleSpecialInputs();
                }
            }
        }
        );
    }

    static bool changeGameAllowed = true;
    // Handle inputs for UI actions while game is running (e.g. non-libretro)
    public static void handleSpecialInputs()
    {
        if (!ControlMap.isActive(LC.JOYPAD_L3))
        {
            // L3 is released, we can accept a new game change next time
            changeGameAllowed = true;
        }

        if (ControlMap.isActive(LC.MODIFIER))
        {
            if (changeGameAllowed && ControlMap.isActive(LC.JOYPAD_L3) && PlayList.Count > 1)
            {
                PlayListIndex++;
                if (PlayListIndex >= PlayList.Count)
                {
                    PlayListIndex = 0;
                }
                loadGame(PlayList[PlayListIndex]);
                changeGameAllowed = false;
            }

            // Reset game
            if (ControlMap.isActive(LC.JOYPAD_R3))
            {
                wrapper_reset();
            }

            // Change active player slot
            if (ControlMap.isActive(LC.JOYPAD_A))
            {
                activePlayerSlot = 0;
            }
            if (ControlMap.isActive(LC.JOYPAD_B))
            {
                activePlayerSlot = 1;
            }
            if (ControlMap.isActive(LC.JOYPAD_X))
            {
                activePlayerSlot = 2;
            }
            if (ControlMap.isActive(LC.JOYPAD_Y))
            {
                activePlayerSlot = 3;
            }
        }
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

        OnPlayerStopPlaying.Invoke();

        // Restore eye resolution upon exiting game
        DeviceController.ApplySettings(false);

#if !UNITY_EDITOR
        StopRunThread();
        //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
        if (GameLoaded) {
            saveState(gameFileName);
            wrapper_unload_game();
        }

        wrapper_retro_deinit();
#endif

        ClearAll();

        WriteConsole("[LibRetroMameCore.End] END  *************************************************");
    }

    private static void ClearAll()
    {
        WriteConsole("[LibRetroMameCore.ClearAll]");

        InteractionAvailable = false;
        FPSControlNoUnity = null;

        ResetTextureData();

        GameTextureLock = new();
        GameTextureBufferSem = new ManualResetEventSlim(false);
        TextureWidth = 0;
        TextureHeight = 0;
        RecreateTexture = true;
        Shader = null;

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
        if (device == LibretroInputDevice.Gamepad.Id)
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
                        RETRO_DEVICE_ID_JOYPAD_R3,
                        RETRO_DEVICE_ID_JOYPAD_L3
                      })
            {
                int ret = deviceIdsJoypad.Active(id);
                if (ret != 0)
                    ConfigManager.WriteConsole($"[InputControlDebug] id:{id} name:{deviceIdsJoypad.Id(id)} ret:{ret}");
            }
        }
        else if (device == LibretroInputDevice.Mouse.Id)
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

        if ((CoinSlot != null && CoinSlot.takeCoin()) || ControlMap.isActive(LC.INSERT))
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


    // Normalize lightgun coords (-0x7fff to 0x7fff) to screen coords (0 to res)
    public static Int16 toScreenCoord(int coord, uint res)
    {
        return (Int16)(((coord + 0x8000) * res) / 0x10000);
    }

    public static void resetMouseAim()
    {
        isMouseAim = libretroInputDevices.Values.ToList().Exists(device => device.Name.Equals(LibretroInputDevice.MousePointer.Name));
        lastCoordX = -0x7fff;   // Assume lightgun starting at top left
        lastCoordY = -0x7fff;
        lastMouseCoordX = 0;    // Assume mouse starting at top left
        lastMouseCoordY = 0;
    }

    // https://github.com/RetroPie/RetroPie-Docs/blob/219c93ca6a81309eed937bb5b7a79b8c71add41b/docs/RetroArch-Configuration.md
    // https://docs.libretro.com/library/mame2003_plus/#default-retropad-layouts
    [AOT.MonoPInvokeCallback(typeof(inputStateHandler))]
    static Int16 inputStateCB(uint port, uint device, uint index, uint id)
    {
        // We are using the modifier key to allow for special actions, ignore all other inputs
        if (ControlMap.isActive(LC.MODIFIER))
        {
            return 0;
        }

#if INPUT_DEBUG
        WriteConsole($"[inputStateCB] dev {device} port {port} index:{index} id: {id}");
#endif

        if (!InteractionAvailable)
        {
#if INPUT_DEBUG
            WriteConsole($"[inputStateCB] !InteractionAvailable");
#endif
            return 0;
        }

        if (WaitToFinishedGameLoad != null && !WaitToFinishedGameLoad.Finished())
        {
#if INPUT_DEBUG
            WriteConsole($"[inputStateCB] WaitToFinishedGameLoad != null && !WaitToFinishedGameLoad.Finished()");
#endif
            return 0;
        }

#if _debug_fps_
      Profiling.input.Start();
#endif

        if (id == RETRO_DEVICE_ID_JOYPAD_MASK && device == LibretroInputDevice.Gamepad.Id)
        {
            int bitmask =
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_B) << 0) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_Y) << 1) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_SELECT) << 2) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_START) << 3) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_UP) << 4) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_DOWN) << 5) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_LEFT) << 6) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_RIGHT) << 7) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_A) << 8) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_X) << 9) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_L) << 10) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_R) << 11) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_L2) << 12) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_R2) << 13) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_L3) << 14) |
                (inputStateCB(port, device, index, RETRO_DEVICE_ID_JOYPAD_R3) << 15);
            return (Int16)bitmask;
        }

        Int16 ret = 0;
        if (device == LibretroInputDevice.Gamepad.Id)
        {
            ret = inputStateCB_GamePad(port, device, index, id);
        }
        else if (device == LibretroInputDevice.Mouse.Id)
        {
            ret = inputStateCB_Mouse(port, device, index, id);
        }
        else if (device == LibretroInputDevice.Lightgun.Id)
        {
            ret = inputStateCB_LightGun(port, device, index, id);
        }
        else if (device == LibretroInputDevice.Pointer.Id)
        {
            ret = inputStateCB_Pointer(port, device, index, id);
        }

#if _debug_fps_
      Profiling.input.Stop();
#endif

#if INPUT_DEBUG
        WriteConsole($"[inputStateCB] RESULT: {ret}");
#endif

        return ret;
    }

    private static Int16 inputStateCB_GamePad(uint port, uint device, uint index, uint id)
    {
        //InputControlDebug(RETRO_DEVICE_JOYPAD);
        if (id == RETRO_DEVICE_ID_JOYPAD_SELECT)
        {
            if (port == activePlayerSlot)
            {
                // WriteConsole($"[inputStateCB_GamePad] RETRO_DEVICE_ID_JOYPAD_SELECT: {CoinSlot.ToString()}");
                return checkForCoins();
            }
        }
        else if (Core.StartsWith("mame") && id == RETRO_DEVICE_ID_JOYPAD_L3)
        {
            //mame menu: joystick right button press and right grip
            return (ControlMap.isActive(LC.JOYPAD_R3) && ControlMap.isActive(LC.JOYPAD_R)) ?
                    (Int16)1 : (Int16)0;
        }
        else
        {
            if (port == activePlayerSlot)
            {
                return (Int16)deviceIdsJoypad.Active(id, 0);
            }
            else
            {
                return (Int16)deviceIdsJoypad.Active(id, (int)port);
            }
            // WriteConsole($"[inputStateCB_GamePad] RETRO_DEVICE_ID_JOYPAD_???: id: {id} active: {ret} - port: {port}");
        }
        return 0;
    }

    private static Int16 updateMouseX(Int16 coordX)
    {
        Int16 resultX = (Int16)(coordX - lastMouseCoordX);
        lastMouseCoordX = coordX;
        return resultX;
    }

    private static Int16 updateMouseY(Int16 coordY)
    {
        Int16 resultY = (Int16)(coordY - lastMouseCoordY);
        lastMouseCoordY = coordY;
        return resultY;
    }

    private static Int16 inputStateCB_Mouse(uint port, uint device, uint index, uint id)
    {
        //InputControlDebug(RETRO_DEVICE_MOUSE);
        if (!isMouseAim)
        {
            // Regular analog stick driven mouse emulation
            return (Int16)deviceIdsMouse.Active(id, (int)port);
        }
        else
        {
            int AbsoluteHitX, AbsoluteHitY;
            lightGunTarget.GetLastHit(out AbsoluteHitX, out AbsoluteHitY);

            // Aim driven mouse emulation (uses lightgun data)
            WriteConsole($"[inputStateCB_Mouse] Lightgun at {AbsoluteHitX}x{AbsoluteHitY}");
            switch (id)
            {
                case RETRO_DEVICE_ID_MOUSE_X:
                    return updateMouseX(toScreenCoord(AbsoluteHitX, TextureWidth));
                case RETRO_DEVICE_ID_MOUSE_Y:
                    return updateMouseY(toScreenCoord(AbsoluteHitY, TextureHeight));
                default:
                    return (Int16)deviceIdsMouse.Active(id, (int)port);
            }
        }
    }

    private static Int16 inputStateCB_LightGun(uint port, uint device, uint index, uint id)
    {

        //WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_LIGHTGUN port {port} index:{index} id: {id}");

        if (lightGunTarget?.lightGunInformation == null || !lightGunTarget.lightGunInformation.active)
        {
            return 0;
        }

        int lastHitX, lastHitY;
        lightGunTarget.GetLastHit(out lastHitX, out lastHitY);
        switch (id)
        {
            case RETRO_DEVICE_ID_LIGHTGUN_SELECT:
                if (port == 0)
                {
                    WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_ID_LIGHTGUN_SELECT: {CoinSlot.ToString()}");
                    return checkForCoins();
                }
                break;
            case RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN:
                if (port != 0)      // Lightgun only works on port 0 ???
                {
                    return 1;
                }
                else
                {
                    WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_ID_LIGHTGUN_IS_OFFSCREEN: {!lightGunTarget.PointingToTheScreen()} ({lastHitX}, {lastHitY}) - port: {port}");
                    return lightGunTarget.PointingToTheScreen() ? (Int16)0 : (Int16)1;
                }
            case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X:
                WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_ID_LIGHTGUN_SCREEN_X - port: {port} - HitX,Y: ({lastHitX}, {lastHitY})");
                return (Int16)lastHitX;
            case RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y:
                WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_ID_LIGHTGUN_SCREEN_Y - port: {port} - HitX,Y: ({lastHitX}, {lastHitY})");
                return (Int16)lastHitY;
            default:
                WriteConsole($"[inputStateCB_LightGun] RETRO_DEVICE_ID_LIGHTGUN_???: id: {id} - port: {port}");
                Int16 ret = (Int16)deviceIdsLightGun.Active(id, (int)port);
                WriteConsole($"[inputStateCB_LightGun] active: {ret}");
                return ret;
        }

        return 0;
    }

    private static Int16 inputStateCB_Pointer(uint port, uint device, uint index, uint id)
    {
        int HitX, HitY;
        lightGunTarget.GetLastHit(out HitX, out HitY);
        switch (id)
        {
            case RETRO_DEVICE_ID_POINTER_X:
                return (Int16)HitX;
            case RETRO_DEVICE_ID_POINTER_Y:
                return (Int16)HitY;
            case RETRO_DEVICE_ID_POINTER_PRESSED:
            case RETRO_DEVICE_ID_POINTER_COUNT:
                if (index > 0)
                {
                    return 0;   // No multitouch support
                }
                return inputStateCB_LightGun(0, LibretroInputDevice.Lightgun.Id, index, RETRO_DEVICE_ID_LIGHTGUN_TRIGGER);
            default:
                return 0;
        }
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
    public static void MoveAudioStreamTo(float[] audioData)
    {
#if !UNITY_EDITOR
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
#endif
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

        public bool isActive(uint mameId, int port = 0)
        {
            return Active(mameId, port) != 0;
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
