/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.curif.LibRetroWrapper;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Video;
#endif
using LC = LibretroControlMapDictionnary;

/// <summary>
/// Handheld device: CRT menu (cores → ROM list) then Libretro gameplay on the portable screen.
/// Supports a single mesh with multiple materials — the screen is the material slot with ScreenPortableGames.
/// </summary>
[RequireComponent(typeof(LibretroControlMap))]
[RequireComponent(typeof(Renderer))]
public class PortableGames : MonoBehaviour
{
    const string LogPrefix = "[PortableGames]";
    const string DefaultSkin = "c64";
    const int VisibleRows = 11;
    const float DefaultScreenCm = 12f;
    const int SquareMenuCharPixels = 8;
    const int SquareMenuCharCount = 32;
    const float DefaultScreenContentScale = 1.08f;

    static readonly string[] RomExtensions =
    {
        ".zip", ".7z", ".nes", ".sfc", ".smc", ".gba", ".gb", ".gbc",
        ".bin", ".cue", ".iso", ".chd", ".pbp", ".md", ".gen", ".sms",
    };

#if UNITY_EDITOR
    const string DefaultEditorVideoRelative = "Video/FMV_Lobby.mp4";

    struct EditorCabinetVideo
    {
        public string Path;
        public string CabinetName;
    }
#endif

    enum Screen
    {
        Idle,
        Cores,
        Games,
        Playing
    }

    [SerializeField] ScreenGenerator screenGenerator;
    [Tooltip("Renderer with multiple materials. Leave empty when PortableGames is on the same object.")]
    [SerializeField] Renderer screenRenderer;
    [Tooltip("CRT material slot (e.g. ScreenPortableGames). Auto-detected from Renderer materials.")]
    [SerializeField] Material screenMaterial;
    [Tooltip("Optional idle/LOD material. Falls back to cabinet ScreenCRTLow_LOD.")]
    [SerializeField] Material screenMaterialOffline;
    [Tooltip("Material slot index for the screen on a multi-material mesh. Auto-detected when empty.")]
    [SerializeField] int screenMaterialIndex;
    [SerializeField] string systemSkin = DefaultSkin;
    [Tooltip("Physical screen size in centimetres (12×12 cm square LCD).")]
    [SerializeField] float screenWidthCm = DefaultScreenCm;
    [SerializeField] float screenHeightCm = DefaultScreenCm;
    [Tooltip("Zoom menu/game image on the LCD (1 = full UV; >1 fills a bit past the bezel).")]
    [SerializeField] float screenContentScaleX = DefaultScreenContentScale;
    [SerializeField] float screenContentScaleY = DefaultScreenContentScale;
    [SerializeField] float navRepeatDelay = 0.16f;
    [SerializeField] bool autoOpenMenuOnStart = false;
    [SerializeField] string gamma = "1.0";
    [SerializeField] string brightness = "1.0";
    [SerializeField] bool playCoinSound = true;
    [Tooltip("Menu CRT shader flip — portable menu uses ScreenGenerator.FlipOutputY instead.")]
    [SerializeField] bool menuInvertX = false;
    [SerializeField] bool menuInvertY = false;
    [Tooltip("Libretro gameplay CRT flip — independent from menu (menu uses ScreenGenerator.FlipOutputY).")]
    [SerializeField] bool gameInvertX = false;
    [SerializeField] bool gameInvertY = true;
#if UNITY_EDITOR
    [Header("Editor Gameplay Preview")]
    [Tooltip("Video stand-in for Libretro frames — same gameShader path as Quest.")]
    [SerializeField] VideoClip editorPreviewVideo;
    [Tooltip("Skip menu on Play and show gameplay preview immediately.")]
    [SerializeField] bool editorGameplayPreviewOnStart;
    [Tooltip("Optional path fallback when Editor Preview clip is empty.")]
    [SerializeField] string editorFallbackVideoPath;
#endif

    Screen currentScreen = Screen.Idle;
    ShaderScreenBase menuShaderOnline;
    ShaderScreenBase menuShaderOffline;
    ShaderScreenBase gameShader;
    LibretroControlMap libretroControlMap;
    CoinSlotController coinSlot;
    ChangeControls changeControls;
    AudioSource audioSource;
    Renderer display;
    string screenName;

    readonly List<string> coreNames = new List<string>();
    readonly List<string> gameNames = new List<string>();
    string selectedCore;
    string selectedGame;

