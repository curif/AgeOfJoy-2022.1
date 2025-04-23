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

    private int ScreenWidth; // Width of the target texture
    private int ScreenHeight; // Height of the target texture
    private int centerStartX;
    private int centerStartY;
    private int lastX, lastY;
    private int characterAreaWidthPixels; // Width of the character area in pixels
    private int characterAreaHeightPixels; // Height of the character area in pixels
    private int characterAreaStartX; // Top-left X pixel coord of character area within texture
    private int characterAreaStartY; // Top-left Y pixel coord of character area within texture

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
        { "lime", new Color32(0, 255, 0, 255) }, // Same as green, but included for completeness
        { "purple", new Color32(128, 0, 128, 255) },
        { "teal", new Color32(0, 128, 128, 255) },
        { "maroon", new Color32(128, 0, 0, 255) },
        { "navy", new Color32(0, 0, 128, 255) },
        { "olive", new Color32(128, 128, 0, 255) },
        { "silver", new Color32(192, 192, 192, 255) },
        { "gray", new Color32(128, 128, 128, 255) },
        { "brown", new Color32(165, 42, 42, 255) },
        { "aqua", new Color32(0, 255, 255, 255) }, // Same as cyan, but included for completeness
        { "fuchsia", new Color32(255, 0, 255, 255) }, // Same as magenta, but included for completeness
        { "lightgray", new Color32(211, 211, 211, 255) },
        { "darkgray", new Color32(169, 169, 169, 255) }
        // You can add more color names and their corresponding Color32 values here
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
        Array.Fill(colorsBackgroundMatrix, charBackgroundColor);

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

        ScreenWidth = CharactersXCount * Skin.Font.CharactersWidth;  // Width of the target texture
        ScreenHeight = CharactersYCount * Skin.Font.CharactersHeight; // Height of the target texture

        // Create an array of colors to fill the centered background in the middle of the texture
        // used to CLEAR faster. Loaded on ResetColors()
        colorsBackgroundMatrix = new Color32[ScreenWidth * ScreenHeight];

        // Calculate the position of the centered area based on the whole texture size
        centerStartX = (TextureWidth - ScreenWidth) / 2;
        centerStartY = (TextureHeight - ScreenHeight) / 2;

        // Create the target texture with the specified width and height, no mips
        screenTexture = new Texture2D(TextureWidth, TextureHeight, TextureFormat.RGBA32, false);
        screenTexture.name = "computer_screen";

        //screenTexture.wrapMode = TextureWrapMode.Clamp;
        screenTexture.filterMode = FilterMode.Bilinear;
        screenTexture.anisoLevel = 0;

        Skin.Font.setOffsets(centerStartX, centerStartY);

        return screenTexture;
    }

    public ScreenGenerator ActivateShader(ShaderScreenBase changeShader)
    {
        createTexture();
        shader = changeShader;
        shader.Activate(screenTexture);
        return this;
    }

    public void Update()
    {
        shader?.Update();
        return;
    }

    // copies the texture to the gpu if it was modified
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

    // The method that prints a single character to the screen
    public ScreenGenerator PrintChar(int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        if (screenTexture == null)
            return this;

        // Check if the coordinates and the character number are valid
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintChar] Invalid parameters x,y: ({x},{y}), charNum: {charNum}");
            return this;
        }

        Skin.Font.PrintChar(screenTexture, x, y, charNum, fgColor, bgColor, translate);
        needsDraw = true;
        return this;
    }

    // The method that prints a string of characters to the screen
    // Returns the number of vertical lines printed
    public int Print(int x, int y, string text, bool inverted = false)
    {
        //ConfigManager.WriteConsoleError($"[ScreenGenerator.Print] {text} {x}x{y}");

        Color32 fgColor = inverted ? charBackgroundColor : charForegroundColor;
        Color32 bgColor = inverted ? charForegroundColor : charBackgroundColor;

        if (screenTexture == null)
            return 0;

        // Check if the coordinates are valid
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.Print] Invalid parameters for Print method, x,y: ({x},{y})");
            return 0;
        }

        // Loop through all the characters in the text string
        int startY = y;
        int i = 0;
        while (i < text.Length)
        {
            // Check for escape character and make sure it's not the last char
            char c = text[i++];
            if (c == '\n')
            {
                NextLine(ref x, ref y);
            }
            else if (c == '\\')
            {
                if (i < text.Length)
                {
                    StringBuilder strIndex = new StringBuilder();
                    // Find the end of the number sequence
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
                    PrintCharPosition((char)index, ref x, ref y, fgColor, bgColor, false);

                }
                else
                {
                    PrintCharPosition('\\', ref x, ref y, fgColor, bgColor);
                }
            }
            else
            {
                PrintCharPosition(c, ref x, ref y, fgColor, bgColor);
            }

            //ConfigManager.WriteConsole($"[ScreenGenerator.Print]char:[{c}] position: {index}");
        }
        //ready for the next line
        lastY++;
        lastX = 0;
        
        if (lastY > ScreenHeight)
            lastY = ScreenHeight;

        return (y - startY) + 1;
    }

    public int Print(string text, bool inverted = false)
    { 
        return Print(lastX, lastY, text, inverted);
    }

    public void Locate(int x, int y)
    {
        // Check if the coordinates are valid
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
            return;
        lastX = x; lastY = y;
    }
    private void PrintCharPosition(char charNum, ref int x, ref int y, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        PrintChar(x, y, charNum, fgColor, bgColor, translate);

        x++;

        if (x >= CharactersXCount)
        {
            NextLine(ref x, ref y);
        }

        lastY = y;
        lastX = x;
    }

    private void NextLine(ref int x, ref int y)
    {
        x = 0;
        y++;
    }

    public ScreenGenerator ClearBackground()
    {
        if (screenTexture != null)
        {
            // Create a new array of color data for the texture
            Color32[] pixels = screenTexture.GetPixels32();
            Array.Fill(pixels, Skin.BorderColor);

            // Apply the modified colors back to the texture
            screenTexture.SetPixels32(pixels);

            needsDraw = true;
        }
        return this;
    }

    // The method that clears the screen with a given color or cyan by default
    public ScreenGenerator Clear()
    {
        // Fill the screen texture with the given color
        if (screenTexture == null)
        {
            createTexture();
            ClearBackground();
        }

        screenTexture.SetPixels32(centerStartX, centerStartY,
                                ScreenWidth, ScreenHeight,
                                colorsBackgroundMatrix);
        needsDraw = true;
        Locate(0, 0);
        return this;
    }

    // The method that prints a string of characters to the center of the X axis
    public ScreenGenerator PrintCentered(int y, string text, bool inverted = false)
    {
        if (screenTexture == null)
            return this;

        // Check if the y coordinate is valid
        if (y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintCentered] Invalid parameters y: ({y})");
            return this;
        }

        // Calculate the x coordinate that will center the text
        int x = (CharactersXCount - text.Length) / 2;

        // Print the text using the Print method with the calculated x coordinate
        Print(x, y, text, inverted);

        return this;
    }

    // The method that prints the same character in a line
    public ScreenGenerator PrintLine(int y, bool inverted, char c = '-')
    {
        if (screenTexture == null)
            return this;
        // Check if the y coordinate is valid
        if (y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintLine] Invalid parameters y: ({y})");
            return this;
        }

        // Create a string of 40 characters using the given character
        string text = new string(c, CharactersXCount - 1);

        // Print the text using the Print method with the x coordinate of 0
        Print(0, y, text, inverted);
        return this;
    }
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
        // Else: Pixel is outside the drawable texture area, do nothing.
    }
    /// <summary>
    /// Draws a single point (pixel) at the specified coordinates within the texture space.
    /// </summary>
    /// <param name="x">X coordinate (pixel space, 0 to TextureWidth-1).</param>
    /// <param name="y">Y coordinate (pixel space, 0 to TextureHeight-1).</param>
    /// <param name="color">The color of the point.</param>
    public ScreenGenerator DrawPoint(int x, int y, Color32 color)
    {
        // Bounds check is handled by DrawPixel
        DrawPixel(x, y, color);
        return this;
    }
    /// <summary>
    /// Draws a line between two points using Bresenham's line algorithm.
    /// Coordinates are relative to the texture space (0,0 to TextureWidth-1, TextureHeight-1).
    /// </summary>
    /// <param name="x1">Start X coordinate.</param>
    /// <param name="y1">Start Y coordinate.</param>
    /// <param name="x2">End X coordinate.</param>
    /// <param name="y2">End Y coordinate.</param>
    /// <param name="color">The color of the line.</param>
    public ScreenGenerator DrawLine(int x1, int y1, int x2, int y2, Color32 color)
    {
        if (screenTexture == null) return this;

        int dx = Mathf.Abs(x2 - x1);
        int dy = -Mathf.Abs(y2 - y1); // Use negative dy for standard Bresenham
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx + dy;

        int currentX = x1;
        int currentY = y1;

        while (true)
        {
            DrawPixel(currentX, currentY, color); // DrawPixel handles bounds checking

            if (currentX == x2 && currentY == y2) break; // Reached the end

            int e2 = 2 * err;
            if (e2 >= dy) // Check against dy threshold
            {
                if (currentX == x2) break; // Prevent X overshoot if already at target X
                err += dy;
                currentX += sx;
            }
            if (e2 <= dx) // Check against dx threshold
            {
                if (currentY == y2) break; // Prevent Y overshoot if already at target Y
                err += dx;
                currentY += sy;
            }
        }
        // needsDraw is set by DrawPixel
        return this;
    }

    /// <summary>
    /// Draws a horizontal line efficiently. Internal helper.
    /// Coordinates are relative to the texture space.
    /// </summary>
    private void DrawHorizontalLine(int xStart, int xEnd, int y, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(xStart, xEnd);
        int end = Mathf.Max(xStart, xEnd);
        for (int x = start; x <= end; x++)
        {
            // DrawPixel handles bounds checking (y and x)
            DrawPixel(x, y, color);
        }
        // Optimization possible here using SetPixels32 for long lines within bounds
    }  
    /// <summary>
    /// Draws a vertical line efficiently. Internal helper.
    /// Coordinates are relative to the texture space.
    /// </summary>
    private void DrawVerticalLine(int x, int yStart, int yEnd, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(yStart, yEnd);
        int end = Mathf.Max(yStart, yEnd);
        for (int y = start; y <= end; y++)
        {
            // DrawPixel handles bounds checking (x and y)
            DrawPixel(x, y, color);
        }
        // Optimization possible here using SetPixels32 for long lines within bounds
    }

    /// <summary>
    /// Draws a rectangle (box). Coordinates are relative to the texture space (0,0 is top-left).
    /// </summary>
    /// <param name="x">Top-left X coordinate.</param>
    /// <param name="y">Top-left Y coordinate.</param>
    /// <param name="width">Width of the box in pixels.</param>
    /// <param name="height">Height of the box in pixels.</param>
    /// <param name="borderColor">Color of the border. Set Alpha to 0 to disable.</param>
    /// <param name="filled">If true, fill the box.</param>
    /// <param name="fillColor">Color to fill the box with (required if filled is true).</param>
    public ScreenGenerator DrawBox(int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || width <= 0 || height <= 0) return this;

        int x2 = x + width - 1;
        int y2 = y + height - 1; // y2 is the bottom-most row in our top-left convention

        // --- Fill ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;

            // Optimized Fill using SetPixels32
            int fillStartX = x;
            int fillStartY = y; // Top row in our convention

            // Clip fill area to texture bounds using our convention
            int clippedStartX = Mathf.Max(fillStartX, 0);
            int clippedStartY = Mathf.Max(fillStartY, 0); // Top row, clipped
            int clippedEndX = Mathf.Min(fillStartX + width, TextureWidth);
            int clippedEndY = Mathf.Min(fillStartY + height, TextureHeight); // Bottom row + 1, clipped

            int clippedWidth = clippedEndX - clippedStartX;
            int clippedHeight = clippedEndY - clippedStartY; // Height of the clipped area

            if (clippedWidth > 0 && clippedHeight > 0)
            {
                Color32[] fillPixels = new Color32[clippedWidth * clippedHeight];
                Array.Fill(fillPixels, fillCol);

                // Calculate the starting Y coordinate for SetPixels32 (bottom-left origin)
                // The bottom-left Y for the block corresponds to the highest Y value in Unity's space,
                // which is derived from the top-most Y value in our space (clippedStartY).
                // The block starts at clippedStartY (top) and extends for clippedHeight pixels down.
                // The bottom row in our space is clippedStartY + clippedHeight - 1.
                // Flipping this gives: TextureHeight - 1 - (clippedStartY + clippedHeight - 1)
                // Simplified: TextureHeight - clippedStartY - clippedHeight
                int unityStartY = TextureHeight - clippedStartY - clippedHeight;

                try
                {
                    // Pass the bottom-left starting X (clippedStartX) and the calculated bottom-left starting Y (unityStartY)
                    screenTexture.SetPixels32(clippedStartX, unityStartY, clippedWidth, clippedHeight, fillPixels);
                    needsDraw = true;
                }
                catch (UnityException ex)
                {
                    Debug.LogError($"[ScreenGenerator.DrawBox] SetPixels32 failed: {ex.Message}. Falling back to pixel-by-pixel fill.");
                    // Fallback (slower): Draw pixel by pixel within the clipped rect
                    // Uses the corrected DrawPixel, so Y-flip is automatic here.
                    for (int iy = clippedStartY; iy < clippedEndY; iy++)
                    { // iy is top-down
                        for (int ix = clippedStartX; ix < clippedEndX; ix++)
                        {
                            DrawPixel(ix, iy, fillCol);
                        }
                    }
                }
            }
        }

        // --- Border ---
        // Uses DrawHorizontalLine/DrawVerticalLine which rely on the corrected DrawPixel.
        // No changes needed here for the border logic itself.
        if (borderColor.a > 0)
        {
            DrawHorizontalLine(x, x2, y, borderColor); // Top (y=0 correctly handled by DrawPixel)
            DrawHorizontalLine(x, x2, y2, borderColor); // Bottom (y2 handled by DrawPixel)
            if (height > 1)
            {
                DrawVerticalLine(x, y + 1, y2 - 1, borderColor);
                DrawVerticalLine(x2, y + 1, y2 - 1, borderColor);
            }
            else if (height == 1 && width > 1)
            {
                // horizontal line already drew endpoints
            }
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

    /// <summary>
    /// Draws a circle using the Midpoint Circle Algorithm.
    /// Coordinates are relative to the texture space.
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radius">Radius in pixels.</param>
    /// <param name="borderColor">Color of the border. Set Alpha to 0 to disable.</param>
    /// <param name="filled">If true, fill the circle.</param>
    /// <param name="fillColor">Color to fill the circle with (required if filled is true).</param>
    public ScreenGenerator DrawCircle(int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radius < 0) return this; // Allow radius 0 (single point)
        if (radius == 0)
        {
            if (filled && fillColor.HasValue) DrawPixel(centerX, centerY, fillColor.Value);
            else if (borderColor.a > 0) DrawPixel(centerX, centerY, borderColor);
            return this;
        }

        // --- Fill (Scanline method) ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rSquared = (float)radius * radius;

            // Iterate through bounding box rows
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                // Calculate horizontal span (x) for this row using circle equation
                float yOffsetSquared = (float)yOffset * yOffset;
                // Ensure argument to Sqrt is non-negative
                if (yOffsetSquared <= rSquared)
                {
                    int span = (int)Mathf.Sqrt(rSquared - yOffsetSquared);
                    int y = centerY + yOffset;
                    // DrawHorizontalLine handles clipping the line to texture bounds
                    DrawHorizontalLine(centerX - span, centerX + span, y, fillCol);
                }
            }
        }

        // --- Border (Midpoint Circle Algorithm) ---
        if (borderColor.a > 0)
        {
            int d = (5 - radius * 4) / 4; // Initial decision parameter
            int x = 0;
            int y = radius;

            do
            {
                // Draw points in all 8 octants using DrawPixel (handles bounds)
                DrawPixel(centerX + x, centerY + y, borderColor);
                DrawPixel(centerX + x, centerY - y, borderColor);
                DrawPixel(centerX - x, centerY + y, borderColor);
                DrawPixel(centerX - x, centerY - y, borderColor);
                DrawPixel(centerX + y, centerY + x, borderColor);
                DrawPixel(centerX + y, centerY - x, borderColor);
                DrawPixel(centerX - y, centerY + x, borderColor);
                DrawPixel(centerX - y, centerY - x, borderColor);

                // Update decision parameter and coordinates
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
            } while (x <= y); // Continue until x surpasses y
        }

        // needsDraw is set by DrawPixel or DrawHorizontalLine
        return this;
    }


    /// <summary>
    /// Draws an ellipse (oval) using a modified Midpoint Algorithm.
    /// Coordinates are relative to the texture space.
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radiusX">Horizontal radius in pixels.</param>
    /// <param name="radiusY">Vertical radius in pixels.</param>
    /// <param name="borderColor">Color of the border. Set Alpha to 0 to disable.</param>
    /// <param name="filled">If true, fill the oval.</param>
    /// <param name="fillColor">Color to fill the oval with (required if filled is true).</param>
    public ScreenGenerator DrawOval(int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radiusX < 0 || radiusY < 0) return this; // Allow radius 0
        if (radiusX == 0 && radiusY == 0)
        {
            if (filled && fillColor.HasValue) DrawPixel(centerX, centerY, fillColor.Value);
            else if (borderColor.a > 0) DrawPixel(centerX, centerY, borderColor);
            return this;
        }
        // Handle lines if one radius is 0
        if (radiusX == 0) { DrawVerticalLine(centerX, centerY - radiusY, centerY + radiusY, filled && fillColor.HasValue ? fillColor.Value : borderColor); return this; }
        if (radiusY == 0) { DrawHorizontalLine(centerX - radiusX, centerX + radiusX, centerY, filled && fillColor.HasValue ? fillColor.Value : borderColor); return this; }


        // --- Fill (Scanline method) ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rxSq = (float)radiusX * radiusX;
            float rySq = (float)radiusY * radiusY;

            // Iterate through bounding box rows
            for (int yOffset = -radiusY; yOffset <= radiusY; yOffset++)
            {
                // Calculate horizontal span (x) for this row using ellipse equation
                float yTerm = (float)(yOffset * yOffset) / rySq;
                if (yTerm <= 1.0f) // Ensure argument to Sqrt is non-negative
                {
                    int span = (int)(radiusX * Mathf.Sqrt(1.0f - yTerm));
                    int y = centerY + yOffset;
                    // DrawHorizontalLine handles clipping the line to texture bounds
                    DrawHorizontalLine(centerX - span, centerX + span, y, fillCol);
                }
            }
        }

        // --- Border (Midpoint Ellipse Algorithm) ---
        if (borderColor.a > 0)
        {
            long rxSq = (long)radiusX * radiusX; // Use long to avoid overflow
            long rySq = (long)radiusY * radiusY;
            long twoRxSq = 2 * rxSq;
            long twoRySq = 2 * rySq;
            long p;
            int x = 0;
            int y = radiusY;
            long px = 0; // Stores 2 * rySq * x
            long py = twoRxSq * y; // Stores 2 * rxSq * y

            // Plot initial points
            DrawEllipsePoints(centerX, centerY, x, y, borderColor);

            // Region 1 (slope |m| < 1)
            p = (long)Math.Round(rySq - (rxSq * radiusY) + (0.25 * rxSq));
            while (px < py) // While in region 1
            {
                x++;
                px += twoRySq;
                if (p < 0) // Midpoint is inside ellipse
                {
                    p += rySq + px;
                }
                else // Midpoint is outside or on ellipse
                {
                    y--;
                    py -= twoRxSq;
                    p += rySq + px - py;
                }
                DrawEllipsePoints(centerX, centerY, x, y, borderColor);
            }

            // Region 2 (slope |m| >= 1)
            // Initial point for region 2 is the last point of region 1
            p = (long)Math.Round(rySq * (x + 0.5) * (x + 0.5) + rxSq * (y - 1) * (y - 1) - rxSq * rySq);
            while (y > 0) // While in region 2
            {
                y--;
                py -= twoRxSq;
                if (p > 0) // Midpoint is outside ellipse
                {
                    p += rxSq - py;
                }
                else // Midpoint is inside or on ellipse
                {
                    x++;
                    px += twoRySq;
                    p += rxSq - py + px;
                }
                DrawEllipsePoints(centerX, centerY, x, y, borderColor);
            }
        }

        // needsDraw is set by DrawPixel, DrawHorizontalLine, or DrawEllipsePoints
        return this;
    }

    // Helper to draw points for all four quadrants of the ellipse
    // Uses DrawPixel which handles texture bounds checking.
    private void DrawEllipsePoints(int cx, int cy, int x, int y, Color32 color)
    {
        DrawPixel(cx + x, cy + y, color);
        DrawPixel(cx - x, cy + y, color);
        DrawPixel(cx + x, cy - y, color);
        DrawPixel(cx - x, cy - y, color);
    }


    /// <summary>
    /// Translates character grid coordinates (e.g., column 10, row 5) to the
    /// top-left pixel coordinate of that character's cell within the full texture space.
    /// Calculates offsets dynamically based on current properties and Skin.Font.
    /// Assumes character grid (0,0) is top-left.
    /// Assumes texture pixel (0,0) is top-left for the result.
    /// </summary>
    /// <param name="charX">Character column index (0 to CharactersXCount-1).</param>
    /// <param name="charY">Character row index (0 to CharactersYCount-1).</param>
    /// <param name="pixelX">Output integer for the calculated absolute pixel X coordinate.</param>
    /// <param name="pixelY">Output integer for the calculated absolute pixel Y coordinate.</param>
    /// <returns>True if the calculation was successful (Skin and Font initialized), false otherwise.</returns>
    public bool TryGetCharPixelPosition(int charX, int charY, out int pixelX, out int pixelY)
    {
        pixelX = -1; // Default error values
        pixelY = -1;

        // Ensure necessary components are initialized
        if (Skin == null || Skin.Font == null)
        {
            Debug.LogError("[ScreenGenerator.TryGetCharPixelPosition] Skin or Font not initialized! Cannot calculate pixel position.");
            return false;
        }

        // 1 & 2. Calculate total character area dimensions dynamically
        int charAreaWidthPixels = CharactersXCount * Skin.Font.CharactersWidth;
        int charAreaHeightPixels = CharactersYCount * Skin.Font.CharactersHeight;

        // 3 & 4. Calculate character area offsets dynamically
        int startX = (TextureWidth - charAreaWidthPixels) / 2;
        int startY = (TextureHeight - charAreaHeightPixels) / 2;

        // 5. Calculate relative pixel offset within the character area
        int relativePixelX = charX * Skin.Font.CharactersWidth;
        int relativePixelY = charY * Skin.Font.CharactersHeight;

        // 6. Calculate absolute pixel coordinates
        pixelX = startX + relativePixelX;
        pixelY = startY + relativePixelY;

        // Optional boundary check for the *input* char coordinates
        // if (charX < 0 || charX >= CharactersXCount || charY < 0 || charY >= CharactersYCount) { ... }
        // Optional boundary check for the *output* pixel coordinates against TextureWidth/Height
        // if (pixelX < 0 || pixelX >= TextureWidth || pixelY < 0 || pixelY >= TextureHeight) { ... }

        return true;
    }

    /// <summary>
    /// Translates character grid coordinates to the top-left pixel coordinate
    /// of that character cell within the full texture space. Returns (-1,-1) if Skin/Font is not ready.
    /// Assumes (0,0) is the top-left character and top-left pixel.
    /// </summary>
    /// <param name="charX">Character column index.</param>
    /// <param name="charY">Character row index.</param>
    /// <returns>Vector2Int containing the calculated pixel coordinates (X, Y), or (-1,-1) on failure.</returns>
    public Vector2Int GetCharPixelPosition(int charX, int charY)
    {
        if (TryGetCharPixelPosition(charX, charY, out int px, out int py))
        {
            return new Vector2Int(px, py);
        }
        else
        {
            // Return an invalid value if Try method failed
            return new Vector2Int(-1, -1);
        }
    }

    /// <summary>
    /// Draws an image (provided as Color32 array) onto the screen texture, rescaling it
    /// using nearest-neighbor sampling to fit the specified destination area.
    /// Prioritizes speed over quality; pixelation is expected.
    /// Assumes screen coordinate (0,0) is top-left.
    /// Corrects for bottom-up ordering of GetPixels32 array.
    /// </summary>
    // ... (parameters remain the same) ...
    public ScreenGenerator DrawImageRescaled(int x, int y, int width, int height, Color32[] pixels, int pixelsWidth, int pixelsHeight)
    {
        // --- Input Validation --- (remains the same)
        if (screenTexture == null) { Debug.LogError("[DrawImageRescaled] Screen texture is not initialized."); return this; }
        if (pixels == null || pixels.Length == 0) { Debug.LogWarning("[DrawImageRescaled] Input pixels array is null or empty."); return this; }
        if (width <= 0 || height <= 0) { return this; } // Nothing to draw
        if (pixelsWidth <= 0 || pixelsHeight <= 0) { Debug.LogWarning($"[DrawImageRescaled] Invalid source image dimensions ({pixelsWidth}x{pixelsHeight})."); return this; }
        if (pixels.Length != pixelsWidth * pixelsHeight) { Debug.LogWarning($"[DrawImageRescaled] Source pixels array length ({pixels.Length}) does not match provided dimensions ({pixelsWidth}x{pixelsHeight}={pixelsWidth * pixelsHeight}). Proceeding cautiously."); }

        // --- Calculate Scaling Factors --- (remains the same)
        float xScale = (float)pixelsWidth / width;
        float yScale = (float)pixelsHeight / height;

        // --- Clip Destination Area --- (remains the same)
        int startScreenX = Mathf.Max(x, 0);
        int startScreenY = Mathf.Max(y, 0);
        int endScreenX = Mathf.Min(x + width, TextureWidth);
        int endScreenY = Mathf.Min(y + height, TextureHeight);

        // --- Iterate through DESTINATION pixels ---
        for (int destY = startScreenY; destY < endScreenY; destY++)
        {
            int relativeY = destY - y;
            // Calculate the corresponding source Y coordinate assuming top-down mapping (0 = top row)
            int sourceY_topDown = (int)((relativeY + 0.5f) * yScale);
            // Clamp this conceptual top-down Y to valid source image bounds
            sourceY_topDown = Mathf.Clamp(sourceY_topDown, 0, pixelsHeight - 1);

            // ***** FIX: Flip the source Y to access the correct row in the bottom-up pixels array *****
            int sourceY_forArrayIndex = pixelsHeight - 1 - sourceY_topDown;
            // ***** END FIX *****

            for (int destX = startScreenX; destX < endScreenX; destX++)
            {
                int relativeX = destX - x;
                // Calculate source X (remains the same)
                int sourceX = (int)((relativeX + 0.5f) * xScale);
                sourceX = Mathf.Clamp(sourceX, 0, pixelsWidth - 1);

                // --- Calculate Source Index and Draw ---
                // Use the *flipped* sourceY_forArrayIndex for indexing the bottom-up array
                int sourceIndex = sourceY_forArrayIndex * pixelsWidth + sourceX;

                if (sourceIndex >= 0 && sourceIndex < pixels.Length)
                {
                    DrawPixel(destX, destY, pixels[sourceIndex]); // DrawPixel handles *destination* Y-flip
                }
            }
        }
        return this;
    }

    // The DrawTextureRescaled method does NOT need changes, as it relies on the corrected DrawImageRescaled.
    public ScreenGenerator DrawTextureRescaled(int x, int y, int width, int height, Texture2D sourceTexture)
    {
        if (sourceTexture == null) { Debug.LogWarning("[DrawTextureRescaled] Source Texture2D is null."); return this; }
        if (!sourceTexture.isReadable) { Debug.LogError($"[DrawTextureRescaled] Source Texture '{sourceTexture.name}' is not readable. Enable 'Read/Write Enabled'."); return this; }

        Color32[] sourcePixels;
        try { sourcePixels = sourceTexture.GetPixels32(); }
        catch (UnityException ex) { Debug.LogError($"[DrawTextureRescaled] Failed to GetPixels32 from '{sourceTexture.name}': {ex.Message}"); return this; }

        int sourceWidth = sourceTexture.width;
        int sourceHeight = sourceTexture.height;

        // Calls the *corrected* DrawImageRescaled
        return DrawImageRescaled(x, y, width, height, sourcePixels, sourceWidth, sourceHeight);
    }


}
