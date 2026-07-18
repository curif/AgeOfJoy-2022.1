/*
This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or (at your option) any later version.
*/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Retro Turbo Vision–style HUD during MR placement ray (white/cyan/yellow on blue panel).
/// Prototype in Assets/ramiro/UIInformation; driven live by MRPlacementRayController.
/// </summary>
public class MRPlacementInfoHUD : MonoBehaviour
{
    const string LogPrefix = "[MRPlacementInfoHUD]";
    // Fits ~380px panel @ mono 14–16px (keep short so content stays inside the chrome).
    const int InnerWidth = 34;
    // Classic Turbo Vision / VGA text palette.
    const string ColorTitle = "FFFF55";
    const string ColorHeader = "55FFFF";
    const string ColorBody = "FFFFFF";

    public static MRPlacementInfoHUD Instance { get; private set; }

    [SerializeField] Vector2 panelSizePixels = new Vector2(380f, 320f);
    [SerializeField] float worldScale = 0.0006f;
    [SerializeField] Vector3 localOffsetFromController = new Vector3(-0.13f, -0.03f, -0.04f);

    /// <summary>Offset from the right-hand / controller. X right, Y up, Z forward (hand axes, no scale).</summary>
    public void SetLocalOffsetFromController(Vector3 localOffset)
    {
        localOffsetFromController = localOffset;
        if (visible)
            UpdatePoseAboveController();
    }

    public Vector3 LocalOffsetFromController
    {
        get => localOffsetFromController;
        set
        {
            localOffsetFromController = value;
            if (visible)
                UpdatePoseAboveController();
        }
    }
    [SerializeField] float borderThicknessPixels = 4f;
    [SerializeField] float cornerRadiusPixels = 8f;
    // Classic TV window blue (#0000AA family).
    [SerializeField] Color backgroundColor = new Color(0f, 0f, 0.67f, 1f);
    [SerializeField] Color borderColor = new Color(0.33f, 1f, 1f, 1f);
    [SerializeField] Color textColor = Color.white;
    [SerializeField] int fontSize = 16;

    Canvas canvas;
    RectTransform canvasRect;
    Image borderImage;
    Image panelImage;
    Text bodyText;
    Sprite roundedSprite;
    Transform anchorOverride;
    bool visible;

    string surfaceLabel = "FLOOR";
    string objectLabel = "OBJECT";
    float scaleValue = 1f;
    float rotationDegrees;
    bool hasValidPreview = true;
    bool showScaleControl = true;
    bool showRotateControl = true;

