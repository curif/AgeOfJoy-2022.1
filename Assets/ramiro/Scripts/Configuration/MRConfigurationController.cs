/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LC = LibretroControlMapDictionnary;

/// <summary>
/// MR layout menus on ConfigurationCabinet CRT (GenericMenu + MRLayoutRegistry).
/// Replaces VR ConfigurationController menus and uGUI MRConfigurationUI.
/// </summary>
public class MRConfigurationController : MonoBehaviour
{
    const string LogPrefix = "[MRConfigurationController]";
    const string DefaultSkin = "c64";
    const int VisibleCabinetRows = 11;
    const int VisibleDebugRows = 10;
    const int VisibleDebugDetailRows = 16;
    const int DebugDetailWrapWidth = 38;
    const int MeshOptionCount = 3;
    const int MeshScanColorsRowIndex = 2;
    const int LightsAutoRowIndex = 0;
    const int ListRowNameWidth = 10;
    const int RoomSkinListRowNameWidth = 22;

    enum RowActionKind
    {
        Add,
        Options,
        Remove,
        Move,
        Tune,
        Decrease,
        Increase,
        ToggleOn,
        ToggleOff,
        Show,
        Hide,
        OpenDetail
    }

    enum Screen
    {
        Idle,
        NavMain,
        Cabinets,
        Adjustments,
        PhoneBooth,
        Environment,
        Mesh,
        Lights,
        Posters,
        RoomSkins,
        PlacedInstances,
        LightTune,
        Debug,
        DebugDetail,
        Help,
        ScanInProgress
    }

    [SerializeField] string systemSkin = DefaultSkin;
    [SerializeField] float navRepeatDelay = 0.16f;
    [SerializeField] float spawnDistanceMeters = 2f;
    [SerializeField] float spawnYOffsetMeters;
    [SerializeField] bool seedExampleCabinetInEditor = true;

    ScreenGenerator screen;
    Renderer display;
    ShaderScreenBase shaderOnline;
    ShaderScreenBase shaderOffline;
    LibretroControlMap libretroControlMap;
    CoinSlotController coinSlot;

    GenericMenu navMenu;
    readonly List<string> catalogNames = new List<string>();
    readonly List<MREnvironmentCatalogEntry> envCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> lightsCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> postersCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> roomSkinsCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentPlacement> placedInstances = new List<MREnvironmentPlacement>();
    readonly List<MRDebugLog.DisplayLine> debugDisplayLines = new List<MRDebugLog.DisplayLine>();
    readonly List<string> debugDetailWrappedLines = new List<string>();
    MRDebugLog.Entry debugDetailEntry;
    int debugDetailScrollOffset;

    MRLayoutRegistry registry;
    MREnvironmentRegistry envRegistry;
    Transform mrSpaceOrigin;
    MRPlacementRayController placementRay;

    Screen currentScreen = Screen.Idle;
    int selectedListIndex;
    int selectedColumnIndex;
    int selectedAdjustmentIndex;
    int selectedLightTuneIndex;
    int listScrollOffset;
    float navCooldown;
    bool sessionActive;
    bool confirmControlWasActive;
    bool secondaryControlWasActive;
    bool backControlWasActive;
    bool placementMoveActive;
    bool placementEnvMoveActive;
    bool placementAddActive;
    bool pendingAddIsEnvironment;
    string movingPlacementId;
    string movingEnvPlacementId;
    string pendingAddCabinetName;
    MREnvironmentCatalogEntry pendingAddEnvEntry;
    MREnvironmentCatalogEntry instancesCatalogEntry;
    Screen instancesReturnScreen;
    string lightTunePlacementId;
    bool pendingReturnToPlacedInstances;
    GameObject pendingAddRoot;
    bool scanRequestInProgress;
    Coroutine scanRoomCoroutine;

    public bool IsSessionActive => sessionActive;

    public void PrepareForCabinet(
        ScreenGenerator screenGenerator,
        LibretroControlMap controlMap,
        CoinSlotController coinSlotController)
    {
        screen = screenGenerator;
        libretroControlMap = controlMap;
        coinSlot = coinSlotController;

        if (screen == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} ScreenGenerator missing");
            return;
        }

        display = screen.GetComponent<Renderer>();
        var shaderConfig = new Dictionary<string, string> { ["damage"] = "none" };
        if (display != null)
        {
            shaderOnline = ShaderScreen.Factory(display, 1, "crt", shaderConfig);
            shaderOffline = ShaderScreen.Factory(display, 1, "crtlod", shaderConfig);
        }

