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
      List<Texture2D> textures = new();
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