    int selectedListIndex;
    int listScrollOffset;
    float navCooldown;
    bool sessionActive;
    bool gameRunning;
    bool confirmControlWasActive;
    bool backControlWasActive;
    bool secondaryControlWasActive;
    bool screenReady;
#if UNITY_EDITOR
    GameVideoPlayer editorVideoPlayer;
    bool editorVideoActive;
#endif

    IEnumerator Start()
    {
        yield return null;

        if (!TryEnsureScreenSetup())
            yield break;

        yield return WaitForCoresReady();

        RefreshCoreList();

#if UNITY_EDITOR
        if (editorGameplayPreviewOnStart && TryStartEditorGameplayPreview())
            yield break;
#endif

        PortableGamesTwoHandGrab twoHandGrab = GetComponent<PortableGamesTwoHandGrab>();
        if (autoOpenMenuOnStart && twoHandGrab == null)
            BeginSession();
    }

    bool TryEnsureScreenSetup()
    {
        if (screenReady)
            return true;

        if (!ResolveScreenTarget())
        {
            ConfigManager.WriteConsoleError(
                $"{LogPrefix} screen material slot not found. On the mesh Renderer, assign " +
                $"ScreenPortableGames to one material slot (current slots: {ListMaterialSlots(screenRenderer)})");
            return false;
        }

        if (screenGenerator == null)
            screenGenerator = EnsureScreenGeneratorOn(screenRenderer.gameObject);

        display = screenRenderer;
        screenName = display.gameObject.name;
        ApplyScreenMaterialToRenderer();

        libretroControlMap = GetComponent<LibretroControlMap>();
        if (libretroControlMap == null)
            libretroControlMap = gameObject.AddComponent<LibretroControlMap>();

        EnsureCoinSlot();

        audioSource = screenGenerator.GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = screenGenerator.gameObject.AddComponent<AudioSource>();

        var shaderConfig = new Dictionary<string, string> { ["damage"] = "none" };
        if (display != null)
            CreateScreenShaders(shaderConfig);

        ConfigureScreenGeneratorAspect();
        screenGenerator.Init(systemSkin);
        ActivateMenuShader(false);
        ShowIdle();
        screenReady = true;
        ConfigManager.WriteConsole(
            $"{LogPrefix} screen {screenWidthCm:F0}×{screenHeightCm:F0} cm, " +
            $"texture {screenGenerator.TextureWidth}×{screenGenerator.TextureHeight}, " +
            $"chars {screenGenerator.CharactersXCount}×{screenGenerator.CharactersYCount}, slot {screenMaterialIndex}");
        return true;
    }

    void ConfigureScreenGeneratorAspect()
    {
        float aspect = screenHeightCm > 0f ? screenWidthCm / screenHeightCm : 1f;

        if (Mathf.Approximately(aspect, 1f))
        {
            screenGenerator.CharactersXCount = SquareMenuCharCount;
            screenGenerator.CharactersYCount = SquareMenuCharCount;
            int px = SquareMenuCharCount * SquareMenuCharPixels;
            screenGenerator.TextureWidth = px;
            screenGenerator.TextureHeight = px;
        }
        else
        {
            screenGenerator.CharactersXCount = 40;
            screenGenerator.CharactersYCount = Mathf.Max(16, Mathf.RoundToInt(40f / aspect));
            int texW = screenGenerator.CharactersXCount * SquareMenuCharPixels;
            int texH = screenGenerator.CharactersYCount * SquareMenuCharPixels;
            screenGenerator.TextureWidth = texW;
            screenGenerator.TextureHeight = texH;
        }

        screenGenerator.FlipOutputY = true;
    }

    bool ResolveScreenTarget()
    {
        if (screenRenderer == null)
            screenRenderer = GetComponent<Renderer>();

        if (screenRenderer == null)
            screenRenderer = FindScreenRendererByMaterial(transform);

        if (screenRenderer == null)
            return false;

        if (TryFindScreenMaterialSlot(screenRenderer, out Material found, out int index))
        {
            if (screenMaterial == null)
                screenMaterial = found;
            screenMaterialIndex = index;
            return true;
        }

        if (screenMaterial != null && TryFindMaterialIndex(screenRenderer, screenMaterial, out index))
        {
            screenMaterialIndex = index;
            return true;
        }

        return screenRenderer.sharedMaterials.Length > 0 && screenMaterial != null;
    }

    static bool TryFindScreenMaterialSlot(Renderer renderer, out Material material, out int index)
    {
        material = null;
        index = -1;
        if (renderer == null)
            return false;

        Material[] shared = renderer.sharedMaterials;
        for (int i = 0; i < shared.Length; i++)
        {
            if (!MaterialLooksLikeScreen(shared[i]))
                continue;

            material = shared[i];
            index = i;
            return true;
        }

        return false;
    }

