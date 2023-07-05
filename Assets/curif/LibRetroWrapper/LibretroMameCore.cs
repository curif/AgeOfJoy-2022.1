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


/*
this class have a lot of static properties, and because of that we only have one game runing at a time.
you can manage callbacks from the libretro api in methods not static.
there are ways to do it, but there are obscure.
*/
public static unsafe class LibretroMameCore
{
    //IL2CPP does not support marshaling delegates that point to instance methods to native code
    //NotSupportedException: To marshal a managed method, please add an attribute named 'MonoPInvokeCallback' to the method definition. 


    [StructLayout(LayoutKind.Sequential)]
    public class retro_system_info
    {
        public string library_name;
        public string library_version;
        public string valid_extensions;
        public bool need_fullpath;
        public bool block_extract;

        public override string ToString()
        {
            return String.Format(
                "------------------- LIBRETRO SYSTEM INFO -----------------------\n" +
                "Library Name: {0} \n" +
                "Library Version: {1} \n" +
                "valid_extensions: {2} \n" +
                "need_fullpath: {3} \n" +
                "block_extract: {4} \n", library_name, library_version, valid_extensions, need_fullpath, block_extract);
        }
    }

    [DllImport("mame2003_plus_libretro_android")]
    private static extern void retro_get_system_info(IntPtr info);

    [DllImport("mame2003_plus_libretro_android")]
    private static extern int retro_api_version();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint APIVersionSignature();


    #region INPUT
    public const int RETRO_DEVICE_TYPE_SHIFT = 8;
    public const int RETRO_DEVICE_MASK = (1 << RETRO_DEVICE_TYPE_SHIFT) - 1;
    public const uint RETRO_DEVICE_NONE = 0;
    public const uint RETRO_DEVICE_JOYPAD = 1;
    public const uint RETRO_DEVICE_MOUSE = 2;
    public const uint RETRO_DEVICE_ANALOG = 5;
    // public const uint RETRO_DEVICE_KEYBOARD   = 3;
    // public const uint RETROK_5              = 53;
    // public const uint RETRO_DEVICE_ANALOG   = 5;
    // // public const uint RETRO_DEVICE_MOUSE    = 2;
    // public const uint RETRO_DEVICE_KEYBOARD = 3;
    // public const uint RETRO_DEVICE_LIGHTGUN = 4;
    // public const uint RETRO_DEVICE_ANALOG   = 5;
    // public const uint RETRO_DEVICE_POINTER  = 6;

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

    private static mameControls deviceIdsJoypad = null;
    private static mameControls deviceIdsMouse = null;
    private static mameControls deviceIdsAnalog = null;
    public static List<string> deviceIdsCombined = null;


    [DllImport("mame2003_plus_libretro_android")]
    private static extern void retro_set_controller_port_device(uint port, uint device);
    // retro_set_input_poll -------------------------------
    private delegate void inputPollHander();
    [DllImport("mame2003_plus_libretro_android")]
    private static extern void retro_set_input_poll(inputPollHander iph);
    static int HotDelaySelectCycles = 0;

    // retro_set_input_state -------------------------------
    private delegate Int16 inputStateHandler(uint port, uint device, uint index, uint id);
    [DllImport("mame2003_plus_libretro_android")]
    private static extern void retro_set_input_state(inputStateHandler ish);

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


