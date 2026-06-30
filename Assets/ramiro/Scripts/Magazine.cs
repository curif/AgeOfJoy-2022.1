/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Material index 0 = interior. Index 2 = exterior (page 1 Left, page N Right, fixed).
/// Interior spread 0: 2|3, then 4|5, … last spread: N-1 on Left.
/// </summary>
[DisallowMultipleComponent]
public class Magazine : MonoBehaviour
{
    const string LogPrefix = "[Magazine]";
    const int MinSpreadIndex = -1;
    const int DefaultInteriorPageMaterialIndex = 0;
    const int DefaultExteriorCoverMaterialIndex = 2;

    [SerializeField] string issueFolderName = MRPaths.ExamplePackageName;
    [SerializeField] Transform leftPage;
    [SerializeField] Transform rightPage;
    [SerializeField] int startSpreadIndex = MinSpreadIndex;
    [SerializeField] bool flipTextureX = true;
    [Tooltip("Material slot for interior page faces on Left and Right.")]
    [SerializeField] int interiorPageMaterialIndex = DefaultInteriorPageMaterialIndex;
    [Tooltip("Material slot for exterior covers (page 1 Left, last page Right).")]
    [SerializeField] int exteriorCoverMaterialIndex = DefaultExteriorCoverMaterialIndex;

    [Header("Editor test")]
    [SerializeField] bool editorPageKeys = true;

    PageFaceBinding leftBinding = new PageFaceBinding();
    PageFaceBinding rightBinding = new PageFaceBinding();

    int currentSpreadIndex = -1;
    int pageCount;
    bool exteriorCoversApplied;

    public string IssueFolderName => issueFolderName;
    public int CurrentSpreadIndex => currentSpreadIndex;
    public int PageCount => pageCount;
    public int SpreadCount => MRMagazineCatalog.GetSpreadCount(pageCount);

    void Awake()
    {
        ResolvePageTransforms();
        leftBinding.Resolve(leftPage, flipTextureX);
        rightBinding.Resolve(rightPage, flipTextureX);
    }

    void Start()
    {
        MRMagazineCatalog.RefreshCache();
        ReloadIssue();
        ShowSpread(startSpreadIndex);
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!editorPageKeys || !Application.isPlaying)
            return;

        if (MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D))
            NextSpread();
        else if (MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A))
            PreviousSpread();
