/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

//#define _debug_fps_
//#define _debug_audio_
#define _debug_

/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.IO;
using Unity.Jobs;
using Unity.Collections;
using System.Diagnostics;


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

    [DllImport ("mame2003_plus_libretro_android")]
    //private static extern void retro_get_system_info([MarshalAs(UnmanagedType.LPStruct)] retro_system_info info);
    private static extern void retro_get_system_info(IntPtr info);

    [DllImport ("mame2003_plus_libretro_android")]
    private static extern int retro_api_version();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] public delegate uint APIVersionSignature();
    
    // retro_set_environment ------------------
    /*retro_set_environment() is guaranteed to be called before retro_init().*/
    //[UnmanagedFunctionPointer(CallingConvention.Cdecl)] 
    private delegate bool EnvironmentHandler(uint cmd, IntPtr data);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_environment(EnvironmentHandler env);

    // retro_set_video_refresh -------------------------------
    private delegate void videoRefreshHandler(IntPtr data, uint width, uint height, uint pitch);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_video_refresh(videoRefreshHandler vrh);
    
#region INPUT
    public const int RETRO_DEVICE_TYPE_SHIFT = 8;
    public const int RETRO_DEVICE_MASK       = (1 << RETRO_DEVICE_TYPE_SHIFT) - 1;
    public const uint RETRO_DEVICE_NONE     = 0;
    public const uint RETRO_DEVICE_JOYPAD   = 1;
    public const uint RETRO_DEVICE_MOUSE    = 2;
    public const uint RETRO_DEVICE_ANALOG   = 5;
    // public const uint RETRO_DEVICE_KEYBOARD   = 3;
    // public const uint RETROK_5              = 53;
    // public const uint RETRO_DEVICE_ANALOG   = 5;
    // // public const uint RETRO_DEVICE_MOUSE    = 2;
    // public const uint RETRO_DEVICE_KEYBOARD = 3;
    // public const uint RETRO_DEVICE_LIGHTGUN = 4;
    // public const uint RETRO_DEVICE_ANALOG   = 5;
    // public const uint RETRO_DEVICE_POINTER  = 6;

    public const uint RETRO_DEVICE_ID_JOYPAD_B      = 0;
    public const uint RETRO_DEVICE_ID_JOYPAD_Y      = 1;
    public const uint RETRO_DEVICE_ID_JOYPAD_SELECT = 2;
    public const uint RETRO_DEVICE_ID_JOYPAD_START  = 3;
    public const uint RETRO_DEVICE_ID_JOYPAD_UP     = 4;
    public const uint RETRO_DEVICE_ID_JOYPAD_DOWN   = 5;
    public const uint RETRO_DEVICE_ID_JOYPAD_LEFT   = 6;
    public const uint RETRO_DEVICE_ID_JOYPAD_RIGHT  = 7;
    public const uint RETRO_DEVICE_ID_JOYPAD_A      = 8;
    public const uint RETRO_DEVICE_ID_JOYPAD_X      = 9;
    public const uint RETRO_DEVICE_ID_JOYPAD_L      = 10;
    public const uint RETRO_DEVICE_ID_JOYPAD_R      = 11;
    public const uint RETRO_DEVICE_ID_JOYPAD_L2     = 12;
    public const uint RETRO_DEVICE_ID_JOYPAD_R2     = 13;
    public const uint RETRO_DEVICE_ID_JOYPAD_L3     = 14;
    public const uint RETRO_DEVICE_ID_JOYPAD_R3     = 15;
    public const uint RETRO_DEVICE_ID_JOYPAD_MASK   = 256;

    public const uint RETRO_DEVICE_ID_ANALOG_X = 0;
    public const uint RETRO_DEVICE_ID_ANALOG_Y = 1;

    public const uint RETRO_DEVICE_ID_MOUSE_X = 0;
    public const uint RETRO_DEVICE_ID_MOUSE_Y = 1;
    public const uint RETRO_DEVICE_ID_MOUSE_LEFT = 2;
    public const uint RETRO_DEVICE_ID_MOUSE_RIGHT = 3;

    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_controller_port_device(uint port, uint device);
    // retro_set_input_poll -------------------------------
    private delegate void inputPollHander();
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_input_poll(inputPollHander iph);
    static int HotDelaySelectCycles = 0;
    
    // retro_set_input_state -------------------------------
    private delegate Int16 inputStateHandler(uint port, uint device, uint index, uint id);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_input_state(inputStateHandler ish);


