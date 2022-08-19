using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class MoviePoster : MonoBehaviour
{
  [SerializeField]
  Texture2D texture;

  // Start is called before the first frame update
  void Start()
  {
    if (texture != null) {
        GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
    }
  }

}
