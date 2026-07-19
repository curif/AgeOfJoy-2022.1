/* 
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

//#define _debug_fps_
//#define _debug_audio_ // capture ~15s of game audio to a WAV on game start, see FlushAudioCaptureIfReady()
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

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_audio_set_output_rate(double rate);


    static object AudioBufferLock = new();
    static int QuestAudioFrequency = 48000; //Quest 2 standar, can change at start

    // Rate the ring-buffer content actually carries (frames/s). Unity's real audio-thread
    // drain rate is not a fixed property of the device: it has been measured anywhere from
    // ~84% of nominal (a ~4us/frame tax, some builds) to exactly nominal (other builds) on
    // the same hardware - it moves with Unity/Meta SDK/OS changes outside our control. So
    // this is NOT a one-shot calibration: every stats window (GetAndResetAudioStats) measures
    // real consumer demand and steers wrapperOutputRate toward it in closed loop, in both
    // directions. PlayerPrefs only seeds the first window after a fresh install/session so
    // playback isn't wrong for the first few seconds; see docs/libretro_audio_system.md.
    const string AudioCalibrationPrefKey = "AudioCalibratedOutputRate";
    static double wrapperOutputRate = 48000;
    // 0 = not yet initialized (first valid window snaps to measured demand instead of EMA-blending)
    static double smoothedDemandRate = 0;

    // audio stats (all fields touched only under AudioBufferLock, except wrapperRuns
    // which is incremented from the run thread via Interlocked)
    static long audioCallbacks = 0;
    static long audioUnderruns = 0;
    static long audioMissingSamples = 0;
    static long audioCopiedFloats = 0;
    static int audioMinOccupancy = int.MaxValue;
    static int audioMaxOccupancy = 0;
    static long wrapperRuns = 0;
    static float expectedFps = 0;
    static DateTime audioStatsSince = DateTime.MinValue;
    // gap detection: if the DSP suspends our filter (voice virtualized / effect
    // bypassed while "silent"), callbacks pause while the wrapper keeps producing
    // and the ring buffer overflows. A gap is a callback arriving much later than
    // the buffer duration it delivers.
    static long audioLastCallbackTimestamp = 0;
    static long audioGaps = 0;
    static float audioMaxGapMs = 0;
    // lock contention: how long the Unity audio thread waits to acquire
    // AudioBufferLock while the native wrapper (run thread) holds it. Long waits
    // stall the DSP mixer itself - the OS output underruns and the whole audio
    // timeline loses blocks. Misses are callbacks that gave up waiting.
    static float audioMaxLockWaitMs = 0;
    static float audioTotalLockWaitMs = 0;
    static long audioLockMisses = 0;
    // Observed capacity of the wrapper's ring buffer (floats). When occupancy reaches
    // it, the native side is about to drop samples uncontrolled; we recenter to half
    // so scheduling jitter has headroom in both directions. If a wrapper build uses a
    // different capacity the check simply never triggers (harmless).
    const int AudioRingCapacityFloats = 8192;
    static long audioRecenters = 0;
    static long audioDiscardedFloats = 0;
    // occupancy at the first and last callback of the stats interval, to derive the
    // true production rate: produced = consumed + discarded + (endOcc - startOcc)
    static int audioIntervalStartOccupancy = -1;
    static int audioIntervalEndOccupancy = 0;

#if _debug_audio_
    // Debug capture: records exactly what MoveAudioStreamTo hands to Unity so the
    // stream can be pulled off the device and listened to in isolation. If the WAV
    // fries, the wrapper/core side is producing bad samples; if it's clean, the
    // corruption happens after our filter (FMOD mixer / spatializer).
    // Buffer fields are touched only under AudioBufferLock; flushing happens on the
    // main thread via FlushAudioCaptureIfReady().
    const int AudioCaptureSeconds = 15;
    static float[] audioCaptureBuffer = null;
    static int audioCapturePos = 0;
    static bool audioCaptureDone = false;
#endif

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

    // Native callback delegates must be kept alive for as long as native code may call
    // them, otherwise the GC can collect them while a P/Invoke still holds the function
    // pointer (random native crash). Root them here instead of passing `new Handler(...)`
    // inline at the call site.
    static wrapperLogHandler wrapperLogDelegate;
    static inputStateHandler inputStateDelegate;
    static EnvironmentHandler environmentDelegate;
    static CreateTextureHandler createTextureDelegate;
    static TextureLockHandler textureLockDelegate;
    static TextureUnlockHandler textureUnlockDelegate;
    static TextureBufferSemAvailableHandler textureBufferSemAvailableDelegate;
    static AudioLockHandler audioLockDelegate;
    static AudioUnlockHandler audioUnlockDelegate;

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
    //private static int lastCoordX;
    // private static int lastCoordY;
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
    private static extern int wrapper_read_memory_map(uint address);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern int wrapper_get_led_state(int led);

    [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
    private static extern void wrapper_led_reset();

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

    /// <summary>
    /// Logs the ACTUAL Android audio device properties (not what Unity was configured
    /// for): the hardware output sample rate and the native burst size. Ground truth
    /// for diagnosing rate mismatches between Unity's DSP and the OS stream.
    /// </summary>
    private static void LogAndroidAudioProperties()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var audioManager = activity.Call<AndroidJavaObject>("getSystemService", "audio"))
            {
                string deviceRate = audioManager.Call<string>("getProperty", "android.media.property.OUTPUT_SAMPLE_RATE");
                string burstFrames = audioManager.Call<string>("getProperty", "android.media.property.OUTPUT_FRAMES_PER_BUFFER");
                ConfigManager.WriteConsole($"[LibRetroMameCore] ANDROID AUDIO DEVICE outputSampleRate: {deviceRate} framesPerBuffer: {burstFrames}");
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleError($"[LibRetroMameCore] ANDROID AUDIO DEVICE query failed: {e.Message}");
        }
#endif
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
        LogAndroidAudioProperties();

        // Warm start only: the closed-loop controller in GetAndResetAudioStats() takes
        // over and corrects this within the first few stats windows regardless of
        // whether this guess is right. Range allows up to nominal (tax-free) rates -
        // a previous session may have measured no tax at all.
        wrapperOutputRate = PlayerPrefs.GetFloat(AudioCalibrationPrefKey, 0f);
        if (wrapperOutputRate < QuestAudioFrequency * 0.5 || wrapperOutputRate > QuestAudioFrequency * 1.05)
            wrapperOutputRate = QuestAudioFrequency;
        smoothedDemandRate = 0;
        ConfigManager.WriteConsole($"[LibRetroMameCore.Start] AUDIO wrapper output rate: {wrapperOutputRate:F0} (nominal {QuestAudioFrequency})");

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

        wrapperLogDelegate = new wrapperLogHandler(WrapperPrintf);
        inputStateDelegate = new inputStateHandler(inputStateCB);
        environmentDelegate = new EnvironmentHandler(EnvironmentHandlerCB);

        int result = wrapper_environment_open(wrapperLogDelegate,
                                                MinLogLevel,
                                                ConfigManager.GameSaveDir,
                                                ConfigManager.SystemDir,
                                                QuestAudioFrequency.ToString(),
                                                inputStateDelegate,
                                                core.Library,
                                                environmentDelegate
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
        createTextureDelegate = new CreateTextureHandler(CreateTextureCB);
        textureLockDelegate = new TextureLockHandler(TextureLockCB);
        textureUnlockDelegate = new TextureUnlockHandler(TextureUnlockCB);
        textureBufferSemAvailableDelegate = new TextureBufferSemAvailableHandler(TextureBufferSemAvailable);
        audioLockDelegate = new AudioLockHandler(AudioLockCB);
        audioUnlockDelegate = new AudioUnlockHandler(AudioUnlockCB);

        wrapper_image_init(createTextureDelegate,
                            textureLockDelegate,
                            textureUnlockDelegate,
                            textureBufferSemAvailableDelegate);
        wrapper_audio_init(audioLockDelegate,
                            audioUnlockDelegate);
        try
        {
            wrapper_audio_set_output_rate(wrapperOutputRate);
        }
        catch (EntryPointNotFoundException)
        {
            // wrapper built before the calibrated-output-rate change: it resamples to
            // its hardcoded 48000. Keep running with the legacy (overflowing) behavior.
            ConfigManager.WriteConsoleWarning("[LibRetroMameCore.Start] wrapper without wrapper_audio_set_output_rate, audio calibration inactive");
            wrapperOutputRate = QuestAudioFrequency;
        }
        wrapper_input_init();

        /* It's impossible to change the Sample Rate, fixed in 48000
        audioConfig.sampleRate = sampleRate;
        AudioSettings.Reset(audioConfig);
        audioConfig = AudioSettings.GetConfiguration();
        WriteConsole($"[LibRetroMameCore.Start] New audio Sample Rate:{audioConfig.sampleRate}");
        */
        // Highest priority (0): with all cabinets alive the scene has many playing
        // AudioSources competing for 32 real voices; if FMOD virtualizes this one,
        // OnAudioFilterRead stops firing while the wrapper keeps producing, the ring
        // buffer overflows and drops chunks (crackling + time-compressed audio).
        Speaker.priority = 0;
        Speaker.Play();