        screen.Init(systemSkin);
        ActivateShader(false);
        BuildNavMenu();
        ShowIdle();
    }

    public void BeginSession()
    {
        if (screen == null)
            return;

        registry = EnsureRegistry();
        envRegistry = EnsureEnvironmentRegistry();
        mrSpaceOrigin = EnsureMrSpaceOrigin();
        placementRay = EnsurePlacementRayController();
        MRAdjustmentsSettings.EnsureLoaded();
        MRPhoneBoothSettings.EnsureLoaded();
        MREffectMeshSettings.EnsureLoaded();
        MRAutoLightingSettings.EnsureLoaded();
        registry.EnsureLayoutLoaded();
        envRegistry.EnsureLayoutLoaded();
        // MR entry already spawned layout + props; respawning here destroys and recreates everything
        // (TV video restarts, visible flicker) when the user reopens the CRT after placement.
        if (registry.SpawnedCount == 0)
            registry.SpawnAll(mrSpaceOrigin);
        if (envRegistry.SpawnedCount == 0)
            envRegistry.SpawnAll(mrSpaceOrigin);

        MRCatalogBootstrap.PrepareCatalog(seedExampleCabinetInEditor);
        RefreshCatalog();
        MREnvironmentCatalog.RefreshCache();
        MRCustomObjectCatalog.RefreshCache();
        RefreshEnvironmentCatalog();
        RefreshLightsCatalog();
        RefreshPostersCatalog();
        RefreshRoomSkinsCatalog();

        BuildNavMenu();
        setupActionMap();
        sessionActive = true;
        confirmControlWasActive = false;
        secondaryControlWasActive = false;
        backControlWasActive = false;
        selectedColumnIndex = 0;
        coinSlot?.insertCoin();

        currentScreen = Screen.NavMain;
        navMenu.selectedIndex = 0;
        navMenu.Deselect();
        ActivateShader(true);
        DrawCurrentScreen();
        MRPhoneBoothVisibility.EnsureMrInstance();
        MRPhoneBoothVisibility.ApplySavedVisibility();
        ConfigManager.WriteConsole($"{LogPrefix} session started (catalog={catalogNames.Count})");
    }

    public void EndSession()
    {
        if (!sessionActive && pendingAddRoot == null && !placementMoveActive && !placementAddActive)
            return;

        sessionActive = false;
        confirmControlWasActive = false;
        secondaryControlWasActive = false;
        backControlWasActive = false;
        if (scanRoomCoroutine != null)
        {
            StopCoroutine(scanRoomCoroutine);
            scanRoomCoroutine = null;
        }
        scanRequestInProgress = false;
        placementMoveActive = false;
        placementEnvMoveActive = false;
        placementAddActive = false;
        movingPlacementId = null;
        movingEnvPlacementId = null;
        CancelPendingAdd(destroyProp: true);
        if (placementRay != null && placementRay.IsActive)
            placementRay.CancelActive();
        currentScreen = Screen.Idle;
        cleanActionMap();
        ActivateShader(false);
        ShowIdle();
        ConfigManager.WriteConsole($"{LogPrefix} session ended");
    }

    /// <summary>CRT off + coin cleared while a game-cabinet placement ray runs; session reopens on next coin insert.</summary>
    public void SuspendForExternalPlacement()
    {
        sessionActive = false;
        confirmControlWasActive = false;
        secondaryControlWasActive = false;
        backControlWasActive = false;
        placementMoveActive = false;
        placementEnvMoveActive = false;
        placementAddActive = false;
        movingPlacementId = null;
        movingEnvPlacementId = null;
        currentScreen = Screen.Idle;
        cleanActionMap();
        ActivateShader(false);
        ShowIdle();
        ConfigManager.WriteConsole($"{LogPrefix} session suspended for cabinet placement");
    }

    void Update()
    {
        if (!sessionActive || screen == null)
            return;

        if (scanRequestInProgress)
            return;

        if (placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        navCooldown -= Time.deltaTime;

        if (WasBackPressed())
        {
            HandleBack();
            return;
        }

        if (navCooldown <= 0f)
        {
            if (UsesRowColumnNavigation(currentScreen))
            {
                int columnDir = ReadHorizontalNavDirection();
                if (columnDir != 0)
                {
                    MoveColumnSelection(columnDir);
                    navCooldown = navRepeatDelay;
                    return;
                }
            }

            if (WasMoveUp() || ReadStickY() > 0.55f)
            {
                MoveSelection(-1);
                navCooldown = navRepeatDelay;
                return;
            }

            if (WasMoveDown() || ReadStickY() < -0.55f)
            {
                MoveSelection(1);
                navCooldown = navRepeatDelay;
                return;
            }
        }

        if (WasConfirmPressed())
        {
            if (navCooldown > 0f)
                SyncConfirmControlEdgeState();
            else
                HandleConfirm();
        }
    }

    void BuildNavMenu()
    {
        if (RequiresRoomScanMenu())
            BuildScanRequiredNavMenu();
        else
            BuildFullNavMenu();
    }

    static bool RequiresRoomScanMenu() => !MRSceneScanState.IsRoomScanned();

    void BuildScanRequiredNavMenu()
    {
        navMenu = new GenericMenu(screen, "SCAN REQUIRED");
        navMenu.AddOption(
            MRSceneScanState.ScanRoomMenuOption,
            "Open Quest Space Setup to map your room");
    }

    void BuildFullNavMenu()
    {
        navMenu = new GenericMenu(screen, "MR CONFIGURATION");
        navMenu.AddOption("CABINETS", "Catalog: add or remove in MR space");
        navMenu.AddOption("ENVIRONMENT", "Build + Custom Objects");
        navMenu.AddOption("MESH", "EffectMesh + scan debug colors");
        navMenu.AddOption("LIGHTS", "Light prefabs from ramiro/Lights");
        navMenu.AddOption("POSTERS", "Wall posters; stick L/R = rotate");
        navMenu.AddOption("ROOM SKIN", "Floor / walls / ceiling textures");
        navMenu.AddOption("MOVE CONFIG", "Reposition ConfigurationCabinetMiniMR");
        navMenu.AddOption("ADJUSTMENTS", "Scale and floor position for game cabinets");
        navMenu.AddOption("PHONE BOOTH", "Show or hide phone booth in MR");
        navMenu.AddOption("DEBUG", "MR errors by date");
        navMenu.AddOption("HELP", "Controls");
        navMenu.AddOption("EXIT", "Close panel");
    }

    void RefreshCatalog()
    {
        catalogNames.Clear();
        catalogNames.AddRange(MRLayoutRegistry.GetCatalogCabinetNames());
        catalogNames.Sort();
    }

    void RefreshEnvironmentCatalog()
    {
        envCatalogEntries.Clear();
        envCatalogEntries.AddRange(MREnvironmentUnifiedCatalog.GetCatalogEntries());
    }

    void RefreshLightsCatalog()
    {
        lightsCatalogEntries.Clear();
        MRLightsCatalog.RefreshCache();
        lightsCatalogEntries.AddRange(MRLightsCatalog.GetCatalogEntries());
    }

    void RefreshPostersCatalog()
    {
        postersCatalogEntries.Clear();
        MRPostersCatalog.RefreshCache();
        postersCatalogEntries.AddRange(MRPostersCatalog.GetCatalogEntries());
    }

    void RefreshRoomSkinsCatalog()
    {
        roomSkinsCatalogEntries.Clear();
        MRRoomSkinCatalog.RefreshCache();
        roomSkinsCatalogEntries.AddRange(MRRoomSkinCatalog.GetCatalogEntries());
    }

    void DrawCurrentScreen()
    {
        screen.Clear();

        switch (currentScreen)
        {
            case Screen.NavMain:
                navMenu.DrawMenu();
                if (RequiresRoomScanMenu())
                    DrawFooter("A: scan room");
                else
                    DrawFooter("STICK: move   A: select   B: back");
                break;
            case Screen.ScanInProgress:
                DrawScanInProgressPage();
                break;
            case Screen.Cabinets:
                DrawCabinetsPage();
                break;
            case Screen.Adjustments:
                DrawAdjustmentsPage();
                break;
            case Screen.PhoneBooth:
                DrawPhoneBoothPage();
                break;
            case Screen.Environment:
                DrawEnvironmentPage();
                break;
            case Screen.Mesh:
                DrawMeshPage();
                break;
            case Screen.Lights:
                DrawLightsPage();
                break;
            case Screen.Posters:
                DrawPostersPage();
                break;
            case Screen.RoomSkins:
                DrawRoomSkinsPage();
                break;
            case Screen.PlacedInstances:
                DrawPlacedInstancesPage();
                break;
            case Screen.LightTune:
                DrawLightTunePage();
                break;
            case Screen.Debug:
                DrawDebugPage();
                break;
            case Screen.DebugDetail:
                DrawDebugDetailPage();
                break;
            case Screen.Help:
                DrawHelpPage();
                break;
        }

        screen.DrawScreen();
    }

    void DrawScanInProgressPage()
    {
        screen.PrintCentered(0, "SCAN ROOM", true);
        screen.PrintLine(1, false, '-');
        screen.PrintCentered(8, "Quest scanner open", true);
        screen.PrintCentered(10, "Walk around your room", false);
        screen.PrintCentered(12, "Finish setup to continue", false);
    }

    void DrawCabinetsPage()
    {
        screen.PrintCentered(0, "CABINETS", true);
        screen.PrintLine(1, false, '-');

        if (catalogNames.Count == 0)
        {
            screen.PrintCentered(8, "Not Found Cabinet", true);
            screen.PrintCentered(10, "Check cabinetsdb/", false);
            DrawFooter("B: back");
            return;
        }

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= catalogNames.Count)
                break;

            string name = Truncate(catalogNames[idx], ListRowNameWidth);
            bool inScene = registry != null && registry.IsCabinetInScene(catalogNames[idx]);
            string suffix = inScene ? "Y " : "N ";
            DrawListRow(row, idx, selectedListIndex, name, GetCabinetRowActions(idx), suffix);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < catalogNames.Count)
            screen.Print(1, 20, Truncate(catalogNames[selectedListIndex], 36), false);

        DrawFooter(RowColumnFooter);
    }

    void DrawAdjustmentsPage()
    {
        screen.PrintCentered(0, "ADJUSTMENTS", true);
        screen.PrintLine(1, false, '-');

        float scale = MRAdjustmentsSettings.CabinetScale;
        float floorPos = MRAdjustmentsSettings.FloorCabinetPosition;

        string scaleLabel = $"Scale {scale:F2}";
        string floorLabel = $"Floor {floorPos:F2}";

        DrawListRow(4, 0, selectedAdjustmentIndex, scaleLabel, AdjustmentValueActions);
        DrawListRow(6, 1, selectedAdjustmentIndex, floorLabel, AdjustmentValueActions);

        screen.Print(1, 10, "1.00 = default", false);
        DrawFooter(RowColumnFooter);
    }

    void DrawEnvironmentPage()
    {
        screen.PrintCentered(0, "ENVIRONMENT", true);
        screen.PrintLine(1, false, '-');

        if (envCatalogEntries.Count == 0)
        {
            screen.PrintCentered(8, "No props found", true);
            screen.PrintCentered(10, "PrefabsEnvironment/ or", false);
            screen.PrintCentered(11, "MR/Custom Objects/", false);
            DrawFooter("B: back");
            return;
        }

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= envCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = envCatalogEntries[idx];
            string label = Truncate(entry.MenuLabel, ListRowNameWidth);
            int count = envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0;
            string suffix = PadLeft(count.ToString(), 2) + " ";
            DrawListRow(row, idx, selectedListIndex, label, GetEnvironmentRowActions(idx), suffix);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < envCatalogEntries.Count)
            screen.Print(1, 20, Truncate(envCatalogEntries[selectedListIndex].ToString(), 36), false);

        DrawFooter(RowColumnFooter);
    }

    void DrawLightsPage()
    {
        screen.PrintCentered(0, "LIGHTS", true);
        screen.PrintLine(1, false, '-');

        DrawListRow(
            4,
            LightsAutoRowIndex,
            selectedListIndex,
            Truncate(MRAutoLightingVisibility.AutoLightLabel, ListRowNameWidth),
            GetAutoLightRowActions());
        screen.Print(1, 5, $"   {Truncate(MRAutoLightingVisibility.GetStatusLabel(), 34)}", false);
        screen.Print(1, 6, "EnterMR only, not placed", false);

        if (lightsCatalogEntries.Count == 0)
        {
            screen.PrintCentered(10, "No light prefabs found", true);
            screen.PrintCentered(12, "ramiro/Lights/", false);
            DrawFooter(RowColumnFooter);
            return;
        }

        int catalogVisibleRows = Mathf.Max(1, VisibleCabinetRows - 1);
        int row = 7;
        for (int i = 0; i < catalogVisibleRows; i++)
        {
            int catalogIdx = listScrollOffset + i;
            if (catalogIdx >= lightsCatalogEntries.Count)
                break;

            int listIdx = catalogIdx + 1;
            MREnvironmentCatalogEntry entry = lightsCatalogEntries[catalogIdx];
            string label = Truncate(entry.MenuLabel, ListRowNameWidth);
            int count = envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0;
            string suffix = PadLeft(count.ToString(), 2) + " ";
            DrawListRow(row, listIdx, selectedListIndex, label, GetLightsRowActions(listIdx), suffix);
            row++;
        }

        if (selectedListIndex > LightsAutoRowIndex
            && selectedListIndex - 1 < lightsCatalogEntries.Count)
        {
            screen.Print(
                1,
                20,
                Truncate(lightsCatalogEntries[selectedListIndex - 1].ToString(), 36),
                false);
        }

        DrawFooter(RowColumnFooter);
    }

    void DrawPostersPage()
    {
        screen.PrintCentered(0, "POSTERS", true);
        screen.PrintLine(1, false, '-');

        if (postersCatalogEntries.Count == 0)
        {
            screen.PrintCentered(8, "No posters found", true);
            screen.PrintCentered(10, "Add images to", false);
            screen.PrintCentered(11, "MR/Posters/", false);
            DrawFooter("B: back");
            return;
        }

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= postersCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = postersCatalogEntries[idx];
            string label = Truncate(entry.MenuLabel, ListRowNameWidth);
            int count = envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0;
            string suffix = PadLeft(count.ToString(), 2) + " ";
            DrawListRow(row, idx, selectedListIndex, label, GetPostersRowActions(idx), suffix);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < postersCatalogEntries.Count)
            screen.Print(1, 20, Truncate(postersCatalogEntries[selectedListIndex].ToString(), 36), false);

        DrawFooter(RowColumnFooter);
    }

    void DrawRoomSkinsPage()
    {
        screen.PrintCentered(0, "ROOM SKIN", true);
        screen.PrintLine(1, false, '-');

        if (roomSkinsCatalogEntries.Count == 0)
        {
            screen.PrintCentered(8, "No room skins found", true);
            screen.PrintCentered(10, "Resources/ramiro/roomskin/", false);
            screen.PrintCentered(11, "or MR/Room Skins/", false);
            DrawFooter("B: back");
            return;
        }

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= roomSkinsCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = roomSkinsCatalogEntries[idx];
            string label = Truncate(entry.DisplayLabel, RoomSkinListRowNameWidth);
            DrawListRow(row, idx, selectedListIndex, label, GetRoomSkinsRowActions(idx), nameWidth: RoomSkinListRowNameWidth);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < roomSkinsCatalogEntries.Count)
            screen.Print(1, 20, Truncate(roomSkinsCatalogEntries[selectedListIndex].DisplayLabel, 36), false);

        screen.Print(1, 22, "A=Add/Rem (no ray)", false);
        DrawFooter(RowColumnFooter);
    }

    void DrawPlacedInstancesPage()
    {
        string title = Truncate(instancesCatalogEntry.MenuLabel, 24);
        int count = placedInstances.Count;
        screen.PrintCentered(0, "PLACED", true);
        screen.Print(1, 1, $"{title} ({count})", false);
        screen.PrintLine(2, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= placedInstances.Count + 1)
                break;

            if (idx < placedInstances.Count)
            {
                MREnvironmentPlacement placement = placedInstances[idx];
                string label = $"#{idx + 1}{SurfaceShortLabel(placement.SurfaceType)}";
                DrawListRow(row, idx, selectedListIndex, label, GetPlacedInstanceRowActions(idx));
            }
            else
                DrawListRow(row, idx, selectedListIndex, "+ Add", GetPlacedInstanceRowActions(idx));

            row++;
        }

        DrawFooter(RowColumnFooter);
    }

    static string SurfaceShortLabel(PlacementSurfaceType surfaceType) =>
        surfaceType switch
        {
            PlacementSurfaceType.Floor => "Flr",
            PlacementSurfaceType.Wall => "Wal",
            PlacementSurfaceType.Ceiling => "Cel",
            _ => "?"
        };

    static string PadLeft(string value, int width)
    {
        if (value.Length >= width)
            return value;
        return value.PadLeft(width);
    }

    bool IsAddAnotherRowSelected() =>
        currentScreen == Screen.PlacedInstances && selectedListIndex == placedInstances.Count;

    void RefreshPlacedInstancesList()
    {
        placedInstances.Clear();
        if (envRegistry == null)
            return;

        placedInstances.AddRange(envRegistry.FindAllPlacementsByCatalogEntry(instancesCatalogEntry));
    }

    void OpenPlacedInstances(MREnvironmentCatalogEntry entry, Screen returnScreen)
    {
        instancesCatalogEntry = entry;
        instancesReturnScreen = returnScreen;
        RefreshPlacedInstancesList();
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        currentScreen = Screen.PlacedInstances;
        DrawCurrentScreen();
    }

    void DrawLightTunePage()
    {
        screen.PrintCentered(0, "LIGHT TUNE", true);
        screen.PrintLine(1, false, '-');

        string label = Truncate(instancesCatalogEntry.MenuLabel, 22);
        int instanceNumber = FindPlacedInstanceIndex(lightTunePlacementId) + 1;
        screen.Print(1, 2, $"{label} #{instanceNumber}", false);

        float intensity = 0f;
        float range = 0f;
        float temperature = 0f;
        if (envRegistry != null)
            envRegistry.TryGetLightSettings(lightTunePlacementId, out intensity, out range, out temperature);

        DrawListRow(5, 0, selectedLightTuneIndex, $"Int {intensity:F1}", AdjustmentValueActions);
        DrawListRow(7, 1, selectedLightTuneIndex, $"Rng {range:F1}", AdjustmentValueActions);
        DrawListRow(9, 2, selectedLightTuneIndex, $"Tmp {temperature:F0}K", AdjustmentValueActions);

        screen.Print(1, 12, "Int/Rng step 0.1", false);
        screen.Print(1, 13, "Tmp step 100K", false);
        DrawFooter(RowColumnFooter);
    }

    int FindPlacedInstanceIndex(string placementId)
    {
        for (int i = 0; i < placedInstances.Count; i++)
        {
            if (placedInstances[i]?.Id == placementId)
                return i;
        }

        return 0;
    }

    void DrawMeshPage()
    {
        screen.PrintCentered(0, "MESH", true);
        screen.PrintLine(1, false, '-');

        DrawListRow(
            3,
            0,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.AnchorMeshLabel, ListRowNameWidth),
            GetMeshRowActions(0));
        screen.Print(1, 4, $"   {Truncate(MREffectMeshVisibility.GetAnchorStatusLabel(), 34)}", false);

        DrawListRow(
            5,
            1,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.GlobalMeshLabel, ListRowNameWidth),
            GetMeshRowActions(1));
        screen.Print(1, 6, $"   {Truncate(MREffectMeshVisibility.GetGlobalStatusLabel(), 34)}", false);

        DrawListRow(
            8,
            MeshScanColorsRowIndex,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.ScanDebugColorsLabel, ListRowNameWidth),
            GetMeshRowActions(MeshScanColorsRowIndex));
        screen.Print(1, 9, $"   {Truncate(MREffectMeshVisibility.GetScanDebugColorsStatusLabel(), 34)}", false);

        screen.Print(1, 12, "Floor=green Wall=orange", false);
        screen.Print(1, 13, "Table=yellow Ceiling=blue", false);
        screen.Print(1, 14, "Turn EffectMesh ON first", false);
        DrawFooter(RowColumnFooter);
    }

    void DrawPhoneBoothPage()
    {
        screen.PrintCentered(0, "PHONE BOOTH", true);
        screen.PrintLine(1, false, '-');

        string status = MRPhoneBoothVisibility.GetStatusLabel();
        screen.Print(1, 4, $"Status: {status}", true);
        DrawListRow(7, 0, 0, "Phone", GetPhoneBoothRowActions());

        screen.Print(1, 10, "Hidden saves room space", false);
        screen.Print(1, 11, "Show restores last pose", false);
        DrawFooter(RowColumnFooter);
    }

    void DrawDebugPage()
    {
        screen.PrintCentered(0, "DEBUG", true);
        screen.PrintLine(1, false, '-');

        debugDisplayLines.Clear();
        debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());

        if (debugDisplayLines.Count == 0)
        {
            screen.PrintCentered(8, "No errors logged", true);
            screen.PrintCentered(10, "Add failures appear here", false);
            DrawFooter("B: back");
            return;
        }

        screen.Print(1, 2, $"{debugDisplayLines.Count} lines (newest first)", false);
        screen.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleDebugRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= debugDisplayLines.Count)
                break;

            MRDebugLog.DisplayLine line = debugDisplayLines[idx];
            if (line.IsDateHeader)
            {
                screen.Print(1, row, line.Text, true);
            }
            else
            {
                string text = Truncate(line.Text, ListRowNameWidth);
                DrawListRow(row, idx, selectedListIndex, text, GetDebugRowActions(idx));
            }

            row++;
        }

        DrawFooter(RowColumnFooter);
    }

    void DrawHelpPage()
    {
        screen.PrintCentered(0, "HELP", true);
        screen.Print(2, 3, "Up/Down: select row", false);
        screen.Print(2, 5, "L/R: select action on row", false);
        screen.Print(2, 7, "A: run action   B: back", false);
        screen.Print(2, 9, "Row: Name|Opts|Add|Rem|", false);
        screen.Print(2, 10, "Placed: Move|Tune|Rem|", false);
        screen.Print(2, 11, "Tune: Int|Rng|Tmp K", false);
        screen.Print(2, 12, "MR Auto: EnterMR lights only", false);
        screen.Print(2, 13, "Cabinets: 1 per game only", false);
        screen.Print(2, 14, "Environment/Lights: unlimited", false);
        screen.Print(2, 15, "Posters: MR/Posters/ images", false);
        screen.Print(2, 16, "Poster place: stick L/R = spin Y", false);
        screen.Print(2, 17, "Placement ray after Add/Move", false);
        screen.Print(2, 19, "Layout = MR/objects-layout.yaml", false);
        DrawFooter("B: back");
    }

    void DrawDebugDetailPage()
    {
        screen.PrintCentered(0, "ERROR DETAIL", true);
        screen.PrintLine(1, false, '-');

        string level = string.IsNullOrEmpty(debugDetailEntry.Level) ? "INFO" : debugDetailEntry.Level;
        screen.Print(1, 2, $"{debugDetailEntry.Timestamp:yyyy-MM-dd HH:mm:ss} {level}", false);
        screen.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleDebugDetailRows; i++)
        {
            int idx = debugDetailScrollOffset + i;
            if (idx >= debugDetailWrappedLines.Count)
                break;

            screen.Print(1, row, debugDetailWrappedLines[idx], false);
            row++;
        }

        if (debugDetailWrappedLines.Count > VisibleDebugDetailRows)
        {
            int page = debugDetailScrollOffset / VisibleDebugDetailRows + 1;
            int pages = (debugDetailWrappedLines.Count + VisibleDebugDetailRows - 1) / VisibleDebugDetailRows;
            screen.Print(1, screen.CharactersYCount - 3, $"Lines {debugDetailScrollOffset + 1}-{Mathf.Min(debugDetailScrollOffset + VisibleDebugDetailRows, debugDetailWrappedLines.Count)} / {debugDetailWrappedLines.Count}  page {page}/{pages}", false);
        }

        DrawFooter("Up/Down: scroll   B: back");
    }

    void ShowIdle()
    {
        if (screen == null)
            return;

        screen.Clear();
        screen.PrintCentered(1, "MR CONFIGURATION", true);
        screen.PrintCentered(8, "Cabinet ready", false);
        screen.PrintCentered(11, "Open panel to edit layout", false);
        screen.DrawScreen();
    }

    void DrawFooter(string text)
    {
        screen.Print(1, screen.CharactersYCount - 2, text, false);
    }

    const string RowColumnFooter = "L/R: action   A: go   B: back";

    static bool UsesRowColumnNavigation(Screen screen) =>
        screen == Screen.Cabinets
        || screen == Screen.Environment
        || screen == Screen.Lights
        || screen == Screen.Posters
        || screen == Screen.RoomSkins
        || screen == Screen.PlacedInstances
        || screen == Screen.Mesh
        || screen == Screen.Adjustments
        || screen == Screen.LightTune
        || screen == Screen.PhoneBooth
        || screen == Screen.Debug;

    static string RowActionLabel(RowActionKind kind) =>
        kind switch
        {
            RowActionKind.Add => "Add",
            RowActionKind.Options => "Opts",
            RowActionKind.Remove => "Rem",
            RowActionKind.Move => "Move",
            RowActionKind.Tune => "Tune",
            RowActionKind.Decrease => "-",
            RowActionKind.Increase => "+",
            RowActionKind.ToggleOn => "On",
            RowActionKind.ToggleOff => "Off",
            RowActionKind.Show => "Show",
            RowActionKind.Hide => "Hide",
            RowActionKind.OpenDetail => "Open",
            _ => "?"
        };

    string FormatActionColumns(IReadOnlyList<RowActionKind> actions, bool rowSelected)
    {
        if (actions == null || actions.Count == 0)
            return string.Empty;

        var parts = new List<string>(actions.Count);
        for (int i = 0; i < actions.Count; i++)
        {
            string label = RowActionLabel(actions[i]);
            if (rowSelected && i == selectedColumnIndex)
                label = $"[{label}]";
            parts.Add(label);
        }

        return "|" + string.Join("|", parts) + "|";
    }

    void DrawListRow(int row, int listIndex, int selectedIndex, string label, IReadOnlyList<RowActionKind> actions, string suffix = null, int nameWidth = ListRowNameWidth)
    {
        bool rowSelected = listIndex == selectedIndex;
        string prefix = rowSelected ? "> " : "  ";
        string suffixPart = string.IsNullOrEmpty(suffix) ? string.Empty : suffix;
        string line = prefix + PadRight(Truncate(label, nameWidth), nameWidth) + suffixPart;
        if (rowSelected)
            line += FormatActionColumns(actions, true);

        if (line.Length > screen.CharactersXCount - 1)
            line = line.Substring(0, screen.CharactersXCount - 1);

        screen.Print(1, row, line, rowSelected);
    }

    void MoveColumnSelection(int delta)
    {
        IReadOnlyList<RowActionKind> actions = GetRowActionsForCurrentSelection();
        if (actions.Count == 0)
            return;

        selectedColumnIndex += delta;
        if (selectedColumnIndex < 0)
            selectedColumnIndex = actions.Count - 1;
        else if (selectedColumnIndex >= actions.Count)
            selectedColumnIndex = 0;

        DrawCurrentScreen();
    }

    void ClampColumnIndexForCurrentRow()
    {
        IReadOnlyList<RowActionKind> actions = GetRowActionsForCurrentSelection();
        if (actions.Count == 0)
        {
            selectedColumnIndex = 0;
            return;
        }

        if (selectedColumnIndex < 0)
            selectedColumnIndex = 0;
        else if (selectedColumnIndex >= actions.Count)
            selectedColumnIndex = actions.Count - 1;
    }

    IReadOnlyList<RowActionKind> GetRowActionsForCurrentSelection()
    {
        return currentScreen switch
        {
            Screen.Cabinets => GetCabinetRowActions(selectedListIndex),
            Screen.Environment => GetEnvironmentRowActions(selectedListIndex),
            Screen.Lights => GetLightsRowActions(selectedListIndex),
            Screen.Posters => GetPostersRowActions(selectedListIndex),
            Screen.RoomSkins => GetRoomSkinsRowActions(selectedListIndex),
            Screen.PlacedInstances => GetPlacedInstanceRowActions(selectedListIndex),
            Screen.Mesh => GetMeshRowActions(selectedListIndex),
            Screen.Adjustments => AdjustmentValueActions,
            Screen.LightTune => AdjustmentValueActions,
            Screen.PhoneBooth => GetPhoneBoothRowActions(),
            Screen.Debug => GetDebugRowActions(selectedListIndex),
            _ => System.Array.Empty<RowActionKind>()
        };
    }

    static readonly RowActionKind[] AdjustmentValueActions =
    {
        RowActionKind.Decrease,
        RowActionKind.Increase
    };

    List<RowActionKind> GetCabinetRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= catalogNames.Count || registry == null)
            return actions;

        if (registry.IsCabinetInScene(catalogNames[index]))
        {
            actions.Add(RowActionKind.Move);
            actions.Add(RowActionKind.Remove);
        }
        else
            actions.Add(RowActionKind.Add);

        return actions;
    }

    List<RowActionKind> GetEnvironmentRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= envCatalogEntries.Count || envRegistry == null)
            return actions;

        int count = envRegistry.GetInstanceCount(envCatalogEntries[index]);
        if (count > 0)
            actions.Add(RowActionKind.Options);
        actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetLightsRowActions(int index)
    {
        if (index == LightsAutoRowIndex)
            return GetAutoLightRowActions();

        var actions = new List<RowActionKind>();
        int catalogIdx = index - 1;
        if (catalogIdx < 0 || catalogIdx >= lightsCatalogEntries.Count || envRegistry == null)
            return actions;

        MREnvironmentCatalogEntry entry = lightsCatalogEntries[catalogIdx];
        int count = envRegistry.GetInstanceCount(entry);
        if (count > 0)
            actions.Add(RowActionKind.Options);
        actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetPostersRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= postersCatalogEntries.Count || envRegistry == null)
            return actions;

        MREnvironmentCatalogEntry entry = postersCatalogEntries[index];
        int count = envRegistry.GetInstanceCount(entry);
        if (count > 0)
            actions.Add(RowActionKind.Options);
        actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetRoomSkinsRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= roomSkinsCatalogEntries.Count || envRegistry == null)
            return actions;

        MREnvironmentCatalogEntry entry = roomSkinsCatalogEntries[index];
        if (envRegistry.GetInstanceCount(entry) > 0)
            actions.Add(RowActionKind.Remove);
        else
            actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetAutoLightRowActions()
    {
        var actions = new List<RowActionKind>();
        if (MRAutoLightingSettings.Enabled)
            actions.Add(RowActionKind.ToggleOff);
        else
            actions.Add(RowActionKind.ToggleOn);
        return actions;
    }

    List<RowActionKind> GetPlacedInstanceRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index == placedInstances.Count)
        {
            actions.Add(RowActionKind.Add);
            return actions;
        }

        if (index < 0 || index >= placedInstances.Count)
            return actions;

        if (instancesCatalogEntry.Source != MREnvironmentObjectSource.RoomSkin)
            actions.Add(RowActionKind.Move);
        if (instancesCatalogEntry.Source == MREnvironmentObjectSource.Light)
            actions.Add(RowActionKind.Tune);
        actions.Add(RowActionKind.Remove);
        return actions;
    }

    List<RowActionKind> GetMeshRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= MeshOptionCount)
            return actions;

        bool enabled = index switch
        {
            0 => MREffectMeshSettings.AnchorMeshEnabled,
            1 => MREffectMeshSettings.GlobalMeshEnabled,
            MeshScanColorsRowIndex => MREffectMeshSettings.ScanDebugColorsEnabled,
            _ => false
        };

        if (enabled)
            actions.Add(RowActionKind.ToggleOff);
        else
            actions.Add(RowActionKind.ToggleOn);

        return actions;
    }

    List<RowActionKind> GetPhoneBoothRowActions()
    {
        var actions = new List<RowActionKind>();
        if (MRPhoneBoothSettings.Visible)
            actions.Add(RowActionKind.Hide);
        else
            actions.Add(RowActionKind.Show);
        return actions;
    }

    List<RowActionKind> GetDebugRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= debugDisplayLines.Count)
            return actions;

        if (!debugDisplayLines[index].IsDateHeader)
            actions.Add(RowActionKind.OpenDetail);

        return actions;
    }

    bool ExecuteSelectedRowAction()
    {
        IReadOnlyList<RowActionKind> actions = GetRowActionsForCurrentSelection();
        if (selectedColumnIndex < 0 || selectedColumnIndex >= actions.Count)
            return false;

        return ExecuteRowAction(actions[selectedColumnIndex]);
    }

    bool ExecuteRowAction(RowActionKind action)
    {
        switch (currentScreen)
        {
            case Screen.Cabinets:
                return ExecuteCabinetRowAction(action);
            case Screen.Environment:
                return ExecuteEnvironmentRowAction(action);
            case Screen.Lights:
                return ExecuteLightsRowAction(action);
            case Screen.Posters:
                return ExecutePostersRowAction(action);
            case Screen.RoomSkins:
                return ExecuteRoomSkinsRowAction(action);
            case Screen.PlacedInstances:
                return ExecutePlacedInstanceRowAction(action);
            case Screen.Mesh:
                ExecuteMeshRowAction(action);
                return true;
            case Screen.Adjustments:
                ExecuteAdjustmentRowAction(action);
                return true;
            case Screen.LightTune:
                ExecuteLightTuneRowAction(action);
                return true;
            case Screen.PhoneBooth:
                ExecutePhoneBoothRowAction(action);
                return true;
            case Screen.Debug:
                return ExecuteDebugRowAction(action);
            default:
                return false;
        }
    }

    bool ExecuteCabinetRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= catalogNames.Count || registry == null)
            return false;

        string cabinetName = catalogNames[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                BeginAddCabinetWithRay(cabinetName);
                return false;
            case RowActionKind.Move:
                if (registry.IsCabinetInScene(cabinetName))
                    BeginMoveCabinetByCatalogName(cabinetName);
                return false;
            case RowActionKind.Remove:
                if (registry.TryRemoveCabinetFromScene(cabinetName))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed {cabinetName}");
                    RefreshCatalog();
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    bool ExecuteEnvironmentRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= envCatalogEntries.Count)
            return false;

        MREnvironmentCatalogEntry entry = envCatalogEntries[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                QueueReturnToPlacedInstances(entry, Screen.Environment);
                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Options:
                OpenPlacedInstances(entry, Screen.Environment);
                return true;
            default:
                return false;
        }
    }

    bool ExecuteRoomSkinsRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= roomSkinsCatalogEntries.Count || envRegistry == null)
            return false;

        MREnvironmentCatalogEntry entry = roomSkinsCatalogEntries[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                BeginAddRoomSkinInstant(entry);
                return false;
            case RowActionKind.Remove:
            {
                IReadOnlyList<MREnvironmentPlacement> placements =
                    envRegistry.FindAllPlacementsByCatalogEntry(entry);
                if (placements.Count == 0)
                    return false;
                if (envRegistry.RemovePlacement(placements[0].Id))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed room skin {entry}");
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            }
            default:
                return false;
        }
    }

    bool ExecuteLightsRowAction(RowActionKind action)
    {
        if (selectedListIndex == LightsAutoRowIndex)
        {
            ExecuteAutoLightRowAction(action);
            return true;
        }

        int catalogIdx = selectedListIndex - 1;
        if (catalogIdx < 0 || catalogIdx >= lightsCatalogEntries.Count)
            return false;

        MREnvironmentCatalogEntry entry = lightsCatalogEntries[catalogIdx];
        switch (action)
        {
            case RowActionKind.Add:
                QueueReturnToPlacedInstances(entry, Screen.Lights);
                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Options:
                OpenPlacedInstances(entry, Screen.Lights);
                return true;
            default:
                return false;
        }
    }

    bool ExecutePostersRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= postersCatalogEntries.Count)
            return false;

        MREnvironmentCatalogEntry entry = postersCatalogEntries[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                QueueReturnToPlacedInstances(entry, Screen.Posters);
                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Options:
                OpenPlacedInstances(entry, Screen.Posters);
                return true;
            default:
                return false;
        }
    }

    void ExecuteAutoLightRowAction(RowActionKind action)
    {
        if (action == RowActionKind.ToggleOn)
            MRAutoLightingVisibility.SetEnabled(true);
        else if (action == RowActionKind.ToggleOff)
            MRAutoLightingVisibility.SetEnabled(false);
    }

    bool ExecutePlacedInstanceRowAction(RowActionKind action)
    {
        if (envRegistry == null)
            return false;

        switch (action)
        {
            case RowActionKind.Add:
                QueueReturnToPlacedInstances(instancesCatalogEntry, instancesReturnScreen);
                BeginAddEnvironmentWithRay(instancesCatalogEntry);
                return false;
            case RowActionKind.Move:
                if (!IsAddAnotherRowSelected() && selectedListIndex < placedInstances.Count)
                    BeginMoveEnvByPlacementId(placedInstances[selectedListIndex].Id);
                return false;
            case RowActionKind.Tune:
                if (!IsAddAnotherRowSelected() && selectedListIndex < placedInstances.Count)
                    OpenLightTuneForPlacement(placedInstances[selectedListIndex].Id);
                return true;
            case RowActionKind.Remove:
                if (IsAddAnotherRowSelected() || selectedListIndex >= placedInstances.Count)
                    return false;
                if (envRegistry.RemovePlacement(placedInstances[selectedListIndex].Id))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed instance {placedInstances[selectedListIndex].Id}");
                    RefreshPlacedInstancesList();
                    if (selectedListIndex >= placedInstances.Count)
                        selectedListIndex = Mathf.Max(0, placedInstances.Count);
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    void ExecuteMeshRowAction(RowActionKind action)
    {
        switch (selectedListIndex)
        {
            case 0:
                if (action == RowActionKind.ToggleOn)
                    MREffectMeshVisibility.SetAnchorMeshEnabled(true);
                else if (action == RowActionKind.ToggleOff)
                    MREffectMeshVisibility.SetAnchorMeshEnabled(false);
                break;
            case 1:
                if (action == RowActionKind.ToggleOn)
                    MREffectMeshVisibility.SetGlobalMeshEnabled(true);
                else if (action == RowActionKind.ToggleOff)
                    MREffectMeshVisibility.SetGlobalMeshEnabled(false);
                break;
            case MeshScanColorsRowIndex:
                if (action == RowActionKind.ToggleOn)
                    MREffectMeshVisibility.SetScanDebugColorsEnabled(true);
                else if (action == RowActionKind.ToggleOff)
                    MREffectMeshVisibility.SetScanDebugColorsEnabled(false);
                break;
        }
    }

    void ExecuteAdjustmentRowAction(RowActionKind action)
    {
        if (registry == null)
            return;

        int direction = action == RowActionKind.Increase ? 1 : action == RowActionKind.Decrease ? -1 : 0;
        if (direction == 0)
            return;

        if (selectedAdjustmentIndex == 0)
            MRAdjustmentsSettings.AdjustCabinetScale(direction);
        else
            MRAdjustmentsSettings.AdjustFloorCabinetPosition(direction);

        registry.ApplyGlobalAdjustmentsToSpawnedFloorCabinets();
    }

    void ExecuteLightTuneRowAction(RowActionKind action)
    {
        int direction = action == RowActionKind.Increase ? 1 : action == RowActionKind.Decrease ? -1 : 0;
        if (direction == 0 || envRegistry == null || string.IsNullOrEmpty(lightTunePlacementId))
            return;

        if (!envRegistry.TryGetLightSettings(lightTunePlacementId, out float intensity, out float range, out float temperature))
            return;

        if (selectedLightTuneIndex == 0)
        {
            intensity = MRLightPlacement.SnapTune(
                intensity + direction * MRLightPlacement.TuneStep,
                MRLightPlacement.MinIntensity,
                MRLightPlacement.MaxIntensity);
        }
        else if (selectedLightTuneIndex == 1)
        {
            range = MRLightPlacement.SnapTune(
                range + direction * MRLightPlacement.TuneStep,
                MRLightPlacement.MinRange,
                MRLightPlacement.MaxRange);
        }
        else
        {
            temperature = MRLightPlacement.SnapTemperature(
                temperature + direction * MRLightPlacement.TemperatureStep);
        }

        envRegistry.TryUpdateLightSettings(lightTunePlacementId, intensity, range, temperature);
    }

    void ExecutePhoneBoothRowAction(RowActionKind action)
    {
        switch (action)
        {
            case RowActionKind.Show:
                if (!MRPhoneBoothSettings.Visible)
                    MRPhoneBoothVisibility.Toggle();
                break;
            case RowActionKind.Hide:
                if (MRPhoneBoothSettings.Visible)
                    MRPhoneBoothVisibility.Toggle();
                break;
        }
    }

    bool ExecuteDebugRowAction(RowActionKind action)
    {
        if (action != RowActionKind.OpenDetail)
            return false;

        debugDisplayLines.Clear();
        debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());
        if (!TryOpenDebugDetailFromSelection())
            return false;

        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        return true;
    }

    void OpenLightTuneForPlacement(string placementId)
    {
        lightTunePlacementId = placementId;
        selectedLightTuneIndex = 0;
        selectedColumnIndex = 0;
        currentScreen = Screen.LightTune;
        DrawCurrentScreen();
    }

    int ReadHorizontalNavDirection()
    {
        float stickX = ReadStickX();
        if (stickX > 0.55f || WasMoveRight())
            return 1;
        if (stickX < -0.55f || WasMoveLeft())
            return -1;
        return 0;
    }

    int GetListCount()
    {
        return currentScreen switch
        {
            Screen.Cabinets => catalogNames.Count,
            Screen.Environment => envCatalogEntries.Count,
            Screen.Lights => Mathf.Max(1, 1 + lightsCatalogEntries.Count),
            Screen.Posters => postersCatalogEntries.Count,
            Screen.RoomSkins => roomSkinsCatalogEntries.Count,
            Screen.PlacedInstances => placedInstances.Count + 1,
            Screen.Mesh => MeshOptionCount,
            Screen.Debug => debugDisplayLines.Count,
            _ => 0
        };
    }

    void MoveSelection(int delta)
    {
        switch (currentScreen)
        {
            case Screen.NavMain:
                if (delta < 0)
                    navMenu.PreviousOption();
                else
                    navMenu.NextOption();
                DrawCurrentScreen();
                break;

            case Screen.Cabinets:
            case Screen.Environment:
            case Screen.Posters:
            case Screen.RoomSkins:
            case Screen.PlacedInstances:
            case Screen.Mesh:
                int count = GetListCount();
                if (count == 0)
                    return;

                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = count - 1;
                else if (selectedListIndex >= count)
                    selectedListIndex = 0;

                selectedColumnIndex = 0;
                ClampColumnIndexForCurrentRow();
                ClampListScroll();
                DrawCurrentScreen();
                break;

            case Screen.Lights:
                int lightsCount = GetListCount();
                if (lightsCount == 0)
                    return;

                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = lightsCount - 1;
                else if (selectedListIndex >= lightsCount)
                    selectedListIndex = 0;

                selectedColumnIndex = 0;
                ClampColumnIndexForCurrentRow();
                ClampLightsScroll();
                DrawCurrentScreen();
                break;

            case Screen.Debug:
                debugDisplayLines.Clear();
                debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());
                if (debugDisplayLines.Count == 0)
                    return;

                selectedListIndex = MoveDebugSelection(delta);
                ClampListScroll();
                DrawCurrentScreen();
                break;

            case Screen.DebugDetail:
                if (debugDetailWrappedLines.Count == 0)
                    return;

                debugDetailScrollOffset += delta;
                if (debugDetailScrollOffset < 0)
                    debugDetailScrollOffset = 0;
                else if (debugDetailScrollOffset > Mathf.Max(0, debugDetailWrappedLines.Count - VisibleDebugDetailRows))
                    debugDetailScrollOffset = Mathf.Max(0, debugDetailWrappedLines.Count - VisibleDebugDetailRows);

                DrawCurrentScreen();
                break;

            case Screen.Adjustments:
                selectedAdjustmentIndex += delta;
                if (selectedAdjustmentIndex < 0)
                    selectedAdjustmentIndex = 1;
                else if (selectedAdjustmentIndex > 1)
                    selectedAdjustmentIndex = 0;
                selectedColumnIndex = 0;
                DrawCurrentScreen();
                break;

            case Screen.LightTune:
                selectedLightTuneIndex += delta;
                if (selectedLightTuneIndex < 0)
                    selectedLightTuneIndex = 2;
                else if (selectedLightTuneIndex > 2)
                    selectedLightTuneIndex = 0;
                selectedColumnIndex = 0;
                DrawCurrentScreen();
                break;
        }
    }

    void HandleConfirm()
    {
        switch (currentScreen)
        {
            case Screen.NavMain:
                navMenu.Select();
                HandleNavChoice(navMenu.GetSelectedOption());
                navMenu.Deselect();
                break;

            case Screen.Cabinets:
            case Screen.Environment:
            case Screen.Lights:
            case Screen.Posters:
            case Screen.RoomSkins:
            case Screen.PlacedInstances:
            case Screen.Mesh:
            case Screen.Adjustments:
            case Screen.LightTune:
            case Screen.PhoneBooth:
            case Screen.Debug:
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;
        }
    }

    void HandleNavChoice(string choice)
    {
        if (RequiresRoomScanMenu() && choice != MRSceneScanState.ScanRoomMenuOption)
            return;

        switch (choice)
        {
            case MRSceneScanState.ScanRoomMenuOption:
                BeginRoomScanFromMenu();
                return;
            case "CABINETS":
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Cabinets;
                break;
            case "ADJUSTMENTS":
                selectedAdjustmentIndex = 0;
                selectedColumnIndex = 0;
                currentScreen = Screen.Adjustments;
                break;
            case "PHONE BOOTH":
                selectedColumnIndex = 0;
                currentScreen = Screen.PhoneBooth;
                break;
            case "ENVIRONMENT":
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Environment;
                break;
            case "MESH":
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Mesh;
                break;
            case "LIGHTS":
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Lights;
                break;
            case "POSTERS":
                RefreshPostersCatalog();
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Posters;
                break;
            case "ROOM SKIN":
                RefreshRoomSkinsCatalog();
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.RoomSkins;
                break;
            case "DEBUG":
                listScrollOffset = 0;
                debugDisplayLines.Clear();
                debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());
                selectedListIndex = FindFirstDebugErrorLineIndex();
                currentScreen = Screen.Debug;
                break;
            case "HELP":
                currentScreen = Screen.Help;
                break;
            case "MOVE CONFIG":
                if (MRConfigurationCabinetController.Instance == null
                    || !MRConfigurationCabinetController.Instance.BeginRepositionWithRay())
                {
                    ConfigManager.WriteConsoleWarning(
                        $"{LogPrefix} MOVE CONFIG failed (cabinet missing or placement ray blocked)");
                    MRDebugLog.LogWarning("MOVE CONFIG failed (cabinet missing or placement ray blocked)");
                    MRTransitionLog.LogWarning("MOVE CONFIG failed");
                    navCooldown = navRepeatDelay;
                    SyncConfirmControlEdgeState();
                    DrawCurrentScreen();
                }
                return;
            case "EXIT":
                MRConfigurationCabinetController.Instance?.CloseEdit();
                return;
            default:
                return;
        }

        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    void BeginRoomScanFromMenu()
    {
        if (scanRequestInProgress)
            return;

        if (scanRoomCoroutine != null)
            StopCoroutine(scanRoomCoroutine);

        scanRoomCoroutine = StartCoroutine(RunRoomScanFromMenu());
    }

    IEnumerator RunRoomScanFromMenu()
    {
        scanRequestInProgress = true;
        currentScreen = Screen.ScanInProgress;
        DrawCurrentScreen();

        Transform player = ResolvePlayerTransform();
        yield return MRSceneScanRequest.RunSpaceSetupAndReload(player);

        scanRequestInProgress = false;
        scanRoomCoroutine = null;
        BuildNavMenu();
        currentScreen = Screen.NavMain;
        navMenu.selectedIndex = 0;
        navMenu.Deselect();
        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();

        if (MRSceneScanState.IsRoomScanned())
            ConfigManager.WriteConsole($"{LogPrefix} room scan complete — full menu unlocked");
        else
            ConfigManager.WriteConsoleWarning($"{LogPrefix} room scan finished without MRUK room");
    }

    static Transform ResolvePlayerTransform()
    {
        var pc = Object.FindObjectOfType<PlayerController>();
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

    void SyncConfirmControlEdgeState()
    {
        confirmControlWasActive = ControlActive(LC.JOYPAD_A);
        secondaryControlWasActive = ControlActive(LC.JOYPAD_Y);
        backControlWasActive = IsBackControlActive();
#if !UNITY_EDITOR
        if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch))
            confirmControlWasActive = true;
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            secondaryControlWasActive = true;
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            backControlWasActive = true;
#endif
    }

    void SyncSecondaryControlEdgeState()
    {
        secondaryControlWasActive = ControlActive(LC.JOYPAD_Y);
#if !UNITY_EDITOR
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            secondaryControlWasActive = true;
#endif
    }

    void SyncBackControlEdgeState()
    {
        backControlWasActive = IsBackControlActive();
    }

    bool IsBackControlActive()
    {
        bool active = ControlActive(LC.JOYPAD_B) || ControlActive(LC.JOYPAD_X);
#if UNITY_EDITOR
        if (MREditorInput.IsAnyHeld(
                KeyCode.Backspace, KeyCode.B, KeyCode.Escape, KeyCode.JoystickButton1))
            active = true;
#else
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.RTouch))
            active = true;
