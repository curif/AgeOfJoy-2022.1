using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        SceneManager.LoadScene("IntroGalleryExterior", LoadSceneMode.Additive);
    }
}