    static bool TryFindMaterialIndex(Renderer renderer, Material target, out int index)
    {
        index = -1;
        if (renderer == null || target == null)
            return false;

        Material[] shared = renderer.sharedMaterials;
        for (int i = 0; i < shared.Length; i++)
        {
            if (shared[i] == target || MaterialsMatch(shared[i], target))
            {
                index = i;
                return true;
            }
        }

        return false;
    }

    static bool MaterialLooksLikeScreen(Material mat)
    {
        if (mat == null)
            return false;

        string matName = mat.name;
        return matName.IndexOf("ScreenPortableGames", StringComparison.OrdinalIgnoreCase) >= 0
            || matName.IndexOf("ScreenCRT", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static bool MaterialsMatch(Material a, Material b)
    {
        if (a == null || b == null)
            return false;

        return a == b
            || string.Equals(StripInstanceSuffix(a.name), StripInstanceSuffix(b.name), StringComparison.OrdinalIgnoreCase);
    }

    static string StripInstanceSuffix(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
            return materialName;

        int instanceIndex = materialName.IndexOf(" (Instance)", StringComparison.Ordinal);
        return instanceIndex >= 0 ? materialName.Substring(0, instanceIndex) : materialName;
    }

    void ApplyScreenMaterialToRenderer()
    {
        if (display == null || screenMaterial == null)
            return;

        Material[] mats = display.materials;
        int idx = Mathf.Clamp(screenMaterialIndex, 0, mats.Length - 1);
        mats[idx] = screenMaterial;
        display.materials = mats;
    }

    void CreateScreenShaders(Dictionary<string, string> shaderConfig)
    {
        int matIndex = Mathf.Clamp(screenMaterialIndex, 0, Mathf.Max(0, display.materials.Length - 1));

        if (screenMaterial != null)
        {
            menuShaderOnline = PortableShaderScreen.FromMaterial(display, matIndex, screenMaterial, shaderConfig);
            Material offlineTemplate = screenMaterialOffline != null
                ? screenMaterialOffline
                : Resources.Load<Material>("Cabinets/PreFab/CRTs/ScreenCRTLow_LOD");
            menuShaderOffline = offlineTemplate != null
                ? PortableShaderScreen.FromMaterial(display, matIndex, offlineTemplate, shaderConfig)
                : ShaderScreen.Factory(display, matIndex, "crtlod", shaderConfig);
            gameShader = PortableShaderScreen.FromMaterial(display, matIndex, screenMaterial, shaderConfig);
            return;
        }

        menuShaderOnline = ShaderScreen.Factory(display, matIndex, "crt", shaderConfig);
        menuShaderOffline = ShaderScreen.Factory(display, matIndex, "crtlod", shaderConfig);
        gameShader = ShaderScreen.Factory(display, matIndex, "crt", shaderConfig);
    }

    static Renderer FindScreenRendererByMaterial(Transform root)
    {
        Renderer self = root.GetComponent<Renderer>();
        if (self != null && TryFindScreenMaterialSlot(self, out _, out _))
            return self;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == self)
                continue;

            if (TryFindScreenMaterialSlot(renderer, out _, out _))
                return renderer;
        }

        return self;
    }

    static ScreenGenerator EnsureScreenGeneratorOn(GameObject target)
    {
        ScreenGenerator sg = target.GetComponent<ScreenGenerator>();
        if (sg == null)
            sg = target.AddComponent<ScreenGenerator>();
        return sg;
    }

    static string ListMaterialSlots(Renderer renderer)
    {
        if (renderer == null)
            return "(no renderer)";

        Material[] shared = renderer.sharedMaterials;
        if (shared.Length == 0)
            return "(empty)";

        var slots = new List<string>();
        for (int i = 0; i < shared.Length; i++)
        {
            string matName = shared[i] != null ? shared[i].name : "(null)";
            slots.Add($"[{i}] {matName}");
        }

        return string.Join(", ", slots);
    }

    static IEnumerator WaitForCoresReady()
    {
        ConfigManager.InitFolders();
        CoresController.EnsureLoaded();

        for (int i = 0; i < 120; i++)
        {
            if (CoresController.GetCoreNames().Count > 0)
                yield break;

            if (i == 1)
                CoresController.EnsureLoaded();

            yield return null;
        }

        ConfigManager.WriteConsoleWarning(
            $"{LogPrefix} no cores registered. Put *_libretro_android.so in " +
            $"{ConfigManager.CoresDir} (Editor: %UserProfile%/cabs/cores/)");
    }

