using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

// Installs an apk through an Android PackageInstaller session, all from C# (JNI), used by APKINSTALL (AGEBasic).
// Needs android.permission.REQUEST_INSTALL_PACKAGES in the manifest and the user's "install unknown apps" consent.
//
// The session result is sent to our own UnityPlayerActivity (singleTask): Unity's onNewIntent() stores it with
// setIntent(), and the coroutine polls getIntent(). No java BroadcastReceiver needed.
// Android kills the app when the update is applied, so SUCCESS is usually never seen.
public static class ApkInstaller
{
    // IDLE | COPYING | COMMITTING | WAITINGUSER | SUCCESS | ABORTED | CONFLICT | STORAGE | INCOMPATIBLE | INVALID | BLOCKED | FAILED
    public static string Status { get; private set; } = "IDLE";
    public static string Message { get; private set; } = "";
    public static float Progress { get; private set; } = 0f;

    public static bool Busy => Status == "COPYING" || Status == "COMMITTING";

    const string ResultAction = "com.curif.AgeOfJoy.APK_INSTALL_STATUS";
    const int ChunkSize = 1 << 20;
    const double CopyBudgetMs = 3.0;     // per frame: the copy into the session runs on the main thread (JNI)
    const float CommitTimeout = 60f;     // seconds without an answer from the installer

    // android constants
    const int STATUS_PENDING_USER_ACTION = -1;
    const int STATUS_SUCCESS = 0;
    const int STATUS_FAILURE_BLOCKED = 2;
    const int STATUS_FAILURE_ABORTED = 3;
    const int STATUS_FAILURE_INVALID = 4;
    const int STATUS_FAILURE_CONFLICT = 5;
    const int STATUS_FAILURE_STORAGE = 6;
    const int STATUS_FAILURE_INCOMPATIBLE = 7;
    const string EXTRA_STATUS = "android.content.pm.extra.STATUS";
    const string EXTRA_STATUS_MESSAGE = "android.content.pm.extra.STATUS_MESSAGE";
    const string EXTRA_INTENT = "android.intent.extra.INTENT";
    const int FLAG_ACTIVITY_NEW_TASK = 0x10000000;
    const int FLAG_ACTIVITY_SINGLE_TOP = 0x20000000;
    const int PENDING_FLAG_UPDATE_CURRENT = 0x08000000;
    const int PENDING_FLAG_MUTABLE = 0x02000000;
    const int MODE_FULL_INSTALL = 1;
    const int MODE_BACKGROUND_ACTIVITY_START_ALLOWED = 1;

    static int generation; // a new install stops the result polling of the previous one

    // the user allowed AGE of Joy to install apps ("install unknown apps").
    public static bool CanInstall()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var activity = Activity())
            using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                return pm.Call<bool>("canRequestPackageInstalls");
        }
        catch (Exception e) { Debug.LogError($"[ApkInstaller] canRequestPackageInstalls: {e.Message}"); }
#endif
        return false;
    }

    // opens the "install unknown apps" screen of AGE of Joy.
    public static void OpenInstallSettings()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var activity = Activity())
            using (var uriClass = new AndroidJavaClass("android.net.Uri"))
            using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier))
            using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_UNKNOWN_APP_SOURCES", uri))
            {
                intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK).Dispose();
                activity.Call("startActivity", intent);
            }
        }
        catch (Exception e) { Debug.LogError($"[ApkInstaller] open install settings: {e.Message}"); }
#endif
    }

    // package name inside the apk, "" if it isn't a valid apk (or not on Android).
    public static string ArchivePackage(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var activity = Activity())
            using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
            using (var info = pm.Call<AndroidJavaObject>("getPackageArchiveInfo", path, 0))
                return info == null ? "" : info.Get<string>("packageName");
        }
        catch (Exception e) { Debug.LogError($"[ApkInstaller] getPackageArchiveInfo: {e.Message}"); }
