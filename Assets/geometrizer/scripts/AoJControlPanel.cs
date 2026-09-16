/*
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

// Editor-only live control panel for driving the running game from the Unity
// editor (flat, non-VR). Open it from the "Age of Joy" menu and dock it before
// entering Play mode. It is a developer tool: it never ships in the APK and it
// draws no in-world UI, so the in-VR simulation is untouched.
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AoJControlPanel : EditorWindow
{
    // --- Teleport section state -------------------------------------------
    private SceneDatabase sceneDatabase;
    private Teleportation teleportation;
    private string[] roomLabels = new string[0];
    private SceneDocument[] roomDocs = new SceneDocument[0];
    private int selectedRoomIndex = 0;

    // --- Show Colliding Objects section state ------------------------------
    private bool showColliders = false;
    [System.NonSerialized] private bool updateHooked = false;
    private bool includeTriggers = true;
    private float colliderMargin = 0.05f;
    private float pollHz = 10f;
    [System.NonSerialized] private double lastPollTime = 0;

    [System.NonSerialized] private GameObject playerObject;
    [System.NonSerialized] private CharacterController playerCC;
    [System.NonSerialized] private Collider[] overlapBuffer = new Collider[64];
    [System.NonSerialized] private readonly RaycastHit[] floorRayBuffer = new RaycastHit[16];
    [System.NonSerialized] private readonly HashSet<Collider> selfColliders = new HashSet<Collider>();
    [System.NonSerialized] private readonly HashSet<GameObject> seenThisPoll = new HashSet<GameObject>();
    [System.NonSerialized] private readonly List<CollisionEntry> collisionEntries = new List<CollisionEntry>();
    [System.NonSerialized] private string collisionStatus = "";

    private struct CollisionEntry
    {
        public string label;
        public GameObject go;
    }
    private Vector2 collisionScroll;

    private Vector2 scroll;

    [MenuItem("Age of Joy/Control Panel")]
    public static void ShowWindow()
    {
        AoJControlPanel window = GetWindow<AoJControlPanel>("AoJ Control Panel");
        window.minSize = new Vector2(320, 160);
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        if (Application.isPlaying)
            Refresh();
        SyncUpdateHook();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        if (updateHooked)
        {
            EditorApplication.update -= OnEditorUpdate;
            updateHooked = false;
        }
    }

    private void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
            Refresh();
        else if (change == PlayModeStateChange.ExitingPlayMode)
            ClearCache();

        SyncUpdateHook();
        Repaint();
    }

    private void ClearCache()
    {
        sceneDatabase = null;
        teleportation = null;
        roomLabels = new string[0];
        roomDocs = new SceneDocument[0];
        selectedRoomIndex = 0;

        playerObject = null;
        playerCC = null;
        selfColliders.Clear();
        collisionEntries.Clear();
        collisionStatus = "";
    }

    // Locates the running room's SceneDatabase (on "FixedObject") and the
    // Teleportation shim (it carries the authored ScenesToUnload list), then
    // rebuilds the room dropdown from the full registry.
    private void Refresh()
    {
        sceneDatabase = FindObjectOfType<SceneDatabase>();
        teleportation = FindObjectOfType<Teleportation>();

        if (sceneDatabase == null || sceneDatabase.Scenes == null)
        {
            roomLabels = new string[0];
            roomDocs = new SceneDocument[0];
            selectedRoomIndex = 0;
            return;
        }

        // ALL registered rooms, not just the spawn-point ones the cabinet shows.
        roomDocs = sceneDatabase.Scenes.Where(s => s != null).ToArray();
        roomLabels = roomDocs.Select(BuildLabel).ToArray();
        selectedRoomIndex = Mathf.Clamp(selectedRoomIndex, 0, Mathf.Max(0, roomDocs.Length - 1));
    }

    private static string BuildLabel(SceneDocument doc)
    {
        string label = !string.IsNullOrEmpty(doc.Description) ? doc.Description
                     : !string.IsNullOrEmpty(doc.SceneName) ? doc.SceneName
                     : (doc.Scene != null && !string.IsNullOrEmpty(doc.Scene.Name) ? doc.Scene.Name : "(unnamed)");

        // '/' is a submenu separator in EditorGUILayout.Popup; neutralize it.
        label = label.Replace('/', '-');

        if (string.IsNullOrEmpty(doc.PlayerSpawnGameObjectName))
            label += "  (no spawn)";

        return label;
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Age of Joy — Live Control Panel", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play mode to use the control panel.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        // Lazily (re)acquire references if we entered Play mode while the window
        // was closed, or after a teleport/reload cleared the scene graph.
        if (sceneDatabase == null || teleportation == null)
            Refresh();

        DrawTeleportSection();

        EditorGUILayout.Space();
        DrawSeparator();
        EditorGUILayout.Space();

        DrawPlayerSection();

        EditorGUILayout.Space();
        DrawSeparator();
        EditorGUILayout.Space();

        DrawCollidersSection();

        EditorGUILayout.EndScrollView();

        // Belt-and-suspenders: keep the update hook in sync with the toggle even
        // after a domain reload (which drops the delegate but keeps showColliders).
        SyncUpdateHook();
    }

    private static void DrawSeparator()
    {
        Rect r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.4f));
    }

    private void DrawTeleportSection()
    {
        EditorGUILayout.LabelField("Teleport", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                Refresh();
            EditorGUILayout.LabelField(
                sceneDatabase == null ? "SceneDatabase: not found" : $"{roomDocs.Length} room(s)",
                EditorStyles.miniLabel);
        }

        if (sceneDatabase == null)
        {
            EditorGUILayout.HelpBox("SceneDatabase not found in the loaded scenes (expected on the 'FixedObject' GameObject).", MessageType.Warning);
            return;
        }

        if (teleportation == null)
        {
            EditorGUILayout.HelpBox("Teleportation component not found in the loaded scenes.", MessageType.Warning);
            return;
        }

        if (roomDocs.Length == 0)
        {
            EditorGUILayout.HelpBox("No rooms registered in the SceneDatabase.", MessageType.Info);
            return;
        }

        selectedRoomIndex = Mathf.Clamp(selectedRoomIndex, 0, roomDocs.Length - 1);
        SceneDocument selected = roomDocs[selectedRoomIndex];
        bool hasSpawn = !string.IsNullOrEmpty(selected.PlayerSpawnGameObjectName);

        using (new EditorGUILayout.HorizontalScope())
        {
            selectedRoomIndex = EditorGUILayout.Popup(selectedRoomIndex, roomLabels);

            using (new EditorGUI.DisabledScope(!hasSpawn))
            {
                if (GUILayout.Button("Teleport", GUILayout.Width(90)))
                    Teleport(selected);
            }
        }

        if (!hasSpawn)
            EditorGUILayout.HelpBox("This room has no PlayerSpawnGameObjectName, so it can't be a teleport destination.", MessageType.Warning);
    }

    // Same code path the configuration cabinet uses: hand the SceneDocument to
    // the Teleportation shim, which forwards to TeleportationController on
    // "FixedObject" (additive load -> move OVRPlayerControllerGalery to the
    // spawn point -> unload old scenes).
    private void Teleport(SceneDocument destination)
    {
        Debug.Log($"[AoJControlPanel] Teleport to '{BuildLabel(destination)}'.");
        teleportation.Teleport(destination);
    }

    // ======================================================================
    //  Player
    // ======================================================================

    private void DrawPlayerSection()
    {
        EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);

        if (playerCC == null)
            AcquirePlayer();

        if (playerCC == null)
        {
            EditorGUILayout.HelpBox("Player CharacterController not found (OVRPlayerControllerGalery).", MessageType.Info);
            return;
        }

        float heightScale = Mathf.Abs(playerCC.transform.lossyScale.y);

        EditorGUI.BeginChangeCheck();
        float newStep = EditorGUILayout.Slider("Step-up height (m)", playerCC.stepOffset, 0f, 1f);
        float newSlope = EditorGUILayout.Slider("Slope limit (deg)", playerCC.slopeLimit, 0f, 90f);
        float newRadius = EditorGUILayout.Slider("Radius (m)", playerCC.radius, 0.01f, 0.5f);
        float newSkin = EditorGUILayout.Slider("Skin width (m)", playerCC.skinWidth, 0.001f, 0.2f);
        if (EditorGUI.EndChangeCheck())
        {
            playerCC.stepOffset = newStep;
            playerCC.slopeLimit = newSlope;
            playerCC.radius = newRadius;
            playerCC.skinWidth = newSkin;
        }

        float stepWorld = playerCC.stepOffset * heightScale;
        EditorGUILayout.LabelField(
            $"grounded: {playerCC.isGrounded}    lastMove: {playerCC.collisionFlags}    step(scaled): {stepWorld:0.###} m    height {playerCC.height:0.##}    center.y {playerCC.center.y:0.###}",
            EditorStyles.miniLabel);

        // Passive floor probe: raycast straight down from the capsule base to the
        // first solid surface (skipping the player's own colliders). Distinguishes
        // "floating above the floor" from "resting but not registering contact".
        EditorGUILayout.LabelField(MeasureFloorGap(), EditorStyles.miniLabel);

        // Unity: skin width should be ~10% of radius. Bigger than the radius is
        // pathological — it breaks step-over and snags the capsule on edges.
        if (playerCC.skinWidth >= playerCC.radius)
            EditorGUILayout.HelpBox(
                $"Skin Width ({playerCC.skinWidth:0.###}) ≥ Radius ({playerCC.radius:0.###}). This breaks step-over and catches the capsule on small steps/edges. Try Skin ≈ {(playerCC.radius * 0.1f):0.###}, or a larger Radius.",
                MessageType.Warning);

        // Step-offset only lifts the controller over obstacles while grounded.
        if (!playerCC.isGrounded)
            EditorGUILayout.HelpBox(
                "Player is NOT grounded — Unity only applies step-offset while the capsule rests on the floor, so steps will block until it's grounded.",
                MessageType.Warning);

        EditorGUILayout.HelpBox("Runtime only — changes here are NOT saved to the scene. Edit FixedScene's CharacterController to persist.", MessageType.None);
    }

    // Downward raycast from the capsule base; returns the gap to the first solid
    // surface below and its name. Purely a measurement — changes no state.
    private string MeasureFloorGap()
    {
        Transform ct = playerCC.transform;
        Vector3 cscale = ct.lossyScale;
        float rScale = Mathf.Max(Mathf.Abs(cscale.x), Mathf.Abs(cscale.z));
        float hScale = Mathf.Abs(cscale.y);
        float cBaseRadius = playerCC.radius * rScale;
        float cHeight = Mathf.Max(playerCC.height * hScale, cBaseRadius * 2f);
        Vector3 cUp = ct.up;
        Vector3 feet = ct.TransformPoint(playerCC.center) - cUp * (cHeight * 0.5f);

        // Start a touch above the base so a slight floor penetration still hits.
        const float startUp = 0.1f;
        Vector3 origin = feet + cUp * startUp;
        int hits = Physics.RaycastNonAlloc(origin, -cUp, floorRayBuffer, startUp + 5f,
                                           Physics.AllLayers, QueryTriggerInteraction.Ignore);

        float bestDist = float.PositiveInfinity;
        string bestName = null;
        for (int i = 0; i < hits; i++)
        {
            if (selfColliders.Contains(floorRayBuffer[i].collider))
                continue;
            if (floorRayBuffer[i].distance < bestDist)
            {
                bestDist = floorRayBuffer[i].distance;
                bestName = floorRayBuffer[i].collider.gameObject.name;
            }
        }

        if (bestName == null)
            return "floor gap: (nothing solid within 5 m below)";

        float gap = bestDist - startUp;
        return $"floor gap: {gap:0.###} m    below: {bestName}";
    }

    // ======================================================================
    //  Show Colliding Objects
    // ======================================================================

    // Subscribes/unsubscribes the per-tick poller so it only runs while the
    // toggle is on and we are in Play mode — off means literally zero cost.
    private void SyncUpdateHook()
    {
        bool shouldHook = Application.isPlaying && showColliders;
        if (shouldHook && !updateHooked)
        {
            EditorApplication.update += OnEditorUpdate;
            updateHooked = true;
        }
        else if (!shouldHook && updateHooked)
        {
            EditorApplication.update -= OnEditorUpdate;
            updateHooked = false;
        }
    }

    private void OnEditorUpdate()
    {
        if (!Application.isPlaying || !showColliders)
            return;

        // While paused, physics is frozen so the last poll is still accurate —
        // don't re-query (keeps the frozen list stable so you can pick a row).
        if (EditorApplication.isPaused)
            return;

        double now = EditorApplication.timeSinceStartup;
        double interval = pollHz > 0f ? 1.0 / pollHz : 0.0;
        if (now - lastPollTime < interval)
            return;
        lastPollTime = now;

        PollCollisions();
        Repaint();
    }

    private void AcquirePlayer()
    {
        playerObject = GameObject.Find("OVRPlayerControllerGalery");
        playerCC = playerObject != null ? playerObject.GetComponentInChildren<CharacterController>() : null;

        selfColliders.Clear();
        if (playerObject != null)
        {
            foreach (Collider col in playerObject.GetComponentsInChildren<Collider>(true))
                selfColliders.Add(col);
        }
    }

    // Overlaps the player's CharacterController capsule (+margin) against the
    // physics scene and records the distinct GameObjects it touches. Catches
    // both solid colliders and trigger volumes so invisible triggers show up.
    private void PollCollisions()
    {
        if (playerObject == null || playerCC == null)
            AcquirePlayer();

        if (playerObject == null)
        {
            collisionStatus = "Player 'OVRPlayerControllerGalery' not found.";
            collisionEntries.Clear();
            return;
        }
        if (playerCC == null)
        {
            collisionStatus = "No CharacterController on the player.";
            collisionEntries.Clear();
            return;
        }

        Transform t = playerCC.transform;
        Vector3 scale = t.lossyScale;
        float radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
        float heightScale = Mathf.Abs(scale.y);

        // Build the capsule's core segment from the UNINFLATED radius, then add
        // the probe reach to the query radius only. Inflating the radius while
        // shrinking the segment (the previous approach) left the top/bottom tips
        // in place, so it only fattened the sides and never reached the floor you
        // stand on or the ceiling. Growing the query radius expands the capsule
        // uniformly in every direction instead.
        float baseRadius = playerCC.radius * radiusScale;
        float height = Mathf.Max(playerCC.height * heightScale, baseRadius * 2f);
        float halfCyl = Mathf.Max(0f, height * 0.5f - baseRadius);
        Vector3 center = t.TransformPoint(playerCC.center);
        Vector3 up = t.up;
        Vector3 p0 = center + up * halfCyl;
        Vector3 p1 = center - up * halfCyl;

        // A CharacterController rests a hair off surfaces (skinWidth + the
        // physics contact offset), so the exact capsule never overlaps the floor
        // it stands on or the wall it's pressed against. Bridge that gap
        // automatically, then add the user's margin on top.
        float radius = baseRadius + playerCC.skinWidth * radiusScale + colliderMargin;

        QueryTriggerInteraction qti = includeTriggers
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        int count = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, overlapBuffer, Physics.AllLayers, qti);
        // Grow the buffer if we filled it, so nothing is missed.
        while (count == overlapBuffer.Length && overlapBuffer.Length < 4096)
        {
            overlapBuffer = new Collider[overlapBuffer.Length * 2];
            count = Physics.OverlapCapsuleNonAlloc(p0, p1, radius, overlapBuffer, Physics.AllLayers, qti);
        }

        // Player's feet (capsule base) and the scaled step-up height, so each
        // hit can be classified as walked-over vs. blocking.
        float feetY = (center - up * (height * 0.5f)).y;
        float stepWorld = playerCC.stepOffset * heightScale;

        collisionEntries.Clear();
        seenThisPoll.Clear();
        for (int i = 0; i < count; i++)
        {
            Collider c = overlapBuffer[i];
            if (c == null || selfColliders.Contains(c))
                continue;

            GameObject go = c.gameObject;
            if (!seenThisPoll.Add(go))
                continue;

            string layerName = LayerMask.LayerToName(go.layer);
            if (string.IsNullOrEmpty(layerName))
                layerName = go.layer.ToString();

            string trig = c.isTrigger ? " [trigger]" : "";

            // How high the collider's top sits above the player's feet decides
            // whether the CharacterController steps over it or is blocked. Uses
            // the world AABB, so it's a heuristic (not Unity's exact step algo),
            // but it flags the low colliders that cause "invisible" bumps.
            string stepTag = "";
            if (!c.isTrigger)
            {
                float overFeet = c.bounds.max.y - feetY;
                if (overFeet <= 0.02f)
                    stepTag = " [floor/below]";
                else if (overFeet <= stepWorld)
                    stepTag = $" [steppable +{overFeet:0.00}m]";
                else
                    stepTag = $" [blocks +{overFeet:0.00}m]";
            }

            // Full hierarchy path so identically-named objects ("Cube") are
            // distinguishable at a glance, plus the layer/trigger/step tags.
            collisionEntries.Add(new CollisionEntry
            {
                label = $"{HierarchyPath(go)}   (layer: {layerName}){trig}{stepTag}",
                go = go,
            });
        }
        collisionEntries.Sort((a, b) => string.Compare(a.label, b.label, System.StringComparison.OrdinalIgnoreCase));
        collisionStatus = "";
    }

    private void DrawCollidersSection()
    {
        EditorGUILayout.LabelField("Show Colliding Objects", EditorStyles.boldLabel);

        bool newToggle = EditorGUILayout.ToggleLeft(
            "Enabled  (polls physics each tick — heavier on CPU)", showColliders);
        if (newToggle != showColliders)
        {
            showColliders = newToggle;
            if (showColliders)
                AcquirePlayer();
            else
                collisionEntries.Clear();
            SyncUpdateHook();
        }

        if (!showColliders)
            return;

        using (new EditorGUI.IndentLevelScope())
        {
            includeTriggers = EditorGUILayout.Toggle("Include triggers", includeTriggers);
            colliderMargin = EditorGUILayout.Slider("Probe margin (m)", colliderMargin, 0f, 0.5f);
            pollHz = EditorGUILayout.Slider("Poll rate (Hz)", pollHz, 1f, 60f);
        }

        EditorGUILayout.Space();

        // Freeze the sim so the list stops flickering and you can pick a row.
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(EditorApplication.isPaused ? "Resume" : "Pause", GUILayout.Width(90)))
                EditorApplication.isPaused = !EditorApplication.isPaused;
            EditorGUILayout.LabelField(
                EditorApplication.isPaused ? "Paused — click Select to jump to an object" : "Running",
                EditorStyles.miniLabel);
        }

        EditorGUILayout.Space();

        if (!string.IsNullOrEmpty(collisionStatus))
        {
            EditorGUILayout.HelpBox(collisionStatus, MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField(
            collisionEntries.Count == 0 ? "Touching: nothing"
                                        : $"Touching {collisionEntries.Count} object(s):",
            EditorStyles.miniBoldLabel);

        collisionScroll = EditorGUILayout.BeginScrollView(collisionScroll, GUILayout.MinHeight(80));
        foreach (CollisionEntry entry in collisionEntries)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(entry.label);
                using (new EditorGUI.DisabledScope(entry.go == null))
                {
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = entry.go;
                        EditorGUIUtility.PingObject(entry.go);
                        SceneView.FrameLastActiveSceneView();
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    // Full "/"-separated path from the scene root, so identically-named objects
    // are told apart at a glance.
    private static string HierarchyPath(GameObject go)
    {
        Transform tr = go.transform;
        string path = tr.name;
        while (tr.parent != null)
        {
            tr = tr.parent;
            path = tr.name + "/" + path;
        }
        return path;
    }
}
#endif
