/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Simple physical magazine: a fixed cover plus four physical sheets (assigned in the Inspector)
/// that are recycled to walk through an arbitrary number of interior pages loaded from a folder.
///
/// Rules kept intentionally simple:
///  - Turning a sheet only rotates it; textures are never changed during the animation.
///  - A sheet is re-textured only after its turn finishes, while it is hidden.
///  - Only 8 page textures live in memory at once (2 per sheet), so page count is unbounded.
/// </summary>
[DisallowMultipleComponent]
public class Magazine : MonoBehaviour
{
    const string LogPrefix = "[Magazine]";

    [Header("Pages folder")]
    [Tooltip("Issue subfolder under MR/Magazines/. Used when Pages Folder Override is empty.")]
    public string issueFolderName = MRPaths.ExamplePackageName;

    [Tooltip("Optional absolute folder path with the page images. Overrides the MR issue folder.")]
    public string pagesFolderOverride = "";

    [Header("Cover image names (fixed)")]
    public string frontCoverImgName = "1.jpg";
    public string insideFrontCoverImgName = "2.jpg";
    public string insideBackCoverImgName = "75.jpg";
    public string backCoverImgName = "76.jpg";

    [Header("Covers")]
    [Tooltip("Left cover (opens). Material slot 0 = front (outside), slot 1 = inside front.")]
    public Transform FrontCover;
    [Tooltip("Right cover. Material slot 0 = back (outside), slot 1 = inside back. Found by name 'RightCover' if empty.")]
    public Transform BackCover;

    [Header("Sheets")]
    [Tooltip("Template leaf; renderer slot 0 = front face, slot 1 = back face. Found by name 'Page' if empty.")]
    public Transform sheetTemplate;
    [Tooltip("How many physical leaves to use. The template is cloned to reach this count.")]
    [Min(2)] public int physicalSheetCount = 4;

    [Header("Page turn")]
    [Min(0.05f)] public float pageTurnDurationSeconds = 0.6f;
    public float pageTurnAngleDegrees = 179f;
    [Tooltip("How far each hidden buffer sheet sits behind the visible top sheet. Only hidden sheets move, so nothing floats; the opaque top fully covers them, making texture swaps invisible.")]
    public float sheetStackDepth = 0.002f;
    public bool editorPageKeys = true;

    [Header("Open")]
    [Min(0.05f)] public float openDurationSeconds = 0.5f;
    public float openAngleDegrees = -180.81f;

    [Header("Texture flip (X)")]
    public bool flipFrontFace = false;
    public bool flipBackFace = true;
    public bool flipFrontCover = true;
    public bool flipInsideFrontCover = false;
    public bool flipInsideBackCover = false;
    public bool flipBackCover = true;

    // ---- runtime ----

    readonly List<string> pageFiles = new List<string>();   // interior page file names (sorted)
    int logicalSheetCount;                                   // ceil(pageFiles/2)
    int currentSpread;                                       // 0..logicalSheetCount

    Leaf[] leaves;                                           // one per physical sheet
    Vector3 baseLocalPosition;                               // shared spine position of every leaf

    readonly List<Texture2D> coverTextures = new List<Texture2D>();
    Coroutine turnRoutine;
    Coroutine openRoutine;

    bool IsBusy => turnRoutine != null || openRoutine != null;

    void Start()
    {
        if (BackCover == null)
            BackCover = FindChild("RightCover");

        BuildPageList();
        BuildLeaves();
        LoadCovers();

        currentSpread = 0;
        Reconcile();
        OpenMagazine();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!editorPageKeys || IsBusy)
            return;

        if (MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A))
            NextPage();
        else if (MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D))
            PreviousPage();
