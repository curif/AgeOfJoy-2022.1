using UnityEngine;

public class HandMeshMaterialSwitcher : MonoBehaviour
{
    public Material lightMaterial;
    public Material darkMaterial;
    public GlobalConfiguration globalConfiguration;

    [SerializeField, Tooltip("Drag the child object with the Renderer component here.")]
    private Renderer handRenderer;

    private void Start()
    {
        GameObject configuration = GameObject.Find("FixedGlobalConfiguration");
        if (configuration != null)
        {
            globalConfiguration = configuration.GetComponent<GlobalConfiguration>();
        }
        OnEnable();
    }

    void OnEnable()
    {
        // Listen for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
        SetMaterial();

    }

    void OnDisable()
    {
        // Stop listening for the config reload message
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
    }

    void OnGlobalConfigChanged()
    {        
        SetMaterial();
    }

    void SetMaterial()
    {
        handRenderer.material = globalConfiguration.Configuration.player.skinColor == "light" ? lightMaterial : darkMaterial;
    }

}
