using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text;
using System.Reflection;

public class ScreenGenerator : MonoBehaviour
{
    public int CharactersXCount = 40;
    public int CharactersYCount = 25;

    public int TextureWidth;
    public int TextureHeight;

    private Color32 charBackgroundColor;
    private Color32 charForegroundColor;

    //public string ShaderName = "crt";
    [SerializeField]
    public Dictionary<string, string> ShaderConfig = new Dictionary<string, string>();

    // The texture that represents the screen, it has 40x25 characters of capacity
    private Texture2D screenTexture;
    private Color32[] colorsBackgroundMatrix;

    // These are pixel dimensions/offsets for the character display area within screenTexture
    private int ScreenWidth; // Width of the character display area in pixels
    private int ScreenHeight; // Height of the character display area in pixels
    private int centerStartX; // Top-left X pixel coord of character area within texture
    private int centerStartY; // Top-left Y pixel coord of character area within texture

    private int lastX, lastY; // Current cursor position (character coordinates)

    private bool needsDraw = false;

    //Renderer
    private ShaderScreenBase shader;

    public ScreenGeneratorSkin Skin;

    public static readonly Dictionary<string, Color32> ColorMap = new Dictionary<string, Color32>()
    {
        { "none", new Color32(0, 0, 0, 0) },
        { "red", new Color32(255, 0, 0, 255) },
        { "green", new Color32(0, 255, 0, 255) },
        { "blue", new Color32(0, 0, 255, 255) },
        { "yellow", new Color32(255, 255, 0, 255) },
        { "cyan", new Color32(0, 255, 255, 255) },
        { "magenta", new Color32(255, 0, 255, 255) },
        { "white", new Color32(255, 255, 255, 255) },
        { "black", new Color32(0, 0, 0, 255) },
        { "orange", new Color32(255, 165, 0, 255) },
        { "lime", new Color32(0, 255, 0, 255) },
        { "purple", new Color32(128, 0, 128, 255) },
        { "teal", new Color32(0, 128, 128, 255) },
        { "maroon", new Color32(128, 0, 0, 255) },
        { "navy", new Color32(0, 0, 128, 255) },
        { "olive", new Color32(128, 128, 0, 255) },
        { "silver", new Color32(192, 192, 192, 255) },
        { "gray", new Color32(128, 128, 128, 255) },
        { "brown", new Color32(165, 42, 42, 255) },
        { "aqua", new Color32(0, 255, 255, 255) },
        { "fuchsia", new Color32(255, 0, 255, 255) },
        { "lightgray", new Color32(211, 211, 211, 255) },
        { "darkgray", new Color32(169, 169, 169, 255) }
    };

    public Texture2D Screen
    {
        get
        {
            return screenTexture;
        }
    }

    public ScreenGenerator Init(string skinName)
    {
        SetSkin(skinName);
        createTexture();
        ResetColors();
        Locate(0, 0); // Initialize cursor position
        return this;
    }

    public Color32 ForegroundColor
    {
        get { return charForegroundColor; }
        set { charForegroundColor = value; }
    }

    public Color32 BackgroundColor
    {
        get { return charBackgroundColor; }
        set { charBackgroundColor = value; }
    }

    public String ForegroundColorString
    {
        get { return Skin.ColorSpace.GetNameByColor(charForegroundColor); }
        set { charForegroundColor = Skin.ColorSpace.GetColorByName(value); }
    }

    public String BackgroundColorString
    {
        get { return Skin.ColorSpace.GetNameByColor(charBackgroundColor); }
        set { charBackgroundColor = Skin.ColorSpace.GetColorByName(value); }
    }

    public ScreenGenerator ResetBackgroundColor()
    {
        charBackgroundColor = Skin.ColorSpace.BackgroundDefault();
        return this;
    }
    public ScreenGenerator ResetForegroundColor()
    {
        charForegroundColor = Skin.ColorSpace.ForegroundDefault();
        return this;
    }
    public ScreenGenerator InvertColors()
    {
        Color32 temp = charBackgroundColor;
        charBackgroundColor = charForegroundColor;
        charForegroundColor = temp;
        return this;
    }

