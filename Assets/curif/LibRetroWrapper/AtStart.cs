using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;



public class AtStart : MonoBehaviour 
{
    IEnumerator Start()
    {
        AsyncOperation introExterior = SceneManager.LoadSceneAsync("IntroGalleryExterior", LoadSceneMode.Additive);
        AsyncOperation introGallery = SceneManager.LoadSceneAsync("IntroGallery", LoadSceneMode.Additive);
        
        while (!introExterior.isDone) {
            yield return null;
        }
        while (!introGallery.isDone) {
            yield return null; 
        }
    }  
}