#endif
        return "";
    }

    // free bytes on the volume of path, -1 if unknown.
    public static long FreeBytes(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var statFs = new AndroidJavaObject("android.os.StatFs", path))
                return statFs.Call<long>("getAvailableBytes");
        }
        catch (Exception e) { Debug.LogError($"[ApkInstaller] StatFs {path}: {e.Message}"); }
#endif
        return -1;
    }

    // false if another install is still copying. host runs the coroutine (any active MonoBehaviour).
    public static bool Install(MonoBehaviour host, string path)
    {
        if (Busy)
            return false;
        Status = "COPYING";
        Message = "";
        Progress = 0f;
        generation++;
        host.StartCoroutine(InstallCoroutine(path, generation));
        return true;
    }

    static void Fail(string message)
    {
        Debug.LogError($"[ApkInstaller] {message}");
        Message = message;
        Status = "FAILED";
    }

    static IEnumerator InstallCoroutine(string path, int myGeneration)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject activity = null, installer = null, session = null, output = null;
        FileStream input = null;
        int sessionId = -1;
        try
        {
            try
            {
                activity = Activity();
                using (var pm = activity.Call<AndroidJavaObject>("getPackageManager"))
                    installer = pm.Call<AndroidJavaObject>("getPackageInstaller");
                input = File.OpenRead(path);
                using (var sessionParams = new AndroidJavaObject("android.content.pm.PackageInstaller$SessionParams", MODE_FULL_INSTALL))
                {
                    sessionParams.Call("setSize", input.Length);
                    sessionId = installer.Call<int>("createSession", sessionParams);
                }
                session = installer.Call<AndroidJavaObject>("openSession", sessionId);
                output = session.Call<AndroidJavaObject>("openWrite", "base.apk", 0L, input.Length);
            }
            catch (Exception e)
            {
                Fail($"can't start the install session: {e.Message}");
                yield break;
            }

            // copy the apk into the session, a few ms per frame. sbyte[] is what JNI maps to java byte[].
            byte[] buffer = new byte[ChunkSize];
            sbyte[] javaBuffer = new sbyte[ChunkSize];
            long done = 0, length = input.Length;
            var watch = new Stopwatch();
            bool finished = false;
            while (!finished)
            {
                watch.Restart();
                try
                {
                    while (watch.Elapsed.TotalMilliseconds < CopyBudgetMs)
                    {
                        int n = input.Read(buffer, 0, buffer.Length);
                        if (n <= 0)
                        {
                            finished = true;
                            break;
                        }
                        Buffer.BlockCopy(buffer, 0, javaBuffer, 0, n);
                        output.Call("write", javaBuffer, 0, n);
                        done += n;
                        Progress = length > 0 ? (float)done / length : 0f;
                    }
                }
                catch (Exception e)
                {
                    Fail($"copy to the install session failed: {e.Message}");
                    yield break;
                }
                if (!finished)
                    yield return null;
            }

            try
            {
                session.Call("fsync", output);
                output.Call("close");
                Progress = 1f;
                Status = "COMMITTING";
                using (var pending = ResultPendingIntent(activity, sessionId))
                using (var sender = pending.Call<AndroidJavaObject>("getIntentSender"))
                    session.Call("commit", sender);
                sessionId = -1; // committed: don't abandon it.
            }
            catch (Exception e)
            {
                Fail($"install commit failed: {e.Message}");
                yield break;
            }
        }
        finally
        {
            input?.Dispose();
            output?.Dispose();
            session?.Dispose();
            if (sessionId != -1 && installer != null)
            {
                try { installer.Call("abandonSession", sessionId); } catch (Exception) { }
            }
            installer?.Dispose();
            activity?.Dispose();
        }

        // wait for the results. The coroutine pauses while the confirm dialog has the focus, that's fine.
        float waited = 0f;
        while (myGeneration == generation && (Status == "COMMITTING" || Status == "WAITINGUSER"))
        {
            yield return new WaitForSecondsRealtime(0.25f);
            try
            {
                PollResult();
            }
            catch (Exception e)
            {
                Fail($"reading the install result failed: {e.Message}");
            }
            if (Status == "COMMITTING")
            {
                waited += 0.25f;
                if (waited > CommitTimeout)
                    Fail("no answer from the Android installer");
            }
        }
