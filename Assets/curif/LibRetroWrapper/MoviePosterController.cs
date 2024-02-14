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
            ConfigManager.WriteConsole($"[MoviePosterController] {idx}+1 movie posters of decade {decade}");

            for (int childIdx = 0; childIdx < gameObject.transform.childCount; childIdx++)
            {
                if (idx < 0)
                {
                    idx = textures.Count - 1;
                }
                gameObject.transform.GetChild(childIdx).gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", textures[idx]);
                idx--;

                // Adding a small delay between setting textures in each iteration
                yield return null;
            }
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
        tex = new Texture2D(2, 2);
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
            Renderer renderer = childObject.GetComponent<Renderer>();

            if (renderer == null)
                throw new Exception($"Poster #{childIndex} doesn't have a renderer");

            Texture2D newTexture;
            try
            {
                newTexture = LoadTexture(newTexturePath);
            }
            catch(Exception e)
            {
              ConfigManager.WriteConsoleException($"[MoviePosterController.Replace] loading texture #{childIndex}  from disk: {newTexturePath} ", e);
              throw;
            }
            
            renderer.material.SetTexture("_MainTex", newTexture);
            return true;
        }
        else
            throw new Exception($"Poster #{childIndex} doesn't exist");

        return false;
    }

    public int Count()
    {
        return gameObject.transform.childCount;
    }
}