    public void BeginSession()
    {
        if (screenGenerator == null)
            return;

        RefreshCoreList();
        SetupMenuActionMap();
        sessionActive = true;
        confirmControlWasActive = false;
        backControlWasActive = false;
        secondaryControlWasActive = false;
        currentScreen = Screen.Cores;
        selectedListIndex = 0;
        listScrollOffset = 0;
        ActivateMenuShader(true);
        DrawCurrentScreen();
        ConfigManager.WriteConsole($"{LogPrefix} session started (cores={coreNames.Count})");
    }

    public void EndSession()
    {
        if (gameRunning)
            StopGame();

        sessionActive = false;
        confirmControlWasActive = false;
        backControlWasActive = false;
        secondaryControlWasActive = false;
        currentScreen = Screen.Idle;
        CleanActionMap();
        ActivateMenuShader(false);
        ShowIdle();
        ConfigManager.WriteConsole($"{LogPrefix} session ended");
    }

    void Update()
    {
#if UNITY_EDITOR
        SyncEditorGameplayVideoShader();
#endif
        if (!IsSessionInputAllowed())
            return;

        if (gameRunning)
        {
            UpdateGameplay();
            if (WasExitGamePressed())
                StopGame();
            return;
        }

        if (!sessionActive || screenGenerator == null)
            return;

        navCooldown -= Time.deltaTime;

        if (WasBackPressed())
        {
            HandleBack();
            return;
        }

        if (navCooldown <= 0f)
        {
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

    void UpdateGameplay()
    {
        if (gameShader == null)
            return;

#if UNITY_EDITOR
        if (editorVideoActive)
        {
            if (editorVideoPlayer != null)
            {
                editorVideoPlayer.Play();
                Texture tex = editorVideoPlayer.texture;
                if (tex != null)
                    gameShader.Refresh(tex);
            }

            ApplyGameScreenTiling();
            gameShader.Update();
            return;
        }
#endif

        if (LibretroMameCore.isRunning(screenName, selectedGame))
        {
            LibretroMameCore.UpdateTexture();
            ApplyGameScreenTiling();
        }

        gameShader.Update();
    }

    void RefreshCoreList()
    {
        coreNames.Clear();
        coreNames.AddRange(CoresController.GetCoreNames());
    }

    void RefreshGameList(string coreName)
    {
        gameNames.Clear();
        if (string.IsNullOrEmpty(coreName))
            return;

        string coreDir = Path.Combine(ConfigManager.RomsDir, coreName);
        if (Directory.Exists(coreDir))
            AddRomsFromDirectory(coreDir);

        gameNames.Sort(StringComparer.OrdinalIgnoreCase);
    }

    void AddRomsFromDirectory(string directory)
    {
        foreach (string file in Directory.GetFiles(directory))
        {
            string ext = Path.GetExtension(file);
            if (string.IsNullOrEmpty(ext))
                continue;
            if (!RomExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                continue;

            string name = Path.GetFileName(file);
            if (!gameNames.Contains(name, StringComparer.OrdinalIgnoreCase))
                gameNames.Add(name);
        }
    }

    void DrawCurrentScreen()
    {
        screenGenerator.Clear();

        switch (currentScreen)
        {
            case Screen.Cores:
                DrawCoresPage();
                break;
            case Screen.Games:
                DrawGamesPage();
                break;
        }

        screenGenerator.DrawScreen();
    }

    void DrawCoresPage()
    {
        screenGenerator.PrintCentered(0, "PORTABLE GAMES", true);
        screenGenerator.PrintLine(1, false, '-');

        if (coreNames.Count == 0)
        {
            screenGenerator.PrintCentered(8, "No cores found", true);
            screenGenerator.PrintCentered(10, "Check cores/ folder", false);
            DrawFooter("B: close");
            return;
        }

        screenGenerator.Print(1, 2, "Core", false);
        screenGenerator.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= coreNames.Count)
                break;

            string name = Truncate(coreNames[idx], 34);
            bool selected = idx == selectedListIndex;
            screenGenerator.Print(1, row, selected ? "> " + name : "  " + name, selected);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < coreNames.Count)
            screenGenerator.Print(1, 20, coreNames[selectedListIndex], false);

        DrawFooter("A: open core   B: close");
    }

    void DrawGamesPage()
    {
        screenGenerator.PrintCentered(0, Truncate(selectedCore ?? "GAMES", 28), true);
        screenGenerator.PrintLine(1, false, '-');

        if (gameNames.Count == 0)
        {
            screenGenerator.PrintCentered(8, "No ROMs found", true);
            screenGenerator.PrintCentered(10, $"downloads/{selectedCore}/", false);
            DrawFooter("B: back");
            return;
        }

        screenGenerator.Print(1, 2, "Game", false);
        screenGenerator.PrintLine(3, false, '-');

        int row = 4;
        for (int i = 0; i < VisibleRows; i++)
        {
            int idx = listScrollOffset + i;
            if (idx >= gameNames.Count)
                break;

            string name = Truncate(gameNames[idx], 34);
            bool selected = idx == selectedListIndex;
            screenGenerator.Print(1, row, selected ? "> " + name : "  " + name, selected);
            row++;
        }

        if (selectedListIndex >= 0 && selectedListIndex < gameNames.Count)
            screenGenerator.Print(1, 20, gameNames[selectedListIndex], false);

        DrawFooter("A: insert coin   B: back");
    }

    void EnsureCoinSlot()
    {
        coinSlot = GetComponentInChildren<CoinSlotController>(true);
        if (coinSlot != null)
        {
            coinSlot.SoundEnabled = playCoinSound;
            return;
        }

        if (GetComponent<AudioSource>() == null)
            gameObject.AddComponent<AudioSource>();

        coinSlot = gameObject.AddComponent<CoinSlotController>();
        coinSlot.SoundEnabled = playCoinSound;
    }

    void PreparePlayerToPlayGame(bool isPlaying)
    {
        if (changeControls == null)
        {
            GameObject player = GameObject.Find("OVRPlayerControllerGalery");
            if (player != null)
                changeControls = player.GetComponent<ChangeControls>();
        }

        changeControls?.PlayerMode(isPlaying);
    }

    void ShowIdle()
    {
        if (screenGenerator == null)
            return;

        screenGenerator.Clear();
        screenGenerator.PrintCentered(1, "PORTABLE GAMES", true);
        screenGenerator.PrintCentered(8, "Handheld ready", false);
        screenGenerator.PrintCentered(11, "Open menu to browse", false);
        screenGenerator.DrawScreen();
    }

    void DrawFooter(string text)
    {
        screenGenerator.Print(1, screenGenerator.CharactersYCount - 2, text, false);
    }

    int GetListCount()
    {
        return currentScreen switch
        {
            Screen.Cores => coreNames.Count,
            Screen.Games => gameNames.Count,
            _ => 0
        };
    }

    void MoveSelection(int delta)
    {
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
        int maxOffset = Mathf.Max(0, count - VisibleRows);
        if (selectedListIndex < listScrollOffset)
            listScrollOffset = selectedListIndex;
        else if (selectedListIndex >= listScrollOffset + VisibleRows)
            listScrollOffset = selectedListIndex - VisibleRows + 1;
        listScrollOffset = Mathf.Clamp(listScrollOffset, 0, maxOffset);
    }

    void HandleConfirm()
    {
        switch (currentScreen)
        {
            case Screen.Cores:
                if (selectedListIndex < 0 || selectedListIndex >= coreNames.Count)
                    return;
                selectedCore = coreNames[selectedListIndex];
                RefreshGameList(selectedCore);
                selectedListIndex = 0;
                listScrollOffset = 0;
                currentScreen = Screen.Games;
                DrawCurrentScreen();
                break;

            case Screen.Games:
                if (selectedListIndex < 0 || selectedListIndex >= gameNames.Count)
                    return;
                selectedGame = gameNames[selectedListIndex];
                StartGame(selectedCore, selectedGame);
                break;
        }
    }

    void HandleBack()
    {
        switch (currentScreen)
        {
            case Screen.Games:
                currentScreen = Screen.Cores;
                selectedListIndex = Mathf.Clamp(
                    coreNames.IndexOf(selectedCore),
                    0,
                    Mathf.Max(0, coreNames.Count - 1));
                listScrollOffset = 0;
                navCooldown = navRepeatDelay;
                SyncBackControlEdgeState();
                DrawCurrentScreen();
                break;

            case Screen.Cores:
                EndSession();
                break;
        }
    }

    void StartGame(string coreName, string gameFile)
    {
        if (string.IsNullOrEmpty(coreName) || string.IsNullOrEmpty(gameFile))
            return;

        if (!CoresController.CoreExists(coreName))
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} core not found: {coreName}");
            return;
        }

