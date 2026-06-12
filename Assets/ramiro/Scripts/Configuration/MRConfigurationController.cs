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
    const int MeshOptionCount = 2;

    enum Screen
    {
        Idle,
        NavMain,
        Cabinets,
        Adjustments,
        PhoneBooth,
        Environment,
        Mesh,
        Debug,
        DebugDetail,
        Help
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
    int selectedAdjustmentIndex;
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
    GameObject pendingAddRoot;

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
        registry.EnsureLayoutLoaded();
        envRegistry.EnsureLayoutLoaded();
        registry.SpawnAll(mrSpaceOrigin);
        envRegistry.SpawnAll(mrSpaceOrigin);

        MRCatalogBootstrap.PrepareCatalog(seedExampleCabinetInEditor);
        RefreshCatalog();
        MREnvironmentCatalog.RefreshCache();
        MRCustomObjectCatalog.RefreshCache();
        RefreshEnvironmentCatalog();

        setupActionMap();
        sessionActive = true;
        confirmControlWasActive = false;
        secondaryControlWasActive = false;
        backControlWasActive = false;
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
            if (currentScreen == Screen.Adjustments)
            {
                int adjustDir = ReadHorizontalAdjustDirection();
                if (adjustDir != 0)
                {
                    ApplyAdjustmentChange(adjustDir);
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

        if (WasSecondaryPressed())
        {
            if (navCooldown > 0f)
                SyncSecondaryControlEdgeState();
            else
                HandleSecondaryAction();
        }
    }

    void BuildNavMenu()
    {
        navMenu = new GenericMenu(screen, "MR CONFIGURATION");
        navMenu.AddOption("CABINETS", "Catalog: add or remove in MR space");
        navMenu.AddOption("ENVIRONMENT", "Build + Custom Objects");
        navMenu.AddOption("MESH", "EffectMesh layers (occluder walls)");
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

    void DrawCurrentScreen()
    {
        screen.Clear();

        switch (currentScreen)
        {
            case Screen.NavMain:
                navMenu.DrawMenu();
                DrawFooter("STICK: move   A: select   B: back");
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

        screen.Print(1, 2, "Name", false);
        screen.Print(22, 2, "In", false);
        screen.Print(28, 2, "Act", false);
        screen.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= catalogNames.Count)
                break;

            string name = Truncate(catalogNames[idx], 18);
            bool inScene = registry != null && registry.IsCabinetInScene(catalogNames[idx]);
            bool selected = idx == selectedListIndex;

            string line = PadRight(name, 20) + (inScene ? "Yes" : " No") + " " + (inScene ? "Rem" : "Add");
            screen.Print(1, row, selected ? "> " + line : "  " + line, selected);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < catalogNames.Count)
            screen.Print(1, 20, Truncate(catalogNames[selectedListIndex], 36), false);

        bool selectedInScene = selectedListIndex >= 0 && selectedListIndex < catalogNames.Count
            && registry != null && registry.IsCabinetInScene(catalogNames[selectedListIndex]);
        DrawFooter(selectedInScene ? "A: Rem   Y: move   B: back" : "A: Add   B: back");
    }

    void DrawAdjustmentsPage()
    {
        screen.PrintCentered(0, "ADJUSTMENTS", true);
        screen.PrintLine(1, false, '-');

        float scale = MRAdjustmentsSettings.CabinetScale;
        float floorPos = MRAdjustmentsSettings.FloorCabinetPosition;

        string scaleLine = $"Scale Cabinets {scale:F2}";
        string floorLine = $"Floor Cabinets Position {floorPos:F2}";

        screen.Print(1, 4, selectedAdjustmentIndex == 0 ? "> " + scaleLine : "  " + scaleLine, selectedAdjustmentIndex == 0);
        screen.Print(1, 6, selectedAdjustmentIndex == 1 ? "> " + floorLine : "  " + floorLine, selectedAdjustmentIndex == 1);

        screen.Print(1, 10, "1.00 = default", false);
        DrawFooter("R stick: select/adjust +/-0.01   B: back");
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

        screen.Print(1, 2, "Name", false);
        screen.Print(22, 2, "In", false);
        screen.Print(28, 2, "Act", false);
        screen.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleCabinetRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= envCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = envCatalogEntries[idx];
            string label = Truncate(entry.MenuLabel, 20);
            bool inScene = envRegistry != null && envRegistry.IsCatalogEntryInScene(entry);
            bool selected = idx == selectedListIndex;

            string line = PadRight(label, 20) + (inScene ? "Yes" : " No") + " " + (inScene ? "Rem" : "Add");
            screen.Print(1, row, selected ? "> " + line : "  " + line, selected);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < envCatalogEntries.Count)
            screen.Print(1, 20, Truncate(envCatalogEntries[selectedListIndex].ToString(), 36), false);

        bool selectedInScene = selectedListIndex >= 0 && selectedListIndex < envCatalogEntries.Count
            && envRegistry != null && envRegistry.IsCatalogEntryInScene(envCatalogEntries[selectedListIndex]);
        DrawFooter(selectedInScene ? "A: Rem   Y: move   B: back" : "A: Add   B: back");
    }

    void DrawMeshPage()
    {
        screen.PrintCentered(0, "MESH", true);
        screen.PrintLine(1, false, '-');

        screen.Print(1, 2, "Name", false);
        screen.Print(22, 2, "On", false);
        screen.Print(28, 2, "Act", false);
        screen.PrintLine(3, false, '-');

        DrawMeshRow(
            4,
            MREffectMeshVisibility.AnchorMeshLabel,
            MREffectMeshSettings.AnchorMeshEnabled,
            MREffectMeshVisibility.GetAnchorStatusLabel(),
            selectedListIndex == 0);
        DrawMeshRow(
            6,
            MREffectMeshVisibility.GlobalMeshLabel,
            MREffectMeshSettings.GlobalMeshEnabled,
            MREffectMeshVisibility.GetGlobalStatusLabel(),
            selectedListIndex == 1);

        screen.Print(1, 12, "Occluder walls in MR", false);
        screen.Print(1, 13, "OFF saves GPU / room scan", false);
        DrawFooter("A: toggle On/Off   B: back");
    }

    void DrawMeshRow(int row, string label, bool enabled, string status, bool selected)
    {
        string line = PadRight(Truncate(label, 18), 20)
            + (enabled ? "Yes" : " No")
            + " "
            + (enabled ? "Off" : " On");
        screen.Print(1, row, selected ? "> " + line : "  " + line, selected);
        screen.Print(1, row + 1, $"   {Truncate(status, 34)}", false);
    }

    void DrawPhoneBoothPage()
    {
        screen.PrintCentered(0, "PHONE BOOTH", true);
        screen.PrintLine(1, false, '-');

        string status = MRPhoneBoothVisibility.GetStatusLabel();
        screen.Print(1, 4, $"Status: {status}", true);

        bool visible = MRPhoneBoothSettings.Visible;
        string action = visible ? "Hide phone booth" : "Show phone booth";
        screen.Print(1, 7, "> " + action, true);

        screen.Print(1, 10, "Hidden saves room space", false);
        screen.Print(1, 11, "Show restores last pose", false);
        DrawFooter("A: toggle show/hide   B: back");
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
            bool selected = idx == selectedListIndex;

            if (line.IsDateHeader)
            {
                screen.Print(1, row, line.Text, true);
            }
            else
            {
                string text = Truncate(line.Text, 34);
                screen.Print(1, row, selected ? "> " + text : "  " + text, selected);
            }

            row++;
        }

        DrawFooter("A: open full   Up/Down: scroll   B: back");
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

    void DrawHelpPage()
    {
        screen.PrintCentered(0, "HELP", true);
        screen.Print(2, 3, "Up/Down: navigate lists", false);
        screen.Print(2, 5, "A: Add/Remove   Y: move", false);
        screen.Print(2, 7, "B: back / close panel", false);
        screen.Print(2, 9, "Add/Move: floor ray", false);
        screen.Print(2, 10, "Adjustments: scale/floor", false);
        screen.Print(2, 11, "Environment: props", false);
        screen.Print(2, 12, "Mesh: EffectMesh on/off", false);
        screen.Print(2, 13, "Phone booth: show/hide", false);
        screen.Print(2, 14, "Debug: errors, A=full text", false);
        screen.Print(2, 15, "R stick L/R: adjust +/-0.01", false);
        screen.Print(2, 16, "R stick L/R: rotate Y*", false);
        screen.Print(2, 17, "* floor/ceiling/wall prefab", false);
        screen.Print(2, 18, "Catalog = cabinetsdb/", false);
        screen.Print(2, 19, "Env = build + Custom Objects", false);
        screen.Print(2, 20, "Layout = MR/*.yaml", false);
        DrawFooter("B: back");
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

    int GetListCount()
    {
        return currentScreen switch
        {
            Screen.Cabinets => catalogNames.Count,
            Screen.Environment => envCatalogEntries.Count,
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
            case Screen.Mesh:
                int count = GetListCount();
                if (count == 0)
                    return;

                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = count - 1;
                else if (selectedListIndex >= count)
                    selectedListIndex = 0;

                ClampListScroll();
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
                DrawCurrentScreen();
                break;
        }
    }

    void ApplyAdjustmentChange(int direction)
    {
        if (registry == null)
            return;

        if (selectedAdjustmentIndex == 0)
            MRAdjustmentsSettings.AdjustCabinetScale(direction);
        else
            MRAdjustmentsSettings.AdjustFloorCabinetPosition(direction);

        registry.ApplyGlobalAdjustmentsToSpawnedFloorCabinets();
        DrawCurrentScreen();
    }

    int ReadHorizontalAdjustDirection()
    {
        float stickX = ReadStickX();
        if (stickX > 0.55f || WasMoveRight())
            return 1;
        if (stickX < -0.55f || WasMoveLeft())
            return -1;
        return 0;
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
                if (ExecuteCabinetToggle(selectedListIndex))
                {
                    RefreshCatalog();
                    DrawCurrentScreen();
                }
                break;
            case Screen.Environment:
                if (ExecuteEnvironmentToggle(selectedListIndex))
                {
                    RefreshEnvironmentCatalog();
                    DrawCurrentScreen();
                }
                break;
            case Screen.PhoneBooth:
                MRPhoneBoothVisibility.Toggle();
                DrawCurrentScreen();
                break;
            case Screen.Mesh:
                ExecuteMeshToggle(selectedListIndex);
                DrawCurrentScreen();
                break;
            case Screen.Debug:
                debugDisplayLines.Clear();
                debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());
                if (TryOpenDebugDetailFromSelection())
                {
                    navCooldown = navRepeatDelay;
                    SyncConfirmControlEdgeState();
                    DrawCurrentScreen();
                }
                break;
        }
    }

    void HandleNavChoice(string choice)
    {
        switch (choice)
        {
            case "CABINETS":
                selectedListIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Cabinets;
                break;
            case "ADJUSTMENTS":
                selectedAdjustmentIndex = 0;
                currentScreen = Screen.Adjustments;
                break;
            case "PHONE BOOTH":
                currentScreen = Screen.PhoneBooth;
                break;
            case "ENVIRONMENT":
                selectedListIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Environment;
                break;
            case "MESH":
                selectedListIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Mesh;
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

    void HandleSecondaryAction()
    {
        switch (currentScreen)
        {
            case Screen.Cabinets:
                if (selectedListIndex >= 0 && selectedListIndex < catalogNames.Count)
                {
                    string cabinetName = catalogNames[selectedListIndex];
                    if (registry != null && registry.IsCabinetInScene(cabinetName))
                        BeginMoveCabinetByCatalogName(cabinetName);
                }
                break;
            case Screen.Environment:
                if (selectedListIndex >= 0 && selectedListIndex < envCatalogEntries.Count)
                {
                    MREnvironmentCatalogEntry entry = envCatalogEntries[selectedListIndex];
                    if (envRegistry != null && envRegistry.IsCatalogEntryInScene(entry))
                        BeginMoveEnvByCatalogEntry(entry);
                }
                break;
        }
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
            case Screen.Cabinets:
            case Screen.Adjustments:
            case Screen.PhoneBooth:
            case Screen.Mesh:
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

    void ExecuteMeshToggle(int index)
    {
        switch (index)
        {
            case 0:
                MREffectMeshVisibility.ToggleAnchorMesh();
                break;
            case 1:
                MREffectMeshVisibility.ToggleGlobalMesh();
                break;
        }
    }

    bool ExecuteEnvironmentToggle(int index)
    {
        if (envRegistry == null || index < 0 || index >= envCatalogEntries.Count)
            return false;

        MREnvironmentCatalogEntry entry = envCatalogEntries[index];
        bool inScene = envRegistry.IsCatalogEntryInScene(entry);

        if (inScene)
        {
            if (envRegistry.TryRemoveCatalogEntryFromScene(entry))
                ConfigManager.WriteConsole($"{LogPrefix} removed env prop {entry}");
            return true;
        }

        BeginAddEnvironmentWithRay(entry);
        return false;
    }

    void BeginAddEnvironmentWithRay(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (placementRay.IsActive)
            return;

        if (entry.Source == MREnvironmentObjectSource.Custom)
        {
            StartCoroutine(BeginAddCustomEnvironmentWithRayCoroutine(entry));
            return;
        }

        BeginAddBuildEnvironmentWithRay(entry);
    }

    void BeginAddBuildEnvironmentWithRay(MREnvironmentCatalogEntry entry)
    {
        string prefabName = entry.Key;
        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        GameObject prefab = MREnvironmentCatalog.LoadPrefab(prefabName);
        MRPlacementProfile prefabProfile = prefab != null ? prefab.GetComponent<MRPlacementProfile>() : null;
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

    void BeginMoveEnvByCatalogEntry(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementEnvMoveActive || placementMoveActive)
            return;

        MREnvironmentPlacement placement = envRegistry.FindPlacementByCatalogEntry(entry);
        if (placement == null || string.IsNullOrEmpty(placement.Id))
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

    bool ExecuteCabinetToggle(int index)
    {
        if (registry == null || index < 0 || index >= catalogNames.Count)
            return false;

        string cabinetName = catalogNames[index];
        bool inScene = registry.IsCabinetInScene(cabinetName);

        if (inScene)
        {
            if (registry.TryRemoveCabinetFromScene(cabinetName))
                ConfigManager.WriteConsole($"{LogPrefix} removed {cabinetName}");
            return true;
        }

        BeginAddCabinetWithRay(cabinetName);
        return false;
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
