using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[DefaultExecutionOrder(-499)] // This will ensure that this script executes before others
public class AtStart : MonoBehaviour 
{
    public void Start()
    {
        MRRuntimeSettings.LogMissingInstanceOnce("AtStart");

        if (MRRuntimeSettings.AutoEnterMrOnFixedSceneBoot)
        {
            ConfigManager.WriteConsole("[AtStart] skipped VR additive scenes — auto MR boot from FixedScene");
            _ = CabinetInformation.PreloadAllAsync();
            return;
        }

        SceneManager.LoadSceneAsync("IntroGalleryExterior", LoadSceneMode.Additive);
        SceneManager.LoadSceneAsync("IntroGallery", LoadSceneMode.Additive);

        // Start preloading cabinet descriptions in the background to warm up the cache
        _ = CabinetInformation.PreloadAllAsync();
    }

    /*
     * permission has been deprecated
     * Leave this code as a reference
    IEnumerator Start()
    {
        SceneManager.LoadSceneAsync("IntroGalleryExterior", LoadSceneMode.Additive);

        if (ConfigManager.ShouldUseInternalStorage())
        {
            ConfigManager.WriteConsole($"[AtStart] must use internal storage, load introGallery");
            SceneManager.LoadSceneAsync("IntroGallery", LoadSceneMode.Additive);
        }
        else
        {
            AsyncOperation introGalleryPermission = null;
            if (!Init.havePublicStorageAccess())
            {
                ConfigManager.WriteConsole($"[AtStart] don't have public storage access, load scene IntroGalleryExteriorPermission");

                introGalleryPermission = SceneManager.LoadSceneAsync("IntroGalleryExteriorPermission", LoadSceneMode.Additive);
            }

            ConfigManager.WriteConsole($"[AtStart] waiting for action permission user...");

            while (!Init.PermissionGranted && !Init.havePublicStorageAccess())
            {
                yield return new WaitForSeconds(2f);
            }

            //init.cs informed that the permission was granted.

            if (Init.havePublicStorageAccess())
            {
                ConfigManager.WriteConsole($"[AtStart] have public storage access, load scene IntroGallery");

                SceneManager.LoadSceneAsync("IntroGallery", LoadSceneMode.Additive);
                
                if (introGalleryPermission != null)
                {
                    while (!introGalleryPermission.isDone)
                        yield return null;

                    SceneManager.UnloadSceneAsync("IntroGalleryExteriorPermission");
                }
            }
        }
    }
    */
}