        LibretroMameCore.Core = coreName;
        if (LibretroMameCore.getPath(gameFile) == null)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} ROM not found: {gameFile} (core={coreName})");
            return;
        }

        SetupGameActionMap();

        coinSlot.clean();
        coinSlot.insertCoin();
        ConfigManager.WriteConsole($"{LogPrefix} coin inserted — starting {gameFile}");

        LibretroMameCore.Speaker = audioSource;
        LibretroMameCore.SecondsToWaitToFinishLoad = 2;
        LibretroMameCore.Brightness = brightness;
        LibretroMameCore.Gamma = gamma;
        LibretroMameCore.Core = coreName;
        LibretroMameCore.CoinSlot = coinSlot;
        LibretroMameCore.Persistent = false;
        LibretroMameCore.CabEnvironment = null;
        LibretroMameCore.Shader = gameShader;
        LibretroMameCore.ControlMap = libretroControlMap;
        LibretroMameCore.libretroInputDevices = new Dictionary<uint, LibretroInputDevice>
        {
            { 0, LibretroInputDevice.Gamepad }
        };

        bool started;
#if UNITY_EDITOR
        RefreshGameShaderFromTemplate();
        LibretroMameCore.simulateInEditor(screenName, gameFile);
        started = TryStartEditorGameVideo(coreName, gameFile);
