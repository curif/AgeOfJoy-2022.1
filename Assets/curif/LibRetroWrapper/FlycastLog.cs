/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

// ── Beta-only tester log ──────────────────────────────────────────────────────
// Comment out the next line for a FINAL (non-beta) release: every write below becomes
// a no-op and no flycast.log is ever created. One line to strip the whole feature.
// (Mirrors the DEBUG_ACTIVE switch at the top of ConfigManager.cs.)
#define FLYCAST_FILE_LOG

using System;
using System.IO;

// FlycastLog — a small, always-on, tester-facing text log dedicated to the Flycast (Vulkan HW) core.
//
// Why this exists: every Flycast boot breadcrumb already flows through ConfigManager.WriteConsole,
// but that only reaches `adb logcat` (or the in-app bug report when a tester happens to have debug
// mode enabled) — invisible to a private-beta tester. This writes those same breadcrumbs, timestamped
// and appended, to a plain text file the tester can open straight off the headset:
//
//     <BaseDir>/Logs/flycast.log   →   /sdcard/Android/data/<bundle>/Logs/flycast.log
//
// the same tree they already browse for cabinets and downloads (no adb, no extra permission).
//
// Independent of DEBUG_ACTIVE / BugReportManager on purpose: a tester who hits a dead cabinet must
// get the reason on the FIRST occurrence, without having turned anything on. Only the Flycast driver
// (LibretroFlycastCore) writes here — nothing else in the app touches this file.
//
// Called only on infrequent lifecycle events (boot, first frame, coin, shutdown), never per frame, so
// the file I/O is not on any VR-perf hot path. Every operation is wrapped so a logging failure
// (permissions, full disk) can never throw into the emulator path.
public static class FlycastLog
{
    const long MaxBytes = 4 * 1024 * 1024;   // past this, roll to flycast.log.old so a long beta can't fill the disk
    static readonly object _lock = new object();
    static string _path;   // null = not resolved yet; "" = resolution failed, logging disabled

    static string Path_
    {
        get
        {
            if (_path == null)
            {
                try
                {
                    // ConfigManager.BaseDir is the app's own external dir — writable without the
                    // storage-permission gate that SystemDir/RomsDir throw on.
                    string dir = System.IO.Path.Combine(ConfigManager.BaseDir, "Logs");
                    Directory.CreateDirectory(dir);
                    _path = System.IO.Path.Combine(dir, "flycast.log");
                }
                catch { _path = ""; }   // give up quietly; never retry-storm
            }
            return _path;
        }
    }

    // Timestamped, blank-line-separated session banner — call once per game boot so a tester can tell
    // one attempt from the next.
    public static void Session(string header) => Write($"{Environment.NewLine}===== {Stamp()}  {header} =====");

    public static void Line(string msg) => Write($"[{Stamp()}] {msg}");
    public static void Err(string msg)  => Write($"[{Stamp()}] ERROR  {msg}");

    static string Stamp() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

    static void Write(string line)
    {
#if FLYCAST_FILE_LOG
        string p = Path_;
        if (string.IsNullOrEmpty(p)) return;
        lock (_lock)
        {
            try
            {
                Roll(p);
                File.AppendAllText(p, line + Environment.NewLine);
            }
            catch { /* logging must never break the emulator */ }
        }
#endif
    }

    static void Roll(string p)
    {
        try
        {
            var fi = new FileInfo(p);
            if (fi.Exists && fi.Length > MaxBytes)
            {
                string old = p + ".old";
                if (File.Exists(old)) File.Delete(old);
                File.Move(p, old);
            }
        }
        catch { }
    }
}
