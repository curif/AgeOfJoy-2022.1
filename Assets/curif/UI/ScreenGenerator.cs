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

    private ShaderScreenBase shader; // This should now refer to YOUR project's ShaderScreenBase
    public ScreenGeneratorSkin Skin; // This should now refer to YOUR project's ScreenGeneratorSkin

    public static readonly Dictionary<string, Color32> ColorMap = new Dictionary<string, Color32>()
    {
        { "none", new Color32(0, 0, 0, 0) }, { "red", new Color32(255, 0, 0, 255) },
        { "green", new Color32(0, 255, 0, 255) }, { "blue", new Color32(0, 0, 255, 255) },
        { "yellow", new Color32(255, 255, 0, 255) }, { "cyan", new Color32(0, 255, 255, 255) },
        { "magenta", new Color32(255, 0, 255, 255) }, { "white", new Color32(255, 255, 255, 255) },
        { "black", new Color32(0, 0, 0, 255) }, { "orange", new Color32(255, 165, 0, 255) },
    };

    public Texture2D Screen { get { return screenTexture; } }

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

    public ScreenGenerator ResetBackgroundColor() { charBackgroundColor = Skin.ColorSpace.BackgroundDefault(); return this; }
    public ScreenGenerator ResetForegroundColor() { charForegroundColor = Skin.ColorSpace.ForegroundDefault(); return this; }
    public ScreenGenerator InvertColors() { Color32 temp = charBackgroundColor; charBackgroundColor = charForegroundColor; charForegroundColor = temp; return this; }

    public ScreenGenerator ResetColors()
    {
        if (Skin == null) SetSkin("DefaultSkin"); // Ensure skin exists before accessing ColorSpace
        ResetBackgroundColor();
        ResetForegroundColor();
        if (colorsBackgroundMatrix != null) { Array.Fill(colorsBackgroundMatrix, charBackgroundColor); }
        return this;
    }

    public ScreenGenerator SetColorSpace(string colorSpaceName) { if (Skin == null) SetSkin("DefaultSkin"); Skin.ColorSpace = ColorSpaceManager.GetColorSpace(colorSpaceName); ResetColors(); return this; }
    public ScreenGenerator SetSkin(string newSkin) { if (Skin == null || Skin.Name != newSkin) { Skin = ScreenGeneratorSkin.GetSkin(newSkin, this); } return this; }
    public ColorSpaceBase GetColorSpace() { return Skin.ColorSpace; }

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
        ResetColors();
        return screenTexture;
    }

    // This method now expects YOUR project's ShaderScreenBase
    public ScreenGenerator ActivateShader(ShaderScreenBase changeShader)
    {
        createTexture();
        shader = changeShader;
        if (shader != null) // Add null check for safety
        {
            shader.Activate(screenTexture);
        }
        return this;
    }

    public void Update() { shader?.Update(); }
    public ScreenGenerator DrawScreen() { if (needsDraw) { screenTexture.Apply(); needsDraw = false; } return this; }

    public ScreenGenerator PrintChar(int x, int y, char charNum) { return PrintChar(x, y, charNum, charForegroundColor, charBackgroundColor); }
    public ScreenGenerator PrintChar(int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        if (screenTexture == null || Skin?.Font == null) return this;
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintChar] Invalid xy: ({x},{y}), char: {charNum}"); return this; }

        Skin.Font.PrintChar(screenTexture, x, y, charNum, fgColor, bgColor, translate);
        needsDraw = true;
        return this;
    }

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
                    else // No digits followed '\', treat as literal '\'
                    {
                        charCode = '\\';
                    }
                    PrintCharPosition((char)charCode, fgColor, bgColor, false);
                }
                else { PrintCharPosition('\\', fgColor, bgColor); } // \ at end of string
            }
            else { PrintCharPosition(c, fgColor, bgColor); }
        }

        //if (this.lastX > 0) { NextLine(); }
        return (this.lastY - initialPrintY) + 1;
    }

    public int Print(string text, bool inverted = false) 
    {
        if (this.lastX > 0) { NextLine(); }
        return Print(this.lastX, this.lastY, text, inverted); 
    }

    public void Locate(int x, int y)
    {
        this.lastX = Mathf.Clamp(x, 0, CharactersXCount - 1);
        this.lastY = Mathf.Clamp(y, 0, CharactersYCount - 1);
    }

    public ScreenGenerator NextChar()
    {
        this.lastX++;
        if (this.lastX >= CharactersXCount) { NextLine(); }
        return this;
    }

    private void PrintCharPosition(char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        PrintChar(this.lastX, this.lastY, charNum, fgColor, bgColor, translate);
        NextChar();
    }

    private void NextLine()
    {
        this.lastX = 0;
        if (this.lastY == CharactersYCount - 1) { ScrollUp(); }
        else { this.lastY++; }
    }

    public void ScrollUp()
    {
        if (screenTexture == null || Skin?.Font == null) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Not initialized."); return; }
        if (this.ScreenWidth <= 0 || this.ScreenHeight <= 0) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Invalid screen pixel dimensions."); return; }

        int charActualPixelHeight = Skin.Font.CharactersHeight;
        if (charActualPixelHeight <= 0) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Invalid char pixel height."); return; }

        int pixelsPerCharRowStrip = this.ScreenWidth * charActualPixelHeight;
        if (pixelsPerCharRowStrip <= 0) { ConfigManager.WriteConsoleError("[SG.ScrollUp] Invalid pixelsPerCharRowStrip."); return; }

        Color32[] allTexturePixels = screenTexture.GetPixels32();
        Color32[] charAreaPixels = new Color32[this.ScreenWidth * this.ScreenHeight];
        int charAreaUnityBottomY = TextureHeight - this.centerStartY - this.ScreenHeight;

        for (int y_offset = 0; y_offset < this.ScreenHeight; y_offset++)
        {
            int y_in_allTex = charAreaUnityBottomY + y_offset;
            int srcIdxInAll = y_in_allTex * TextureWidth + this.centerStartX;
            int destIdxInCharArea = y_offset * this.ScreenWidth;
            if (srcIdxInAll + this.ScreenWidth <= allTexturePixels.Length && destIdxInCharArea + this.ScreenWidth <= charAreaPixels.Length) // Boundary check
            {
                Array.Copy(allTexturePixels, srcIdxInAll, charAreaPixels, destIdxInCharArea, this.ScreenWidth);
            }
        }

        int srcScrollIdx = 0;
        int destScrollIdx = pixelsPerCharRowStrip;
        int numPixelsToScroll = (CharactersYCount - 1) * pixelsPerCharRowStrip;

        if (CharactersYCount > 1 && numPixelsToScroll > 0 &&
            destScrollIdx < charAreaPixels.Length && // Ensure destination start is within bounds
            srcScrollIdx + numPixelsToScroll <= charAreaPixels.Length && // Ensure source read is within bounds
            destScrollIdx + numPixelsToScroll <= charAreaPixels.Length)  // Ensure destination write is within bounds
        {
            Array.Copy(charAreaPixels, srcScrollIdx, charAreaPixels, destScrollIdx, numPixelsToScroll);
        }

        if (CharactersYCount > 0 && pixelsPerCharRowStrip > 0 && pixelsPerCharRowStrip <= charAreaPixels.Length)
        {
            Array.Fill(charAreaPixels, this.charBackgroundColor, 0, pixelsPerCharRowStrip);
        }

        screenTexture.SetPixels32(this.centerStartX, charAreaUnityBottomY, this.ScreenWidth, this.ScreenHeight, charAreaPixels);
        needsDraw = true;
        //ConfigManager.WriteConsole("[SG.ScrollUp] done");
    }

    public ScreenGenerator ClearBackground()
    {
        if (screenTexture != null && Skin != null) // Ensure Skin is available for BorderColor
        {
            Color32[] pixels = screenTexture.GetPixels32();
            Array.Fill(pixels, Skin.BorderColor);
            screenTexture.SetPixels32(pixels);
            needsDraw = true;
        }
        return this;
    }

    public ScreenGenerator Clear()
    {
        if (screenTexture == null) { createTexture(); }
        if (Skin == null) SetSkin("DefaultSkin"); // Ensure skin exists

        if (colorsBackgroundMatrix != null && colorsBackgroundMatrix.Length > 0 && !colorsBackgroundMatrix[0].Equals(charBackgroundColor))
        {
            Array.Fill(colorsBackgroundMatrix, charBackgroundColor);
        }
        ClearBackground();
        screenTexture.SetPixels32(centerStartX, centerStartY, ScreenWidth, ScreenHeight, colorsBackgroundMatrix);
        needsDraw = true;
        Locate(0, 0);
        return this;
    }

    public ScreenGenerator PrintCentered(int y, string text, bool inverted = false)
    {
        if (screenTexture == null) return this;
        if (y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintCentered] Invalid y: {y}"); return this; }
        string singleLineText = text.Replace("\n", "").Replace("\r", "");
        int x = (CharactersXCount - singleLineText.Length) / 2;
        if (x < 0) x = 0;
        Print(x, y, text, inverted);
        return this;
    }

    public ScreenGenerator PrintLine(int y, bool inverted, char c = '-')
    {
        if (screenTexture == null) return this;
        if (y < 0 || y >= CharactersYCount) { ConfigManager.WriteConsoleError($"[SG.PrintLine] Invalid y: {y}"); return this; }
        Print(0, y, new string(c, CharactersXCount), inverted);
        return this;
    }

    public Vector2Int GetCharPixelPosition(int charX, int charY)
    {
        if (TryGetCharPixelPosition(charX, charY, out int px, out int py)) { return new Vector2Int(px, py); }
        return new Vector2Int(-1, -1);
    }

    public bool TryGetCharPixelPosition(int charX, int charY, out int pixelX, out int pixelY)
    {
        pixelX = -1; pixelY = -1;
        if (Skin?.Font == null) { Debug.LogError("[SG.TryGetCharPixelPos] Skin or Font not initialized."); return false; }
        pixelX = this.centerStartX + (charX * Skin.Font.CharactersWidth);
        pixelY = this.centerStartY + (charY * Skin.Font.CharactersHeight);
        return true;
    }

    public ScreenGenerator DrawPoint(int x, int y, Color32 color)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawPoint(this.screenTexture, x, y, color, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawLine(int x1, int y1, int x2, int y2, Color32 color)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawLine(this.screenTexture, x1, y1, x2, y2, color, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawBox(int x, int y, int width, int height, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawBox(this.screenTexture, x, y, width, height, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawCircle(int centerX, int centerY, int radius, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawCircle(this.screenTexture, centerX, centerY, radius, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawOval(int centerX, int centerY, int radiusX, int radiusY, Color32 borderColor, bool filled, Color32? fillColor = null)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawOval(this.screenTexture, centerX, centerY, radiusX, radiusY, borderColor, filled, fillColor, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawImageRescaled(int x, int y, int width, int height, Color32[] pixels, int pixelsWidth, int pixelsHeight)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawImageRescaled(this.screenTexture, x, y, width, height, pixels, pixelsWidth, pixelsHeight, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
    public ScreenGenerator DrawTextureRescaled(int x, int y, int width, int height, Texture2D sourceTexture)
    {
        if (screenTexture != null && TextureDrawingUtilities.DrawTextureRescaled(this.screenTexture, x, y, width, height, sourceTexture, this.TextureWidth, this.TextureHeight)) { this.needsDraw = true; }
        return this;
    }
}