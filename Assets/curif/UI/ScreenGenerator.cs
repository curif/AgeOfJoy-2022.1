using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;

// Ensure your project has its own definitions for:
// - ConfigManager (with at least WriteConsoleError)
// - ColorSpaceBase
// - ColorSpaceManager
// - ShaderScreenBase
// - FontChars
// - ScreenGeneratorSkin
// - TextureDrawingUtilities (with ScrollRectangleVerticalBurst)

public class ScreenGenerator : MonoBehaviour
{
    public int CharactersXCount = 40;
    public int CharactersYCount = 25;

    public int TextureWidth = 320;
    public int TextureHeight = 200;

    private Color32 charBackgroundColor;
    private Color32 charForegroundColor;

    [SerializeField]
    public Dictionary<string, string> ShaderConfig = new Dictionary<string, string>();

    private Texture2D screenTexture;
    private Color32[] colorsBackgroundMatrix;

    private int ScreenWidth;
    private int ScreenHeight;
    private int centerStartX;
    private int centerStartY;

    private int lastX, lastY; // Current cursor position

    private bool needsDraw = false;

    private ShaderScreenBase shader;
    public ScreenGeneratorSkin Skin;

    public static readonly Dictionary<string, Color32> ColorMap = new Dictionary<string, Color32>()
    {
        { "none", new Color32(0, 0, 0, 0) }, { "red", new Color32(255, 0, 0, 255) },
        { "green", new Color32(0, 255, 0, 255) }, { "blue", new Color32(0, 0, 255, 255) },
        { "yellow", new Color32(255, 255, 0, 255) }, { "cyan", new Color32(0, 255, 255, 255) },
        { "magenta", new Color32(255, 0, 255, 255) }, { "white", new Color32(255, 255, 255, 255) },
        { "black", new Color32(0, 0, 0, 255) }, { "orange", new Color32(255, 165, 0, 255) },
    };

    /// <summary>
    /// Gets the underlying texture used for the screen display.
    /// </summary>
    public Texture2D Screen { get { return screenTexture; } }

    /// <summary>
    /// Initializes the ScreenGenerator with a specific skin.
    /// Creates the texture, resets colors, and sets the cursor to (0,0).
    /// </summary>
    /// <param name="skinName">The name of the skin to use.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator Init(string skinName)
    {
        SetSkin(skinName);
        createTexture();
        ResetColors();
        Locate(0, 0);
        return this;
    }

    public Color32 ForegroundColor { get { return charForegroundColor; } set { charForegroundColor = value; } }
    public Color32 BackgroundColor { get { return charBackgroundColor; } set { charBackgroundColor = value; } }
    public String ForegroundColorString { get { return Skin.ColorSpace.GetNameByColor(charForegroundColor); } set { charForegroundColor = Skin.ColorSpace.GetColorByName(value); } }
    public String BackgroundColorString { get { return Skin.ColorSpace.GetNameByColor(charBackgroundColor); } set { charBackgroundColor = Skin.ColorSpace.GetColorByName(value); } }

    /// <summary>
    /// Resets the character background color to the skin's default.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ResetBackgroundColor() { charBackgroundColor = Skin.ColorSpace.BackgroundDefault(); return this; }

    /// <summary>
    /// Resets the character foreground color to the skin's default.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ResetForegroundColor() { charForegroundColor = Skin.ColorSpace.ForegroundDefault(); return this; }

    /// <summary>
    /// Swaps the current foreground and background colors.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator InvertColors() { Color32 temp = charBackgroundColor; charBackgroundColor = charForegroundColor; charForegroundColor = temp; return this; }

    /// <summary>
    /// Resets both foreground and background colors to their defaults for the current skin.
    /// Fills the background color matrix if it exists.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ResetColors()
    {
        if (Skin == null) SetSkin("DefaultSkin"); // Ensure skin exists before accessing ColorSpace
        ResetBackgroundColor();
        ResetForegroundColor();
        if (colorsBackgroundMatrix != null) { Array.Fill(colorsBackgroundMatrix, charBackgroundColor); }
        return this;
    }