    public ScreenGenerator ResetColors()
    {
        ResetBackgroundColor();
        ResetForegroundColor();

        //for Clear()
        if (colorsBackgroundMatrix != null) // Ensure matrix exists
        {
            Array.Fill(colorsBackgroundMatrix, charBackgroundColor);
        }
        return this;
    }

    public ScreenGenerator SetColorSpace(string colorSpaceName)
    {
        Skin.ColorSpace = ColorSpaceManager.GetColorSpace(colorSpaceName);
        ResetColors();
        return this;
    }

    public ScreenGenerator SetSkin(string newSkin)
    {
        if (Skin == null || Skin.Name != newSkin)
        {
            Skin = ScreenGeneratorSkin.GetSkin(newSkin, this);
        }
        return this;
    }

    public ColorSpaceBase GetColorSpace()
    {
        return Skin.ColorSpace;
    }

    private Texture2D createTexture()
    {
        if (screenTexture != null)
            return screenTexture;

        // These are pixel dimensions of the character display area
        ScreenWidth = CharactersXCount * Skin.Font.CharactersWidth;
        ScreenHeight = CharactersYCount * Skin.Font.CharactersHeight;

        colorsBackgroundMatrix = new Color32[ScreenWidth * ScreenHeight];

        // These are pixel offsets for the top-left of the character display area
        centerStartX = (TextureWidth - ScreenWidth) / 2;
        centerStartY = (TextureHeight - ScreenHeight) / 2;

        screenTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        screenTexture.name = "computer_screen";
        screenTexture.filterMode = FilterMode.Bilinear;
        screenTexture.anisoLevel = 0;

        Skin.Font.setOffsets(centerStartX, centerStartY); // Font might need absolute offsets
        ResetColors(); // Initialize colorsBackgroundMatrix after it's sized

        return screenTexture;
    }

    public ScreenGenerator ActivateShader(ShaderScreenBase changeShader)
    {
        createTexture(); // Ensure texture exists
        shader = changeShader;
        shader.Activate(screenTexture);
        return this;
    }

    public void Update()
    {
        shader?.Update();
    }

    public ScreenGenerator DrawScreen()
    {
        if (needsDraw)
        {
            screenTexture.Apply();
            needsDraw = false;
        }
        return this;
    }

    public ScreenGenerator PrintChar(int x, int y, char charNum)
    {
        return PrintChar(x, y, charNum, charForegroundColor, charBackgroundColor);
    }

