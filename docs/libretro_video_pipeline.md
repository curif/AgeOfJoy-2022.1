# Libretro Video Pipeline

How a frame travels from a libretro core to a Unity `Texture2D` on a cabinet's screen mesh.

> **Status: software rendering only.** Libretro cores that render in software (mame2003-plus, fbneo for most cores, neogeo, etc.) work. Cores that require a hardware GL or Vulkan context (Mupen64, Beetle PSX HW, FFmpeg, etc.) currently produce a black screen — the hardware paths are partially wired but the texture upload side is a stub. See §9.

---

## 1. Pipeline at a glance

There is **one global static `Texture2D`** ([`LibretroMameCore.GameTexture`](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L227)) in `TextureFormat.RGB565`. The entire pipeline's job is to fill it once per emulator frame and let the cabinet's screen shader sample it.

```
┌─────────────────────── Worker thread (Task.Run) ──────────────────────┐
│                                                                       │
│  wrapper_run()    ← runs at core's FPS                                │
│      │                                                                │
│      ▼                                                                │
│  core emulates one frame                                              │
│      │                                                                │
│      ▼                                                                │
│  core calls retro_video_refresh(data, w, h, pitch)                    │
│      │                                                                │
│      ▼                                                                │
│  wrapper_image_video_refresh_cb  (image.c)                            │
│      ├─ data == NULL                  → return (frame dupe)           │
│      ├─ data == HW_FRAME_BUFFER_VALID → log + return (stub, §9)       │
│      ├─ dimensions changed            → CreateTextureCB(w, h)         │
│      ├─ switch (pixel_format)                                         │
│      │    ├─ RGB565    → memcpy / row-pitch copy                      │
│      │    ├─ 0RGB1555  → bit-shift to RGB565                          │
│      │    └─ XRGB8888  → bit-shift to RGB565                          │
│      ├─ computeAverageRGB565 → light_red/green/blue (for glow, §8)    │
│      └─ swapBuffers: bufIdx flip + GameTextureBufferSem.Set()         │
└───────────────────────────────────────────────────────────────────────┘
                                  │
                                  │  (atomic handoff via mutex + semaphore)
                                  ▼
┌───────────────────────── Main thread (Update) ────────────────────────┐
│                                                                       │
│  LibretroScreenController.Update()                                    │
│      │                                                                │
│      ▼                                                                │
│  LibretroMameCore.UpdateTexture()                                     │
│      └─ wrapper_is_hardware_rendering() ? LoadVulkanTextureData (stub)│
│                                         : LoadTextureData             │
│                                                │                      │
│                                                ▼                      │
│  LoadTextureData:                                                     │
│      ├─ Monitor.Enter(GameTextureLock)                                │
│      ├─ if GameTextureBufferSem.Wait(0):                              │
│      │    ├─ wrapper_image_get_buffer() → IntPtr                      │
│      │    ├─ InitializeTexture()  ← if RecreateTexture                │
│      │    ├─ GameTexture.LoadRawTextureData(ptr, size)                │
│      │    ├─ GameTexture.Apply(false, false)                          │
│      │    └─ GameTextureBufferSem.Reset()                             │
│      └─ Monitor.Exit                                                  │
│                                                                       │
│  shader.Update()                                                      │
│  (The CRT material's _MainTex was already set to GameTexture          │
│   by shader.Activate(GameTexture) when the player inserted a coin)    │
└───────────────────────────────────────────────────────────────────────┘
```

The texture is *the* synchronization point. Worker thread fills, main thread uploads, GPU samples.

---

## 2. The libretro contract

Every libretro core implements `retro_set_video_refresh(retro_video_refresh_t)` — the frontend hands the core a function pointer, the core calls it once per emulated frame:

```c
typedef void (RETRO_CALLCONV *retro_video_refresh_t)(const void *data, unsigned width,
                                                     unsigned height, size_t pitch);
```

Before any frames arrive, the core declares its pixel format via the environment callback:

```c
case RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
    pixel_format = *(const enum retro_pixel_format*)data;
```