    /// <summary>
    /// Sets the color space for the screen and resets colors.
    /// </summary>
    /// <param name="colorSpaceName">Name of the color space to apply.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator SetColorSpace(string colorSpaceName) { if (Skin == null) SetSkin("DefaultSkin"); Skin.ColorSpace = ColorSpaceManager.GetColorSpace(colorSpaceName); ResetColors(); return this; }

    /// <summary>
    /// Sets the skin for the screen.
    /// </summary>
    /// <param name="newSkin">Name of the skin to apply.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator SetSkin(string newSkin) { if (Skin == null || Skin.Name != newSkin) { Skin = ScreenGeneratorSkin.GetSkin(newSkin, this); } return this; }

    /// <summary>
    /// Gets the current color space.
    /// </summary>
    /// <returns>The current ColorSpaceBase instance.</returns>
    public ColorSpaceBase GetColorSpace() { return Skin.ColorSpace; }

    /// <summary>
    /// Creates the screen texture if it doesn't exist.
    /// Calculates dimensions and initializes the background color matrix.
    /// </summary>
    /// <returns>The screen texture.</returns>
    private Texture2D createTexture()
    {
        if (screenTexture != null) return screenTexture;
        if (Skin == null) SetSkin("DefaultSkin");

        ScreenWidth = CharactersXCount * Skin.Font.CharactersWidth;
        ScreenHeight = CharactersYCount * Skin.Font.CharactersHeight;
        colorsBackgroundMatrix = new Color32[ScreenWidth * ScreenHeight];

        centerStartX = (TextureWidth - ScreenWidth) / 2;
        centerStartY = (TextureHeight - ScreenHeight) / 2;

        screenTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false)
        {
            name = "computer_screen",
            filterMode = FilterMode.Point,
            anisoLevel = 0
        };
        Skin.Font.setOffsets(centerStartX, centerStartY);
        ResetColors(); // Initialize colorsBackgroundMatrix with default background
        Clear(); // Apply border and initial background colors to texture
        return screenTexture;
    }

    /// <summary>
    /// Activates a shader for post-processing the screen texture.
    /// </summary>
    /// <param name="changeShader">The shader to activate.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ActivateShader(ShaderScreenBase changeShader)
    {
        createTexture(); // Ensure texture exists
        shader = changeShader;
        if (shader != null)
        {
            shader.Activate(screenTexture);
        }
        return this;
    }

    /// <summary>
    /// Called by Unity every frame. Updates the active shader if any.
    /// </summary>
    public void Update() { shader?.Update(); }

    /// <summary>
    /// Applies any pending changes to the screen texture if needsDraw is true.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator DrawScreen() { 
        if (needsDraw) { 
            if (spritesEnabled) {
                GetWorkingTexture().Apply();
                Color32[] pixels = baseTexture.GetPixels32();
                var sortedSprites = System.Linq.Enumerable.OrderBy(System.Linq.Enumerable.Where(activeSprites.Values, s => s.Visible && s.Texture != null), s => s.Z);
                foreach (var s in sortedSprites) {
                    DrawSpriteToBuffer(pixels, s.Texture, s.X, s.Y);
                }
                screenTexture.SetPixels32(pixels);
                screenTexture.Apply();
            } else {
                screenTexture.Apply(); 
            }
            needsDraw = false; 
        } 
        return this; 
    }

    public class ScreenSprite
    {
        public string Name;
        public Texture2D Texture;
        public int X, Y, Z;
        public bool Visible = true;
    }

    private Dictionary<string, Texture2D> spriteCache = new Dictionary<string, Texture2D>();
    private Dictionary<string, ScreenSprite> activeSprites = new Dictionary<string, ScreenSprite>();
    private Texture2D baseTexture;
    private bool spritesEnabled = false;

    public void LoadSprite(string name, string path)
    {
        if (System.IO.File.Exists(path))
        {
            StartCoroutine(CabinetTextureCache.LoadAndCacheAsync(path, (tex) => {
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Point;
                    spriteCache[name] = tex;
                }
            }, makeNoLongerReadable: false));
        }
    }

    public void DrawSprite(string name, int x, int y, int z)
    {
        if (!spriteCache.ContainsKey(name)) return;
        if (!spritesEnabled) EnableSprites();
        
        if (!activeSprites.ContainsKey(name)) {
            activeSprites[name] = new ScreenSprite() { Name = name };
        }
        activeSprites[name].Texture = spriteCache[name];
        activeSprites[name].X = x;
        activeSprites[name].Y = y;
        activeSprites[name].Z = z;
        activeSprites[name].Visible = true;
        needsDraw = true;
    }
    
    public void RemoveSprite(string name) {
        if (activeSprites.ContainsKey(name)) {
            activeSprites.Remove(name);
            needsDraw = true;
        }
    }

    public void ClearSprites()
    {
        activeSprites.Clear();
        spriteCache.Clear();
        spritesEnabled = false;
        if (baseTexture != null)
        {
            UnityEngine.Object.Destroy(baseTexture);
            baseTexture = null;
        }
        needsDraw = true;
    }

    public bool IsSpriteLoaded(string name)
    {
        return spriteCache.ContainsKey(name) && spriteCache[name] != null;
    }

    private void EnableSprites()
    {
        if (!spritesEnabled) {
            spritesEnabled = true;
            if (baseTexture == null) {
                baseTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
                baseTexture.filterMode = FilterMode.Point;
                if (screenTexture != null) {
                    baseTexture.SetPixels32(screenTexture.GetPixels32());
                    baseTexture.Apply();
                }
            }
        }
    }

    private Texture2D GetWorkingTexture()
    {
        if (spritesEnabled)
        {
            if (baseTexture == null) EnableSprites();
            return baseTexture;
        }
        return screenTexture;
    }

    private void DrawSpriteToBuffer(Color32[] dest, Texture2D src, int destX, int destY)
    {
        Color32[] srcPixels = src.GetPixels32();
        int srcWidth = src.width;
        int srcHeight = src.height;
        
        // Unity textures have origin at bottom-left, but we treat (0,0) as top-left in our screen coordinate system.
        // Therefore, y goes from 0 (top) down.
        // TextureDrawingUtilities flips Y like this: int unityY = TextureHeight - 1 - dy;
        for (int sy = 0; sy < srcHeight; sy++)
        {
            // If src is a Unity texture, its row 0 is the bottom row of pixels in the image.
            // If we want it to draw top-down matching the screen, we must read the src from top to bottom.
            int srcY = srcHeight - 1 - sy;
            int dy = destY + sy;
            if (dy < 0 || dy >= TextureHeight) continue;
            
            for (int sx = 0; sx < srcWidth; sx++)
            {
                int dx = destX + sx;
                if (dx < 0 || dx >= TextureWidth) continue;
                
                int unityY = TextureHeight - 1 - dy;
                if (unityY < 0 || unityY >= TextureHeight) continue;
                
                int srcIdx = srcY * srcWidth + sx;
                Color32 srcC = srcPixels[srcIdx];
                if (srcC.a == 0) continue; // transparent
                
                int destIdx = unityY * TextureWidth + dx;
                if (srcC.a == 255) {
                    dest[destIdx] = srcC;
                } else {
                    Color32 bg = dest[destIdx];
                    float a = srcC.a / 255f;
                    dest[destIdx] = new Color32(
                        (byte)(srcC.r * a + bg.r * (1 - a)),
                        (byte)(srcC.g * a + bg.g * (1 - a)),
                        (byte)(srcC.b * a + bg.b * (1 - a)),
                        255
                    );
                }
            }
        }
    }

    /// <summary>
    /// Prints a character at the specified (x,y) character coordinates using current colors.
    /// </summary>
    /// <param name="x">Character column.</param>
    /// <param name="y">Character row.</param>
    /// <param name="charNum">Character to print.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator PrintChar(int x, int y, char charNum) { return PrintChar(x, y, charNum, charForegroundColor, charBackgroundColor); }

    /// <summary>
    /// Prints a character at the specified (x,y) character coordinates with specified colors.
    /// </summary>
    /// <param name="x">Character column.</param>
    /// <param name="y">Character row.</param>
    /// <param name="charNum">Character to print.</param>
    /// <param name="fgColor">Foreground color.</param>
    /// <param name="bgColor">Background color.</param>
    /// <param name="translate">Whether to translate the character code (e.g., for custom fonts).</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator PrintChar(int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        if (screenTexture == null || Skin?.Font == null) return this;
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintChar] Invalid xy: ({x},{y}), char: {charNum}"); return this; }

        Skin.Font.PrintChar(screenTexture, x, y, charNum, fgColor, bgColor, translate);
        needsDraw = true;
        return this;
    }

    /// <summary>
    /// Prints a string at the specified (x,y) character coordinates.
    /// Handles newlines and special character codes (e.g., \123).
    /// Updates cursor position.
    /// </summary>
    /// <param name="x">Starting character column.</param>
    /// <param name="y">Starting character row.</param>
    /// <param name="text">String to print.</param>
    /// <param name="inverted">If true, swaps foreground and background colors for this print.</param>
    /// <returns>The number of lines printed.</returns>
    public int Print(int x, int y, string text, bool inverted = false)
    {
        Color32 fgColor = inverted ? charBackgroundColor : charForegroundColor;
        Color32 bgColor = inverted ? charForegroundColor : charBackgroundColor;

        if (screenTexture == null) return 0;
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.Print] Invalid xy: ({x},{y})"); return 0; }

        int initialPrintY = y;
        this.lastX = x;
        this.lastY = y;

        for (int i = 0; i < text.Length;)
        {
            char c = text[i++];
            if (c == '\n') { NextLine(); }
            else if (c == '\\')
            {
                if (i < text.Length)
                {
                    StringBuilder strIndex = new StringBuilder();
                    while (i < text.Length && char.IsDigit(text[i]) && strIndex.Length < 4) { strIndex.Append(text[i++]); }
                    int charCode = '?'; // Default for invalid sequence
                    if (strIndex.Length > 0)
                    {
                        if (int.TryParse(strIndex.ToString(), out int parsedCode) && parsedCode < 256)
                        {
                            charCode = parsedCode;
                        }
                    }
                    else
                    {
                        charCode = '\\'; // No digits followed '\', treat as literal '\'
                    }
                    PrintCharPosition((char)charCode, fgColor, bgColor, false); // Special codes are not typically translated by font map
                }
                else { PrintCharPosition('\\', fgColor, bgColor); } // '\' at end of string
            }
            else { PrintCharPosition(c, fgColor, bgColor); }
        }
        return (this.lastY - initialPrintY) + 1;
    }

    /// <summary>
    /// Prints a string at the current cursor position. Moves to next line if cursor is not at column 0.
    /// </summary>
    /// <param name="text">String to print.</param>
    /// <param name="inverted">If true, swaps foreground and background colors for this print.</param>
    /// <returns>The number of lines printed.</returns>
    public int Print(string text, bool inverted = false)
    {
        if (this.lastX > 0) { NextLine(); } // Behave like a new line print if not at start of line
        return Print(this.lastX, this.lastY, text, inverted);
    }

    /// <summary>
    /// Sets the cursor position to the specified (x,y) character coordinates.
    /// Clamps values to be within screen character bounds.
    /// </summary>
    /// <param name="x">Character column.</param>
    /// <param name="y">Character row.</param>
    public void Locate(int x, int y)
    {
        this.lastX = Mathf.Clamp(x, 0, CharactersXCount - 1);
        this.lastY = Mathf.Clamp(y, 0, CharactersYCount - 1);
    }

    /// <summary>
    /// Advances the cursor to the next character position. Handles line wrapping and scrolling.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator NextChar()
    {
        this.lastX++;
        if (this.lastX >= CharactersXCount) { NextLine(); }
        return this;
    }

    /// <summary>
    /// Prints a single character at the current cursor position and advances the cursor.
    /// </summary>
    private void PrintCharPosition(char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        PrintChar(this.lastX, this.lastY, charNum, fgColor, bgColor, translate);
        NextChar();
    }

    /// <summary>
    /// Moves the cursor to the beginning of the next line.
    /// If at the last line, scrolls the screen content up.
    /// </summary>
    private void NextLine()
    {
        this.lastX = 0;
        if (this.lastY == CharactersYCount - 1) { ScrollUp(); }
        else { this.lastY++; }
    }

    /// <summary>
    /// Scrolls the entire character display area vertically by a number of character lines.
    /// Uses the Burst-enabled scrolling utility.
    /// </summary>
    /// <param name="linesToScroll">Number of character lines to scroll. Positive for content to scroll DOWN (new space at top), negative for content to scroll UP (new space at bottom).</param>
    /// <param name="fillColorOverride">Optional color to fill the new area. If null, uses current charBackgroundColor.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ScrollCharacterArea(int linesToScroll, Color32? fillColorOverride = null)
    {
        if (screenTexture == null || Skin?.Font == null || linesToScroll == 0)
        {
            // ConfigManager.WriteConsoleError("[SG.ScrollCharacterArea] Invalid parameters or not initialized."); // Allow silent fail for linesToScroll = 0
            if (linesToScroll != 0) ConfigManager.WriteConsoleError("[SG.ScrollCharacterArea] Invalid parameters or not initialized.");
            return this;
        }

        int charPixelWidth = Skin.Font.CharactersWidth;
        int charPixelHeight = Skin.Font.CharactersHeight;

        if (charPixelWidth <= 0 || charPixelHeight <= 0)
        {
            ConfigManager.WriteConsoleError("[SG.ScrollCharacterArea] Invalid character pixel dimensions.");
            return this;
        }

        int scrollAmountPixelsY = linesToScroll * charPixelHeight;
        Color32 fillColor = fillColorOverride ?? this.charBackgroundColor;

        // Correction for inverted scrolling:
        // The 'linesToScroll' convention is: positive for content down (new space top), negative for content up (new space bottom).
        // - If linesToScroll is negative (e.g., -1 for ScrollUp), content moves UP. Pixels must shift DOWN.
        //   scrollAmountPixelsY will be negative (e.g., -charPixelHeight).
        // - If linesToScroll is positive (e.g., +1 for ScrollDown), content moves DOWN. Pixels must shift UP.
        //   scrollAmountPixelsY will be positive (e.g., +charPixelHeight).
        //
        // If TextureDrawingUtilities.ScrollRectangleVerticalBurst interprets its scroll parameter such that:
        //   - Positive values shift pixels DOWN.
        //   - Negative values shift pixels UP.
        // Then, to make content scroll UP (pixels shift DOWN), we need to pass a POSITIVE value to the utility.
        //   (linesToScroll < 0 gives scrollAmountPixelsY < 0. We need -scrollAmountPixelsY).
        // To make content scroll DOWN (pixels shift UP), we need to pass a NEGATIVE value to the utility.
        //   (linesToScroll > 0 gives scrollAmountPixelsY > 0. We need -scrollAmountPixelsY).
        // Hence, we invert the sign of scrollAmountPixelsY.
        int utilityScrollAmountPixelsY = -scrollAmountPixelsY;

        if (TextureDrawingUtilities.ScrollRectangleVerticalBurst(
                GetWorkingTexture(),
                this.centerStartX, this.centerStartY,   // Top-left pixel of the character area (in Unity texture coords this is bottom-left of area)
                this.ScreenWidth, this.ScreenHeight,    // Pixel dimensions of the character area
                utilityScrollAmountPixelsY, fillColor)) // Pass the corrected scroll amount
        {
            this.needsDraw = true;
        }
        return this;
    }

    /// <summary>
    /// Scrolls a sub-rectangle of the character display area vertically.
    /// </summary>
    /// <param name="charRectX">Starting character column of the sub-rectangle.</param>
    /// <param name="charRectY">Starting character row of the sub-rectangle.</param>
    /// <param name="charRectWidth">Width of the sub-rectangle in characters.</param>
    /// <param name="charRectHeight">Height of the sub-rectangle in characters.</param>
    /// <param name="linesToScroll">Number of character lines to scroll. Positive for content DOWN, negative for content UP.</param>
    /// <param name="fillColorOverride">Optional color to fill the new area. If null, uses current charBackgroundColor.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ScrollCharacterSubRectVertical(
        int charRectX, int charRectY,
        int charRectWidth, int charRectHeight,
        int linesToScroll, Color32? fillColorOverride = null)
    {
        if (screenTexture == null || Skin?.Font == null || linesToScroll == 0 ||
            charRectWidth <= 0 || charRectHeight <= 0)
        {
            // ConfigManager.WriteConsoleError("[SG.ScrollCharacterSubRectVertical] Invalid parameters or not initialized.");
            if (linesToScroll != 0) ConfigManager.WriteConsoleError("[SG.ScrollCharacterSubRectVertical] Invalid parameters or not initialized.");
            return this;
        }

        if (charRectX < 0 || charRectY < 0 ||
            charRectX + charRectWidth > this.CharactersXCount ||
            charRectY + charRectHeight > this.CharactersYCount)
        {
            ConfigManager.WriteConsoleError("[SG.ScrollCharacterSubRectVertical] Specified character rectangle is out of bounds.");
            return this;
        }

        int charPixelWidth = Skin.Font.CharactersWidth;
        int charPixelHeight = Skin.Font.CharactersHeight;

        if (charPixelWidth <= 0 || charPixelHeight <= 0)
        {
            ConfigManager.WriteConsoleError("[SG.ScrollCharacterSubRectVertical] Invalid character pixel dimensions from Skin.Font.");
            return this;
        }

        int pixelRectX = this.centerStartX + (charRectX * charPixelWidth);
        int pixelRectY = this.centerStartY + (charRectY * charPixelHeight);
        int pixelRectWidth = charRectWidth * charPixelWidth;
        int pixelRectHeight = charRectHeight * charPixelHeight;
        int scrollAmountPixelsY = linesToScroll * charPixelHeight;
        Color32 fillColor = fillColorOverride ?? this.charBackgroundColor;

        // Apply the same sign inversion logic as in ScrollCharacterArea
        int utilityScrollAmountPixelsY = -scrollAmountPixelsY;

        if (TextureDrawingUtilities.ScrollRectangleVerticalBurst(
                GetWorkingTexture(),
                pixelRectX, pixelRectY,
                pixelRectWidth, pixelRectHeight,
                utilityScrollAmountPixelsY, fillColor)) // Pass the corrected scroll amount
        {
            this.needsDraw = true;
        }
        return this;
    }

    /// <summary>
    /// Scrolls the screen content up by one line.
    /// Old top line disappears, new blank line appears at the bottom.
    /// </summary>
    public void ScrollUp()
    {
        if (screenTexture == null || Skin?.Font == null) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Not initialized."); return; }
        if (this.ScreenWidth <= 0 || this.ScreenHeight <= 0) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Invalid screen pixel dimensions."); return; }
        if (Skin.Font.CharactersHeight <= 0) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Invalid char pixel height."); return; }

        // To scroll content UP, linesToScroll should be negative.
        ScrollCharacterArea(-1, this.charBackgroundColor);
    }

    /// <summary>
    /// Clears the entire texture to the skin's border color.
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator ClearBackground()
    {
        if (GetWorkingTexture() != null && Skin != null)
        {
            Color32[] pixels = GetWorkingTexture().GetPixels32(); // Gets all pixels of the texture
            if (pixels.Length > 0) // Check if pixels array is not empty
            {
                Array.Fill(pixels, Skin.BorderColor);
                GetWorkingTexture().SetPixels32(pixels); // Apply to the whole texture
                needsDraw = true;
            }
        }
        return this;
    }

    /// <summary>
    /// Clears the character display area to the current background color.
    /// Also clears the entire texture background to the border color first.
    /// Resets cursor to (0,0).
    /// </summary>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator Clear()
    {
        if (screenTexture == null) { createTexture(); } // Ensure texture and skin are initialized
        if (Skin == null) SetSkin("DefaultSkin");

        // Update colorsBackgroundMatrix if current charBackgroundColor has changed
        // This ensures the character area is cleared with the *current* background color
        if (colorsBackgroundMatrix != null && colorsBackgroundMatrix.Length > 0 && (colorsBackgroundMatrix.Length == 0 || !colorsBackgroundMatrix[0].Equals(charBackgroundColor)))
        {
            Array.Fill(colorsBackgroundMatrix, charBackgroundColor);
        }

        ClearBackground(); // Fill entire texture with border color

        // Fill the character screen area with the current background color
        if (colorsBackgroundMatrix != null && colorsBackgroundMatrix.Length > 0)
        {
            GetWorkingTexture().SetPixels32(centerStartX, centerStartY, ScreenWidth, ScreenHeight, colorsBackgroundMatrix);
        }

        needsDraw = true;
        Locate(0, 0);
        return this;
    }

    /// <summary>
    /// Prints text centered horizontally on a specified line.
    /// </summary>
    /// <param name="y">Character row to print on.</param>
    /// <param name="text">String to print (newlines are stripped).</param>
    /// <param name="inverted">If true, use inverted colors.</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator PrintCentered(int y, string text, bool inverted = false)
    {
        if (screenTexture == null) return this;
        if (y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintCentered] Invalid y: {y}"); return this; }
        string singleLineText = text.Replace("\n", "").Replace("\r", "");
        int x = (CharactersXCount - singleLineText.Length) / 2;
        if (x < 0) x = 0; // Clamp if text is wider than screen
        Print(x, y, singleLineText, inverted); // Use singleLineText to avoid issues with original Print's newline handling
        return this;
    }

    /// <summary>
    /// Fills an entire character line with a specified character.
    /// </summary>
    /// <param name="y">Character row to fill.</param>
    /// <param name="inverted">If true, use inverted colors.</param>
    /// <param name="c">Character to fill the line with (default is '-').</param>
    /// <returns>The ScreenGenerator instance.</returns>
    public ScreenGenerator PrintLine(int y, bool inverted, char c = '-')
    {
        if (screenTexture == null) return this;
        if (y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintLine] Invalid y: {y}"); return this; }
        Print(0, y, new string(c, CharactersXCount), inverted);
        return this;
    }

    /// <summary>
    /// Gets the top-left pixel coordinates of a character cell.
    /// In Unity's texture coordinate system (0,0 is bottom-left), this will be the bottom-left of the character cell.
    /// </summary>
    /// <param name="charX">Character column.</param>
    /// <param name="charY">Character row.</param>
    /// <returns>Vector2Int with pixel coordinates, or (-1,-1) if invalid.</returns>
    public Vector2Int GetCharPixelPosition(int charX, int charY)
    {
        if (TryGetCharPixelPosition(charX, charY, out int px, out int py)) { return new Vector2Int(px, py); }
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Tries to get the top-left pixel coordinates of a character cell.
    /// In Unity's texture coordinate system (0,0 is bottom-left), this will be the bottom-left of the character cell.
    /// </summary>
    /// <param name="charX">Character column.</param>
    /// <param name="charY">Character row.</param>
    /// <param name="pixelX">Output pixel X coordinate.</param>
    /// <param name="pixelY">Output pixel Y coordinate.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public bool TryGetCharPixelPosition(int charX, int charY, out int pixelX, out int pixelY)
    {
        pixelX = -1; pixelY = -1;
        if (Skin?.Font == null)
        {
            // Debug.LogError("[SG.TryGetCharPixelPos] Skin or Font not initialized."); // Potentially noisy
            return false;
        }
        // Note: centerStartX/Y are bottom-left of the character grid area within the texture.
        // charX/Y are 0-indexed from top-left of character grid.
        // To map to texture coordinates (Y up):
        // pixelX = centerStartX + (charX * char_width)
        // pixelY for character row 'charY' (0 is top row):
        // The top character row (charY=0) corresponds to the highest pixel rows in the character screen area.
        // The character screen area's top is at centerStartY + ScreenHeight.
        // The Y coordinate of character row 'charY' (bottom edge of char) is:
        // centerStartY + (CharactersYCount - 1 - charY) * Skin.Font.CharactersHeight
        // This is if charY is 0 = top row.
        // The provided code seems to assume charY is already in bottom-up system for this calculation,
        // or Font.PrintChar handles the Y inversion.
        // The existing calculation: centerStartY + (charY * Skin.Font.CharactersHeight) implies charY is 0=bottom row for this calc.
        // This is consistent if PrintChar and other functions treat charY as 0=top row and convert internally,
        // or if Font.PrintChar expects charY as 0=top row and its internal 'centerStartY' usage accounts for it.
        // For now, will keep the original calculation as it's used by Font.setOffsets and likely PrintChar.
        pixelX = this.centerStartX + (charX * Skin.Font.CharactersWidth);
        pixelY = this.centerStartY + (charY * Skin.Font.CharactersHeight); // This is the bottom-left of the char cell if charY is 0=bottom
        return true;
    }

    // --- Direct Drawing Methods using TextureDrawingUtilities ---

    /// <summary>Draws a single point on the screen texture.</summary>
    public ScreenGenerator DrawPoint(int x, int y, Color32 color)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawPoint(GetWorkingTexture(), x, y, color, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws a line on the screen texture.</summary>
    public ScreenGenerator DrawLine(int x1, int y1, int x2, int y2, Color32 color)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawLine(GetWorkingTexture(), x1, y1, x2, y2, color, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws a box (rectangle) on the screen texture.</summary>
    public ScreenGenerator DrawBox(int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawBox(GetWorkingTexture(), x, y, width, height, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws a circle on the screen texture.</summary>
    public ScreenGenerator DrawCircle(int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawCircle(GetWorkingTexture(), centerX, centerY, radius, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws an oval (ellipse) on the screen texture.</summary>
    public ScreenGenerator DrawOval(int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawOval(GetWorkingTexture(), centerX, centerY, radiusX, radiusY, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws an image (from pixel array) rescaled onto the screen texture.</summary>
    public ScreenGenerator DrawImageRescaled(int x, int y, int width, int height, Color32[] pixels, int pixelsWidth, int pixelsHeight)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawImageRescaled(GetWorkingTexture(), x, y, width, height, pixels, pixelsWidth, pixelsHeight, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    /// <summary>Draws a source texture rescaled onto the screen texture.</summary>
    public ScreenGenerator DrawTextureRescaled(int x, int y, int width, int height, Texture2D sourceTexture)
    {
        if (GetWorkingTexture() != null && TextureDrawingUtilities.DrawTextureRescaled(GetWorkingTexture(), x, y, width, height, sourceTexture, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
}