#else
        started = LibretroMameCore.Start(screenName, gameFile, new List<string>());
#endif

        if (!started)
        {
            ConfigManager.WriteConsoleError($"{LogPrefix} failed to start {gameFile}");
            coinSlot.clean();
            SetupMenuActionMap();
            return;
        }

        PreparePlayerToPlayGame(true);

#if UNITY_EDITOR
        ActivateEditorGameplayShader();
#else
        LibretroMameCore.StartInteractions();
        LibretroMameCore.StartRunThread();
        ActivateGameShaderForPlay(LibretroMameCore.GameTexture);
#endif
        libretroControlMap.Enable(true);
        gameRunning = true;
        currentScreen = Screen.Playing;
        sessionActive = false;
        ConfigManager.WriteConsole($"{LogPrefix} playing {gameFile} (core={coreName})");
    }

    void StopGame()
    {
        if (!gameRunning)
            return;

#if UNITY_EDITOR
        StopEditorGameplaySimulation();
#endif

        if (LibretroMameCore.isRunning(screenName, selectedGame))
            LibretroMameCore.End(screenName, selectedGame);

        PreparePlayerToPlayGame(false);
        coinSlot?.clean();
        gameRunning = false;
        libretroControlMap.Clean();
        SetupMenuActionMap();
        sessionActive = true;
        currentScreen = Screen.Games;
        ActivateMenuShader(true);
        DrawCurrentScreen();
        ConfigManager.WriteConsole($"{LogPrefix} stopped {selectedGame}");
    }

    void SetupMenuActionMap()
    {
        ControlMapConfiguration conf = new DefaultControlMap();
#if UNITY_EDITOR
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
#endif
        libretroControlMap.CreateFromConfiguration(conf, "inputMap_PortableGames_Menu_" + name);
        libretroControlMap.Enable(true);
    }

    void SetupGameActionMap()
    {
        ControlMapConfiguration conf = new GlobalControlMap();
#if UNITY_EDITOR
        conf.AddMap(LC.KEYB_UP, ControlMapPathDictionary.KEYBOARD_W);
        conf.AddMap(LC.KEYB_DOWN, ControlMapPathDictionary.KEYBOARD_S);
        conf.AddMap(LC.KEYB_LEFT, ControlMapPathDictionary.KEYBOARD_A);
        conf.AddMap(LC.KEYB_RIGHT, ControlMapPathDictionary.KEYBOARD_D);
#endif
        libretroControlMap.CreateFromConfiguration(conf, "inputMap_PortableGames_Game_" + name);
        libretroControlMap.Enable(true);
    }

    void CleanActionMap()
    {
        if (libretroControlMap != null)
            libretroControlMap.Clean();
    }

    void ActivateMenuShader(bool online)
    {
        if (screenGenerator == null)
            return;

        ShaderScreenBase shader = online ? menuShaderOnline : menuShaderOffline;
        screenGenerator.ActivateShader(shader);
        shader.Invert(menuInvertX, menuInvertY);
        ApplyScreenContentScale(shader);
    }

    void ApplyGameScreenTiling()
    {
        if (gameShader == null)
            return;

        gameShader.Invert(gameInvertX, gameInvertY);
    }

    void ActivateGameShaderForPlay(Texture texture)
    {
        if (gameShader == null)
            return;

        gameShader.Activate(texture != null ? texture : ShaderScreenBase.StandByTexture);
        ApplyGameScreenTiling();
        ApplyScreenContentScale(gameShader);
    }

    void ApplyScreenContentScale(ShaderScreenBase shader)
    {
        if (shader is PortableShaderCRT portable)
            portable.ApplyContentScale(screenContentScaleX, screenContentScaleY);
    }

