/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;

/// <summary>
/// Drives <c>ramiro/PageBend</c> (Blender Simple Deform style) on a renderer.
/// Auto-fills page width/height from mesh bounds when enabled.
/// </summary>
[DisallowMultipleComponent]
[ExecuteAlways]
public class PageBend : MonoBehaviour
{
    public const string BendAmountProperty = "_BendAmount";
    public const string PageWidthProperty = "_PageWidth";
    public const string BendHeightProperty = "_BendHeight";
    public const string PageFrontProperty = "_PageFront";
    public const string PageBackProperty = "_PageBack";

    [Header("Textures")]
    public Texture pageFront;
    public Texture pageBack;

    [Header("Bend")]
    [Range(0f, 1f)] public float bendAmount = 0.5f;
    public float bendHeight = 0.15f;
    public bool bendDown;
    [Tooltip("When on, _PageWidth comes from mesh bounds (recommended).")]
    public bool autoPageSize = true;

#if UNITY_EDITOR
    [Header("Editor Preview")]
    public bool editorPreviewKeys = true;
#endif

    Renderer targetRenderer;
    Material runtimeMaterial;
    int bendAmountId;
    int pageWidthId;
    int bendHeightId;
    int bendDirectionId;
    int pageFrontId;
    int pageBackId;

    void OnEnable()
    {
        CacheIds();
        Apply();
    }

    void Update()
    {
        Apply();

#if UNITY_EDITOR
        if (!Application.isPlaying && editorPreviewKeys)
            HandleEditorPreviewKeys();
#endif
    }

    void OnValidate()
    {
        CacheIds();
        Apply();
    }

    void OnDestroy()
    {
        if (runtimeMaterial != null)
            DestroyImmediate(runtimeMaterial);
    }

    void CacheIds()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        bendAmountId = Shader.PropertyToID(BendAmountProperty);
        pageWidthId = Shader.PropertyToID(PageWidthProperty);
        bendHeightId = Shader.PropertyToID(BendHeightProperty);
        bendDirectionId = Shader.PropertyToID("_BendDirection");
        pageFrontId = Shader.PropertyToID(PageFrontProperty);
        pageBackId = Shader.PropertyToID(PageBackProperty);
    }

    public void SetBendAmount(float amount)
    {
        bendAmount = Mathf.Clamp01(amount);
        Apply();
    }

    public void SetTextures(Texture front, Texture back)
    {
        pageFront = front;
        pageBack = back;
        Apply();
    }

    void Apply()
    {
        if (targetRenderer == null)
            return;

        Material source = targetRenderer.sharedMaterial;
        if (source == null || source.shader == null)
            return;

        if (!source.shader.name.Contains("PageBend"))
            return;

        if (runtimeMaterial == null || runtimeMaterial.shader != source.shader)
        {
            if (runtimeMaterial != null)
                DestroyImmediate(runtimeMaterial);

            runtimeMaterial = new Material(source);
            targetRenderer.material = runtimeMaterial;
        }

        if (autoPageSize)
            ApplyPageSizeFromBounds();

        if (runtimeMaterial.HasProperty(bendAmountId))
            runtimeMaterial.SetFloat(bendAmountId, bendAmount);

        if (runtimeMaterial.HasProperty(bendHeightId))
        {
            float height = bendHeight;
            if (autoPageSize && targetRenderer != null)
                height *= Mathf.Max(GetPageWidth(), 0.0001f);
            runtimeMaterial.SetFloat(bendHeightId, height);
        }

        if (runtimeMaterial.HasProperty(bendDirectionId))
            runtimeMaterial.SetFloat(bendDirectionId, bendDown ? 1f : 0f);

        if (pageFront != null && runtimeMaterial.HasProperty(pageFrontId))
            runtimeMaterial.SetTexture(pageFrontId, pageFront);

        if (pageBack != null && runtimeMaterial.HasProperty(pageBackId))
            runtimeMaterial.SetTexture(pageBackId, pageBack);
    }

    void ApplyPageSizeFromBounds()
    {
        float width = GetPageWidth();
        if (runtimeMaterial.HasProperty(pageWidthId))
            runtimeMaterial.SetFloat(pageWidthId, width);
    }

    float GetPageWidth()
    {
        if (targetRenderer is MeshRenderer meshRenderer
            && meshRenderer.GetComponent<MeshFilter>()?.sharedMesh != null)
        {
            Vector3 size = meshRenderer.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            size = Vector3.Scale(size, transform.lossyScale);
            return Mathf.Max(size.x, 0.0001f);
        }

        return Mathf.Max(targetRenderer.localBounds.size.x, 0.0001f);
    }

#if UNITY_EDITOR
    void HandleEditorPreviewKeys()
    {
        if (UnityEditor.Selection.activeGameObject != gameObject)
            return;

        if (UnityEngine.Input.GetKey(KeyCode.LeftBracket))
            bendAmount = Mathf.Max(0f, bendAmount - Time.deltaTime * 0.8f);
        if (UnityEngine.Input.GetKey(KeyCode.RightBracket))
            bendAmount = Mathf.Min(1f, bendAmount + Time.deltaTime * 0.8f);
    }
#endif
}
