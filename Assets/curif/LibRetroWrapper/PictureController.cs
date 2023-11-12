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
        {
          idx = textures.Count - 1;
        }
        //Assets/Resources/Decoration/Pictures/FramedPicturePrefab.prefab
        GameObject framePrefab = gameObject.transform.GetChild(childIdx).gameObject;
        GameObject picture = framePrefab.transform.GetChild(2).gameObject;
        picture.GetComponent<Renderer>().material.SetTexture("_MainTex", textures[idx]);

        idx--;
      }
    }

  }
}
