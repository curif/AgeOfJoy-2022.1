/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Drives <c>ramiro/SimplePageBend</c> bend parameters on a material instance at runtime.
/// Assign a subdivided page mesh (many segments on X) and a material using SimplePageBend.
/// </summary>
[DisallowMultipleComponent]
public class PageBendController : MonoBehaviour
{
    [SerializeField] Material pageMaterial;
    [SerializeField] float bendAmount;
    [SerializeField] float pivotX = -0.5f;
    [SerializeField] float bendScale = 1f;

    [Tooltip("When on, creates a runtime material copy so the shared asset is not modified.")]
    [SerializeField] bool useMaterialInstance = true;

    static readonly int BendAmountId = Shader.PropertyToID("_BendAmount");
    static readonly int PivotXId = Shader.PropertyToID("_PivotX");
    static readonly int BendScaleId = Shader.PropertyToID("_BendScale");

    Material runtimeMaterial;

    void Awake()
    {
        if (pageMaterial == null)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                pageMaterial = renderer.sharedMaterial;
        }

        if (useMaterialInstance && pageMaterial != null)
        {
            runtimeMaterial = new Material(pageMaterial);
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = runtimeMaterial;
        }
    }

    void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);
    }

    void Update()
    {
        Material target = runtimeMaterial != null ? runtimeMaterial : pageMaterial;
        if (target == null)
            return;

        target.SetFloat(BendAmountId, bendAmount);
        target.SetFloat(PivotXId, pivotX);
        target.SetFloat(BendScaleId, bendScale);
    }

    public void SetBendAmount(float amount) => bendAmount = amount;
}