#endif
        return active;
    }

    void HandleBack()
    {
        switch (currentScreen)
        {
            case Screen.DebugDetail:
                currentScreen = Screen.Debug;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.LightTune:
                currentScreen = Screen.PlacedInstances;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.PlacedInstances:
                currentScreen = instancesReturnScreen;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.Cabinets:
            case Screen.Adjustments:
            case Screen.PhoneBooth:
            case Screen.Mesh:
            case Screen.Lights:
            case Screen.Posters:
            case Screen.RoomSkins:
            case Screen.Environment:
            case Screen.Debug:
            case Screen.Help:
                currentScreen = Screen.NavMain;
                navMenu.selectedIndex = 0;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.NavMain:
                MRConfigurationCabinetController.Instance?.CloseEdit();
                break;
        }
    }

    void QueueReturnToPlacedInstances(MREnvironmentCatalogEntry entry, Screen returnScreen)
    {
        instancesCatalogEntry = entry;
        instancesReturnScreen = returnScreen;
        pendingReturnToPlacedInstances = true;
    }

    void BeginAddEnvironmentWithRay(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (entry.Source == MREnvironmentObjectSource.RoomSkin)
        {
            BeginAddRoomSkinInstant(entry);
            return;
        }

        if (placementRay == null || placementRay.IsActive)
            return;

        if (entry.Source == MREnvironmentObjectSource.Custom)
        {
            StartCoroutine(BeginAddCustomEnvironmentWithRayCoroutine(entry));
            return;
        }

        if (entry.Source == MREnvironmentObjectSource.Light)
        {
            BeginAddLightWithRay(entry);
            return;
        }

        if (entry.Source == MREnvironmentObjectSource.Poster)
        {
            BeginAddPosterWithRay(entry);
            return;
        }

        BeginAddBuildEnvironmentWithRay(entry);
    }

    void BeginAddRoomSkinInstant(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (placementRay != null && placementRay.IsActive)
            return;

        StartCoroutine(BeginAddRoomSkinInstantCoroutine(entry));
    }

    IEnumerator BeginAddRoomSkinInstantCoroutine(MREnvironmentCatalogEntry entry)
    {
        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        yield return MRRoomSurfaceSkin.ApplyPackageWhenReady(entry.Key);

        bool saved = envRegistry.TryFinalizeRoomSkinInstant(entry);
        if (!saved)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} room skin add failed for {entry}");
            MRDebugLog.LogError($"Room skin add failed: {entry}");
        }

        ShowIdleAfterExternalPlacement();
        ConfigManager.WriteConsole($"{LogPrefix} room skin applied {entry}");
    }

    void BeginAddPosterWithRay(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (placementRay.IsActive)
            return;

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        ComputeInitialPlacementPose(PlacementSurfaceType.Wall, out Vector3 worldPos, out Quaternion worldRot);

        if (!envRegistry.TrySpawnTransientCatalogEntry(entry, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} poster add ray spawn failed for {entry}");
            MRDebugLog.LogError($"Poster add failed: {entry} (spawn)");
            return;
        }

        BeginEnvironmentPlacementRay(entry, root);
        ConfigManager.WriteConsole($"{LogPrefix} poster add ray begin {entry}");
    }

    void BeginAddLightWithRay(MREnvironmentCatalogEntry entry)
    {
        string prefabName = entry.Key;
        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        GameObject prefab = MRLightsCatalog.LoadPrefab(prefabName);
        MRPlacementProfile prefabProfile = MRPlacementProfile.Resolve(prefab);
        PlacementSurfaceType initialSurface = prefabProfile != null
            ? prefabProfile.surfaceType
            : PlacementSurfaceType.Floor;
        ComputeInitialPlacementPose(initialSurface, out Vector3 worldPos, out Quaternion worldRot);

        if (!envRegistry.TrySpawnTransientCatalogEntry(entry, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} light add ray spawn failed for {entry}");
            MRDebugLog.LogError($"Light add failed: {entry} (spawn)");
            return;
        }

        BeginEnvironmentPlacementRay(entry, root);
        ConfigManager.WriteConsole($"{LogPrefix} light add ray begin {entry}");
    }

    void BeginAddBuildEnvironmentWithRay(MREnvironmentCatalogEntry entry)
    {
        string prefabName = entry.Key;
        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        GameObject prefab = MREnvironmentCatalog.LoadPrefab(prefabName);
        MRPlacementProfile prefabProfile = MRPlacementProfile.Resolve(prefab);
        PlacementSurfaceType initialSurface = prefabProfile != null
            ? prefabProfile.surfaceType
            : PlacementSurfaceType.Floor;
        ComputeInitialPlacementPose(initialSurface, out Vector3 worldPos, out Quaternion worldRot);

        if (!envRegistry.TrySpawnTransientCatalogEntry(entry, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} env add ray spawn failed for {entry}");
            MRDebugLog.LogError($"Environment add failed: {entry} (spawn)");
            return;
        }

        BeginEnvironmentPlacementRay(entry, root);
        ConfigManager.WriteConsole($"{LogPrefix} env add ray begin {entry}");
    }

    IEnumerator BeginAddCustomEnvironmentWithRayCoroutine(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementRay.IsActive)
            yield break;

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        PlacementSurfaceType initialSurface = PlacementSurfaceType.Floor;
        if (MRCustomObjectDefinition.TryLoad(entry.Key, out MRCustomObjectDefinition definition))
            initialSurface = definition.GetSurfaceType();

        ComputeInitialPlacementPose(initialSurface, out Vector3 worldPos, out Quaternion worldRot);

        var spawnResult = new CustomObjectSpawnResult();
        yield return envRegistry.TrySpawnTransientCatalogEntryAsync(
            entry, mrSpaceOrigin, worldPos, worldRot, spawnResult);

        if (!spawnResult.Success || spawnResult.Root == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} custom env spawn failed for {entry}");
            MRDebugLog.LogError($"Custom object add failed: {entry} (spawn)");
            yield break;
        }

        BeginEnvironmentPlacementRay(entry, spawnResult.Root);
        ConfigManager.WriteConsole($"{LogPrefix} custom env add ray begin {entry}");
    }

    void BeginEnvironmentPlacementRay(MREnvironmentCatalogEntry entry, GameObject root)
    {
        placementAddActive = true;
        pendingAddIsEnvironment = true;
        pendingAddEnvEntry = entry;
        pendingAddCabinetName = null;
        pendingAddRoot = root;

        MRPlacementProfile profile = MRPlacementProfile.Resolve(root);
        PlacementSurfaceType surfaceType = profile != null
            ? profile.surfaceType
            : PlacementSurfaceType.Floor;
        PlacementFacingAxis facingAxis = profile != null
            ? profile.facingAxis
            : PlacementFacingAxis.PositiveZ;

        placementRay.BeginMove(
            root,
            surfaceType,
            facingAxis,
            confirmCallback: (finalPos, finalRot, anchorUuid) =>
            {
                placementAddActive = false;
                MREnvironmentCatalogEntry catalogEntry = pendingAddEnvEntry;
                GameObject spawned = pendingAddRoot;
                pendingAddEnvEntry = default;
                pendingAddIsEnvironment = false;
                pendingAddRoot = null;

                if (spawned != null)
                    spawned.transform.SetPositionAndRotation(finalPos, finalRot);

                bool saved = envRegistry.TryFinalizeTransientCatalogEntry(
                    catalogEntry, spawned, mrSpaceOrigin, finalPos, finalRot, anchorUuid);

                if (!saved)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} env add confirm but save failed ({catalogEntry})");
                    MRDebugLog.LogError($"Environment add failed: {catalogEntry} (save layout)");
                }

                ShowIdleAfterExternalPlacement();
            },
            cancelCallback: () =>
            {
                CancelPendingAdd(destroyProp: true);
                ShowIdleAfterExternalPlacement();
            });
    }

    void BeginMoveEnvByPlacementId(string placementId)
    {
        if (envRegistry == null || placementRay == null || placementEnvMoveActive || placementMoveActive)
            return;

        if (string.IsNullOrEmpty(placementId))
            return;

        MREnvironmentPlacement placement = envRegistry.FindPlacementById(placementId);
        if (placement == null)
            return;

        if (!envRegistry.TryGetSpawnedRoot(placement.Id, out GameObject spawnedRoot) || spawnedRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} env move skipped, spawned root missing for {placement.Id}");
            MRDebugLog.LogWarning($"Environment move skipped: root missing ({placement.Id})");
            return;
        }

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        placementEnvMoveActive = true;
        movingEnvPlacementId = placement.Id;
        if (currentScreen == Screen.PlacedInstances)
            pendingReturnToPlacedInstances = true;

        placementRay.BeginMove(
            spawnedRoot,
            placement.SurfaceType,
            placement.FacingAxis,
            confirmCallback: (worldPos, worldRot, anchorUuid) =>
            {
                placementEnvMoveActive = false;
                bool saved = envRegistry.TryUpdatePlacementPose(
                    movingEnvPlacementId, mrSpaceOrigin, worldPos, worldRot, anchorUuid);
                if (!saved)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} env move confirm but save failed ({movingEnvPlacementId})");
                    MRDebugLog.LogError($"Environment move failed: save layout ({movingEnvPlacementId})");
                }

                movingEnvPlacementId = null;
                ShowIdleAfterExternalPlacement();
            },
            cancelCallback: () =>
            {
                placementEnvMoveActive = false;
                movingEnvPlacementId = null;
                ShowIdleAfterExternalPlacement();
            });

        ConfigManager.WriteConsole($"{LogPrefix} env move begin {placement.DisplayLabel} ({placement.Id})");
    }

    void BeginAddCabinetWithRay(string cabinetName)
    {
        if (registry == null || placementRay == null || placementMoveActive || placementAddActive)
            return;

        if (placementRay.IsActive)
            return;

        StartCoroutine(BeginAddCabinetWithRayCoroutine(cabinetName));
    }

    IEnumerator BeginAddCabinetWithRayCoroutine(string cabinetName)
    {
        placementAddActive = true;
        pendingAddIsEnvironment = false;
        pendingAddCabinetName = cabinetName;
        pendingAddEnvEntry = default;
        pendingAddRoot = null;

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        ComputeInitialFloorPose(out Vector3 worldPos, out Quaternion worldRot);

        MRTransitionLog.LogStep("PlacementAdd", $"spawn begin {cabinetName}");
        var spawnResult = new MRLayoutRegistry.CabinetSpawnYieldResult();
        yield return registry.TrySpawnTransientCabinetAsync(
            cabinetName, mrSpaceOrigin, worldPos, worldRot, spawnResult);

        if (!spawnResult.Success || spawnResult.Root == null)
        {
            CancelPendingAdd(destroyProp: false);
            ConfigManager.WriteConsoleWarning($"{LogPrefix} add ray spawn failed for {cabinetName}");
            MRDebugLog.LogError($"Cabinet add failed: {cabinetName} (spawn)");
            ShowIdleAfterExternalPlacement();
            yield break;
        }

        GameObject root = spawnResult.Root;
        pendingAddRoot = root;
        MRTransitionLog.LogStep("PlacementAdd", $"spawn done {cabinetName} — placement ray");

        MRPlacementProfile profile = MRPlacementProfile.Resolve(root);
        PlacementSurfaceType surfaceType = profile != null
            ? profile.surfaceType
            : PlacementSurfaceType.Floor;
        PlacementFacingAxis facingAxis = profile != null
            ? profile.facingAxis
            : PlacementFacingAxis.PositiveZ;

        placementRay.BeginMove(
            root,
            surfaceType,
            facingAxis,
            confirmCallback: (finalPos, finalRot, anchorUuid) =>
            {
                placementAddActive = false;
                string name = pendingAddCabinetName;
                GameObject spawned = pendingAddRoot;
                pendingAddCabinetName = null;
                pendingAddRoot = null;

                if (spawned != null)
                    spawned.transform.SetPositionAndRotation(finalPos, finalRot);

                bool saved = registry.TryFinalizeTransientCabinetAdd(
                    name, spawned, mrSpaceOrigin, finalPos, finalRot, anchorUuid);

                if (!saved)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} add confirm but save failed ({name})");
                    MRDebugLog.LogError($"Cabinet add failed: {name} (save layout)");
                }

                ShowIdleAfterExternalPlacement();
            },
            cancelCallback: () =>
            {
                CancelPendingAdd(destroyProp: true);
                ShowIdleAfterExternalPlacement();
            });

        ConfigManager.WriteConsole($"{LogPrefix} add ray begin {cabinetName}");
    }

    void ShowIdleAfterExternalPlacement()
    {
        if (pendingReturnToPlacedInstances)
        {
            pendingReturnToPlacedInstances = false;
            RefreshPlacedInstancesList();
            currentScreen = Screen.PlacedInstances;
            if (placedInstances.Count > 0)
                selectedListIndex = placedInstances.Count - 1;
            else
                selectedListIndex = 0;
            ClampListScroll();
        }

        if (sessionActive)
            DrawCurrentScreen();
        else
            ShowIdle();
    }

    void CancelPendingAdd(bool destroyProp)
    {
        placementAddActive = false;
        if (destroyProp && pendingAddRoot != null)
        {
            if (pendingAddIsEnvironment && envRegistry != null)
                envRegistry.DestroyTransientProp(pendingAddRoot);
            else if (registry != null)
                registry.DestroyTransientCabinet(pendingAddRoot);
        }

        pendingAddCabinetName = null;
        pendingAddEnvEntry = default;
        pendingAddIsEnvironment = false;
        pendingAddRoot = null;
    }

    void BeginMoveCabinetByCatalogName(string cabinetDBName)
    {
        if (registry == null || placementRay == null || placementMoveActive || placementEnvMoveActive)
            return;

        MRCabinetPlacement placement = registry.FindPlacementByCabinetDBName(cabinetDBName);
        if (placement == null || string.IsNullOrEmpty(placement.Id))
            return;

        if (!registry.TryGetSpawnedRoot(placement.Id, out GameObject spawnedRoot) || spawnedRoot == null)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} move skipped, spawned root missing for {placement.Id}");
            MRDebugLog.LogWarning($"Cabinet move skipped: root missing ({placement.Id})");
            return;
        }

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        placementMoveActive = true;
        movingPlacementId = placement.Id;

        placementRay.BeginMove(
            spawnedRoot,
            placement.SurfaceType,
            placement.FacingAxis,
            confirmCallback: (worldPos, worldRot, anchorUuid) =>
            {
                placementMoveActive = false;
                bool saved = registry.TryUpdatePlacementPose(
                    movingPlacementId, mrSpaceOrigin, worldPos, worldRot, anchorUuid);
                if (!saved)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} move confirm but save failed ({movingPlacementId})");
                    MRDebugLog.LogError($"Cabinet move failed: save layout ({movingPlacementId})");
                }

                movingPlacementId = null;
                ShowIdleAfterExternalPlacement();
            },
            cancelCallback: () =>
            {
                placementMoveActive = false;
                movingPlacementId = null;
                ShowIdleAfterExternalPlacement();
            });

        ConfigManager.WriteConsole($"{LogPrefix} move begin {placement.DisplayLabel} ({placement.Id})");
    }

    void ClampListScroll()
    {
        int count = GetListCount();
        if (count <= 0)
        {
            selectedListIndex = 0;
            listScrollOffset = 0;
            return;
        }

        selectedListIndex = Mathf.Clamp(selectedListIndex, 0, count - 1);
        int maxOffset = Mathf.Max(0, count - VisibleCabinetRows);
        if (selectedListIndex < listScrollOffset)
            listScrollOffset = selectedListIndex;
        else if (selectedListIndex >= listScrollOffset + VisibleCabinetRows)
            listScrollOffset = selectedListIndex - VisibleCabinetRows + 1;
        listScrollOffset = Mathf.Clamp(listScrollOffset, 0, maxOffset);
    }

    void ClampLightsScroll()
    {
        int count = Mathf.Max(1, 1 + lightsCatalogEntries.Count);
        selectedListIndex = Mathf.Clamp(selectedListIndex, 0, count - 1);

        int catalogCount = lightsCatalogEntries.Count;
        int catalogVisibleRows = Mathf.Max(1, VisibleCabinetRows - 1);
        if (selectedListIndex == LightsAutoRowIndex || catalogCount == 0)
        {
            listScrollOffset = 0;
            return;
        }

        int catalogIdx = selectedListIndex - 1;
        int maxOffset = Mathf.Max(0, catalogCount - catalogVisibleRows);
        if (catalogIdx < listScrollOffset)
            listScrollOffset = catalogIdx;
        else if (catalogIdx >= listScrollOffset + catalogVisibleRows)
            listScrollOffset = catalogIdx - catalogVisibleRows + 1;
        listScrollOffset = Mathf.Clamp(listScrollOffset, 0, maxOffset);
    }

    void ComputeInitialFloorPose(out Vector3 worldPos, out Quaternion worldRot)
    {
        ComputeInitialPlacementPose(PlacementSurfaceType.Floor, out worldPos, out worldRot);
    }

    void ComputeInitialPlacementPose(PlacementSurfaceType surfaceType, out Vector3 worldPos, out Quaternion worldRot)
    {
        ComputeSpawnPose(out worldPos, out worldRot);

        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        if (surfaces == null)
            return;

        if (surfaceType == PlacementSurfaceType.Ceiling)
        {
            if (surfaces.TryGetCeilingPointAt(worldPos, out Vector3 ceilingPoint))
                worldPos = ceilingPoint;
            return;
        }

        if (surfaceType == PlacementSurfaceType.Table)
        {
            if (surfaces.TryGetTablePointAt(worldPos, out Vector3 tablePoint))
                worldPos = tablePoint;
            return;
        }

        if (surfaceType == PlacementSurfaceType.Wall)
        {
            Transform player = Camera.main != null ? Camera.main.transform : transform;
            if (surfaces.TryGetWallMountedFramePose(
                    player,
                    spawnDistanceMeters,
                    0.05f,
                    out worldPos,
                    out worldRot,
                    PlacementFacingAxis.NegativeX))
                return;
        }

        if (surfaces.TryGetFloorPointAt(worldPos, out Vector3 floorPoint))
            worldPos = floorPoint;
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
        worldPos.y = (mrSpaceOrigin != null ? mrSpaceOrigin.position.y : 0f) + spawnYOffsetMeters;
        worldRot = Quaternion.LookRotation(-forward, Vector3.up);
    }

    void setupActionMap()
    {
        if (libretroControlMap == null)
            libretroControlMap = GetComponent<LibretroControlMap>();
        if (libretroControlMap == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} LibretroControlMap missing");
            return;
        }

        ControlMapConfiguration conf = new DefaultControlMap();
