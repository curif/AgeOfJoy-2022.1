using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoviePosterController : MonoBehaviour
{
  [Tooltip("The decade of the posters (70/80/90)")]
  [SerializeField]
  int decade = 80;

  void Start()
  {

    System.Random random = new System.Random(DateTime.Now.Second);
    
    //load and randomize
    if (gameObject.transform.childCount > 0)
    {
      List<Texture2D> textures = new();
      textures.AddRange(Resources.LoadAll<Texture2D>($"Decoration/MoviePoster/Pictures/{decade}"));
      //List<Texture2D> texturesRandomized = textures.OrderBy(a => random.Next()).ToList(); //no orderby?
      textures.Sort((x, y) => random.Next() > random.Next() ? 1 : -1);
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
      }
    }

  }
}
