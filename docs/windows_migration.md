# Age of Joy: Windows Migration Strategy

Porting a VR-native application like *Age of Joy* to a flat Windows Desktop experience is a profound architectural shift. It requires translating a **physical interaction paradigm** (6DOF spatial movement, grabbing, pushing) into a **symbolic interaction paradigm** (mouse and keyboard, raycasts, button presses).

Here is the strategic analysis of how to achieve a fully functioning Windows build, broken down by the core challenges.

---

## Phase 1: The Emulator Section (Libretro Wrapper)
Libretro cores are compiled C/C++ libraries. The project currently uses an Android Shared Object (`.so` file, e.g., `mame2003_plus_libretro_android.so`). Windows cannot read `.so` files; it requires a Dynamic Link Library (`.dll`).

**The Strategy:**
1.  **Acquire the Windows Core:** You do not need to rewrite the emulator. You just need the Windows equivalent of the core you are using. Download the pre-compiled `mame2003_plus_libretro.dll` (for Windows x64) directly from the official Libretro buildbot.
2.  **Unity Plugin Routing:** 
    *   Place the `.so` file in `Assets/Plugins/Android/` and check "Android" in the Unity Inspector's Plugin settings.
    *   Place the `.dll` file in `Assets/Plugins/x86_64/` and check "Standalone" and "Windows".
    *   Ensure `[DllImport("retro")]` calls in `LibretroMameCore.cs` do not hardcode the `.so` extension. Unity will automatically route the `DllImport` to the `.dll` on Windows and the `.so` on Android.
3.  **The Graphics Bridge:** The `libVulkanPlugin` custom C++ Vulkan plugin (used to bridge texture memory between Libretro and Unity) **must be recompiled for Windows as a `.dll`** using Visual Studio or MinGW. If it's pure Vulkan, the code is highly portable, but the build toolchain needs to target Windows.

---

## Phase 2: Interaction Paradigm (Coins, Lightguns, and Cabs)
In VR, a user physically grabs a coin and pushes it into a slot. On Windows, this physical simulation must be replaced by a standard First-Person Shooter (FPS) interaction model.

**The Strategy: Raycast Abstraction**
1.  **The Player Controller:** Swap the `OVRPlayerController` for a standard `CharacterController` (WASD movement, Mouse look). 
2.  **The Crosshair & Raycaster:** Place a small dot in the center of the UI. Cast a `Physics.Raycast` from the center of the camera forward.
3.  **Abstracting the Actions:**
    *   **Inserting a Coin:** When the raycast hits a `CoinSlotController`'s collider, show a UI prompt: *"Press [E] to Insert Coin"*. When 'E' is pressed, bypass the physical coin logic and directly call the C# method `CoinSlotController.insertCoin()`.
    *   **Playing a Game:** When the raycast hits the arcade screen, show *"Press [Enter] to Play"*. When pressed:
        *   Disable WASD movement.
        *   Lock the mouse cursor to the center of the screen (or allow it to move within the screen bounds for Lightgun games).
        *   Switch the Action Map to the Libretro controls so keyboard presses are routed to the emulator.
    *   **Lightguns:** In VR, you aim a 3D gun model. On Windows, the Lightgun is simply the Mouse. You map Mouse X/Y to the Libretro Lightgun axes, and Left Click to the trigger.
    *   **Exiting:** Map the `ESC` key to trigger the `ExitGame()` routine, returning the player to WASD walking mode.

---

## Phase 3: Decoupling Meta/Android SDKs
The project is heavily intertwined with `com.meta.xr.*` packages. If Unity tries to compile Windows code that references `OVRInput` or `OVRManager`, the build might fail, or it will throw runtime errors because no headset is connected.

**The Strategy: Preprocessor Directives & Interfaces**
All XR-specific code must be isolated.

1.  **Code Wrappers:** Any file that references Meta packages must be wrapped in platform directives, or use Unity's generic XR system where possible.
    ```csharp
    #if UNITY_ANDROID
        OVRPlugin.SystemHeadset headsetType = OVRPlugin.GetSystemHeadsetType();
    #else
        // Windows fallback logic
    #endif
    ```
2.  **Input Abstraction:** Since the project uses `UnityEngine.InputSystem`, create a new Input Scheme in the Action Asset specifically for "Desktop Player" (WASD + Mouse) alongside the existing "XR Player" scheme. 

---

## Phase 4: AGEBasic Compatibility
The AGEBasic language currently relies on physical VR events (e.g., `on-collision-start` for coins hitting slots, or `on-touch-start`).

**The Strategy: Event Mapping**
Do not rewrite every `description.yaml` in the database. Instead, the Windows engine should "fake" these physical events.
*   If an AGEBasic script listens for `on-touch-start` on a button, and the Windows user clicks that button with their mouse, the Desktop Raycast script should trigger the `grabDetection.OnPlayerTouchEnter` event programmatically. 
*   This way, the AGEBasic script has no idea it is running on Windows; it just knows the event fired, ensuring 100% logic compatibility across both platforms.

---

## Recommended Order of Operations

1.  **Step 1 (The Foundation):** Create the generic Player Rig. Build a simple Desktop First Person controller that is spawned conditionally if `#if UNITY_STANDALONE_WIN` is true, and disables the `OVRPlayerControllerGalery`.
2.  **Step 2 (The Core):** Source the `mame2003_plus` Windows `.dll` and any custom graphic bridge DLLs, put them in `Assets/Plugins/x86_64`, and ensure the game doesn't crash on startup in the Unity Editor (which runs on Windows).
3.  **Step 3 (Interactions):** Write the `DesktopInteractionCaster` script to send clicks to the Cabinet's arcade buttons and coin slots using Raycasts.
4.  **Step 4 (Input):** Route Keyboard/Gamepad inputs into the existing `LibretroControlMap`.