    public static MRPlacementInfoHUD Ensure()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("MRPlacementInfoHUD");
        Instance = go.AddComponent<MRPlacementInfoHUD>();
        DontDestroyOnLoad(go);
        return Instance;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildUi();
        Hide();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (roundedSprite != null)
        {
            if (roundedSprite.texture != null)
                Destroy(roundedSprite.texture);
            Destroy(roundedSprite);
            roundedSprite = null;
        }
    }

    void LateUpdate()
    {
        if (!visible || canvas == null)
            return;

        UpdatePoseAboveController();
    }

    public void Show(
        PlacementSurfaceType surface,
        float scale,
        float rotationDeg,
        bool allowRotate,
        bool allowScale,
        bool previewValid,
        string objectName = null)
    {
        ApplyState(surface, scale, rotationDeg, previewValid, objectName, allowRotate, allowScale);

        if (canvas == null)
            BuildUi();

        visible = true;
        if (canvas != null)
            canvas.gameObject.SetActive(true);

        RefreshText();
        UpdatePoseAboveController();
    }

    public void Refresh(
        PlacementSurfaceType surface,
        float scale,
        float rotationDeg,
        bool allowRotate,
        bool allowScale,
        bool previewValid,
        string objectName = null)
    {
        if (!visible)
            return;

        ApplyState(surface, scale, rotationDeg, previewValid, objectName, allowRotate, allowScale);
        RefreshText();
    }

    public void Hide()
    {
        visible = false;
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    /// <summary>Optional anchor for UIInformation / editor preview (e.g. AGEOfJoyHandRightPrefab).</summary>
    public void SetAnchorOverride(Transform anchor)
    {
        anchorOverride = anchor;
    }

    public void ClearAnchorOverride()
    {
        anchorOverride = null;
    }

    /// <summary>Static sample for UIInformation / Editor play mode without a live ray.</summary>
    public void ShowDemoPreview()
    {
        Show(
            PlacementSurfaceType.Floor,
            scale: 1f,
            rotationDeg: 0f,
            allowRotate: true,
            allowScale: true,
            previewValid: true,
            objectName: "Nome do Objeto");
    }

    void ApplyState(
        PlacementSurfaceType surface,
        float scale,
        float rotationDeg,
        bool previewValid,
        string objectName,
        bool allowRotate,
        bool allowScale)
    {
        surfaceLabel = surface.ToString().ToUpperInvariant();
        scaleValue = scale;
        rotationDegrees = rotationDeg;
        hasValidPreview = previewValid;
        showRotateControl = allowRotate;
        showScaleControl = allowScale;
        if (!string.IsNullOrEmpty(objectName))
            objectLabel = objectName.ToUpperInvariant();
    }

    void RefreshText()
    {
        if (bodyText == null)
            return;

        bodyText.text = BuildPanelText();
    }

    string BuildPanelText()
    {
        // ASCII frame only — Quest/Android OS fonts miss CP437 box glyphs and substitute
        // proportional fallbacks, which shifts the whole panel content to the right.
        string bar = "+" + new string('-', InnerWidth) + "+";
        string mid = "+" + new string('-', InnerWidth) + "+";
        string divider = "  " + new string('-', InnerWidth - 2);

        int scalePercent = Mathf.RoundToInt(Mathf.Max(0.01f, scaleValue) * 100f);
        float rot = Mathf.Repeat(rotationDegrees, 360f);
        string rotLabel = Mathf.RoundToInt(rot).ToString("000") + " deg";
        string displayName = string.IsNullOrEmpty(objectLabel) ? "OBJECT" : objectLabel.ToUpperInvariant();
        if (displayName.Length > InnerWidth - 2)
            displayName = displayName.Substring(0, InnerWidth - 2);

        string surface = string.IsNullOrEmpty(surfaceLabel) ? "FLOOR" : surfaceLabel.ToUpperInvariant();

        var sb = new System.Text.StringBuilder(1024);
        sb.Append(Paint(bar, ColorHeader)).Append('\n');
        sb.Append(Row(Center(displayName), ColorTitle)).Append('\n');
        sb.Append(Paint(mid, ColorHeader)).Append('\n');
        sb.Append(Row(PadColumns("CONTROLS", "ACTION", 20), ColorHeader)).Append('\n');
        sb.Append(Row(divider, ColorHeader)).Append('\n');
        if (showRotateControl)
            sb.Append(Row(PadColumns("[STICK L/R]", "ROTATE", 20), ColorBody)).Append('\n');
        if (showScaleControl)
            sb.Append(Row(PadColumns("[GRIP]+[L/R]", "SCALE", 20), ColorBody)).Append('\n');
        sb.Append(Row(PadColumns("[TRIGGER]", "CONFIRM", 20), ColorBody)).Append('\n');
        sb.Append(Row(PadColumns("[B]", "CANCEL", 20), ColorBody)).Append('\n');
        sb.Append(Row("", ColorBody)).Append('\n');
        sb.Append(Paint(mid, ColorHeader)).Append('\n');
        sb.Append(Row(Dotted("OBJECT", displayName), ColorBody)).Append('\n');
        sb.Append(Row(Dotted("SURFACE", surface), ColorBody)).Append('\n');
        if (showScaleControl)
            sb.Append(Row(Dotted("SCALE", scalePercent + "%"), ColorBody)).Append('\n');
        if (showRotateControl)
            sb.Append(Row(Dotted("ROTATION", rotLabel), ColorBody)).Append('\n');
        sb.Append(Paint(bar, ColorHeader));
        return sb.ToString();
    }

    static string Paint(string content, string hex)
    {
        return "<color=#" + hex + ">" + content + "</color>";
    }

    static string Row(string content, string hex)
    {
        if (content == null)
            content = string.Empty;
        if (content.Length > InnerWidth)
            content = content.Substring(0, InnerWidth);
        content = content.PadRight(InnerWidth);
        // Frame in cyan, content in title/body color (classic TV dialog).
        return Paint("|", ColorHeader) + Paint(content, hex) + Paint("|", ColorHeader);
    }

    static string Center(string content)
    {
        if (content == null)
            content = string.Empty;
        if (content.Length >= InnerWidth)
            return content.Substring(0, InnerWidth);

        int pad = InnerWidth - content.Length;
        int left = pad / 2;
        return new string(' ', left) + content + new string(' ', pad - left);
    }

    static string PadColumns(string left, string right, int leftWidth)
    {
        left = "  " + (left ?? string.Empty);
        right = right ?? string.Empty;
        if (left.Length < leftWidth)
            left = left.PadRight(leftWidth);
        else if (left.Length > leftWidth)
            left = left.Substring(0, leftWidth);
        return left + right;
    }

    static string Dotted(string key, string value)
    {
        const int keyWidth = 16;
        key = "  " + (key ?? string.Empty);
        if (key.Length < keyWidth)
            key = key.PadRight(keyWidth, '.');
        else
            key = key.Substring(0, keyWidth - 1) + ".";

        value = value ?? string.Empty;
        // Keep key + space + value within InnerWidth so the ASCII box stays intact.
        int maxValue = Mathf.Max(1, InnerWidth - keyWidth - 1);
        if (value.Length > maxValue)
            value = value.Substring(0, maxValue);

        return key + " " + value;
    }

    void UpdatePoseAboveController()
    {
        if (canvasRect == null)
            return;

        ResolveAnchor(out Vector3 pos, out Vector3 lookFrom);
        canvasRect.position = pos;

        Vector3 toViewer = lookFrom - pos;
        if (toViewer.sqrMagnitude > 0.0001f)
            canvasRect.rotation = Quaternion.LookRotation(-toViewer.normalized, Vector3.up);
    }

    void ResolveAnchor(out Vector3 worldPos, out Vector3 viewerPos)
    {
        Transform pointer = anchorOverride != null
            ? anchorOverride
            : ResolveRightControllerTransform();
        Transform viewer = Camera.main != null ? Camera.main.transform : null;
        viewerPos = viewer != null ? viewer.position : (pointer != null ? pointer.position : transform.position);

        if (pointer != null)
        {
            // Hand/controller bones often have non-uniform scale — avoid TransformPoint.
            worldPos = pointer.position
                + pointer.right * localOffsetFromController.x
                + pointer.up * localOffsetFromController.y
                + pointer.forward * localOffsetFromController.z;
            return;
        }

        if (viewer != null)
        {
            worldPos = viewer.position
                + viewer.right * localOffsetFromController.x
                + viewer.up * (0.05f + localOffsetFromController.y)
                + viewer.forward * (0.55f + localOffsetFromController.z);
            return;
        }

        worldPos = transform.position + localOffsetFromController;
    }

    void BuildUi()
    {
        if (canvas != null)
            return;

        GameObject canvasGo = new GameObject("PlacementInfoCanvas");
        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGo.AddComponent<GraphicRaycaster>().enabled = false;

        canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSizePixels;
        canvasRect.localScale = Vector3.one * worldScale;

        GameObject borderGo = new GameObject("Border");
        borderGo.transform.SetParent(canvasRect, false);
        RectTransform borderRect = borderGo.AddComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;
        borderImage = borderGo.AddComponent<Image>();
        borderImage.color = borderColor;
        borderImage.sprite = EnsureRoundedSprite();
        borderImage.type = Image.Type.Sliced;
        borderImage.pixelsPerUnitMultiplier = 1f;

        GameObject panelGo = new GameObject("Background");
        panelGo.transform.SetParent(borderRect, false);
        RectTransform panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        float t = Mathf.Max(1f, borderThicknessPixels);
        panelRect.offsetMin = new Vector2(t, t);
        panelRect.offsetMax = new Vector2(-t, -t);
        panelImage = panelGo.AddComponent<Image>();
        panelImage.color = backgroundColor;
        panelImage.sprite = EnsureRoundedSprite();
        panelImage.type = Image.Type.Sliced;
        panelImage.pixelsPerUnitMultiplier = 1f;
        panelGo.AddComponent<RectMask2D>();

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(panelRect, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        // Symmetric insets so the TV box is centered in the blue panel (was 16/-4 → content shifted right).
        textRect.offsetMin = new Vector2(10f, 12f);
        textRect.offsetMax = new Vector2(-10f, -12f);

        bodyText = textGo.AddComponent<Text>();
        bodyText.font = ResolveMonospaceFont();
        bodyText.fontSize = fontSize;
        bodyText.fontStyle = FontStyle.Normal;
        bodyText.color = textColor;
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.horizontalOverflow = HorizontalWrapMode.Overflow;
        bodyText.verticalOverflow = VerticalWrapMode.Truncate;
        bodyText.supportRichText = true;
        bodyText.lineSpacing = 0.95f;
        bodyText.resizeTextForBestFit = false;

        ConfigManager.WriteConsole($"{LogPrefix} panel + terminal text built font={bodyText.font?.name}");
    }

    static Font ResolveMonospaceFont()
    {
        // Prefer fonts that exist on the target OS. Consolas is Editor/Windows-only;
        // Quest/Android typically has Droid Sans Mono / Noto Sans Mono.
        string[] candidates =
#if UNITY_ANDROID && !UNITY_EDITOR
        {
            "Droid Sans Mono",
            "Noto Sans Mono",
            "DroidSansMono",
            "Courier",
            "monospace"
        };
#else
        {
            "Consolas",
            "Courier New",
            "Lucida Console",
            "Fixedsys",
            "Liberation Mono",
            "DejaVu Sans Mono",
            "Droid Sans Mono",
            "Courier"
        };
#endif

        Font font = Font.CreateDynamicFontFromOSFont(candidates, 18);
        if (font != null)
            return font;

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    Sprite EnsureRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        int size = 64;
        int radius = Mathf.Clamp(Mathf.RoundToInt(cornerRadiusPixels), 4, size / 2 - 1);
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "PlacementHudRounded";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color32[] pixels = new Color32[size * size];
        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 fill = new Color32(255, 255, 255, 255);
        float r = radius;
        float rSq = r * r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = true;
                float cx = x < radius ? r - 0.5f - x : (x >= size - radius ? x - (size - r - 0.5f) : 0f);
                float cy = y < radius ? r - 0.5f - y : (y >= size - radius ? y - (size - r - 0.5f) : 0f);
                if (x < radius || x >= size - radius)
                {
                    if (y < radius || y >= size - radius)
                        inside = (cx * cx + cy * cy) <= rSq;
                }

                pixels[y * size + x] = inside ? fill : clear;
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply(false, true);

        float border = radius;
        roundedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
        roundedSprite.name = "PlacementHudRoundedSprite";
        return roundedSprite;
    }

    static Transform ResolveRightControllerTransform()
    {
        ChangeControls controls = FindObjectOfType<ChangeControls>();
        if (controls != null && controls.RightHand != null)
            return controls.RightHand.transform;

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.xrorigin != null)
        {
            foreach (Transform t in pc.xrorigin.GetComponentsInChildren<Transform>(true))
            {
                string name = t.name.ToLowerInvariant();
                if (name.Contains("right") && (name.Contains("controller") || name.Contains("hand")))
                    return t;
            }
        }

        return null;
    }
}