    public ScreenGenerator PrintChar(int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        if (screenTexture == null || Skin == null || Skin.Font == null)
            return this;

        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintChar] Invalid parameters x,y: ({x},{y}), charNum: {charNum}");
            return this;
        }

        Skin.Font.PrintChar(screenTexture, x, y, charNum, fgColor, bgColor, translate);
        needsDraw = true;
        return this;
    }

    public int Print(int x, int y, string text, bool inverted = false)
    {
        Color32 fgColor = inverted ? charBackgroundColor : charForegroundColor;
        Color32 bgColor = inverted ? charForegroundColor : charBackgroundColor;

        if (screenTexture == null)
            return 0;

        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.Print] Invalid parameters for Print method, x,y: ({x},{y})");
            return 0;
        }

        int startY = y; // To calculate lines printed

        // Initialize lastX and lastY based on the input x, y for the start of this print operation
        // PrintCharPosition will update them as it goes.
        lastX = x;
        lastY = y;

        for (int i = 0; i < text.Length; /* i incremented in loop */)
        {
            char c = text[i++];
            if (c == '\n')
            {
                NextLine(ref lastX, ref lastY); // Use lastX, lastY directly as the cursor
            }
            else if (c == '\\')
            {
                if (i < text.Length)
                {
                    StringBuilder strIndex = new StringBuilder();
                    while (i < text.Length && char.IsDigit(text[i]) && strIndex.Length < 4)
                    {
                        strIndex.Append(text[i]);
                        i++;
                    }
                    int index;
                    if (strIndex.Length > 0)
                    {
                        if (!int.TryParse(strIndex.ToString(), out index) || index >= 256)
                        {
                            index = '?';
                        }
                    }
                    else
                    {
                        index = '\\';
                    }
                    PrintCharPosition((char)index, ref lastX, ref lastY, fgColor, bgColor, false);
                }
                else
                {
                    PrintCharPosition('\\', ref lastX, ref lastY, fgColor, bgColor);
                }
            }
            else
            {
                PrintCharPosition(c, ref lastX, ref lastY, fgColor, bgColor);
            }
        }

        // Implicit newline behavior after a print command finishes:
        // If the last character printed did NOT result in the cursor already being at the beginning of a new line
        // (i.e., lastX > 0), then move to the beginning of the next line.
        if (lastX > 0)
        {
            NextLine(ref lastX, ref lastY); // This will handle y increment and scrolling if needed
        }
        // If lastX was already 0 (because of a wrap or explicit \n), then the cursor is already at the start of a new line.
        // No further action needed here.

        return (lastY - startY) + 1; // Calculate lines printed based on starting Y and final Y
    }


    private void NextLine(ref int x, ref int y)
    {
        x = 0; // Always move to the beginning of the new line

        // If we are currently on the last line, a 'next line' means we need to scroll.
        if (y == CharactersYCount - 1)
        {
            ScrollUp();
        }
        else // We are not on the last line, so just increment y normally
        {
            y++;
        }
    }

    public int Print(string text, bool inverted = false)
    {
        return Print(lastX, lastY, text, inverted);
    }

    public void Locate(int x, int y)
    {
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            //ConfigManager.WriteConsoleError($"[ScreenGenerator.Locate] Invalid coordinates: ({x},{y})");
            // Optionally clamp or return, for now, allow setting slightly out of bounds
            // if it's immediately corrected by a printing action that scrolls.
            // For safety, let's clamp.
            lastX = Mathf.Clamp(x, 0, CharactersXCount - 1);
            lastY = Mathf.Clamp(y, 0, CharactersYCount - 1);
            return;
        }
        lastX = x;
        lastY = y;
    }

    private void PrintCharPosition(char charNum, ref int x, ref int y, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        // At this point, x and y are character coordinates for the current char.
        // They should be within [0, CharactersXCount-1] and [0, CharactersYCount-1].
        PrintChar(x, y, charNum, fgColor, bgColor, translate);

        x++; // Advance column for the *next* character

        if (x >= CharactersXCount) // If advanced column is off-screen right
        {
            NextLine(ref x, ref y); // Move to next line (x=0, y++), potentially scrolls
                                    // y might become CharactersYCount here, but NextLine fixes it to CharactersYCount-1 if scroll occurs
        }

        // Update global cursor to where the *next* character will be printed
        lastX = x;
        lastY = y;
    }
    /// <summary>
    /// Scrolls the entire character display area up by one line.
    /// The top line is discarded, and the bottom line is cleared.
    /// This version extracts the character area, manipulates it, and sets it back,
    /// which is efficient for partial GPU updates.
    /// </summary>
    public void ScrollUp()
    {
        if (screenTexture == null || Skin == null || Skin.Font == null)
        {
            ConfigManager.WriteConsoleError("[ScreenGenerator.ScrollUp] Cannot scroll, essential components not initialized.");
            return;
        }
        // ScreenWidth and ScreenHeight are the pixel dimensions of the character display area
        if (this.ScreenWidth <= 0 || this.ScreenHeight <= 0)
        {
            ConfigManager.WriteConsoleError("[ScreenGenerator.ScrollUp] Character area pixel dimensions are invalid.");
            return;
        }

        int charActualPixelHeight = Skin.Font.CharactersHeight; // Pixel height of one character
        if (charActualPixelHeight <= 0)
        {
            ConfigManager.WriteConsoleError("[ScreenGenerator.ScrollUp] Character pixel height is invalid.");
            return;
        }

        // pixelsPerCharRowStrip is the total number of pixels in one character line
        // (spanning ScreenWidth pixels wide, and charActualPixelHeight pixels tall)
        int pixelsPerCharRowStrip = this.ScreenWidth * charActualPixelHeight;
        if (pixelsPerCharRowStrip <= 0)
        {
            ConfigManager.WriteConsoleError("[ScreenGenerator.ScrollUp] Calculated pixelsPerCharRowStrip is invalid.");
            return;
        }

        // --- Step 1: Get all pixels from the main texture ---
        Color32[] allTexturePixels = screenTexture.GetPixels32();

        // --- Step 2: Create a dedicated array for our character display area ---
        Color32[] charAreaPixels = new Color32[this.ScreenWidth * this.ScreenHeight];

        int charAreaUnityBottomY = TextureHeight - this.centerStartY - this.ScreenHeight;

        // --- Step 3: Extract the character display area from allTexturePixels into charAreaPixels ---
        for (int y_offset_in_char_area = 0; y_offset_in_char_area < this.ScreenHeight; y_offset_in_char_area++)
        {
            int y_in_allTexture = charAreaUnityBottomY + y_offset_in_char_area;
            int sourceRowStartIndexInAllPixels = y_in_allTexture * TextureWidth + this.centerStartX;
            int destRowStartIndexInCharArea = y_offset_in_char_area * this.ScreenWidth;
            Array.Copy(allTexturePixels, sourceRowStartIndexInAllPixels,
                       charAreaPixels, destRowStartIndexInCharArea,
                       this.ScreenWidth);
        }

        // --- Step 4: Perform the scroll operation within the contiguous charAreaPixels ---
        // To scroll content UP on the screen:
        // Data from the bottom (CharactersYCount - 1) lines is shifted "up" one slot in the array.
        // The top visual line's data is overwritten.
        // The bottom visual line's original space in the array becomes the new empty line.

        // Source: Starts at the beginning of charAreaPixels (bottom-most visual line's data).
        int sourceStartIndexInCharArea = 0;
        // Destination: Starts one character-line-height "up" in the array.
        int destStartIndexInCharArea = pixelsPerCharRowStrip;
        // Length: The total number of pixels for (CharactersYCount - 1) lines.
        int numPixelsToCopyForScroll = (CharactersYCount - 1) * pixelsPerCharRowStrip;

        if (CharactersYCount > 1 && numPixelsToCopyForScroll > 0) // Ensure there's something to scroll
        {
            Array.Copy(charAreaPixels, sourceStartIndexInCharArea,
                       charAreaPixels, destStartIndexInCharArea,
                       numPixelsToCopyForScroll);
        }

        // --- Step 5: Clear the new bottom character line ---
        // This is the first block of pixels in charAreaPixels, representing the bottom-most visual line.
        if (CharactersYCount > 0) // Ensure there's at least one line to clear
        {
            Array.Fill(charAreaPixels, this.charBackgroundColor, 0, pixelsPerCharRowStrip);
        }

        // --- Step 6: Set the modified charAreaPixels back to the correct rectangle on the screenTexture ---
        screenTexture.SetPixels32(this.centerStartX, charAreaUnityBottomY,
                                    this.ScreenWidth, this.ScreenHeight,
                                    charAreaPixels);

        needsDraw = true;
    }
    public ScreenGenerator ClearBackground()
    {
        if (screenTexture != null)
        {
            Color32[] pixels = screenTexture.GetPixels32(); // Gets all pixels of the texture
            Array.Fill(pixels, Skin.BorderColor);
            screenTexture.SetPixels32(pixels);
            needsDraw = true;
        }
        return this;
    }

    public ScreenGenerator Clear()
    {
        if (screenTexture == null)
        {
            createTexture(); // This will also call ResetColors if texture was null
        }

        // If Clear is called, it should also reset colorsBackgroundMatrix if charBackgroundColor changed
        // This is handled if ResetColors() is called when charBackgroundColor is set, or ensure
        // colorsBackgroundMatrix is always up-to-date with the current charBackgroundColor.
        // For now, ResetColors() ensures colorsBackgroundMatrix is based on Skin defaults.
        // If charBackgroundColor can change independently and Clear() should use the *current* one:
        if (colorsBackgroundMatrix.Length > 0 && colorsBackgroundMatrix[0].Equals(charBackgroundColor) == false)
        {
            Array.Fill(colorsBackgroundMatrix, charBackgroundColor); // Update if different
        }


        ClearBackground(); // Clear entire texture with border color first

        // Then draw the character area with its background
        screenTexture.SetPixels32(centerStartX, centerStartY, // These are top-left pixel coords
                                ScreenWidth, ScreenHeight,    // Pixel dimensions of char area
                                colorsBackgroundMatrix);      // Background colors for char area
        needsDraw = true;
        Locate(0, 0);
        return this;
    }

    public ScreenGenerator PrintCentered(int y, string text, bool inverted = false)
    {
        if (screenTexture == null)
            return this;

        if (y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintCentered] Invalid parameters y: ({y})");
            return this;
        }
        // Strip newlines from text for centering calculation as it's for a single line.
        string singleLineText = text.Replace("\n", "").Replace("\r", "");
        int x = (CharactersXCount - singleLineText.Length) / 2;
        if (x < 0) x = 0; // If text is too long, start at column 0

        Print(x, y, text, inverted); // Use original text for Print to handle any intended newlines
        return this;
    }

    public ScreenGenerator PrintLine(int y, bool inverted, char c = '-')
    {
        if (screenTexture == null)
            return this;
        if (y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintLine] Invalid parameters y: ({y})");
            return this;
        }
        // Create a string for the full width
        string text = new string(c, CharactersXCount);
        Print(0, y, text, inverted);
        return this;
    }

    // ... (DrawPixel, DrawPoint, DrawLine, DrawHorizontalLine, DrawVerticalLine, DrawBox, DrawCircle, DrawOval, DrawEllipsePoints)
    // ... (TryGetCharPixelPosition, GetCharPixelPosition, DrawImageRescaled, DrawTextureRescaled)
    // These graphic methods are assumed to be correct as per the original problem statement, focusing on text printing and scrolling.
    // Ensure DrawPixel correctly handles Y-coordinate flipping for Unity's bottom-left texture origin if it's not already.
    // The provided DrawPixel seems to handle this: int unityY = TextureHeight - 1 - y;

    /// <summary>
    /// Internal helper to draw a single pixel directly onto the screen texture.
    /// Performs bounds checking against the full texture dimensions AND
    /// flips the Y-coordinate to match Unity's bottom-left origin convention.
    /// </summary>
    /// <param name="x">X coordinate within the texture (0 to TextureWidth-1, assuming 0 is left).</param>
    /// <param name="y">Y coordinate within the texture (0 to TextureHeight-1, assuming 0 is TOP).</param>
    /// <param name="color">The color of the pixel.</param>
    private void DrawPixel(int x, int y, Color32 color)
    {
        // Check bounds using the input coordinate convention (0,0 is top-left)
        if (screenTexture != null && x >= 0 && x < TextureWidth && y >= 0 && y < TextureHeight)
        {
            // Flip the Y coordinate for SetPixel (0,0 is bottom-left)
            int unityY = TextureHeight - 1 - y;
            screenTexture.SetPixel(x, unityY, color);
            needsDraw = true; // Mark texture as modified
        }
    }
    public ScreenGenerator DrawPoint(int x, int y, Color32 color)
    {
        DrawPixel(x, y, color);
        return this;
    }
    public ScreenGenerator DrawLine(int x1, int y1, int x2, int y2, Color32 color)
    {
        if (screenTexture == null) return this;

        int dx = Mathf.Abs(x2 - x1);
        int dy = -Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx + dy;

        int currentX = x1;
        int currentY = y1;

        while (true)
        {
            DrawPixel(currentX, currentY, color);
            if (currentX == x2 && currentY == y2) break;
            int e2 = 2 * err;
            if (e2 >= dy)
            {
                if (currentX == x2) break;
                err += dy;
                currentX += sx;
            }
            if (e2 <= dx)
            {
                if (currentY == y2) break;
                err += dx;
                currentY += sy;
            }
        }
        return this;
    }

    private void DrawHorizontalLine(int xStart, int xEnd, int y, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(xStart, xEnd);
        int end = Mathf.Max(xStart, xEnd);
        for (int x = start; x <= end; x++)
        {
            DrawPixel(x, y, color);
        }
    }
    private void DrawVerticalLine(int x, int yStart, int yEnd, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(yStart, yEnd);
        int end = Mathf.Max(yStart, yEnd);
        for (int y = start; y <= end; y++)
        {
            DrawPixel(x, y, color);
        }
    }

    public ScreenGenerator DrawBox(int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || width <= 0 || height <= 0) return this;

        int x2 = x + width - 1;
        int y2 = y + height - 1;

        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            int fillStartX = x;
            int fillStartY = y;
            int clippedStartX = Mathf.Max(fillStartX, 0);
            int clippedStartY = Mathf.Max(fillStartY, 0);
            int clippedEndX = Mathf.Min(fillStartX + width, TextureWidth);
            int clippedEndY = Mathf.Min(fillStartY + height, TextureHeight);

            int clippedWidth = clippedEndX - clippedStartX;
            int clippedHeight = clippedEndY - clippedStartY;

            if (clippedWidth > 0 && clippedHeight > 0)
            {
                Color32[] fillPixels = new Color32[clippedWidth * clippedHeight];
                Array.Fill(fillPixels, fillCol);
                int unityStartY = TextureHeight - clippedStartY - clippedHeight;

                try
                {
                    screenTexture.SetPixels32(clippedStartX, unityStartY, clippedWidth, clippedHeight, fillPixels);
                    needsDraw = true;
                }
                catch (UnityException ex)
                {
                    Debug.LogError($"[ScreenGenerator.DrawBox] SetPixels32 failed: {ex.Message}. Falling back to pixel-by-pixel fill.");
                    for (int iy = clippedStartY; iy < clippedEndY; iy++)
                    {
                        for (int ix = clippedStartX; ix < clippedEndX; ix++)
                        {
                            DrawPixel(ix, iy, fillCol);
                        }
                    }
                }
            }
        }

        if (borderColor.a > 0)
        {
            DrawHorizontalLine(x, x2, y, borderColor);
            DrawHorizontalLine(x, x2, y2, borderColor);
            if (height > 1)
            {
                DrawVerticalLine(x, y + 1, y2 - 1, borderColor);
                DrawVerticalLine(x2, y + 1, y2 - 1, borderColor);
            }
            else if (height == 1 && width > 1) { }
            else if (width == 1 && height > 1)
            {
                DrawVerticalLine(x, y, y2, borderColor);
            }
            else if (width == 1 && height == 1)
            {
                DrawPixel(x, y, borderColor);
            }
        }
        return this;
    }
    public ScreenGenerator DrawCircle(int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radius < 0) return this;
        if (radius == 0)
        {
            if (filled && fillColor.HasValue) DrawPixel(centerX, centerY, fillColor.Value);
            else if (borderColor.a > 0) DrawPixel(centerX, centerY, borderColor);
            return this;
        }

        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rSquared = (float)radius * radius;
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                float yOffsetSquared = (float)yOffset * yOffset;
                if (yOffsetSquared <= rSquared)
                {
                    int span = (int)Mathf.Sqrt(rSquared - yOffsetSquared);
                    int y = centerY + yOffset;
                    DrawHorizontalLine(centerX - span, centerX + span, y, fillCol);
                }
            }
        }
        if (borderColor.a > 0)
        {
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do
            {
                DrawPixel(centerX + x, centerY + y, borderColor);
                DrawPixel(centerX + x, centerY - y, borderColor);
                DrawPixel(centerX - x, centerY + y, borderColor);
                DrawPixel(centerX - x, centerY - y, borderColor);
                DrawPixel(centerX + y, centerY + x, borderColor);
                DrawPixel(centerX + y, centerY - x, borderColor);
                DrawPixel(centerX - y, centerY + x, borderColor);
                DrawPixel(centerX - y, centerY - x, borderColor);
                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            } while (x <= y);
        }
        return this;
    }
    public ScreenGenerator DrawOval(int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radiusX < 0 || radiusY < 0) return this;
        if (radiusX == 0 && radiusY == 0)
        {
            if (filled && fillColor.HasValue) DrawPixel(centerX, centerY, fillColor.Value);
            else if (borderColor.a > 0) DrawPixel(centerX, centerY, borderColor);
            return this;
        }
        if (radiusX == 0) { DrawVerticalLine(centerX, centerY - radiusY, centerY + radiusY, filled && fillColor.HasValue ? fillColor.Value : borderColor); return this; }
        if (radiusY == 0) { DrawHorizontalLine(centerX - radiusX, centerX + radiusX, centerY, filled && fillColor.HasValue ? fillColor.Value : borderColor); return this; }

        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rxSq_float = (float)radiusX * radiusX; // Use float for Sqrt precision
            float rySq_float = (float)radiusY * radiusY;

            for (int yOffset = -radiusY; yOffset <= radiusY; yOffset++)
            {
                float yTerm = (float)(yOffset * yOffset) / rySq_float;
                if (yTerm <= 1.0f)
                {
                    // Ensure Sqrt argument is non-negative, calculate span
                    int span = (int)(radiusX * Mathf.Sqrt(Mathf.Max(0f, 1.0f - yTerm)));
                    int y = centerY + yOffset;
                    DrawHorizontalLine(centerX - span, centerX + span, y, fillCol);
                }
            }
        }

        if (borderColor.a > 0)
        {
            long rxSq = (long)radiusX * radiusX;
            long rySq = (long)radiusY * radiusY;
            long twoRxSq = 2 * rxSq;
            long twoRySq = 2 * rySq;
            long p;
            int x = 0;
            int y = radiusY;
            long px = 0;
            long py = twoRxSq * y;

            DrawEllipsePoints(centerX, centerY, x, y, borderColor);

            p = (long)Math.Round(rySq - (rxSq * radiusY) + (0.25 * rxSq));
            while (px < py)
            {
                x++;
                px += twoRySq;
                if (p < 0)
                {
                    p += rySq + px;
                }
                else
                {
                    y--;
                    py -= twoRxSq;
                    p += rySq + px - py;
                }
                DrawEllipsePoints(centerX, centerY, x, y, borderColor);
            }

            p = (long)Math.Round(rySq * (x + 0.5) * (x + 0.5) + rxSq * (y - 1) * (y - 1) - rxSq * rySq);
            while (y > 0)
            {
                y--;
                py -= twoRxSq;
                if (p > 0)
                {
                    p += rxSq - py;
                }
                else
                {
                    x++;
                    px += twoRySq;
                    p += rxSq - py + px;
                }
                DrawEllipsePoints(centerX, centerY, x, y, borderColor);
            }
        }
        return this;
    }
    private void DrawEllipsePoints(int cx, int cy, int x, int y, Color32 color)
    {
        DrawPixel(cx + x, cy + y, color);
        DrawPixel(cx - x, cy + y, color);
        DrawPixel(cx + x, cy - y, color);
        DrawPixel(cx - x, cy - y, color);
    }
    public bool TryGetCharPixelPosition(int charX, int charY, out int pixelX, out int pixelY)
    {
        pixelX = -1;
        pixelY = -1;

        if (Skin == null || Skin.Font == null)
        {
            Debug.LogError("[ScreenGenerator.TryGetCharPixelPosition] Skin or Font not initialized! Cannot calculate pixel position.");
            return false;
        }
        // Use existing member variables centerStartX, centerStartY which are calculated in createTexture
        // and ScreenWidth, ScreenHeight which are char area pixel dimensions
        int charAreaWidthPixels = this.ScreenWidth; // CharactersXCount * Skin.Font.CharactersWidth;
        int charAreaHeightPixels = this.ScreenHeight; // CharactersYCount * Skin.Font.CharactersHeight;

        int startX = this.centerStartX; //(TextureWidth - charAreaWidthPixels) / 2;
        int startY = this.centerStartY; //(TextureHeight - charAreaHeightPixels) / 2;

        int relativePixelX = charX * Skin.Font.CharactersWidth;
        int relativePixelY = charY * Skin.Font.CharactersHeight;

        pixelX = startX + relativePixelX;
        pixelY = startY + relativePixelY;
        return true;
    }
    public Vector2Int GetCharPixelPosition(int charX, int charY)
    {
        if (TryGetCharPixelPosition(charX, charY, out int px, out int py))
        {
            return new Vector2Int(px, py);
        }
        else
        {
            return new Vector2Int(-1, -1);
        }
    }
    public ScreenGenerator DrawImageRescaled(int x, int y, int width, int height, Color32[] pixels, int pixelsWidth, int pixelsHeight)
    {
        if (screenTexture == null) { Debug.LogError("[DrawImageRescaled] Screen texture is not initialized."); return this; }
        if (pixels == null || pixels.Length == 0) { Debug.LogWarning("[DrawImageRescaled] Input pixels array is null or empty."); return this; }
        if (width <= 0 || height <= 0) { return this; }
        if (pixelsWidth <= 0 || pixelsHeight <= 0) { Debug.LogWarning($"[DrawImageRescaled] Invalid source image dimensions ({pixelsWidth}x{pixelsHeight})."); return this; }
        if (pixels.Length != pixelsWidth * pixelsHeight) { Debug.LogWarning($"[DrawImageRescaled] Source pixels array length ({pixels.Length}) does not match provided dimensions ({pixelsWidth}x{pixelsHeight}={pixelsWidth * pixelsHeight}). Proceeding cautiously."); }

        float xScale = (float)pixelsWidth / width;
        float yScale = (float)pixelsHeight / height;

        int startScreenX = Mathf.Max(x, 0);
        int startScreenY = Mathf.Max(y, 0);
        int endScreenX = Mathf.Min(x + width, TextureWidth);
        int endScreenY = Mathf.Min(y + height, TextureHeight);

        for (int destY = startScreenY; destY < endScreenY; destY++)
        {
            int relativeY = destY - y;
            int sourceY_topDown = (int)((relativeY + 0.5f) * yScale);
            sourceY_topDown = Mathf.Clamp(sourceY_topDown, 0, pixelsHeight - 1);
            int sourceY_forArrayIndex = pixelsHeight - 1 - sourceY_topDown;

            for (int destX = startScreenX; destX < endScreenX; destX++)
            {
                int relativeX = destX - x;
                int sourceX = (int)((relativeX + 0.5f) * xScale);
                sourceX = Mathf.Clamp(sourceX, 0, pixelsWidth - 1);
                int sourceIndex = sourceY_forArrayIndex * pixelsWidth + sourceX;

                if (sourceIndex >= 0 && sourceIndex < pixels.Length)
                {
                    DrawPixel(destX, destY, pixels[sourceIndex]);
                }
            }
        }
        return this;
    }
    public ScreenGenerator DrawTextureRescaled(int x, int y, int width, int height, Texture2D sourceTexture)
    {
        if (sourceTexture == null) { Debug.LogWarning("[DrawTextureRescaled] Source Texture2D is null."); return this; }
        if (!sourceTexture.isReadable) { Debug.LogError($"[DrawTextureRescaled] Source Texture '{sourceTexture.name}' is not readable. Enable 'Read/Write Enabled'."); return this; }

        Color32[] sourcePixels;
        try { sourcePixels = sourceTexture.GetPixels32(); }
        catch (UnityException ex) { Debug.LogError($"[DrawTextureRescaled] Failed to GetPixels32 from '{sourceTexture.name}': {ex.Message}"); return this; }

        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;
        return DrawImageRescaled(x, y, width, height, sourcePixels, sourceWidth, sourceHeight);
    }
}