#if UNITY_EDITOR
    void ActivateEditorGameplayShader()
    {
        if (editorVideoActive && editorVideoPlayer != null && editorVideoPlayer.texture != null)
            ActivateGameShaderForPlay(editorVideoPlayer.texture);
        else
            ActivateGameShaderForPlay(ShaderScreenBase.StandByTexture);
    }

    void SyncEditorGameplayVideoShader()
    {
        if (!editorVideoActive || !gameRunning || editorVideoPlayer == null || gameShader == null)
            return;

        Texture tex = editorVideoPlayer.texture;
        if (tex != null && gameShader.Texture != tex)
            ActivateGameShaderForPlay(tex);
    }

    void RefreshGameShaderFromTemplate()
    {
        if (display == null || screenMaterial == null)
            return;

        int matIndex = Mathf.Clamp(screenMaterialIndex, 0, Mathf.Max(0, display.materials.Length - 1));
        var shaderConfig = new Dictionary<string, string> { ["damage"] = "none" };
        gameShader = PortableShaderScreen.FromMaterial(display, matIndex, screenMaterial, shaderConfig);
        LibretroMameCore.Shader = gameShader;
    }

    bool TryStartEditorGameplayPreview()
    {
        RefreshGameShaderFromTemplate();
        SetupGameActionMap();

        if (!TryStartEditorGameVideo(null, null))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} editor: set Editor Preview Video or a fallback clip");
            return false;
        }

        ActivateEditorGameplayShader();
        PreparePlayerToPlayGame(true);
        libretroControlMap.Enable(true);
        gameRunning = true;
        currentScreen = Screen.Playing;
        ConfigManager.WriteConsole($"{LogPrefix} editor: gameplay preview");
        return true;
    }

    void StopEditorGameplaySimulation()
    {
        StopEditorGameVideo();
    }

    void EnsureEditorVideoPlayer()
    {
        if (editorVideoPlayer != null)
            return;

        GameObject host = screenGenerator != null ? screenGenerator.gameObject : gameObject;

        if (host.GetComponent<VideoPlayer>() == null)
            host.AddComponent<VideoPlayer>();

        if (host.GetComponent<TextureCache>() == null)
            host.AddComponent<TextureCache>();

        editorVideoPlayer = host.GetComponent<GameVideoPlayer>();
        if (editorVideoPlayer == null)
            editorVideoPlayer = host.AddComponent<GameVideoPlayer>();
    }

    bool TryStartEditorGameVideo(string coreName, string gameFile)
    {
        string videoPath = null;
        string source = null;

        if (TryResolveEditorPreviewVideoPath(out videoPath))
        {
            source = "editor override";
        }
        else if (!string.IsNullOrEmpty(gameFile)
            && TryResolveEditorCabinetVideo(coreName, gameFile, out EditorCabinetVideo cabVideo))
        {
            videoPath = cabVideo.Path;
            source = $"cabinet {cabVideo.CabinetName}";
        }
        else
        {
            videoPath = ResolveEditorFallbackVideoPath();
            if (!string.IsNullOrEmpty(videoPath))
                source = "fallback";
        }

        if (string.IsNullOrEmpty(videoPath))
        {
            ConfigManager.WriteConsoleWarning(
                $"{LogPrefix} editor: no video for {gameFile} — set Editor Preview or cabinetsdb/*/description.yaml (video.file)");
            return false;
        }

        EnsureEditorVideoPlayer();
        // Gameplay uses gameInvert — same CRT tiling path as Quest Libretro frames.
        editorVideoPlayer.setVideo(videoPath, gameShader, gameInvertX, gameInvertY);
        editorVideoPlayer.SetLoop(true);
        editorVideoPlayer.Play();
        editorVideoActive = true;
        ConfigManager.WriteConsole($"{LogPrefix} editor: playing video {videoPath} ({source})");
        return true;
    }

    void StopEditorGameVideo()
    {
        editorVideoActive = false;
        if (editorVideoPlayer == null)
            return;

        editorVideoPlayer.StopAndReset();
    }

    bool TryResolveEditorCabinetVideo(string coreName, string gameFile, out EditorCabinetVideo result)
    {
        result = default;
        if (!Directory.Exists(ConfigManager.CabinetsDB))
            return false;

        EditorCabinetVideo? romMatch = null;

        foreach (string cabDir in Directory.GetDirectories(ConfigManager.CabinetsDB))
        {
            CabinetInformation info = CabinetInformation.fromYaml(cabDir, cache: true);
            if (info?.video == null || string.IsNullOrEmpty(info.video.file))
                continue;

            if (!CabinetRomMatches(info, gameFile))
                continue;

            string videoPath = info.getPath(info.video.file);
            if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
                continue;

            var candidate = new EditorCabinetVideo
            {
                Path = videoPath,
                CabinetName = !string.IsNullOrEmpty(info.name) ? info.name : Path.GetFileName(cabDir)
            };

            if (!string.IsNullOrEmpty(coreName)
                && string.Equals(info.core, coreName, StringComparison.OrdinalIgnoreCase))
            {
                result = candidate;
                return true;
            }

            romMatch ??= candidate;
        }

        if (!romMatch.HasValue)
            return false;

        result = romMatch.Value;
        return true;
    }

    static bool CabinetRomMatches(CabinetInformation info, string gameFile)
    {
        if (info == null || string.IsNullOrEmpty(gameFile))
            return false;

        if (info.roms != null)
        {
            foreach (string rom in info.roms)
            {
                if (RomFileEquals(rom, gameFile))
                    return true;
            }
        }

        return !string.IsNullOrEmpty(info.rom) && RomFileEquals(info.rom, gameFile);
    }

    static bool RomFileEquals(string cabinetRom, string gameFile)
    {
        return string.Equals(cabinetRom, gameFile, StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetFileName(cabinetRom), Path.GetFileName(gameFile), StringComparison.OrdinalIgnoreCase);
    }

    bool TryResolveEditorPreviewVideoPath(out string path)
    {
        path = null;
        if (editorPreviewVideo == null)
            return false;

        string assetPath = AssetDatabase.GetAssetPath(editorPreviewVideo);
        if (string.IsNullOrEmpty(assetPath))
            return false;

        path = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        return File.Exists(path);
    }

    string ResolveEditorFallbackVideoPath()
    {
        if (!string.IsNullOrEmpty(editorFallbackVideoPath) && File.Exists(editorFallbackVideoPath))
            return editorFallbackVideoPath;

        string bundled = Path.Combine(Application.dataPath, DefaultEditorVideoRelative);
        return File.Exists(bundled) ? bundled : null;
    }
