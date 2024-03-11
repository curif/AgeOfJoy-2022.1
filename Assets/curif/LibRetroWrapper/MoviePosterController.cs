/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Security.Cryptography;

public class MoviePosterController : MonoBehaviour
{
    [Tooltip("The decade of the posters (70/80/90)")]
    [SerializeField]
    int decade = 80;

    void Start()
    {
        StartCoroutine(RandomizePosters());
    }

    IEnumerator RandomizePosters()
    {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

        // Yielding one frame allows Unity to finish initializing objects
        yield return null;

        //load and randomize
        if (gameObject.transform.childCount > 0)
        {
            List<Texture2D> textures = new List<Texture2D>();
            textures.AddRange(Resources.LoadAll<Texture2D>($"Decoration/MoviePoster/Pictures/{decade}"));
            textures.Sort((x, y) => UnityEngine.Random.Range(0f, 1f) > UnityEngine.Random.Range(0f, 1f) ? 1 : -1);
            int idx = textures.Count - 1;
            ConfigManager.WriteConsole($"[MoviePosterController] {idx + 1} movie posters of decade {decade}");

            for (int childIdx = 0; childIdx < gameObject.transform.childCount; childIdx++)
            {
                if (idx < 0)
                    idx = textures.Count - 1;

                Transform poster = gameObject.transform.GetChild(childIdx);
                replacePosterTexture(poster, textures[idx]);
                idx--;
            }

            // Adding a small delay between setting textures in each iteration
            yield return null;
        }
    }

    private static void replacePosterTexture(Transform poster, Texture2D texture)
    {
        if (poster.childCount > 0)
        {
            //it's a framedPoster
            // ConfigManager.WriteConsole($"[MoviePosterController] #{childIdx} framedPoster: {poster.gameObject.name}");
            Transform picture = poster.GetChild(0);
            // ConfigManager.WriteConsole($"[MoviePosterController] #{childIdx} picture: {picture.gameObject.name}");
            Renderer renderer = picture.gameObject.GetComponent<Renderer>();
            renderer.material.SetTexture("_Albedo", texture);

            if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
                renderer.material.SetFloat("_EmissiveAmount", 0.6f);
        }
        else
        {
            //old style
            poster.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
        }
    }


    // load a texture from disk.
    private static Texture2D LoadTexture(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (!File.Exists(filePath))
            throw new Exception($"loading picture, file doesn't exists: {filePath}");

        fileData = File.ReadAllBytes(filePath);
        tex = new Texture2D(2, 2, TextureFormat.RGB565, true);
        if (tex == null)
            throw new Exception($"Error creating texture for {filePath}");

        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

        return tex;
    }
    public bool Replace(int childIndex, string newTexturePath)
    {
        if (childIndex >= 0 && childIndex < gameObject.transform.childCount)
        {
            GameObject childObject = gameObject.transform.GetChild(childIndex).gameObject;

            Texture2D newTexture;
            try
            {
                newTexture = LoadTexture(newTexturePath);
            }
            catch (Exception e)
            {
                ConfigManager.WriteConsoleException($"[MoviePosterController.Replace] loading texture #{childIndex}  from disk: {newTexturePath} ", e);
                throw;
            }

            replacePosterTexture(childObject.transform, newTexture);
            return true;
        }
        else
            throw new Exception($"Poster #{childIndex} doesn't exist");

    }

    public int Count()
    {
        return gameObject.transform.childCount;
    }
}
