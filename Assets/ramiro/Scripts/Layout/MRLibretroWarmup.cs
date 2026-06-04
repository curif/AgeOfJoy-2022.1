/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

/// <summary>
/// MR-only: LibretroMameCore static ctor creates Texture2D and must run on the main thread
/// before CRT OnAudioFilterRead can touch the type from the audio thread.
/// </summary>
public static class MRLibretroWarmup
{
    static bool warmed;

    public static void EnsureOnMainThread()
    {
        if (warmed)
            return;

        _ = LibretroMameCore.isRunning(string.Empty, string.Empty);
        warmed = true;
        ConfigManager.WriteConsole("[MRLibretroWarmup] LibretroMameCore static init on main thread");
    }
}
