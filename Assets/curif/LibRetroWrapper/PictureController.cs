/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PictureController : MonoBehaviour
{

    void Start()
    {

        System.Random random = new System.Random(DateTime.Now.Second);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);

        //load and randomize
        if (gameObject.transform.childCount > 0)
        {
            List<Texture2D> textures = new List<Texture2D>();
            textures.AddRange(Resources.LoadAll<Texture2D>($"Decoration/Pictures/"));
            //List<Texture2D> texturesRandomized = textures.OrderBy(a => random.Next()).ToList(); //no orderby?
            textures.Sort((x, y) => random.Next() > random.Next() ? 1 : -1);
            int idx = textures.Count - 1;
            ConfigManager.WriteConsole($"[PictureController] {idx}+1 pictures");

            for (int childIdx = 0; childIdx < gameObject.transform.childCount; childIdx++)
            {
                if (idx < 0)
                    idx = textures.Count - 1;
                
                Transform FramePicture = gameObject.transform.GetChild(childIdx);
                if (FramePicture.childCount > 0)
                {
                    //it's a framedPoster
                    // ConfigManager.WriteConsole($"[MoviePosterController] #{childIdx} framedPoster: {poster.gameObject.name}");
                    Transform picture = FramePicture.GetChild(0);
                    // ConfigManager.WriteConsole($"[MoviePosterController] #{childIdx} picture: {picture.gameObject.name}");
                    Renderer renderer = picture.gameObject.GetComponent<Renderer>();
                    renderer.material.SetTexture("_Albedo", textures[idx]);

                    // if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
                    //     renderer.material.SetFloat("_EmissiveAmount", 0.6f);
                    
                    idx--;
                }
                else
                {
                    //old style
                    FramePicture.gameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", textures[idx]);
                }
                idx--;
            }
        }

    }
}