    [StructLayout(LayoutKind.Sequential)]
    public struct retro_log_callback
    {
        public IntPtr log; // retro_log_printf_t
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_message
    {
        public string msg;        /* Message to be displayed. */
        public uint frames;     /* Duration in frames of message. */
    }

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
                WriteConsole($"[{level}] {str}");
            }
            else
            {
                WriteConsole($"[{level}] {format}");
            }
            // Marshal.FreeHGlobal(buf); //the pointer dies with the program, no memleak here.
        }
    }

    #endregion
    #region AUDIO

    // retro_set_audio_sample -------------------------------
    private delegate void audioSampleHandler(Int16 left, Int16 right);
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_set_audio_sample(audioSampleHandler sah);

    // retro_set_audio_sample_batch -------------------------------
    private delegate ulong audioSampleBatchHandler(short* data, ulong frames);
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_set_audio_sample_batch(audioSampleBatchHandler sah);

    // retro_set_audio_sample_batch -------------------------------
    public delegate void AudiobufferStatustHandler(bool active, uint occupancy, bool underrun_likely);
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_audio_buffer_status_callback
    {
        public IntPtr callback;
    }

    public static AudiobufferStatustHandler AudioBufferStatusInfo;

    // audio buffer ===============
    public static List<float> AudioBatch = new List<float>();
    static uint AudioBufferMaxOccupancy = 1024 * 8;
    static int QuestAudioFrequency = 48000; //Quest 2 standar, can change at start

    #endregion

    // retro_init -------------------------------
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_init();
    // deinit do nothing
    // https://github.com/libretro/mame2003-plus-libretro/blob/f34453af7f71c31a48d26db9d78aa04a5575ef9a/src/mame2003/mame2003.c#L401
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_deinit();
    // retro_run -------------------------------
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_run();
    // retro_load_game -------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct retro_game_info
    {
        public string path;
        public string data;
        public uint size;
        public string meta;
    }
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool retro_load_game(ref retro_game_info game);
    [DllImport("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern void retro_unload_game();

#if _serialize_
    // serialization -------------------
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern uint retro_serialize_size();
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool retro_serialize(IntPtr info, uint size);
    [DllImport ("mame2003_plus_libretro_android", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool retro_unserialize(IntPtr info, uint size);
#endif

    [StructLayout(LayoutKind.Sequential)]
    public class retro_system_timing
    {
        public double fps;             /* FPS of video content. */
        public double sample_rate;     /* Sampling rate of audio. */
    };

    // user control
    //games have to initialize and then they can accept controls.
    private static Waiter WaitToFinishedGameLoad = null;
    private static FpsControl controlForAskIfTimeToExitGame;

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
    static FpsControl FPSControl;
    public static Texture2D GameTexture;
    public static ShaderScreenBase shader;
    // public static GameObject Camera;

    //parameters ================

    public static int SecondsToWaitToFinishLoad = 2;

    //components parameters
    public static Renderer Display;
    public static AudioSource Speaker;
    public static CoinSlotController CoinSlot;

    //game info and storage.
    public static retro_system_info SystemInfo = new retro_system_info();
    static string GameFileName = "";
    static string ScreenName = ""; //name of the screen of the cabinet where is running the game
    public static string PathBase = "";

    //Status flags
    public static bool GameLoaded = false;
    static bool Initialized = false;

#if _debug_fps_
    //Profiling
    static StopWatches Profiling;
#endif

    // C Wrappers 
    //
    //image
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_image_prev_load_game();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_image_init();
    private delegate void CreateTextureHandler(uint width, uint height);
    private delegate void LoadTextureDataHandler(IntPtr data, uint size);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_image_set_texture_cb(CreateTextureHandler CreateTexture, LoadTextureDataHandler LoadTextureData);

    //environment.
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_environment_init(logHandler lg, byte[] _save_directory, byte[] _system_directory, byte[] _sample_rate);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_environment_set_game_parameters(byte[] _gamma, byte[] _brightness);
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_environment_get_av_info();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern double wrapper_environment_get_fps();
    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern double wrapper_environment_get_sample_rate();


    // pointer vault
    private static MarshalHelpPtrVault PtrVault = new MarshalHelpPtrVault();
    private static MarshalHelpPtrVault PtrVaultNoFreed = new MarshalHelpPtrVault();

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

        //deviceIdsJoypad.addMap(RETRO_DEVICE_ID_JOYPAD_MASK, "JOYPAD_MASK");

        List<string> joy = deviceIdsJoypad.ControlsList();
        List<string> mouse = deviceIdsMouse.ControlsList();

        //analog isn't ready
        // List<string> analog = deviceIdsAnalog.ControlsList();
        // deviceIdsCombined = mouse.Concat(joy).Concat(analog).ToList();

        deviceIdsCombined = mouse.Concat(joy).ToList();
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

    public static bool Start(string screenName, string gameFileName)
    {

        string path = ConfigManager.RomsDir + "/" + gameFileName;

        if (!String.IsNullOrEmpty(GameFileName) || !String.IsNullOrEmpty(ScreenName))
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR: MAME previously initalized with [{GameFileName} in {ScreenName}], End() is needed");
            return false;
        }
        if (GameLoaded)
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR a game was loaded previously ({GameFileName}), it's neccesary to call End() before the Start()");
            return false;
        }
        if (!File.Exists(path))
        {
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} don't exists or inaccesible.");
            return false;
        }

        if (!Initialized)
        {
            WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");
            WriteConsole("[LibRetroMameCore.Start] ------------------- LIBRETRO INIT -----------------------");
            WriteConsole("[LibRetroMameCore.Start] ---------------------------------------------------------");

            //Audio configuration
            var audioConfig = AudioSettings.GetConfiguration();
            QuestAudioFrequency = audioConfig.sampleRate;
            WriteConsole($"[LibRetroMameCore.Start] AUDIO Quest Sample Rate:{QuestAudioFrequency} dspBufferSize: {audioConfig.dspBufferSize}");

            WriteConsole("[LibRetroMameCore.Start] wrapper_environment_init/retro_set_environment");
            // should run first.
            //wrapper_environment_init(Marshal.GetFunctionPointerForDelegate(new logHandler(MamePrintf)));
            int result = wrapper_environment_init(new logHandler(MamePrintf), Encoding.ASCII.GetBytes(ConfigManager.GameSaveDir),
                                                 Encoding.ASCII.GetBytes(ConfigManager.SystemDir),
                                                 Encoding.ASCII.GetBytes(QuestAudioFrequency.ToString())
                                                );
            if (result != 0)
            {
                WriteConsole("[LibRetroMameCore.Start] wrapper_environment_init failed");
                return false;
            }

            WriteConsole("[LibRetroMameCore.Start] wrapper_image_init");
            wrapper_image_init();
            wrapper_image_set_texture_cb(new CreateTextureHandler(CreateTexture), new LoadTextureDataHandler(LoadTextureData));

            WriteConsole("[LibRetroMameCore.Start] retro_set_audio_sample");
            retro_set_audio_sample(new audioSampleHandler(audioSampleCB));
            WriteConsole("[LibRetroMameCore.Start] retro_set_audio_sample_batch");
            retro_set_audio_sample_batch(new audioSampleBatchHandler(audioSampleBatchCB));
            WriteConsole("[LibRetroMameCore.Start] retro_set_input_poll");
            retro_set_input_poll(new inputPollHander(inputPollCB));
            WriteConsole("[LibRetroMameCore.Start] retro_set_input_state");
            retro_set_input_state(new inputStateHandler(inputStateCB));

            WriteConsole("[LibRetroMameCore.Start] call retro_init");
            retro_init(); //do almost nothing https://github.com/libretro/mame2003-plus-libretro/blob/f34453af7f71c31a48d26db9d78aa04a5575ef9a/src/mame2003/mame2003.c#L182

            WriteConsole("[LibRetroMameCore.Start] retro_set_controller_port_device");
            retro_set_controller_port_device(port: 0, device: RETRO_DEVICE_JOYPAD);

            getSystemInfo();
            if (!SystemInfo.need_fullpath)
            {
                ClearAll();
                WriteConsole("[LibRetroMameCore.Start] ERROR only implemented MAME full path");
                return false;
            }

            Initialized = true;
        }

        GameFileName = gameFileName;
        ScreenName = screenName;

        WriteConsole($"------------------- retro_load_game {GameFileName} in {ScreenName}");

        //controls
        assignControls();

        retro_game_info game = new retro_game_info();
        game.path = path;
        game.size = 0;

        WriteConsole($"[LibRetroMameCore.Start] wrapper_image_prev_load_game/retro_load_game - loading:{path}");
        wrapper_environment_set_game_parameters(
                                      Encoding.ASCII.GetBytes(Gamma),
                                      Encoding.ASCII.GetBytes(Brightness));
        wrapper_image_prev_load_game(); //in order...
        GameLoaded = retro_load_game(ref game);

        if (!GameLoaded)
        {
            ClearAll();
            WriteConsole($"[LibRetroMameCore.Start] ERROR {path} MAME can't start the game, please check if it is the correct version and is supported in MAME2003+ in https://buildbot.libretro.com/compatibility_lists/cores/mame2003-plus/mame2003-plus.html.");
            return false;
        }
        WriteConsole($"[LibRetroMameCore.Start] Game Loaded:{path}");

        wrapper_environment_get_av_info();
        FPSControl = new FpsControl((float)wrapper_environment_get_fps());

        /* It's impossible to change the Sample Rate, fixed in 48000
        audioConfig.sampleRate = sampleRate;
        AudioSettings.Reset(audioConfig);
        audioConfig = AudioSettings.GetConfiguration();
        WriteConsole($"[LibRetroMameCore.Start] New audio Sample Rate:{audioConfig.sampleRate}");
        */

        WriteConsole($"[LibRetroMameCore.Start] AUDIO Mame2003+ frequency {wrapper_environment_get_sample_rate()} | Quest: {QuestAudioFrequency}");
        Speaker.Play();

        WriteConsole($"[LibRetroMameCore.Start] Game Loaded: {GameLoaded} in {GameFileName} in {ScreenName} ");

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
        return true;
    }

    public static bool isRunning(string screenName, string gameFileName)
    {
        return GameLoaded && GameFileName == gameFileName && screenName == ScreenName;
    }

    public static void Run(string screenName, string gameFileName)
    {
        if (!isRunning(screenName, gameFileName))
        {
            return;
        }
        //WriteConsole($"[LibRetroMameCore.Run] running screen: {screenName} game: {gameFileName}");

        // https://docs.unity3d.com/ScriptReference/Time-deltaTime.html
        FPSControl.CountTimeFrame();
        while (FPSControl.isTime())
        {

#if _debug_fps_
            // https://github.com/libretro/mame2003-plus-libretro/blob/6de44ee0a37b32a85e0aec013924bef34996ef35/src/mame2003/video.c#L400
            // https://github.com/libretro/mame2003-plus-libretro/issues/1323
            uint AudioPercentOccupancy = (uint)AudioBatch.Count * (uint)100 / AudioBufferMaxOccupancy; 
            Profiling = new StopWatches();
            Profiling.retroRun.Start();
            retro_run();
            Profiling.retroRun.Stop();
            WriteConsole($"[Run] {Profiling.ToString()} | Audio occupancy {AudioPercentOccupancy}%");
#else
            retro_run();
#endif
        }

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

        if (!Speaker.isPlaying)
            Speaker.Play(); //why is this neccesary?

        return;
    }

    public static void End(string screenName, string gameFileName)
    {
        if (gameFileName != GameFileName || screenName != ScreenName)
        {
            return;
        }

        WriteConsole($"[LibRetroMameCore.End] Unload game: {GameFileName}");
        //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
        retro_unload_game();

        ClearAll();

        WriteConsole("[LibRetroMameCore.End] END  *************************************************");
    }

    private static void ClearAll()
    {
        WriteConsole("[LibRetroMameCore.ClearAll]");
        FPSControl = null;
        GameTexture = null;
        AudioBatch = new List<float>();

        if (Speaker != null && Speaker.isPlaying)
        {
            WriteConsole("[LibRetroMameCore.ClearAll] Pause Speaker");
            Speaker.Pause();
            Speaker = null;
        }

        if (PtrVault != null)
        {
            WriteConsole("[LibRetroMameCore.ClearAll] Free Pointers");
            PtrVault.Free();
            PtrVault = new MarshalHelpPtrVault();
        }

        GameFileName = "";
        ScreenName = "";
        GameLoaded = false;

        CoinSlot?.clean();
        CoinSlot = null;

        WriteConsole("[LibRetroMameCore.ClearAll] Unloaded and clear  *************************************************");
    }


    [AOT.MonoPInvokeCallback(typeof(CreateTextureHandler))]
    static void CreateTexture(uint width, uint height)
    {
        WriteConsole($"[CreateTexture] {width}, {height}");
        GameTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB565, false);
        GameTexture.filterMode = FilterMode.Point;
        shader.Texture = GameTexture;
    }
    [AOT.MonoPInvokeCallback(typeof(LoadTextureDataHandler))]
    static void LoadTextureData(IntPtr data, uint size)
    {
        //WriteConsole($"[LoadTextureData] {size} bytes");
        GameTexture.LoadRawTextureData(data, (int)size);
        GameTexture.Apply(false, false);
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

    // https://github.com/RetroPie/RetroPie-Docs/blob/219c93ca6a81309eed937bb5b7a79b8c71add41b/docs/RetroArch-Configuration.md
    // https://docs.libretro.com/library/mame2003_plus/#default-retropad-layouts
    [AOT.MonoPInvokeCallback(typeof(inputStateHandler))]
    static Int16 inputStateCB(UInt32 port, UInt32 device, UInt32 index, UInt32 id)
    {
        Int16 ret = 0;

        if (WaitToFinishedGameLoad != null && !WaitToFinishedGameLoad.Finished())
            return ret;

        /*
        if (device == RETRO_DEVICE_JOYPAD)
            WriteConsole($"[inputStateCB] dev {device} port {port} index:{index} id:{deviceIdsJoypad.Id(id)}");
        else if (device == RETRO_DEVICE_MOUSE)
            WriteConsole($"[inputStateCB] dev {device} port {port} index:{index} id:{deviceIdsMouse.Id(id)}");
        else
            WriteConsole($"[inputStateCB] device unknown! {device}");
        */

        //if (port != 0)
        //    return ret;

#if _debug_fps_
      Profiling.input.Start();
#endif
        //port: 0 device: 1 index: 0 id: 2 (select) Coin

        if (device == RETRO_DEVICE_JOYPAD)
        {
            //InputControlDebug(RETRO_DEVICE_JOYPAD);
            switch (id)
            {
                case RETRO_DEVICE_ID_JOYPAD_SELECT:
                    //WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_JOYPAD_SELECT: {CoinSlot.ToString()}");
                    ret = (CoinSlot != null && CoinSlot.takeCoin()) ||
                            ControlMap.Active("INSERT") != 0 ? (Int16)1 : (Int16)0;
                    if (ret == 1)
                    { //hack for pacman and others.
                        HotDelaySelectCycles = 5;
                    }

                    if (HotDelaySelectCycles > 0 && ret != (Int16)1)
                    {
                        HotDelaySelectCycles--;
                        ret = (Int16)1;
                        //ConfigManager.WriteConsole($"[inputStateCB] TakeCoin JOYPAD_SELECT: ret: {ret} cycle# {HotDelaySelectCycles}");
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
                    break;
            }
            if (ret != 0)
                ConfigManager.WriteConsole($"[inputStateCB] JOYPAD id: {id} name: {deviceIdsJoypad.Id(id)} ret: {ret}");
        }

        else if (device == RETRO_DEVICE_MOUSE)
        {
            //InputControlDebug(RETRO_DEVICE_MOUSE);
            ret = (Int16)deviceIdsMouse.Active(id, (int)port);
            if (ret != 0)
                ConfigManager.WriteConsole($"[inputStateCB] MOUSE id: {id} name: {deviceIdsMouse.Id(id)} ret: {ret}");

        }

#if _debug_fps_
      Profiling.input.Stop();
#endif

        return ret;
    }

    [AOT.MonoPInvokeCallback(typeof(audioSampleHandler))]
    static void audioSampleCB(Int16 left, Int16 right)
    {
        WriteConsole("[LibRetroMameCore.audioSampleCB] left: " + left + " right: " + right);
        return;
    }

    [AOT.MonoPInvokeCallback(typeof(audioSampleBatchHandler))]
    static ulong audioSampleBatchCB(short* data, ulong frames)
    {
        //WriteConsole($"[LibRetroMameCore.audioSampleBatchCB] AUDIO IN from MAME - frames:{frames} batch actual load: {AudioBatch.Count}");

        if (data == (short*)IntPtr.Zero)
        {
            return 0;
        }

        if (AudioBatch.Count > AudioBufferMaxOccupancy)
        {
            //overrun
            return 0;
        }

#if _debug_fps_
        Profiling.audio.Start();
#endif

        var inBuffer = new List<float>();
        for (ulong i = 0; i < frames * 2; ++i)
        {
            // float value = Mathf.Clamp(data[i]  / 32768f, -1.0f, 1.0f); // 0.000030517578125f o 0.00048828125; //to convert from 16 to fp 
            float value = data[i] / 32768f;
            inBuffer.Add(value);
        }

        double ratio = (double)wrapper_environment_get_sample_rate() / QuestAudioFrequency;
        int outSample = 0;
        while (true)
        {
            int inBufferIndex = (int)(outSample++ * ratio);
            if (inBufferIndex < (int)frames * 2)
                AudioBatch.Add(inBuffer[inBufferIndex]);
            else
                break;
        }

#if _debug_fps_
        Profiling.audio.Stop();
#endif
        return frames;
    }

    public static void MoveAudioStreamTo(string _gameFileName, float[] data)
    {
        if (!GameLoaded || GameFileName != _gameFileName)
        {
            //It is neccesary to call Run() method for the game in the parameter?
            return;
        }
        if (AudioBatch == null || AudioBatch.Count == 0)
        {
            return;
        }

        int toCopy = AudioBatch.Count >= data.Length ? data.Length : AudioBatch.Count;

        if (toCopy > 0)
        {
            AudioBatch.CopyTo(0, data, 0, toCopy);
            AudioBatch.RemoveRange(0, toCopy);
        }
#if _debug_audio_
        WriteConsole($"[LibRetroMameCore.LoadAudio] AUDIO OUT output buffer length: {data.Length} frames loaded from MAME: {AudioBatch.Count} toCopy: {toCopy} ");
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
    private static void getSystemInfo()
    {
        WriteConsole("[LibRetroMameCore.getSystemInfo] retro_get_system_info ");
        SystemInfo = new retro_system_info();
        MarshalHelpCalls<retro_system_info> m = new MarshalHelpCalls<retro_system_info>();
        retro_get_system_info(m.GetPtr(SystemInfo));
        m.CopyTo(SystemInfo).Free();
        WriteConsole(SystemInfo.ToString());
    }



    //storage pointers to unmanaged memory
    public class MarshalHelpPtrVault
    {
        private List<IntPtr> vault = new List<IntPtr>();

        public IntPtr GetPtr(string str)
        {
            IntPtr p = Marshal.StringToHGlobalAnsi(str);
            if (p == IntPtr.Zero)
                throw new OutOfMemoryException();
            vault.Add(p);
            WriteConsole($"[LibRetroMameCore.MarshalHelpPtrVault] add: {p} ");
            return p;
        }
        public IntPtr GetPtr(bool b)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(b));
            if (p == IntPtr.Zero)
                throw new OutOfMemoryException();
            Marshal.WriteByte(p, b ? (byte)1 : (byte)0);
            vault.Add(p);
            return p;
        }
        public void Free()
        {
            foreach (IntPtr p in vault)
            {
                WriteConsole($"[LibRetroMameCore.MarshalHelpPtrVault] free: {p} ");
                Marshal.FreeHGlobal(p);
            }
            vault = new List<IntPtr>();
        }
        ~MarshalHelpPtrVault()
        {
            WriteConsole($"[LibRetroMameCore.MarshalHelpPtrVault] destroy ");
            Free();
        }
    }

    //helper to convert structs to pointers used call functions.
    public class MarshalHelpCalls<T>
    {
        IntPtr _p = IntPtr.Zero;
        //Alloc global memory and copy the object, returns the pointer.
        public IntPtr GetPtr(T obj)
        {
            _p = Marshal.AllocHGlobal(Marshal.SizeOf<T>(obj));
            if (_p == IntPtr.Zero)
                throw new OutOfMemoryException();
            Marshal.StructureToPtr(obj, _p, false);
            return _p;
        }
        public IntPtr Ptr
        {
            get
            {
                return _p;
            }
        }
        //copy from global memory to the object. Returns this.
        public MarshalHelpCalls<T> CopyTo(T obj)
        {
            if (_p == IntPtr.Zero)
                throw new OutOfMemoryException();
            Marshal.PtrToStructure(_p, obj);
            return this;
        }
        //frees global memory. return this
        public MarshalHelpCalls<T> Free()
        {
            if (_p != IntPtr.Zero)
            {
                // Marshal.DestroyStructure(_p, typeof(T));
                Marshal.FreeHGlobal(_p);
                _p = IntPtr.Zero;
            }
            return this;
        }
        ~MarshalHelpCalls()
        {
            Free();
        }
    }

    public class FpsControl
    {
        float timeBalance = 0;
        float timePerFrame = 0;
        uint frameCount = 0;
        float acumTime = 0;

        public FpsControl(float FPSExpected)
        {
            timeBalance = 0;
            frameCount = 0;
            timePerFrame = 1f / FPSExpected;
        }
        public void CountTimeFrame()
        {
            timeBalance += Time.deltaTime;
            acumTime += Time.deltaTime;
            frameCount++;
        }
        public void Reset()
        {
            timeBalance = 0;
            frameCount = 0;
            acumTime = 0;
        }
        public float fps()
        {
            return frameCount / acumTime;
        }
        public bool isTime()
        {
            //deltaTime: time from the last call
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
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"[{GameFileName}] - {st}");
    }

    public class Waiter
    {
        public bool _finished = false;
        DateTime _started = DateTime.MaxValue;
        int _waitSecs = 0;

        public Waiter(int _pwaitSecs)
        {
            _waitSecs = _pwaitSecs;
        }
        public int WaitSecs
        {
            get
            {
                return _waitSecs;
            }
        }
        public void reset()
        {
            _started = DateTime.MaxValue;
            _finished = false;
        }
        public bool Finished()
        {
            if (!_finished)
            {
                if (_started == DateTime.MaxValue)
                {
                    _started = DateTime.Now;
                }
                _finished = _started.AddSeconds(_waitSecs) < DateTime.Now;
            }
            return _finished;
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
