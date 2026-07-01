/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using UnityEngine;

/// <summary>
/// One runtime mesh clone per interior spread; textures assigned once at load.
/// Forward/back turn only rotates sheets — no texture swap during turns.
/// </summary>
[DisallowMultipleComponent]
public class Magazine : MonoBehaviour
{
    const string LogPrefix = "[Magazine]";

    [Header("Textures folder")]
    [Tooltip("Subfolder name under MR/Magazines/ (e.g. Dezembro_1997_45).")]
    [SerializeField] string issueFolderName = MRPaths.ExamplePackageName;

    [Header("Cover textures")]
    [SerializeField] string frontCoverImgName = "1.jpg";
    [SerializeField] string insideFrontCoverImgName = "2.jpg";
    [SerializeField] string insideBackCoverImgName = "75.jpg";
    [SerializeField] string backCoverImgName = "76.jpg";

    [Header("Cover materials")]
    [SerializeField] Material frontCover;
    [SerializeField] Material insideFrontCover;
    [SerializeField] Material insideBackCover;
    [SerializeField] Material backCover;

    [Header("Page materials")]
    [SerializeField] Material pageFront;
    [SerializeField] Material pageBack;

    [Header("Cover")]
    [SerializeField] Transform FrontCover;

    [Header("Page")]
    [Tooltip("Leaf mesh template. One clone is spawned per interior spread at load.")]
    [SerializeField] Transform page;
    [Tooltip("Local Y step between stacked sheets (sheet N = base − N × step).")]
    [SerializeField] float sheetStackOffsetY = 0.0001f;

    Transform[] sheets;
    CoverFaceBinding[] sheetBindings;
    int[] sheetFrontSlots;
    int[] sheetBackSlots;

    int sheetCount;
    int currentSpread;
    int stackBaseSibling;
    float[] sheetZRotation;

    [Header("Page turn")]
    [Min(0.05f)]
    [SerializeField] float pageTurnDurationSeconds = 0.5f;
    [SerializeField] float pageTurnAngleDegrees = 179f;
    [Tooltip("Each turned sheet rests at base angle minus (sheet index × step).")]
    [SerializeField] float sheetTurnAngleStepDegrees = 0.01f;
    [SerializeField] bool editorPageKeys = true;

    [Header("Open")]
    [Min(0.05f)]
    [SerializeField] float openDurationSeconds = 0.5f;
    [SerializeField] float openAngleDegrees = 180f;

    [Header("Texture")]
    [Tooltip("Flip X on exterior faces (front / back). Interior faces use the opposite.")]
    [SerializeField] bool flipLeftCoverTextureX = true;
    [SerializeField] bool flipRightCoverTextureX = true;

    CoverFaceBinding leftCoverBinding = new CoverFaceBinding();
    CoverFaceBinding rightCoverBinding = new CoverFaceBinding();

    int frontCoverSlot = -1;
    int insideBackCoverSlot = -1;
    int insideFrontCoverSlot = -1;
    int backCoverSlot = -1;

    Coroutine openRoutine;
    Coroutine pageTurnRoutine;

    Transform ActiveSheet =>
        sheets != null && currentSpread >= 0 && currentSpread < sheetCount
            ? sheets[currentSpread]
            : page;

    void Awake()
    {
        if (FrontCover == null)
            FrontCover = FindChildTransform("LeftCover");

        if (page == null)
            page = FindChildTransform("page");

        leftCoverBinding.Resolve(FrontCover);
        rightCoverBinding.Resolve(FindChildTransform("RightCover"));
        ResolveCoverMaterialSlots();
    }

    void EnsureSheetsForIssue()
    {
        int requiredCount = GetSpreadCount();
        if (page == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} page not found — cannot spawn sheets.");
            return;
        }

        stackBaseSibling = page.GetSiblingIndex();

        if (sheets != null && sheets.Length > requiredCount)
        {
            for (int i = requiredCount; i < sheets.Length; i++)
            {
                if (sheets[i] != null)
                    Destroy(sheets[i].gameObject);
            }
        }

        AllocateSheetArrays(requiredCount);

        sheets[0] = page;
        if (sheetBindings[0] == null)
            sheetBindings[0] = new CoverFaceBinding();

