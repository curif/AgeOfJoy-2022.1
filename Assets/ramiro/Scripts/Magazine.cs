/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Simple physical magazine: a fixed cover plus four physical sheets (assigned in the Inspector)
/// that are recycled to walk through an arbitrary number of interior pages loaded from a folder.
///
/// Rules kept intentionally simple:
///  - Turning a sheet rotates the transform; mid-turn curl uses PageConeBend (_BendAmount) when pageMaterial has it.
///  - MagazinePaper is textures only (no vertex bend).
///  - A sheet is re-textured only after its turn finishes, while it is hidden.
///  - Page textures are LRU-cached (decoded once); loads after a turn are spread across frames
///    so Quest does not hitch or OOM on large JPEGs + mipmaps.
/// </summary>
[DisallowMultipleComponent]
public class Magazine : MonoBehaviour
{
    const string LogPrefix = "[Magazine]";
    /// <summary>Cap decode size — full-res magazine scans + mipmaps OOM Quest after a few turns.</summary>
    const int MaxPageTextureSize = 1536;
    const int TextureCacheCapacity = 12;
    /// <summary>Aniso 4 is enough at arm's length; 8 costs extra on Quest GPU.</summary>
    const int PageTextureAniso = 4;

    [Header("Pages folder")]
    [Tooltip("Issue subfolder under MR/Magazines/. Used when Pages Folder Override is empty.")]
    public string issueFolderName = MRPaths.ExamplePackageName;

    [Tooltip("Optional absolute folder path with the page images. Overrides the MR issue folder.")]
    public string pagesFolderOverride = "";

    [Header("Cover image names (fixed)")]
    [Tooltip("Inferred from sorted page images (1st/2nd/penultimate/last) unless magazine.yaml overrides.")]
    public string frontCoverImgName = "";
    public string insideFrontCoverImgName = "";
    public string insideBackCoverImgName = "";
    public string backCoverImgName = "";

    [Header("Covers")]
    [Tooltip("Left cover (opens). outside = frontCoverImgName, inside = insideFrontCoverImgName.")]
    public Transform FrontCover;
    [Tooltip("Right cover. outside = backCoverImgName, inside = insideBackCoverImgName.")]
    public Transform BackCover;

    [Header("Sheets")]
    [Tooltip("Template leaf; renderer slot 0 = front face, slot 1 = back face. Found by name 'Page' if empty.")]
    public Transform sheetTemplate;
    [Tooltip("How many physical leaves to use. The template is cloned to reach this count.")]
    [Min(2)] public int physicalSheetCount = 4;
    [Tooltip("Leaf material: PageBend.mat (ramiro/PageConeBend) for curl, or Magazine.mat (ramiro/MagazinePaper) for flat pages.")]
    [FormerlySerializedAs("pageTurnMaterial")]
    public Material pageMaterial;

    [Header("Page turn")]
    [Min(0.05f)] public float pageTurnDurationSeconds = 0.6f;
    public float pageTurnAngleDegrees = 179f;
    [Tooltip("How far each hidden buffer sheet sits behind the visible top sheet. Only hidden sheets move, so nothing floats; the opaque top fully covers them, making texture swaps invisible.")]
    public float sheetStackDepth = 0.002f;

    [Header("Page bend (PageConeBend shader)")]
    [Tooltip("Peak _BendAmount at mid-turn. Ignored when pageMaterial uses MagazinePaper (no bend).")]
    public float bendPeakAmount = 1.5f;
    public float bendScale = 1f;
    [Tooltip("When on, pivot X is mesh.bounds.min.x (spine side).")]
    public bool autoBendPivot = true;
    [Tooltip("Used when Auto Bend Pivot is off.")]
    public float bendPivotX;
#if UNITY_EDITOR
    [Tooltip("Editor only — arrow keys / A D turn pages while the magazine is held (G or XR grab).")]
    public bool editorPageKeys = true;
#endif

    [Header("Open")]
    [Min(0.05f)] public float openDurationSeconds = 0.5f;
    public float openAngleDegrees = -180.81f;

    [Header("Texture flip")]
    public bool flipFrontX = true;
    public bool flipFrontY = true;
    public bool flipBackX = false;
    public bool flipBackY = true;

    // ---- runtime ----

    readonly List<string> pageFiles = new List<string>();   // interior page file names (sorted)
    int logicalSheetCount;                                   // ceil(pageFiles/2)
    int currentSpread;                                       // 0..logicalSheetCount

    Leaf[] leaves;                                           // one per physical sheet
    Vector3 baseLocalPosition;                               // shared spine position of every leaf
    Quaternion baseLocalRotation;

    readonly List<Texture2D> coverTextures = new List<Texture2D>();
    readonly List<Material> coverMaterials = new List<Material>();
    readonly Dictionary<string, Texture2D> pageTextureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
    readonly LinkedList<string> pageTextureLru = new LinkedList<string>();
    Coroutine turnRoutine;
    Coroutine openRoutine;
    Coroutine contentLoadRoutine;
    bool deferContentLoading;
    bool contentLoaded;
    bool startCompleted;
    bool openWhenContentReady;
    bool asyncLoadRequestedBeforeStart;

