/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void ReplaceChildTexture(int childIndex, Texture2D newTexture)
    {
        if (childIndex >= 0 && childIndex < gameObject.transform.childCount)
        {
            GameObject childObject = gameObject.transform.GetChild(childIndex).gameObject;
            Renderer renderer = childObject.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.SetTexture("_MainTex", newTexture);
            }
        }
    }
}