        for (int i = 1; i < requiredCount; i++)
        {
            if (sheetBindings[i] == null)
                sheetBindings[i] = new CoverFaceBinding();

            if (sheets[i] == null)
                sheets[i] = SpawnSheetFromTemplate($"page_runtime_{i}", pageFront, pageBack, i);
        }

        sheetCount = requiredCount;

        if (page != null)
            EnsureRendererMaterialInstances(page);

        for (int i = 0; i < sheetCount; i++)
            sheetBindings[i].Resolve(sheets[i]);

        ResolvePageMaterialSlots();

        ConfigManager.WriteConsole(
            $"{LogPrefix} spawned {sheetCount} sheets (1 template + {sheetCount - 1} runtime).");
    }

    void AllocateSheetArrays(int count)
    {
        sheets = new Transform[count];
        sheetZRotation = new float[count];
        sheetFrontSlots = new int[count];
        sheetBackSlots = new int[count];

        var newBindings = new CoverFaceBinding[count];
        if (sheetBindings != null)
        {
            for (int i = 0; i < count && i < sheetBindings.Length; i++)
                newBindings[i] = sheetBindings[i];
        }

        for (int i = 0; i < count; i++)
        {
            if (newBindings[i] == null)
                newBindings[i] = new CoverFaceBinding();
        }

        sheetBindings = newBindings;
    }

    int GetSpreadCount()
    {
        int maxSpread = GetMaxSpreadIndex();
        return maxSpread >= 0 ? maxSpread + 1 : 1;
    }

    Transform SpawnSheetFromTemplate(string sheetName, Material frontMaterial, Material backMaterial, int stackIndex)
    {
        GameObject sheetObject = Instantiate(page.gameObject, page.parent);
        sheetObject.name = sheetName;

        Transform sheetTransform = sheetObject.transform;
        ApplySheetStackPosition(sheetTransform, stackIndex);
        sheetTransform.localRotation = Quaternion.identity;
        sheetTransform.localScale = page.localScale;
        sheetTransform.SetSiblingIndex(page.GetSiblingIndex() + 1 + stackIndex);

        AssignPageMaterials(sheetTransform, frontMaterial, backMaterial);
        EnsureRendererMaterialInstances(sheetTransform);
        return sheetTransform;
    }

    static void EnsureRendererMaterialInstances(Transform leafRoot)
    {
        Renderer renderer = leafRoot.GetComponentInChildren<MeshRenderer>(true);
        if (renderer == null)
            return;

        Material[] materials = renderer.materials;
        renderer.materials = materials;
    }

    void AssignPageMaterials(Transform leafRoot, Material frontMaterial, Material backMaterial)
    {
        Renderer renderer = leafRoot.GetComponentInChildren<MeshRenderer>(true);
        if (renderer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {leafRoot.name} has no MeshRenderer.");
            return;
        }

        int frontSlot = ResolveMaterialSlot(renderer, frontMaterial, "PageFront");
        int backSlot = ResolveMaterialSlot(renderer, backMaterial, "PageBack");
        AssignMaterialToSlot(renderer, frontSlot, frontMaterial);
        AssignMaterialToSlot(renderer, backSlot, backMaterial);
    }

    void ResetAllSheetRotations()
    {
        if (sheetZRotation == null)
            return;

        for (int i = 0; i < sheetCount; i++)
            sheetZRotation[i] = 0f;
    }

    void ApplySheetStackPosition(Transform sheetTransform, int sheetId)
    {
        if (sheetTransform == null || page == null)
            return;

        Vector3 basePosition = page.localPosition;
        sheetTransform.localPosition = basePosition - Vector3.up * (sheetId * sheetStackOffsetY);
    }

    void ApplyInitialStackOrder()
    {
        if (sheets == null)
            return;

        for (int i = 0; i < sheetCount; i++)
        {
            if (sheets[i] == null)
                continue;

            ApplySheetStackPosition(sheets[i], i);
            sheets[i].SetSiblingIndex(stackBaseSibling + i);
            float angle = i < currentSpread ? GetTurnedSheetAngle(i) : 0f;
            sheetZRotation[i] = angle;
            SetPageRotation(sheets[i], angle);
        }
    }

    void ClearSheetTextures(int sheetId)
    {
        ClearLeafTextures(sheetBindings[sheetId], sheetFrontSlots[sheetId], sheetBackSlots[sheetId]);
    }

    void Start()
    {
        MRMagazineCatalog.RefreshCache();
        EnsureSheetsForIssue();
        ApplyCoverTextures();
        currentSpread = 0;
        ResetAllSheetRotations();
        ApplyAllSheetSpreads();
        ApplyInitialStackOrder();
        OpenMagazine();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!editorPageKeys || !Application.isPlaying || IsBusy())
            return;

        if (MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A))
            NextPage();
        else if (MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D))
            PreviousPage();
