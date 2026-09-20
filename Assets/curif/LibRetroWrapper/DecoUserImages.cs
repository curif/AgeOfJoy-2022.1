/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// Shared helper for the decoration drop-folders (deco/pictures, deco/posters).
// Both PictureController and MoviePosterController use it so the scan, cache-reconcile,
// and load-gating logic lives in one place.
public static class DecoUserImages
{
    private const string CACHE_EXTENSION = ".aojv1";
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg" };

    // Serializes the decode step across ALL deco controllers: UnityWebRequestTexture decodes at
    // full source resolution before the downscale to the forced size, so a large source is a big
    // transient allocation. Letting only one deco decode run at a time avoids stacking those spikes
    // (a room typically loads pictures + posters concurrently). Cabinet loads don't use this gate.
    private static bool decodeInFlight = false;

    // Is the "randomize user posters and pictures" option on for the scene this decoration lives in?
    //
    // Resolved at load time by lookup rather than an inspector reference on purpose: PictureController
    // and MoviePosterController are placed directly in 20+ scenes, often several instances per scene,
    // so wiring a RoomConfiguration field into each one by hand would be a large error-prone pass.
    //
    // Order: the room config of the caller's own scene (already merged over global by RoomConfiguration),
    // then global on its own (covers scenes with no RoomConfiguration, e.g. IntroGallery), then the
    // default. Anything unset anywhere means ON, which is the behavior users had before the option existed.
    public static bool RandomizeEnabled(GameObject context)
    {
        bool? configured = null;

        try
        {
            RoomConfiguration room = FindRoomConfigurationInScene(context);
            configured = room?.Configuration?.deco?.randomizeUserImages;

            if (configured == null)
            {
                GameObject globalGO = GameObject.Find("FixedGlobalConfiguration");
                GlobalConfiguration global = globalGO?.GetComponent<GlobalConfiguration>();
                configured = global?.Configuration?.deco?.randomizeUserImages;
            }
        }
        catch (Exception e)
        {
            // Never let a config lookup stop the room from decorating itself.
            ConfigManager.WriteConsoleException("[DecoUserImages] reading deco configuration", e);
            return ConfigInformation.Deco.randomizeUserImagesDefault;
        }

        bool active = configured ?? ConfigInformation.Deco.randomizeUserImagesDefault;
        ConfigManager.WriteConsole($"[DecoUserImages] randomize user images: {active} (configured: {configured?.ToString() ?? "unset"})");
        return active;
    }

    // Walks the root objects of the caller's own scene looking for a RoomConfiguration anywhere
    // beneath them. Matching on the component, not on a GameObject name, because the naming is not
    // consistent across scenes ("RoomConfiguration", "RoomConfigurationRoom001", ...).
    private static RoomConfiguration FindRoomConfigurationInScene(GameObject context)
    {
        if (context == null)
            return null;

        foreach (GameObject root in context.scene.GetRootGameObjects())
        {
            RoomConfiguration found = root.GetComponentInChildren<RoomConfiguration>(true);
            if (found != null)
                return found;
        }

        return null;
    }

    // Returns the top-level user images in a drop folder (non-recursive, so it never descends into
    // the cache/ subfolder), and reconciles the cache dir: any *.aojv1 whose source image is gone
    // is deleted, so "removed from the drop folder" also means "its cache is gone".
    public static List<string> ScanAndReconcile(string dropDir, string cacheDir)
    {
        var images = new List<string>();

        if (string.IsNullOrEmpty(dropDir) || !Directory.Exists(dropDir))
            return images;

        try
        {
            foreach (string file in Directory.GetFiles(dropDir))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (Array.IndexOf(ImageExtensions, ext) >= 0)
                    images.Add(file);
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[DecoUserImages] scanning {dropDir}", e);
            return images;
        }

        ReconcileCache(dropDir, cacheDir);
        return images;
    }

    // Delete orphaned cache files (source image no longer present at the drop folder's top level).
    private static void ReconcileCache(string dropDir, string cacheDir)
    {
        if (string.IsNullOrEmpty(cacheDir) || !Directory.Exists(cacheDir))
            return;

        try
        {
            foreach (string cacheFile in Directory.GetFiles(cacheDir, "*" + CACHE_EXTENSION))
            {
                // cacheFile name is "<sourcefilename>.aojv1" -> strip the extension back to the source name.
                string cacheName = Path.GetFileName(cacheFile);
                string sourceName = cacheName.Substring(0, cacheName.Length - CACHE_EXTENSION.Length);
                string sourcePath = Path.Combine(dropDir, sourceName);

                if (!File.Exists(sourcePath))
                {
                    ConfigManager.WriteConsole($"[DecoUserImages] orphaned cache, removing: {cacheFile}");
                    TextureDiskCache.DeleteCache(sourcePath, cacheDir);
                }
            }
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"[DecoUserImages] reconciling {cacheDir}", e);
        }
    }

    // Loads one image through the shared texture cache at a forced size, serialized against other
    // deco loads. Deco textures are intentionally NOT pinned, so they stay evictable in the shared
    // LRU and can never starve pinned cabinet art.
    public static IEnumerator LoadGated(string path, Action<Texture2D> onComplete, string cacheDir, int forceWidth, int forceHeight)
    {
        while (decodeInFlight)
            yield return null;

        decodeInFlight = true;
        try
        {
            // forceCompress:true so deco is ALWAYS bounded (forced size + ETC2), even when the user
            // has cabinet "original textures" mode on — otherwise a wall of big source images would
            // sit at native resolution and risk OOM.
            // generateMipmaps:true so wall art doesn't shimmer/alias at distance and oblique angles.
            yield return CabinetTextureCache.LoadAndCacheAsync(
                path, onComplete,
                makeNoLongerReadable: true,
                forceCompress: true,
                cacheDir: cacheDir,
                forceWidth: forceWidth,
                forceHeight: forceHeight,
                generateMipmaps: true);
        }
        finally
        {
            decodeInFlight = false;
        }
    }
}