#endif
    }

    void OnDestroy()
    {
        leftBinding.Release();
        rightBinding.Release();
    }

    public void Configure(string issueName)
    {
        issueFolderName = issueName;
        ReloadIssue();
        ShowSpread(0);
    }

    public void ReloadIssue()
    {
        exteriorCoversApplied = false;
        MRMagazineCatalog.RefreshCache();
        pageCount = MRMagazineCatalog.GetPageCount(issueFolderName);

        if (pageCount == 0)
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} no pages for issue '{issueFolderName}' in {MRPaths.MagazinesDir} " +
                "(use MR/Magazines/{issue}/1.png, 2.png, …)");
        }
        else
        {
            if (pageCount < 4)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} issue '{issueFolderName}' has {pageCount} page(s); " +
                    "need at least 4 (cover 1, interior 2|3, back cover N).");
            }
            else if (pageCount % 2 != 0)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} issue '{issueFolderName}' has {pageCount} page(s); " +
                    "magazine layout expects an even count (cover + inside back + back cover).");
            }

            for (int i = 0; i < pageCount; i++)
            {
                string fileName = MRMagazineCatalog.GetPageFileName(issueFolderName, i);
                int numbered = MRMagazineCatalog.TryParseLeadingPageNumber(fileName);
                if (numbered != i + 1)
                {
                    ConfigManager.WriteConsoleWarning(
                        $"{LogPrefix} issue '{issueFolderName}' — expected file {i + 1}.* at index {i}, " +
                        $"got '{fileName}'. Use consecutive names: 1.png, 2.png, …");
                    break;
                }
            }

            ConfigManager.WriteConsole(
                $"{LogPrefix} issue '{issueFolderName}' — {pageCount} page(s), {SpreadCount} spread(s)");
        }
    }

    public void ShowSpread(int spreadIndex)
    {
        if (pageCount <= 0)
        {
            leftBinding.ClearAll();
            rightBinding.ClearAll();
            currentSpreadIndex = 0;
            return;
        }

        int maxSpread = SpreadCount - 1;
        spreadIndex = Mathf.Clamp(spreadIndex, MinSpreadIndex, maxSpread);
        currentSpreadIndex = spreadIndex;

        EnsureExteriorCovers();

        if (spreadIndex < 0)
        {
            leftBinding.ClearSlot(interiorPageMaterialIndex);
            rightBinding.ClearSlot(interiorPageMaterialIndex);

            ConfigManager.WriteConsole(
                $"{LogPrefix} spread ext-only " +
                $"(ext {FormatPageNumber(1)}|{FormatPageNumber(pageCount)})");
            return;
        }

        MRMagazineCatalog.GetSpreadInteriorPageNumbers(spreadIndex, pageCount, out int readingLeft, out int readingRight);
        ApplyInteriorSpread(readingLeft, readingRight);

        ConfigManager.WriteConsole(
            $"{LogPrefix} spread {spreadIndex + 1}/{SpreadCount} " +
            $"(int {FormatPageNumber(readingLeft)}|{FormatPageNumber(readingRight)}, " +
            $"ext {FormatPageNumber(1)}|{FormatPageNumber(pageCount)})");
    }

    void ApplyInteriorSpread(int readingLeft, int readingRight)
    {
        ApplyInterior(leftBinding, readingLeft);
        ApplyInterior(rightBinding, readingRight);
    }

    void EnsureExteriorCovers()
    {
        if (exteriorCoversApplied || pageCount <= 0)
            return;

        ApplySlot(leftBinding, exteriorCoverMaterialIndex, 1, force: true);
        ApplySlot(rightBinding, exteriorCoverMaterialIndex, pageCount, force: true);
        exteriorCoversApplied = true;
    }

    void ApplyInterior(PageFaceBinding binding, int pageNumberOneBased)
    {
        ApplySlot(binding, interiorPageMaterialIndex, pageNumberOneBased, force: false);
    }

    static string FormatPageNumber(int pageNumber) =>
        pageNumber > 0 ? pageNumber.ToString() : "—";

    public void NextSpread()
    {
        if (SpreadCount <= 0 || IsOnPenultimateInteriorPage())
            return;

        ShowSpread(Mathf.Min(currentSpreadIndex + 1, SpreadCount - 1));
    }

    bool IsOnPenultimateInteriorPage()
    {
        if (pageCount < 2 || currentSpreadIndex < 0)
            return false;

        int penultimatePage = pageCount - 1;
        MRMagazineCatalog.GetSpreadInteriorPageNumbers(currentSpreadIndex, pageCount, out int readingLeft, out int readingRight);
        return readingLeft == penultimatePage || readingRight == penultimatePage;
    }

    public void PreviousSpread()
    {
        if (SpreadCount <= 0 || currentSpreadIndex <= 0)
            return;

        ShowSpread(currentSpreadIndex - 1);
    }

    void ApplySlot(PageFaceBinding binding, int materialIndex, int pageNumberOneBased, bool force)
    {
        if (binding.Renderer == null)
            return;

        if (pageNumberOneBased <= 0 || pageNumberOneBased > pageCount)
        {
            binding.ClearSlot(materialIndex);
            return;
        }

        if (!force && binding.GetAppliedPageNumber(materialIndex) == pageNumberOneBased)
            return;

        Texture2D texture = MRMagazineCatalog.LoadPageTexture(issueFolderName, pageNumberOneBased - 1);
        if (texture == null)
        {
            binding.ClearSlot(materialIndex);
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} failed to load page {pageNumberOneBased} of '{issueFolderName}'");
            return;
        }

        binding.SetSlotTexture(materialIndex, texture, pageNumberOneBased);
    }

    void ResolvePageTransforms()
    {
        if (leftPage == null)
            leftPage = transform.Find("Left");

        if (rightPage == null)
            rightPage = transform.Find("Right");
    }

    sealed class PageFaceBinding
    {
        public Renderer Renderer { get; private set; }

        readonly Dictionary<int, MaterialSlot> slots = new Dictionary<int, MaterialSlot>();
        Material[] cachedMaterials;
        bool flipTextureX;

        public void Resolve(Transform pageRoot, bool flipX)
        {
            flipTextureX = flipX;
            Renderer = null;
            Release();

            if (pageRoot == null)
                return;

            Renderer renderer = pageRoot.GetComponent<Renderer>();
            if (renderer == null)
            {
                foreach (Renderer childRenderer in pageRoot.GetComponentsInChildren<Renderer>(true))
                {
                    if (childRenderer is MeshRenderer)
                    {
                        renderer = childRenderer;
                        break;
                    }
                }
            }

            if (renderer == null)
                return;

            Renderer = renderer;
            cachedMaterials = Renderer.materials;
        }

        public int GetAppliedPageNumber(int materialIndex)
        {
            return slots.TryGetValue(materialIndex, out MaterialSlot slot) ? slot.AppliedPageNumber : 0;
        }

        public void SetSlotTexture(int materialIndex, Texture2D texture, int pageNumberOneBased)
        {
            if (Renderer == null)
            {
                if (texture != null)
                    Object.Destroy(texture);
                return;
            }

            if (!slots.TryGetValue(materialIndex, out MaterialSlot slot))
            {
                slot = new MaterialSlot();
                slots[materialIndex] = slot;
            }

            if (slot.OwnedTexture != null)
            {
                Object.Destroy(slot.OwnedTexture);
                slot.OwnedTexture = null;
            }

            slot.OwnedTexture = texture;
            slot.AppliedPageNumber = pageNumberOneBased;
            if (!EnsureRuntimeMaterial(materialIndex, slot))
            {
                Object.Destroy(texture);
                slot.OwnedTexture = null;
                slot.AppliedPageNumber = 0;
                return;
            }

            ApplyTextureToMaterial(slot.RuntimeMaterial, texture);
            PushMaterialsToRenderer();
        }

        public void ClearSlot(int materialIndex)
        {
            if (!slots.TryGetValue(materialIndex, out MaterialSlot slot))
                return;

            if (slot.OwnedTexture != null)
            {
                Object.Destroy(slot.OwnedTexture);
                slot.OwnedTexture = null;
            }

            slot.AppliedPageNumber = 0;

            if (slot.RuntimeMaterial != null)
                slot.RuntimeMaterial.color = new Color(0.92f, 0.92f, 0.9f, 1f);
        }

        public void ClearAll()
        {
            foreach (int materialIndex in new List<int>(slots.Keys))
                ClearSlot(materialIndex);
        }

        public void Release()
        {
            foreach (MaterialSlot slot in slots.Values)
            {
                if (slot.OwnedTexture != null)
                    Object.Destroy(slot.OwnedTexture);

                if (slot.RuntimeMaterial != null)
                    Object.Destroy(slot.RuntimeMaterial);
            }

            slots.Clear();
            cachedMaterials = null;
        }

        bool EnsureRuntimeMaterial(int materialIndex, MaterialSlot slot)
        {
            if (Renderer == null)
                return false;

            if (cachedMaterials == null)
                cachedMaterials = Renderer.materials;

            if (materialIndex < 0 || materialIndex >= cachedMaterials.Length)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} {Renderer.name} has {cachedMaterials.Length} material(s); " +
                    $"page face index {materialIndex} is out of range.");
                return false;
            }

            if (slot.RuntimeMaterial != null)
                return true;

            Material template = cachedMaterials[materialIndex];
            Shader shader = template != null ? template.shader : null;
            if (shader == null || shader.name == "Hidden/InternalErrorShader")
            {
                shader = Shader.Find("Unlit/Texture");
                if (shader == null)
                    shader = Shader.Find("Sprites/Default");
            }

            slot.RuntimeMaterial = template != null ? new Material(template) : new Material(shader);
            slot.RuntimeMaterial.shader = shader;
            slot.RuntimeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            cachedMaterials[materialIndex] = slot.RuntimeMaterial;
            return true;
        }

        void PushMaterialsToRenderer()
        {
            if (Renderer == null || cachedMaterials == null)
                return;

            foreach (KeyValuePair<int, MaterialSlot> entry in slots)
            {
                if (entry.Value.RuntimeMaterial == null)
                    continue;

                if (entry.Key < 0 || entry.Key >= cachedMaterials.Length)
                    continue;

                cachedMaterials[entry.Key] = entry.Value.RuntimeMaterial;
            }

            Renderer.materials = cachedMaterials;
        }

        void ApplyTextureToMaterial(Material material, Texture2D texture)
        {
            material.mainTexture = texture;
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);

            if (flipTextureX)
            {
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTextureScale("_MainTex", new Vector2(-1f, 1f));
                    material.SetTextureOffset("_MainTex", new Vector2(1f, 0f));
                }

                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTextureScale("_BaseMap", new Vector2(-1f, 1f));
                    material.SetTextureOffset("_BaseMap", new Vector2(1f, 0f));
                }

                material.mainTextureScale = new Vector2(-1f, 1f);
                material.mainTextureOffset = new Vector2(1f, 0f);
            }
            else
            {
                material.mainTextureScale = Vector2.one;
                material.mainTextureOffset = Vector2.zero;
            }

            material.color = Color.white;
        }

        sealed class MaterialSlot
        {
            public Material RuntimeMaterial;
            public Texture2D OwnedTexture;
            public int AppliedPageNumber;
        }
    }
}
