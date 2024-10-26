using UnityEngine;

public class HandMeshMaterialSwitcher : MonoBehaviour
{
    public Material lightMaterial;
    public Material darkMaterial;
    public GlobalConfiguration globalConfiguration;

    [SerializeField, Tooltip("Drag the child object with the Renderer component here.")]
    private Renderer handRenderer;

    private bool isListenerAdded = false;

    private void Start()
    {
        GameObject configuration = GameObject.Find("FixedGlobalConfiguration");
        if (configuration != null)
        {
            globalConfiguration = configuration.GetComponent<GlobalConfiguration>();
        }
        OnEnable();
    }



    void addListener()
    {
        if (isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.AddListener(OnGlobalConfigChanged);
        isListenerAdded = true;
    }
    void removeListener()
    {
        if (!isListenerAdded) return;
        globalConfiguration?.OnGlobalConfigChanged.RemoveListener(OnGlobalConfigChanged);
        isListenerAdded = false;
    }

    void OnEnable()
    {
        addListener();
        SetMaterial();

    }

    void OnDisable()
    {
        removeListener();
    }

    void OnGlobalConfigChanged()
    {        
        SetMaterial();
    }

    void SetMaterial()
    {
        if (globalConfiguration != null)
        {
            handRenderer.material = globalConfiguration.Configuration.player.skinColor == "light" ? lightMaterial : darkMaterial;

        }
    }

}
