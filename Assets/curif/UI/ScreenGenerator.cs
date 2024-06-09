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
    public Color32 ScreenBackgroundColor = new Color32(134, 122, 222, 255);

    private Color32 charBackgroundColor = new Color32(0, 0, 0, 255);
    private Color32 charForegroundColor = new Color32(255, 255, 255, 255);

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

    private bool needsDraw = false;

    //Color space
    private ColorSpaceBase colorSpace = ColorSpaceManager.GetColorSpace("c64");

    //Renderer
    private ShaderScreenBase shader;

    private ScreenGeneratorFont font;

    public Texture2D Screen
    {
        get
        {
            return screenTexture;
        }
    }

    // The method that runs at the start of the game
    private void Start()
    {
        ResetColors();
        //createTexture();
        //ClearBackground();
        //Clear();
        //DrawScreen();
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
        get { return colorSpace.GetNameByColor(charForegroundColor); }
        set { charForegroundColor = colorSpace.GetColorByName(value); }
    }

    public String BackgroundColorString
    {
        get { return colorSpace.GetNameByColor(charBackgroundColor); }
        set { charBackgroundColor = colorSpace.GetColorByName(value); }
    }

    public ScreenGenerator ResetBackgroundColor()
    {
        charBackgroundColor = colorSpace.BackgroundDefault();
        return this;
    }
    public ScreenGenerator ResetForegroundColor()
    {
        charForegroundColor = colorSpace.ForegroundDefault();
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
        colorSpace = ColorSpaceManager.GetColorSpace(colorSpaceName);
        ResetColors();
        return this;
    }
    public ColorSpaceBase GetColorSpace()
    {
        return colorSpace;
    }

    private Texture2D createTexture()
    {
        if (screenTexture != null)
            return screenTexture;

        //font = new CPCScreenGeneratorFont(this);
        font = new C64ScreenGeneratorFont(this);

        ScreenWidth = CharactersXCount * font.CharactersWidth;  // Width of the target texture
        ScreenHeight = CharactersYCount * font.CharactersHeight; // Height of the target texture

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

        font.setOffsets(centerStartX, centerStartY);

        return screenTexture;
    }

    public ScreenGenerator ActivateShader(ShaderScreenBase changeShader)
    {
        if (screenTexture == null)
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
    public ScreenGenerator PrintChar(int x, int y, char charNum, Color32 fgColor, Color32 bgColor)
    {
        if (screenTexture == null)
            return this;

        // Check if the coordinates and the character number are valid
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintChar] Invalid parameters x,y: ({x},{y}), charNum: {charNum}");
            return this;
        }

        font.PrintChar(screenTexture, x, y, charNum, fgColor, bgColor);
        needsDraw = true;
        return this;
    }

    // The method that prints a string of characters to the screen
    public ScreenGenerator Print(int x, int y, string text, bool inverted = false)
    {
        Color32 fgColor = inverted ? charBackgroundColor : charForegroundColor;
        Color32 bgColor = inverted ? charForegroundColor : charBackgroundColor;

        if (screenTexture == null)
            return this;

        // Check if the coordinates are valid
        if (x < 0 || x >= CharactersXCount || y < 0 || y >= CharactersYCount)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.Print] Invalid parameters for Print method, x,y: ({x},{y})");
            return this;
        }

        // Loop through all the characters in the text string
        int charpos = x;
        int i = 0;
        while (i < text.Length)
        {
            // Check for escape character and make sure it's not the last char
            char c = text[i++];
            if (c == '\\')
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
                    PrintCharPosition((char)index, ref charpos, ref y, fgColor, bgColor);

                }
                else
                {
                    PrintCharPosition('\\', ref charpos, ref y, fgColor, bgColor);
                }
            }
            else
            {
                PrintCharPosition(c, ref charpos, ref y, fgColor, bgColor);
            }

            //ConfigManager.WriteConsole($"[ScreenGenerator.Print]char:[{c}] position: {index}");
        }

        return this;
    }

    private void PrintCharPosition(char charNum, ref int x, ref int y, Color32 fgColor, Color32 bgColor)
    {
        PrintChar(x, y, charNum, fgColor, bgColor);

        x++;
        if (x >= CharactersXCount)
        {
            x = 0;
            y++;
        }
    }

    public ScreenGenerator ClearBackground()
    {

        if (screenTexture != null)
        {
            // Create a new array of color data for the texture
            Color32[] pixels = screenTexture.GetPixels32();
            Array.Fill(pixels, ScreenBackgroundColor);

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

}
