/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MRConfigurationUI : MonoBehaviour
{
    const string LogPrefix = "[MRConfigurationUI]";
    const string UiLayerName = "MRConfigurationUI";
    const string NotFoundCabinetMessage = "Not Found Cabinet";

    static readonly Color NeonGreen = new Color32(0x00, 0xff, 0x99, 0xff);
    static readonly Color NeonGreenDark = new Color32(0x00, 0x2b, 0x1f, 0xff);
    static readonly Color MainBg = new Color32(0x11, 0x11, 0x11, 0xff);
    static readonly Color HeaderBg = new Color32(0x10, 0x10, 0x10, 0xff);
    static readonly Color SidebarBg = new Color32(0x09, 0x09, 0x09, 0xff);
    static readonly Color ContentBg = new Color32(0x15, 0x15, 0x15, 0xff);
    static readonly Color WindowBg = new Color32(0x0d, 0x0d, 0x0d, 0xff);
    static readonly Color FooterBg = new Color32(0x05, 0x05, 0x05, 0xff);
    static readonly Color GrayText = new Color32(0xaa, 0xaa, 0xaa, 0xff);
    static readonly Color LightText = new Color32(0xcc, 0xcc, 0xcc, 0xff);
    static readonly Color TableBorder = Color.white;
    static readonly Color SelectedBg = new Color32(0x00, 0x33, 0x22, 0xff);
    static readonly Color ButtonFace = new Color32(0xc0, 0xc0, 0xc0, 0xff);
    static readonly Color ButtonRemove = new Color32(0xff, 0xcc, 0xcc, 0xff);
    static readonly Color ButtonAdd = new Color32(0xcc, 0xff, 0xcc, 0xff);

    [Header("World-space display")]
    [SerializeField] Transform modelRoot;
    [SerializeField] Transform screenAnchor;
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform panelRoot;
    [Tooltip("When enabled, keeps the Canvas parent as placed in the scene (under the 3D model).")]
    [SerializeField] bool preserveSceneCanvasTransform = true;
    [Tooltip("RectTransform of the world-space Canvas on the 3D frame (Inspector values).")]
    [SerializeField] Vector3 canvasLocalPosition = new Vector3(0.001f, -0.0048f, 0.0018f);
    [SerializeField] Vector3 canvasLocalEuler = Vector3.zero;
    [SerializeField] Vector3 canvasLocalScale = new Vector3(0.000765891f, 0.000781534f, 0.00054038f);
    [SerializeField] Vector2 canvasSizeDelta = new Vector2(604.7664f, 595.2363f);
    [Tooltip("Extra local rotation applied to UIRoot (content root inside the Canvas).")]
    [SerializeField] Vector3 uiRootLocalEuler = Vector3.zero;

    [Header("Catalog")]
    [SerializeField] bool bootstrapCatalogOnStart = true;
    [SerializeField] bool seedExampleCabinetInEditor = true;
    [SerializeField] bool enableVideoPreview = true;

    [Header("Input")]
    [SerializeField] float spawnDistanceMeters = 2f;
    [SerializeField] float spawnYOffsetMeters;
    [SerializeField] float navRepeatDelay = 0.16f;

    enum Page
    {
        Cabinets,
        AddedCabinets,
        Help
    }

    sealed class CabinetRowView
    {
        public string CabinetDBName;
        public bool InScene;
        public string VideoPath;
        public string MarqueePath;
        public Image Background;
        public Text NameText;
        public Text AddedText;
        public Text ActionText;
        public RawImage Preview;
        public Text PreviewFallback;
    }

    readonly List<CabinetRowView> rows = new List<CabinetRowView>();
    readonly List<Text> menuTexts = new List<Text>();

    Font uiFont;
    int uiLayer = -1;
    MRLayoutRegistry registry;
    Transform mrSpaceOrigin;
    Transform displaySurface;

    RectTransform uiRoot;
    RectTransform contentRoot;
    RectTransform cabinetsPage;
    RectTransform addedPage;
    RectTransform helpPage;
    RectTransform tableBody;
    RectTransform addedTableBody;

    ScrollRect cabinetsScroll;
    ScrollRect addedScroll;

    enum Focus
    {
        Sidebar,
        Content
    }

    Page activePage = Page.Cabinets;
    Focus activeFocus = Focus.Sidebar;
    int selectedIndex;
    float navCooldown;

    VideoPlayer previewVideoPlayer;
    VideoPlayer thumbnailVideoPlayer;
    GameObject playVideoHost;
    GameObject thumbnailVideoHost;
    RenderTexture previewRenderTexture;
    RenderTexture thumbnailRenderTexture;
    CabinetRowView previewBoundRow;
    string previewCurrentPath;

    readonly Dictionary<string, Texture2D> previewThumbnailCache = new Dictionary<string, Texture2D>(System.StringComparer.OrdinalIgnoreCase);
    readonly Queue<CabinetRowView> thumbnailLoadQueue = new Queue<CabinetRowView>();
    Coroutine thumbnailLoadCoroutine;
    Coroutine previewPlayCoroutine;

    void Awake()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureUiLayerResolved();

        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);

        SetupCanvas();
        BuildUi();

        registry = EnsureRegistry();
        mrSpaceOrigin = EnsureMrSpaceOrigin();

        registry.EnsureLayoutLoaded();
        registry.SpawnAll(mrSpaceOrigin);

        if (bootstrapCatalogOnStart)
            MRCatalogBootstrap.PrepareCatalog(seedExampleCabinetInEditor);

        RefreshAll();
    }

    void OnEnable()
    {
        if (tableBody != null)
            RefreshAll();
    }

    void OnDestroy()
    {
        StopPreviewVideo();

        if (thumbnailLoadCoroutine != null)
        {
            StopCoroutine(thumbnailLoadCoroutine);
            thumbnailLoadCoroutine = null;
        }

        thumbnailLoadQueue.Clear();

        foreach (Texture2D tex in previewThumbnailCache.Values)
        {
            if (tex != null)
                Destroy(tex);
        }
        previewThumbnailCache.Clear();

        if (previewRenderTexture != null)
        {
            previewRenderTexture.Release();
            Destroy(previewRenderTexture);
        }

        if (thumbnailRenderTexture != null)
        {
            thumbnailRenderTexture.Release();
            Destroy(thumbnailRenderTexture);
        }

        if (playVideoHost != null)
            Destroy(playVideoHost);

        if (thumbnailVideoHost != null)
            Destroy(thumbnailVideoHost);
    }

    void Update()
    {
        navCooldown -= Time.deltaTime;

        if (activeFocus == Focus.Sidebar)
        {
            if (WasMoveRight() && CanFocusContent())
            {
                SetFocus(Focus.Content);
                return;
            }

            if (WasMoveUp())
            {
                SetPagePrevious();
                return;
            }

            if (WasMoveDown())
            {
                SetPageNext();
                return;
            }

            if (navCooldown <= 0f)
            {
                if (IsMoveUpHeld())
                {
                    SetPagePrevious();
                    return;
                }

                if (IsMoveDownHeld())
                {
                    SetPageNext();
                    return;
                }
            }

            return;
        }

        if (!CanFocusContent())
        {
            SetFocus(Focus.Sidebar);
            return;
        }

        if (WasMoveLeft())
        {
            SetFocus(Focus.Sidebar);
            return;
        }

        if (WasMoveUp())
            MoveSelection(-1);
        else if (WasMoveDown())
            MoveSelection(1);
        else if (navCooldown <= 0f)
        {
            if (IsMoveUpHeld())
                MoveSelection(-1);
            else if (IsMoveDownHeld())
                MoveSelection(1);
        }

        if (WasConfirmPressed())
            ExecuteSelectedRow();
    }

    bool CanFocusContent() =>
        activePage == Page.Cabinets && rows.Count > 0;

    void SetFocus(Focus focus)
    {
        if (activeFocus == focus)
            return;

        activeFocus = focus;
        navCooldown = navRepeatDelay;
        ApplyActivePage();
    }

    void SetupCanvas()
    {
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ApplyUiLayer(canvasGo);
            canvasGo.transform.SetParent(transform, false);
            canvas = canvasGo.GetComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        if (canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

        GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
            raycaster.enabled = false;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = canvasSizeDelta;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (!preserveSceneCanvasTransform || !IsCanvasPlacedInScene())
        {
            Transform parent = ResolveDisplayParent();
            displaySurface = EnsureChild(parent, "UISurface");
            canvas.transform.SetParent(displaySurface, false);
        }
        else
        {
            displaySurface = canvas.transform.parent;
        }

        ApplyCanvasRectTransform(canvasRect);

        uiRoot = CreateOrGetRect(canvas.transform, "UIRoot");
        Stretch(uiRoot);
        uiRoot.localScale = Vector3.one;
        uiRoot.localRotation = Quaternion.Euler(uiRootLocalEuler);

        if (panelRoot == null)
        {
            panelRoot = CreateOrGetRect(uiRoot, "Panel");
        }
        else
        {
            panelRoot.SetParent(uiRoot, false);
        }

        Stretch(panelRoot);
        ApplyUiLayer(canvas.gameObject);
    }

    void EnsureUiLayerResolved()
    {
        if (uiLayer >= 0)
            return;

        uiLayer = LayerMask.NameToLayer(UiLayerName);
        if (uiLayer < 0)
            ConfigManager.WriteConsoleWarning($"{LogPrefix} layer '{UiLayerName}' not found; cabinet UI may not render.");
    }

    void ApplyUiLayer(GameObject go)
    {
        if (go == null)
            return;

        EnsureUiLayerResolved();
        if (uiLayer >= 0)
            go.layer = uiLayer;
    }

    void ApplyUiLayerRecursive(Transform root)
    {
        if (root == null)
            return;

        EnsureUiLayerResolved();
        if (uiLayer < 0)
            return;

        SetLayerRecursive(root, uiLayer);
    }

    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursive(t.GetChild(i), layer);
    }

    RectTransform CreateOrGetRect(Transform parent, string name)
    {
        Transform existing = parent.Find(name);

        if (existing != null)
        {
            RectTransform existingRect = existing.GetComponent<RectTransform>();

            if (existingRect != null)
                return existingRect;
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        ApplyUiLayer(go);
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    void ApplyCanvasRectTransform(RectTransform canvasRect)
    {
        if (canvasRect == null)
            return;

        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.zero;
        canvasRect.pivot = new Vector2(0.5f, 0.5f);
        canvasRect.sizeDelta = canvasSizeDelta;
        canvasRect.localPosition = canvasLocalPosition;
        canvasRect.localRotation = Quaternion.Euler(canvasLocalEuler);
        canvasRect.localScale = canvasLocalScale;
    }

    bool IsCanvasPlacedInScene()
    {
        if (canvas == null)
            return false;

        Transform displayParent = ResolveDisplayParent();
        Transform t = canvas.transform;
        while (t != null)
        {
            if (t == displayParent)
                return true;
            t = t.parent;
        }

        return false;
    }

    Transform ResolveDisplayParent()
    {
        if (screenAnchor != null)
            return screenAnchor;

        if (modelRoot != null)
            return modelRoot;

        Transform foundModel = transform.Find("TempModelCabinet");
        if (foundModel != null)
            return foundModel;

        return transform;
    }

    static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
            return child;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go.transform;
    }

    void BuildUi()
    {
        ClearChildren(panelRoot);

        Image rootImage = AddImage(panelRoot.gameObject, MainBg);
        rootImage.raycastTarget = false;

        VerticalLayoutGroup pageLayout = panelRoot.GetComponent<VerticalLayoutGroup>();
        if (pageLayout == null)
            pageLayout = panelRoot.gameObject.AddComponent<VerticalLayoutGroup>();
        pageLayout.padding = new RectOffset(0, 0, 0, 0);
        pageLayout.spacing = 0f;
        pageLayout.childControlWidth = true;
        pageLayout.childControlHeight = true;
        pageLayout.childForceExpandWidth = true;
        pageLayout.childForceExpandHeight = false;

        BuildHeader(panelRoot);
        RectTransform body = BuildBody(panelRoot);
        BuildFooter(panelRoot);

        RectTransform sidebar = BuildSidebar(body);
        contentRoot = BuildContent(body);

        cabinetsPage = BuildPage(contentRoot, "CabinetsPage");
        addedPage = BuildPage(contentRoot, "AddedCabinetsPage");
        helpPage = BuildPage(contentRoot, "HelpPage");

        BuildCabinetsPage(cabinetsPage);
        BuildAddedCabinetsPage(addedPage);
        BuildHelpPage(helpPage);

        AddBoxBorder(panelRoot, 1f, NeonGreen);
        ApplyUiLayerRecursive(canvas.transform);
        ApplyActivePage();
    }

    void BuildHeader(RectTransform parent)
    {
        RectTransform header = CreateLayoutRect("Header", parent, 40f, -1f);
        AddImage(header.gameObject, HeaderBg);
        AddBoxBorder(header, 1f, NeonGreen, top: false, bottom: true, left: false, right: false);

        VerticalLayoutGroup layout = header.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 2, 2);
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 0f;

        Text title = CreateText("MR CONFIGURATION", 13, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter);
        title.transform.SetParent(header, false);
        AddLayout(title.gameObject, -1f, 16f);

        Text subtitle = CreateText("Configuration for the MR environment", 8, FontStyle.Normal, GrayText, TextAnchor.MiddleCenter);
        subtitle.transform.SetParent(header, false);
        AddLayout(subtitle.gameObject, -1f, 12f);
    }

    RectTransform BuildBody(RectTransform parent)
    {
        RectTransform body = CreateLayoutRect("Body", parent, -1f, 1f);

        HorizontalLayoutGroup layout = body.GetComponent<HorizontalLayoutGroup>();
        if (layout == null)
            layout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        return body;
    }

    void BuildFooter(RectTransform parent)
    {
        RectTransform footer = CreateLayoutRect("Footer", parent, 24f, -1f);
        AddImage(footer.gameObject, FooterBg);
        AddBoxBorder(footer, 1f, NeonGreen, top: true, bottom: false, left: false, right: false);

        Text text = CreateText("© 1990 MR CONFIGURATION • Age Of Joy", 9, FontStyle.Normal, GrayText, TextAnchor.MiddleCenter);
        text.transform.SetParent(footer, false);
        Stretch(text.rectTransform);
    }

    RectTransform BuildSidebar(RectTransform parent)
    {
        RectTransform sidebar = CreateLayoutRect("Sidebar", parent, -1f, 0.15f);
        AddImage(sidebar.gameObject, SidebarBg);
        AddBoxBorder(sidebar, 1f, NeonGreen, top: false, bottom: false, left: false, right: true);

        VerticalLayoutGroup layout = sidebar.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(8, 6, 8, 6);
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        menuTexts.Clear();
        CreateMenuText(sidebar, "Cabinets");
        CreateMenuText(sidebar, "Added Cabinets");
        CreateMenuText(sidebar, "Help");

        return sidebar;
    }

    RectTransform BuildContent(RectTransform parent)
    {
        RectTransform content = CreateLayoutRect("Content", parent, -1f, 0.85f);
        AddImage(content.gameObject, ContentBg);

        return content;
    }

    void CreateMenuText(RectTransform parent, string label)
    {
        Text text = CreateText(label, 11, FontStyle.Normal, NeonGreen, TextAnchor.MiddleLeft);
        text.transform.SetParent(parent, false);

        LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 14f;
        layout.minHeight = 14f;
        layout.flexibleHeight = 0f;
        layout.flexibleWidth = 1f;

        menuTexts.Add(text);
    }

    RectTransform BuildPage(RectTransform parent, string name)
    {
        RectTransform page = CreateRect(name, parent);
        Stretch(page);
        return page;
    }

    void BuildCabinetsPage(RectTransform page)
    {
        RectTransform windowContent = BuildWindow(page, "CABINETS");
        cabinetsScroll = BuildScrollTable(windowContent, out tableBody);

        RectTransform header = CreateRow(tableBody, "CabinetsHeader", 28f, NeonGreenDark);
        BuildCabinetHeader(header);
    }

    void BuildAddedCabinetsPage(RectTransform page)
    {
        RectTransform windowContent = BuildWindow(page, "ADDED CABINETS");
        addedScroll = BuildScrollTable(windowContent, out addedTableBody);

        RectTransform header = CreateRow(addedTableBody, "AddedHeader", 28f, NeonGreenDark);
        AddBoxBorder(header, 1f, TableBorder, top: true, bottom: false, left: false, right: false);
        CreateCell(header, "", 0.18f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, NeonGreenDark, drawLeftBorder: true);
        CreateCell(header, "Name", 0.42f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleLeft, NeonGreenDark);
        CreateCell(header, "Position", 0.28f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleLeft, NeonGreenDark);
        CreateCell(header, "Status", 0.12f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, NeonGreenDark);
    }

    void BuildHelpPage(RectTransform page)
    {
        RectTransform windowContent = BuildWindow(page, "HELP");

        Text text = CreateText(
            "Use Up / Down to navigate the focused list (sidebar pages or cabinet table).\n\n" +
            "Press Right to move focus from the sidebar to the cabinet table.\n" +
            "Press Left to move focus back from the table to the sidebar.\n\n" +
            "Press Enter or the confirm button to add or remove the selected cabinet from the MR environment.\n\n" +
            "This interface uses a retro 90s black and neon green style.",
            11,
            FontStyle.Normal,
            LightText,
            TextAnchor.UpperLeft
        );

        text.transform.SetParent(windowContent, false);
        Stretch(text.rectTransform, new Vector2(10, 10), new Vector2(-10, -10));
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
    }

    RectTransform BuildWindow(RectTransform parent, string title)
    {
        RectTransform window = CreateRect("Window", parent);
        Stretch(window, new Vector2(6, 6), new Vector2(-6, -6));
        AddImage(window.gameObject, WindowBg);
        AddBoxBorder(window, 1f, NeonGreen);

        VerticalLayoutGroup layout = window.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform header = CreateLayoutRect("WindowHeader", window, 22f, -1f);
        AddImage(header.gameObject, NeonGreenDark);
        AddBoxBorder(header, 1f, NeonGreen, top: false, bottom: true, left: false, right: false);

        Text titleText = CreateText(title, 11, FontStyle.Bold, NeonGreen, TextAnchor.MiddleLeft);
        titleText.transform.SetParent(header, false);
        Stretch(titleText.rectTransform, new Vector2(8, 0), new Vector2(-8, 0));

        RectTransform content = CreateLayoutRect("WindowContent", window, -1f, 1f);
        AddImage(content.gameObject, WindowBg);

        return content;
    }

    ScrollRect BuildScrollTable(RectTransform parent, out RectTransform content)
    {
        GameObject scrollGo = new GameObject("ScrollTable", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        ApplyUiLayer(scrollGo);
        scrollGo.transform.SetParent(parent, false);
        RectTransform scrollRect = scrollGo.GetComponent<RectTransform>();
        Stretch(scrollRect, new Vector2(6, 6), new Vector2(-6, -6));
        AddImage(scrollGo, WindowBg);

        GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        ApplyUiLayer(viewportGo);
        viewportGo.transform.SetParent(scrollRect, false);
        RectTransform viewport = viewportGo.GetComponent<RectTransform>();
        Stretch(viewport);
        AddImage(viewportGo, WindowBg);
        viewportGo.GetComponent<Mask>().showMaskGraphic = true;

        GameObject contentGo = new GameObject("TableBody", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        ApplyUiLayer(contentGo);
        contentGo.transform.SetParent(viewport, false);
        content = contentGo.GetComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = contentGo.GetComponent<VerticalLayoutGroup>();
        layout.spacing = 0f;
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;

        return scroll;
    }

    void BuildCabinetHeader(RectTransform row)
    {
        AddBoxBorder(row, 1f, TableBorder, top: true, bottom: false, left: false, right: false);
        CreateCell(row, "", 0.18f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, NeonGreenDark, drawLeftBorder: true);
        CreateCell(row, "Name", 0.42f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleLeft, NeonGreenDark);
        CreateCell(row, "Added", 0.18f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, NeonGreenDark);
        CreateCell(row, "Action", 0.22f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, NeonGreenDark);
    }

    void RefreshAll()
    {
        RefreshCabinets();
        RefreshAddedCabinets();
        ApplyActivePage();
    }

    void RefreshCabinets()
    {
        if (tableBody == null)
            return;

        StopPreviewVideo();
        previewBoundRow = null;

        ClearTableExceptHeader(tableBody);
        rows.Clear();

        if (thumbnailLoadCoroutine != null)
        {
            StopCoroutine(thumbnailLoadCoroutine);
            thumbnailLoadCoroutine = null;
        }
        thumbnailLoadQueue.Clear();

        registry = registry ?? EnsureRegistry();
        registry.EnsureLayoutLoaded();

        List<string> catalog = MRLayoutRegistry.GetCatalogCabinetNames();

        if (catalog.Count == 0)
        {
            RectTransform row = CreateRow(tableBody, "EmptyRow", 32f, WindowBg);
            CreateCell(row, "", 0.18f, 10, FontStyle.Normal, LightText, TextAnchor.MiddleCenter, WindowBg, drawLeftBorder: true);
            CreateCell(row, NotFoundCabinetMessage, 0.42f, 11, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, WindowBg);
            CreateCell(row, "-", 0.18f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, WindowBg);
            CreateCell(row, "", 0.22f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, WindowBg);
            ApplyUiLayerRecursive(tableBody);
            Canvas.ForceUpdateCanvases();
            return;
        }

        for (int i = 0; i < catalog.Count; i++)
        {
            string cabinetName = catalog[i];
            bool inScene = registry.IsCabinetInScene(cabinetName);
            CabinetRowView row = CreateCabinetRow(cabinetName, inScene);
            rows.Add(row);
            if (enableVideoPreview)
                ConfigManager.WriteConsole($"{LogPrefix} row '{cabinetName}' video={(string.IsNullOrEmpty(row.VideoPath) ? "(none)" : row.VideoPath)} marquee={(string.IsNullOrEmpty(row.MarqueePath) ? "(none)" : row.MarqueePath)}");
            QueueRowThumbnailLoad(row);
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, rows.Count - 1);
        UpdateRowVisuals();
        ApplyUiLayerRecursive(tableBody);
        Canvas.ForceUpdateCanvases();
    }

    void RefreshAddedCabinets()
    {
        if (addedTableBody == null)
            return;

        ClearTableExceptHeader(addedTableBody);

        registry = registry ?? EnsureRegistry();
        registry.EnsureLayoutLoaded();

        var placements = registry.Placements;

        if (placements == null || placements.Count == 0)
        {
            RectTransform row = CreateRow(addedTableBody, "EmptyAddedRow", 32f, WindowBg);
            CreateCell(row, "", 0.18f, 10, FontStyle.Normal, LightText, TextAnchor.MiddleCenter, WindowBg);
            CreateCell(row, "No cabinets added", 0.42f, 11, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, WindowBg);
            CreateCell(row, "-", 0.28f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, WindowBg);
            CreateCell(row, "-", 0.12f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, WindowBg);
            ApplyUiLayerRecursive(addedTableBody);
            return;
        }

        for (int i = 0; i < placements.Count; i++)
        {
            var placement = placements[i];
            if (placement == null)
                continue;

            float x = placement.Position != null ? placement.Position.X : 0f;
            float y = placement.Position != null ? placement.Position.Y : 0f;
            float z = placement.Position != null ? placement.Position.Z : 0f;

            RectTransform row = CreateRow(addedTableBody, "AddedCabinetRow", 32f, WindowBg);
            CreateCell(row, "", 0.18f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleCenter, WindowBg, drawLeftBorder: true);
            CreateCell(row, placement.DisplayLabel, 0.42f, 10, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, WindowBg);
            CreateCell(row, $"X:{x:F1} Y:{y:F1} Z:{z:F1}", 0.28f, 10, FontStyle.Normal, Color.white, TextAnchor.MiddleLeft, WindowBg);
            CreateCell(row, "Active", 0.12f, 10, FontStyle.Bold, NeonGreen, TextAnchor.MiddleCenter, WindowBg);
        }

        ApplyUiLayerRecursive(addedTableBody);
    }

    CabinetRowView CreateCabinetRow(string cabinetName, bool inScene)
    {
        RectTransform row = CreateRow(tableBody, "CabinetRow", 36f, WindowBg);

        RawImage preview;
        Text previewFallback;

        string videoPath = null;
        string marqueePath = null;
        if (enableVideoPreview)
            ResolveCabinetAssets(cabinetName, out videoPath, out marqueePath);

        string previewHint = videoPath ?? marqueePath;
        CreatePreviewCell(row, 0.18f, previewHint, out preview, out previewFallback);

        Text nameText = CreateCell(row, cabinetName, 0.42f, 10, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, WindowBg);
        Text addedText = CreateCell(row, inScene ? "Yes" : "No", 0.18f, 10, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter, WindowBg);
        Text actionText = CreateButtonCell(row, inScene ? "Remove" : "Add", 0.22f, inScene ? ButtonRemove : ButtonAdd);

        return new CabinetRowView
        {
            CabinetDBName = cabinetName,
            InScene = inScene,
            VideoPath = videoPath,
            MarqueePath = marqueePath,
            Background = row.GetComponent<Image>(),
            NameText = nameText,
            AddedText = addedText,
            ActionText = actionText,
            Preview = preview,
            PreviewFallback = previewFallback
        };
    }

    RectTransform CreateRow(RectTransform parent, string name, float preferredHeight, Color background)
    {
        RectTransform row = CreateLayoutRect(name, parent, preferredHeight, -1f);
        AddImage(row.gameObject, background);
        AddBoxBorder(row, 1f, TableBorder, top: false, bottom: true, left: false, right: false);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        return row;
    }

    Text CreateCell(RectTransform row, string text, float flexibleWidth, int fontSize, FontStyle style, Color color, TextAnchor anchor, Color background, bool drawLeftBorder = false)
    {
        RectTransform cell = CreateLayoutRect("Cell", row, -1f, flexibleWidth);
        AddImage(cell.gameObject, background);
        AddBoxBorder(cell, 1f, TableBorder, top: false, bottom: false, left: drawLeftBorder, right: true);

        Text label = CreateText(text, fontSize, style, color, anchor);
        label.transform.SetParent(cell, false);
        Stretch(label.rectTransform, new Vector2(4, 2), new Vector2(-4, -2));

        return label;
    }

    Text CreateButtonCell(RectTransform row, string text, float flexibleWidth, Color buttonColor)
    {
        RectTransform cell = CreateLayoutRect("ActionCell", row, -1f, flexibleWidth);
        AddImage(cell.gameObject, WindowBg);
        AddBoxBorder(cell, 1f, TableBorder, top: false, bottom: false, left: false, right: true);

        RectTransform button = CreateRect("Button", cell);
        button.anchorMin = new Vector2(0.08f, 0.18f);
        button.anchorMax = new Vector2(0.92f, 0.82f);
        button.offsetMin = Vector2.zero;
        button.offsetMax = Vector2.zero;
        AddImage(button.gameObject, buttonColor);
        AddBoxBorder(button, 1f, TableBorder, top: true, bottom: true, left: true, right: true);

        Text label = CreateText(text, 10, FontStyle.Bold, Color.black, TextAnchor.MiddleCenter);
        label.transform.SetParent(button, false);
        Stretch(label.rectTransform);

        return label;
    }

    void CreatePreviewCell(RectTransform row, float flexibleWidth, string videoPath, out RawImage preview, out Text fallback)
    {
        RectTransform cell = CreateLayoutRect("PreviewCell", row, -1f, flexibleWidth);
        AddImage(cell.gameObject, Color.black);
        AddBoxBorder(cell, 1f, TableBorder, top: false, bottom: false, left: true, right: true);

        GameObject previewGo = new GameObject("Preview", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        ApplyUiLayer(previewGo);
        previewGo.transform.SetParent(cell, false);
        preview = previewGo.GetComponent<RawImage>();
        preview.color = Color.black;
        Stretch(preview.rectTransform, new Vector2(3, 3), new Vector2(-3, -3));

        fallback = CreateText(string.IsNullOrEmpty(videoPath) ? "no video" : "...", 8, FontStyle.Italic, LightText, TextAnchor.MiddleCenter);
        fallback.transform.SetParent(cell, false);
        Stretch(fallback.rectTransform);
    }

    void QueueRowThumbnailLoad(CabinetRowView row)
    {
        if (!enableVideoPreview || row == null)
            return;

        if (TryApplyRowThumbnail(row))
            return;

        if (string.IsNullOrEmpty(row.VideoPath))
        {
            SetPreviewPlaceholder(row, "no preview");
            return;
        }

        thumbnailLoadQueue.Enqueue(row);
        if (thumbnailLoadCoroutine == null)
            thumbnailLoadCoroutine = StartCoroutine(ProcessThumbnailLoadQueue());
    }

    bool TryApplyRowThumbnail(CabinetRowView row, bool force = false)
    {
        if (row == null || row.Preview == null)
            return false;

        if (string.IsNullOrEmpty(row.VideoPath) && string.IsNullOrEmpty(row.MarqueePath))
        {
            SetPreviewPlaceholder(row, "no preview");
            return true;
        }

        if (!force && previewRenderTexture != null && row.Preview.texture == previewRenderTexture)
            return true;

        Texture2D thumb = GetOrLoadThumbnailTexture(row);
        if (thumb == null)
            return false;

        row.Preview.texture = thumb;
        row.Preview.color = Color.white;
        if (row.PreviewFallback != null)
            row.PreviewFallback.enabled = false;
        return true;
    }

    void RestoreRowThumbnail(CabinetRowView row)
    {
        if (row == null)
            return;

        if (!TryApplyRowThumbnail(row, force: true))
            SetPreviewPlaceholder(row, string.IsNullOrEmpty(row.VideoPath) ? "no video" : "...");
    }

    void SetPreviewPlaceholder(CabinetRowView row, string message)
    {
        if (row == null || row.Preview == null)
            return;

        row.Preview.texture = null;
        row.Preview.color = Color.black;
        if (row.PreviewFallback != null)
        {
            row.PreviewFallback.enabled = true;
            row.PreviewFallback.text = message;
        }
    }

    static string GetThumbnailCacheKey(CabinetRowView row) =>
        row?.CabinetDBName;

    static string GetThumbnailDiskPath(CabinetRowView row)
    {
        if (row == null || string.IsNullOrEmpty(row.CabinetDBName))
            return null;

        return Path.Combine(ConfigManager.CabinetsDB, row.CabinetDBName, "mr-catalog-preview.png");
    }

    Texture2D GetOrLoadThumbnailTexture(CabinetRowView row)
    {
        if (row == null)
            return null;

        string cacheKey = GetThumbnailCacheKey(row);
        if (string.IsNullOrEmpty(cacheKey))
            return null;

        if (previewThumbnailCache.TryGetValue(cacheKey, out Texture2D cached) && cached != null)
            return cached;

        Texture2D fromMarquee = LoadPngToTexture(row.MarqueePath, "Marquee_" + row.CabinetDBName, minBytes: 0);
        if (fromMarquee != null)
        {
            previewThumbnailCache[cacheKey] = fromMarquee;
            return fromMarquee;
        }

        Texture2D fromCache = LoadPngToTexture(GetThumbnailDiskPath(row), "Thumb_" + row.CabinetDBName, minBytes: 2048);
        if (fromCache != null)
        {
            previewThumbnailCache[cacheKey] = fromCache;
            return fromCache;
        }

        return null;
    }

    Texture2D LoadPngToTexture(string pngPath, string textureName, int minBytes = 0)
    {
        if (string.IsNullOrEmpty(pngPath) || !File.Exists(pngPath))
            return null;

        try
        {
            var info = new FileInfo(pngPath);
            if (minBytes > 0 && info.Length < minBytes)
            {
                ConfigManager.WriteConsole($"{LogPrefix} skipping small cache png {pngPath} ({info.Length} bytes)");
                try { File.Delete(pngPath); } catch { }
                return null;
            }

            byte[] data = File.ReadAllBytes(pngPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!tex.LoadImage(data))
            {
                Destroy(tex);
                return null;
            }
            tex.name = textureName;
            return tex;
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} png load failed: {pngPath} — {e.Message}");
            return null;
        }
    }

    void SaveThumbnailToDisk(CabinetRowView row, Texture2D texture)
    {
        string pngPath = GetThumbnailDiskPath(row);
        if (string.IsNullOrEmpty(pngPath) || texture == null)
            return;

        try
        {
            string dir = Path.GetDirectoryName(pngPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllBytes(pngPath, texture.EncodeToPNG());
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} thumbnail save failed: {pngPath} — {e.Message}");
        }
    }

    IEnumerator ProcessThumbnailLoadQueue()
    {
        EnsureThumbnailVideoPlayer();

        while (thumbnailLoadQueue.Count > 0)
        {
            CabinetRowView row = thumbnailLoadQueue.Dequeue();
            if (row == null || row.Preview == null || string.IsNullOrEmpty(row.VideoPath))
                continue;

            string path = row.VideoPath;
            string cabinetName = row.CabinetDBName;

            if (TryApplyRowThumbnail(row))
                continue;

            yield return ResetAndPrepareVideoPlayer(thumbnailVideoPlayer, path);

            if (row.Preview == null || row.VideoPath != path || row.CabinetDBName != cabinetName)
                continue;

            yield return CaptureVideoFirstFrame(thumbnailVideoPlayer);

            if (row.Preview == null || row.VideoPath != path || row.CabinetDBName != cabinetName)
                continue;

            Texture2D thumb = CaptureRenderTextureToTexture2D(thumbnailRenderTexture);
            if (thumb != null)
                ApplyCapturedThumbnail(row, thumb);
            else
                SetPreviewPlaceholder(row, "...");
        }

        thumbnailLoadCoroutine = null;
    }

    static Texture2D CaptureRenderTextureToTexture2D(RenderTexture rt)
    {
        if (rt == null || !rt.IsCreated())
            return null;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        return tex;
    }

    void ApplyCapturedThumbnail(CabinetRowView row, Texture2D thumb)
    {
        if (row == null || thumb == null)
            return;

        string cacheKey = GetThumbnailCacheKey(row);
        if (string.IsNullOrEmpty(cacheKey))
            return;

        previewThumbnailCache[cacheKey] = thumb;
        SaveThumbnailToDisk(row, thumb);

        if (row.Preview != null)
            TryApplyRowThumbnail(row);
    }

    static string ToVideoPlayerUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return null;

        string full = Path.GetFullPath(filePath);
        if (full.IndexOf("://", System.StringComparison.Ordinal) >= 0)
            return full;

        return "file:///" + full.Replace("\\", "/");
    }

    IEnumerator ResetAndPrepareVideoPlayer(VideoPlayer vp, string filePath, float timeoutSeconds = 12f)
    {
        if (vp == null || string.IsNullOrEmpty(filePath))
            yield break;

        string url = ToVideoPlayerUrl(filePath);
        vp.Stop();
        vp.url = null;
        yield return null;
        yield return null;

        vp.url = url;
        vp.Prepare();

        float deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (!vp.isPrepared && Time.realtimeSinceStartup < deadline)
            yield return null;
    }

    IEnumerator CaptureVideoFirstFrame(VideoPlayer vp, float timeoutSeconds = 8f)
    {
        if (vp == null || !vp.isPrepared)
            yield break;

        ClearRenderTexture(thumbnailRenderTexture, Color.black);

        bool gotFrame = false;
        void OnFrameReady(VideoPlayer source, long frameIdx)
        {
            gotFrame = true;
        }

        vp.sendFrameReadyEvents = true;
        vp.frameReady += OnFrameReady;
        vp.time = 0;
        vp.Play();

        float deadline = Time.realtimeSinceStartup + timeoutSeconds;
        while (!gotFrame && vp.frame < 1 && Time.realtimeSinceStartup < deadline)
            yield return null;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        vp.frameReady -= OnFrameReady;
        vp.sendFrameReadyEvents = false;
        vp.Stop();
    }

    static void ClearRenderTexture(RenderTexture rt, Color color)
    {
        if (rt == null)
            return;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, color);
        RenderTexture.active = prev;
    }

    void DetachPlayRenderTextureFromAllRows()
    {
        if (previewRenderTexture == null)
            return;

        foreach (CabinetRowView row in rows)
        {
            if (row?.Preview == null || row.Preview.texture != previewRenderTexture)
                continue;

            RestoreRowThumbnail(row);
        }
    }

    void EnsureThumbnailVideoPlayer()
    {
        if (thumbnailRenderTexture == null)
        {
            thumbnailRenderTexture = new RenderTexture(192, 108, 0, RenderTextureFormat.ARGB32)
            {
                name = "MRConfigurationUI_ThumbRT",
                useMipMap = false,
                autoGenerateMips = false
            };
            thumbnailRenderTexture.Create();
        }

        if (thumbnailVideoHost == null)
        {
            thumbnailVideoHost = new GameObject("MRConfigurationUI_ThumbnailVideo");
            thumbnailVideoHost.transform.SetParent(transform, false);
            thumbnailVideoHost.hideFlags = HideFlags.DontSave;
        }

        if (thumbnailVideoPlayer == null)
        {
            thumbnailVideoPlayer = thumbnailVideoHost.AddComponent<VideoPlayer>();
            thumbnailVideoPlayer.playOnAwake = false;
            thumbnailVideoPlayer.isLooping = false;
            thumbnailVideoPlayer.waitForFirstFrame = true;
            thumbnailVideoPlayer.skipOnDrop = true;
            thumbnailVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            thumbnailVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            thumbnailVideoPlayer.source = VideoSource.Url;
            thumbnailVideoPlayer.targetTexture = thumbnailRenderTexture;
        }
    }

    void ClearTableExceptHeader(RectTransform body)
    {
        for (int i = body.childCount - 1; i >= 1; i--)
            Destroy(body.GetChild(i).gameObject);
    }

    void ApplyActivePage()
    {
        if (cabinetsPage != null)
            cabinetsPage.gameObject.SetActive(activePage == Page.Cabinets);

        if (addedPage != null)
            addedPage.gameObject.SetActive(activePage == Page.AddedCabinets);

        if (helpPage != null)
            helpPage.gameObject.SetActive(activePage == Page.Help);

        bool sidebarFocused = activeFocus == Focus.Sidebar;

        for (int i = 0; i < menuTexts.Count; i++)
        {
            bool isActive = i == (int)activePage;
            string label = menuTexts[i].text.TrimStart('►', ' ');
            menuTexts[i].text = isActive && sidebarFocused ? "► " + label : label;
            menuTexts[i].color = isActive ? Color.white : NeonGreen;
            menuTexts[i].fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
        }

        if (activePage == Page.AddedCabinets)
            RefreshAddedCabinets();

        if (activePage != Page.Cabinets)
            StopPreviewVideo();
        else
            UpdateRowVisuals();
    }

    void SetPagePrevious()
    {
        activePage = (Page)(((int)activePage + 2) % 3);
        ApplyActivePage();
        navCooldown = navRepeatDelay;
    }

    void SetPageNext()
    {
        activePage = (Page)(((int)activePage + 1) % 3);
        ApplyActivePage();
        navCooldown = navRepeatDelay;
    }

    void MoveSelection(int delta)
    {
        if (rows.Count == 0)
            return;

        selectedIndex += delta;

        if (selectedIndex < 0)
            selectedIndex = rows.Count - 1;
        else if (selectedIndex >= rows.Count)
            selectedIndex = 0;

        UpdateRowVisuals();
        ScrollToSelected();
        navCooldown = navRepeatDelay;
    }

    void UpdateRowVisuals()
    {
        CabinetRowView selectedRow = null;

        for (int i = 0; i < rows.Count; i++)
        {
            CabinetRowView row = rows[i];
            bool selected = i == selectedIndex;

            if (row.Background != null)
                row.Background.color = selected ? SelectedBg : WindowBg;

            string caret = selected && activeFocus == Focus.Content ? "► " : (selected ? "▸ " : "");
            row.NameText.text = caret + row.CabinetDBName;
            row.AddedText.text = row.InScene ? "Yes" : "No";
            row.ActionText.text = row.InScene ? "Remove" : "Add";

            if (selected)
                selectedRow = row;
        }

        UpdatePreviewBinding(selectedRow);
    }

    void ScrollToSelected()
    {
        if (cabinetsScroll == null || tableBody == null || rows.Count == 0)
            return;

        Canvas.ForceUpdateCanvases();

        RectTransform rowRect = rows[selectedIndex].Background.rectTransform;
        RectTransform viewport = cabinetsScroll.viewport;

        float contentHeight = tableBody.rect.height;
        float viewportHeight = viewport.rect.height;

        if (contentHeight <= viewportHeight)
            return;

        float rowTop = -rowRect.anchoredPosition.y;
        float rowBottom = rowTop + rowRect.rect.height;
        float scrollOffset = tableBody.anchoredPosition.y;

        if (rowTop < scrollOffset)
            scrollOffset = rowTop;
        else if (rowBottom > scrollOffset + viewportHeight)
            scrollOffset = rowBottom - viewportHeight;

        float maxOffset = contentHeight - viewportHeight;
        tableBody.anchoredPosition = new Vector2(tableBody.anchoredPosition.x, Mathf.Clamp(scrollOffset, 0f, maxOffset));
    }

    void ExecuteSelectedRow()
    {
        if (selectedIndex < 0 || selectedIndex >= rows.Count)
            return;

        CabinetRowView row = rows[selectedIndex];

        if (row.InScene)
        {
            if (registry.TryRemoveCabinetFromScene(row.CabinetDBName))
                ConfigManager.WriteConsole($"{LogPrefix} removed {row.CabinetDBName}");
        }
        else
        {
            ComputeSpawnPose(out Vector3 worldPos, out Quaternion worldRot);

            if (registry.TryAddCabinetToScene(row.CabinetDBName, mrSpaceOrigin, worldPos, worldRot))
                ConfigManager.WriteConsole($"{LogPrefix} added {row.CabinetDBName}");
        }

        RefreshAll();
    }

    void ComputeSpawnPose(out Vector3 worldPos, out Quaternion worldRot)
    {
        Transform view = Camera.main != null ? Camera.main.transform : transform;

        Vector3 forward = view.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        worldPos = view.position + forward * spawnDistanceMeters;
        worldPos.y = mrSpaceOrigin.position.y + spawnYOffsetMeters;
        worldRot = Quaternion.LookRotation(-forward, Vector3.up);
    }

    void ResolveCabinetAssets(string cabinetName, out string videoPath, out string marqueePath)
    {
        videoPath = null;
        marqueePath = null;

        if (string.IsNullOrEmpty(cabinetName))
            return;

        try
        {
            string cabinetPath = Path.GetFullPath(Path.Combine(ConfigManager.CabinetsDB, cabinetName));

            if (!Directory.Exists(cabinetPath))
                return;

            CabinetInformation info = CabinetInformation.fromYaml(cabinetPath, cache: false);

            if (info == null)
                return;

            if (info.video != null && !string.IsNullOrEmpty(info.video.file))
            {
                string candidate = Path.GetFullPath(info.getPath(info.video.file));
                if (File.Exists(candidate))
                    videoPath = candidate;
            }

            marqueePath = ResolveMarqueeArtPath(info);
        }
        catch (System.Exception e)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} asset resolve failed for {cabinetName}: {e.Message}");
        }
    }

    static string ResolveMarqueeArtPath(CabinetInformation info)
    {
        if (info == null || info.Parts == null)
            return null;

        foreach (var part in info.Parts)
        {
            if (part == null || part.art == null || string.IsNullOrEmpty(part.art.file))
                continue;

            bool isMarquee = string.Equals(part.type, "marquee", System.StringComparison.OrdinalIgnoreCase)
                          || string.Equals(part.name, "marquee", System.StringComparison.OrdinalIgnoreCase);
            if (!isMarquee)
                continue;

            string artPath = Path.GetFullPath(info.getPath(part.art.file));
            if (File.Exists(artPath))
                return artPath;
        }

        return null;
    }

    void UpdatePreviewBinding(CabinetRowView selectedRow)
    {
        if (!enableVideoPreview)
        {
            StopPreviewVideo();
            return;
        }

        if (previewBoundRow != null && previewBoundRow != selectedRow)
            RestoreRowThumbnail(previewBoundRow);

        previewBoundRow = selectedRow;

        if (selectedRow == null || string.IsNullOrEmpty(selectedRow.VideoPath))
        {
            StopPreviewVideo();
            if (selectedRow != null)
                SetPreviewPlaceholder(selectedRow, "no video");
            return;
        }

        TryApplyRowThumbnail(selectedRow);
        PlayPreviewVideo(selectedRow);
    }

    void PlayPreviewVideo(CabinetRowView row)
    {
        if (previewPlayCoroutine != null)
        {
            StopCoroutine(previewPlayCoroutine);
            previewPlayCoroutine = null;
        }

        if (row == null || string.IsNullOrEmpty(row.VideoPath))
            return;

        previewPlayCoroutine = StartCoroutine(PlayPreviewVideoRoutine(row));
    }

    IEnumerator PlayPreviewVideoRoutine(CabinetRowView row)
    {
        EnsurePreviewVideoPlayer();

        if (previewVideoPlayer == null || previewRenderTexture == null || row == null)
            yield break;

        string path = row.VideoPath;
        DetachPlayRenderTextureFromAllRows();

        if (row.PreviewFallback != null)
            row.PreviewFallback.enabled = false;

        if (previewCurrentPath == path && previewVideoPlayer.isPlaying)
        {
            if (row.Preview != null)
            {
                row.Preview.texture = previewRenderTexture;
                row.Preview.color = Color.white;
            }
            yield break;
        }

        previewCurrentPath = path;
        yield return ResetAndPrepareVideoPlayer(previewVideoPlayer, path);

        if (previewBoundRow != row || row.Preview == null || row.VideoPath != path)
            yield break;

        previewVideoPlayer.isLooping = true;
        previewVideoPlayer.Play();

        row.Preview.texture = previewRenderTexture;
        row.Preview.color = Color.white;
        if (row.PreviewFallback != null)
            row.PreviewFallback.enabled = false;

        previewPlayCoroutine = null;
    }

    void StopPreviewVideo()
    {
        previewCurrentPath = null;

        if (previewPlayCoroutine != null)
        {
            StopCoroutine(previewPlayCoroutine);
            previewPlayCoroutine = null;
        }

        if (previewVideoPlayer != null)
        {
            previewVideoPlayer.Stop();
            previewVideoPlayer.url = null;
        }

        if (previewBoundRow != null)
            RestoreRowThumbnail(previewBoundRow);
    }

    void EnsurePreviewVideoPlayer()
    {
        if (previewRenderTexture == null)
        {
            previewRenderTexture = new RenderTexture(192, 108, 0, RenderTextureFormat.ARGB32)
            {
                name = "MRConfigurationUI_PreviewRT",
                useMipMap = false,
                autoGenerateMips = false
            };
            previewRenderTexture.Create();
        }

        if (playVideoHost == null)
        {
            playVideoHost = new GameObject("MRConfigurationUI_PlayVideo");
            playVideoHost.transform.SetParent(transform, false);
            playVideoHost.hideFlags = HideFlags.DontSave;
        }

        if (previewVideoPlayer == null)
        {
            previewVideoPlayer = playVideoHost.AddComponent<VideoPlayer>();
            previewVideoPlayer.playOnAwake = false;
            previewVideoPlayer.isLooping = true;
            previewVideoPlayer.waitForFirstFrame = true;
            previewVideoPlayer.skipOnDrop = true;
            previewVideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            previewVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            previewVideoPlayer.source = VideoSource.Url;
            previewVideoPlayer.targetTexture = previewRenderTexture;
        }
    }

    MRLayoutRegistry EnsureRegistry()
    {
        if (MRLayoutRegistry.Instance != null)
            return MRLayoutRegistry.Instance;

        GameObject go = new GameObject("MRLayoutRegistry_TestUI");
        return go.AddComponent<MRLayoutRegistry>();
    }

    Transform EnsureMrSpaceOrigin()
    {
        if (MixedRealityManager.Instance != null && MixedRealityManager.Instance.MRSpaceOrigin != null)
            return MixedRealityManager.Instance.MRSpaceOrigin;

        Transform existing = transform.Find("MRSpaceOrigin_TestUI");

        if (existing != null)
            return existing;

        GameObject go = new GameObject("MRSpaceOrigin_TestUI");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        return go.transform;
    }

    bool WasMoveUp()
    {
        return navCooldown <= 0f && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W));
    }

    bool WasMoveDown()
    {
        return navCooldown <= 0f && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S));
    }

    bool WasMoveLeft()
    {
        return navCooldown <= 0f && (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A));
    }

    bool WasMoveRight()
    {
        return navCooldown <= 0f && (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D));
    }

    bool IsMoveUpHeld()
    {
        return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) || ReadStickY() > 0.55f;
    }

    bool IsMoveDownHeld()
    {
        return Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) || ReadStickY() < -0.55f;
    }

    bool IsMoveLeftHeld()
    {
        return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A) || ReadStickX() < -0.55f;
    }

    bool IsMoveRightHeld()
    {
        return Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D) || ReadStickX() > 0.55f;
    }

    static float ReadStickX()
    {
#if UNITY_EDITOR
        return Input.GetAxisRaw("Horizontal");
#else
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        return stick.x;
#endif
    }

    static float ReadStickY()
    {
#if UNITY_EDITOR
        return Input.GetAxisRaw("Vertical");
#else
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        return stick.y;
#endif
    }

    static bool WasConfirmPressed()
    {
#if UNITY_EDITOR
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton0);
#else
        return OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