#if UNITY_EDITOR
        // Override conflicting default button bindings for editor MR menu flow.
        conf.RemoveMaps(LC.JOYPAD_A);
        conf.RemoveMaps(LC.JOYPAD_B);
        conf.RemoveMaps(LC.JOYPAD_X);
        conf.RemoveMaps(LC.JOYPAD_Y);

        conf.AddMap(LC.JOYPAD_UP, ControlMapPathDictionary.KEYBOARD_W);
        conf.AddMap(LC.JOYPAD_DOWN, ControlMapPathDictionary.KEYBOARD_S);
        conf.AddMap(LC.JOYPAD_LEFT, ControlMapPathDictionary.KEYBOARD_A);
        conf.AddMap(LC.JOYPAD_RIGHT, ControlMapPathDictionary.KEYBOARD_D);
        conf.AddMap(LC.JOYPAD_A, ControlMapPathDictionary.KEYBOARD_ENTER);
        conf.AddMap(LC.JOYPAD_B, ControlMapPathDictionary.KEYBOARD_ESC);
        conf.AddMap(LC.JOYPAD_X, ControlMapPathDictionary.KEYBOARD_ESC);
        conf.AddMap(LC.JOYPAD_Y, ControlMapPathDictionary.KEYBOARD_SPACE);