#else
        Fail("APK install is only available on Android");
        yield break;
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    static AndroidJavaObject Activity()
    {
        using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            return player.GetStatic<AndroidJavaObject>("currentActivity");
    }

    static int SdkInt()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            return version.GetStatic<int>("SDK_INT");
    }

    // PendingIntent that brings the session result back to our activity (singleTask -> onNewIntent -> setIntent).
    static AndroidJavaObject ResultPendingIntent(AndroidJavaObject activity, int sessionId)
    {
        using (var intent = new AndroidJavaObject("android.content.Intent", ResultAction))
        using (var component = activity.Call<AndroidJavaObject>("getComponentName"))
        using (var pendingClass = new AndroidJavaClass("android.app.PendingIntent"))
        {
            intent.Call<AndroidJavaObject>("setComponent", component).Dispose();
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_SINGLE_TOP).Dispose();
            int flags = PENDING_FLAG_UPDATE_CURRENT | PENDING_FLAG_MUTABLE;

            // android 14+: the installer starts our activity with our (creator) rights only if we allow it.
            if (SdkInt() >= 34)
            {
                try
                {
                    using (var optionsClass = new AndroidJavaClass("android.app.ActivityOptions"))
                    using (var options = optionsClass.CallStatic<AndroidJavaObject>("makeBasic"))
                    {
                        options.Call<AndroidJavaObject>("setPendingIntentCreatorBackgroundActivityStartMode",
                                                        MODE_BACKGROUND_ACTIVITY_START_ALLOWED).Dispose();
                        using (var bundle = options.Call<AndroidJavaObject>("toBundle"))
                            return pendingClass.CallStatic<AndroidJavaObject>("getActivity", activity, sessionId, intent, flags, bundle);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ApkInstaller] creator background start mode not set: {e.Message}");
                }
            }
            return pendingClass.CallStatic<AndroidJavaObject>("getActivity", activity, sessionId, intent, flags);
        }
    }

    static void PollResult()
    {
        using (var activity = Activity())
        using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
        {
            if (intent == null || intent.Call<string>("getAction") != ResultAction)
                return;

            int status = intent.Call<int>("getIntExtra", EXTRA_STATUS, -999);
            string message = intent.Call<string>("getStringExtra", EXTRA_STATUS_MESSAGE) ?? "";

            // consume it: the same result must not be read twice.
            using (var empty = new AndroidJavaObject("android.content.Intent"))
                activity.Call("setIntent", empty);

            Debug.Log($"[ApkInstaller] session result {status} {message}");
            Message = message;
            switch (status)
            {
                case STATUS_PENDING_USER_ACTION:
                    using (var confirm = intent.Call<AndroidJavaObject>("getParcelableExtra", EXTRA_INTENT))
                    {
                        if (confirm == null)
                        {
                            Fail("install confirmation missing");
                            return;
                        }
                        confirm.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK).Dispose();
                        Status = "WAITINGUSER";
                        activity.Call("startActivity", confirm);
                    }
                    break;
                case STATUS_SUCCESS: Status = "SUCCESS"; break;
                case STATUS_FAILURE_ABORTED: Status = "ABORTED"; break;
                case STATUS_FAILURE_CONFLICT: Status = "CONFLICT"; break;
                case STATUS_FAILURE_STORAGE: Status = "STORAGE"; break;
                case STATUS_FAILURE_INCOMPATIBLE: Status = "INCOMPATIBLE"; break;
                case STATUS_FAILURE_INVALID: Status = "INVALID"; break;
                case STATUS_FAILURE_BLOCKED: Status = "BLOCKED"; break;
                default: Status = "FAILED"; break;
            }
        }
    }
#endif
}
