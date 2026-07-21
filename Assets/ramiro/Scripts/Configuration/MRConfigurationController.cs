/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
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
    const int VisibleCabinetRows = 8;
    const int VisibleDebugRows = 10;
    const int VisibleDebugDetailRows = 16;
    const int VisibleHelpRows = 18;
    const int DebugDetailWrapWidth = 38;
    const int HelpWrapWidth = 38;
    const int MeshOptionCount = 3;
    const int MeshScanColorsRowIndex = 2;
    /// <summary>Bookshelf Add/Remove row before PrefabsEnvironment packages on Official Objects home.</summary>
    const int OfficialCategoryCount = 1;
    const int CustomCategoryCount = 2; // Posters + Room Skin rows before packages on home table
    const int ConfigCategoryCount = 4;
    const int GlobalLightOptionCount = 2;
    const int ListRowNameWidth = 10;
    const int CabinetListRowNameWidth = 18;
    /// <summary>Name width for flag-column lists whose ACTION cell must fit "Press A to REMOVE" (17 chars).</summary>
    const int ActionListRowNameWidth = 16;
    const int RoomSkinListRowNameWidth = ActionListRowNameWidth;
    const int TuneListRowNameWidth = 22;
    const int TableFlagWidth = 3;
    const int CabinetAlphabetCount = 27; // ALL + A-Z
    const int LightsGlobalShortcutCount = 1; // Global Light row above placeable lights
    /// <summary>CABINETS / POSTERS table below Show All/Added + separator + two alphabet rows.</summary>
    const int CabinetsTableStartRow = 6;
    const int PostersTableStartRow = 6;
    /// <summary>Unified CUSTOM OBJECTS home table (Posters + Room Skin + packages).</summary>
    const int CustomHomeTableStartRow = 2;
    int tableNameWidth = ListRowNameWidth;
    int tableFlagWidth = TableFlagWidth;
    bool tableShowFlag;
    int tableActWidth = 8;
    bool tableGridActive;

    enum CabinetListFilter
    {
        ShowAll,
        ShowAdded
    }

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
        GlobalLight,
        PhoneBooth,
        Config,
        CustomObjects,
        CustomObjectsOthers,
        OfficialObjects,
        OfficialObjectsOthers,
        Mesh,
        Lights,
        Posters,
        Magazines,
        RoomSkins,
        PlacedInstances,
        LightTune,
        Debug,
        DebugDetail,
        Help,
        ScanInProgress,
        DeleteConfigsConfirm,
        DeleteConfigsDone
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
    readonly List<string> filteredCabinetNames = new List<string>();
    CabinetListFilter cabinetListFilter = CabinetListFilter.ShowAll;
    bool cabinetFocusOnFilter = true;
    /// <summary>When <see cref="cabinetFocusOnFilter"/>, true = alphabet row, false = Show All/Added.</summary>
    bool cabinetFocusOnAlphabet;
    /// <summary>0 = ALL, 1 = A … 26 = Z.</summary>
    int cabinetLetterFilter;
    readonly List<MREnvironmentCatalogEntry> filteredEnvCatalogEntries = new List<MREnvironmentCatalogEntry>();
    CabinetListFilter envListFilter = CabinetListFilter.ShowAll;
    bool envListFocusOnFilter = true;
    /// <summary>When filter-focused on Posters: true = alphabet, false = Show All/Added.</summary>
    bool envFocusOnAlphabet;
    /// <summary>0 = ALL, 1 = A … 26 = Z (Posters letter filter).</summary>
    int envLetterFilter;
    readonly List<MREnvironmentCatalogEntry> officialHomeCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> customHomeCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> customCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> officialCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> lightsCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> postersCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> magazinesCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentCatalogEntry> roomSkinsCatalogEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<MREnvironmentPlacement> placedInstances = new List<MREnvironmentPlacement>();
    /// <summary>Show Added flat list for multi-copy catalogs (e.g. Posters): one row per placement.</summary>
    readonly List<MREnvironmentPlacement> showAddedEnvPlacements = new List<MREnvironmentPlacement>();
    readonly List<MREnvironmentCatalogEntry> showAddedEnvPlacementEntries = new List<MREnvironmentCatalogEntry>();
    readonly List<int> showAddedEnvPlacementCopyIndex = new List<int>();
    readonly List<MRDebugLog.DisplayLine> debugDisplayLines = new List<MRDebugLog.DisplayLine>();
    readonly List<string> debugDetailWrappedLines = new List<string>();
    readonly List<string> helpWrappedLines = new List<string>();
    MRDebugLog.Entry debugDetailEntry;
    int debugDetailScrollOffset;
    int helpScrollOffset;

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
    int lastDeletedYamlCount;
    Coroutine scanRoomCoroutine;
    string scanStatusLine = "";
    string pendingSelStatus;
    string pendingActStatus;
    bool cursorBlinkVisible = true;
    float cursorBlinkTimer;
    const float CursorBlinkHalfPeriod = 0.22f;

    string SelectionCursorPrefix => cursorBlinkVisible ? ">>" : "  ";

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
        // MR-only: softer CRT sampling up close. Do not change ScreenGenerator defaults (curif).
        if (screen.Screen != null)
        {
            screen.Screen.filterMode = FilterMode.Bilinear;
            screen.Screen.anisoLevel = 4;
        }
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
        if (MixedRealityManager.Instance != null)
            MixedRealityManager.Instance.EnsureLayoutPathsBoundForEditSession();
        else
            MRActiveRoom.TryBindFromDevice();
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
        RefreshObjectCatalogs();
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
        if (!MRPhoneBoothVisibility.IsPhoneBoothSuppressedForQuickTravel())
        {
            MRPhoneBoothVisibility.EnsureMrInstance();
            MRPhoneBoothVisibility.ApplySavedVisibility();
        }
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

        TickSelectionCursorBlink();

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
            else if (currentScreen == Screen.Cabinets && cabinetFocusOnFilter)
            {
                int filterDir = ReadHorizontalNavDirection();
                if (filterDir != 0)
                {
                    if (cabinetFocusOnAlphabet)
                        CycleCabinetLetterFilter(filterDir);
                    else
                        CycleCabinetListFilter(filterDir);
                    navCooldown = navRepeatDelay;
                    return;
                }
            }
            else if (UsesEnvListFilterFocus(currentScreen) && envListFocusOnFilter)
            {
                int filterDir = ReadHorizontalNavDirection();
                if (filterDir != 0)
                {
                    if (UsesAlphabetLetterFilter(currentScreen) && envFocusOnAlphabet)
                        CycleEnvLetterFilter(filterDir);
                    else
                        CycleEnvListFilter(filterDir);
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

    void TickSelectionCursorBlink()
    {
        if (currentScreen == Screen.Idle
            || currentScreen == Screen.ScanInProgress
            || currentScreen == Screen.DeleteConfigsDone)
            return;

        cursorBlinkTimer += Time.unscaledDeltaTime;
        if (cursorBlinkTimer < CursorBlinkHalfPeriod)
            return;

        cursorBlinkTimer = 0f;
        cursorBlinkVisible = !cursorBlinkVisible;
        if (navMenu != null)
            navMenu.selectionCursor = SelectionCursorPrefix;
        DrawCurrentScreen();
    }

    void ResetSelectionCursorBlink()
    {
        cursorBlinkVisible = true;
        cursorBlinkTimer = 0f;
        if (navMenu != null)
            navMenu.selectionCursor = ">>";
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
        navMenu = new GenericMenu(screen, "SCAN REQUIRED")
        {
            alignTop = true,
            leftAlign = true
        };
        navMenu.AddOption(
            MRSceneScanState.ScanRoomMenuOption,
            "Open Quest Space Setup to map your room");
    }

    void BuildFullNavMenu()
    {
        navMenu = new GenericMenu(screen, "MR CONFIGURATION")
        {
            alignTop = true,
            leftAlign = true
        };
        if (!MRPhoneBoothVisibility.IsPhoneBoothSuppressedForQuickTravel())
            navMenu.AddOption("PHONE BOOTH", "Show/hide MR travel booth");
        navMenu.AddOption("CABINETS", "Catalog: add or remove in MR space");
        navMenu.AddOption("CUSTOM OBJECTS", "Others + Posters + Room Skin");
        navMenu.AddOption("OFFICIAL OBJECTS", "Bookshelves + PrefabsEnvironment");
        navMenu.AddOption("OFFICIAL LIGHTS", "Placeable lights + global fill");
        navMenu.AddOption("CONFIG", "Move, scale, mesh, delete");
        navMenu.AddOption("DEBUG", "MR errors by date");
        navMenu.AddOption("HELP", "Controls + objects guide");
        navMenu.AddOption("EXIT", "Close panel");
    }

    void RefreshCatalog()
    {
        catalogNames.Clear();
        catalogNames.AddRange(MRLayoutRegistry.GetCatalogCabinetNames());
        catalogNames.Sort();
        RebuildFilteredCabinetNames();
    }

    void RebuildFilteredCabinetNames()
    {
        filteredCabinetNames.Clear();
        for (int i = 0; i < catalogNames.Count; i++)
        {
            string name = catalogNames[i];
            if (cabinetListFilter == CabinetListFilter.ShowAdded)
            {
                if (registry == null || !registry.IsCabinetInScene(name))
                    continue;
            }

            if (!CabinetNameMatchesLetterFilter(name))
                continue;

            filteredCabinetNames.Add(name);
        }
    }

    bool CabinetNameMatchesLetterFilter(string name)
    {
        if (cabinetLetterFilter <= 0)
            return true;
        if (string.IsNullOrEmpty(name))
            return false;

        char want = (char)('A' + cabinetLetterFilter - 1);
        return char.ToUpperInvariant(name[0]) == want;
    }

    void SetCabinetListFilter(CabinetListFilter filter)
    {
        if (cabinetListFilter == filter)
            return;

        cabinetListFilter = filter;
        RebuildFilteredCabinetNames();
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        // Keep focus on the Show All / Show Added tabs when switching with L/R.
        cabinetFocusOnFilter = true;
        cabinetFocusOnAlphabet = false;
        ResetSelectionCursorBlink();
        DrawCurrentScreen();
    }

    void CycleCabinetListFilter(int direction)
    {
        int next = ((int)cabinetListFilter + direction + 2) % 2;
        SetCabinetListFilter((CabinetListFilter)next);
    }

    void SetCabinetLetterFilter(int letterIndex)
    {
        int next = Mathf.Clamp(letterIndex, 0, CabinetAlphabetCount - 1);
        if (cabinetLetterFilter == next)
            return;

        cabinetLetterFilter = next;
        RebuildFilteredCabinetNames();
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        cabinetFocusOnFilter = true;
        cabinetFocusOnAlphabet = true;
        ResetSelectionCursorBlink();
        DrawCurrentScreen();
    }

    void CycleCabinetLetterFilter(int direction)
    {
        int next = (cabinetLetterFilter + direction + CabinetAlphabetCount) % CabinetAlphabetCount;
        SetCabinetLetterFilter(next);
    }

    static bool IsCustomObjectsListScreen(Screen screen) =>
        screen == Screen.CustomObjects
        || screen == Screen.CustomObjectsOthers
        || screen == Screen.Posters
        || screen == Screen.RoomSkins;

    static bool IsOfficialObjectsListScreen(Screen screen) =>
        screen == Screen.OfficialObjects
        || screen == Screen.OfficialObjectsOthers
        || screen == Screen.Lights;

    static bool IsFilteredEnvCatalogListScreen(Screen screen) =>
        IsCustomObjectsListScreen(screen)
        || screen == Screen.OfficialObjects
        || screen == Screen.OfficialObjectsOthers;

    static bool UsesEnvListFilterFocus(Screen screen) =>
        (IsFilteredEnvCatalogListScreen(screen)
            && screen != Screen.RoomSkins
            && screen != Screen.CustomObjects
            && screen != Screen.OfficialObjects)
        || screen == Screen.Lights
        || screen == Screen.PlacedInstances;

    // Poster catalog + PlacedInstances (inside an object) use Show All / Show Added Add/Remove.
    static bool UsesShowAllAddedAddRemove(Screen screen) =>
        screen == Screen.Posters
        || screen == Screen.PlacedInstances;

    static bool UsesAlphabetLetterFilter(Screen screen) =>
        screen == Screen.Posters;

    void RebuildOfficialHomeCatalog()
    {
        officialHomeCatalogEntries.Clear();
        officialHomeCatalogEntries.AddRange(lightsCatalogEntries);
        officialHomeCatalogEntries.AddRange(officialCatalogEntries);
    }

    void RebuildCustomHomeCatalog()
    {
        customHomeCatalogEntries.Clear();
        customHomeCatalogEntries.AddRange(customCatalogEntries);
        customHomeCatalogEntries.AddRange(postersCatalogEntries);
        customHomeCatalogEntries.AddRange(roomSkinsCatalogEntries);
    }

    IReadOnlyList<MREnvironmentCatalogEntry> GetEnvCatalogSourceEntries()
    {
        return currentScreen switch
        {
            Screen.CustomObjects => customCatalogEntries,
            Screen.CustomObjectsOthers => customCatalogEntries,
            Screen.Posters => postersCatalogEntries,
            Screen.RoomSkins => roomSkinsCatalogEntries,
            Screen.OfficialObjects => officialCatalogEntries,
            Screen.OfficialObjectsOthers => officialCatalogEntries,
            Screen.Lights => lightsCatalogEntries,
            _ => System.Array.Empty<MREnvironmentCatalogEntry>()
        };
    }

    void RebuildFilteredEnvCatalogEntries()
    {
        filteredEnvCatalogEntries.Clear();
        IReadOnlyList<MREnvironmentCatalogEntry> source = GetEnvCatalogSourceEntries();
        for (int i = 0; i < source.Count; i++)
        {
            MREnvironmentCatalogEntry entry = source[i];
            if (envListFilter == CabinetListFilter.ShowAdded)
            {
                if (envRegistry == null || envRegistry.GetInstanceCount(entry) <= 0)
                    continue;
            }

            if (UsesAlphabetLetterFilter(currentScreen)
                && !EnvNameMatchesLetterFilter(entry.DisplayLabel, entry.MenuLabel))
                continue;

            filteredEnvCatalogEntries.Add(entry);
        }

        if (UsesShowAllAddedAddRemove(currentScreen) && envListFilter == CabinetListFilter.ShowAdded)
            RebuildShowAddedEnvPlacements();
        else
            ClearShowAddedEnvPlacements();
    }

    void ClearShowAddedEnvPlacements()
    {
        showAddedEnvPlacements.Clear();
        showAddedEnvPlacementEntries.Clear();
        showAddedEnvPlacementCopyIndex.Clear();
    }

    void RebuildShowAddedEnvPlacements()
    {
        ClearShowAddedEnvPlacements();
        if (envRegistry == null)
            return;

        IReadOnlyList<MREnvironmentCatalogEntry> source = GetEnvCatalogSourceEntries();
        for (int i = 0; i < source.Count; i++)
        {
            MREnvironmentCatalogEntry entry = source[i];
            if (UsesAlphabetLetterFilter(currentScreen)
                && !EnvNameMatchesLetterFilter(entry.DisplayLabel, entry.MenuLabel))
                continue;

            IReadOnlyList<MREnvironmentPlacement> placements = envRegistry.FindAllPlacementsByCatalogEntry(entry);
            for (int p = 0; p < placements.Count; p++)
            {
                showAddedEnvPlacements.Add(placements[p]);
                showAddedEnvPlacementEntries.Add(entry);
                showAddedEnvPlacementCopyIndex.Add(p + 1);
            }
        }
    }

    int GetShowAllAddedListCount()
    {
        if (envListFilter == CabinetListFilter.ShowAdded)
            return showAddedEnvPlacements.Count;
        return filteredEnvCatalogEntries.Count;
    }

    bool EnvNameMatchesLetterFilter(string menuLabel, string displayLabel)
    {
        if (envLetterFilter <= 0)
            return true;

        string name = !string.IsNullOrEmpty(menuLabel) ? menuLabel : displayLabel;
        if (string.IsNullOrEmpty(name))
            return false;

        char want = (char)('A' + envLetterFilter - 1);
        return char.ToUpperInvariant(name[0]) == want;
    }

    void ResetEnvListFilterUi()
    {
        envListFilter = CabinetListFilter.ShowAll;
        envListFocusOnFilter = true;
        envFocusOnAlphabet = false;
        envLetterFilter = 0;
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        RebuildFilteredEnvCatalogEntries();
    }

    void SetEnvListFilter(CabinetListFilter filter)
    {
        if (envListFilter == filter)
            return;

        envListFilter = filter;
        if (currentScreen != Screen.PlacedInstances)
            RebuildFilteredEnvCatalogEntries();
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        envListFocusOnFilter = true;
        envFocusOnAlphabet = false;
        ResetSelectionCursorBlink();
        DrawCurrentScreen();
    }

    void CycleEnvListFilter(int direction)
    {
        int next = ((int)envListFilter + direction + 2) % 2;
        SetEnvListFilter((CabinetListFilter)next);
    }

    void SetEnvLetterFilter(int letterIndex)
    {
        int next = Mathf.Clamp(letterIndex, 0, CabinetAlphabetCount - 1);
        if (envLetterFilter == next)
            return;

        envLetterFilter = next;
        RebuildFilteredEnvCatalogEntries();
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        envListFocusOnFilter = true;
        envFocusOnAlphabet = true;
        ResetSelectionCursorBlink();
        DrawCurrentScreen();
    }

    void CycleEnvLetterFilter(int direction)
    {
        int next = (envLetterFilter + direction + CabinetAlphabetCount) % CabinetAlphabetCount;
        SetEnvLetterFilter(next);
    }

    void RefreshObjectCatalogs()
    {
        customCatalogEntries.Clear();
        MRCustomObjectCatalog.RefreshCache();
        foreach (string packageName in MRCustomObjectCatalog.GetPackageNames())
        {
            customCatalogEntries.Add(MREnvironmentCatalogEntry.FromCustom(
                packageName,
                MRCustomObjectCatalog.GetDisplayLabel(packageName)));
        }

        officialCatalogEntries.Clear();
        MREnvironmentCatalog.RefreshCache();
        foreach (string prefabName in MREnvironmentCatalog.GetPlaceablePrefabNames())
        {
            officialCatalogEntries.Add(MREnvironmentCatalogEntry.FromBuild(
                prefabName,
                MREnvironmentCatalog.GetDisplayLabel(prefabName)));
        }
    }

    List<MREnvironmentCatalogEntry> GetActiveObjectCatalogEntries() =>
        currentScreen == Screen.CustomObjectsOthers ? customCatalogEntries : officialCatalogEntries;

    Screen GetActiveObjectCatalogReturnScreen() =>
        currentScreen == Screen.CustomObjectsOthers ? Screen.CustomObjectsOthers : Screen.OfficialObjectsOthers;

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

    void RefreshMagazinesCatalog()
    {
        magazinesCatalogEntries.Clear();
        magazinesCatalogEntries.AddRange(MRBookshelfCatalog.GetCatalogEntries());
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
        pendingSelStatus = null;
        pendingActStatus = null;
        tableGridActive = false;
        if (navMenu != null)
            navMenu.selectionCursor = SelectionCursorPrefix;

        switch (currentScreen)
        {
            case Screen.NavMain:
                navMenu.DrawMenu();
                if (RequiresRoomScanMenu())
                {
                    DrawScanRequiredDiagnostic();
                    DrawFooter("A: scan room");
                }
                else
                    DrawFooter("Stick Up/Down: move   A: open");
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
            case Screen.GlobalLight:
                DrawGlobalLightPage();
                break;
            case Screen.PhoneBooth:
                DrawPhoneBoothPage();
                break;
            case Screen.Config:
                DrawConfigCategoryPage();
                break;
            case Screen.DeleteConfigsConfirm:
                DrawDeleteConfigsConfirmPage();
                break;
            case Screen.DeleteConfigsDone:
                DrawDeleteConfigsDonePage();
                break;
            case Screen.CustomObjectsOthers:
            case Screen.OfficialObjectsOthers:
                DrawObjectCatalogPage();
                break;
            case Screen.CustomObjects:
                DrawCustomObjectsCategoryPage();
                break;
            case Screen.OfficialObjects:
                DrawOfficialObjectsCategoryPage();
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
            case Screen.Magazines:
                DrawMagazinesPage();
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

        if (ShouldShowBackFooterHint())
            DrawBackFooterHint();

        screen.DrawScreen();
    }

    static bool ShouldShowBackFooterHint(Screen screen) =>
        screen != Screen.Idle
        && screen != Screen.NavMain
        && screen != Screen.ScanInProgress;

    bool ShouldShowBackFooterHint() => ShouldShowBackFooterHint(currentScreen);

    void DrawBackFooterHint()
    {
        // Left corner of the bottom footer row (all screens except home).
        screen.Print(1, screen.CharactersYCount - 1, BackFooterHint, false);
    }

    void DrawScanInProgressPage()
    {
        screen.PrintCentered(0, "SCAN ROOM", true);
        screen.PrintLine(1, false, '-');
        if (!string.IsNullOrEmpty(scanStatusLine))
        {
            screen.PrintCentered(8, scanStatusLine, true);
        }
        else
        {
            screen.PrintCentered(8, "Quest scanner open", true);
            screen.PrintCentered(10, "Walk around your room", false);
            screen.PrintCentered(12, "Finish setup to continue", false);
        }
    }

    void DrawScanRequiredDiagnostic()
    {
        bool mrukReady = MRUK.Instance != null;
        MRUKRoom room = mrukReady ? MRUK.Instance.GetCurrentRoom() : null;
        bool hasRoom = room != null && room.Anchors != null && room.Anchors.Count > 0;
        bool hasFloor = room?.FloorAnchor != null;
        int wallCount = room?.WallAnchors != null ? room.WallAnchors.Count : 0;

        int row = 3;
        screen.PrintLine(2, false, '-');

        if (!hasRoom)
        {
            string mrukLabel = mrukReady ? "MRUK:ready" : "MRUK:NULL";
            screen.PrintCentered(row++, mrukLabel, false);
            screen.PrintCentered(row++, MRSceneLoadState.LastLoadResult.ToString(), false);
            string fault = MRSceneLoadState.LastFaultDetail;
            if (!string.IsNullOrEmpty(fault))
                screen.PrintCentered(row++, Truncate(fault, 36), false);
        }
        else
        {
            string floorStr = hasFloor ? "Floor:OK" : "Floor:MISSING";
            string wallStr = wallCount > 0 ? $"Walls:{wallCount}" : "Walls:MISSING";
            screen.PrintCentered(row++, $"{floorStr}  {wallStr}", false);
            screen.PrintCentered(row++, $"Anchors:{room.Anchors.Count}", false);
        }
    }

    void DrawCabinetsPage()
    {
        screen.PrintCentered(0, "CABINETS", true);
        screen.PrintLine(1, false, '-');
        DrawCabinetFilterTabs(2);
        DrawMenuSeparatorLine(3);
        DrawCabinetAlphabetTabs(4);

        if (catalogNames.Count == 0)
        {
            screen.PrintCentered(10, "Not Found Cabinet", true);
            screen.PrintCentered(12, "Check cabinetsdb/", false);
            DrawFooter("B: back");
            return;
        }

        if (filteredCabinetNames.Count == 0)
        {
            string emptyMsg = cabinetListFilter == CabinetListFilter.ShowAdded
                ? "No added cabinets"
                : cabinetLetterFilter > 0
                    ? $"No cabinets for {(char)('A' + cabinetLetterFilter - 1)}"
                    : "No cabinets";
            screen.PrintCentered(10, emptyMsg, true);
            DrawFooter(cabinetFocusOnFilter
                ? (cabinetFocusOnAlphabet
                    ? "Stick L/R: letter   Down: list"
                    : "Stick L/R: filter   Down: letter")
                : "Stick Up/Down: move");
            return;
        }

        int rowSelected = cabinetFocusOnFilter ? -1 : selectedListIndex;
        int visibleRows = GetFilterListVisibleRows(CabinetsTableStartRow);
        int row = BeginListTable(null, "NAME", null, CabinetListRowNameWidth, startRow: CabinetsTableStartRow);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= filteredCabinetNames.Count)
                break;

            string name = Truncate(filteredCabinetNames[idx], CabinetListRowNameWidth);
            row = DrawListRow(row, idx, rowSelected, name, GetCabinetRowActions(idx), nameWidth: CabinetListRowNameWidth);
        }

        string tableFooter;
        if (cabinetFocusOnFilter)
        {
            tableFooter = cabinetFocusOnAlphabet
                ? $"Stick L/R: letter  Down: list ({filteredCabinetNames.Count})"
                : $"Stick L/R: filter  Down: letter ({filteredCabinetNames.Count})";
        }
        else
            tableFooter = $"Stick Up to alphabet  {filteredCabinetNames.Count} items";

        EndListTable(row, tableFooter);
        DrawFooter(cabinetFocusOnFilter
            ? (cabinetFocusOnAlphabet
                ? "Stick L/R: letter   Down: list"
                : "Stick L/R: filter   Down: letter")
            : "Stick Up/Down: move");
    }

    void DrawCabinetFilterTabs(int y) =>
        DrawShowAllAddedFilterTabs(y, cabinetFocusOnFilter && !cabinetFocusOnAlphabet, cabinetListFilter);

    void DrawCabinetAlphabetTabs(int y) =>
        DrawAlphabetFilterTabs(y, cabinetFocusOnFilter && cabinetFocusOnAlphabet, cabinetLetterFilter);

    void DrawEnvAlphabetTabs(int y) =>
        DrawAlphabetFilterTabs(y, envListFocusOnFilter && envFocusOnAlphabet, envLetterFilter);

    void DrawAlphabetFilterTabs(int y, bool focusActive, int letterFilter)
    {
        // Two lines so ALL + A-Z fit on the 40-col CRT.
        DrawAlphabetFilterLine(y, 0, 13, focusActive, letterFilter);     // ALL A … M
        DrawAlphabetFilterLine(y + 1, 14, 26, focusActive, letterFilter); // N … Z
    }

    void DrawAlphabetFilterLine(int y, int fromIdx, int toIdx, bool focusActive, int letterFilter)
    {
        int x = 0;
        screen.Print(x, y, "|", false);
        x += 1;

        for (int idx = fromIdx; idx <= toIdx; idx++)
        {
            string label = idx == 0 ? "ALL" : ((char)('A' + idx - 1)).ToString();
            bool lit = focusActive && letterFilter == idx;
            if (idx > fromIdx)
            {
                screen.Print(x, y, " ", false);
                x += 1;
            }

            string token = lit ? SelectionCursorPrefix + label : label;
            PrintSelectionSegment(x, y, token, lit);
            x += token.Length;
        }

        if (x < screen.CharactersXCount)
            screen.Print(x, y, "|", false);
    }

    void DrawEnvFilterTabs(int y) =>
        DrawShowAllAddedFilterTabs(
            y,
            envListFocusOnFilter && !(UsesAlphabetLetterFilter(currentScreen) && envFocusOnAlphabet),
            envListFilter);

    void DrawShowAllAddedFilterTabs(int y, bool focusOnFilter, CabinetListFilter activeFilter)
    {
        // | >>Show All | Show Added |  — yellow + >> only while filter row is focused
        int x = 0;
        screen.Print(x, y, "|", false);
        x += 1;

        string allLabel = FormatShowAllAddedTabLabel(
            focusOnFilter, activeFilter, CabinetListFilter.ShowAll, "Show All");
        PrintSelectionSegment(x, y, allLabel, focusOnFilter && activeFilter == CabinetListFilter.ShowAll);
        x += allLabel.Length;

        screen.Print(x, y, "|", false);
        x += 1;

        string addedLabel = FormatShowAllAddedTabLabel(
            focusOnFilter, activeFilter, CabinetListFilter.ShowAdded, "Show Added");
        PrintSelectionSegment(x, y, addedLabel, focusOnFilter && activeFilter == CabinetListFilter.ShowAdded);
        x += addedLabel.Length;

        screen.Print(x, y, "|", false);
    }

    string FormatShowAllAddedTabLabel(
        bool focusOnFilter,
        CabinetListFilter activeFilter,
        CabinetListFilter tab,
        string text)
    {
        bool isActive = activeFilter == tab;
        string prefix = focusOnFilter && isActive ? SelectionCursorPrefix : "  ";
        return prefix + text + " ";
    }

    void DrawAdjustmentsPage()
    {
        float scale = MRAdjustmentsSettings.CabinetScale;
        float floorPos = MRAdjustmentsSettings.FloorCabinetPosition;

        string scaleLabel = $"Scale {scale:F2}";
        string floorLabel = $"Floor {floorPos:F2}";

        const int adjustmentsNameWidth = 22;
        int row = BeginListTable("ADJUSTMENTS", "ITEM", null, adjustmentsNameWidth, startRow: 0);
        row = DrawListRow(row, 0, selectedAdjustmentIndex, scaleLabel, AdjustmentValueActions, nameWidth: adjustmentsNameWidth);
        row = DrawListRow(row, 1, selectedAdjustmentIndex, floorLabel, AdjustmentValueActions, nameWidth: adjustmentsNameWidth);
        EndListTable(row);
        DrawFooter("Up/Down: row   L/R: +/-   A: change");
    }

    void DrawGlobalLightPage()
    {
        MRAutoLightingSettings.EnsureLoaded();
        bool on = MRAutoLightingSettings.Enabled;
        float intensity = MRAutoLightingSettings.Intensity;

        const int nameWidth = 22;
        int row = BeginListTable("GLOBAL LIGHT", "ITEM", null, nameWidth, startRow: 0);
        row = DrawListRow(
            row,
            0,
            selectedListIndex,
            Truncate("Fill (all objects)", nameWidth),
            on
                ? new List<RowActionKind> { RowActionKind.ToggleOff }
                : new List<RowActionKind> { RowActionKind.ToggleOn },
            nameWidth: nameWidth);
        row = DrawListRow(
            row,
            1,
            selectedListIndex,
            $"Int {intensity:F1}",
            AdjustmentValueActions,
            nameWidth: nameWidth);
        EndListTable(row);
        DrawFooter("Up/Down: row   L/R: +/-   A: on/off or step");
    }

    void DrawConfigCategoryPage()
    {
        screen.PrintCentered(0, "CONFIG", true);
        screen.PrintLine(1, false, '-');

        string[] labels =
        {
            "Move Config",
            "Adjustments",
            "Mesh",
            "Delete Configs"
        };

        int row = BeginListTable(null, "NAME", null, CabinetListRowNameWidth, startRow: 2);
        for (int i = 0; i < ConfigCategoryCount; i++)
        {
            row = DrawListRow(
                row,
                i,
                selectedListIndex,
                Truncate(labels[i], CabinetListRowNameWidth),
                GetConfigCategoryRowActions(i),
                nameWidth: CabinetListRowNameWidth);
        }

        EndListTable(row, $"{ConfigCategoryCount} items");
        DrawFooter("Stick Up/Down: move");
    }

    void DrawDeleteConfigsConfirmPage()
    {
        screen.PrintCentered(0, "DELETE CONFIGS", true);
        screen.PrintLine(1, false, '-');

        screen.PrintCentered(3, "Erase ALL .yaml under MR/?", true);
        screen.PrintCentered(5, "cabinets-layout.yaml", false);
        screen.PrintCentered(6, "objects-layout.yaml", false);
        screen.PrintCentered(7, "+ package yaml (objects/skins)", false);
        screen.PrintCentered(8, "YAML only — assets kept", false);
        screen.PrintCentered(10, "Placed MR objects despawn", false);
        screen.PrintCentered(11, "This cannot be undone", true);

        string[] labels =
        {
            "Yes, delete (Press A to confirm)",
            "No, cancel (Press A to go back)"
        };

        for (int i = 0; i < 2; i++)
        {
            int y = 13 + i * 2;
            bool selected = selectedListIndex == i;
            PrintSelectionLine(
                1,
                y,
                (selected ? SelectionCursorPrefix : "  ") + labels[i],
                selected);
            if (i == 0)
                DrawMenuSeparatorLine(y + 1);
        }

        DrawFooter("Stick Up/Down: move");
    }

    void DrawDeleteConfigsDonePage()
    {
        screen.PrintCentered(0, "DELETE CONFIGS", true);
        screen.PrintLine(1, false, '-');
        screen.PrintCentered(6, "Done", true);
        screen.PrintCentered(8, $"Deleted {lastDeletedYamlCount} YAML file(s)", false);
        screen.PrintCentered(10, "MR objects despawned", false);
        screen.PrintCentered(11, "Place config cabinet with ray", false);
        DrawFooter("Press A or B to go back");
    }

    int GetCustomHomeVisibleRows() =>
        GetFilterListVisibleRows(CustomHomeTableStartRow);

    int GetCustomHomeListCount() =>
        CustomCategoryCount + filteredEnvCatalogEntries.Count;

    int SumCatalogInstanceCounts(IReadOnlyList<MREnvironmentCatalogEntry> entries)
    {
        if (envRegistry == null || entries == null)
            return 0;

        int total = 0;
        for (int i = 0; i < entries.Count; i++)
            total += envRegistry.GetInstanceCount(entries[i]);
        return total;
    }

    int GetOfficialObjectsVisibleRows() =>
        GetFilterListVisibleRows(2);

    int GetOfficialHomeListCount() =>
        OfficialCategoryCount + filteredEnvCatalogEntries.Count;

    /// <summary>
    /// Catalog/filter list data rows that fit above the in-box footer and page footer.
    /// Keep in sync with draw loops, EndListTable padding, and ClampListScroll.
    /// </summary>
    int GetFilterListVisibleRows(int tableStartRow = 3)
    {
        if (screen == null)
            return Mathf.Max(1, VisibleCabinetRows);

        // BeginListTable (no in-box title): top + header + rule = 3 rows before first data line.
        // Pass tableStartRow+2 when the table includes an in-box title (e.g. Magazines/Debug).
        int firstDataRow = tableStartRow + 3;
        // EndListTable with footer strip (3) + page footer zone SEL/hint/B (3).
        int lastDataExclusive = screen.CharactersYCount - 3 - 3;
        int fit = lastDataExclusive - firstDataRow;
        return Mathf.Max(1, fit);
    }

    void DrawMenuSeparatorLine(int y)
    {
        if (screen == null || y < 0 || y >= screen.CharactersYCount - 1)
            return;
        screen.PrintLine(y, false, '-');
    }

    void DrawCustomObjectsCategoryPage()
    {
        // Unified table: Posters + Room Skin + packages (no Show All / Add).
        envListFilter = CabinetListFilter.ShowAll;
        envListFocusOnFilter = false;
        RebuildFilteredEnvCatalogEntries();

        screen.PrintCentered(0, "CUSTOM OBJECTS", true);
        screen.PrintLine(1, false, '-');

        int listCount = GetCustomHomeListCount();
        int visibleRows = GetCustomHomeVisibleRows();
        int row = BeginListTable(
            null,
            "NAME",
            "#",
            ActionListRowNameWidth,
            startRow: CustomHomeTableStartRow);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= listCount)
                break;

            string label;
            string flag;
            if (idx == 0)
            {
                label = "Posters";
                flag = SumCatalogInstanceCounts(postersCatalogEntries).ToString();
            }
            else if (idx == 1)
            {
                label = "Room Skin";
                flag = SumCatalogInstanceCounts(roomSkinsCatalogEntries).ToString();
            }
            else
            {
                MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[idx - CustomCategoryCount];
                label = Truncate(entry.DisplayLabel, ActionListRowNameWidth);
                flag = (envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0).ToString();
            }

            row = DrawListRow(
                row,
                idx,
                selectedListIndex,
                Truncate(label, ActionListRowNameWidth),
                GetCustomHomeRowActions(idx),
                flag,
                nameWidth: ActionListRowNameWidth);
        }

        EndListTable(row, $"{listCount} items");
        DrawFooter("Stick Up/Down: move");
    }

    void DrawOfficialObjectsCategoryPage()
    {
        // Unified table: Bookshelves + PrefabsEnvironment packages (Official Lights is on main nav).
        RefreshObjectCatalogs();
        RefreshMagazinesCatalog();
        envListFilter = CabinetListFilter.ShowAll;
        envListFocusOnFilter = false;
        RebuildFilteredEnvCatalogEntries();

        screen.PrintCentered(0, "OFFICIAL OBJECTS", true);
        screen.PrintLine(1, false, '-');

        int listCount = GetOfficialHomeListCount();
        if (listCount == 0)
        {
            screen.PrintCentered(8, "No official objects found", true);
            screen.PrintCentered(10, "PrefabsEnvironment/", false);
            DrawFooter("Stick Up/Down: move");
            return;
        }

        int visibleRows = GetOfficialObjectsVisibleRows();
        int row = BeginListTable(
            null,
            "NAME",
            "#",
            ActionListRowNameWidth,
            startRow: 2);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= listCount)
                break;

            string label;
            string flag;
            if (idx == 0)
            {
                label = "Bookshelf";
                flag = (envRegistry != null ? envRegistry.GetBookshelfInstanceCount() : 0).ToString();
            }
            else
            {
                MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[idx - OfficialCategoryCount];
                label = Truncate(entry.DisplayLabel, ActionListRowNameWidth);
                flag = (envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0).ToString();
            }

            row = DrawListRow(
                row,
                idx,
                selectedListIndex,
                Truncate(label, ActionListRowNameWidth),
                GetOfficialHomeRowActions(idx),
                flag,
                nameWidth: ActionListRowNameWidth);
        }

        EndListTable(row, $"{listCount} items");
        DrawFooter("Stick Up/Down: move");
    }

    void DrawObjectCatalogPage()
    {
        if (currentScreen == Screen.CustomObjectsOthers)
        {
            DrawCustomObjectsOthersPage();
            return;
        }

        if (currentScreen == Screen.OfficialObjectsOthers)
        {
            DrawOfficialObjectsOthersPage();
            return;
        }
    }

    void DrawCustomObjectsOthersPage()
    {
        DrawCustomEnvCatalogListPage(
            "OTHERS",
            "No custom packages",
            "MR/Custom Objects/",
            ActionListRowNameWidth,
            useMenuLabel: false);
    }

    void DrawOfficialObjectsOthersPage()
    {
        DrawCustomEnvCatalogListPage(
            "OTHERS",
            "No official props",
            "PrefabsEnvironment/",
            ActionListRowNameWidth,
            useMenuLabel: false);
    }

    void DrawCustomEnvCatalogListPage(
        string title,
        string emptyCatalogMessage,
        string emptyCatalogHint,
        int nameWidth,
        bool useMenuLabel,
        int flagWidth = TableFlagWidth)
    {
        RebuildFilteredEnvCatalogEntries();

        bool useAlphabet = UsesAlphabetLetterFilter(currentScreen);
        int tableStartRow = useAlphabet ? PostersTableStartRow : 3;

        screen.PrintCentered(0, title, true);
        screen.PrintLine(1, false, '-');
        DrawEnvFilterTabs(2);
        if (useAlphabet)
        {
            DrawMenuSeparatorLine(3);
            DrawEnvAlphabetTabs(4);
        }

        IReadOnlyList<MREnvironmentCatalogEntry> source = GetEnvCatalogSourceEntries();
        if (source.Count == 0)
        {
            screen.PrintCentered(useAlphabet ? 10 : 8, emptyCatalogMessage, true);
            screen.PrintCentered(useAlphabet ? 12 : 10, emptyCatalogHint, false);
            DrawFooter("B: back");
            return;
        }

        if (UsesShowAllAddedAddRemove(currentScreen) && envListFilter == CabinetListFilter.ShowAdded)
        {
            if (showAddedEnvPlacements.Count == 0)
            {
                string emptyAdded = useAlphabet && envLetterFilter > 0
                    ? $"No added for {(char)('A' + envLetterFilter - 1)}"
                    : "No added objects";
                screen.PrintCentered(useAlphabet ? 10 : 8, emptyAdded, true);
                DrawFooter(useAlphabet && envListFocusOnFilter
                    ? (envFocusOnAlphabet
                        ? "Stick L/R: letter   Down: list"
                        : "Stick L/R: filter   Down: letter")
                    : "Stick L/R: filter   Down: list");
                return;
            }

            int rowSelectedAdded = envListFocusOnFilter ? -1 : selectedListIndex;
            int addedVisibleRows = GetFilterListVisibleRows(tableStartRow);
            int rowAdded = BeginListTable(null, "COPY", null, nameWidth, startRow: tableStartRow);
            for (int i = 0; i < addedVisibleRows; i++)
            {
                int idx = listScrollOffset + i;
                if (idx >= showAddedEnvPlacements.Count)
                    break;

                MREnvironmentCatalogEntry entry = showAddedEnvPlacementEntries[idx];
                MREnvironmentPlacement placement = showAddedEnvPlacements[idx];
                int copyIndex = showAddedEnvPlacementCopyIndex[idx];
                string name = Truncate(useMenuLabel ? entry.MenuLabel : entry.DisplayLabel, Mathf.Max(6, nameWidth - 8));
                string label = Truncate($"{name} #{copyIndex} {SurfaceShortLabel(placement.SurfaceType)}", nameWidth);
                rowAdded = DrawListRow(
                    rowAdded,
                    idx,
                    rowSelectedAdded,
                    label,
                    GetShowAllAddedEnvRowActions(idx),
                    nameWidth: nameWidth);
            }

            string addedFooter;
            if (envListFocusOnFilter && useAlphabet)
            {
                addedFooter = envFocusOnAlphabet
                    ? $"Stick L/R: letter  Down: list ({showAddedEnvPlacements.Count})"
                    : $"Stick L/R: filter  Down: letter ({showAddedEnvPlacements.Count})";
            }
            else
            {
                addedFooter = envListFocusOnFilter
                    ? $"Stick L/R to filter  Down: list ({showAddedEnvPlacements.Count})"
                    : useAlphabet
                        ? $"Stick Up to alphabet  {showAddedEnvPlacements.Count} copies"
                        : $"Stick Up to filters  {showAddedEnvPlacements.Count} copies";
            }

            EndListTable(rowAdded, addedFooter);
            DrawFooter(envListFocusOnFilter
                ? (useAlphabet
                    ? (envFocusOnAlphabet
                        ? "Stick L/R: letter   Down: list"
                        : "Stick L/R: filter   Down: letter")
                    : "Stick L/R: filter   Down: list")
                : "Stick Up/Down: move   A: remove");
            return;
        }

        if (filteredEnvCatalogEntries.Count == 0)
        {
            string emptyMsg = useAlphabet && envLetterFilter > 0
                ? $"No posters for {(char)('A' + envLetterFilter - 1)}"
                : "No added objects";
            screen.PrintCentered(useAlphabet ? 10 : 8, emptyMsg, true);
            DrawFooter(useAlphabet && envListFocusOnFilter
                ? (envFocusOnAlphabet
                    ? "Stick L/R: letter   Down: list"
                    : "Stick L/R: filter   Down: letter")
                : "Stick L/R: filter   Down: list");
            return;
        }

        int rowSelected = envListFocusOnFilter ? -1 : selectedListIndex;
        // Middle column: how many copies of that catalog entry are already placed.
        int visibleRows = GetFilterListVisibleRows(tableStartRow);
        int row = BeginListTable(
            null,
            "NAME",
            "#",
            nameWidth,
            startRow: tableStartRow,
            flagWidth: flagWidth);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= filteredEnvCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[idx];
            string label = Truncate(useMenuLabel ? entry.MenuLabel : entry.DisplayLabel, nameWidth);
            string flag = (envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0).ToString();
            row = DrawListRow(
                row,
                idx,
                rowSelected,
                label,
                UsesShowAllAddedAddRemove(currentScreen)
                    ? GetShowAllAddedEnvRowActions(idx)
                    : GetCustomEnvCatalogRowActions(idx),
                flag,
                nameWidth: nameWidth);
        }

        string listFooter;
        if (envListFocusOnFilter && useAlphabet)
        {
            listFooter = envFocusOnAlphabet
                ? $"Stick L/R: letter  Down: list ({filteredEnvCatalogEntries.Count})"
                : $"Stick L/R: filter  Down: letter ({filteredEnvCatalogEntries.Count})";
        }
        else
        {
            listFooter = envListFocusOnFilter
                ? $"Stick L/R to filter  Down: list ({filteredEnvCatalogEntries.Count})"
                : useAlphabet
                    ? $"Stick Up to alphabet  {filteredEnvCatalogEntries.Count} items"
                    : $"Stick Up to filters  {filteredEnvCatalogEntries.Count} items";
        }

        EndListTable(row, listFooter);
        DrawFooter(envListFocusOnFilter
            ? (useAlphabet
                ? (envFocusOnAlphabet
                    ? "Stick L/R: letter   Down: list"
                    : "Stick L/R: filter   Down: letter")
                : "Stick L/R: filter   Down: list")
            : UsesShowAllAddedAddRemove(currentScreen)
                ? "Stick Up/Down: move   A: add"
                : "Stick Up/Down: move");
    }

    void DrawPostersPage()
    {
        DrawCustomEnvCatalogListPage(
            "POSTERS",
            "No posters found",
            "MR/Posters/",
            ActionListRowNameWidth,
            useMenuLabel: true,
            flagWidth: TableFlagWidth - 1);
    }

    void DrawRoomSkinsPage()
    {
        // One active skin per surface — no Show All / Show Added; Add/Remove toggle on same list.
        envListFilter = CabinetListFilter.ShowAll;
        envListFocusOnFilter = false;
        RebuildFilteredEnvCatalogEntries();

        screen.PrintCentered(0, "ROOM SKIN", true);
        screen.PrintLine(1, false, '-');

        if (roomSkinsCatalogEntries.Count == 0)
        {
            screen.PrintCentered(8, "No room skins found", true);
            screen.PrintCentered(10, "MR/Room Skins/", false);
            DrawFooter("Stick Up/Down: move");
            return;
        }

        int row = BeginListTable(null, "NAME", "#", RoomSkinListRowNameWidth, startRow: 2);
        int visibleRows = GetFilterListVisibleRows(2);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= filteredEnvCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[idx];
            string label = Truncate(entry.DisplayLabel, RoomSkinListRowNameWidth);
            string flag = (envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0).ToString();
            row = DrawListRow(
                row,
                idx,
                selectedListIndex,
                label,
                GetRoomSkinsRowActions(idx),
                flag,
                nameWidth: RoomSkinListRowNameWidth);
        }

        EndListTable(row, $"{filteredEnvCatalogEntries.Count} skins");
        DrawFooter("Stick Up/Down: move");
    }

    void DrawLightsPage()
    {
        RebuildFilteredEnvCatalogEntries();

        screen.PrintCentered(0, "OFFICIAL LIGHTS", true);
        screen.PrintLine(1, false, '-');
        DrawEnvFilterTabs(2);

        if (envListFilter == CabinetListFilter.ShowAdded
            && filteredEnvCatalogEntries.Count == 0
            && envListFocusOnFilter)
        {
            screen.PrintCentered(8, "No added objects", true);
            return;
        }

        int listCount = GetLightsListCount();
        int rowSelected = envListFocusOnFilter ? -1 : selectedListIndex;
        int row = BeginListTable(null, "NAME", "#", ActionListRowNameWidth, startRow: 3);

        if (lightsCatalogEntries.Count == 0 && listCount <= LightsGlobalShortcutCount)
        {
            // Still show Global Light shortcut even if no placeable prefabs.
            row = DrawListRow(
                row,
                0,
                rowSelected,
                "Global Light",
                GetLightsListRowActions(0),
                "-",
                nameWidth: ActionListRowNameWidth);
            EndListTable(row, "No light prefabs in ramiro/Lights/");
            DrawFooter(envListFocusOnFilter
                ? "Stick L/R: filter   Down: list"
                : "Stick Up/Down: move");
            return;
        }

        int catalogVisibleRows = GetFilterListVisibleRows();
        for (int i = 0; i < catalogVisibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= listCount)
                break;

            if (idx == 0)
            {
                row = DrawListRow(
                    row,
                    idx,
                    rowSelected,
                    "Global Light",
                    GetLightsListRowActions(idx),
                    "-",
                    nameWidth: ActionListRowNameWidth);
                continue;
            }

            int catalogIdx = idx - LightsGlobalShortcutCount;
            if (catalogIdx < 0 || catalogIdx >= filteredEnvCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[catalogIdx];
            string label = Truncate(entry.MenuLabel, ActionListRowNameWidth);
            string flag = (envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0).ToString();
            row = DrawListRow(
                row,
                idx,
                rowSelected,
                label,
                GetLightsListRowActions(idx),
                flag,
                nameWidth: ActionListRowNameWidth);
        }

        EndListTable(row, envListFocusOnFilter
            ? $"Stick L/R to filter  Down: list ({listCount})"
            : $"Stick Up to filters  {listCount} items");
        DrawFooter(envListFocusOnFilter
            ? "Stick L/R: filter   Down: list"
            : "Stick Up/Down: move");
    }

    int GetLightsListCount() =>
        LightsGlobalShortcutCount + filteredEnvCatalogEntries.Count;

    List<RowActionKind> GetLightsListRowActions(int listIndex)
    {
        var actions = new List<RowActionKind>();
        if (listIndex < 0 || listIndex >= GetLightsListCount())
            return actions;

        // Global Light shortcut + each placeable light: enter detail (Add / Remove / Tune).
        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    List<RowActionKind> GetLightsFilteredRowActions(int filteredCatalogIndex)
    {
        var actions = new List<RowActionKind>();
        if (filteredCatalogIndex < 0 || filteredCatalogIndex >= filteredEnvCatalogEntries.Count)
            return actions;

        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    void DrawMagazinesPage()
    {
        if (magazinesCatalogEntries.Count == 0)
        {
            screen.PrintCentered(0, "BOOKSHELVES", true);
            screen.PrintLine(1, false, '-');
            screen.PrintCentered(8, "No bookshelves available", true);
            screen.PrintCentered(10, "Add numbered images to", false);
            screen.PrintCentered(11, "MR/Magazines/<issue>/", false);
            DrawFooter("B: back");
            return;
        }

        int row = BeginListTable("BOOKSHELVES", "NAME", "#", ListRowNameWidth, startRow: 0);
        // Title row in table (+2 chrome vs filter lists); first data = 0+3+2 = 5.
        int visibleRows = GetFilterListVisibleRows(tableStartRow: 2);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= magazinesCatalogEntries.Count)
                break;

            MREnvironmentCatalogEntry entry = magazinesCatalogEntries[idx];
            string label = Truncate(entry.MenuLabel, ListRowNameWidth);
            int count = envRegistry != null ? envRegistry.GetInstanceCount(entry) : 0;
            row = DrawListRow(row, idx, selectedListIndex, label, GetMagazinesRowActions(idx), count.ToString());
        }

        EndListTable(row, $"{RowColumnFooter}  {magazinesCatalogEntries.Count} items");
    }

    void DrawPlacedInstancesPage()
    {
        string title = Truncate(instancesCatalogEntry.DisplayLabel, 28);
        int count = placedInstances.Count;
        bool showAdded = envListFilter == CabinetListFilter.ShowAdded;
        int listCount = GetPlacedInstancesListCount();

        screen.PrintCentered(0, title, true);
        screen.PrintLine(1, false, '-');
        DrawShowAllAddedFilterTabs(2, envListFocusOnFilter, envListFilter);

        // Same model as Posters: Show All = Add only; Show Added = one row per copy.
        if (showAdded && count == 0)
        {
            screen.PrintCentered(10, "No added copies", true);
            DrawFooter(envListFocusOnFilter
                ? "Stick L/R: filter"
                : "Stick Up/Down: move   A: remove");
            return;
        }

        int rowSelected = envListFocusOnFilter ? -1 : selectedListIndex;
        string nameCol = showAdded ? "COPY" : "NAME";
        int visibleRows = GetFilterListVisibleRows();
        // Show All: middle column = how many copies of this object are placed.
        // Show Added: per-copy list — no count column (each row is one instance).
        int row = BeginListTable(
            null,
            nameCol,
            showAdded ? null : "#",
            ListRowNameWidth,
            startRow: 3);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= listCount)
                break;

            if (!showAdded)
            {
                row = DrawListRow(
                    row,
                    idx,
                    rowSelected,
                    "+ Add",
                    GetPlacedInstanceRowActions(idx),
                    count.ToString());
                continue;
            }

            MREnvironmentPlacement placement = placedInstances[idx];
            string label = $"#{idx + 1} {SurfaceShortLabel(placement.SurfaceType)}";
            row = DrawListRow(row, idx, rowSelected, label, GetPlacedInstanceRowActions(idx));
        }

        EndListTable(row, envListFocusOnFilter
            ? $"Stick L/R to filter  Down: list ({listCount})"
            : showAdded
                ? $"{count} copies"
                : "A: add copy");

        if (envListFocusOnFilter)
            DrawFooter("Stick L/R: filter   Down: list");
        else if (showAdded)
            DrawFooter(PlacedInstanceShowAddedHasTune()
                ? "Stick Up/Down: row   L/R: option   A: ok"
                : "Stick Up/Down: move   A: remove");
        else
            DrawFooter("Stick Up/Down: move   A: add");
    }

    static string SurfaceShortLabel(PlacementSurfaceType surfaceType) =>
        surfaceType switch
        {
            PlacementSurfaceType.Floor => "Flr",
            PlacementSurfaceType.Wall => "Wal",
            PlacementSurfaceType.Ceiling => "Cel",
            PlacementSurfaceType.Object => "Obj",
            _ => "?"
        };

    static string PadLeft(string value, int width)
    {
        if (value.Length >= width)
            return value;
        return value.PadLeft(width);
    }

    int GetPlacedInstancesListCount()
    {
        // Poster-style: Show All is Add-only; Show Added lists each copy.
        if (envListFilter == CabinetListFilter.ShowAdded)
            return placedInstances.Count;
        return 1;
    }

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
        envListFilter = CabinetListFilter.ShowAll;
        envListFocusOnFilter = true;
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        listScrollOffset = 0;
        currentScreen = Screen.PlacedInstances;
        DrawCurrentScreen();
    }

    void DrawLightTunePage()
    {
        string label = Truncate(instancesCatalogEntry.MenuLabel, 18);
        int instanceNumber = FindPlacedInstanceIndex(lightTunePlacementId) + 1;

        float intensity = 0f;
        float range = 0f;
        float temperature = 0f;
        if (envRegistry != null)
            envRegistry.TryGetLightSettings(lightTunePlacementId, out intensity, out range, out temperature);

        int row = BeginListTable($"LIGHT TUNE {label} #{instanceNumber}", "ITEM", null, TuneListRowNameWidth, startRow: 0);
        row = DrawListRow(row, 0, selectedLightTuneIndex, $"Int {intensity:F1}", AdjustmentValueActions, nameWidth: TuneListRowNameWidth);
        row = DrawListRow(row, 1, selectedLightTuneIndex, $"Rng {range:F1}", AdjustmentValueActions, nameWidth: TuneListRowNameWidth);
        row = DrawListRow(row, 2, selectedLightTuneIndex, $"Tmp {temperature:F0}K", AdjustmentValueActions, nameWidth: TuneListRowNameWidth);
        EndListTable(row, "Int/Rng 0.1  Tmp 100K");
        DrawFooter("Up/Down: row   L/R: +/-   A: change");
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
        const int meshNameWidth = 18;
        int row = BeginListTable("MESH", "ITEM", null, meshNameWidth, startRow: 0);

        row = DrawListRow(
            row,
            0,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.AnchorMeshLabel, meshNameWidth),
            GetMeshRowActions(0),
            nameWidth: meshNameWidth);
        row = DrawListRow(
            row,
            1,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.GlobalMeshLabel, meshNameWidth),
            GetMeshRowActions(1),
            nameWidth: meshNameWidth);
        row = DrawListRow(
            row,
            MeshScanColorsRowIndex,
            selectedListIndex,
            Truncate(MREffectMeshVisibility.ScanDebugColorsLabel, meshNameWidth),
            GetMeshRowActions(MeshScanColorsRowIndex),
            nameWidth: meshNameWidth);

        EndListTable(row);
        DrawFooter("Stick Up/Down: move");
    }

    void DrawPhoneBoothPage()
    {
        screen.PrintCentered(0, "PHONE BOOTH", true);
        screen.PrintLine(1, false, '-');

        screen.Print(1, 3, "Grab the handset to travel", false);
        screen.Print(1, 4, "VR gallery <-> MR room", false);

        bool visible = MRPhoneBoothSettings.Visible;

        // Single action — yellow bar; Press A toggles visibility.
        string actionLabel = visible
            ? SelectionCursorPrefix + " Press A to HIDE booth"
            : SelectionCursorPrefix + " Press A to SHOW booth";
        PrintSelectionLine(1, 8, actionLabel, true);
    }

    void DrawDebugPage()
    {
        debugDisplayLines.Clear();
        debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());

        if (debugDisplayLines.Count == 0)
        {
            screen.PrintCentered(0, "DEBUG", true);
            screen.PrintLine(1, false, '-');
            screen.PrintCentered(8, "No errors logged", true);
            screen.PrintCentered(10, "Add failures appear here", false);
            DrawFooter("B: back");
            return;
        }

        int row = BeginListTable("DEBUG", "LOG", null, ListRowNameWidth, startRow: 0);
        // In-box title adds +2 chrome vs untitled tables (same as Magazines).
        int visibleRows = GetFilterListVisibleRows(tableStartRow: 2);
        for (int i = 0; i < visibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= debugDisplayLines.Count)
                break;

            MRDebugLog.DisplayLine line = debugDisplayLines[idx];
            if (line.IsDateHeader)
            {
                // Date band — Clipper style: no horizontal rule between records.
                string dateCell = Truncate(line.Text, tableNameWidth);
                screen.Print(0, row, BuildTableRow(dateCell, null, string.Empty), true);
                row += 1;
            }
            else
            {
                string text = Truncate(line.Text, ListRowNameWidth);
                row = DrawListRow(row, idx, selectedListIndex, text, GetDebugRowActions(idx));
            }
        }

        EndListTable(row, $"{debugDisplayLines.Count} lines  " + RowColumnFooter);
    }

    void DrawHelpPage()
    {
        screen.PrintCentered(0, "HELP", true);
        screen.PrintLine(1, false, '-');

        int row = 2;
        for (int i = 0; i < VisibleHelpRows; i++)
        {
            int idx = helpScrollOffset + i;
            if (idx >= helpWrappedLines.Count)
                break;

            string line = helpWrappedLines[idx];
            if (!string.IsNullOrEmpty(line))
                screen.Print(1, row, line, false);
            row++;
        }

        if (helpWrappedLines.Count > VisibleHelpRows)
        {
            int page = helpScrollOffset / VisibleHelpRows + 1;
            int pages = (helpWrappedLines.Count + VisibleHelpRows - 1) / VisibleHelpRows;
            screen.Print(
                1,
                screen.CharactersYCount - 3,
                $"Page {page}/{pages}  Up/Down: scroll",
                false);
        }

        DrawFooter("Stick Up/Down: scroll");
    }

    void BuildHelpWrappedLines()
    {
        helpWrappedLines.Clear();
        foreach (string line in GetHelpSourceLines())
        {
            if (string.IsNullOrEmpty(line))
            {
                helpWrappedLines.Add(string.Empty);
                continue;
            }

            if (line.Length <= HelpWrapWidth)
                helpWrappedLines.Add(line);
            else
                helpWrappedLines.AddRange(WrapDebugText(line, HelpWrapWidth));
        }
    }

    /// <summary>
    /// In-headset HELP — condensed from docs/MR_USER_GUIDE.en.md (keep in sync).
    /// Deep package authoring stays in AOJ MR Studio / guide §11.
    /// Prefer lines ≤ HelpWrapWidth (38).
    /// </summary>
    static IEnumerable<string> GetHelpSourceLines()
    {
        yield return "CONTROLS (CRT)";
        yield return "Up/Down: move in lists";
        yield return "L/R: switch action when shown";
        yield return "  (e.g. +/- or REMOVE/REPOS)";
        yield return "A: confirm   B: go back";
        yield return "Coin in slot: open panel";
        yield return "EXIT + A: close panel";
        yield return string.Empty;
        yield return "COMMON FLOW";
        yield return "1) Open category with A";
        yield return "2) Pick item (Up/Down)";
        yield return "3) If Press A to enter:";
        yield return "   open that object screen";
        yield return "4) Show All: A = Add (ray)";
        yield return "5) Show Added: Up/Down + A";
        yield return "   removes selected copy";
        yield return "   Lights also L/R Tune";
        yield return "6) Ray: trigger place  B cancel";
        yield return "   R-stick L/R = rotate";
        yield return "   grip + R-stick L/R = scale";
        yield return "     (posters / custom objs)";
        yield return "Green = OK   Red = blocked";
        yield return string.Empty;
        yield return "MENU";
        yield return "PHONE BOOTH - show/hide";
        yield return "CABINETS - arcade machines";
        yield return "CUSTOM OBJECTS - packages,";
        yield return "  posters, room skins";
        yield return "OFFICIAL OBJECTS - bookshelves";
        yield return "  + built-in props";
        yield return "CONFIG - move, scale, light,";
        yield return "  mesh, delete YAML";
        yield return "DEBUG - MR error log";
        yield return "HELP - this guide";
        yield return "EXIT - close CRT";
        yield return string.Empty;
        yield return "CABINETS";
        yield return "One machine per game.";
        yield return "Show All: A Add, or Remove";
        yield return "  if already placed";
        yield return "Show Added: A = Remove";
        yield return "No Move here — Remove then";
        yield return "  Add again to reposition";
        yield return "Floor; R-stick yaw OK";
        yield return string.Empty;
        yield return "CUSTOM OBJECTS";
        yield return "Home: Posters, Room Skin,";
        yield return "  then packages (enter each)";
        yield return "Posters: Show All Add /";
        yield return "  Show Added = Remove / Repos";
        yield return "  Scale on ray: grip+R-stick";
        yield return "  Folder MR/Posters/ wall";
        yield return "Room Skin: A Add or Remove";
        yield return "  No ray — room mesh only";
        yield return "  Folder MR/Room Skins/";
        yield return "Packages: enter → filters";
        yield return "  Show All: Add only";
        yield return "  Show Added: Remove / Repos";
        yield return "  Scale on ray: grip+R-stick";
        yield return "  Folder MR/Custom Objects/";
        yield return "Make packages: AOJ MR Studio";
        yield return "  (Windows USB) — see guide";
        yield return string.Empty;
        yield return "OFFICIAL OBJECTS";
        yield return "Bookshelf: A = Add/Remove";
        yield return "  (one shelf; MR/Magazines/)";
        yield return "  numbered page images";
        yield return "  Floor; grab magazines";
        yield return "Props: enter → Show All/Added";
        yield return "Most: Up/Down + A remove";
        yield return "PF_Fan, Portable Games…";
        yield return string.Empty;
        yield return "PORTABLE GAMES";
        yield return "OFFICIAL → Portable Games";
        yield return "Show All → Add stand on";
        yield return "  TABLE with placement ray";
        yield return "Hold BOTH handles to play";
        yield return "Release a hand → stand";
        yield return "Cores/ROMs same as VR:";
        yield return "  cores/ and downloads/";
        yield return string.Empty;
        yield return "PHONE BOOTH";
        yield return "Handset: travel VR <-> MR";
        yield return "Menu: show or hide booth";
        yield return "Booth must be visible to";
        yield return "  leave MR";
        yield return string.Empty;
        yield return "CONFIG";
        yield return "Move Config: wall ray";
        yield return "Adjustments: all cabinets";
        yield return "  scale + floor Y";
        yield return "  Up/Down row  L/R +/-";
        yield return "  A applies step";
        yield return "Global Light: cool fill all";
        yield return "  objects  ON/OFF + intens";
        yield return "Mesh: enable/disable mesh";
        yield return "Delete Configs: erase";
        yield return "  MR/*.yaml (assets kept)";
        yield return "  objects despawn; no undo";
        yield return string.Empty;
        yield return "FILES (on Quest)";
        yield return "MR/Custom Objects/";
        yield return "MR/Posters/";
        yield return "MR/Magazines/";
        yield return "MR/Room Skins/";
        yield return "Layouts auto-saved";
        yield return string.Empty;
        yield return "REQUIREMENTS";
        yield return "Quest with passthrough";
        yield return "Room mapped (Space Setup)";
        yield return "Floor + at least one wall";
        yield return "Launch AOJ once for folders";
        yield return string.Empty;
        yield return "LIMITS";
        yield return "No VR room walking in MR";
        yield return "Room scan needed to place";
        yield return "Use DEBUG if something fails";
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
        if (!string.IsNullOrEmpty(pendingSelStatus))
        {
            string act = string.IsNullOrEmpty(pendingActStatus) ? "-" : pendingActStatus;
            string status = Truncate($"SEL:{pendingSelStatus}  ACT:{act}", screen.CharactersXCount - 2);
            PrintSelectionLine(1, screen.CharactersYCount - 3, status, true);
        }

        screen.Print(1, screen.CharactersYCount - 2, text, false);
    }

    const string BackFooterHint = "Press B to go back";
    const string RowColumnFooter = "L/R ACT  A OK  B ESC";

    bool UsesRowColumnNavigation(Screen screen) =>
        screen == Screen.Magazines
        || (screen == Screen.PlacedInstances
            && envListFilter == CabinetListFilter.ShowAdded
            && !envListFocusOnFilter
            && PlacedInstanceShowAddedHasTune())
        || screen == Screen.Mesh
        || screen == Screen.Adjustments
        || screen == Screen.GlobalLight
        || screen == Screen.LightTune
        || screen == Screen.Debug;

    static bool UsesPlusMinusActionCursor(Screen screen) =>
        screen == Screen.Adjustments
        || screen == Screen.GlobalLight
        || screen == Screen.LightTune;

    /// <summary>
    /// Show Added copy rows with Remove + Tune: L/R moves >> on the ACTION option (like Adjustments +/-).
    /// </summary>
    bool UsesShowAddedOptionCursor() =>
        currentScreen == Screen.PlacedInstances
        && envListFilter == CabinetListFilter.ShowAdded
        && !envListFocusOnFilter
        && PlacedInstanceShowAddedHasTune();

    static string RowActionLabel(RowActionKind kind) =>
        kind switch
        {
            RowActionKind.Add => "ADD",
            RowActionKind.Options => "OPTS",
            RowActionKind.Remove => "REMOVE",
            RowActionKind.Move => "REPOS",
            RowActionKind.Tune => "TUNE",
            RowActionKind.Decrease => "-",
            RowActionKind.Increase => "+",
            RowActionKind.ToggleOn => "ON",
            RowActionKind.ToggleOff => "OFF",
            RowActionKind.Show => "SHOW",
            RowActionKind.Hide => "HIDE",
            RowActionKind.OpenDetail => "ENTER",
            _ => "?"
        };

    /// <summary>
    /// Clipper/DOS-style table. Pass null/empty title to omit the in-box title row
    /// (use an external header like Phone Booth instead).
    /// Returns first data row. Call EndListTable after the data loop.
    /// </summary>
    int BeginListTable(string title, string nameCol, string flagCol, int nameWidth, int startRow = 0, int flagWidth = -1)
    {
        tableNameWidth = nameWidth;
        tableShowFlag = !string.IsNullOrEmpty(flagCol);
        tableFlagWidth = tableShowFlag
            ? Mathf.Max(1, flagWidth > 0 ? flagWidth : TableFlagWidth)
            : TableFlagWidth;
        tableActWidth = ComputeTableActWidth(tableNameWidth, tableShowFlag);
        tableGridActive = true;

        int r = startRow;
        screen.Print(0, r++, BuildOuterTopRule(), false);
        if (!string.IsNullOrEmpty(title))
        {
            screen.Print(0, r++, BuildTitleRow(title), false);
            screen.Print(0, r++, BuildColumnRule(), false);
        }

        screen.Print(
            0,
            r++,
            BuildTableRow(
                PadRight(Truncate(nameCol, tableNameWidth), tableNameWidth),
                tableShowFlag ? PadRight(Truncate(flagCol, tableFlagWidth), tableFlagWidth) : null,
                PadRight("ACTION", tableActWidth)),
            false);
        screen.Print(0, r++, BuildColumnRule(), false);
        return r;
    }

    /// <summary>Closes the Clipper table with optional footer strip (like "Registros: 003").
    /// Pads empty data rows so the box stretches down to just above the page footer.</summary>
    void EndListTable(int rowAfterData, string footer = null)
    {
        if (!tableGridActive)
            return;

        int chromeRows = string.IsNullOrEmpty(footer) ? 1 : 3;
        const int pageFooterRows = 3; // SEL / hint / Press B
        int maxDataExclusive = screen.CharactersYCount - pageFooterRows - chromeRows;
        if (maxDataExclusive < rowAfterData)
            maxDataExclusive = rowAfterData;

        string emptyName = new string(' ', tableNameWidth);
        string emptyFlag = tableShowFlag ? new string(' ', tableFlagWidth) : null;
        string emptyAct = new string(' ', tableActWidth);
        while (rowAfterData < maxDataExclusive)
        {
            screen.Print(0, rowAfterData, BuildTableRow(emptyName, emptyFlag, emptyAct), false);
            rowAfterData++;
        }

        if (string.IsNullOrEmpty(footer))
        {
            screen.Print(0, rowAfterData, BuildOuterBottomRule(), false);
        }
        else
        {
            screen.Print(0, rowAfterData, BuildColumnCloseRule(), false);
            screen.Print(0, rowAfterData + 1, BuildFooterRow(footer), false);
            screen.Print(0, rowAfterData + 2, BuildOuterBottomRule(), false);
        }

        tableGridActive = false;
    }

    int ComputeTableActWidth(int nameWidth, bool showFlag)
    {
        // Full-width: |name|flag|act|  or |name|act|
        int used = 1 + nameWidth + 1 + 1;
        if (showFlag)
            used += tableFlagWidth + 1;
        return Mathf.Max(4, screen.CharactersXCount - used);
    }

    char TableH => ScreenGeneratorFont.GLYPH_HORIZONTAL_BORDER;
    char TableV => ScreenGeneratorFont.GLYPH_VERTICAL_BORDER;
    char TableTL => ScreenGeneratorFont.GLYPH_LEFT_UPPER_CORNER;
    char TableTR => ScreenGeneratorFont.GLYPH_RIGHT_UPPER_CORNER;
    char TableBL => ScreenGeneratorFont.GLYPH_LOWER_LEFT_CORNER;
    char TableBR => ScreenGeneratorFont.GLYPH_LOWER_RIGHT_CORNER;

    string BuildOuterTopRule()
    {
        var parts = new System.Text.StringBuilder(screen.CharactersXCount);
        parts.Append(TableTL);
        parts.Append(new string(TableH, screen.CharactersXCount - 2));
        parts.Append(TableTR);
        return FitTableLine(parts.ToString());
    }

    string BuildOuterBottomRule()
    {
        var parts = new System.Text.StringBuilder(screen.CharactersXCount);
        parts.Append(TableBL);
        parts.Append(new string(TableH, screen.CharactersXCount - 2));
        parts.Append(TableBR);
        return FitTableLine(parts.ToString());
    }

    string BuildColumnRule()
    {
        // Font has no ├┼┤ — solid H rule with side verts (no GLYPH_GRID checkerboard).
        var parts = new System.Text.StringBuilder(screen.CharactersXCount);
        parts.Append(TableV);
        parts.Append(new string(TableH, screen.CharactersXCount - 2));
        parts.Append(TableV);
        return FitTableLine(parts.ToString());
    }

    string BuildColumnCloseRule() => BuildColumnRule();

    string BuildTitleRow(string title)
    {
        int inner = screen.CharactersXCount - 2;
        string text = Truncate(title ?? "", inner);
        int pad = Mathf.Max(0, (inner - text.Length) / 2);
        string centered = new string(' ', pad) + text;
        if (centered.Length < inner)
            centered = centered.PadRight(inner);
        return FitTableLine(TableV + centered + TableV);
    }

    string BuildFooterRow(string footer)
    {
        int inner = screen.CharactersXCount - 2;
        string text = " " + Truncate(footer ?? "", inner - 1);
        if (text.Length < inner)
            text = text.PadRight(inner);
        return FitTableLine(TableV + text + TableV);
    }

    string BuildTableRow(string nameCell, string flagCell, string actCell)
    {
        var parts = new System.Text.StringBuilder(screen.CharactersXCount);
        parts.Append(TableV);
        parts.Append(PadRight(Truncate(nameCell ?? "", tableNameWidth), tableNameWidth));
        parts.Append(TableV);
        if (tableShowFlag)
        {
            parts.Append(PadRight(Truncate(flagCell ?? "", tableFlagWidth), tableFlagWidth));
            parts.Append(TableV);
        }

        parts.Append(PadRight(Truncate(actCell ?? "", tableActWidth), tableActWidth));
        parts.Append(TableV);
        return FitTableLine(parts.ToString());
    }

    string FitTableLine(string line)
    {
        int width = screen.CharactersXCount;
        if (line.Length == width)
            return line;
        if (line.Length > width)
            return line.Substring(0, width);
        return line.PadRight(width);
    }

    string FormatActionCell(IReadOnlyList<RowActionKind> actions, bool rowSelected)
    {
        if (!rowSelected || actions == null || actions.Count == 0)
            return string.Empty;

        // Cabinets / Bookshelf / Room Skin / Poster / object copies: Press A to ADD or REMOVE.
        if ((currentScreen == Screen.Cabinets
                || currentScreen == Screen.OfficialObjects
                || currentScreen == Screen.RoomSkins
                || UsesShowAllAddedAddRemove(currentScreen))
            && actions.Count == 1
            && (actions[0] == RowActionKind.Add || actions[0] == RowActionKind.Remove)
            && !(currentScreen == Screen.Cabinets && cabinetFocusOnFilter)
            && !(UsesShowAllAddedAddRemove(currentScreen) && envListFocusOnFilter))
        {
            string verb = actions[0] == RowActionKind.Remove ? "REMOVE" : "ADD";
            return $"Press A to {verb}";
        }

        // Env catalog lists / CONFIG: Press A to enter.
        if ((currentScreen == Screen.Config
                || currentScreen == Screen.CustomObjects
                || (IsFilteredEnvCatalogListScreen(currentScreen)
                    && currentScreen != Screen.Posters
                    && currentScreen != Screen.RoomSkins))
            && actions.Count == 1
            && !envListFocusOnFilter
            && actions[0] == RowActionKind.OpenDetail)
        {
            return "Press A to enter";
        }

        if (currentScreen == Screen.Lights && actions.Count == 1 && !envListFocusOnFilter)
        {
            if (actions[0] == RowActionKind.OpenDetail)
                return "Press A to enter";

            string verb = actions[0] switch
            {
                RowActionKind.Remove => "REMOVE",
                RowActionKind.Add => "ADD",
                RowActionKind.ToggleOn => "ON",
                RowActionKind.ToggleOff => "OFF",
                _ => RowActionLabel(actions[0])
            };
            return $"Press A to {verb}";
        }

        if (currentScreen == Screen.Mesh && actions.Count == 1)
        {
            // ToggleOn = currently off → enable; ToggleOff = currently on → disable.
            string verb = actions[0] == RowActionKind.ToggleOff ? "disable" : "enable";
            return $"Press A to {verb}";
        }

        var parts = new List<string>(actions.Count);
        for (int i = 0; i < actions.Count; i++)
        {
            string label = RowActionLabel(actions[i]);
            if (i == selectedColumnIndex)
                label = $"*{label}*";
            parts.Add(label);
        }

        return string.Join(" ", parts);
    }

    string FormatPlusMinusActionCell(IReadOnlyList<RowActionKind> actions)
    {
        var parts = new List<string>(actions.Count);
        for (int i = 0; i < actions.Count; i++)
        {
            string label = RowActionLabel(actions[i]);
            parts.Add((i == selectedColumnIndex ? SelectionCursorPrefix : "  ") + label);
        }

        return string.Join(" ", parts);
    }

    int GetTableActionColumnX()
    {
        int x = 1 + tableNameWidth + 1;
        if (tableShowFlag)
            x += tableFlagWidth + 1;
        return x;
    }

    /// <summary>
    /// One Clipper data row (vertical bars only — no horizontal line under each record).
    /// Returns the next free screen row.
    /// </summary>
    int DrawListRow(
        int row,
        int listIndex,
        int selectedIndex,
        string label,
        IReadOnlyList<RowActionKind> actions,
        string flag = null,
        int nameWidth = ListRowNameWidth,
        bool? useFlagColumn = null)
    {
        bool rowSelected = listIndex == selectedIndex;
        bool lit = rowSelected;
        bool showFlag = useFlagColumn ?? flag != null;

        if (!tableGridActive || tableNameWidth != nameWidth || tableShowFlag != showFlag)
        {
            tableNameWidth = nameWidth;
            tableShowFlag = showFlag;
            tableActWidth = ComputeTableActWidth(tableNameWidth, tableShowFlag);
            tableGridActive = true;
        }

        string prefix = lit ? SelectionCursorPrefix : "  ";
        int labelBudget = Mathf.Max(1, tableNameWidth - prefix.Length);
        string nameCell = prefix + PadRight(Truncate(label, labelBudget), labelBudget);
        string flagCell = showFlag ? PadRight(Truncate(flag ?? "", tableFlagWidth), tableFlagWidth) : null;

        bool plusMinusCursor = rowSelected
            && UsesPlusMinusActionCursor(currentScreen)
            && actions != null
            && actions.Count >= 2
            && actions[0] == RowActionKind.Decrease
            && actions[1] == RowActionKind.Increase;

        bool showAddedOptionCursor = rowSelected
            && UsesShowAddedOptionCursor()
            && actions != null
            && actions.Count >= 2;

        if (plusMinusCursor || showAddedOptionCursor)
        {
            string actCell = FormatPlusMinusActionCell(actions);
            string line = BuildTableRow(nameCell, flagCell, actCell);
            if (row >= 0 && row < screen.CharactersYCount - 1)
            {
                screen.Print(0, row, line, false);

                // Yellow only on the selected ACTION token (>> REMOVE / >> TUNE, or +/-).
                int actX = GetTableActionColumnX();
                int tokenOffset = 0;
                for (int i = 0; i < actions.Count; i++)
                {
                    string tokenLabel = RowActionLabel(actions[i]);
                    string token = (i == selectedColumnIndex ? SelectionCursorPrefix : "  ") + tokenLabel;
                    if (i == selectedColumnIndex)
                    {
                        PrintSelectionSegment(actX + tokenOffset, row, token, true);
                        break;
                    }

                    tokenOffset += token.Length + 1;
                }
            }

            return row + 1;
        }

        string actCellDefault = FormatActionCell(actions, rowSelected);
        string lineDefault = BuildTableRow(nameCell, flagCell, actCellDefault);

        if (row >= 0 && row < screen.CharactersYCount - 1)
            PrintSelectionLine(0, row, lineDefault, lit);

        return row + 1;
    }

    /// <summary>Yellow selection for a fixed-width segment (does not span the full row).</summary>
    void PrintSelectionSegment(int x, int y, string text, bool lit)
    {
        if (lit)
        {
            screen.ForegroundColorString = "black";
            screen.BackgroundColorString = "yellow";
            screen.Print(x, y, text, false);
            screen.ResetColors();
        }
        else
            screen.Print(x, y, text, false);
    }

    /// <summary>Selected row as a full-width yellow bar (C64 yellow bg + black text).</summary>
    void PrintSelectionLine(int x, int y, string text, bool lit)
    {
        if (lit)
        {
            int width = Mathf.Max(1, screen.CharactersXCount - x);
            if (text.Length < width)
                text = PadRight(Truncate(text, width), width);
            else if (text.Length > width)
                text = text.Substring(0, width);

            screen.ForegroundColorString = "black";
            screen.BackgroundColorString = "yellow";
            screen.Print(x, y, text, false);
            screen.ResetColors();
        }
        else
            screen.Print(x, y, text, false);
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

        ResetSelectionCursorBlink();
        DrawCurrentScreen();
    }

    string SelPrefix(int index) =>
        selectedListIndex == index ? SelectionCursorPrefix : "  ";

    bool SelLit(int index) =>
        selectedListIndex == index;

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
            Screen.Config => GetConfigCategoryRowActions(selectedListIndex),
            Screen.CustomObjects => GetCustomHomeRowActions(selectedListIndex),
            Screen.CustomObjectsOthers => GetCustomEnvCatalogRowActions(selectedListIndex),
            Screen.OfficialObjectsOthers => GetCustomEnvCatalogRowActions(selectedListIndex),
            Screen.OfficialObjects => GetOfficialHomeRowActions(selectedListIndex),
            Screen.Lights => GetLightsRowActionsForSelection(),
            Screen.Posters => GetShowAllAddedEnvRowActions(selectedListIndex),
            Screen.Magazines => GetMagazinesRowActions(selectedListIndex),
            Screen.RoomSkins => GetRoomSkinsRowActions(selectedListIndex),
            Screen.PlacedInstances => GetPlacedInstanceRowActions(selectedListIndex),
            Screen.Mesh => GetMeshRowActions(selectedListIndex),
            Screen.Adjustments => AdjustmentValueActions,
            Screen.GlobalLight => GetGlobalLightRowActions(selectedListIndex),
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

    List<RowActionKind> GetConfigCategoryRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= ConfigCategoryCount)
            return actions;

        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    List<RowActionKind> GetCabinetRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= filteredCabinetNames.Count || registry == null)
            return actions;

        if (registry.IsCabinetInScene(filteredCabinetNames[index]))
            actions.Add(RowActionKind.Remove);
        else
            actions.Add(RowActionKind.Add);

        return actions;
    }

    List<RowActionKind> GetCustomHomeRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= GetCustomHomeListCount())
            return actions;

        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    List<RowActionKind> GetOfficialHomeRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= GetOfficialHomeListCount())
            return actions;

        // Bookshelf is Add/Remove on this home row — no second BOOKSHELVES screen.
        if (index == 0)
        {
            if (magazinesCatalogEntries.Count == 0 || envRegistry == null)
                return actions;

            if (envRegistry.GetBookshelfInstanceCount() > 0)
                actions.Add(RowActionKind.Remove);
            else
                actions.Add(RowActionKind.Add);

            return actions;
        }

        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    List<RowActionKind> GetCustomEnvCatalogRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= filteredEnvCatalogEntries.Count)
            return actions;

        actions.Add(RowActionKind.OpenDetail);
        return actions;
    }

    List<RowActionKind> GetObjectCatalogRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        List<MREnvironmentCatalogEntry> entries = GetActiveObjectCatalogEntries();
        if (index < 0 || index >= entries.Count || envRegistry == null)
            return actions;

        int count = envRegistry.GetInstanceCount(entries[index]);
        if (count > 0)
            actions.Add(RowActionKind.Options);
        actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetLightsRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= lightsCatalogEntries.Count || envRegistry == null)
            return actions;

        MREnvironmentCatalogEntry entry = lightsCatalogEntries[index];
        int count = envRegistry.GetInstanceCount(entry);
        if (count > 0)
            actions.Add(RowActionKind.Options);
        actions.Add(RowActionKind.Add);
        return actions;
    }

    List<RowActionKind> GetShowAllAddedEnvRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (envRegistry == null)
            return actions;

        // Show Added: one row per placed copy. Show All: one row per catalog entry (Add).
        if (envListFilter == CabinetListFilter.ShowAdded)
        {
            if (index >= 0 && index < showAddedEnvPlacements.Count)
                actions.Add(RowActionKind.Remove);
            return actions;
        }

        if (index >= 0 && index < filteredEnvCatalogEntries.Count)
            actions.Add(RowActionKind.Add);

        return actions;
    }

    List<RowActionKind> GetPostersFilteredRowActions(int index) =>
        GetShowAllAddedEnvRowActions(index);

    List<RowActionKind> GetPostersRowActions(int index) =>
        GetShowAllAddedEnvRowActions(index);

    List<RowActionKind> GetMagazinesRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= magazinesCatalogEntries.Count || envRegistry == null)
            return actions;

        // Only one bookshelf in the MR room — Add or Remove on the same row.
        // Match by source (not issue list) so the row still offers Remove after
        // the active magazines change in magazines.yaml.
        if (envRegistry.GetBookshelfInstanceCount() > 0)
            actions.Add(RowActionKind.Remove);
        else
            actions.Add(RowActionKind.Add);

        return actions;
    }

    List<RowActionKind> GetRoomSkinsRowActions(int index)
    {
        var actions = new List<RowActionKind>();
        if (index < 0 || index >= filteredEnvCatalogEntries.Count || envRegistry == null)
            return actions;

        // One skin per surface: active (#=1) → Remove, otherwise Add.
        if (envRegistry.GetInstanceCount(filteredEnvCatalogEntries[index]) > 0)
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

    List<RowActionKind> GetGlobalLightRowActions(int index)
    {
        if (index == 0)
            return GetAutoLightRowActions();
        if (index == 1)
            return new List<RowActionKind>(AdjustmentValueActions);
        return new List<RowActionKind>();
    }

    List<RowActionKind> GetPlacedInstanceRowActions(int index)
    {
        var actions = new List<RowActionKind>();

        // Poster-style: Show All → Add only. Show Added → Remove (+ Tune only if supported).
        if (envListFilter == CabinetListFilter.ShowAll)
        {
            if (index == 0 && CanAddPlacedInstanceCopy())
                actions.Add(RowActionKind.Add);
            return actions;
        }

        if (index < 0 || index >= placedInstances.Count)
            return actions;

        actions.Add(RowActionKind.Remove);
        // Lights: Tune (intensity/range/temp) + Move to reposition.
        if (instancesCatalogEntry.Source == MREnvironmentObjectSource.Light)
        {
            actions.Add(RowActionKind.Tune);
            actions.Add(RowActionKind.Move);
        }
        else if (MRPlacementRayController.ExpectsStickScale(instancesCatalogEntry.Source))
            actions.Add(RowActionKind.Move);
        return actions;
    }

    bool CanAddPlacedInstanceCopy()
    {
        if (instancesCatalogEntry.Source == MREnvironmentObjectSource.Bookshelf
            && envRegistry != null
            && envRegistry.GetBookshelfInstanceCount() > 0)
            return false;

        return true;
    }

    static bool SupportsPlacedInstanceTune(MREnvironmentObjectSource source) =>
        source == MREnvironmentObjectSource.Light;

    bool PlacedInstanceShowAddedHasTune() =>
        SupportsPlacedInstanceTune(instancesCatalogEntry.Source)
        || MRPlacementRayController.ExpectsStickScale(instancesCatalogEntry.Source);

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
            case Screen.CustomObjects:
                return ExecuteCustomObjectsHomeRowAction(action);
            case Screen.OfficialObjects:
                return ExecuteOfficialObjectsHomeRowAction(action);
            case Screen.CustomObjectsOthers:
            case Screen.OfficialObjectsOthers:
                return ExecuteCustomEnvCatalogRowAction(action);
            case Screen.RoomSkins:
                return ExecuteRoomSkinsRowAction(action);
            case Screen.Posters:
                return ExecutePostersRowAction(action);
            case Screen.Lights:
                return ExecuteLightsRowAction(action);
            case Screen.Magazines:
                return ExecuteMagazinesRowAction(action);
            case Screen.PlacedInstances:
                return ExecutePlacedInstanceRowAction(action);
            case Screen.Mesh:
                ExecuteMeshRowAction(action);
                return true;
            case Screen.Adjustments:
                ExecuteAdjustmentRowAction(action);
                return true;
            case Screen.GlobalLight:
                ExecuteGlobalLightRowAction(action);
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
        if (selectedListIndex < 0 || selectedListIndex >= filteredCabinetNames.Count || registry == null)
            return false;

        string cabinetName = filteredCabinetNames[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                BeginAddCabinetWithRay(cabinetName);
                return false;
            case RowActionKind.Remove:
                if (registry.TryRemoveCabinetFromScene(cabinetName))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed {cabinetName}");
                    RefreshCatalog();
                    if (selectedListIndex >= filteredCabinetNames.Count)
                        selectedListIndex = Mathf.Max(0, filteredCabinetNames.Count - 1);
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    bool ExecuteCustomObjectsHomeRowAction(RowActionKind action)
    {
        if (action != RowActionKind.OpenDetail)
            return false;

        if (selectedListIndex < CustomCategoryCount)
        {
            OpenCustomObjectsCategory();
            return false;
        }

        int packageIndex = selectedListIndex - CustomCategoryCount;
        if (packageIndex < 0 || packageIndex >= filteredEnvCatalogEntries.Count || envRegistry == null)
            return false;

        OpenPlacedInstances(filteredEnvCatalogEntries[packageIndex], Screen.CustomObjects);
        return false;
    }

    bool ExecuteOfficialObjectsHomeRowAction(RowActionKind action)
    {
        if (selectedListIndex == 0)
            return ExecuteOfficialHomeBookshelfAction(action);

        if (action != RowActionKind.OpenDetail)
            return false;

        int packageIndex = selectedListIndex - OfficialCategoryCount;
        if (packageIndex < 0 || packageIndex >= filteredEnvCatalogEntries.Count || envRegistry == null)
            return false;

        OpenPlacedInstances(filteredEnvCatalogEntries[packageIndex], Screen.OfficialObjects);
        return false;
    }

    bool ExecuteOfficialHomeBookshelfAction(RowActionKind action)
    {
        if (envRegistry == null)
            return false;

        RefreshMagazinesCatalog();
        if (magazinesCatalogEntries.Count == 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf unavailable — no magazine issues");
            MRDebugLog.LogWarning("Bookshelf unavailable: add images under MR/Magazines/");
            return true;
        }

        MREnvironmentCatalogEntry entry = magazinesCatalogEntries[0];
        switch (action)
        {
            case RowActionKind.Add:
                if (envRegistry.GetBookshelfInstanceCount() > 0)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf add blocked — only one allowed");
                    return true;
                }

                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Remove:
            {
                MREnvironmentPlacement placement = envRegistry.FindFirstBookshelfPlacement();
                if (placement == null)
                    return false;
                if (envRegistry.RemovePlacement(placement.Id))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed bookshelf {entry}");
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            }
            default:
                return false;
        }
    }

    bool ExecuteCustomEnvCatalogRowAction(RowActionKind action)
    {
        if (envRegistry == null)
            return false;

        // Show Added multi-copy catalogs: remove the selected placement row.
        if (UsesShowAllAddedAddRemove(currentScreen)
            && envListFilter == CabinetListFilter.ShowAdded
            && action == RowActionKind.Remove)
        {
            if (selectedListIndex < 0 || selectedListIndex >= showAddedEnvPlacements.Count)
                return false;

            string placementId = showAddedEnvPlacements[selectedListIndex].Id;
            if (!envRegistry.RemovePlacement(placementId))
                return false;

            ConfigManager.WriteConsole($"{LogPrefix} removed placement {placementId}");
            RebuildFilteredEnvCatalogEntries();
            if (selectedListIndex >= showAddedEnvPlacements.Count)
                selectedListIndex = Mathf.Max(0, showAddedEnvPlacements.Count - 1);
            ClampColumnIndexForCurrentRow();
            ClampListScroll();
            return true;
        }

        if (selectedListIndex < 0 || selectedListIndex >= filteredEnvCatalogEntries.Count)
            return false;

        MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Remove:
                if (envRegistry.TryRemoveCatalogEntryFromScene(entry))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed {entry}");
                    RebuildFilteredEnvCatalogEntries();
                    if (selectedListIndex >= filteredEnvCatalogEntries.Count)
                        selectedListIndex = Mathf.Max(0, filteredEnvCatalogEntries.Count - 1);
                    ClampColumnIndexForCurrentRow();
                    ClampListScroll();
                    return true;
                }
                return false;
            case RowActionKind.OpenDetail:
            case RowActionKind.Options:
                OpenPlacedInstances(entry, currentScreen);
                return true;
            default:
                return false;
        }
    }

    bool ExecutePostersRowAction(RowActionKind action) =>
        ExecuteCustomEnvCatalogRowAction(action);

    bool ExecuteObjectCatalogRowAction(RowActionKind action)
    {
        List<MREnvironmentCatalogEntry> entries = GetActiveObjectCatalogEntries();
        if (selectedListIndex < 0 || selectedListIndex >= entries.Count)
            return false;

        MREnvironmentCatalogEntry entry = entries[selectedListIndex];
        Screen returnScreen = GetActiveObjectCatalogReturnScreen();
        switch (action)
        {
            case RowActionKind.Add:
                QueueReturnToPlacedInstances(entry, returnScreen);
                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Options:
                OpenPlacedInstances(entry, returnScreen);
                return true;
            default:
                return false;
        }
    }

    bool ExecuteRoomSkinsRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= filteredEnvCatalogEntries.Count || envRegistry == null)
            return false;

        MREnvironmentCatalogEntry entry = filteredEnvCatalogEntries[selectedListIndex];
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
                    RebuildFilteredEnvCatalogEntries();
                    if (selectedListIndex >= filteredEnvCatalogEntries.Count)
                        selectedListIndex = Mathf.Max(0, filteredEnvCatalogEntries.Count - 1);
                    ClampColumnIndexForCurrentRow();
                    ClampListScroll();
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
        if (action != RowActionKind.OpenDetail)
            return false;

        if (selectedListIndex < LightsGlobalShortcutCount)
        {
            OpenGlobalLight();
            return false;
        }

        int catalogIndex = selectedListIndex - LightsGlobalShortcutCount;
        if (catalogIndex < 0 || catalogIndex >= filteredEnvCatalogEntries.Count || envRegistry == null)
            return false;

        OpenPlacedInstances(filteredEnvCatalogEntries[catalogIndex], Screen.Lights);
        return false;
    }

    List<RowActionKind> GetLightsRowActionsForSelection()
    {
        return GetLightsListRowActions(selectedListIndex);
    }

    bool ExecuteMagazinesRowAction(RowActionKind action)
    {
        if (selectedListIndex < 0 || selectedListIndex >= magazinesCatalogEntries.Count || envRegistry == null)
            return false;

        MREnvironmentCatalogEntry entry = magazinesCatalogEntries[selectedListIndex];
        switch (action)
        {
            case RowActionKind.Add:
                if (envRegistry.GetBookshelfInstanceCount() > 0)
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf add blocked — only one allowed");
                    return true;
                }

                BeginAddEnvironmentWithRay(entry);
                return false;
            case RowActionKind.Remove:
            {
                // Match by source: the placed shelf may hold a different issue
                // list than the current catalog entry (magazines.yaml changed).
                MREnvironmentPlacement placement = envRegistry.FindFirstBookshelfPlacement();
                if (placement == null)
                    return false;
                if (envRegistry.RemovePlacement(placement.Id))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed bookshelf {entry}");
                    if (selectedListIndex >= magazinesCatalogEntries.Count)
                        selectedListIndex = Mathf.Max(0, magazinesCatalogEntries.Count - 1);
                    ClampColumnIndexForCurrentRow();
                    return true;
                }
                return false;
            }
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

    void ExecuteGlobalLightRowAction(RowActionKind action)
    {
        if (selectedListIndex == 0)
        {
            ExecuteAutoLightRowAction(action);
            return;
        }

        if (selectedListIndex != 1)
            return;

        int direction = action == RowActionKind.Increase ? 1 : action == RowActionKind.Decrease ? -1 : 0;
        if (direction == 0)
            return;

        MRAutoLightingVisibility.AdjustIntensity(direction);
    }

    bool ExecutePlacedInstanceRowAction(RowActionKind action)
    {
        if (envRegistry == null)
            return false;

        switch (action)
        {
            case RowActionKind.Add:
                if (!CanAddPlacedInstanceCopy())
                {
                    ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf add blocked — only one allowed");
                    return true;
                }

                QueueReturnToPlacedInstances(instancesCatalogEntry, instancesReturnScreen);
                BeginAddEnvironmentWithRay(instancesCatalogEntry);
                return false;
            case RowActionKind.Move:
                if (selectedListIndex < placedInstances.Count)
                    BeginMoveEnvByPlacementId(placedInstances[selectedListIndex].Id);
                return false;
            case RowActionKind.Tune:
                if (selectedListIndex < placedInstances.Count)
                    OpenLightTuneForPlacement(placedInstances[selectedListIndex].Id);
                return true;
            case RowActionKind.Remove:
                if (selectedListIndex >= placedInstances.Count)
                    return false;
                if (envRegistry.RemovePlacement(placedInstances[selectedListIndex].Id))
                {
                    ConfigManager.WriteConsole($"{LogPrefix} removed instance {placedInstances[selectedListIndex].Id}");
                    RefreshPlacedInstancesList();
                    if (selectedListIndex >= placedInstances.Count)
                        selectedListIndex = Mathf.Max(0, placedInstances.Count - 1);
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
            Screen.Cabinets => filteredCabinetNames.Count,
            Screen.CustomObjects => GetCustomHomeListCount(),
            Screen.CustomObjectsOthers => filteredEnvCatalogEntries.Count,
            Screen.OfficialObjects => GetOfficialHomeListCount(),
            Screen.Config => ConfigCategoryCount,
            Screen.DeleteConfigsConfirm => 2,
            Screen.OfficialObjectsOthers => filteredEnvCatalogEntries.Count,
            Screen.Lights => GetLightsListCount(),
            Screen.Posters => GetShowAllAddedListCount(),
            Screen.Magazines => magazinesCatalogEntries.Count,
            Screen.RoomSkins => filteredEnvCatalogEntries.Count,
            Screen.PlacedInstances => GetPlacedInstancesListCount(),
            Screen.Mesh => MeshOptionCount,
            Screen.GlobalLight => GlobalLightOptionCount,
            Screen.Debug => debugDisplayLines.Count,
            _ => 0
        };
    }

    void MoveLightsSelection(int delta)
    {
        ResetSelectionCursorBlink();

        if (envListFocusOnFilter)
        {
            if (delta > 0)
            {
                envListFocusOnFilter = false;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                DrawCurrentScreen();
            }

            return;
        }

        int count = GetLightsListCount();
        if (delta < 0 && selectedListIndex == 0)
        {
            envListFocusOnFilter = true;
            DrawCurrentScreen();
            return;
        }

        selectedListIndex += delta;
        if (selectedListIndex < 0)
            selectedListIndex = 0;
        else if (selectedListIndex >= count)
            selectedListIndex = count - 1;

        selectedColumnIndex = 0;
        ClampColumnIndexForCurrentRow();
        ClampLightsScroll();
        DrawCurrentScreen();
    }

    void MoveEnvCatalogSelection(int delta)
    {
        ResetSelectionCursorBlink();

        bool useAlphabet = UsesAlphabetLetterFilter(currentScreen);

        if (envListFocusOnFilter)
        {
            if (useAlphabet && !envFocusOnAlphabet)
            {
                // Show All / Show Added row
                if (delta > 0)
                {
                    envFocusOnAlphabet = true;
                    DrawCurrentScreen();
                }

                return;
            }

            if (useAlphabet && envFocusOnAlphabet)
            {
                // Alphabet row
                if (delta < 0)
                {
                    envFocusOnAlphabet = false;
                    DrawCurrentScreen();
                    return;
                }

                if (delta > 0 && GetListCount() > 0)
                {
                    envListFocusOnFilter = false;
                    envFocusOnAlphabet = false;
                    selectedListIndex = 0;
                    selectedColumnIndex = 0;
                    listScrollOffset = 0;
                    DrawCurrentScreen();
                }

                return;
            }

            // Non-alphabet screens: filter row → list
            if (delta > 0 && GetListCount() > 0)
            {
                envListFocusOnFilter = false;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                DrawCurrentScreen();
            }

            return;
        }

        int count = GetListCount();
        if (count == 0)
        {
            envListFocusOnFilter = true;
            if (useAlphabet)
                envFocusOnAlphabet = true;
            DrawCurrentScreen();
            return;
        }

        if (delta < 0 && selectedListIndex == 0)
        {
            envListFocusOnFilter = true;
            if (useAlphabet)
                envFocusOnAlphabet = true;
            DrawCurrentScreen();
            return;
        }

        selectedListIndex += delta;
        if (selectedListIndex < 0)
            selectedListIndex = 0;
        else if (selectedListIndex >= count)
            selectedListIndex = count - 1;

        selectedColumnIndex = 0;
        ClampColumnIndexForCurrentRow();
        ClampListScroll();
        DrawCurrentScreen();
    }

    void MovePlacedInstancesSelection(int delta)
    {
        ResetSelectionCursorBlink();

        if (envListFocusOnFilter)
        {
            if (delta > 0 && GetPlacedInstancesListCount() > 0)
            {
                envListFocusOnFilter = false;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                DrawCurrentScreen();
            }

            return;
        }

        int count = GetPlacedInstancesListCount();
        if (count == 0)
        {
            envListFocusOnFilter = true;
            DrawCurrentScreen();
            return;
        }

        if (delta < 0 && selectedListIndex == 0)
        {
            envListFocusOnFilter = true;
            DrawCurrentScreen();
            return;
        }

        selectedListIndex += delta;
        if (selectedListIndex < 0)
            selectedListIndex = 0;
        else if (selectedListIndex >= count)
            selectedListIndex = count - 1;

        selectedColumnIndex = 0;
        ClampColumnIndexForCurrentRow();
        ClampListScroll();
        DrawCurrentScreen();
    }

    void MoveCabinetSelection(int delta)
    {
        ResetSelectionCursorBlink();

        if (cabinetFocusOnFilter)
        {
            if (!cabinetFocusOnAlphabet)
            {
                // Show All / Show Added row
                if (delta > 0)
                {
                    cabinetFocusOnAlphabet = true;
                    DrawCurrentScreen();
                }

                return;
            }

            // Alphabet row
            if (delta < 0)
            {
                cabinetFocusOnAlphabet = false;
                DrawCurrentScreen();
                return;
            }

            if (delta > 0 && filteredCabinetNames.Count > 0)
            {
                cabinetFocusOnFilter = false;
                cabinetFocusOnAlphabet = false;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                DrawCurrentScreen();
            }

            return;
        }

        int count = filteredCabinetNames.Count;
        if (count == 0)
        {
            cabinetFocusOnFilter = true;
            cabinetFocusOnAlphabet = true;
            DrawCurrentScreen();
            return;
        }

        if (delta < 0 && selectedListIndex == 0)
        {
            cabinetFocusOnFilter = true;
            cabinetFocusOnAlphabet = true;
            DrawCurrentScreen();
            return;
        }

        selectedListIndex += delta;
        if (selectedListIndex < 0)
            selectedListIndex = 0;
        else if (selectedListIndex >= count)
            selectedListIndex = count - 1;

        selectedColumnIndex = 0;
        ClampColumnIndexForCurrentRow();
        ClampListScroll();
        DrawCurrentScreen();
    }

    void MoveSelection(int delta)
    {
        ResetSelectionCursorBlink();
        switch (currentScreen)
        {
            case Screen.NavMain:
                if (delta < 0)
                    navMenu.PreviousOption();
                else
                    navMenu.NextOption();
                DrawCurrentScreen();
                break;

            case Screen.Config:
                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = ConfigCategoryCount - 1;
                else if (selectedListIndex >= ConfigCategoryCount)
                    selectedListIndex = 0;
                DrawCurrentScreen();
                break;

            case Screen.DeleteConfigsConfirm:
                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = 1;
                else if (selectedListIndex > 1)
                    selectedListIndex = 0;
                DrawCurrentScreen();
                break;

            case Screen.CustomObjects:
            case Screen.OfficialObjects:
            case Screen.RoomSkins:
            case Screen.Magazines:
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

            case Screen.Cabinets:
                MoveCabinetSelection(delta);
                break;

            case Screen.Lights:
                MoveLightsSelection(delta);
                break;

            case Screen.CustomObjectsOthers:
            case Screen.OfficialObjectsOthers:
            case Screen.Posters:
                MoveEnvCatalogSelection(delta);
                break;

            case Screen.PlacedInstances:
                MovePlacedInstancesSelection(delta);
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

            case Screen.Help:
                if (helpWrappedLines.Count == 0)
                    return;

                helpScrollOffset += delta;
                if (helpScrollOffset < 0)
                    helpScrollOffset = 0;
                else if (helpScrollOffset > Mathf.Max(0, helpWrappedLines.Count - VisibleHelpRows))
                    helpScrollOffset = Mathf.Max(0, helpWrappedLines.Count - VisibleHelpRows);

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

            case Screen.GlobalLight:
                selectedListIndex += delta;
                if (selectedListIndex < 0)
                    selectedListIndex = GlobalLightOptionCount - 1;
                else if (selectedListIndex >= GlobalLightOptionCount)
                    selectedListIndex = 0;
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

            case Screen.Config:
                OpenConfigCategory();
                break;

            case Screen.DeleteConfigsConfirm:
                HandleDeleteConfigsConfirmChoice();
                break;

            case Screen.DeleteConfigsDone:
                currentScreen = Screen.Config;
                selectedListIndex = 3;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;

            case Screen.CustomObjects:
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;

            case Screen.Cabinets:
                if (cabinetFocusOnFilter)
                    return;
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;

            case Screen.OfficialObjects:
            case Screen.CustomObjectsOthers:
            case Screen.OfficialObjectsOthers:
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;

            case Screen.Posters:
            case Screen.Lights:
                if (envListFocusOnFilter)
                    return;
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;

            case Screen.PlacedInstances:
                if (envListFocusOnFilter)
                    return;
                if (ExecuteSelectedRowAction())
                    DrawCurrentScreen();
                break;

            case Screen.RoomSkins:
            case Screen.Magazines:
            case Screen.Mesh:
            case Screen.Adjustments:
            case Screen.GlobalLight:
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
                cabinetListFilter = CabinetListFilter.ShowAll;
                cabinetLetterFilter = 0;
                cabinetFocusOnFilter = true;
                cabinetFocusOnAlphabet = false;
                RebuildFilteredCabinetNames();
                currentScreen = Screen.Cabinets;
                break;
            case "ADJUSTMENTS":
            case "MESH":
            case "MOVE CONFIG":
                return;
            case "PHONE BOOTH":
                if (MRPhoneBoothVisibility.IsPhoneBoothSuppressedForQuickTravel())
                    return;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                currentScreen = Screen.PhoneBooth;
                break;
            case "CUSTOM OBJECTS":
                RefreshObjectCatalogs();
                RefreshPostersCatalog();
                RefreshRoomSkinsCatalog();
                envListFilter = CabinetListFilter.ShowAll;
                RebuildFilteredEnvCatalogEntries();
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.CustomObjects;
                break;
            case "OFFICIAL OBJECTS":
                RefreshObjectCatalogs();
                RefreshMagazinesCatalog();
                envListFilter = CabinetListFilter.ShowAll;
                envListFocusOnFilter = false;
                RebuildFilteredEnvCatalogEntries();
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.OfficialObjects;
                break;
            case "OFFICIAL LIGHTS":
                OpenOfficialLights();
                return;
            case "CONFIG":
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Config;
                break;
            case "DEBUG":
                listScrollOffset = 0;
                debugDisplayLines.Clear();
                debugDisplayLines.AddRange(MRDebugLog.BuildDisplayLines());
                selectedListIndex = FindFirstDebugErrorLineIndex();
                currentScreen = Screen.Debug;
                break;
            case "HELP":
                helpScrollOffset = 0;
                BuildHelpWrappedLines();
                currentScreen = Screen.Help;
                break;
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
        scanStatusLine = "Opening Quest Space Setup...";
        currentScreen = Screen.ScanInProgress;
        DrawCurrentScreen();

        Transform player = ResolvePlayerTransform();
        yield return MRSceneScanRequest.RunSpaceSetupAndReload(player);

        scanStatusLine = "Reloading room data...";
        DrawCurrentScreen();
        yield return null;

        bool scanned = MRSceneScanState.IsRoomScanned();
        scanStatusLine = scanned ? "Room loaded!" : "Room not found";
        DrawCurrentScreen();

        yield return new WaitForSecondsRealtime(2f);

        scanStatusLine = "";
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

        if (scanned)
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
            case Screen.CustomObjectsOthers:
            case Screen.Posters:
            case Screen.RoomSkins:
            {
                int returnRow = currentScreen == Screen.RoomSkins ? 1 : 0;
                currentScreen = Screen.CustomObjects;
                envListFilter = CabinetListFilter.ShowAll;
                envListFocusOnFilter = false;
                RebuildFilteredEnvCatalogEntries();
                selectedListIndex = returnRow;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            }
            case Screen.Lights:
                currentScreen = Screen.NavMain;
                navMenu.selectedIndex = 0;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.Magazines:
            {
                currentScreen = Screen.OfficialObjects;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                envListFocusOnFilter = false;
                RefreshObjectCatalogs();
                RefreshMagazinesCatalog();
                RebuildFilteredEnvCatalogEntries();
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            }
            case Screen.OfficialObjectsOthers:
                currentScreen = Screen.OfficialObjects;
                selectedListIndex = OfficialCategoryCount;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                envListFocusOnFilter = false;
                RefreshObjectCatalogs();
                RebuildFilteredEnvCatalogEntries();
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.GlobalLight:
                currentScreen = Screen.Lights;
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                envListFocusOnFilter = false;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.Adjustments:
            case Screen.Mesh:
                currentScreen = Screen.Config;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.DeleteConfigsConfirm:
            case Screen.DeleteConfigsDone:
                currentScreen = Screen.Config;
                selectedListIndex = 3;
                navCooldown = navRepeatDelay;
                SyncConfirmControlEdgeState();
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;
            case Screen.Cabinets:
            case Screen.PhoneBooth:
            case Screen.Config:
            case Screen.CustomObjects:
            case Screen.OfficialObjects:
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

    void OpenConfigCategory()
    {
        switch (selectedListIndex)
        {
            case 0:
                if (!TryBeginMoveConfig())
                {
                    navCooldown = navRepeatDelay;
                    SyncConfirmControlEdgeState();
                    DrawCurrentScreen();
                }
                return;
            case 1:
                selectedAdjustmentIndex = 0;
                selectedColumnIndex = 0;
                currentScreen = Screen.Adjustments;
                break;
            case 2:
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Mesh;
                break;
            default:
                // Default selection to "No, cancel" so A alone cannot wipe by accident.
                selectedListIndex = 1;
                currentScreen = Screen.DeleteConfigsConfirm;
                break;
        }

        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    void HandleDeleteConfigsConfirmChoice()
    {
        if (selectedListIndex != 0)
        {
            currentScreen = Screen.Config;
            selectedListIndex = 3;
            navCooldown = navRepeatDelay;
            SyncConfirmControlEdgeState();
            SyncBackControlEdgeState();
            DrawCurrentScreen();
            return;
        }

        ExecuteDeleteAllLayoutConfigs();
    }

    void ExecuteDeleteAllLayoutConfigs()
    {
        lastDeletedYamlCount = MRPaths.DeleteAllYamlFiles();
        registry?.ClearLayoutAndDespawn();
        envRegistry?.ClearLayoutAndDespawn();
        RefreshObjectCatalogs();
        RefreshLightsCatalog();
        RefreshPostersCatalog();
        RefreshMagazinesCatalog();
        RefreshRoomSkinsCatalog();

        ConfigManager.WriteConsole($"{LogPrefix} deleted {lastDeletedYamlCount} YAML file(s) and despawned MR objects");
        MRDebugLog.LogWarning($"Deleted {lastDeletedYamlCount} MR YAML file(s)");
        MRTransitionLog.LogStep("DeleteConfigs", $"deletedFiles={lastDeletedYamlCount}");

        // Start config-cabinet placement ray (CRT suspends). Skip Done screen when ray is active.
        if (MRConfigurationCabinetController.Instance != null
            && MRConfigurationCabinetController.Instance.ResetPlacementAfterConfigWipe())
        {
            currentScreen = Screen.Idle;
            return;
        }

        currentScreen = Screen.DeleteConfigsDone;
        selectedListIndex = 0;
        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    bool TryBeginMoveConfig()
    {
        if (MRConfigurationCabinetController.Instance != null
            && MRConfigurationCabinetController.Instance.BeginRepositionWithRay())
            return true;

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} MOVE CONFIG failed (cabinet missing or placement ray blocked)");
        MRDebugLog.LogWarning("MOVE CONFIG failed (cabinet missing or placement ray blocked)");
        MRTransitionLog.LogWarning("MOVE CONFIG failed");
        return false;
    }

    void OpenCustomObjectsCategory()
    {
        switch (selectedListIndex)
        {
            case 0:
                RefreshPostersCatalog();
                ResetEnvListFilterUi();
                currentScreen = Screen.Posters;
                break;
            default:
                RefreshRoomSkinsCatalog();
                envListFilter = CabinetListFilter.ShowAll;
                envListFocusOnFilter = false;
                RebuildFilteredEnvCatalogEntries();
                selectedListIndex = 0;
                selectedColumnIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.RoomSkins;
                break;
        }

        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    void OpenOfficialLights()
    {
        RefreshLightsCatalog();
        ResetEnvListFilterUi();
        currentScreen = Screen.Lights;
        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    void OpenGlobalLight()
    {
        selectedListIndex = 0;
        selectedColumnIndex = 0;
        currentScreen = Screen.GlobalLight;
        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
    }

    void OpenOfficialObjectsCategory()
    {
        // Legacy entry for Official Objects sub-lists (props). Bookshelf is
        // Add/Remove on the Official Objects home row — no Magazines screen.
        RefreshObjectCatalogs();
        ResetEnvListFilterUi();
        currentScreen = Screen.OfficialObjectsOthers;

        navCooldown = navRepeatDelay;
        SyncConfirmControlEdgeState();
        SyncBackControlEdgeState();
        DrawCurrentScreen();
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

        if (entry.Source == MREnvironmentObjectSource.Bookshelf)
        {
            BeginAddBookshelfWithRay(entry);
            return;
        }

        if (entry.Source == MREnvironmentObjectSource.Magazine)
        {
            BeginAddMagazineWithRay(entry);
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

        ComputeInitialPlacementPose(
            PlacementSurfaceType.Wall,
            out Vector3 worldPos,
            out Quaternion worldRot,
            PlacementFacingAxis.NegativeX);

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

    void BeginAddMagazineWithRay(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (placementRay.IsActive)
            return;

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        GameObject prefab = MREnvironmentCatalog.LoadMagazinePrefab();
        MRPlacementProfile prefabProfile = MRPlacementProfile.Resolve(prefab);
        PlacementSurfaceType initialSurface = prefabProfile != null
            ? prefabProfile.surfaceType
            : PlacementSurfaceType.Table;
        ComputeInitialPlacementPose(initialSurface, out Vector3 worldPos, out Quaternion worldRot);

        if (!envRegistry.TrySpawnTransientCatalogEntry(entry, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} magazine add ray spawn failed for {entry}");
            MRDebugLog.LogError($"Magazine add failed: {entry} (spawn)");
            return;
        }

        BeginEnvironmentPlacementRay(entry, root);
        ConfigManager.WriteConsole($"{LogPrefix} magazine add ray begin {entry}");
    }

    void BeginAddBookshelfWithRay(MREnvironmentCatalogEntry entry)
    {
        if (envRegistry == null || placementRay == null || placementMoveActive || placementEnvMoveActive || placementAddActive)
            return;

        if (placementRay.IsActive)
            return;

        if (envRegistry.GetBookshelfInstanceCount() > 0)
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf add blocked — only one allowed");
            return;
        }

        MRConfigurationCabinetController.Instance?.SuspendEditForGameCabinetPlacement();

        GameObject prefab = MREnvironmentCatalog.LoadPrefab(MREnvironmentCatalog.BookshelfPrefabName);
        MRPlacementProfile prefabProfile = MRPlacementProfile.Resolve(prefab);
        PlacementSurfaceType initialSurface = prefabProfile != null
            ? prefabProfile.surfaceType
            : PlacementSurfaceType.Floor;
        ComputeInitialPlacementPose(initialSurface, out Vector3 worldPos, out Quaternion worldRot);

        if (!envRegistry.TrySpawnTransientCatalogEntry(entry, mrSpaceOrigin, worldPos, worldRot, out GameObject root))
        {
            ConfigManager.WriteConsoleWarning($"{LogPrefix} bookshelf add ray spawn failed for {entry}");
            MRDebugLog.LogError($"Bookshelf add failed: {entry} (spawn)");
            return;
        }

        BeginEnvironmentPlacementRay(entry, root);
        ConfigManager.WriteConsole($"{LogPrefix} bookshelf add ray begin {entry}");
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
            confirmCallback: (finalPos, finalRot, anchor) =>
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
                    catalogEntry, spawned, mrSpaceOrigin, finalPos, finalRot, anchor);

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
            },
            allowStickScale: MRPlacementRayController.ExpectsStickScale(entry.Source));
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
            confirmCallback: (worldPos, worldRot, anchor) =>
            {
                placementEnvMoveActive = false;
                bool saved = envRegistry.TryUpdatePlacementPose(
                    movingEnvPlacementId, mrSpaceOrigin, worldPos, worldRot, anchor);
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
            },
            allowStickScale: MRPlacementRayController.ExpectsStickScale(placement));

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
            confirmCallback: (finalPos, finalRot, anchor) =>
            {
                placementAddActive = false;
                string name = pendingAddCabinetName;
                GameObject spawned = pendingAddRoot;
                pendingAddCabinetName = null;
                pendingAddRoot = null;

                if (spawned != null)
                    spawned.transform.SetPositionAndRotation(finalPos, finalRot);

                bool saved = registry.TryFinalizeTransientCabinetAdd(
                    name, spawned, mrSpaceOrigin, finalPos, finalRot, anchor);

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
            envListFilter = CabinetListFilter.ShowAll;
            envListFocusOnFilter = false;
            currentScreen = Screen.PlacedInstances;
            if (placedInstances.Count > 0)
                selectedListIndex = placedInstances.Count - 1;
            else
                selectedListIndex = placedInstances.Count; // "+ Add" when empty
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
            confirmCallback: (worldPos, worldRot, anchor) =>
            {
                placementMoveActive = false;
                bool saved = registry.TryUpdatePlacementPose(
                    movingPlacementId, mrSpaceOrigin, worldPos, worldRot, anchor);
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
        int visibleRows = VisibleCabinetRows;
        if (currentScreen == Screen.CustomObjects)
            visibleRows = GetCustomHomeVisibleRows();
        else if (currentScreen == Screen.OfficialObjects)
            visibleRows = GetOfficialObjectsVisibleRows();
        else if (currentScreen == Screen.Cabinets)
            visibleRows = GetFilterListVisibleRows(CabinetsTableStartRow);
        else if (currentScreen == Screen.Posters)
            visibleRows = GetFilterListVisibleRows(PostersTableStartRow);
        else if (currentScreen == Screen.CustomObjectsOthers
            || currentScreen == Screen.OfficialObjectsOthers
            || currentScreen == Screen.RoomSkins
            || currentScreen == Screen.Magazines
            || currentScreen == Screen.PlacedInstances)
            visibleRows = GetFilterListVisibleRows(
                currentScreen == Screen.RoomSkins || currentScreen == Screen.Magazines ? 2 : 3);
        else if (currentScreen == Screen.Debug)
            visibleRows = GetFilterListVisibleRows(tableStartRow: 2);
        int maxOffset = Mathf.Max(0, count - visibleRows);
        if (selectedListIndex < listScrollOffset)
            listScrollOffset = selectedListIndex;
        else if (selectedListIndex >= listScrollOffset + visibleRows)
            listScrollOffset = selectedListIndex - visibleRows + 1;
        listScrollOffset = Mathf.Clamp(listScrollOffset, 0, maxOffset);
    }

    void ClampLightsScroll()
    {
        int count = Mathf.Max(0, GetLightsListCount());
        if (count == 0)
        {
            selectedListIndex = 0;
            listScrollOffset = 0;
            return;
        }

        selectedListIndex = Mathf.Clamp(selectedListIndex, 0, count - 1);

        int catalogVisibleRows = GetFilterListVisibleRows();
        int maxOffset = Mathf.Max(0, count - catalogVisibleRows);
        if (selectedListIndex < listScrollOffset)
            listScrollOffset = selectedListIndex;
        else if (selectedListIndex >= listScrollOffset + catalogVisibleRows)
            listScrollOffset = selectedListIndex - catalogVisibleRows + 1;
        listScrollOffset = Mathf.Clamp(listScrollOffset, 0, maxOffset);
    }

    void ComputeInitialFloorPose(out Vector3 worldPos, out Quaternion worldRot)
    {
        ComputeInitialPlacementPose(PlacementSurfaceType.Floor, out worldPos, out worldRot);
    }

    void ComputeInitialPlacementPose(
        PlacementSurfaceType surfaceType,
        out Vector3 worldPos,
        out Quaternion worldRot,
        PlacementFacingAxis wallFacingAxis = PlacementFacingAxis.NegativeX)
    {
        Transform player = Camera.main != null ? Camera.main.transform : transform;
        MREnvironmentSurfaces surfaces = MREnvironmentSurfaces.Instance;
        Vector3 footprint = Vector3.one * 0.45f;
        float nearDistance = Mathf.Min(spawnDistanceMeters, 1.25f);

        // Always start near the player, inside the room (no room-center / random teleport).
        if (surfaces == null
            || !surfaces.TryGetInRoomPoseNearPlayer(
                player,
                nearDistance,
                footprint,
                out worldPos,
                out worldRot,
                towardRoomCenterBlend: 0f))
        {
            ComputeSpawnPoseInFrontOfPlayer(player, nearDistance, out worldPos, out worldRot);
        }

        if (Mathf.Abs(spawnYOffsetMeters) > 0.0001f)
            worldPos.y += spawnYOffsetMeters;

        if (surfaces == null)
            return;

        if (surfaceType == PlacementSurfaceType.Wall)
        {
            if (surfaces.TryGetWallMountedFramePose(
                    player,
                    spawnDistanceMeters,
                    0.05f,
                    out Vector3 wallPos,
                    out Quaternion wallRot,
                    wallFacingAxis))
            {
                worldPos = wallPos;
                worldRot = wallRot;
            }
            else
            {
                worldPos.y = player.position.y;
            }

            return;
        }

        if (surfaceType == PlacementSurfaceType.Ceiling)
        {
            if (surfaces.TryGetCeilingPointAt(worldPos, out Vector3 ceilingPoint))
                worldPos = ceilingPoint;
            worldRot = RotationFacingPlayer(worldPos, player.position);
            return;
        }

        if (surfaceType == PlacementSurfaceType.Table)
        {
            if (surfaces.TryGetTablePointAt(worldPos, out Vector3 tablePoint))
                worldPos = tablePoint;
            else if (surfaces.TryGetFloorPointAt(worldPos, out Vector3 floorUnderTable))
                worldPos = floorUnderTable + Vector3.up * 0.75f;
            worldRot = RotationFacingPlayer(worldPos, player.position);
        }
    }

    void ComputeSpawnPose(out Vector3 worldPos, out Quaternion worldRot)
    {
        Transform player = Camera.main != null ? Camera.main.transform : transform;
        ComputeSpawnPoseInFrontOfPlayer(player, spawnDistanceMeters, out worldPos, out worldRot);
    }

    static void ComputeSpawnPoseInFrontOfPlayer(
        Transform player,
        float distanceMeters,
        out Vector3 worldPos,
        out Quaternion worldRot)
    {
        Vector3 forward = player != null ? player.forward : Vector3.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        forward.Normalize();

        Vector3 eye = player != null ? player.position : Vector3.zero;
        float distance = distanceMeters > 0.1f ? distanceMeters : 1.5f;
        worldPos = eye + forward * distance;
        worldPos.y = eye.y;
        worldRot = RotationFacingPlayer(worldPos, eye);
    }

    static Quaternion RotationFacingPlayer(Vector3 objectPosition, Vector3 playerPosition)
    {
        Vector3 face = playerPosition - objectPosition;
        face.y = 0f;
        if (face.sqrMagnitude < 0.001f)
            face = Vector3.forward;
        return Quaternion.LookRotation(face.normalized, Vector3.up);
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
            return string.Empty;
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