#endif
        libretroControlMap.CreateFromConfiguration(conf, "inputMap_MRConfiguration_" + name);
        libretroControlMap.Enable(true);
    }

    void cleanActionMap()
    {
        if (libretroControlMap != null)
            libretroControlMap.Clean();
    }

    void ActivateShader(bool online)
    {
        if (screen == null)
            return;

        screen.ActivateShader(online ? shaderOnline : shaderOffline);
    }

    bool ControlActive(string mameControl)
    {
        if (libretroControlMap == null)
            return false;

        try
        {
            return libretroControlMap.Active(mameControl) != 0;
        }
        catch
        {
            libretroControlMap.Enable(true);
            return false;
        }
    }

    static MREnvironmentRegistry EnsureEnvironmentRegistry()
    {
        if (MREnvironmentRegistry.Instance != null)
            return MREnvironmentRegistry.Instance;

        var existing = FindObjectOfType<MREnvironmentRegistry>();
        if (existing != null)
            return existing;

        var go = new GameObject("MREnvironmentRegistry");
        return go.AddComponent<MREnvironmentRegistry>();
    }

    static MRLayoutRegistry EnsureRegistry()
    {
        if (MRLayoutRegistry.Instance != null)
            return MRLayoutRegistry.Instance;

        var existing = FindObjectOfType<MRLayoutRegistry>();
        if (existing != null)
            return existing;

        var go = new GameObject("MRLayoutRegistry");
        return go.AddComponent<MRLayoutRegistry>();
    }

    static Transform EnsureMrSpaceOrigin()
    {
        if (MixedRealityManager.Instance != null && MixedRealityManager.Instance.MRSpaceOrigin != null)
            return MixedRealityManager.Instance.MRSpaceOrigin;

        var existing = GameObject.Find("MRSpaceOrigin_TestUI");
        if (existing != null)
            return existing.transform;

        var go = new GameObject("MRSpaceOrigin_TestUI");
        return go.transform;
    }

    MRPlacementRayController EnsurePlacementRayController()
    {
        MRPlacementRayController existing = FindObjectOfType<MRPlacementRayController>();
        if (existing != null)
            return existing;

        GameObject host = new GameObject("MRPlacementRayController");
        return host.AddComponent<MRPlacementRayController>();
    }

    int FindFirstDebugErrorLineIndex()
    {
        for (int i = 0; i < debugDisplayLines.Count; i++)
        {
            if (!debugDisplayLines[i].IsDateHeader)
                return i;
        }

        return 0;
    }

    int MoveDebugSelection(int delta)
    {
        if (debugDisplayLines.Count == 0)
            return 0;

        int index = selectedListIndex;
        for (int attempt = 0; attempt < debugDisplayLines.Count; attempt++)
        {
            index += delta;
            if (index < 0)
                index = debugDisplayLines.Count - 1;
            else if (index >= debugDisplayLines.Count)
                index = 0;

            if (!debugDisplayLines[index].IsDateHeader)
                return index;
        }

        return selectedListIndex;
    }

    bool TryOpenDebugDetailFromSelection()
    {
        if (selectedListIndex < 0 || selectedListIndex >= debugDisplayLines.Count)
            return false;

        MRDebugLog.DisplayLine line = debugDisplayLines[selectedListIndex];
        if (line.IsDateHeader || line.EntryIndex < 0)
            return false;

        if (!MRDebugLog.TryGetEntry(line.EntryIndex, out MRDebugLog.Entry entry))
            return false;

        debugDetailEntry = entry;
        debugDetailScrollOffset = 0;
        BuildDebugDetailWrappedLines();
        currentScreen = Screen.DebugDetail;
        return true;
    }

    void BuildDebugDetailWrappedLines()
    {
        debugDetailWrappedLines.Clear();
        if (string.IsNullOrEmpty(debugDetailEntry.Message))
            return;

        debugDetailWrappedLines.AddRange(WrapDebugText(debugDetailEntry.Message, DebugDetailWrapWidth));
    }

    static List<string> WrapDebugText(string text, int maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(text) || maxWidth <= 0)
            return lines;

        int pos = 0;
        while (pos < text.Length)
        {
            int remaining = text.Length - pos;
            int len = Mathf.Min(maxWidth, remaining);

            if (len < remaining)
            {
                int breakAt = text.LastIndexOf(' ', pos + len - 1, len);
                if (breakAt > pos)
                    len = breakAt - pos;
                else if (len < maxWidth)
                    len = Mathf.Min(maxWidth, remaining);
            }

            if (len <= 0)
                len = Mathf.Min(maxWidth, remaining);

            string chunk = text.Substring(pos, len).Trim();
            if (!string.IsNullOrEmpty(chunk))
                lines.Add(chunk);

            pos += len;
            while (pos < text.Length && text[pos] == ' ')
                pos++;

            if (len <= 0)
                break;
        }

        return lines;
    }

    static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value))
            return "(unnamed)";
        if (value.Length <= maxLen)
            return value;
        return value.Substring(0, maxLen - 3) + "...";
    }

    static string PadRight(string value, int width)
    {
        if (value.Length >= width)
            return value.Substring(0, width);
        return value.PadRight(width);
    }

    float ReadStickY()
    {
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(KeyCode.UpArrow) || MREditorInput.IsHeld(KeyCode.W))
            return 1f;
        if (MREditorInput.IsHeld(KeyCode.DownArrow) || MREditorInput.IsHeld(KeyCode.S))
            return -1f;
        return MREditorInput.GamepadStickY();