#if _debug_audio_
        lock (AudioBufferLock)
        {
            audioCaptureBuffer = new float[QuestAudioFrequency * 2 * AudioCaptureSeconds];
            audioCapturePos = 0;
            audioCaptureDone = false;
        }
        ConfigManager.WriteConsole($"[LibRetroMameCore.Start] audio capture armed: {AudioCaptureSeconds}s at {QuestAudioFrequency}Hz stereo");
#endif

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

    public static uint getMemorySize(uint region) => wrapper_get_memory_size(region);

    public const int MAX_LEDS = 8;

    /// <summary>
    /// Returns the current state of LED <paramref name="led"/> (0–7).
    /// Returns 0 (off), 1 (on), or -1 if index is out of range or game not loaded.
    /// </summary>
    public static int getLedState(int led)
    {
        if (!GameLoaded) return -1;
        return wrapper_get_led_state(led);
    }

    public static int getMemory(uint region, uint offset)
    {
        if (!GameLoaded)
            throw new Exception($"[getMemory] Can't operate on memory of non loaded games.");

        uint size = wrapper_get_memory_size(region);
        if (size == 0)
        {
            // Region not available via standard API — fall back to memory map descriptors
            int mapValue = wrapper_read_memory_map(offset);
            if (mapValue < 0)
                throw new Exception($"[getMemory] Address 0x{offset:X} not found in memory map (region {region} also unavailable).");
            return mapValue;
        }

        if (offset >= size)
        {
            ConfigManager.WriteConsole($"[getMemory] Offset 0x{offset:X} ({offset}) is out of bounds for region {region} (size={size}).");
            throw new Exception($"[getMemory] Offset {offset} is out of bounds for region {region} (size: {size}).");
        }
        char* data = wrapper_get_memory_data(region);
        if (data == null)
        {
            ConfigManager.WriteConsole($"[getMemory] Memory region {region} returned a null pointer.");
            throw new Exception($"[getMemory] Memory region {region} returned a null pointer.");
        }
        return (byte)data[offset];
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

        expectedFps = (float)GetFps();
        Interlocked.Exchange(ref wrapperRuns, 0);
        FPSControlNoUnity = new(expectedFps);

        retroRunTaskCancellationToken = new();
        // LongRunning gets this its own dedicated thread instead of a thread-pool worker,
        // and we raise its priority: previously this loop busy-spun at 100% of a core,
        // which the Android scheduler penalizes (preferred preemption victim). If it got
        // preempted while wrapper_audio holds AudioBufferLock mid-write, Unity's audio
        // thread stalled inside MoveAudioStreamTo waiting on the same lock - a glitch
        // even though the ring buffer had data. Sleeping when there's slack removes the
        // spin and the priority reduces how often this thread gets bumped.
        retroRunTask = Task.Factory.StartNew(() =>
        {
            Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            var fpsControl = FPSControlNoUnity;
            ConfigManager.WriteConsole($"[StartRunThread.retroRunTask]task start running IsCancellationRequested: {retroRunTaskCancellationToken.IsCancellationRequested} status: {retroRunTask.Status}");
            while (!retroRunTaskCancellationToken.IsCancellationRequested)
            {
                fpsControl.CountTimeFrame();
                if (fpsControl.isTime())
                {
                    // ConfigManager.WriteConsole($"[StartRunThread.retroRunTask] wrapper_run -------------------------");
                    wrapper_run();
                    Interlocked.Increment(ref wrapperRuns);
                    // ConfigManager.WriteConsole($"[retroRunTask] wrapper_run end IsCancellationRequested: {retroRunTaskCancellationToken.IsCancellationRequested} status: {retroRunTask.Status} -------------------------");
                    handleSpecialInputs();
                }
                else
                {
                    float secondsLeft = fpsControl.SecondsUntilNextFrame;
                    if (secondsLeft > 0.002f)
                        Thread.Sleep(1);
                    else
                        Thread.Yield();
                }
            }
        },
        retroRunTaskCancellationToken.Token,
        TaskCreationOptions.LongRunning,
        TaskScheduler.Default
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

        // save whatever was captured even if the 15s buffer didn't fill
        FlushAudioCaptureIfReady(force: true);

        SaveAudioCalibration();

        ClearAll();

        WriteConsole("[LibRetroMameCore.End] END  *************************************************");
    }

     /// <summary>Unload when the owning screen was destroyed before End() (e.g. MR DespawnAllAsync).</summary>
    public static void ForceEndActiveGame()
    {
        if (!GameLoaded && string.IsNullOrEmpty(GameFileName))
            return;

        if (!string.IsNullOrEmpty(GameFileName) && !string.IsNullOrEmpty(ScreenName))
        {
            WriteConsole($"[LibRetroMameCore.ForceEndActiveGame] {GameFileName} on {ScreenName}");
            End(ScreenName, GameFileName);
            return;
        }

        WriteConsole("[LibRetroMameCore.ForceEndActiveGame] clearing stale GameLoaded flag");
        ClearAll();
    }

    /// <summary>
    /// Persists the closed-loop controller's current rate as next session's warm start.
    /// Main thread only (PlayerPrefs).
    /// </summary>
    private static void SaveAudioCalibration()
    {
        double rate;
        lock (AudioBufferLock)
        {
            rate = smoothedDemandRate;
        }

        if (rate <= 0)
            return; // controller never got a valid window this session, keep previous warm start

        if (rate < QuestAudioFrequency * 0.5 || rate > QuestAudioFrequency * 1.05)
            return; // implausible, keep previous calibration

        PlayerPrefs.SetFloat(AudioCalibrationPrefKey, (float)rate);
        PlayerPrefs.Save();
        ConfigManager.WriteConsole($"[LibRetroMameCore] AUDIO calibration saved: {rate:F0} frames/s (nominal {QuestAudioFrequency})");
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

        // AudioBufferLock is intentionally never reassigned: it must stay the same
        // object for the process lifetime since native code can still be mid-callback
        // via the lock/unlock handlers around a ClearAll().
        lock (AudioBufferLock)
        {
            audioCallbacks = 0;
            audioUnderruns = 0;
            audioMissingSamples = 0;
            audioCopiedFloats = 0;
            audioMinOccupancy = int.MaxValue;
            audioMaxOccupancy = 0;
            audioGaps = 0;
            audioMaxGapMs = 0;
            audioLastCallbackTimestamp = 0;
            audioLockMisses = 0;
            audioMaxLockWaitMs = 0;
            audioTotalLockWaitMs = 0;
            audioRecenters = 0;
            audioDiscardedFloats = 0;
            audioIntervalStartOccupancy = -1;
            audioIntervalEndOccupancy = 0;
            audioStatsSince = DateTime.MinValue;
            smoothedDemandRate = 0;
        }
        Interlocked.Exchange(ref wrapperRuns, 0);

        if (Speaker != null && Speaker.isPlaying)
        {
            WriteConsole("[LibRetroMameCore.ClearAll] Pause Speaker");
            Speaker.Pause();
        }
        if (Speaker != null)
            Speaker.priority = 128; // restore default for attract-mode duty
        Speaker = null;

        GameFileName = "";
        ScreenName = "";
        GameLoaded = false;
#if !UNITY_EDITOR
        wrapper_led_reset();
#endif

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
        //lastCoordX = -0x7fff;   // Assume lightgun starting at top left
        // lastCoordY = -0x7fff;
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
    public static void MoveAudioStreamTo(float[] audioData, int channels)
    {
#if !UNITY_EDITOR
        // Never let the DSP mixer thread stall on the producer: if the wrapper (run
        // thread) holds AudioBufferLock too long, waiting here delays the whole audio
        // mix and the OS output underruns. Measure the wait; on timeout output one
        // silent block instead of blocking.
        long waitStart = Stopwatch.GetTimestamp();
        bool acquired = Monitor.TryEnter(AudioBufferLock, 4);
        float lockWaitMs = (Stopwatch.GetTimestamp() - waitStart) * 1000f / Stopwatch.Frequency;
        if (!acquired)
        {
            Array.Clear(audioData, 0, audioData.Length);
            // diagnostic-only fields; racing the lock holder here is acceptable
            audioLockMisses++;
            if (lockWaitMs > audioMaxLockWaitMs)
                audioMaxLockWaitMs = lockWaitMs;
            return;
        }
        try
        {
            audioTotalLockWaitMs += lockWaitMs;
            if (lockWaitMs > audioMaxLockWaitMs)
                audioMaxLockWaitMs = lockWaitMs;

            // Call the C functions to access the audio data
            IntPtr audioBufferPtr = wrapper_audio_get_audio_buffer_pointer();
            int audioBufferOccupancy = wrapper_audio_get_audio_buffer_occupancy();

            // Ring full (typically after the silent game boot fills it): skip ahead to
            // half occupancy in one controlled jump. One audible seam now instead of a
            // continuous stream of tiny uncontrolled drops at every scheduling hiccup.
            if (audioBufferOccupancy >= AudioRingCapacityFloats)
            {
                int discard = audioBufferOccupancy / 2;
                if (channels > 0)
                    discard -= discard % channels;
                wrapper_audio_consume_buffer(discard);
                audioBufferOccupancy -= discard;
                audioBufferPtr = wrapper_audio_get_audio_buffer_pointer();
                audioRecenters++;
                audioDiscardedFloats += discard;
            }

            if (audioIntervalStartOccupancy < 0)
                audioIntervalStartOccupancy = audioBufferOccupancy;
            audioIntervalEndOccupancy = audioBufferOccupancy;

            int toCopy = audioBufferOccupancy >= audioData.Length ? audioData.Length : audioBufferOccupancy;

            // Never split a stereo/multi-channel frame across the copy boundary:
            // an odd toCopy would permanently shift the L/R interleave from here on.
            if (channels > 0)
                toCopy -= toCopy % channels;

            audioCallbacks++;
            audioCopiedFloats += toCopy;
            if (audioBufferOccupancy < audioMinOccupancy)
                audioMinOccupancy = audioBufferOccupancy;
            if (audioBufferOccupancy > audioMaxOccupancy)
                audioMaxOccupancy = audioBufferOccupancy;
            if (toCopy < audioData.Length)
            {
                audioUnderruns++;
                audioMissingSamples += audioData.Length - toCopy;
            }

            long nowTs = Stopwatch.GetTimestamp();
            if (audioLastCallbackTimestamp != 0)
            {
                float deltaMs = (nowTs - audioLastCallbackTimestamp) * 1000f / Stopwatch.Frequency;
                // nominal spacing is the buffer duration; 1.5x tolerates jitter
                float nominalMs = (audioData.Length / 2f) / QuestAudioFrequency * 1000f;
                if (deltaMs > nominalMs * 1.5f)
                    audioGaps++;
                if (deltaMs > audioMaxGapMs)
                    audioMaxGapMs = deltaMs;
            }
            audioLastCallbackTimestamp = nowTs;

            if (toCopy > 0)
            {
                // Convert the IntPtr to a float array
                Marshal.Copy(audioBufferPtr, audioData, 0, toCopy);

                // Consume the data in the C buffer
                wrapper_audio_consume_buffer(toCopy);
            }

            // On underrun, zero the unfilled tail instead of leaving stale samples in
            // place: a waveform that jumps mid-cycle is an audible pop, dozens of which
            // per second is what "frying" sounds like. Silence is far less objectionable.
            if (toCopy < audioData.Length)
                Array.Clear(audioData, toCopy, audioData.Length - toCopy);

#if _debug_audio_
            if (audioCaptureBuffer != null && !audioCaptureDone)
            {
                // Don't waste the capture window on the (long, silent) game boot:
                // recording starts at the first audible sample.
                bool startCapture = audioCapturePos > 0;
                if (!startCapture)
                {
                    for (int i = 0; i < audioData.Length; i++)
                    {
                        if (audioData[i] > 0.01f || audioData[i] < -0.01f)
                        {
                            startCapture = true;
                            break;
                        }
                    }
                }

                if (startCapture)
                {
                    int n = Math.Min(audioData.Length, audioCaptureBuffer.Length - audioCapturePos);
                    Array.Copy(audioData, 0, audioCaptureBuffer, audioCapturePos, n);
                    audioCapturePos += n;
                    if (audioCapturePos >= audioCaptureBuffer.Length)
                        audioCaptureDone = true;
                }
            }
#endif
        }
        finally
        {
            Monitor.Exit(AudioBufferLock);
        }
#endif
    }

    /// <summary>
    /// Call from the main thread (e.g. LibretroScreenController.Update). When the
    /// capture buffer is full, writes it as a 16-bit PCM stereo WAV on a background
    /// task and logs the path so it can be pulled with adb and listened to.
    /// No-op unless _debug_audio_ is defined at the top of this file.
    /// </summary>
    public static void FlushAudioCaptureIfReady(bool force = false)
    {
#if _debug_audio_
        float[] buffer;
        int length;
        lock (AudioBufferLock)
        {
            if (audioCaptureBuffer == null || (!audioCaptureDone && !force) || audioCapturePos == 0)
                return;
            buffer = audioCaptureBuffer;
            length = audioCapturePos;
            audioCaptureBuffer = null;
            audioCapturePos = 0;
            audioCaptureDone = false;
        }

        int sampleRate = (int)wrapperOutputRate; // rate the ring content actually carries
        string path = $"{ConfigManager.GameSaveDir}/audio_capture.wav";
        Task.Run(() =>
        {
            try
            {
                WriteWav(path, buffer, length, sampleRate, 2);
                ConfigManager.WriteConsole($"[LibRetroMameCore] AUDIO CAPTURE saved: {path} ({length} samples, {length / 2 / sampleRate}s at {sampleRate}Hz)");
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleError($"[LibRetroMameCore] AUDIO CAPTURE failed: {e.Message}");
            }
        });
#endif
    }

#if _debug_audio_
    private static void WriteWav(string path, float[] samples, int length, int sampleRate, int channels)
    {
        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (var bw = new BinaryWriter(fs))
        {
            int dataBytes = length * 2; // 16-bit PCM
            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + dataBytes);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16);
            bw.Write((short)1); // PCM
            bw.Write((short)channels);
            bw.Write(sampleRate);
            bw.Write(sampleRate * channels * 2); // byte rate
            bw.Write((short)(channels * 2)); // block align
            bw.Write((short)16); // bits per sample
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write(dataBytes);
            for (int i = 0; i < length; i++)
            {
                float clamped = samples[i] < -1f ? -1f : (samples[i] > 1f ? 1f : samples[i]);
                bw.Write((short)(clamped * 32767f));
            }
        }
    }