    bool IsBusy => turnRoutine != null || openRoutine != null;

    void Start()
    {
        if (BackCover == null)
            BackCover = FindChild("RightCover");

        MRPaths.EnsureFolders();
        ApplyIssueCovers();

        BuildPageList();
        BuildLeaves();
        startCompleted = true;

        // A grab arrived while this shelf magazine was still dormant — begin
        // the staged load now that the page structure exists.
        if (asyncLoadRequestedBeforeStart)
        {
            asyncLoadRequestedBeforeStart = false;
            BeginEnsureContentLoaded();
            return;
        }

        if (deferContentLoading)
            return;

        LoadContentTextures();
    }

    /// <summary>
    /// Skip cover/page texture decoding in <see cref="Start"/> until
    /// <see cref="EnsureContentLoaded"/>. Used by shelf proxies so nine hidden
    /// prepared magazines don't decode ~70 textures while docked.
    /// Must be set right after Instantiate, before Start runs.
    /// </summary>
    public void SetDeferContentLoading(bool defer)
    {
        deferContentLoading = defer;
    }

    public void EnsureContentLoaded()
    {
        if (contentLoaded)
            return;

        // Called before Start ran — let Start do the full load instead.
        if (!startCompleted)
        {
            deferContentLoading = false;
            return;
        }

        LoadContentTextures();
    }

    /// <summary>
    /// Loads a deferred shelf magazine over several frames, avoiding a large
    /// decode burst on the frame in which the user grabs it.
    /// </summary>
    public void BeginEnsureContentLoaded()
    {
        if (contentLoaded || contentLoadRoutine != null)
            return;

        // Start has not run yet (component was disabled while docked) — the
        // leaves/page list don't exist. Remember the request; Start honors it.
        if (!startCompleted)
        {
            asyncLoadRequestedBeforeStart = true;
            return;
        }

        contentLoadRoutine = StartCoroutine(LoadContentTexturesAsync());
    }

    void LoadContentTextures()
    {
        contentLoaded = true;
        LoadCovers();

        currentSpread = 0;
        Reconcile();

        if (FrontCover != null)
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    IEnumerator LoadContentTexturesAsync()
    {
        yield return ApplyCoverTexturesAsync(
            GetCoverRenderer(FrontCover),
            insideFrontCoverImgName,
            frontCoverImgName);
        yield return ApplyCoverTexturesAsync(
            GetCoverRenderer(BackCover),
            insideBackCoverImgName,
            backCoverImgName);

        currentSpread = 0;
        yield return ReconcileAsync();
        contentLoaded = true;
        contentLoadRoutine = null;

        if (FrontCover != null)
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, 0f);

        if (openWhenContentReady)
        {
            openWhenContentReady = false;
            OpenMagazine();
        }
    }

    void OnDestroy()
    {
        if (turnRoutine != null) StopCoroutine(turnRoutine);
        if (openRoutine != null) StopCoroutine(openRoutine);
        if (contentLoadRoutine != null) StopCoroutine(contentLoadRoutine);

        if (leaves != null)
        {
            for (int i = 0; i < leaves.Length; i++)
            {
                leaves[i]?.ReleaseTextureRefs();
                leaves[i]?.DestroyPageMaterial();

                // Destroy the runtime clones (index 0 is the template kept in the scene).
                if (i > 0 && leaves[i] != null && leaves[i].Transform != null)
                    Destroy(leaves[i].Transform.gameObject);
            }
        }

        ClearPageTextureCache();

        foreach (Texture2D texture in coverTextures)
        {
            if (texture != null)
                Destroy(texture);
        }
        coverTextures.Clear();

        foreach (Material material in coverMaterials)
        {
            if (material != null)
                Destroy(material);
        }
        coverMaterials.Clear();
    }

    // ------------------------------------------------------------------ setup

    string PagesFolder()
    {
        return string.IsNullOrEmpty(pagesFolderOverride)
            ? MRPaths.GetMagazineIssueDir(issueFolderName)
            : pagesFolderOverride;
    }

    /// <summary>Sets the MR/Magazines issue folder before <see cref="Start"/> (MR placement spawn).</summary>
    public void ConfigureIssue(string issueName)
    {
        if (string.IsNullOrWhiteSpace(issueName))
            return;

        issueFolderName = issueName.Trim();
        pagesFolderOverride = "";
    }

    void ApplyIssueCovers()
    {
        if (!string.IsNullOrEmpty(pagesFolderOverride))
        {
            if (MRMagazineIssueDefinition.TryResolveFromDir(issueFolderName, pagesFolderOverride, out MRMagazineIssueDefinition overrideDef))
            {
                overrideDef.ApplyTo(this);
                ConfigManager.WriteConsole($"{LogPrefix} covers resolved ({pagesFolderOverride})");
            }
            return;
        }

        if (string.IsNullOrEmpty(issueFolderName))
            return;

        if (MRMagazineIssueDefinition.TryResolve(issueFolderName, out MRMagazineIssueDefinition definition))
        {
            definition.ApplyTo(this);
            ConfigManager.WriteConsole($"{LogPrefix} covers resolved ({issueFolderName})");
        }
    }