#else
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        return stick.y;
#endif
    }

    float ReadStickX()
    {
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(KeyCode.D) || MREditorInput.IsHeld(KeyCode.RightArrow))
            return 1f;
        if (MREditorInput.IsHeld(KeyCode.A) || MREditorInput.IsHeld(KeyCode.LeftArrow))
            return -1f;
        return MREditorInput.GamepadStickX();
#else
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        return stick.x;
#endif
    }

    bool WasMoveUp() =>
#if UNITY_EDITOR
        MREditorInput.WasAnyPressed(KeyCode.UpArrow, KeyCode.W);
#else
        false;
#endif

    bool WasMoveDown() =>
#if UNITY_EDITOR
        MREditorInput.WasAnyPressed(KeyCode.DownArrow, KeyCode.S);
#else
        false;
#endif

    bool WasMoveLeft() =>
#if UNITY_EDITOR
        MREditorInput.WasAnyPressed(KeyCode.LeftArrow, KeyCode.A);
#else
        false;
#endif

    bool WasMoveRight() =>
#if UNITY_EDITOR
        MREditorInput.WasAnyPressed(KeyCode.RightArrow, KeyCode.D);
#else
        false;
#endif

    bool WasSecondaryPressed()
    {
        bool active = ControlActive(LC.JOYPAD_Y);
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(KeyCode.Space) || MREditorInput.IsHeld(KeyCode.JoystickButton3))
            active = true;
#else
        if (OVRInput.Get(OVRInput.Button.Two, OVRInput.Controller.LTouch))
            active = true;
#endif

        bool pressed = active && !secondaryControlWasActive;
        secondaryControlWasActive = active;
        return pressed;
    }

    bool WasConfirmPressed()
    {
        bool active = ControlActive(LC.JOYPAD_A);
#if UNITY_EDITOR
        if (MREditorInput.IsAnyHeld(KeyCode.Return, KeyCode.JoystickButton0))
            active = true;
#else
        if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch))
            active = true;
#endif

        bool pressed = active && !confirmControlWasActive;
        confirmControlWasActive = active;
        return pressed;
    }

    bool WasBackPressed()
    {
        bool active = IsBackControlActive();
        bool pressed = active && !backControlWasActive;
        backControlWasActive = active;
        return pressed;
    }
}