#endif

    /// <summary>
    /// Formats and resets the audio underrun/occupancy counters accumulated since the
    /// last call. Returns null when there were no audio callbacks to report (e.g. no
    /// game currently running audio through MoveAudioStreamTo).
    /// </summary>
    public static string GetAndResetAudioStats()
    {
        long callbacks, underruns, missing, copied, gaps, lockMisses, recenters;
        int minOccupancy, maxOccupancy;
        float maxGapMs, maxLockWaitMs, totalLockWaitMs;
        DateTime since;
        DateTime now = DateTime.Now;
        long discarded;
        int startOcc, endOcc;
        lock (AudioBufferLock)
        {
            recenters = audioRecenters;
            audioRecenters = 0;
            discarded = audioDiscardedFloats;
            audioDiscardedFloats = 0;
            startOcc = audioIntervalStartOccupancy;
            endOcc = audioIntervalEndOccupancy;
            audioIntervalStartOccupancy = -1;
            callbacks = audioCallbacks;
            underruns = audioUnderruns;
            missing = audioMissingSamples;
            copied = audioCopiedFloats;
            minOccupancy = audioMinOccupancy;
            maxOccupancy = audioMaxOccupancy;
            gaps = audioGaps;
            maxGapMs = audioMaxGapMs;
            lockMisses = audioLockMisses;
            maxLockWaitMs = audioMaxLockWaitMs;
            totalLockWaitMs = audioTotalLockWaitMs;
            since = audioStatsSince;

            audioCallbacks = 0;
            audioUnderruns = 0;
            audioMissingSamples = 0;
            audioCopiedFloats = 0;
            audioMinOccupancy = int.MaxValue;
            audioMaxOccupancy = 0;
            audioGaps = 0;
            audioMaxGapMs = 0;
            audioLockMisses = 0;
            audioMaxLockWaitMs = 0;
            audioTotalLockWaitMs = 0;
            audioStatsSince = now;
        }
        long runs = Interlocked.Exchange(ref wrapperRuns, 0);

        if (callbacks == 0)
            return null;
        if (since == DateTime.MinValue)
            return null; // first interval has no reliable start time, skip it

        float elapsed = (float)(now - since).TotalSeconds;
        if (elapsed <= 0f)
            return null;

        // consumption is what Unity actually pulled; production is what the emulator
        // should generate at the requested output rate (stereo floats). If produced/s
        // exceeds consumed/s the native ring buffer overflows and drops chunks:
        // crackling plus time-compressed (fast) audio.
        float consumedPerSec = copied / elapsed;
        float expectedPerSec = (float)wrapperOutputRate * 2;
        float runsPerSec = runs / elapsed;

        // Closed-loop rate control. The real consumer demand is (copied+missing)/2:
        // valid whether the ring is starved (copied+missing = what Unity asked for) or
        // healthy/overflowing (missing=0, copied = full demand). Unlike a one-shot
        // calibration this adapts in both directions and never gets stuck - see
        // docs/libretro_audio_system.md for why the drain rate isn't a fixed constant.
        if (elapsed >= 1f)
        {
            double measuredDemand = (copied + missing) / 2.0 / elapsed;
            if (measuredDemand >= QuestAudioFrequency * 0.5 && measuredDemand <= QuestAudioFrequency * 1.05)
            {
                smoothedDemandRate = smoothedDemandRate <= 0
                    ? measuredDemand
                    : 0.5 * smoothedDemandRate + 0.5 * measuredDemand;

                // proportional trim: steer ring occupancy toward mid-ring so producer
                // jitter has headroom on both sides without drifting the rate long-term
                const int setpointFloats = AudioRingCapacityFloats / 2;
                double correction = startOcc >= 0 ? (setpointFloats - endOcc) / 2.0 / elapsed : 0;

                double newRate = smoothedDemandRate + correction;
                if (Math.Abs(newRate - wrapperOutputRate) > wrapperOutputRate * 0.001)
                {
                    wrapperOutputRate = newRate;
                    try
                    {
                        wrapper_audio_set_output_rate(wrapperOutputRate);
                    }
                    catch (EntryPointNotFoundException)
                    {
                        // wrapper built before the settable-output-rate change: nothing to adjust
                    }
                }
            }
        }

        // conservation: everything that entered the ring this interval either got
        // copied to Unity, discarded by recenters, or is still sitting in the ring.
        // Missing term: whatever the NATIVE side dropped on its own (invisible to us).
        float producedPerSec = (copied + discarded + (startOcc >= 0 ? endOcc - startOcc : 0)) / elapsed;

        return $"[AudioStats] elapsed: {elapsed:F1}s callbacks/s: {callbacks / elapsed:F1} " +
               $"consumed floats/s: {consumedPerSec:F0} expected: {expectedPerSec:F0} ({(consumedPerSec / expectedPerSec * 100f):F1}%) " +
               $"underruns: {underruns} missing: {missing} occupancy min/max: {(minOccupancy == int.MaxValue ? 0 : minOccupancy)}/{maxOccupancy} " +
               $"gaps: {gaps} maxGap: {maxGapMs:F1}ms " +
               $"lockWait avg/max: {(totalLockWaitMs / callbacks):F2}/{maxLockWaitMs:F2}ms lockMisses: {lockMisses} " +
               $"recenters: {recenters} discarded/s: {discarded / elapsed:F0} produced floats/s: {producedPerSec:F0} " +
               $"runs/s: {runsPerSec:F2} expectedFps: {expectedFps:F2} " +
               $"demand: {(elapsed >= 1f ? (copied + missing) / 2.0 / elapsed : 0):F0} rate: {wrapperOutputRate:F0}";
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

        /// <summary>Seconds remaining until isTime() would next return true, based on the
        /// balance as of the last CountTimeFrame() call. Used by the run thread to sleep
        /// instead of busy-spinning when there's slack before the next frame is due.</summary>
        public float SecondsUntilNextFrame
        {
            get { return timePerFrame - timeBalance; }
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