#endif
    }

    void OnDestroy()
    {
        if (turnRoutine != null) StopCoroutine(turnRoutine);
        if (openRoutine != null) StopCoroutine(openRoutine);

        if (leaves != null)
        {
            for (int i = 0; i < leaves.Length; i++)
            {
                leaves[i]?.DestroyTextures();

                // Destroy the runtime clones (index 0 is the template kept in the scene).
                if (i > 0 && leaves[i] != null && leaves[i].Transform != null)
                    Destroy(leaves[i].Transform.gameObject);
            }
        }

        foreach (Texture2D texture in coverTextures)
        {
            if (texture != null)
                Destroy(texture);
        }
        coverTextures.Clear();
    }

    // ------------------------------------------------------------------ setup

    string PagesFolder()
    {
        return string.IsNullOrEmpty(pagesFolderOverride)
            ? MRPaths.GetMagazineIssueDir(issueFolderName)
            : pagesFolderOverride;
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

        int count = Mathf.Max(2, physicalSheetCount);
        var list = new List<Leaf>();

        Leaf first = MakeLeaf(template);
        if (first == null)
            return;
        list.Add(first);

        // Clone the template to reach the desired number of physical leaves.
        for (int i = 1; i < count; i++)
        {
            GameObject clone = Instantiate(template.gameObject, template.parent);
            clone.name = $"{template.name}_{i}";
            clone.transform.localPosition = template.localPosition;
            clone.transform.localRotation = template.localRotation;
            clone.transform.localScale = template.localScale;

            Leaf leaf = MakeLeaf(clone.transform);
            if (leaf != null)
                list.Add(leaf);
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

        return new Leaf(sheet, renderer);
    }

    void LoadCovers()
    {
        Renderer left = FrontCover != null ? FrontCover.GetComponentInChildren<MeshRenderer>(true) : null;
        Renderer right = BackCover != null ? BackCover.GetComponentInChildren<MeshRenderer>(true) : null;

        SetCoverFace(left, 0, frontCoverImgName, flipFrontCover);
        SetCoverFace(left, 1, insideFrontCoverImgName, flipInsideFrontCover);
        SetCoverFace(right, 0, backCoverImgName, flipBackCover);
        SetCoverFace(right, 1, insideBackCoverImgName, flipInsideBackCover);
    }

    void SetCoverFace(Renderer renderer, int slot, string imageName, bool flipX)
    {
        if (renderer == null || slot >= renderer.materials.Length)
            return;

        Texture2D texture = LoadTexture(imageName);
        if (texture == null)
            return;

        coverTextures.Add(texture);
        ApplyTexture(renderer.materials[slot], texture, flipX);
    }

    // ------------------------------------------------------------------ navigation

    public bool CanGoNextPage() => !IsBusy && currentSpread < logicalSheetCount;
    public bool CanGoPreviousPage() => !IsBusy && currentSpread > 0;

    public void NextPage()
    {
        if (!CanGoNextPage())
            return;

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

        float elapsed = 0f;
        SetAngle(leaf, fromAngle);
        while (elapsed < pageTurnDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pageTurnDurationSeconds);
            SetAngle(leaf, Mathf.Lerp(fromAngle, toAngle, t * t * (3f - 2f * t)));
            yield return null;
        }
        SetAngle(leaf, toAngle);

        leaf.Animating = false;
        currentSpread += spreadDelta;

        // Textures are only touched here, after the animation, while sheets are settled.
        Reconcile();

        turnRoutine = null;
        ConfigManager.WriteConsole($"{LogPrefix} spread {currentSpread}/{logicalSheetCount}");
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

        // Release leaves no longer needed.
        foreach (Leaf leaf in leaves)
        {
            if (!leaf.Animating && (leaf.Sheet < 0 || !desired.Contains(leaf.Sheet)))
                leaf.Sheet = -1;
        }

        // Assign missing sheets to free leaves. Position each leaf UNDER its (already textured)
        // top first, then swap its texture — so a texture change never happens on an exposed leaf.
        foreach (int sheet in desired)
        {
            if (LeafForSheet(sheet) != null)
                continue;

            Leaf free = FreeLeaf();
            if (free == null)
                break;

            free.Sheet = sheet;
            free.Transform.gameObject.SetActive(true);
            PositionLeaf(free);              // hide it behind the opaque top of its side
            LoadSheetTextures(free, sheet);  // now the swap is covered and invisible
        }

        // Position everything; park unused leaves.
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

        if (leaf.Renderer != null)
            leaf.Renderer.enabled = rank == 0;
    }

    static void SetAngle(Leaf leaf, float zAngle)
    {
        leaf.Transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
    }

    void LoadSheetTextures(Leaf leaf, int sheet)
    {
        leaf.DestroyTextures();

        leaf.FrontTexture = LoadTexture(PageFile(sheet * 2));
        leaf.BackTexture = LoadTexture(PageFile(sheet * 2 + 1));

        if (leaf.Renderer.materials.Length > 0)
            ApplyTexture(leaf.Renderer.materials[0], leaf.FrontTexture, flipFrontFace);
        if (leaf.Renderer.materials.Length > 1)
            ApplyTexture(leaf.Renderer.materials[1], leaf.BackTexture, flipBackFace);
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

        if (openRoutine != null)
            StopCoroutine(openRoutine);

        openRoutine = StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        float target = openAngleDegrees;
        float elapsed = 0f;
        while (elapsed < openDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDurationSeconds);
            FrontCover.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, target, t * t * (3f - 2f * t)));
            yield return null;
        }
        FrontCover.localRotation = Quaternion.Euler(0f, 0f, target);
        openRoutine = null;
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

        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(File.ReadAllBytes(path)))
        {
            Destroy(texture);
            ConfigManager.WriteConsoleWarning($"{LogPrefix} failed to decode: {path}");
            return null;
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    static void ApplyTexture(Material material, Texture2D texture, bool flipX)
    {
        if (material == null)
            return;

        material.mainTexture = texture;
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);

        material.mainTextureScale = flipX ? new Vector2(-1f, 1f) : Vector2.one;
        material.mainTextureOffset = flipX ? new Vector2(1f, 0f) : Vector2.zero;
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
        public int Sheet = -1;
        public bool Animating;
        public Texture2D FrontTexture;
        public Texture2D BackTexture;

        public Leaf(Transform transform, Renderer renderer)
        {
            Transform = transform;
            Renderer = renderer;
        }

        public void DestroyTextures()
        {
            if (FrontTexture != null) Object.Destroy(FrontTexture);
            if (BackTexture != null) Object.Destroy(BackTexture);
            FrontTexture = null;
            BackTexture = null;
        }
    }
}
