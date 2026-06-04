/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using Meta.XR.MRUtilityKit;
using UnityEngine;
using UnityEngine.UI;

/// <summary>World-space panel in front of the player with MRUK room scan results.</summary>
public class MRRoomInfoUI : MonoBehaviour
{
    const string LogPrefix = "[MRRoomInfoUI]";
    const string NotFoundMessage = "eu não achei";

    public static MRRoomInfoUI Instance { get; private set; }

    [SerializeField] float distanceMeters = 1.1f;
    [SerializeField] float heightOffsetMeters = -0.05f;
    [SerializeField] Vector2 panelSizePixels = new Vector2(520f, 320f);
    [SerializeField] float worldScale = 0.001f;
    [SerializeField] bool facePlayerEveryFrame = true;

    Canvas canvas;
    RectTransform canvasRect;
    Text bodyText;
    bool visible;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
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
    }

    void LateUpdate()
    {
        if (!visible || canvas == null)
            return;

        Transform player = ResolvePlayerTransform();
        if (player == null)
            return;

        PlaceInFrontOfPlayer(player);
    }

    public void Show()
    {
        if (!MRRuntimeSettings.ShowRoomAnchorInfoCanvas)
        {
            Hide();
            return;
        }

        RefreshContent();
        visible = true;
        if (canvas != null)
            canvas.gameObject.SetActive(true);

        Transform player = ResolvePlayerTransform();
        if (player != null)
            PlaceInFrontOfPlayer(player);

        ConfigManager.WriteConsole($"{LogPrefix} shown");
    }

    public void Hide()
    {
        visible = false;
        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }

    public void RefreshContent()
    {
        if (bodyText == null)
            return;

        bodyText.text = BuildInfoText();
    }

    string BuildInfoText()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!MRScenePermissions.IsGranted)
        {
            return NotFoundMessage + "\n\n" +
                "Permissão da sala negada.\n" +
                "Ajustes > Privacidade > Dados espaciais\n" +
                "e permita para o Age of Joy.";
        }
#endif

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        MRUKRoom room = surfaces != null ? surfaces.CurrentRoom : null;

        if (!IsRoomFound(room, surfaces))
        {
            string extra = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            if (MRScenePermissions.IsGranted)
            {
                extra = "\n\n" + MRSceneLoadState.LastLoadMessage;
                if (MRSceneLoadState.LastLoadResult == MRUK.LoadDeviceResult.NoRoomsFound)
                    extra += "\n\nAo entrar em MR, conclua o Space Setup\nquando o Quest pedir para mapear a sala.";
            }
#endif
            return NotFoundMessage + extra;
        }

        string roomName = GetRoomDisplayName(room);
        int anchorCount = room.Anchors.Count;
        int wallCount = room.WallAnchors != null ? room.WallAnchors.Count : 0;
        string source = surfaces != null ? surfaces.ProbeSource : "?";
        string floor = surfaces != null && surfaces.HasFloor ? $"sim (Y={surfaces.FloorHeight:F2}m)" : "não";
        string ceiling = surfaces != null && surfaces.HasCeiling ? $"sim (Y={surfaces.CeilingHeight:F2}m)" : "não";

        string permissionLine = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        permissionLine = $"Permissão: {MRScenePermissions.StatusText}\n";
#endif

        return
            $"Nome: {roomName}\n" +
            permissionLine +
            $"Carregamento: {MRSceneLoadState.LastLoadMessage}\n" +
            $"Anchors: {anchorCount}\n" +
            $"Paredes: {wallCount}\n" +
            $"Chão: {floor}\n" +
            $"Teto: {ceiling}\n" +
            $"Fonte: {source}";
    }

    static bool IsRoomFound(MRUKRoom room, MREnvironmentSurfaces surfaces)
    {
        if (room == null)
            return false;

        if (room.Anchors == null || room.Anchors.Count == 0)
            return false;

        if (surfaces != null && surfaces.UsesMrukAnchors)
            return true;

        return room.FloorAnchor != null || room.CeilingAnchor != null || room.WallAnchors.Count > 0;
    }

    static string GetRoomDisplayName(MRUKRoom room)
    {
        if (room == null)
            return NotFoundMessage;

        string name = room.gameObject.name;
        if (string.IsNullOrWhiteSpace(name))
            return "Sala";

        return name;
    }

    void PlaceInFrontOfPlayer(Transform player)
    {
        if (canvasRect == null)
            return;

        Vector3 forward = player.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = player.forward;
        forward.Normalize();

        Vector3 position = player.position + Vector3.up * heightOffsetMeters + forward * distanceMeters;
        canvasRect.position = position;

        if (facePlayerEveryFrame)
        {
            Vector3 toPlayer = player.position - position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 0.001f)
                canvasRect.rotation = Quaternion.LookRotation(-toPlayer.normalized, Vector3.up);
        }
    }

    void BuildUi()
    {
        var canvasGo = new GameObject("MRRoomInfoCanvas");
        canvasGo.transform.SetParent(transform, false);

        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGo.AddComponent<GraphicRaycaster>().enabled = false;

        canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = panelSizePixels;
        canvasRect.localScale = Vector3.one * worldScale;

        var panelGo = new GameObject("Panel");
        panelGo.transform.SetParent(canvasRect, false);

        var panelRect = panelGo.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var panelImage = panelGo.AddComponent<Image>();
        panelImage.color = new Color(0.06f, 0.08f, 0.12f, 0.88f);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(panelRect, false);

        var textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 24f);
        textRect.offsetMax = new Vector2(-24f, -24f);

        bodyText = textGo.AddComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize = 28;
        bodyText.color = Color.white;
        bodyText.alignment = TextAnchor.UpperLeft;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Overflow;
        bodyText.text = NotFoundMessage;
    }

    static Transform ResolvePlayerTransform()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null && pc.PlayerControllerGameObject != null)
            return pc.PlayerControllerGameObject.transform;
        if (pc != null)
            return pc.transform;

        var tagged = GameObject.FindGameObjectWithTag("Player");
        if (tagged != null)
            return tagged.transform;

        if (Camera.main != null)
            return Camera.main.transform;

        return null;
    }
}