#endregion
#region LOG
    // LibRetro log callback implementation ----------------------------------------
    public enum retro_log_level
    {
        RETRO_LOG_DEBUG = 0,
        RETRO_LOG_INFO  = 1,
        RETRO_LOG_WARN  = 2,
        RETRO_LOG_ERROR = 3
    }
    static retro_log_level MinLogLevel = retro_log_level.RETRO_LOG_INFO;

    public delegate void logHandler(retro_log_level level, [In, MarshalAs(UnmanagedType.LPStr)] string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_log_callback
    {
        public IntPtr log; // retro_log_printf_t
    }

    static int bufLogSize = 2*1024;
    static IntPtr buf = Marshal.AllocHGlobal(bufLogSize); //there is a risk here.
            
    [AOT.MonoPInvokeCallback (typeof(logHandler))]
    public static void MamePrintf(retro_log_level level, string format, 
                                  IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8, IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12)
    {
        if (level >= MinLogLevel) {
            snprintf(buf, bufLogSize, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            string str = Marshal.PtrToStringAnsi(buf);
            WriteConsole($"[{level}] {str}");
            // Marshal.FreeHGlobal(buf); //te pointer dies with the program, no memleak here.
        }
    }
    // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.dllimportattribute.callingconvention?view=net-6.0
    //https://www.codeproject.com/Articles/19274/A-printf-implementation-in-C
    // public static extern int sprintf(IntPtr buffer, string format, __arglist); __arglist fails
    // based on @asimonf implementation
    //https://github.com/asimonf/RetroLite/blob/4a8acd5a1db353bfa76e6af238523260483e0b89/LibRetro/Native/LinuxHelper.cs
    [DllImport("c", CallingConvention = CallingConvention.Cdecl)]
    private static extern int snprintf(IntPtr buffer, int maxSize, string format, IntPtr arg1, IntPtr arg2, IntPtr arg3, IntPtr arg4, IntPtr arg5, IntPtr arg6, IntPtr arg7, IntPtr arg8,
                                       IntPtr arg9, IntPtr arg10, IntPtr arg11, IntPtr arg12);

#endregion
#region AUDIO

    // retro_set_audio_sample -------------------------------
    private delegate void audioSampleHandler(Int16 left, Int16 right);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_audio_sample(audioSampleHandler sah);

    // retro_set_audio_sample_batch -------------------------------
    private delegate ulong audioSampleBatchHandler(short* data, ulong frames);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_set_audio_sample_batch(audioSampleBatchHandler sah);

    // [DllImport ("mame2003_plus_libretro_android")]
    // private static extern void retro_audio_buff_status_cb(bool active, uint occupancy, bool underrun_likely);

    // retro_set_audio_sample_batch -------------------------------
    public delegate void AudiobufferStatustHandler(bool active, uint occupancy, bool underrun_likely);
    [StructLayout(LayoutKind.Sequential)]
    public struct retro_audio_buffer_status_callback {
        public IntPtr callback;
    }

    public static AudiobufferStatustHandler AudioBufferStatusInfo;

    // audio buffer ===============
    public static List<float> AudioBatch = new List<float>();
    static uint AudioBufferMaxOccupancy = 1024 * 8;
    static int QuestAudioFrequency = 48000; //Quest 2 standar, can change at start

#endregion

    // retro_init -------------------------------
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_init();
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_deinit();
    // retro_run -------------------------------
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_run();
    // retro_load_game -------------------------------
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct retro_game_info
    {
        public char* path;
        public void* data;
        public uint size;
        public char* meta;
    }
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern bool retro_load_game(ref retro_game_info game);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern void retro_unload_game();
    
    // serialization -------------------
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern uint retro_serialize_size();
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern bool retro_serialize(IntPtr info, uint size);
    [DllImport ("mame2003_plus_libretro_android")]
    private static extern bool retro_unserialize(IntPtr info, uint size);

    [StructLayout(LayoutKind.Sequential)]
    public class retro_game_geometry
    {
        public uint base_width;    /* Nominal video width of game. */
        public uint base_height;   /* Nominal video height of game. */
        public uint max_width;     /* Maximum possible width of game. */
        public uint max_height;    /* Maximum possible height of game. */

        public float aspect_ratio;  /* Nominal aspect ratio of game. If
                                    * aspect_ratio is <= 0.0, an aspect ratio
                                    * of base_width / base_height is assumed.
                                    * A frontend could override this setting,
                                    * if desired. */
    };
    [StructLayout(LayoutKind.Sequential)]
    public class retro_system_timing
    {
        public double fps;             /* FPS of video content. */
        public double sample_rate;     /* Sampling rate of audio. */
    };
    [StructLayout(LayoutKind.Sequential)]
    public class retro_system_av_info
    {
        public retro_game_geometry geometry;
        public retro_system_timing timing;

        public override string ToString() {
            return String.Format("Geo:\n" +
                          "    base_width:{0}\n" +
                          "    base_height:{1}\n" +
                          "    max_width:{2}\n" +
                          "    max_height:{3}\n" +
                          "    aspect_ratio:{4}\n" +
                          "system timing:\n " +
                          "    fps:{5}\n " +
                          "    sample_rate (audio):{6}\n ",
                          GameAVInfo.geometry.base_width, GameAVInfo.geometry.base_height, GameAVInfo.geometry.max_width, 
                          GameAVInfo.geometry.max_height, GameAVInfo.geometry.aspect_ratio,
                          GameAVInfo.timing.fps, GameAVInfo.timing.sample_rate
                          );
        }
    };

    [DllImport ("mame2003_plus_libretro_android")]       
    private static extern void retro_get_system_av_info(IntPtr info);
    
    // pixel formats
    public enum retro_pixel_format
    {
        RETRO_PIXEL_FORMAT_0RGB1555 = 0,
        RETRO_PIXEL_FORMAT_XRGB8888 = 1,
        RETRO_PIXEL_FORMAT_RGB565   = 2,
        RETRO_PIXEL_FORMAT_UNKNOWN  = Int32.MaxValue
    }
    public static retro_pixel_format pixelFormat;
    private static List<retro_pixel_format> acceptedPixelFormats = new List<retro_pixel_format> { 
        retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555,
        retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888,
        retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565
    };
    
    // user control
    //games have to initialize and then they can accept controls.
    private static Waiter WaitToFinishedGameLoad = null;
    private static FpsControl controlForAskIfTimeToExitGame;

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

    //image ===========    
    static FpsControl FPSControl;
    public static Texture2D GameTexture;
    // public static GameObject Camera;

    //parameters ================
    
    public static int SecondsToWaitToFinishLoad = 2;
    
    //components parameters
    public static Renderer Display;
    public static AudioSource Speaker;
    public static CoinSlotController CoinSlot; 

    //game info and storage.
    public static retro_system_info SystemInfo = new();
    public static retro_system_av_info GameAVInfo = new();
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

    //Mame configuration
    private static string FrameSkip = "default"; //"auto_aggressive"; //10"; //"auto";
    private static string SkipDisclaimer = "enabled";
    private static string SkipWarnings = "enabled";

    public enum GammaOptions {
        GAMA_0_5,
        GAMA_0_6,
        GAMA_0_7,
        GAMA_0_8,
        GAMA_0_9,
        GAMA_1,
        GGAMA_1_1,
        GGAMA_1_2,
        GGAMA_1_3,
        GGAMA_1_4,
        GGAMA_1_5,
        GGAMA_1_6,
        GGAMA_1_7,
        GGAMA_1_8,
        GGAMA_1_9,
        GAMA_2
    }
    public enum BrightnessOptions {
        BRIGHT_0_2,
        BRIGHT_0_3,
        BRIGHT_0_4,
        BRIGHT_0_5,
        BRIGHT_0_6,
        BRIGHT_0_7,
        BRIGHT_0_8,
        BRIGHT_0_9,
        BRIGHT_1,
        GBRIGHT_1_1,
        GBRIGHT_1_2,
        GBRIGHT_1_3,
        GBRIGHT_1_4,
        GBRIGHT_1_5,
        GBRIGHT_1_6,
        GBRIGHT_1_7,
        GBRIGHT_1_8,
        GBRIGHT_1_9,
        BRIGHT_2
    }
    public static GammaOptions Gamma = GammaOptions.GAMA_1;
    private static string[] GammaOptionsList = "0.2|0.3|0.4|0.5|0.6|0.7|0.8|0.9|1.0|1.1|1.2|1.3|1.4|1.5|1.6|1.7|1.8|1.9|2.0".Split("|");
    public static BrightnessOptions Brightness = BrightnessOptions.BRIGHT_1;
    private static string[] BrightnessOptionsList = "0.2|0.3|0.4|0.5|0.6|0.7|0.8|0.9|1.0|1.1|1.2|1.3|1.4|1.5|1.6|1.7|1.8|1.9|2.0".Split("|");

    // private static string ShowGameInfo = "disabled";
    
    private static MarshalHelpPtrVault PtrVault = new();

    public static bool Start(string screenName, string gameFileName) {

        if (! Initialized) {
            WriteConsole("---------------------------------------------------------");
            WriteConsole("------------------- LIBRETRO INIT -----------------------");
            WriteConsole("---------------------------------------------------------");

            //Audio configuration
            var audioConfig = AudioSettings.GetConfiguration();
            QuestAudioFrequency = audioConfig.sampleRate;
            WriteConsole($"[LibRetroMameCore.Start] AUDIO Quest Sample Rate:{QuestAudioFrequency} dspBufferSize: {audioConfig.dspBufferSize}");
            
            WriteConsole("------------------- SETs");
            WriteConsole("retro_set_environment");
            retro_set_environment(new EnvironmentHandler(environmentCB));
            WriteConsole("retro_set_video_refresh");
            retro_set_video_refresh(new videoRefreshHandler(videoRefreshCB));
            WriteConsole("retro_set_audio_sample");
            retro_set_audio_sample(new audioSampleHandler(audioSampleCB));
            WriteConsole("retro_set_audio_sample_batch");
            retro_set_audio_sample_batch(new audioSampleBatchHandler(audioSampleBatchCB));
            WriteConsole("retro_set_input_poll");
            retro_set_input_poll(new inputPollHander(inputPollCB));
            WriteConsole("retro_set_input_state");
            retro_set_input_state(new inputStateHandler(inputStateCB));

            WriteConsole("------------------- retro_init");
            retro_init();

            WriteConsole("retro_set_controller_port_device");
            retro_set_controller_port_device(port: 0, device: RETRO_DEVICE_JOYPAD);

            GetSystemInfo();
            
            Initialized = true;

        }

        if (GameLoaded) {
            WriteConsole($"[LibRetroMameCore.Start] ERROR the Start method was called, but a Game was loaded previously ({GameFileName}), it's neccesary to call End() before the Start()");
            return false;
        }

        if (!String.IsNullOrEmpty(GameFileName) || !String.IsNullOrEmpty(ScreenName)) {
            WriteConsole($"[LibRetroMameCore.Start] ERROR: MAME previously initalized with [{GameFileName} in {ScreenName}], End() is needed");
            return false;
        }

        GameFileName = gameFileName;
        ScreenName = screenName;

        WriteConsole($"------------------- retro_load_game {GameFileName} in {ScreenName}");
        if (SystemInfo.need_fullpath) {
            string path = ConfigManager.RomsDir + "/" + GameFileName;
            if (File.Exists(path) ) {
                retro_game_info game = new retro_game_info();
                MarshalHelpPtrVault p = new();
                game.path = (char*)p.GetPtr(path);
                game.size = 0;
                game.data = (char*)IntPtr.Zero;

                // MarshalHelpCalls<retro_game_info> gameInfo = new();
                //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L740
                //in this instance MAME call the needed callbacks to establish the game parameters.
                WriteConsole($"[LibRetroMameCore.Start] loading:{path}");
                GameLoaded = retro_load_game(ref game);
                p.Free();

                if (! GameLoaded) {
                    ClearAll();
                    WriteConsole($"[LibRetroMameCore.Start] ERROR {path} MAME can't start the game, please check if it is the correct version and is supported in MAME2003+ in https://buildbot.libretro.com/compatibility_lists/cores/mame2003-plus/mame2003-plus.html.");
                    return false;
                }
                else 
                    WriteConsole($"[LibRetroMameCore.Start] Game Loaded:{path}");
            }
            else {
                ClearAll();
                WriteConsole($"[LibRetroMameCore.Start] ERROR {path} don't exists or inaccesible.");
                return false;
            }
        }
        else {
            ClearAll();
            WriteConsole("[LibRetroMameCore.Start] ERROR only implemented MAME full path");
            return false;
        }

        if (GameLoaded)
        {
          getAVGameInfo();

          FPSControl = new FpsControl((float)GameAVInfo.timing.fps);

          /* It's impossible to change the Sample Rate, fixed in 48000
          audioConfig.sampleRate = sampleRate;
          AudioSettings.Reset(audioConfig);
          audioConfig = AudioSettings.GetConfiguration();
          WriteConsole($"[LibRetroMameCore.Start] New audio Sample Rate:{audioConfig.sampleRate}");
          */

          WriteConsole(
            $"[LibRetroMameCore.Start] AUDIO Mame2003+ frequency {GameAVInfo.timing.sample_rate} | Quest: {QuestAudioFrequency}");

          // Audioclips are not sinchronized with the video.
          //Speaker.clip = AudioClip.Create("MameAudioSource", sampleRate * 2, 2, sampleRate, true, OnAudioRead /*, onAudioChangePosition*/);

          Speaker.Play();
          WriteConsole($"[LibRetroMameCore.Start] Game Loaded: {GameLoaded} in {GameFileName} in {ScreenName} ");

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
        }
        return true;
    }

    public static bool isRunning(string screenName, string gameFileName) {
        return GameLoaded && GameFileName == gameFileName && screenName == ScreenName;
    }

    public static void Run(string screenName, string gameFileName) {
        if (!isRunning(screenName, gameFileName)) {
            return;
        }
        // WriteConsole($"[LibRetroMameCore.Run] running screen: {screenName} game:{gameFileName}");
        
        // https://docs.unity3d.com/ScriptReference/Time-deltaTime.html
        FPSControl.CountTimeFrame();
        while (FPSControl.isTime()) {
            // https://github.com/libretro/mame2003-plus-libretro/blob/6de44ee0a37b32a85e0aec013924bef34996ef35/src/mame2003/video.c#L400
            // https://github.com/libretro/mame2003-plus-libretro/issues/1323
            uint AudioPercentOccupancy = (uint)AudioBatch.Count * (uint)100 / AudioBufferMaxOccupancy; 
            // AudioBufferStatusInfo(true, AudioPercentOccupancy, AudioPercentOccupancy > 80);

#if _debug_fps_
            Profiling = new();
            Profiling.retroRun.Start();
            retro_run();
            Profiling.retroRun.Stop();
            WriteConsole($"[Run] {Profiling.ToString() | Audio occupancy {AudioPercentOccupancy}%}");
#else

            retro_run();

#endif
        }

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

        if (!Speaker.isPlaying) {
            Speaker.Play(); //why is this neccesary?
        }

#if _debug_fps_
        WriteConsole($"[Run] {FPSControl.ToString()} ");
#endif
        return;
    }

    public static void End(string screenName, string gameFileName)     {
        if (gameFileName != GameFileName || screenName != ScreenName) {
            return;
        }

        WriteConsole($"[LibRetroMameCore.Run] Unload game: {GameFileName}");
        //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L1054
        retro_unload_game();

        ClearAll();

        //LockControls(false);

        WriteConsole("[LibRetroMameCore.Run] End *************************************************");
    }

    private static void ClearAll() {
        //TODO
        WriteConsole("[LibRetroMameCore.ClearAll]  *************************************************");
        FPSControl = null;
        GameTexture = null;
        AudioBatch =  new List<float>();
        // SystemInfo = new();
        GameAVInfo = new();

        if (Speaker != null && Speaker.isPlaying) {
            WriteConsole("[LibRetroMameCore.ClearAll] Pause Speaker");
            Speaker.Pause();
            Speaker = null;
        }

        if (PtrVault != null) {
            WriteConsole("[LibRetroMameCore.ClearAll] Free Pointers");
            PtrVault.Free();
            PtrVault = new();
        }

        GameFileName = "";
        ScreenName = "";
        GameLoaded = false;

        CoinSlot?.clean();
        CoinSlot = null;
        
        WriteConsole("[LibRetroMameCore.ClearAll] Unloaded and clear  *************************************************");
    }

    //#define RETRO_ENVIRONMENT_EXPERIMENTAL 65536 0x10000
    private enum envCmds
    {
        RETRO_ENVIRONMENT_SET_ROTATION = 1,
        RETRO_ENVIRONMENT_SET_MESSAGE = 6,
        RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL = 8, //TODO mame2003
        RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY = 9,
        RETRO_ENVIRONMENT_SET_PIXEL_FORMAT = 10,
        RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS = 11,
        RETRO_ENVIRONMENT_GET_VARIABLE = 15,
        RETRO_ENVIRONMENT_SET_VARIABLES = 16,
        RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE = 17,
        RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES = 24,
        RETRO_ENVIRONMENT_GET_LOG_INTERFACE = 27, //TODO mame2003
        RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY = 31,
        RETRO_ENVIRONMENT_SET_CONTROLLER_INFO = 35,
        RETRO_ENVIRONMENT_SET_GEOMETRY = 37,
        RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION = 52,
        RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK = 62,
        RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY = 63,
        RETRO_ENVIRONMENT_GET_VFS_INTERFACE = (45 | 0x10000),
        RETRO_ENVIRONMENT_GET_LED_INTERFACE = (46 | 0x10000),
        RETRO_ENVIRONMENT_GET_INPUT_BITMASKS = (51 | 0x10000)
    }

    //must to be struct to be unmanaged?
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct retro_variable_pointers {
        public char *key;
        public char *value;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct retro_variable {
        public string key;
        public string value;
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private class retro_input_descriptor
    {
        /* Associates given parameters with a description. */
        public uint port;
        public uint device;
        public uint index;
        public uint id;

        /* Human readable description for parameters.
            * The pointer must remain valid until
            * retro_unload_game() is called. */
        [MarshalAs(UnmanagedType.LPTStr)] public string description;
    }
    /*
    [AOT.MonoPInvokeCallback (typeof(retro_audio_buffer_status_callback))]
    public static void audioBufferStatusCB(bool active, uint occupancy, bool underrun_likely)
    {
        WriteConsole("[LibRetroMameCore.audioBufferStatusCB] active " + active);
    }
    */
    
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct retro_controller_description
    {
        /* Human-readable description of the controller. Even if using a generic
            * input device type, this can be set to the particular device type the
            * core uses. */
        [MarshalAs(UnmanagedType.LPTStr)] public string desc;

        /* Device type passed to retro_set_controller_port_device(). If the device
            * type is a sub-class of a generic input device type, use the
            * RETRO_DEVICE_SUBCLASS macro to create an ID.
            *
            * E.g. RETRO_DEVICE_SUBCLASS(RETRO_DEVICE_JOYPAD, 1). */
        public uint id;
    };
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct retro_controller_info
    {
        public IntPtr types; //retro_controller_description
        public uint num_types;
    };

    [AOT.MonoPInvokeCallback (typeof(EnvironmentHandler))]
    static unsafe bool environmentCB(uint cmd, IntPtr data)
    {
        //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c
        switch ((envCmds)cmd) {
            case envCmds.RETRO_ENVIRONMENT_GET_VARIABLE:
            {
                // WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_VARIABLE ");
                if (data == IntPtr.Zero)
                    return false;

                // retro_variable gvar = (retro_variable)Marshal.PtrToStructure(data, typeof(retro_variable));
                retro_variable_pointers *gvp = (retro_variable_pointers*)data; //Marshal.PtrToStructure(data, typeof(retro_variable_pointers));
                string key = Marshal.PtrToStringAnsi(new IntPtr(gvp->key));
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_VARIABLE key " + key);
                switch (key) {
                    //mame2000-sample_rate = 48000 default
                    //mame2000-stereo = true default
                    case "mame2003-plus_frameskip":
                        gvp->value = (char *)PtrVault.GetPtr(FrameSkip);
                        WriteConsole("FrameSkip value to return:" + FrameSkip);
                        return true;
                    case "mame2003-plus_skip_disclaimer":
                        gvp->value = (char *)PtrVault.GetPtr(SkipDisclaimer);
                        WriteConsole("SkipDisclaimer value to return:" + SkipDisclaimer);
                        return true;
                    case "mame2003-plus_skip_warnings":
                        gvp->value = (char *)PtrVault.GetPtr(SkipWarnings);
                        WriteConsole("SkipWarnings value to return:" + SkipWarnings);
                        return true;
                    //audio
                    case "mame2003-plus_use_samples":
                        string use_samples = "enabled";
                        gvp->value = (char *)PtrVault.GetPtr(use_samples);
                        WriteConsole("use_samples value to return:" + use_samples);
                        return true;
                    case "mame2003-plus_xy_device":
                        //https://github.com/libretro/mame2003-plus-libretro/blob/15349c45296e16f9385a90002018d920e8f3f872/src/mame2003/core_options.c#L66
                        string xyDevice = "mouse";
                        gvp->value = (char *)PtrVault.GetPtr(xyDevice);
                        WriteConsole("xy_device value to return:" + xyDevice);
                        return true;
                    case "mame2003-plus_machine_timing":
                        string machine_timing = "disabled";
                        gvp->value = (char *)PtrVault.GetPtr(machine_timing);
                        WriteConsole("input_interface value to return:" + machine_timing);
                        return true;
                    case "mame2003-plus_sample_rate":
                        string _freq = QuestAudioFrequency.ToString();
                        gvp->value = (char *)PtrVault.GetPtr(_freq);
                        WriteConsole("AudioSampleRate value to return:" + _freq);
                        return true;
                    case "mame2003-plus_brightness":
                        //mame2003-plus_brightness set to value:Brightness; 1.0|0.2|0.3|0.4|0.5|0.6|0.7|0.8|0.9|1.1|1.2|1.3|1.4|1.5|1.6|1.7|1.8|1.9|2.0
                        string optionBri = BrightnessOptionsList[(int)Brightness];
                        gvp->value = (char *)PtrVault.GetPtr(optionBri);
                        WriteConsole("plus_brightness value to return:" + optionBri);
                        return true;
                    case "mame2003-plus_gamma":
                        //mame2003-plus_gamma set to value:Gamma Correction; 1.0|0.5|0.6|0.7|0.8|0.9|1.1|1.2|1.3|1.4|1.5|1.6|1.7|1.8|1.9|2.0
                        string optionGa = GammaOptionsList[(int)Gamma];
                        gvp->value = (char *)PtrVault.GetPtr(optionGa);
                        WriteConsole("plus_gamma value to return:" + optionGa);
                        return true;
                    case "mame2003-plus_input_interface":
                        string retropad = "simultaneous";
                        gvp->value = (char *)PtrVault.GetPtr(retropad);
                        WriteConsole("input_interface value to return:" + retropad);
                        return true;
                    case "mame2003-plus_cpu_clock_scale":
                        string clockScale = "default";
                        gvp->value = (char *)PtrVault.GetPtr(clockScale);
                        WriteConsole("cpu_clock_scale value to return:" + clockScale);
                        return true;
                    // case "mame2000-show_gameinfo": why hangs?
                    //     gvp->value = (char *)PtrVault.GetPtr(ShowGameInfo);
                    //     WriteConsole("value to return:" + ShowGameInfo);
                    //     break;
                }

                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_VARIABLES:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_VARIABLES");

                if (data == IntPtr.Zero)
                    return false;

                //show control variables in the log
                retro_variable_pointers *gvpSetVar = (retro_variable_pointers*)data;
                if (gvpSetVar->key != null) {
                    do {
                        string MameVar = Marshal.PtrToStringAnsi(new IntPtr(gvpSetVar->key));
                        string MameOptions = Marshal.PtrToStringAnsi(new IntPtr(gvpSetVar->value));
                        WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_VARIABLES key " + MameVar + " set to value:" + MameOptions);
                        data += Marshal.SizeOf<retro_variable_pointers>();
                        gvpSetVar = (retro_variable_pointers*)data;
                    } while (gvpSetVar->key != null);
                    WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_VARIABLES END");
                }
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY {ConfigManager.SystemDir}");
                if (data != IntPtr.Zero) {
                    //even in C this is obscure.
                    *(char**)data = (char *)PtrVault.GetPtr(ConfigManager.SystemDir);
                }
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY {ConfigManager.GameSaveDir}");
                // https://www.quora.com/Do-arcades-ever-reset-high-scores-on-their-machines
                //  most coin-operated video games and pinball machines have the option of being adjusted to reset the high scores after a certain number of plays or after a certain amount of time.
                if (data != IntPtr.Zero) {
                    //even in C this is obscure.
                    *(char**)data = (char *)PtrVault.GetPtr(ConfigManager.GameSaveDir);
                }
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS:
            {
                WriteConsole("[environmentCB] RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS  ");
                if (data != IntPtr.Zero) {
                    retro_input_descriptor inputDesc = (retro_input_descriptor)Marshal.PtrToStructure(data, typeof(retro_input_descriptor));
                    while (inputDesc.id != 0) {
                        WriteConsole("[environmentCB] RETRO_ENVIRONMENT_SET_INPUT_DESCRIPTORS  port: " + inputDesc.port + " device: " + inputDesc.device + " index: " + inputDesc.index + " id: " + inputDesc.id + " " + inputDesc.description);
                        data += Marshal.SizeOf(inputDesc);
                        Marshal.PtrToStructure(data, inputDesc);
                    }
                }
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_CONTROLLER_INFO:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_CONTROLLER_INFO  ");
                /*
                if (data == IntPtr.Zero) {
                    return false;
                }
                retro_controller_info controllerInfo = (retro_controller_info)Marshal.PtrToStructure<retro_controller_info>(data);
                IntPtr typesPtr = controllerInfo.types;
                retro_controller_description typesDesc = (retro_controller_description)Marshal.PtrToStructure<retro_controller_description>(typesPtr);
                while (typesDesc.id != 0) {
                    WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_CONTROLLER_INFO  description: {typesDesc.desc} id: {typesDesc.id}");
                    typesPtr += Marshal.SizeOf<retro_controller_description>();
                    typesDesc = (retro_controller_description)Marshal.PtrToStructure<retro_controller_description>(typesPtr);
                }
                */
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_PIXEL_FORMAT");
                if (data == IntPtr.Zero)
                    return false;

                pixelFormat = (retro_pixel_format)Marshal.ReadInt32(data);
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_PIXEL_FORMAT pixelformat: " + pixelFormat);
                if (! acceptedPixelFormats.Contains(pixelFormat)) {
                    WriteConsole("[LibRetroMameCore.environmentCB] ERROR == pixel format not supported ==" );
                    return false;
                }
                return true;
            }

            case envCmds.RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK:
            {
                //https://github.com/libretro/RetroArch/blob/37c56d0d09a1d455353c14a3e0860b7834f9c4b8/runloop.c#L2382
                if (data == IntPtr.Zero) {
                    WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK disabled - core don't specify data structure");
                    return true;
                }
                IntPtr cb = ((retro_audio_buffer_status_callback*)data)->callback;
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK AudioBufferStatusInfo function pointer {cb}");
                if (cb != IntPtr.Zero) {
                    AudioBufferStatusInfo = Marshal.GetDelegateForFunctionPointer<AudiobufferStatustHandler>(cb);
                }
                else {
                    WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_AUDIO_BUFFER_STATUS_CALLBACK function pointer not specified");
                    AudioBufferStatusInfo = null;
                }

                //Notifies a libretro core of the current occupancy level of the frontend audio buffer.
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE:
            {
                //calls every frame or so.
                //to tell to the core that a variable change (by the user)
                // WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_VARIABLE_UPDATE (not implemented)");
                //Marshal.WriteByte(data, 0, Convert.ToByte(0));
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY (not implemented)");
                // WriteConsole(String.Format("[LibRetroMameCore.environmentCB] NOT IMPLEMENTED - RETRO_ENVIRONMENT_SET_MINIMUM_AUDIO_LATENCY: {0}", (uint)Marshal.ReadInt32(data)));
                return false;
            }
            
            //not in Mame2003+
            case envCmds.RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_INPUT_DEVICE_CAPABILITIES only RETRO_DEVICE_JOYPAD");
                if (data == IntPtr.Zero)
                    return false;
                ulong mask = 1 << (int)RETRO_DEVICE_JOYPAD;
                *(ulong*)data = mask ;
                return true;
            }

            // case envCmds.RETRO_ENVIRONMENT_GET_INPUT_BITMASKS:
            //     // https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L603
            //     bool AcceptBitmaps = true;
            //     WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_INPUT_BITMASKS: {AcceptBitmaps}");
            //     if (data == IntPtr.Zero) {
            //         return false;
            //     }
            //     if (data != IntPtr.Zero) {
            //         WriteConsole(String.Format("[LibRetroMameCore.environmentCB] setting pointer"));
            //         *(bool*)data = AcceptBitmaps;
            //     }                
            //     return AcceptBitmaps;
            
            
            case envCmds.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_LOG_INTERFACE");
                if (data == IntPtr.Zero)
                    return false;
                ((retro_log_callback*)data)->log = Marshal.GetFunctionPointerForDelegate(new logHandler(MamePrintf));
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL:
            {
                //as of June 2021, the libretro performance profile callback is not known
                // * to be implemented by any frontends including RetroArch
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_PERFORMANCE_LEVEL (not implemented)");
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_VFS_INTERFACE:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_VFS_INTERFACE (not implemented, relay on M2003 own implimentation)");
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_LED_INTERFACE:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_LED_INTERFACE (not implemented)");
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_MESSAGE:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_MESSAGE (not implemented)");
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_ROTATION:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_ROTATION");
                if (data != IntPtr.Zero) {
                    //Message from MAME: This port of RetroArch does not support rotation or it has been disabled. Mame will rotate internally
                    uint rotation = *(uint*)data;
                    WriteConsole($"[LibRetroMameCore.environmentCB] please set the screen with rotation {rotation} in the Unity scene to avoid rotations in CPU.");
                }
                return true;
            }
            case envCmds.RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION:
            {
                WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION (not implemented)");
                return false;
            }
            case envCmds.RETRO_ENVIRONMENT_SET_GEOMETRY:
            {
                if (data != IntPtr.Zero) {
                    WriteConsole($"[LibRetroMameCore.environmentCB] RETRO_ENVIRONMENT_SET_GEOMETRY");
                    GameAVInfo.geometry = (retro_game_geometry)Marshal.PtrToStructure<retro_game_geometry>(data);
                    WriteConsole($"[LibRetroMameCore.environmentCB] {GameAVInfo.ToString()}");
                    return true;
                }

                return false;
            }
            default:
            {
                WriteConsole("[LibRetroMameCore.environmentCB] Unknown cmd " + cmd);
                return false;
            }
        }

        return true;
    }

    private static void CopyImageData0RGB1555toRGB565(IntPtr imageData, int width, int height, int pitch, Texture2D texture)
    {
        // Get the pointer to the image data as a byte array
        byte[] data = new byte[pitch * height];
        Marshal.Copy(imageData, data, 0, data.Length);

        // Lock the texture data and set the pixels directly
        Color[] pixels = texture.GetPixels();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Get the 0RGB1555 pixel value
                ushort pixel = BitConverter.ToUInt16(data, y * pitch + x * 2);

                // Extract the 0RGB components from the pixel
                byte r = (byte)((pixel >> 10) & 0x1F);
                byte g = (byte)((pixel >> 5) & 0x1F);
                byte b = (byte)(pixel & 0x1F);

                // Convert the 0RGB values to RGB565 format
                ushort newPixel = (ushort)((r << 11) | (g << 6) | b);

                // Set the pixel in the texture
                pixels[y * (int)width + x] = new Color((float)r / 31.0f, (float)g / 31.0f, (float)b / 31.0f);
            }
        }

        // Apply the changes to the texture
        texture.SetPixels(pixels);
        texture.Apply();
    }

    [AOT.MonoPInvokeCallback (typeof(videoRefreshHandler))]
    static void videoRefreshCB(IntPtr data, uint width, uint height, uint pitch) {
        // WriteConsole($"[LibRetroMameCore.videoRefreshCB] w: {width}  h: {height} pitch: {pitch} fmt: {pixelFormat}");
        if (data == IntPtr.Zero) {
            //https://github.com/libretro/mame2000-libretro/blob/6d0b1e1fe287d6d8536b53a4840e7d152f86b34b/src/libretro/libretro.c#L699
            return;
        }

        //just one pixel format is recognized in the actual implemetation. Conversion to other formats are expensive in C#
        if (! acceptedPixelFormats.Contains(pixelFormat)) 
            return;

#if _debug_fps_
        Profiling.video.Start();
#endif
        if (GameTexture == null) {
            WriteConsole($"[LibRetroMameCore.videoRefreshCB] create new texture w: {width}  h: {height} pitch: {pitch} fmt: {pixelFormat}");
            // https://docs.unity3d.com/ScriptReference/TextureFormat.html
            switch (pixelFormat) {
                case retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555:
                case retro_pixel_format.RETRO_PIXEL_FORMAT_RGB565:
                    GameTexture = new Texture2D((int)width, (int)height, TextureFormat.RGB565, false);
                    GameTexture.filterMode = FilterMode.Point;
                    break;
                case retro_pixel_format.RETRO_PIXEL_FORMAT_XRGB8888:
                    GameTexture = new Texture2D((int)width, (int)height, TextureFormat.BGRA32, false);
                    GameTexture.filterMode = FilterMode.Point;
                    break;
            }
            Display.materials[1].SetTexture("_MainTex", GameTexture);
        }

        if (pixelFormat == retro_pixel_format.RETRO_PIXEL_FORMAT_0RGB1555) 
        {
            CopyImageData0RGB1555toRGB565(data, (int)width, (int)height, (int)pitch, GameTexture);
        }
        else 
        {
            GameTexture.LoadRawTextureData(data, Mathf.RoundToInt(height*pitch));
            GameTexture.Apply(false, false);
        }

#if _debug_fps_
        Profiling.video.Stop();
#endif
        
        return;
    }

    [AOT.MonoPInvokeCallback (typeof(inputPollHander))]
    static void inputPollCB()
    {
        //WriteConsole("[inputPollCB] ");
        return;
    }

    // https://github.com/RetroPie/RetroPie-Docs/blob/219c93ca6a81309eed937bb5b7a79b8c71add41b/docs/RetroArch-Configuration.md
    // https://docs.libretro.com/library/mame2003_plus/#default-retropad-layouts
    [AOT.MonoPInvokeCallback (typeof(inputStateHandler))]
    static Int16 inputStateCB(UInt32 port, UInt32 device, UInt32 index, UInt32 id) {
        Int16 ret = 0;

        if (!WaitToFinishedGameLoad.Finished())
          return ret;

        //WriteConsole($"[inputStateCB] dev {device} port {port} index:{index} id:{id}");

        if (port != 0)
          return ret;


#if _debug_fps_
        Profiling.input.Start();
#endif
        //port: 0 device: 1 index: 0 id: 2 (select) Coin

//        if (id == RETRO_DEVICE_ID_JOYPAD_SELECT)
//        {
//            WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_JOYPAD_SELECT: dev {device} port {port}");
//            ret = (CoinSlot != null && CoinSlot.takeCoin()) ? (Int16)1:(Int16)0;
//            return ret;
//        }

        else if (device == RETRO_DEVICE_JOYPAD) {
            switch (id) {
                case RETRO_DEVICE_ID_JOYPAD_B:
                    ret = OVRInput.Get(OVRInput.RawButton.B) || OVRInput.Get(OVRInput.RawButton.RIndexTrigger)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_A:
                    ret =  OVRInput.Get(OVRInput.RawButton.A)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_X:
                    ret =  OVRInput.Get(OVRInput.RawButton.X)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_Y:
                    ret =  OVRInput.Get(OVRInput.RawButton.Y)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_UP:
                    ret =  OVRInput.Get(OVRInput.RawButton.LThumbstickUp)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_DOWN:
                    ret =  OVRInput.Get(OVRInput.RawButton.LThumbstickDown)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_RIGHT:
                    ret =  OVRInput.Get(OVRInput.RawButton.LThumbstickRight)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_LEFT:
                    ret =  OVRInput.Get(OVRInput.RawButton.LThumbstickLeft)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_SELECT:                    
                    //WriteConsole($"[inputStateCB] RETRO_DEVICE_ID_JOYPAD_SELECT: {CoinSlot.ToString()}");
                    ret = (CoinSlot != null && CoinSlot.takeCoin()) ? (Int16)1:(Int16)0;
                    if (ret == 1)
                        HotDelaySelectCycles = 5;
                    if (HotDelaySelectCycles > 0 && ret != (Int16)1) {
                        HotDelaySelectCycles--;
                        ret = (Int16)1;
                    }

                    break;
                case RETRO_DEVICE_ID_JOYPAD_START:
                    ret =  OVRInput.Get(OVRInput.RawButton.Start)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_L:
                    ret =  OVRInput.Get(OVRInput.RawButton.LIndexTrigger)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_R:
                    ret =  OVRInput.Get(OVRInput.RawButton.RIndexTrigger)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_L2:
                    ret =  OVRInput.Get(OVRInput.RawButton.LHandTrigger)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_R2:
                    ret =  OVRInput.Get(OVRInput.RawButton.RHandTrigger)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_L3:
                    ret =  OVRInput.Get(OVRInput.RawButton.LThumbstick)? (Int16)1:(Int16)0;
                    break;
                case RETRO_DEVICE_ID_JOYPAD_R3:
                    ret =  OVRInput.Get(OVRInput.RawButton.RThumbstick)? (Int16)1:(Int16)0;
                    break;            
                }
        }
            /*
        else if (device == RETRO_DEVICE_ANALOG) {
          Vector2 thumbstickPosition = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

          switch (id) {
            case RETRO_DEVICE_ID_ANALOG_X:
              //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
              ret =  (Int16)(thumbstickPosition.x * 32768); // X-coordinate of the thumbstick position
              break;
            case RETRO_DEVICE_ID_ANALOG_Y:
              ret =  (Int16)(thumbstickPosition.y * 32768); // y-coordinate of the thumbstick position
              break;
          }
          WriteConsole($"[inputStateCB] ANALOG port: {port} device: {device} index: {index} id: {id} ret: {ret}");
        }*/
        else if (device == RETRO_DEVICE_MOUSE) {
          Vector2 thumbstickPosition = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);

          switch (id) {
            case RETRO_DEVICE_ID_MOUSE_X:
              //left-to-right movement, range of [-0x7fff, 0x7fff], -32768 to 32767
              if (thumbstickPosition.x > 0)
                ret = (Int16)10;
              else if (thumbstickPosition.x < 0)
                ret = (Int16)(-10);
              break;
            case RETRO_DEVICE_ID_MOUSE_Y:
              if (thumbstickPosition.y > 0)
                ret = (Int16)10;
              else if (thumbstickPosition.y < 0)
                ret = (Int16)(-10);
              break;
            case RETRO_DEVICE_ID_MOUSE_LEFT:
              ret = OVRInput.Get(OVRInput.RawButton.B)? (Int16)1:(Int16)0;
              break;
            case RETRO_DEVICE_ID_MOUSE_RIGHT:
              ret =  OVRInput.Get(OVRInput.RawButton.A)? (Int16)1:(Int16)0;
              break;
          }
          //WriteConsole($"[inputStateCB] MOUSE port: {port} device: {device} index: {index} id: {id} ret: {ret}");
        }
        /*
        Mame2003+ didn't use buttons masks.
        static Func<OVRInput.RawButton, uint, Int16> TransBits = (Btn, retroId) => (Int16)(OVRInput.Get(Btn)? (1 << (Int16)retroId) : 0);
         WriteConsole($"[LibRetroMameCore.inputStateCB] id {id} | device {device} | port {port}");
        if (port == 0 && device == RETRO_DEVICE_JOYPAD && id == RETRO_DEVICE_ID_JOYPAD_MASK) {

            Int16 bits = 0;
            bits |= TransBits(OVRInput.RawButton.B, RETRO_DEVICE_ID_JOYPAD_B);
            bits |= TransBits(OVRInput.RawButton.A, RETRO_DEVICE_ID_JOYPAD_A);
            bits |= TransBits(OVRInput.RawButton.X, RETRO_DEVICE_ID_JOYPAD_X);
            bits |= TransBits(OVRInput.RawButton.Y, RETRO_DEVICE_ID_JOYPAD_Y);
            bits |= TransBits(OVRInput.RawButton.LThumbstickUp, RETRO_DEVICE_ID_JOYPAD_UP);
            bits |= TransBits(OVRInput.RawButton.LThumbstickDown, RETRO_DEVICE_ID_JOYPAD_DOWN);
            bits |= TransBits(OVRInput.RawButton.LThumbstickLeft, RETRO_DEVICE_ID_JOYPAD_LEFT);
            bits |= TransBits(OVRInput.RawButton.LThumbstickRight, RETRO_DEVICE_ID_JOYPAD_RIGHT);
            bits |= TransBits(OVRInput.RawButton.Start, RETRO_DEVICE_ID_JOYPAD_START);
            bits |= TransBits(OVRInput.RawButton.LIndexTrigger, RETRO_DEVICE_ID_JOYPAD_SELECT);

            WriteConsole($"[LibRetroMameCore.inputStateCB] RETRO_DEVICE_ID_JOYPAD_MASK returns {bits}");
            return bits;
        }
        */

#if _debug_fps_
        Profiling.input.Stop();
#endif
        //WriteConsole($"[inputStateCB] port: {port} device: {device} index: {index} id: {id} ret: {ret}");

        return ret;
    }
  
    [AOT.MonoPInvokeCallback (typeof(audioSampleHandler))]
    static void audioSampleCB(Int16 left, Int16 right) {
        WriteConsole("[LibRetroMameCore.audioSampleCB] left: " + left + " right: " + right);
        return;
    }

    /*
    * One frame is defined as a sample of left and right channels, interleaved.
    * I.e. int16_t buf[4] = { l, r, l, r }; would be 2 frames.
    * Only one of the audio callbacks must ever be used.
    */
    
    /*
    [AOT.MonoPInvokeCallback (typeof(audioSampleBatchHandler))]
    static ulong audioSampleBatchCB(short* data, ulong frames) {

        if (data == (short*)IntPtr.Zero) {
            return 0;
        }

#if _debug_audio_
        WriteConsole($"[LibRetroMameCore.audioSampleBatchCB] AUDIO IN from MAME - frames:{frames} batch actual load: {AudioBatch.Count}");
#endif
        if (AudioBatch.Count > AudioBufferMaxOccupancy) {
            //overrun
            WriteConsole($"[LibRetroMameCore.audioSampleBatchCB] AUDIO IN OVERRUN");
            return 0;
        }
#if _debug_fps_
        Profiling.audio.Start();
#endif
        
        for (ulong i = 0; i < frames*2; ++i) {
            //float value = Mathf.Clamp(data[i]  / 32768f, -1.0f, 1.0f); // 0.000030517578125f o 0.00048828125; //to convert from 16 to fp 
            float value = data[i] * 0.000030517578125f; 
            AudioBatch.Add(value);
        }

#if _debug_fps_
        Profiling.audio.Stop();
#endif
        return frames;
    }
    * */

    
    
    [AOT.MonoPInvokeCallback (typeof(audioSampleBatchHandler))]
    static ulong audioSampleBatchCB(short* data, ulong frames) {
        //WriteConsole($"[LibRetroMameCore.audioSampleBatchCB] AUDIO IN from MAME - frames:{frames} batch actual load: {AudioBatch.Count}");
        
        if (data == (short*)IntPtr.Zero) {
            return 0;
        }

        if (AudioBatch.Count > AudioBufferMaxOccupancy) {
            //overrun
            return 0;
        }

#if _debug_fps_
        Profiling.audio.Start();
#endif

        var inBuffer = new List<float>();
        for (ulong i = 0; i < frames*2; ++i) {
            // float value = Mathf.Clamp(data[i]  / 32768f, -1.0f, 1.0f); // 0.000030517578125f o 0.00048828125; //to convert from 16 to fp 
            float value = data[i] / 32768f; 
            inBuffer.Add(value);
        }
        
        double ratio = (double) GameAVInfo.timing.sample_rate / QuestAudioFrequency;
        int outSample = 0;
        while (true) {
            int inBufferIndex = (int)(outSample++ * ratio);
            if (inBufferIndex < (int)frames*2)
                AudioBatch.Add(inBuffer[inBufferIndex]);
            else
                break;
        }

#if _debug_fps_
        Profiling.audio.Stop();
#endif
        return frames;
    }
    
    
    
    // public static void OnAudioRead(float[] data) {
    //     if (AudioBatch == null || AudioBatch.Count == 0) {
    //         return;
    //     }
    //     int toCopy = AudioBatch.Count >= data.Length? data.Length : AudioBatch.Count;   
    //     WriteConsole($"[LibRetroMameCore.OnAudioRead] AUDIO OUT output buffer length: {data.Length} frames loaded from MAME: {AudioBatch.Count} toCopy: {toCopy} ");
    //     if (toCopy > 0) {
    //         AudioBatch.CopyTo(0, data, 0, toCopy);
    //         AudioBatch.RemoveRange(0, toCopy);
    //     }
    // }

    public static void MoveAudioStreamTo(string _gameFileName, float[] data) {
        if (!GameLoaded || GameFileName != _gameFileName) {
            //It is neccesary to call Run() method for the game in the parameter?
            return;
        }
        if (AudioBatch == null || AudioBatch.Count == 0) {
            return;
        }

        int toCopy = AudioBatch.Count >= data.Length? data.Length : AudioBatch.Count;   
        
        if (toCopy > 0) {
            AudioBatch.CopyTo(0, data, 0, toCopy);
            AudioBatch.RemoveRange(0, toCopy);
        }
#if _debug_audio_
        WriteConsole($"[LibRetroMameCore.LoadAudio] AUDIO OUT output buffer length: {data.Length} frames loaded from MAME: {AudioBatch.Count} toCopy: {toCopy} ");
#endif

    }

#region serialization
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

#endregion

    /*
    public static void onAudioChangePosition(uint newPos) {
        AudioBufferPosition = newPos;
1265    */

    private static void getAVGameInfo() {
        MarshalHelpCalls<retro_system_av_info> m = new();
        WriteConsole("[LibRetroMameCore.getAVGameInfo] retro_get_system_av_info: ");
        retro_get_system_av_info(m.PtrFrom(new retro_system_av_info()));
        GameAVInfo = m.GetObjectAndFree(GameAVInfo);
        WriteConsole(GameAVInfo.ToString());
    }

    private static void GetSystemInfo() {
        MarshalHelpCalls<retro_system_info> m = new();
        WriteConsole("[LibRetroMameCore.GetSystemInfo] retro_get_system_info: ");
        SystemInfo = new retro_system_info();
        retro_get_system_info(m.PtrFrom(SystemInfo));
        m.GetObjectAndFree(SystemInfo);
        WriteConsole(SystemInfo.ToString());
    }
    
     //storage pointers to unmanaged memory
    public class MarshalHelpPtrVault {
        private List<IntPtr> vault = new();

        public IntPtr GetPtr(string str) {
            
            IntPtr p = Marshal.StringToHGlobalAnsi(str);
            if (p == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
            vault.Add(p);
            return p;
        }
         public IntPtr GetPtr(bool b) {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf<bool>());
            if (p == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
            Marshal.WriteByte(p, b? (byte)1: (byte)0);

            vault.Add(p);
            return p;
        }
        public void Free() {
            foreach(IntPtr p in vault) {
                Marshal.FreeHGlobal(p);
            }
            vault = new List<IntPtr>();
        }
        ~MarshalHelpPtrVault() {
            Free();
        }
    }

    //helper to convert structs to pointers used call functions.
    public class MarshalHelpCalls<T> {
        IntPtr _p = IntPtr.Zero;
        // T _obj;
        //no ref, out or whatever works well marshaling.
        public MarshalHelpCalls() {
            // _obj = obj;
            _p = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
            if (_p == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
        }
        public IntPtr Ptr {
            get {
                return _p;
            }
        }
        /*
        public T Obj {
            get {
                return _obj;
            }
        }
        */
        public void free() {
            if (_p != IntPtr.Zero) {
                // Marshal.DestroyStructure(_p, typeof(T));
                Marshal.FreeHGlobal(_p);
                _p = IntPtr.Zero;
            }
        }
        public IntPtr PtrFrom(T _obj) {
            Marshal.StructureToPtr(_obj, _p, false);
            return _p;
        }
        /*
        public MarshalHelpCalls<T> Alloc() {
            free();
            
            if (_p == IntPtr.Zero) {
                throw new OutOfMemoryException();
            }
            Marshal.StructureToPtr(_obj, _p, false);
            return this;
        }
        */
        public T GetObjectAndFree(T _obj) {
            if (_p == IntPtr.Zero) {
                throw new InvalidOperationException("Ptr is empty, call Alloc() first");
            }
            //load the object with the data of the unmanaged memory and free the pointer.
            Marshal.PtrToStructure(_p, _obj);
            free();
            return _obj;
        }
        
        ~MarshalHelpCalls() {
            free();
        }
    }

   public class FpsControl {
        float timeBalance = 0;
        float timePerFrame = 0;
        uint frameCount = 0;
        float acumTime = 0;

        public FpsControl(float FPSExpected) {
            timeBalance = 0;
            frameCount = 0;
            timePerFrame = 1f / FPSExpected;
        }
        public void CountTimeFrame() {
            timeBalance += Time.deltaTime;
            acumTime += Time.deltaTime;
            frameCount++;
        }
        public void Reset() {
            timeBalance = 0;
            frameCount = 0;
            acumTime = 0;
        }
        public float fps() {
            return frameCount / acumTime;
        }
        public bool isTime() {
            //deltaTime: time from the last call
            if (timeBalance >= timePerFrame) {
                timeBalance -= timePerFrame;
                return true;
            }
            return false;
        }
        public float DelayedFrames() {
            return timeBalance/timePerFrame;
        }
        public override string ToString() {
            return $"timePerFrame: {timePerFrame} delayed frames: {DelayedFrames()} fps: {fps()} frames total: {frameCount}";
        }
    }

    [Conditional("_debug_")]
    public static void WriteConsole(string st) {
        UnityEngine.Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"[{GameFileName}] - {st}");
    }

    // button simulation
    class ButtonDone {
        public bool done = false;
        DateTime started = new DateTime();
        //some machines check the time to protect himself from cheaters.
        // https://docs.mamedev.org/usingmame/commonissues.html?highlight=insert%20coin#why-does-my-game-show-an-error-screen-if-i-insert-coins-rapidly
        double WaitSecs = 0.5;
        public uint id = 0;
        public ButtonDone(uint _id) {
            this.id = _id;
        }
        public bool Pushed() {
            if (! done) {
                if (started == new DateTime()) {
                    started = DateTime.Now;
                }
                done = started.AddSeconds(WaitSecs) < DateTime.Now;
            }
            return done;
        }
    }
    public class Waiter {
        public bool _finished = false;
        DateTime _started = DateTime.MaxValue;
        int _waitSecs = 0;
        
        public Waiter(int _pwaitSecs) {
            _waitSecs = _pwaitSecs;
        }
        public int WaitSecs {
            get {
                return _waitSecs;
            }
        }
        public void reset() {
            _started = DateTime.MaxValue;
            _finished = false;
        }
        public bool Finished() {
            if (!_finished) {
                if (_started == DateTime.MaxValue) {
                    _started = DateTime.Now;
                }
                _finished = _started.AddSeconds(_waitSecs) < DateTime.Now;
            }
            return _finished;
        }
    }
#if _debug_fps_
    public class StopWatches {
        public Stopwatch audio = new();
        public Stopwatch video = new();
        public Stopwatch input = new();
        public Stopwatch retroRun = new();

        public StopWatches() {
            audio = new();
            video = new();
            input = new();
            retroRun = new();
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