#endif
    }

    void OnDestroy()
    {
        if (openRoutine != null)
            StopCoroutine(openRoutine);

        if (pageTurnRoutine != null)
            StopCoroutine(pageTurnRoutine);

        leftCoverBinding.Release();
        rightCoverBinding.Release();

        if (sheetBindings != null)
        {
            for (int i = 0; i < sheetBindings.Length; i++)
                sheetBindings[i]?.Release();
        }

        if (sheets != null)
        {
            for (int i = 1; i < sheets.Length; i++)
            {
                if (sheets[i] != null)
                    Destroy(sheets[i].gameObject);
            }
        }
    }

    public void ReloadIssue()
    {
        MRMagazineCatalog.RefreshCache();
        EnsureSheetsForIssue();
        currentSpread = 0;
        ResetAllSheetRotations();
        ApplyAllSheetSpreads();
        ApplyInitialStackOrder();
        ApplyCoverTextures();
    }

    public void OpenMagazine()
    {
        if (FrontCover == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} OpenMagazine — FrontCover not assigned.");
            return;
        }

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenMagazineRoutine());
    }

    public void NextPage()
    {
        if (!CanGoNextPage())
            return;

        if (pageTurnRoutine != null)
            StopCoroutine(pageTurnRoutine);

        pageTurnRoutine = StartCoroutine(RunPageTurn(TurnForwardRoutine()));
    }

    public void PreviousPage()
    {
        if (!CanGoPreviousPage())
            return;

        if (pageTurnRoutine != null)
            StopCoroutine(pageTurnRoutine);

        pageTurnRoutine = StartCoroutine(RunPageTurn(TurnBackwardRoutine()));
    }

    IEnumerator RunPageTurn(IEnumerator turnRoutine)
    {
        yield return turnRoutine;
        pageTurnRoutine = null;
    }

    public bool CanGoNextPage()
    {
        return !IsBusy() && currentSpread + 1 < sheetCount && HasSpread(currentSpread + 1);
    }

    public bool CanGoPreviousPage()
    {
        return !IsBusy() && currentSpread > 0;
    }

    bool IsBusy()
    {
        return openRoutine != null || pageTurnRoutine != null;
    }

    void ApplyAllSheetSpreads()
    {
        for (int i = 0; i < sheetCount; i++)
            ApplySpreadToSheet(i, i);
    }

    void ApplySpreadToSheet(int sheetId, int spreadIndex)
    {
        if (!HasSpread(spreadIndex))
        {
            ClearSheetTextures(sheetId);
            return;
        }

        ApplySpreadToLeaf(
            sheetBindings[sheetId],
            sheetFrontSlots[sheetId],
            sheetBackSlots[sheetId],
            pageFront,
            pageBack,
            spreadIndex,
            GetSheetLabel(sheetId));
    }

    string GetSheetLabel(int sheetId)
    {
        return sheetId == 0 ? "page" : $"page_runtime_{sheetId}";
    }

    int GetMaxSpreadIndex()
    {
        GetInteriorPageRange(out int firstPageNumber, out int lastPageNumber);
        if (lastPageNumber < firstPageNumber)
            return -1;

        return (lastPageNumber - firstPageNumber) / 2;
    }

    bool HasSpread(int spreadIndex)
    {
        return spreadIndex >= 0 && spreadIndex <= GetMaxSpreadIndex();
    }

    float GetTurnedSheetAngle(int sheetId)
    {
        return pageTurnAngleDegrees - sheetId * sheetTurnAngleStepDegrees;
    }

    IEnumerator TurnForwardRoutine()
    {
        int turningSheetId = currentSpread;
        Transform sheetTransform = sheets[turningSheetId];
        float angle = GetTurnedSheetAngle(turningSheetId);
        float duration = pageTurnDurationSeconds;

        yield return AnimatePageRotation(sheetTransform, sheetZRotation[turningSheetId], angle, duration);
        sheetZRotation[turningSheetId] = angle;
        currentSpread++;

        ConfigManager.WriteConsole($"{LogPrefix} forward → spread {currentSpread}");
    }

    IEnumerator TurnBackwardRoutine()
    {
        int turningSheetId = currentSpread - 1;
        Transform sheetTransform = sheets[turningSheetId];
        float startAngle = sheetZRotation[turningSheetId];
        float duration = pageTurnDurationSeconds;

        yield return AnimatePageRotation(sheetTransform, startAngle, 0f, duration);
        sheetZRotation[turningSheetId] = 0f;
        currentSpread--;

        ConfigManager.WriteConsole($"{LogPrefix} backward → spread {currentSpread}");
    }

    static void SetPageRotation(Transform pageTransform, float zAngle)
    {
        if (pageTransform != null)
            pageTransform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }

    static IEnumerator AnimatePageRotation(Transform target, float startAngle, float endAngle, float duration)
    {
        if (target == null)
            yield break;

        float elapsed = 0f;
        target.localRotation = Quaternion.Euler(0f, 0f, startAngle);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float z = Mathf.Lerp(startAngle, endAngle, t);
            target.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        target.localRotation = Quaternion.Euler(0f, 0f, endAngle);
    }

    void ApplyCoverTextures()
    {
        if (!MRMagazineCatalog.IssueExists(issueFolderName))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} issue '{issueFolderName}' not found in {MRPaths.MagazinesDir}");
            return;
        }

        ApplyCoverTexture(leftCoverBinding, frontCover, frontCoverSlot, frontCoverImgName, "front", flipLeftCoverTextureX);
        ApplyCoverTexture(leftCoverBinding, insideFrontCover, insideFrontCoverSlot, insideFrontCoverImgName, "inside front", !flipLeftCoverTextureX);
        ApplyCoverTexture(rightCoverBinding, insideBackCover, insideBackCoverSlot, insideBackCoverImgName, "inside back", !flipRightCoverTextureX);
        ApplyCoverTexture(rightCoverBinding, backCover, backCoverSlot, backCoverImgName, "back", flipRightCoverTextureX);
    }

    void ApplySpreadToLeaf(
        CoverFaceBinding binding,
        int frontSlot,
        int backSlot,
        Material frontMaterial,
        Material backMaterial,
        int spreadIndex,
        string leafLabel)
    {
        if (!MRMagazineCatalog.IssueExists(issueFolderName))
            return;

        if (!HasSpread(spreadIndex))
        {
            ClearLeafTextures(binding, frontSlot, backSlot);
            return;
        }

        GetSpreadPageNumbers(spreadIndex, out int frontPageNumber, out int backPageNumber);

        ApplyPageTexture(binding, frontSlot, frontPageNumber, $"{leafLabel} front", flipRightCoverTextureX, frontMaterial);
        ApplyPageTexture(binding, backSlot, backPageNumber, $"{leafLabel} back", !flipRightCoverTextureX, backMaterial);
    }

    void GetSpreadPageNumbers(int spreadIndex, out int frontPageNumber, out int backPageNumber)
    {
        GetInteriorPageRange(out int firstPageNumber, out int lastPageNumber);
        frontPageNumber = firstPageNumber + spreadIndex * 2;
        backPageNumber = Mathf.Min(frontPageNumber + 1, lastPageNumber);
    }

    void ClearLeafTextures(CoverFaceBinding binding, int frontSlot, int backSlot)
    {
        binding?.ClearSlot(frontSlot);
        binding?.ClearSlot(backSlot);
    }

    void GetInteriorPageRange(out int firstPageNumber, out int lastPageNumber)
    {
        int insideFrontPage = MRMagazineCatalog.TryParseLeadingPageNumber(insideFrontCoverImgName);
        int insideBackPage = MRMagazineCatalog.TryParseLeadingPageNumber(insideBackCoverImgName);

        firstPageNumber = insideFrontPage > 0 ? insideFrontPage + 1 : 1;
        lastPageNumber = insideBackPage > 0 ? insideBackPage - 1 : firstPageNumber - 1;
    }

    void ApplyPageTexture(
        CoverFaceBinding binding,
        int materialSlot,
        int pageNumber,
        string label,
        bool flipTextureX,
        Material materialTemplate)
    {
        if (binding == null || binding.Renderer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {label} skipped — page mesh not found.");
            return;
        }

        if (materialSlot < 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {label} skipped — material slot not found on {binding.Renderer.name}.");
            return;
        }

        Texture2D texture = MRMagazineCatalog.LoadPageTextureByFileName(issueFolderName, pageNumber.ToString());
        if (texture == null)
        {
            binding.ClearSlot(materialSlot);
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} failed to load {label} page {pageNumber} in '{issueFolderName}'");
            return;
        }

        binding.SetTexture(texture, materialSlot, materialTemplate, flipTextureX);
        ConfigManager.WriteConsole($"{LogPrefix} {label} = page {pageNumber}");
    }

    void ResolvePageMaterialSlots()
    {
        if (sheetBindings == null)
            return;

        for (int i = 0; i < sheetCount; i++)
            ResolvePageMeshSlots(sheetBindings[i], pageFront, pageBack, ref sheetFrontSlots[i], ref sheetBackSlots[i]);
    }

    void ResolvePageMeshSlots(
        CoverFaceBinding binding,
        Material frontMaterial,
        Material backMaterial,
        ref int frontSlot,
        ref int backSlot)
    {
        Renderer renderer = binding.Renderer;
        if (renderer == null)
            return;

        frontSlot = ResolveMaterialSlot(renderer, frontMaterial, "PageFront");
        backSlot = ResolveMaterialSlot(renderer, backMaterial, "PageBack");

        AssignMaterialToSlot(renderer, frontSlot, frontMaterial);
        AssignMaterialToSlot(renderer, backSlot, backMaterial);
    }

    void ApplyCoverTexture(
        CoverFaceBinding binding,
        Material materialTemplate,
        int materialSlot,
        string imageName,
        string label,
        bool flipTextureX)
    {
        if (binding.Renderer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {label} cover skipped — mesh not found.");
            return;
        }

        if (materialSlot < 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} {label} cover skipped — material slot not found on {binding.Renderer.name}.");
            return;
        }

        if (string.IsNullOrWhiteSpace(imageName))
        {
            binding.ClearSlot(materialSlot);
            return;
        }

        Texture2D texture = MRMagazineCatalog.LoadPageTextureByFileName(issueFolderName, imageName.Trim());
        if (texture == null)
        {
            binding.ClearSlot(materialSlot);
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} failed to load {label} cover '{imageName}' in '{issueFolderName}'");
            return;
        }

        binding.SetTexture(texture, materialSlot, materialTemplate, flipTextureX);
        ConfigManager.WriteConsole($"{LogPrefix} {label} cover = {imageName}");
    }

    void ResolveCoverMaterialSlots()
    {
        Renderer leftRenderer = leftCoverBinding.Renderer;
        Renderer rightRenderer = rightCoverBinding.Renderer;

        frontCoverSlot = ResolveMaterialSlot(leftRenderer, frontCover, "Front");
        insideFrontCoverSlot = ResolveMaterialSlot(leftRenderer, insideFrontCover, "InsideFront");
        insideBackCoverSlot = ResolveMaterialSlot(rightRenderer, insideBackCover, "InsideBack");
        backCoverSlot = ResolveMaterialSlot(rightRenderer, backCover, "Back");

        AssignMaterialToSlot(leftRenderer, frontCoverSlot, frontCover);
        AssignMaterialToSlot(leftRenderer, insideFrontCoverSlot, insideFrontCover);
        AssignMaterialToSlot(rightRenderer, insideBackCoverSlot, insideBackCover);
        AssignMaterialToSlot(rightRenderer, backCoverSlot, backCover);
    }

    IEnumerator OpenMagazineRoutine()
    {
        float targetAngle = Mathf.Abs(openAngleDegrees);
        float duration = openDurationSeconds;
        float elapsed = 0f;

        FrontCover.localRotation = Quaternion.Euler(0f, 0f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float z = Mathf.Lerp(0f, targetAngle, t);
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, z);
            yield return null;
        }

        FrontCover.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
        openRoutine = null;
        ConfigManager.WriteConsole($"{LogPrefix} OpenMagazine — Z = {targetAngle}°");
    }

    Transform FindChildTransform(string objectName)
    {
        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
                return child;
        }

        return null;
    }

    static int ResolveMaterialSlot(Renderer renderer, Material assignedMaterial, string fallbackPrefix)
    {
        if (renderer == null)
            return -1;

        if (assignedMaterial != null)
        {
            Material[] sharedMaterials = renderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                if (sharedMaterials[i] == assignedMaterial)
                    return i;
            }

            int byName = FindMaterialSlotByName(sharedMaterials, assignedMaterial.name);
            if (byName >= 0)
                return byName;
        }

        return FindMaterialSlotByPrefix(renderer, fallbackPrefix);
    }

    static int FindMaterialSlotByName(Material[] materials, string materialName)
    {
        if (materials == null || string.IsNullOrEmpty(materialName))
            return -1;

        for (int i = 0; i < materials.Length; i++)
        {
            Material material = materials[i];
            if (material != null && material.name.StartsWith(materialName, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    static void AssignMaterialToSlot(Renderer renderer, int slotIndex, Material material)
    {
        if (renderer == null || material == null || slotIndex < 0)
            return;

        Material[] materials = renderer.sharedMaterials;
        if (slotIndex >= materials.Length)
            return;

        materials[slotIndex] = material;
        renderer.sharedMaterials = materials;
    }

    static int FindMaterialSlotByPrefix(Renderer renderer, string materialNamePrefix)
    {
        if (renderer == null || string.IsNullOrEmpty(materialNamePrefix))
            return -1;

        Material[] sharedMaterials = renderer.sharedMaterials;
        for (int i = 0; i < sharedMaterials.Length; i++)
        {
            Material material = sharedMaterials[i];
            if (material != null && material.name.StartsWith(materialNamePrefix, System.StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    sealed class CoverFaceBinding
    {
        public Renderer Renderer { get; private set; }

        readonly System.Collections.Generic.Dictionary<int, MaterialSlot> slots
            = new System.Collections.Generic.Dictionary<int, MaterialSlot>();

        public void Resolve(Transform coverRoot)
        {
            Release();

            if (coverRoot == null)
                return;

            Renderer = coverRoot.GetComponentInChildren<MeshRenderer>(true);
        }

        public void SetTexture(Texture2D texture, int materialIndex, Material materialTemplate, bool flipTextureX)
        {
            if (Renderer == null)
            {
                if (texture != null)
                    Object.Destroy(texture);
                return;
            }

            Material[] materials = Renderer.materials;
            if (materialIndex < 0 || materialIndex >= materials.Length)
            {
                ConfigManager.WriteConsoleWarning(
                    $"{LogPrefix} {Renderer.name} slot {materialIndex} is out of range ({materials.Length} materials).");
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

            Material template = materialTemplate != null ? materialTemplate : materials[materialIndex];
            if (slot.RuntimeMaterial == null || slot.RuntimeMaterial.shader != template.shader)
            {
                if (slot.RuntimeMaterial != null)
                    Object.Destroy(slot.RuntimeMaterial);

                slot.RuntimeMaterial = new Material(template);
                slot.RuntimeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            }

            ApplyTextureToMaterial(slot.RuntimeMaterial, texture, flipTextureX);
            materials[materialIndex] = slot.RuntimeMaterial;
            Renderer.materials = materials;
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

            if (Renderer == null || slot.RuntimeMaterial == null)
                return;

            Material[] sharedMaterials = Renderer.sharedMaterials;
            Material[] materials = Renderer.materials;
            if (materialIndex < materials.Length && materialIndex < sharedMaterials.Length)
                materials[materialIndex] = sharedMaterials[materialIndex];

            Renderer.materials = materials;
            Object.Destroy(slot.RuntimeMaterial);
            slot.RuntimeMaterial = null;
            slots.Remove(materialIndex);
        }

        public void Release()
        {
            foreach (int materialIndex in new System.Collections.Generic.List<int>(slots.Keys))
                ClearSlot(materialIndex);

            Renderer = null;
        }

        static void ApplyTextureToMaterial(Material material, Texture2D texture, bool flipTextureX)
        {
            material.mainTexture = texture;
            if (material.HasProperty("_MainTex"))
                material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseMap"))
                material.SetTexture("_BaseMap", texture);

            if (flipTextureX)
            {
                material.mainTextureScale = new Vector2(-1f, 1f);
                material.mainTextureOffset = new Vector2(1f, 0f);
            }
            else
            {
                material.mainTextureScale = Vector2.one;
                material.mainTextureOffset = Vector2.zero;
            }
        }

        sealed class MaterialSlot
        {
            public Texture2D OwnedTexture;
            public Material RuntimeMaterial;
        }
    }
}