#endif

    bool WasExitGamePressed()
    {
        if (libretroControlMap.isActive(LC.MODIFIER) && libretroControlMap.isActive(LC.EXIT))
            return true;

#if UNITY_EDITOR
        if (MREditorInput.WasAnyPressed(KeyCode.Escape))
            return true;
#endif
        return WasSecondaryPressed();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (gameRunning && LibretroMameCore.isRunning(screenName, selectedGame))
            LibretroMameCore.MoveAudioStreamTo(data);
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        StopEditorGameplaySimulation();
#endif
        if (gameRunning && LibretroMameCore.isRunning(screenName, selectedGame))
            LibretroMameCore.End(screenName, selectedGame);
    }

    static string Truncate(string value, int maxLen)
    {
        if (string.IsNullOrEmpty(value))
            return "(unnamed)";
        if (value.Length <= maxLen)
            return value;
        return value.Substring(0, maxLen - 3) + "...";
    }

    void SyncConfirmControlEdgeState()
    {
        confirmControlWasActive = ControlActive(LC.JOYPAD_A);
#if !UNITY_EDITOR
        if (OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch))
            confirmControlWasActive = true;
#endif
    }

    void SyncBackControlEdgeState()
    {
        backControlWasActive = IsBackControlActive();
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

    float ReadStickY()
    {
#if UNITY_EDITOR
        if (MREditorInput.IsHeld(KeyCode.UpArrow) || MREditorInput.IsHeld(KeyCode.W))
            return 1f;
        if (MREditorInput.IsHeld(KeyCode.DownArrow) || MREditorInput.IsHeld(KeyCode.S))
            return -1f;
        return MREditorInput.GamepadStickY();
#else
        Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
        return stick.y;
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

    bool IsSessionInputAllowed()
    {
#if UNITY_EDITOR
        if (editorGameplayPreviewOnStart && (editorVideoActive || gameRunning))
            return true;
#endif
        PortableGamesTwoHandGrab twoHandGrab = GetComponent<PortableGamesTwoHandGrab>();
        return twoHandGrab == null || twoHandGrab.IsDualHeld;
    }
}
