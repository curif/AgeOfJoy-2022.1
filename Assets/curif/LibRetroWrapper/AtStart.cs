using UnityEngine;
using UnityEngine.SceneManagement;

public class AtStart : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("IntroGallery", LoadSceneMode.Additive);
        SceneManager.LoadScene("IntroGalleryExterior", LoadSceneMode.Additive);
    }
}