…stored in a static in [`cwrapper/environment.c:6`](../Assets/curif/LibRetroWrapper/cwrapper/environment.c#L6) and read back in `image.c` when each frame arrives. Three formats are supported:

| `retro_pixel_format` | Source bits per pixel | Conversion needed |
|---|---|---|
| `RETRO_PIXEL_FORMAT_RGB565` | 16 | None (preserve only) |
| `RETRO_PIXEL_FORMAT_0RGB1555` | 16 | Yes — pad green channel |
| `RETRO_PIXEL_FORMAT_XRGB8888` | 32 | Yes — quantize to 5/6/5 |

Anything else is `RETRO_PIXEL_FORMAT_UNKNOWN` and frames are dropped.

The `data` argument can be one of three things:

- A pointer to pixel data — the normal case.
- `NULL` — frame dupe (don't repaint, the previous frame stands).
- `RETRO_HW_FRAME_BUFFER_VALID` (sentinel value `-1`) — "I rendered to the hardware framebuffer instead; go pull it from there." See §9.

---

## 3. Setup phase

When a coin is inserted, `LibretroMameCore.Start(screenName, gameFileName, ...)` runs. The video-relevant calls:

```csharp
// 1. Placeholder texture (will be resized once the core declares its dimensions)
GameTexture = new Texture2D(200, 200, TextureFormat.RGB565, false);
GameTexture.filterMode = FilterMode.Bilinear;

// 2. Load the core .so, open the environment, set pixel format
wrapper_environment_open(...);
wrapper_environment_init();

// 3. Register the video callbacks with the C wrapper
wrapper_image_init(new CreateTextureHandler(CreateTextureCB),
                   new TextureLockHandler(TextureLockCB),
                   new TextureUnlockHandler(TextureUnlockCB),
                   new TextureBufferSemAvailableHandler(TextureBufferSemAvailable));
```

`wrapper_image_init` on the C side stores the four function pointers as globals, then registers the C-side `wrapper_image_video_refresh_cb` with the core via the cached `retro_set_video_refresh`:

```c
handlers->retro_set_video_refresh(&wrapper_image_video_refresh_cb);
CreateTextureCB = createTexture;
TextureLockCB = textureLock;
TextureUnlockCB = textureUnlock;
TextureSemAvailableCB = textureSemAvailable;
```

The four callbacks are how the C layer reaches back into managed C# code (with `[AOT.MonoPInvokeCallback]` attributes for IL2CPP — required on Android).

---

## 4. The run loop

[`LibretroMameCore.StartRunThread()`](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1090) spawns a `Task.Run` that loops `wrapper_run()` at the core's reported FPS:

```csharp
retroRunTask = Task.Run(() =>
{
    while (!retroRunTaskCancellationToken.IsCancellationRequested)
    {
        FPSControlNoUnity.CountTimeFrame();
        if (FPSControlNoUnity.isTime())
        {
            wrapper_run();              // ← drives one emulator frame
            handleSpecialInputs();
        }
    }
});
```

This is a .NET worker thread — not Unity's main thread. Everything the core does (CPU emulation, video callback, audio callback) happens off-thread, freeing Unity to render VR at 90 Hz. The cost of crossing back to the main thread is the texture upload (§6), gated by a semaphore so the main thread only does work when there's a new frame.

`wrapper_run()` is one `retro_run()` cycle. The core internally:
1. Reads input via the polled input callback.
2. Emulates one frame.
3. Calls `retro_video_refresh(data, w, h, pitch)` zero or one time (zero if it dupes the frame).
4. Calls `retro_audio_sample_batch(...)` to deliver audio.

---

## 5. The video refresh callback (C side, worker thread)

[`wrapper_image_video_refresh_cb`](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L144) is what runs each time the core produces a frame. The full flow:

### 5a. Early outs

```c
if (!data) return;                                    // frame dupe
if (data == RETRO_HW_FRAME_BUFFER_VALID) {            // HW path — see §9
    wrapper_environment_log(...);
    return;
}
if (!CreateTextureCB) return;                          // not initialized yet
if (no_draw) return;                                   // explicitly suspended
if (pixel_format == RETRO_PIXEL_FORMAT_UNKNOWN) return;
```

### 5b. Dimension change check

If the core's reported width/height differs from the cached `image_width`/`image_height` (or this is the first frame), the texture must be re-allocated:

```c
if (!create_texture_called || image_width != width || image_height != height) {
    create_texture_called = (char)1;
    TextureLockCB();
    CreateTextureCB(width, height);   // ← C# callback
    TextureUnlockCB();
    image_height = height;
    image_width = width;
}
```

The mutex protects against the main thread reading `TextureWidth`/`TextureHeight` while it's being updated. On the C# side, `CreateTextureCB` just stores values and sets a flag — the actual `Texture2D.Reinitialize` happens on the main thread next frame:

```csharp
[AOT.MonoPInvokeCallback(typeof(CreateTextureHandler))]
static void CreateTextureCB(uint width, uint height)
{
    TextureWidth = width;
    TextureHeight = height;
    RecreateTexture = true;     // main thread will pick this up
}
```

This split (worker thread can't touch Unity APIs) is the whole reason for the indirection.

### 5c. Pixel format conversion

Each format has its own converter in [`image_conversion.c`](../Assets/curif/LibRetroWrapper/cwrapper/image_conversion.c). All output goes into one of two output buffers (`outputData[2][1024*768*2]`).

**RGB565 (no conversion needed):**

```c
case RETRO_PIXEL_FORMAT_RGB565: {
    size_t imageSize = width * height * 2;
    if (pitch == width * 2) {
        imageBuf = wrapper_image_preserve(data, imageSize, bufIdx);   // memcpy
    } else {
        imageBuf = storeRGB565Image(data, width, height, pitch, bufIdx);  // row-by-row
    }
    ...
}
```

`wrapper_image_preserve` is a single `memcpy` when pitch equals `width*2`. When the core pads each row (pitch > width\*2), `storeRGB565Image` walks rows individually skipping the padding.

**0RGB1555 → RGB565:**

The source format is `0 RRRRR GGGGG BBBBB` (16 bits, top bit unused, 5 bits per channel). The target is `RRRRR GGGGGG BBBBB` (5/6/5). Green needs a padding bit; the conversion duplicates the green MSB:

```c
unsigned short pixel = *inputRow++;
*outputRow++ = ((pixel & 0x7FE0) << 1) | (pixel & 0x1F);
//                 ^^^^^^                  ^^^^^^
//             rrrrrgggggg shifted left   blue bits, unchanged
```

The mask `0x7FE0` selects bits 14-5 (the 5 red + 5 green bits), shifting them up by 1 puts them in bits 15-6 — exactly where RGB565 wants red (15-11) and green (10-5). Blue stays in bits 4-0.

**XRGB8888 → RGB565:**

The source format is little-endian B G R X (one byte each, X unused). The target packs each channel into fewer bits:

```c
for (int x = 0; x < width; x++) {
    unsigned char r = inputRow[2];
    unsigned char g = inputRow[1];
    unsigned char b = inputRow[0];
    unsigned short rgb565 = ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3);
    *(unsigned short *)outputRow = rgb565;
    inputRow  += 4;
    outputRow += 2;
}
```

The bit math:
- `(r & 0xF8) << 8` — top 5 bits of red → bits 15-11
- `(g & 0xFC) << 3` — top 6 bits of green → bits 10-5
- `b >> 3` — top 5 bits of blue → bits 4-0

This is a lossy step — 24-bit color is quantized to 16-bit. For the CRT aesthetic this codebase targets it's a non-issue; the scanline shader masks it.

### 5d. Buffer swap

Once a buffer is filled:

```c
void swapBuffers(unsigned char* imageBuf, unsigned size) {
    TextureLockCB();
    imageBuffer = imageBuf;
    imageSize   = size;
    bufIdx = (bufIdx + 1) % 2;       // ← next write goes to the other buffer
    TextureSemAvailableCB();         // ← signal C# that a new frame is ready
    TextureUnlockCB();
}
```

Three things happen under the mutex:
1. The published pointer (`imageBuffer`) is updated — main thread reads this.
2. `bufIdx` flips so the **next** frame writes to the **other** buffer. This way the worker can begin filling buffer N+1 while the main thread is still reading buffer N. No tearing.
3. The semaphore is set, telling the main thread "there's a new frame."

`TextureSemAvailableCB` on the C# side is one line:

```csharp
[AOT.MonoPInvokeCallback(typeof(TextureBufferSemAvailableHandler))]
public static void TextureBufferSemAvailable() => GameTextureBufferSem.Set();
```

`GameTextureBufferSem` is a `ManualResetEventSlim` — set by the worker, reset by the main thread after consumption.

---

## 6. The texture upload (main thread)

[`LibretroScreenController.Update()`](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs#L635) is called by Unity every frame. When a game is running:

```csharp
if (LibretroMameCore.isRunning(ScreenName, GameFile))
{
    // ... compute screen-glow color (see §8) ...
    LibretroMameCore.UpdateTexture();
}
shader.Update();
```

[`UpdateTexture()`](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L985) dispatches by render path:

```csharp
public static void UpdateTexture()
{
#if !UNITY_EDITOR
    if (wrapper_is_hardware_rendering())
        LoadVulkanTextureData();    // currently a stub — see §9
    else
        LoadTextureData();
#endif
}
```

[`LoadTextureData()`](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1023) is the working software path:

```csharp
public static void LoadTextureData()
{
    lock (GameTextureLock)
    {
        if (GameTextureBufferSem.Wait(0))    // is there a new frame?
        {
            IntPtr data = wrapper_image_get_buffer();
            int size    = wrapper_image_get_buffer_size();
            if (data != IntPtr.Zero)
            {
                InitializeTexture();                          // resize if needed
                GameTexture.LoadRawTextureData(data, size);   // ← native copy into Texture2D
                GameTexture.Apply(false, false);              // ← push to GPU
            }
            GameTextureBufferSem.Reset();
        }
    }
}
```

Key details:

- **`Wait(0)`** is non-blocking — it returns `true` once if the semaphore is set, `false` otherwise. If the main thread is faster than the core, it returns `false` most ticks (nothing to do). If the core is faster, frames get coalesced (the semaphore is binary — multiple `Set()` calls collapse to one).
- **`LoadRawTextureData(IntPtr, int)`** copies bytes directly from the C heap into the texture's native memory without involving managed allocations. This is the only Unity API path that lets you upload pre-formatted bytes to a `Texture2D` in one step.
- **`Apply(false, false)`** — first arg `false` skips mipmap regeneration (CRT screens don't mip), second arg `false` keeps the CPU side readable (we'll overwrite it again next frame).
- The whole block is inside `lock (GameTextureLock)` so the worker can't be mid-`swapBuffers` while we're reading the pointer. (The semaphore tells us *that* a frame is ready, the mutex protects *which* buffer.)

[`InitializeTexture`](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L968) handles the resize case:

```csharp
public static bool InitializeTexture()
{
    if (TextureWidth != 0 && RecreateTexture)
    {
        GameTexture.Reinitialize((int)TextureWidth, (int)TextureHeight);
        ResetTextureData();                    // black out the texture
        RecreateTexture = false;
        Shader.Refresh(GameTexture);           // material re-binds to the new texture instance
        Shader.ApplyConfiguration();
        return true;
    }
    return false;
}
```

`Texture2D.Reinitialize` is in-place — same C# object, new dimensions on the GPU. But it produces a *new texture handle*, which means any material that referenced the old handle now points at garbage. `Shader.Refresh` is what rebinds the material's `_MainTex` to the reinitialized texture.

---

## 7. Material binding

The screen mesh's `Renderer` has two material slots. Slot 1 is the screen surface. When the player inserts a coin and the game starts running, [`LibretroScreenController.cs:447`](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs#L447) does:

```csharp
shader.Activate(LibretroMameCore.GameTexture);
shader.Invert(GameInvertX, GameInvertY);
```

The `shader` here is a `ShaderScreenBase` produced earlier by `ShaderScreen.Factory(...)` — a `ShaderCRT`, `ShaderProjector`, etc., depending on `crt.screen.shader` in the cabinet YAML.

[`ShaderScreenBase.Activate`](../Assets/curif/LibRetroWrapper/ShaderScreen.cs#L52) swaps the material at slot 1 and binds the texture:

```csharp
Material[] mats = display.materials;
mats[position] = material;            // CRT material (ScreenCRTHigh.mat, etc.)
display.materials = mats;
material = display.materials[position];
if (texture != null)
    Texture = texture;                // sets material._MainTex = GameTexture
```

After this, the CRT material's `_MainTex` *is* `GameTexture`. Any subsequent `LoadRawTextureData` + `Apply` in `LoadTextureData` (§6) is immediately visible because the GPU samples whatever the texture handle currently holds.

`ShaderCRT.Texture` (the setter):

```csharp
set {
    Texture t = (Texture)value;
    Vector4 crtParameters = new Vector4(t.width, t.height, 0f, 0f);
    material.SetTexture("_MainTex", t);
    material.SetVector("_CRTParameters", crtParameters);
    if (v4Invert != null) material.SetVector("_CRTTiling", (Vector4)v4Invert);
}
```

The `_CRTParameters` vector tells the scanline shader the source resolution so the scanline density matches the emulated frame.

See [docs/screentype_system.md](screentype_system.md) for how the shader and screen prefab are chosen from the cabinet's YAML.

---

## 8. The screen-glow side channel

Each frame, just before `swapBuffers`, the worker thread runs [`computeAverageRGB565`](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L102):

```c
void computeAverageRGB565(const unsigned short* imageBuf, int width, int height) {
    int primeStrides[] = { 17, 19, 23, 29 };
    int strideY = 23;
    for (int y = 0; y < height; y += strideY) {
        int strideX = primeStrides[y % numPrimes];
        for (int x = 0; x < width; x += strideX) {
            unsigned short pixel = imageBuf[y * width + x];
            totalRed   += (pixel >> 11) & 0x1F;
            totalGreen += (pixel >> 5)  & 0x3F;
            totalBlue  +=  pixel        & 0x1F;
            sampleCount++;
        }
    }
    light_red   = (totalRed   / sampleCount) / 31.0f;   // 0..1
    light_green = (totalGreen / sampleCount) / 63.0f;
    light_blue  = (totalBlue  / sampleCount) / 31.0f;
}
```

The prime strides on both axes deliberately avoid aliasing with screen patterns (regular grids, scanlines). Roughly `(width * height) / (23 * ~22) ≈ 0.2%` of pixels are sampled — cheap.

The three floats are exposed via [`wrapper_image_get_light_red()`](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L27) etc. and consumed by `LibretroScreenController.Update`:

```csharp
if (screenLightON)
{
    r = LibretroMameCore.getLightRed();
    g = LibretroMameCore.getLightGreen();
    b = LibretroMameCore.getLightBlue();
    screenGlowLight.color = new Color(r, g, b);
    float luminance = (r + g + b) / 3f;
    screenGlowLight.intensity = luminance * globalConfiguration.Configuration.cabinet.screenGlowIntensity;
}
```

`screenGlowLight` is a Unity `Light` parented to the screen prefab — the result is that a cabinet displaying mostly-blue content casts a blue glow on nearby surfaces, mostly-red content casts red, etc. The intensity scales with global `screenGlowIntensity` so it can be turned off entirely. See [docs/roomlight_system.md](roomlight_system.md) for the broader lighting system; this is a per-cabinet effect distinct from the global mood light.

---

## 9. Hardware rendering status & roadmap to Flycast/NAOMI

**Status:** partially wired. Negotiation works, device creation works, the core's `VkImage` is being delivered to C# land each frame. What's missing is (a) a handful of synchronization callbacks that are currently no-ops, (b) the C# code that wraps the `VkImage` as a Unity `Texture2D`, and (c) a couple of small accessors so the C# side knows the image's dimensions and format. This section maps the current state in detail and outlines a path to running Dreamcast/NAOMI through Flycast — the primary target.

### 9.1 Three-layer architecture

```
┌──────────────────────────────────── Unity render thread ─────────────────────────────────┐
│                                                                                          │
│  IUnityGraphicsVulkan                                                                    │
│       │                                                                                  │
│       ▼                                                                                  │
│  libVulkanPlugin.so  (libVulkanPlugin/source/VulkanPlugin.cpp)                           │
│       UnityPluginLoad: cache UnityVulkanInstance (instance, physDevice, device,          │
│                                                  graphicsQueue, queueFamilyIndex,        │
│                                                  getInstanceProcAddr, pipelineCache)     │
│       7 extern "C" getters:                                                              │
│         GetVkInstance / GetVkPhysicalDevice / GetVkDevice / GetVkQueue                   │
│         GetQueueFamilyIndex / GetPFNvkGetInstanceProcAddr / GetVkPipelineCache           │
│       │                                                                                  │
│       ▼ DllImport                                                                        │
│  UnityVulkan.cs MonoBehaviour                                                            │
│       Start(): pulls all 7 handles, then calls                                           │
│       LibretroVulkan.Init(vkInstance, vkPhysicalDevice, vkDevice, pfn)                   │
│       │                                                                                  │
│       ▼                                                                                  │
│  LibretroVulkan.cs  (C# static)                                                          │
│       WrapperInit() → wrapper_init_vulkan(...) in C                                      │
│       VulcanImageCB(vkImage) ← called from C side when set_image fires                   │
│                                                                                          │
└──────────────────────────────────────────┬───────────────────────────────────────────────┘
                                           │ (C# stores Unity Vulkan handles in C globals)
                                           ▼
┌─────────────────────────────────────── Core / worker thread ─────────────────────────────┐
│                                                                                          │
│  cwrapper/vulkan.c                                                                       │
│      RETRO_ENVIRONMENT_GET_PREFERRED_HW_RENDER       → RETRO_HW_CONTEXT_VULKAN ✅        │
│      RETRO_ENVIRONMENT_SET_HW_RENDER                 → cache callback, flip flag ✅      │
│      RETRO_ENVIRONMENT_SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE                       │
│           dlopen libvulkan.so → resolve vkCreateInstance/                                │
│              vkGetInstanceProcAddr/vkGetDeviceProcAddr                                   │
│           call hw_vulkan_interface.create_device(                                        │
│              context, Unity's vkInstance, Unity's vkPhysicalDevice, VK_NULL_HANDLE,      │
│              vkGetInstanceProcAddr, "VK_KHR_swapchain", 0, NULL, 0, &requiredFeatures)   │
│           call hw_render_callback.context_reset()  ← core builds its renderer ✅         │
│      RETRO_ENVIRONMENT_GET_HW_RENDER_INTERFACE                                           │
│           returns retro_hw_render_interface_vulkan populated with                        │
│             Unity's instance + core-created device/queue + 8 callbacks ✅                │
│                                                                                          │
│  Per frame: core calls set_image(image_view, image_layout, create_info, ...) ✅          │
│      vulkan_set_image stashes image; calls vulcanImageCB(create_info.image)              │
│      → hops to C#, sets vkImage + vkImageReady = true                                    │
│                                                                                          │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

**What this means:** when a Vulkan core is loaded, the core's `retro_run` is producing real frames into a `VkImage` allocated on a `VkDevice` that was created with Unity's `VkInstance` and `VkPhysicalDevice`. The C# layer already holds the `VkImage` handle. The remaining work is (a) keep the core and Unity from racing on the queue, and (b) wrap that `VkImage` as a `Texture2D` for the screen shader.

### 9.2 The negotiation that works

When a HW-context core boots, it makes four environment calls. The current code handles each:

| libretro env call | Where | What we do |
|---|---|---|
| `GET_PREFERRED_HW_RENDER` | [environment.c:629](../Assets/curif/LibRetroWrapper/cwrapper/environment.c#L629) | Answer `RETRO_HW_CONTEXT_VULKAN`. Steers GL/Vulkan dual-mode cores like Flycast to Vulkan. |
| `SET_HW_RENDER` | [environment.c:636](../Assets/curif/LibRetroWrapper/cwrapper/environment.c#L636) | Copy `retro_hw_render_callback` into a static; set `hardware_rendering = true` (visible to C# as `wrapper_is_hardware_rendering()`). |
| `SET_HW_RENDER_CONTEXT_NEGOTIATION_INTERFACE` | [environment.c:644](../Assets/curif/LibRetroWrapper/cwrapper/environment.c#L644) | dlopen libvulkan.so, resolve three function pointers, call the core's `create_device` with Unity's instance + physical device. Then call `hw_render_callback.context_reset()` to let the core initialize its renderer. |
| `GET_HW_RENDER_INTERFACE` | [environment.c:651](../Assets/curif/LibRetroWrapper/cwrapper/environment.c#L651) | Return `&hw_render_interface` populated with Unity's instance, the core-created device/queue, and 8 callbacks. |

The `create_device` call is the most interesting one ([vulkan.c:264](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L264)):

```c
bool result = hw_vulkan_interface.create_device(
    &context,                          // OUT: VkPhysicalDevice/VkDevice/VkQueue+index/presentationQueue
    vkInstance,                        // IN: Unity's VkInstance
    vkPhysicalDevice,                  // IN: Unity's VkPhysicalDevice (the core MUST honor this)
    VK_NULL_HANDLE,                    // surface — none, we don't present
    vkFunctions.vkGetInstanceProcAddr,
    &required_device_extensions,       // "VK_KHR_swapchain"
    0,                                 // (the count is 0, see warning below)
    NULL, 0,
    &required_features                 // zeroed, no specific features required
);
```

The libretro contract for `create_device` says: *"If gpu is not VK_NULL_HANDLE, the physical device provided to the frontend must be this PhysicalDevice."* So the core is required to build its `VkDevice` on Unity's GPU. This is the load-bearing detail that makes the rest possible — see §9.5 for what happens if that contract is violated.

> **Bug in current code:** `required_device_extensions` is declared as a single `char*` and passed as `&required_device_extensions` (a `char**`), but the extension count is hardcoded to `0`. So the extension isn't actually being requested. For Flycast this is probably fine — it doesn't need swapchain — but it's worth fixing.

### 9.3 The image hand-off that works

Once the core's renderer is up, every frame goes through this path:

1. Core renders into its own `VkImage` (allocated via the negotiated `VkDevice`).
2. Core calls `hw_render_interface_vulkan.set_image(handle, image, semaphores, src_queue_family)` — that's our `vulkan_set_image` in [vulkan.c:35](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L35).
3. `vulkan_set_image` stashes `image_view`, `image_layout`, `create_info`, semaphore array, src queue family.
4. It calls `vulcanImageCB(create_info.image)` — a function pointer registered by C# during `wrapper_init_vulkan`.
5. On the C# side, `LibretroVulkan.VulcanImageCB(IntPtr vkImage)` ([LibretroVulkan.cs:60](../Assets/curif/LibRetroWrapper/LibretroVulkan.cs#L60)) sets `vkImage` and `vkImageReady = true`.
6. Core calls `retro_video_refresh_t(RETRO_HW_FRAME_BUFFER_VALID, w, h, 0)` to say "the image you got via set_image is the frame for this run cycle."
7. **Now things break.** `wrapper_image_video_refresh_cb` sees the sentinel and just returns ([image.c:157](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L157)). C# has the `VkImage` but no semaphore signal, and even if it did, `LoadVulkanTextureData` doesn't wrap it.

### 9.4 What's stubbed (the actual blockers)

| Stub | File:line | Libretro contract | What we do | Symptom |
|---|---|---|---|---|
| `vulkan_lock_queue` / `unlock_queue` | [vulkan.c:114-122](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L114) | When the core submits directly to `queue`, it must serialize with the frontend's own submissions. | No-ops. | Concurrent `vkQueueSubmit` — Vulkan UB. Most dangerous stub. Likely crash/corruption under load. |
| `vulkan_get_sync_index` | [vulkan.c:86](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L86) | Return which swapchain buffer is currently in flight (0..N-1). | Always returns 0 (comment: `// no idea !`). | Core thinks there's only one buffer; can't pipeline frames. |
| `vulkan_get_sync_index_mask` | [vulkan.c:92](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L92) | Return a bitmask of valid sync indices. | Always returns `1` (i.e. only index 0). | Same. |
| `vulkan_wait_sync_index` | [vulkan.c:98](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L98) | Block CPU-side until the frontend has finished with the buffer at the current sync index. | No-op. | Core overwrites its image while Unity is sampling. Visible as torn/corrupt frames. |
| `vulkan_set_signal_semaphore` | [vulkan.c:124](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L124) | Frontend signals this semaphore when done with the image; core waits on it before reusing. | No-op. | Race ahead by the core, same hazard as wait_sync_index. |
| `vulkan_set_command_buffers` | [vulkan.c:103](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L103) | Core hands us cmd buffers; frontend must submit them on its next queue submit. | Caches pointer, never submits. | If the core uses this mode instead of submitting itself, its rendering work is dropped. |
| `image.c` HW short-circuit | [image.c:157](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L157) | Use this as the signal that a new HW frame is ready for upload. | Logs + returns. | Main thread never knows a frame arrived. |
| `LoadVulkanTextureData` body | [LibretroMameCore.cs:1001](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs#L1001) | Wrap the `VkImage` as a `Texture2D` and bind to the screen shader. | Body is commented out except for logging. | Even with everything else fixed, the texture never reaches the GPU's sampler. |
| `wrapper_vulkan_get_image_info` (doesn't exist) | — | C# needs width/height/`VkFormat` to call `CreateExternalTexture`. | The data is in `create_info` on the C side but no accessor surfaces it. | `LoadVulkanTextureData` has hardcoded 640×480 RGBA32 in a comment because it has nothing better. |

### 9.5 The cross-device hazard

The libretro Vulkan contract says the core builds a `VkDevice` on the frontend-supplied `VkPhysicalDevice`, and the returned `VkQueue` is from that device. **This is the only reason the whole thing has a chance of working** — a `VkImage` is only usable on the `VkDevice` that allocated it. If the core ignored our `VkPhysicalDevice` and built its own (which the contract allows when we pass `VK_NULL_HANDLE`, but **not** when we pass a real handle), the resulting `VkImage` would be unsampleable on Unity's device. The fix in that case would require external memory APIs (`VK_KHR_external_memory`) and explicit OS-level handle export — significantly more involved.

Today the current call ([vulkan.c:267](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L267)) passes Unity's `vkPhysicalDevice`, so a well-behaved core (Flycast does honor this) builds the `VkDevice` correctly. **Watch for this when debugging blank-screen issues on a new core:** the log line at [vulkan.c:277-293](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L277) prints `context.gpu` — if that doesn't match the `vkPhysicalDevice` Unity gave us, the rest of the pipeline can't work, and no amount of fixing the stubs will help. The fallback in that case is to either:
- Patch the core to honor the GPU handle (upstream change), or
- Implement external-memory image sharing (significant native + plugin work).

The good news: Flycast, FBNeo Vulkan, Beetle PSX HW, and Mupen64 all honor the GPU handle. The known offenders are some research-grade cores.

### 9.6 Architectural roadmap

The work breaks into phases, each of which lands a verifiable behavior change. The order matters because Phase 1 prevents crashes that would otherwise hide bugs in Phase 2.

```
Phase 1   Synchronization safety                          → no UB, no crashes
Phase 2   Image wrapping                                  → first frame visible
Phase 3   Layout transitions                              → frame is correct, not corrupt
Phase 4   Flycast integration (drop core, BIOS, YAML)     → Dreamcast/NAOMI runs
Phase 5   Polish (pipeline cache, cmd buffers, build)     → production quality
```

**Phase 1 — Synchronization safety.** Today's no-op queue lock and sync index callbacks are a latent crash. Until the core and Unity coordinate on `graphicsQueue` access and on per-frame resource lifetimes, anything you build on top will be unreliable. This phase needs no Unity-side changes — it's all in `vulkan.c` plus a small C/C# accessor for fences.

**Phase 2 — Image wrapping.** Once it's safe to read the core's image, expose its dimensions and `VkFormat` to C# (new wrapper accessor), call `Texture2D.CreateExternalTexture` in `LoadVulkanTextureData`, and assign the result to `GameTexture` (or a parallel handle the shader can bind). The existing `Shader.Refresh` chain will pick it up. The signaling path can mirror the software path: when `vulkan_set_image` fires, also set the existing `GameTextureBufferSem` so the main thread's `LoadTextureData` dispatch reaches `LoadVulkanTextureData`.

**Phase 3 — Layout transitions.** A `VkImage` sampled by a fragment shader needs to be in `VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL` (or `VK_IMAGE_LAYOUT_GENERAL`). The libretro contract says the core delivers it in either layout. Verify this is happening; if not, insert a pipeline barrier through Unity's command buffer before sampling.

**Phase 4 — Flycast integration.** Once the pipeline can carry HW frames, Flycast becomes a userspace drop-in: copy `libflycast_libretro_android.so` into the user cores directory (auto-discovered by [CoresController.cs:131](../Assets/curif/LibRetroWrapper/CoresController.cs#L131)), drop BIOS files into `SystemDir`, configure a cabinet YAML to use `core: flycast`, and push the `flycast_renderer = "Vulkan"` core option via `EnvironmentHandlerCB` so it doesn't fall back to GL.

**Phase 5 — Polish.** Use Unity's `VkPipelineCache` to speed Flycast's initial shader compile. Fix `Application.mk` API level. Implement `set_command_buffers` for cores that use the deferred-submission mode. See §9.8 and §9.9.

### 9.7 Recommended starting points per stub

These are my best-guess concrete API choices based on the Unity Vulkan plugin docs. None of this has been run against the actual code, so treat the specifics as "recommended starting points" rather than authoritative — particularly the bits involving `IUnityGraphicsVulkanV2`, where the exact event-callback timing can be subtle.

**`vulkan_lock_queue` / `vulkan_unlock_queue`:** The libretro contract says these serialize the core's `vkQueueSubmit` against the frontend's. The Unity-canonical answer is `IUnityGraphicsVulkanV2::AccessQueue` (declared in [IUnityGraphicsVulkan.h:232](../libVulkanPlugin/source/IUnityGraphicsVulkan.h#L232)), which runs a callback with exclusive queue ownership. But `AccessQueue` is callback-style and per-event — it doesn't fit the lock/unlock idiom the core uses.

Recommended approach: expose a `lockQueue`/`unlockQueue` pair from libVulkanPlugin that takes a global mutex on a `std::mutex` shared between the plugin and the core wrapper. On Unity's side, configure events with `kUnityVulkanGraphicsQueueAccess_Allow` and wrap them in the same mutex. This is coarser than `AccessQueue` but matches the call shape.

**Sync indices and waits:** Implement a simple 2-frame ring (sync_index = `frameCounter % 2`, mask = `0b11`). Allocate two `VkFence` objects in the plugin, one per slot. After Unity's per-frame submission, signal slot `frameCounter % 2`'s fence. `vulkan_wait_sync_index` waits on the fence at the current slot. This is the minimal correct implementation.

**`vulkan_set_signal_semaphore`:** When the core hands us a semaphore, queue a `vkQueueSubmit` with `pSignalSemaphores = &semaphore` and no waits, on a command buffer that contains a layout transition no-op. Or, more elegantly: include it as a signal in Unity's next normal submission via `AccessQueue`. The semaphore must be signalled exactly once per frame even on dupes — track this carefully.

**`image.c:157` signal hand-off:** Replace the early return with a call to `TextureSemAvailableCB()` (the existing software-path signal). The C# main thread will then enter the dispatch in `UpdateTexture`, see `wrapper_is_hardware_rendering()` is true, and route to `LoadVulkanTextureData`. Reuses the existing handshake machinery.

**`wrapper_vulkan_get_image_info`:** Add a tiny C accessor that pulls `create_info.extent.width`, `create_info.extent.height`, and `create_info.format` out of the cached struct in `vulkan_set_image`. Expose via DllImport. C# calls it before `CreateExternalTexture`.

**`LoadVulkanTextureData` body:**

```csharp
// pseudo-code, untested
public static void LoadVulkanTextureData()
{
    if (!LibretroVulkan.isVkImageReady()) return;
    IntPtr vkImage = LibretroVulkan.GetVkImage();

    uint w = wrapper_vulkan_get_image_width();
    uint h = wrapper_vulkan_get_image_height();
    int vkFormat = wrapper_vulkan_get_image_format();
    TextureFormat unityFormat = MapVkFormatToUnity(vkFormat);  // VK_FORMAT_R8G8B8A8_UNORM → RGBA32

    if (GameTexture == null || GameTexture.width != w || GameTexture.height != h
        || GameTexture.format != unityFormat || RecreateTexture)
    {
        if (GameTexture != null) UnityEngine.Object.Destroy(GameTexture);
        GameTexture = Texture2D.CreateExternalTexture((int)w, (int)h, unityFormat,
                                                       mipChain: false, linear: false, vkImage);
        Shader.Refresh(GameTexture);
        Shader.ApplyConfiguration();
        RecreateTexture = false;
    }
    else
    {
        GameTexture.UpdateExternalTexture(vkImage);
    }
}
```

`UpdateExternalTexture` reuses the `Texture2D` shell with a new native handle — important when the core swaps images each frame (Flycast does this) rather than reusing a single image.

### 9.8 Flycast-specific notes (Dreamcast / NAOMI)

- **Renderer selection.** Flycast supports OpenGL and Vulkan; we need Vulkan. Set the core option via `EnvironmentHandlerCB`:
  ```yaml
  # ConfigCoresDir/flycast.yaml — see Core.ReadCoreEnvironment()
  environment:
    prefix: flycast
    properties:
      flycast_renderer: "Vulkan"
  ```
  Without this Flycast tries GL first and we'll get a non-Vulkan negotiation that the current code doesn't handle.
- **BIOS files.** Flycast needs `dc_boot.bin`, `dc_flash.bin` in `SystemDir`. NAOMI additionally needs `naomi.zip` (BIOS) and `awbios.zip` for Atomiswave games.
- **Native resolution.** NAOMI games are typically 640×480 (or 480×640 vertical for shmups). These fit comfortably in any Unity `Texture2D`. The software path's 1024×768 ceiling doesn't apply here — we skip the converters entirely on HW path.
- **Threaded rendering.** Flycast supports a "threaded rendering" core option. It's worth experimenting with for performance once basics work, but start with it off so the synchronization issues are easier to debug.
- **`wait_sync_index` is critical for Flycast.** It uses sync indices aggressively to manage its texture cache. The no-op implementation will cause visible texture corruption within seconds of gameplay.
- **No code changes needed to register the core.** Drop `libflycast_libretro_android.so` into `ConfigManager.CoresDir`; [CoresController.SyncCores](../Assets/curif/LibRetroWrapper/CoresController.cs#L88) copies it to `InternalCoresDir`, and [ScanForUserCores](../Assets/curif/LibRetroWrapper/CoresController.cs#L131) registers it as `flycast` (extracted from `lib<name>_libretro_android.so`).

### 9.9 libVulkanPlugin build notes

The plugin is built outside the main Unity build via Android NDK:

```
libVulkanPlugin/projects/Android/jni/
  Android.mk       # module definition
  Application.mk   # ABIs and platform
```

Two things in [`Application.mk`](../libVulkanPlugin/projects/Android/jni/Application.mk) will bite someone:

```
APP_ABI      := armeabi-v7a arm64-v8a x86 x86_64   ← only arm64-v8a is needed for Quest
APP_PLATFORM := android-9                          ← way too old; Vulkan requires API 24+
```

`APP_PLATFORM := android-9` is Gingerbread, which predates Vulkan by ~5 years. Likely the Unity build process overrides this when packaging, but rebuilding the plugin standalone with `ndk-build` against this `Application.mk` would produce a binary that misses required Vulkan symbols. Bump to `APP_PLATFORM := android-24` (matching Quest's minimum API) and drop ABIs other than `arm64-v8a` to halve build time.

The plugin currently lives outside the .gitignore'd `Assets/Plugins/Android64/*` directory but the built `.so` is excluded from version control ([.gitignore:126](../.gitignore#L126)) — so whoever picks up this work needs to (re)build the plugin and place the output in the right place for Unity to package it.

### 9.10 Open questions / risks

- **Queue-access timing with Unity.** `IUnityGraphicsVulkanV2::AccessQueue` is callback-driven and may not give us synchronous-style mutex access. If it doesn't, the cleanest answer might be to use a mutex shared with Unity's render thread via `InterceptInitialization` to hook `vkQueueSubmit` itself — significantly more invasive.
- **The `get_proc_address = vkGetInstanceProcAddr` line.** [vulkan.c:295](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c#L295) has a comment "This seems wrong. Why have I done that?" The `retro_hw_render_callback.get_proc_address` is supposed to be the function the core uses to resolve its own symbols. Passing `vkGetInstanceProcAddr` here works because that's how Vulkan loaders work — but the libretro contract for non-Vulkan cores expects a GL-style proc resolver. For Vulkan cores it's probably correct. Verify on a first Flycast boot.
- **Single-image vs swapchain.** Today the design assumes the core hands us one image (or a small set) via `set_image`. Some cores allocate a real swapchain internally. The current `vulkan_set_image` always overwrites the stashed image — if the core gives us a new image each frame (Flycast does), `UpdateExternalTexture` on the Unity side keeps the texture wrapper alive across changes. If a core gives us the same image with new contents each frame, just `Refresh` is enough. Both should work, but it's an "if it doesn't render, look here" candidate.
- **Pipeline cache.** Unity's `VkPipelineCache` is fetched into C# and passed to libretro via `wrapper_init_vulkan`, but never used in `vulkan.c`. Flycast does a lot of shader compilation on first boot — wiring this cache in could save significant time. Not blocking, but high-value.
- **`required_device_extensions` count is 0.** Bug noted in §9.2. Probably fine for Flycast which doesn't need swapchain, but should be fixed.

This is on the roadmap; see [docs/windows_migration.md](windows_migration.md) for related cross-platform considerations.

---

## 10. Limits and gotchas

- **One game at a time, globally.** `LibretroMameCore` is a `static` class and `GameTexture` is a single static `Texture2D`. The arcade can have many cabinets, but only one cabinet runs a live libretro game at any moment. Other cabinets in the room show attract videos via `GameVideoPlayer`, not live emulation. The "currently playing" state is the cabinet the player has inserted a coin into.
- **Max frame size: 1024 × 768.** `MAX_OUTPUT_SIZE = 1024 * 768 * 2` in [image_conversion.c:4](../Assets/curif/LibRetroWrapper/cwrapper/image_conversion.c#L4). A core reporting larger dimensions causes the converters to return `NULL`, the frame is silently dropped, and the screen freezes on the last delivered frame. Every arcade core we target fits comfortably — mame2003-plus and fbneo games are mostly 224p–480i. Even PS1 and N64 at their native output (typically 320×240 or 640×480) fit easily; what blocks those consoles isn't this ceiling, it's that their accurate cores require a hardware context (§9).
- **All formats normalize to RGB565.** The Texture2D is `TextureFormat.RGB565` and the converters dictate the output. There's no way to keep 24-bit color through the pipeline without changing both the texture format and every converter. For an arcade aesthetic this is fine.
- **The semaphore is binary, not counting.** Two frames produced before one upload = one frame visible. The dropped frame is the *older* one (the worker overwrote it in the still-locked-for-writes buffer on its way to producing N+1). In practice the worker is FPS-limited (`FPSControlNoUnity.isTime()`) so this is rare.
- **`bufIdx` parity must match between writer and reader.** The C side decides which buffer to *write* to (`outputData[bufIdx]`). The C# side reads `wrapper_image_get_buffer()` which returns whichever buffer was the destination of the most recent `swapBuffers`. The two indices are coupled implicitly through the published pointer — never trust the static `bufIdx` from C# directly.
- **The shader chain matters during resize.** `InitializeTexture` calls `Shader.Refresh(GameTexture)` after `Reinitialize`. Skipping this leaves the material pointing at a stale texture handle.
- **`#if !UNITY_EDITOR`** wraps the actual upload path. In the Editor, `UpdateTexture` is a no-op — the screen stays black unless you've mocked the texture for testing. The editor's libretro `.so` doesn't load on Windows host anyway.
- **No HDR.** Bilinear filtering on RGB565 inherits the precision limits of the format. Banding can be visible on smooth gradients; the CRT/scanline shaders mostly mask this.

---

## 11. File map

| Concern | File |
|---|---|
| Libretro headers (canonical) | [Assets/curif/LibRetroWrapper/cwrapper/libretro.h](../Assets/curif/LibRetroWrapper/cwrapper/libretro.h) |
| Pixel format storage / env callback | [Assets/curif/LibRetroWrapper/cwrapper/environment.c](../Assets/curif/LibRetroWrapper/cwrapper/environment.c), [environment.h](../Assets/curif/LibRetroWrapper/cwrapper/environment.h) |
| Video refresh callback (worker thread, C) | [Assets/curif/LibRetroWrapper/cwrapper/image.c:144](../Assets/curif/LibRetroWrapper/cwrapper/image.c#L144) |
| Pixel format conversions | [Assets/curif/LibRetroWrapper/cwrapper/image_conversion.c](../Assets/curif/LibRetroWrapper/cwrapper/image_conversion.c) |
| Vulkan negotiation (C) | [Assets/curif/LibRetroWrapper/cwrapper/vulkan.c](../Assets/curif/LibRetroWrapper/cwrapper/vulkan.c) |
| C# P/Invoke bridge, run loop, GameTexture | [Assets/curif/LibRetroWrapper/LibretroMameCore.cs](../Assets/curif/LibRetroWrapper/LibretroMameCore.cs) |
| Per-frame `Update()` driver | [Assets/curif/LibRetroWrapper/LibretroScreenController.cs:635](../Assets/curif/LibRetroWrapper/LibretroScreenController.cs#L635) |
| Vulkan plumbing (C#) | [Assets/curif/LibRetroWrapper/LibretroVulkan.cs](../Assets/curif/LibRetroWrapper/LibretroVulkan.cs), [UnityVulkan.cs](../Assets/curif/LibRetroWrapper/UnityVulkan.cs) |
| Native Vulkan plugin | [libVulkanPlugin/](../libVulkanPlugin/) |
| Shader system (material binding) | [Assets/curif/LibRetroWrapper/ShaderScreen.cs](../Assets/curif/LibRetroWrapper/ShaderScreen.cs), [ShaderCRT.cs](../Assets/curif/LibRetroWrapper/ShaderCRT.cs), [ShaderProjector.cs](../Assets/curif/LibRetroWrapper/ShaderProjector.cs) |
| Screen prefab / `_MainTex` target | [docs/screentype_system.md](screentype_system.md) |
