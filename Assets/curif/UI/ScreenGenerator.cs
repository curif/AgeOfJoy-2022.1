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
    /// Internal helper to draw a single pixel onto the screen texture.
    /// Handles coordinate translation from character area space to texture space
    /// and performs bounds checking.
    /// </summary>
    /// <param name="x">X coordinate within the character pixel area (0 to characterAreaWidthPixels-1)</param>
    /// <param name="y">Y coordinate within the character pixel area (0 to characterAreaHeightPixels-1)</param>
    /// <param name="color">The color of the pixel.</param>
    private void DrawPixel(int x, int y, Color32 color)
    {
        if (screenTexture == null) return;

        // Translate coordinates from character area space to full texture space
        int texX = x + characterAreaStartX;
        int texY = y + characterAreaStartY;

        // Bounds check against the *entire* texture dimensions
        if (texX >= 0 && texX < TextureWidth && texY >= 0 && texY < TextureHeight)
        {
            screenTexture.SetPixel(texX, texY, color);
            needsDraw = true; // Mark texture as modified
        }
        // Else: Pixel is outside the drawable texture area, do nothing.
    }

    /// <summary>
    /// Draws a single point (pixel) at the specified coordinates within the character area.
    /// </summary>
    /// <param name="x">X coordinate (pixel space, 0 to characterAreaWidthPixels-1).</param>
    /// <param name="y">Y coordinate (pixel space, 0 to characterAreaHeightPixels-1).</param>
    /// <param name="color">The color of the point.</param>
    public ScreenGenerator DrawPoint(int x, int y, Color32 color)
    {
        // Optional: Bounds check against character area dimensions *before* translation
        if (x < 0 || x >= characterAreaWidthPixels || y < 0 || y >= characterAreaHeightPixels)
        {
            // ConfigManager.WriteConsole($"[ScreenGenerator.DrawPoint] Point ({x},{y}) outside character area.");
            return this; // Optionally log or handle out-of-bounds points
        }
        DrawPixel(x, y, color);
        return this;
    }

    /// <summary>
    /// Draws a line between two points using Bresenham's line algorithm.
    /// Coordinates are relative to the character pixel area.
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
        int dy = -Mathf.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx + dy;

        int currentX = x1;
        int currentY = y1;

        while (true)
        {
            DrawPixel(currentX, currentY, color); // Draw the current pixel

            if (currentX == x2 && currentY == y2) break; // Reached the end

            int e2 = 2 * err;
            if (e2 >= dy) // Error threshold crossed?
            {
                if (currentX == x2) break; // Prevent X overshoot
                err += dy;
                currentX += sx;
            }
            if (e2 <= dx) // Error threshold crossed?
            {
                if (currentY == y2) break; // Prevent Y overshoot
                err += dx;
                currentY += sy;
            }
        }
        needsDraw = true; // Ensure flag is set (though DrawPixel does it too)
        return this;
    }

    /// <summary>
    /// Draws a horizontal line efficiently. Internal helper.
    /// </summary>
    private void DrawHorizontalLine(int xStart, int xEnd, int y, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(xStart, xEnd);
        int end = Mathf.Max(xStart, xEnd);
        for (int x = start; x <= end; x++)
        {
            DrawPixel(x, y, color);
        }
        // Optimization possible here using SetPixels32 for long lines
    }

    /// <summary>
    /// Draws a vertical line efficiently. Internal helper.
    /// </summary>
    private void DrawVerticalLine(int x, int yStart, int yEnd, Color32 color)
    {
        if (screenTexture == null) return;
        int start = Mathf.Min(yStart, yEnd);
        int end = Mathf.Max(yStart, yEnd);
        for (int y = start; y <= end; y++)
        {
            DrawPixel(x, y, color);
        }
        // Optimization possible here using SetPixels32 for long lines
    }


    /// <summary>
    /// Draws a rectangle (box). Coordinates are relative to the character pixel area.
    /// </summary>
    /// <param name="x">Top-left X coordinate.</param>
    /// <param name="y">Top-left Y coordinate.</param>
    /// <param name="width">Width of the box in pixels.</param>
    /// <param name="height">Height of the box in pixels.</param>
    /// <param name="borderColor">Color of the border.</param>
    /// <param name="filled">If true, fill the box.</param>
    /// <param name="fillColor">Color to fill the box with (required if filled is true).</param>
    public ScreenGenerator DrawBox(int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || width <= 0 || height <= 0) return this;

        int x2 = x + width - 1;
        int y2 = y + height - 1;

        // --- Fill ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;

            // Optimized Fill using SetPixels32 (Much faster than DrawPixel per pixel)
            int fillWidth = width;
            int fillHeight = height;
            int fillStartX = x;
            int fillStartY = y;

            // Clip fill area to character area bounds
            int fillEndX = fillStartX + fillWidth;
            int fillEndY = fillStartY + fillHeight;

            fillStartX = Mathf.Max(fillStartX, 0);
            fillStartY = Mathf.Max(fillStartY, 0);
            fillEndX = Mathf.Min(fillEndX, characterAreaWidthPixels);
            fillEndY = Mathf.Min(fillEndY, characterAreaHeightPixels);

            fillWidth = fillEndX - fillStartX;
            fillHeight = fillEndY - fillStartY;

            if (fillWidth > 0 && fillHeight > 0)
            {
                Color32[] fillPixels = new Color32[fillWidth * fillHeight];
                Array.Fill(fillPixels, fillCol);

                // Translate fill start coordinates to texture space
                int texFillStartX = fillStartX + characterAreaStartX;
                int texFillStartY = fillStartY + characterAreaStartY;

                // Check if fill area is within texture bounds before SetPixels32
                if (texFillStartX >= 0 && texFillStartX + fillWidth <= TextureWidth &&
                    texFillStartY >= 0 && texFillStartY + fillHeight <= TextureHeight)
                {
                    screenTexture.SetPixels32(texFillStartX, texFillStartY, fillWidth, fillHeight, fillPixels);
                    needsDraw = true;
                }
                else
                {
                    // Fallback or partial fill if SetPixels32 cannot be used due to boundary issues
                    // (could implement pixel-by-pixel DrawPixel loop here if needed for partial fills crossing texture edge)
                    // For simplicity, we'll just log if the full optimized fill isn't possible.
                    // ConfigManager.WriteConsole($"[ScreenGenerator.DrawBox] Fill area ({fillStartX},{fillStartY} -> {fillEndX},{fillEndY}) partially outside texture. Optimized fill skipped.");
                    // Simple fallback (slower):
                    for (int iy = fillStartY; iy < fillEndY; iy++)
                    {
                        for (int ix = fillStartX; ix < fillEndX; ix++)
                        {
                            DrawPixel(ix, iy, fillCol);
                        }
                    }
                }
            }

            /* // Slow fill (pixel by pixel) - Keep for reference
            for (int iy = y; iy <= y2; iy++)
            {
                for (int ix = x; ix <= x2; ix++)
                {
                    DrawPixel(ix, iy, fillCol);
                }
            }
            */
        }

        // --- Border ---
        // Draw border after fill so it's on top
        if (borderColor.a > 0) // Check if border color is visible
        {
            // Use efficient horizontal/vertical line helpers
            DrawHorizontalLine(x, x2, y, borderColor);      // Top
            DrawHorizontalLine(x, x2, y2, borderColor);     // Bottom
            DrawVerticalLine(x, y + 1, y2 - 1, borderColor); // Left (avoid corners)
            DrawVerticalLine(x2, y + 1, y2 - 1, borderColor);// Right (avoid corners)

            // DrawLine version (slightly less efficient for axis-aligned lines):
            // DrawLine(x, y, x2, y, borderColor);       // Top
            // DrawLine(x, y2, x2, y2, borderColor);    // Bottom
            // DrawLine(x, y + 1, x, y2 - 1, borderColor); // Left (avoid corners)
            // DrawLine(x2, y + 1, x2, y2 - 1, borderColor); // Right (avoid corners)
        }


        needsDraw = true;
        return this;
    }


    /// <summary>
    /// Draws a circle using the Midpoint Circle Algorithm.
    /// Coordinates are relative to the character pixel area.
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radius">Radius in pixels.</param>
    /// <param name="borderColor">Color of the border.</param>
    /// <param name="filled">If true, fill the circle.</param>
    /// <param name="fillColor">Color to fill the circle with (required if filled is true).</param>
    public ScreenGenerator DrawCircle(int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radius <= 0) return this;

        // --- Fill (Scanline method) ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            int rSquared = radius * radius;

            // Iterate through bounding box rows
            for (int y = -radius; y <= radius; y++)
            {
                // Calculate horizontal span (x) for this row using circle equation
                // x^2 + y^2 = r^2  => x^2 = r^2 - y^2 => x = +/- sqrt(r^2 - y^2)
                int span = (int)Mathf.Sqrt(rSquared - y * y);
                DrawHorizontalLine(centerX - span, centerX + span, centerY + y, fillCol);
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
                // Draw points in all 8 octants
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
            } while (x <= y);
        }

        needsDraw = true;
        return this;
    }


    /// <summary>
    /// Draws an ellipse (oval) using a modified Midpoint Algorithm.
    /// Coordinates are relative to the character pixel area.
    /// </summary>
    /// <param name="centerX">Center X coordinate.</param>
    /// <param name="centerY">Center Y coordinate.</param>
    /// <param name="radiusX">Horizontal radius in pixels.</param>
    /// <param name="radiusY">Vertical radius in pixels.</param>
    /// <param name="borderColor">Color of the border.</param>
    /// <param name="filled">If true, fill the oval.</param>
    /// <param name="fillColor">Color to fill the oval with (required if filled is true).</param>
    public ScreenGenerator DrawOval(int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture == null || radiusX <= 0 || radiusY <= 0) return this;

        // --- Fill (Scanline method) ---
        if (filled && fillColor.HasValue)
        {
            Color32 fillCol = fillColor.Value;
            float rxSq = radiusX * radiusX;
            float rySq = radiusY * radiusY;

            // Iterate through bounding box rows
            for (int y = -radiusY; y <= radiusY; y++)
            {
                // Calculate horizontal span (x) for this row using ellipse equation
                // (x/rx)^2 + (y/ry)^2 = 1 => x^2 / rx^2 = 1 - y^2 / ry^2
                // x^2 = rx^2 * (1 - y^2 / ry^2)
                // x = +/- rx * sqrt(1 - y^2 / ry^2)
                // Ensure argument to Sqrt is non-negative
                float yTerm = (float)(y * y) / rySq;
                if (yTerm <= 1.0f)
                {
                    int span = (int)(radiusX * Mathf.Sqrt(1.0f - yTerm));
                    DrawHorizontalLine(centerX - span, centerX + span, centerY + y, fillCol);
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
            long px = 0;
            long py = twoRxSq * y;

            // Plot initial points
            DrawEllipsePoints(centerX, centerY, x, y, borderColor);

            // Region 1
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

            // Region 2
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

        needsDraw = true;
        return this;
    }

    // Helper to draw points for all four quadrants of the ellipse
    private void DrawEllipsePoints(int cx, int cy, int x, int y, Color32 color)
    {
        DrawPixel(cx + x, cy + y, color);
        DrawPixel(cx - x, cy + y, color);
        DrawPixel(cx + x, cy - y, color);
        DrawPixel(cx - x, cy - y, color);
    }

}