    void BuildPageList()
    {
        pageFiles.Clear();

        string folder = PagesFolder();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} folder not found: {folder}");
        }
        else
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                string name = Path.GetFileName(file);
                if (IsSupportedImage(name) && !IsCoverImage(name))
                    pageFiles.Add(name);
            }
            pageFiles.Sort(CompareByLeadingNumber);
        }

        logicalSheetCount = (pageFiles.Count + 1) / 2;
        ConfigManager.WriteConsole($"{LogPrefix} {pageFiles.Count} interior page(s), {logicalSheetCount} sheet(s).");
    }

    void BuildLeaves()
    {
        leaves = new Leaf[0];

        Transform template = sheetTemplate != null ? sheetTemplate : FindChild("Page");
        if (template == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} no sheet template (assign one or name it 'Page').");
            return;
        }

        baseLocalPosition = template.localPosition;
        baseLocalRotation = template.localRotation;

        Leaf first = MakeLeaf(template);
        if (first == null)
            return;

        leaves = new[] { first };
        EnsurePageMaterial(first);
        EnsurePhysicalSheetCount();
    }

    void EnsurePhysicalSheetCount()
    {
        if (leaves == null || leaves.Length == 0 || leaves[0] == null)
            return;

        Transform template = leaves[0].Transform;
        int count = Mathf.Max(2, physicalSheetCount);
        var list = new List<Leaf> { leaves[0] };

        for (int i = 1; i < count; i++)
        {
            GameObject clone = Instantiate(template.gameObject, template.parent);
            clone.name = $"{template.name}_{i}";
            clone.transform.localPosition = baseLocalPosition;
            clone.transform.localRotation = baseLocalRotation;
            clone.transform.localScale = template.localScale;

            Leaf leaf = MakeLeaf(clone.transform);
            if (leaf != null)
            {
                EnsurePageMaterial(leaf);
                list.Add(leaf);
            }
        }

        leaves = list.ToArray();
    }

    Leaf MakeLeaf(Transform sheet)
    {
        Renderer renderer = sheet.GetComponentInChildren<MeshRenderer>(true);
        if (renderer == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} sheet '{sheet.name}' has no MeshRenderer.");
            return null;
        }

        return new Leaf(sheet, renderer, ResolveLeafPivot(renderer));
    }

    float ResolveLeafPivot(Renderer renderer)
    {
        if (!autoBendPivot)
            return bendPivotX;

        MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
            return meshFilter.sharedMesh.bounds.min.x;

        return bendPivotX;
    }

    void EnsurePageMaterial(Leaf leaf)
    {
        if (leaf == null || leaf.Renderer == null || pageMaterial == null)
            return;

        if (leaf.PageMaterial == null)
            leaf.PageMaterial = new Material(pageMaterial);

        Material[] mats = leaf.Renderer.materials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = leaf.PageMaterial;
        leaf.Renderer.materials = mats;

        SetPageBend(leaf, 0f);
    }

    void LoadCovers()
    {
        // Mesh normals invert shader front/back — outside maps to _BackTex, inside to _FrontTex.
        ApplyCoverTextures(
            GetCoverRenderer(FrontCover),
            insideFrontCoverImgName,
            frontCoverImgName);

        ApplyCoverTextures(
            GetCoverRenderer(BackCover),
            insideBackCoverImgName,
            backCoverImgName);
    }

    static Renderer GetCoverRenderer(Transform coverRoot)
    {
        return coverRoot != null ? coverRoot.GetComponentInChildren<MeshRenderer>(true) : null;
    }

    void ApplyCoverTextures(
        Renderer renderer,
        string frontImageName,
        string backImageName)
    {
        if (renderer == null)
            return;

        Texture2D front = LoadAndTrackCoverTexture(frontImageName);
        Texture2D back = LoadAndTrackCoverTexture(backImageName);
        if (front == null && back == null)
            return;

        Material mat = CreateCoverMaterial(renderer);
        coverMaterials.Add(mat);
        ApplyPageTextures(mat, front, back);

        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = mat;
        renderer.materials = mats;
    }

    IEnumerator ApplyCoverTexturesAsync(
        Renderer renderer,
        string frontImageName,
        string backImageName)
    {
        if (renderer == null)
            yield break;

        Texture2D front = LoadAndTrackCoverTexture(frontImageName);
        yield return null;
        Texture2D back = LoadAndTrackCoverTexture(backImageName);
        yield return null;
        if (front == null && back == null)
            yield break;

        Material mat = CreateCoverMaterial(renderer);
        coverMaterials.Add(mat);
        ApplyPageTextures(mat, front, back);

        Material[] mats = renderer.materials;
        for (int i = 0; i < mats.Length; i++)
            mats[i] = mat;
        renderer.materials = mats;
    }

    Texture2D LoadAndTrackCoverTexture(string imageName)
    {
        // Covers stay outside the page LRU — they must survive many interior turns.
        Texture2D texture = DecodePageTextureFromDisk(imageName);
        if (texture != null)
            coverTextures.Add(texture);
        return texture;
    }

    Material CreateCoverMaterial(Renderer renderer)
    {
        if (pageMaterial != null)
            return new Material(pageMaterial);

        if (renderer != null && renderer.sharedMaterial != null)
            return new Material(renderer.sharedMaterial);

        return new Material(Shader.Find("ramiro/MagazinePaper"));
    }

    // ------------------------------------------------------------------ navigation

    public bool CanGoNextPage() => contentLoaded && !IsBusy && currentSpread < logicalSheetCount;
    public bool CanGoPreviousPage() => contentLoaded && !IsBusy && currentSpread > 0;

    public void NextPage()
    {
        if (!CanGoNextPage())
            return;

        EnsureContentLoaded();
        Leaf leaf = LeafForSheet(currentSpread);      // right-top sheet
        if (leaf == null)
            return;

        // Reveal the next right page (it was hidden and already textured).
        ShowReveal(LeafForSheet(currentSpread + 1), 0f);
        turnRoutine = StartCoroutine(TurnSheet(leaf, 0f, pageTurnAngleDegrees, +1));
    }

    public void PreviousPage()
    {
        if (!CanGoPreviousPage())
            return;

        EnsureContentLoaded();
        Leaf leaf = LeafForSheet(currentSpread - 1);   // left-top sheet
        if (leaf == null)
            return;

        // Reveal the previous left page (hidden and already textured).
        ShowReveal(LeafForSheet(currentSpread - 2), pageTurnAngleDegrees);
        turnRoutine = StartCoroutine(TurnSheet(leaf, pageTurnAngleDegrees, 0f, -1));
    }

    /// <summary>Puts a hidden buffer at its top resting pose and turns its renderer on, ready to be uncovered.</summary>
    void ShowReveal(Leaf leaf, float restAngle)
    {
        if (leaf == null)
            return;

        leaf.Transform.localPosition = baseLocalPosition;
        SetAngle(leaf, restAngle);
        SetPageBend(leaf, 0f);
        if (leaf.Renderer != null)
            leaf.Renderer.enabled = true;
    }

    IEnumerator TurnSheet(Leaf leaf, float fromAngle, float toAngle, int spreadDelta)
    {
        leaf.Animating = true;

        // Keep the moving sheet just above both stacks so it never overlaps a resting top.
        Vector3 lifted = baseLocalPosition;
        lifted.y += sheetStackDepth * leaves.Length;
        leaf.Transform.localPosition = lifted;

        // Decode the sheet that will enter the window after this turn, one face per frame.
        int preloadSheet = spreadDelta > 0 ? currentSpread + 2 : currentSpread - 3;
        int preloadPhase = 0;

        float elapsed = 0f;
        SetAngle(leaf, fromAngle);
        while (elapsed < pageTurnDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pageTurnDurationSeconds);
            float smooth = t * t * (3f - 2f * t);
            SetAngle(leaf, Mathf.Lerp(fromAngle, toAngle, smooth));
            // Shader bend peaks mid-turn; transform handles the main flip.
            SetPageBend(leaf, Mathf.Sin(smooth * Mathf.PI) * bendPeakAmount);

            if (preloadPhase == 0)
            {
                PreloadSheetFace(preloadSheet, frontFace: true);
                preloadPhase = 1;
            }
            else if (preloadPhase == 1)
            {
                PreloadSheetFace(preloadSheet, frontFace: false);
                preloadPhase = 2;
            }

            yield return null;
        }
        SetAngle(leaf, toAngle);
        SetPageBend(leaf, 0f);

        leaf.Animating = false;
        currentSpread += spreadDelta;

        // Usually cache hits after preload; still yield so any miss cannot freeze a frame.
        yield return ReconcileAsync();

        turnRoutine = null;
    }

    void PreloadSheetFace(int sheet, bool frontFace)
    {
        if (sheet < 0 || sheet >= logicalSheetCount)
            return;

        LoadTexture(PageFile(sheet * 2 + (frontFace ? 0 : 1)));
    }

    // ------------------------------------------------------------------ recycling

    /// <summary>
    /// Maps the physical leaves to the logical sheets around the current spread
    /// (c-2 .. c+1), re-textures any leaf that changed identity (always hidden), and stacks them.
    /// </summary>
    void Reconcile()
    {
        if (leaves == null || leaves.Length == 0)
            return;

        List<int> desired = DesiredSheets();
        ReleaseLeavesOutside(desired);

        foreach (int sheet in desired)
        {
            if (LeafForSheet(sheet) != null)
                continue;

            Leaf free = FreeLeaf();
            if (free == null)
                break;

            free.Sheet = sheet;
            free.Transform.gameObject.SetActive(true);
            PositionLeaf(free);
            LoadSheetTextures(free, sheet);
        }

        PositionSettledLeaves();
    }

    /// <summary>Same as <see cref="Reconcile"/> but yields after each decode so the frame budget stays safe.</summary>
    IEnumerator ReconcileAsync()
    {
        if (leaves == null || leaves.Length == 0)
            yield break;

        List<int> desired = DesiredSheets();
        ReleaseLeavesOutside(desired);

        foreach (int sheet in desired)
        {
            if (LeafForSheet(sheet) != null)
                continue;

            Leaf free = FreeLeaf();
            if (free == null)
                break;

            free.Sheet = sheet;
            free.Transform.gameObject.SetActive(true);
            PositionLeaf(free);

            free.ReleaseTextureRefs();
            free.FrontTexture = LoadTexture(PageFile(sheet * 2));
            yield return null;
            free.BackTexture = LoadTexture(PageFile(sheet * 2 + 1));
            if (free.PageMaterial != null)
                ApplyPageTextures(free.PageMaterial, free.FrontTexture, free.BackTexture);
            yield return null;
        }

        PositionSettledLeaves();
    }

    void ReleaseLeavesOutside(List<int> desired)
    {
        foreach (Leaf leaf in leaves)
        {
            if (!leaf.Animating && (leaf.Sheet < 0 || !desired.Contains(leaf.Sheet)))
            {
                leaf.Sheet = -1;
                leaf.ReleaseTextureRefs();
            }
        }
    }

    void PositionSettledLeaves()
    {
        foreach (Leaf leaf in leaves)
        {
            if (leaf.Animating)
                continue;

            if (leaf.Sheet >= 0)
            {
                leaf.Transform.gameObject.SetActive(true);
                PositionLeaf(leaf);
            }
            else
            {
                leaf.Transform.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>The (up to) four sheets around the current spread, clamped to the real range.</summary>
    List<int> DesiredSheets()
    {
        var result = new List<int>(4);
        for (int k = currentSpread - 2; k <= currentSpread + 1; k++)
        {
            if (k >= 0 && k < logicalSheetCount)
                result.Add(k);
        }
        return result;
    }

    Leaf LeafForSheet(int sheet)
    {
        if (sheet < 0)
            return null;

        foreach (Leaf leaf in leaves)
        {
            if (leaf.Sheet == sheet)
                return leaf;
        }
        return null;
    }

    Leaf FreeLeaf()
    {
        foreach (Leaf leaf in leaves)
        {
            if (!leaf.Animating && leaf.Sheet < 0)
                return leaf;
        }
        return null;
    }

    void PositionLeaf(Leaf leaf)
    {
        bool turned = leaf.Sheet < currentSpread;
        int rank = turned ? (currentSpread - 1) - leaf.Sheet : leaf.Sheet - currentSpread;

        // Top sheet of each side (rank 0) stays at the base spine position; hidden buffers
        // sit strictly behind it. Buffers also have their renderer turned off, so a covered
        // sheet is invisible from any angle and re-texturing it can never be seen.
        Vector3 pos = baseLocalPosition;
        pos.y -= rank * sheetStackDepth;
        leaf.Transform.localPosition = pos;

        SetAngle(leaf, turned ? pageTurnAngleDegrees : 0f);
        SetPageBend(leaf, 0f);

        if (leaf.Renderer != null)
            leaf.Renderer.enabled = rank == 0;
    }

    static void SetAngle(Leaf leaf, float zAngle)
    {
        leaf.Transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }

    void SetPageBend(Leaf leaf, float amount)
    {
        if (leaf?.PageMaterial == null)
            return;

        Material material = leaf.PageMaterial;
        if (material.HasProperty("_BendAmount"))
            material.SetFloat("_BendAmount", amount);
        if (material.HasProperty("_PivotX"))
            material.SetFloat("_PivotX", leaf.PivotX);
        if (material.HasProperty("_BendScale"))
            material.SetFloat("_BendScale", bendScale);

        if (material.HasProperty("_PageWidth") && leaf.Renderer != null)
        {
            MeshFilter meshFilter = leaf.Renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                float width = meshFilter.sharedMesh.bounds.size.x;
                width = Mathf.Max(width * Mathf.Abs(leaf.Transform.lossyScale.x), 0.0001f);
                material.SetFloat("_PageWidth", width);
            }
        }
    }

    void LoadSheetTextures(Leaf leaf, int sheet)
    {
        leaf.ReleaseTextureRefs();

        leaf.FrontTexture = LoadTexture(PageFile(sheet * 2));
        leaf.BackTexture = LoadTexture(PageFile(sheet * 2 + 1));

        if (leaf.PageMaterial != null)
        {
            ApplyPageTextures(leaf.PageMaterial, leaf.FrontTexture, leaf.BackTexture);
            return;
        }

        if (leaf.Renderer.materials.Length > 0)
            ApplyTexture(leaf.Renderer.materials[0], leaf.FrontTexture, frontSide: true);
        if (leaf.Renderer.materials.Length > 1)
            ApplyTexture(leaf.Renderer.materials[1], leaf.BackTexture, frontSide: false);
    }

    string PageFile(int index)
    {
        return index >= 0 && index < pageFiles.Count ? pageFiles[index] : null;
    }

    // ------------------------------------------------------------------ cover open

    public void OpenMagazine()
    {
        if (FrontCover == null)
            return;

        if (!contentLoaded)
        {
            openWhenContentReady = true;
            BeginEnsureContentLoaded();
            return;
        }

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(AnimateCover(NormalizeZAngle(FrontCover.localEulerAngles.z), openAngleDegrees));
    }

    public void CloseMagazine()
    {
        if (FrontCover == null)
            return;

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(AnimateCover(NormalizeZAngle(FrontCover.localEulerAngles.z), 0f));
    }

    /// <summary>
    /// Closes a shelf magazine without destroying/recreating leaves or decoding
    /// pages. Its current spread stays cached for the next pickup.
    /// </summary>
    public void DockOnShelf()
    {
        openWhenContentReady = false;

        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        if (leaves != null)
        {
            foreach (Leaf leaf in leaves)
            {
                if (leaf != null)
                    leaf.Animating = false;
            }
            PositionSettledLeaves();
        }

        if (FrontCover != null)
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    /// <summary>Destroys cloned sheets, returns to spread 0, and snaps the cover closed.</summary>
    public void ResetMagazine()
    {
        if (turnRoutine != null)
        {
            StopCoroutine(turnRoutine);
            turnRoutine = null;
        }

        if (openRoutine != null)
        {
            StopCoroutine(openRoutine);
            openRoutine = null;
        }

        currentSpread = 0;

        // Content never decoded (deferred shelf magazine) — nothing to rebuild.
        if (contentLoaded)
        {
            DestroyClonesKeepTemplate();
            EnsurePhysicalSheetCount();
            Reconcile();
        }

        if (FrontCover != null)
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, 0f);

        ConfigManager.WriteConsole($"{LogPrefix} reset to closed");
    }

    void DestroyClonesKeepTemplate()
    {
        if (leaves == null || leaves.Length == 0 || leaves[0] == null)
            return;

        Leaf templateLeaf = leaves[0];
        templateLeaf.Animating = false;
        templateLeaf.Sheet = -1;
        templateLeaf.ReleaseTextureRefs();
        templateLeaf.DestroyPageMaterial();

        for (int i = 1; i < leaves.Length; i++)
        {
            if (leaves[i] == null)
                continue;

            leaves[i].Animating = false;
            leaves[i].Sheet = -1;
            leaves[i].ReleaseTextureRefs();
            leaves[i].DestroyPageMaterial();

            if (leaves[i].Transform != null)
                Destroy(leaves[i].Transform.gameObject);
        }

        ClearPageTextureCache();

        Transform template = templateLeaf.Transform;
        if (template != null)
        {
            template.localPosition = baseLocalPosition;
            template.localRotation = baseLocalRotation;
            template.gameObject.SetActive(true);

            if (templateLeaf.Renderer != null)
                templateLeaf.Renderer.enabled = true;
        }

        EnsurePageMaterial(templateLeaf);
        leaves = new[] { templateLeaf };
    }

    IEnumerator AnimateCover(float fromAngle, float toAngle)
    {
        float elapsed = 0f;
        while (elapsed < openDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDurationSeconds);
            float smooth = t * t * (3f - 2f * t);
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(fromAngle, toAngle, smooth));
            yield return null;
        }
        FrontCover.localRotation = Quaternion.Euler(0f, 0f, toAngle);
        openRoutine = null;
    }

    static float NormalizeZAngle(float z)
    {
        if (z > 180f)
            z -= 360f;
        return z;
    }

    // ------------------------------------------------------------------ helpers

    Texture2D LoadTexture(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
            return null;

        string path = Path.Combine(PagesFolder(), imageName.Trim());
        if (!File.Exists(path))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} image not found: {path}");
            return null;
        }

        if (pageTextureCache.TryGetValue(path, out Texture2D cached) && cached != null)
        {
            TouchPageTextureLru(path);
            return cached;
        }

        Texture2D texture = DecodePageTextureAtPath(path);
        if (texture == null)
            return null;

        CachePageTexture(path, texture);
        return texture;
    }

    Texture2D DecodePageTextureFromDisk(string imageName)
    {
        if (string.IsNullOrWhiteSpace(imageName))
            return null;

        string path = Path.Combine(PagesFolder(), imageName.Trim());
        if (!File.Exists(path))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} image not found: {path}");
            return null;
        }

        return DecodePageTextureAtPath(path);
    }

    Texture2D DecodePageTextureAtPath(string path)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            ConfigManager.WriteConsoleException($"{LogPrefix} read failed: {path}", e);
            return null;
        }

        // Prefer LoadImage straight into a mipmapped texture (no CPU pixel round-trip).
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: true);
        if (!texture.LoadImage(bytes))
        {
            Destroy(texture);
            ConfigManager.WriteConsoleWarning($"{LogPrefix} failed to decode: {path}");
            return null;
        }

        int maxDim = Mathf.Max(texture.width, texture.height);
        if (maxDim > MaxPageTextureSize)
        {
            Texture2D scaled = BuildDownsampledPageTexture(texture);
            Destroy(texture);
            texture = scaled;
            if (texture == null)
                return null;
        }

        texture.name = Path.GetFileNameWithoutExtension(path);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = PageTextureAniso;
        return texture;
    }

    /// <summary>Downsample oversized scans into a mipmapped GPU texture (readable discarded).</summary>
    static Texture2D BuildDownsampledPageTexture(Texture2D source)
    {
        if (source == null)
            return null;

        int srcW = source.width;
        int srcH = source.height;
        int maxDim = Mathf.Max(srcW, srcH);
        float scale = maxDim > MaxPageTextureSize ? (float)MaxPageTextureSize / maxDim : 1f;
        int dstW = Mathf.Max(1, Mathf.RoundToInt(srcW * scale));
        int dstH = Mathf.Max(1, Mathf.RoundToInt(srcH * scale));

        if (dstW == srcW && dstH == srcH)
            return null;

        Color32[] pixels = DownsampleNearest(source.GetPixels32(), srcW, srcH, dstW, dstH);
        var texture = new Texture2D(dstW, dstH, TextureFormat.RGBA32, mipChain: true);
        texture.SetPixels32(pixels);
        texture.Apply(updateMipmaps: true, makeNoLongerReadable: true);
        return texture;
    }

    static Color32[] DownsampleNearest(Color32[] src, int srcW, int srcH, int dstW, int dstH)
    {
        var dst = new Color32[dstW * dstH];
        for (int y = 0; y < dstH; y++)
        {
            int srcY = y * srcH / dstH;
            int srcRow = srcY * srcW;
            int dstRow = y * dstW;
            for (int x = 0; x < dstW; x++)
                dst[dstRow + x] = src[srcRow + (x * srcW / dstW)];
        }
        return dst;
    }

    void CachePageTexture(string path, Texture2D texture)
    {
        if (string.IsNullOrEmpty(path) || texture == null)
            return;

        if (pageTextureCache.TryGetValue(path, out Texture2D existing) && existing != null && existing != texture)
            Destroy(existing);

        pageTextureCache[path] = texture;
        TouchPageTextureLru(path);
        EvictPageTextureCacheIfNeeded();
    }

    void TouchPageTextureLru(string path)
    {
        LinkedListNode<string> node = pageTextureLru.Find(path);
        if (node != null)
            pageTextureLru.Remove(node);
        pageTextureLru.AddFirst(path);
    }

    void EvictPageTextureCacheIfNeeded()
    {
        while (pageTextureCache.Count > TextureCacheCapacity && pageTextureLru.Count > 0)
        {
            string oldest = pageTextureLru.Last.Value;
            if (IsPageTextureAssignedToLeaf(oldest))
            {
                // Keep in-use textures; rotate to front so we try another candidate next.
                pageTextureLru.RemoveLast();
                pageTextureLru.AddFirst(oldest);
                break;
            }

            pageTextureLru.RemoveLast();
            if (pageTextureCache.TryGetValue(oldest, out Texture2D tex))
            {
                pageTextureCache.Remove(oldest);
                if (tex != null)
                    Destroy(tex);
            }
        }
    }

    bool IsPageTextureAssignedToLeaf(string path)
    {
        if (leaves == null || !pageTextureCache.TryGetValue(path, out Texture2D tex) || tex == null)
            return false;

        for (int i = 0; i < leaves.Length; i++)
        {
            Leaf leaf = leaves[i];
            if (leaf == null)
                continue;
            if (leaf.FrontTexture == tex || leaf.BackTexture == tex)
                return true;
        }

        return false;
    }

    void ClearPageTextureCache()
    {
        foreach (KeyValuePair<string, Texture2D> pair in pageTextureCache)
        {
            if (pair.Value != null)
                Destroy(pair.Value);
        }

        pageTextureCache.Clear();
        pageTextureLru.Clear();

        if (leaves == null)
            return;

        for (int i = 0; i < leaves.Length; i++)
            leaves[i]?.ReleaseTextureRefs();
    }

    void ApplyTexture(Material material, Texture2D texture, bool frontSide)
    {
        if (material == null || texture == null)
            return;

        material.mainTexture = texture;

        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);

        if (frontSide)
        {
            if (material.HasProperty("_FrontTex"))
                material.SetTexture("_FrontTex", texture);
            if (material.HasProperty("_PageFront"))
                material.SetTexture("_PageFront", texture);
            SetMaterialFlip(material, "_FlipFrontX", flipFrontX);
            SetMaterialFlip(material, "_FlipFrontY", flipFrontY);
            SetMaterialFlip(material, "_FlipFrontU", flipFrontX);
            SetMaterialFlip(material, "_FlipFrontV", flipFrontY);
        }
        else
        {
            if (material.HasProperty("_BackTex"))
                material.SetTexture("_BackTex", texture);
            if (material.HasProperty("_PageBack"))
                material.SetTexture("_PageBack", texture);
            SetMaterialFlip(material, "_FlipBackX", flipBackX);
            SetMaterialFlip(material, "_FlipBackY", flipBackY);
            SetMaterialFlip(material, "_FlipBackU", flipBackX);
            SetMaterialFlip(material, "_FlipBackV", flipBackY);
        }

        // Shaders sem toggles de flip: fallback via mainTexture scale (X only).
        bool hasFlipToggle = frontSide
            ? material.HasProperty("_FlipFrontX") || material.HasProperty("_FlipFrontU")
            : material.HasProperty("_FlipBackX") || material.HasProperty("_FlipBackU");

        if (!hasFlipToggle)
        {
            bool flipX = frontSide ? flipFrontX : flipBackX;
            bool flipY = frontSide ? flipFrontY : flipBackY;
            material.mainTextureScale = new Vector2(flipX ? -1f : 1f, flipY ? -1f : 1f);
            material.mainTextureOffset = new Vector2(flipX ? 1f : 0f, flipY ? 1f : 0f);
        }
    }

    void ApplyPageTextures(Material material, Texture2D front, Texture2D back)
    {
        if (material == null)
            return;

        string shaderName = material.shader != null ? material.shader.name : "";

        if (shaderName.Contains("PageConeBend"))
        {
            if (material.HasProperty("_PageFront"))
                material.SetTexture("_PageFront", front);
            if (material.HasProperty("_PageBack"))
                material.SetTexture("_PageBack", back);
        }
        else
        {
            if (material.HasProperty("_FrontTex"))
                material.SetTexture("_FrontTex", front);
            if (material.HasProperty("_BackTex"))
                material.SetTexture("_BackTex", back);
        }

        ApplyTextureFlips(material);
    }

    void ApplyTextureFlips(Material material)
    {
        SetMaterialFlip(material, "_FlipFrontX", flipFrontX);
        SetMaterialFlip(material, "_FlipFrontY", flipFrontY);
        SetMaterialFlip(material, "_FlipBackX", flipBackX);
        SetMaterialFlip(material, "_FlipBackY", flipBackY);
        SetMaterialFlip(material, "_FlipFrontU", flipFrontX);
        SetMaterialFlip(material, "_FlipFrontV", flipFrontY);
        SetMaterialFlip(material, "_FlipBackU", flipBackX);
        SetMaterialFlip(material, "_FlipBackV", flipBackY);
    }

    static void SetMaterialFlip(Material material, string property, bool enabled)
    {
        if (material != null && material.HasProperty(property))
            material.SetFloat(property, enabled ? 1f : 0f);
    }

    bool IsCoverImage(string name)
    {
        return NameEquals(name, frontCoverImgName)
            || NameEquals(name, insideFrontCoverImgName)
            || NameEquals(name, insideBackCoverImgName)
            || NameEquals(name, backCoverImgName);
    }

    static bool NameEquals(string a, string b)
    {
        return !string.IsNullOrEmpty(a) && !string.IsNullOrEmpty(b)
            && string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }

    static bool IsSupportedImage(string name)
    {
        string ext = Path.GetExtension(name).ToLowerInvariant();
        return ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".webp" || ext == ".bmp";
    }

    static int CompareByLeadingNumber(string a, string b)
    {
        int na = LeadingNumber(a);
        int nb = LeadingNumber(b);
        if (na >= 0 && nb >= 0 && na != nb)
            return na.CompareTo(nb);

        return string.Compare(a, b, System.StringComparison.OrdinalIgnoreCase);
    }

    static int LeadingNumber(string fileName)
    {
        string name = Path.GetFileNameWithoutExtension(fileName);
        int digits = 0;
        while (digits < name.Length && char.IsDigit(name[digits]))
            digits++;

        return digits > 0 && int.TryParse(name.Substring(0, digits), out int value) ? value : -1;
    }

    Transform FindChild(string childName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
                return child;
        }
        return null;
    }

    /// <summary>A single physical sheet: transform + renderer + the two textures it currently owns.</summary>
    class Leaf
    {
        public readonly Transform Transform;
        public readonly Renderer Renderer;
        public readonly float PivotX;
        public Material PageMaterial;
        public int Sheet = -1;
        public bool Animating;
        public Texture2D FrontTexture;
        public Texture2D BackTexture;

        public Leaf(Transform transform, Renderer renderer, float pivotX)
        {
            Transform = transform;
            Renderer = renderer;
            PivotX = pivotX;
        }

        public void ReleaseTextureRefs()
        {
            FrontTexture = null;
            BackTexture = null;
        }

        public void DestroyPageMaterial()
        {
            if (PageMaterial != null)
                UnityEngine.Object.Destroy(PageMaterial);
            PageMaterial = null;
        }
    }
}