#endif
    }

    RectTransform CreateLayoutRect(string name, Transform parent, float preferredHeight, float flexibleWidth)
    {
        RectTransform rect = CreateRect(name, parent);
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();

        if (preferredHeight > 0f)
            layout.preferredHeight = preferredHeight;

        if (flexibleWidth > 0f)
            layout.flexibleWidth = flexibleWidth;
        else if (Mathf.Approximately(flexibleWidth, -1f))
            layout.flexibleWidth = 1f;

        if (Mathf.Approximately(preferredHeight, -1f))
            layout.flexibleHeight = 1f;

        return rect;
    }

    RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ApplyUiLayer(go);
        return go.GetComponent<RectTransform>();
    }

    Text CreateText(string value, int size, FontStyle style, Color color, TextAnchor anchor)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        ApplyUiLayer(go);
        Text text = go.GetComponent<Text>();
        text.font = uiFont;
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    static Image AddImage(GameObject go, Color color)
    {
        Image image = go.GetComponent<Image>();

        if (image == null)
            image = go.AddComponent<Image>();

        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static void AddBoxBorder(RectTransform target, float thickness, Color color)
    {
        AddBoxBorder(target, thickness, color, top: true, bottom: true, left: true, right: true);
    }

    static void AddBoxBorder(RectTransform target, float thickness, Color color, bool top, bool bottom, bool left, bool right)
    {
        if (top)    CreateBorderLine(target, "Border_Top",    new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, thickness), color);
        if (bottom) CreateBorderLine(target, "Border_Bottom", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, thickness), color);
        if (left)   CreateBorderLine(target, "Border_Left",   new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(thickness, 0), color);
        if (right)  CreateBorderLine(target, "Border_Right",  new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(thickness, 0), color);
    }

    static void CreateBorderLine(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.transform.SetAsLastSibling();
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = Vector2.zero;
        go.GetComponent<LayoutElement>().ignoreLayout = true;
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    static void AddLayout(GameObject go, float flexibleWidth, float preferredHeight)
    {
        LayoutElement layout = go.GetComponent<LayoutElement>();

        if (layout == null)
            layout = go.AddComponent<LayoutElement>();

        if (flexibleWidth > 0f)
            layout.flexibleWidth = flexibleWidth;
        else if (Mathf.Approximately(flexibleWidth, -1f))
            layout.flexibleWidth = 1f;

        if (preferredHeight > 0f)
            layout.preferredHeight = preferredHeight;
    }

    static void Stretch(RectTransform rect)
    {
        Stretch(rect, Vector2.zero, Vector2.zero);
    }

    static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            DestroyImmediate(parent.GetChild(i).gameObject);
    }
}
