using UnityEngine;
using System.Collections.Generic;

public class ScreenGenerator : MonoBehaviour
{
    // The texture that represents a matrix of characters, each character is 8x8 pixels
    public Texture2D c64Font;

    public int CharactersWidth = 40;
    public int CharactersHeight = 25;

    public Color BackgroundColor;

    public string ShaderName = "damage";
    [SerializeField]
    public Dictionary<string, string> ShaderConfig = new Dictionary<string, string>();

    // The list that stores the pixels for each character
    private List<Color[]> charPixels;

    // The string that contains the list of characters in the same order as the font texture
    // the arroba means "no replace"
    private string characterListOrder = "@ABCDEFGHIJKLMNOPQRSTUVWXYZ@@@@+ !\"#$%&'()*+,-./0123456789:;<=>?@@|@@@_";
    private string characterListOrderAlternate = "@abcdefghijklmnopqrstuvwxyz@@@@+ !\"#$%&'()*+,-./0123456789:;<=>?@@|@@@_";
    private int characterPositionForNotFound = 34; //" "

    // The texture that represents the screen, it has 40x25 characters of capacity
    private Texture2D c64Screen;
    private Color[] colorsBackgroundMatrix;
    private int ScreenWidth; // Width of the target texture
    private int ScreenHeight; // Height of the target texture

    //Renderer
    private Renderer display;
    private ShaderScreenBase shader;

    public Texture2D Screen
    {
        get
        {
            return c64Screen;
        }
    }

    // The method that runs at the start of the game
    private void Start()
    {
        display = GetComponent<Renderer>();

        ScreenWidth = CharactersWidth * 8;  // Width of the target texture
        ScreenHeight = CharactersHeight * 8; // Height of the target texture
        // Create the target texture with the specified width and height
        c64Screen = new Texture2D(ScreenWidth, ScreenHeight);

        // Set the target texture to be readable and writable
        c64Screen.wrapMode = TextureWrapMode.Clamp;
        c64Screen.filterMode = FilterMode.Point;
        c64Screen.anisoLevel = 0;

        ShaderConfig["damage"] = "low";
        shader = ShaderScreen.Factory(display, 1, ShaderName, ShaderConfig);
        shader.Texture = c64Screen;

        //characters
        // Initialize the list
        charPixels = new List<Color[]>();

        // Loop through all the characters in the font texture
        for (int i = 0; i < 256; i++)
        {
            // Calculate the pixel coordinates for the source texture
            int srcX = (i % 32) * 8;
            int srcY = (7 - i / 32) * 8; // Subtract i / 32 from 7 to flip the origin
                                         // Get the pixels for the current character and add them to the list
            Color[] pixels = c64Font.GetPixels(srcX, srcY, 8, 8);
            charPixels.Add(pixels);
        }

        //Colors
        // Create an array of colors to fill the texture
        colorsBackgroundMatrix = new Color[ScreenWidth * ScreenHeight];
        // Set all pixels in the colors array to the BackgroundColor color
        for (int i = 0; i < colorsBackgroundMatrix.Length; i++)
        {
            colorsBackgroundMatrix[i] = BackgroundColor;
        }

    }
    public void Update()
    {
        shader.Update();
        return;
    }

    // The method that prints a single character to the screen
    public void PrintChar(int x, int y, int charNum, bool inverted)
    {
        if (c64Screen == null)
            return;
        // Check if the coordinates and the character number are valid
        if (x < 0 || x >= CharactersWidth || y < 0 || y >= CharactersHeight || charNum < 0 || charNum >= 128)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintChar] Invalid parameters x,y: ({x},{y}), charNum: {charNum}");
            return;
        }
        // Calculate the pixel coordinates for the destination texture
        int destX = x * 8;
        int destY = (CharactersHeight - 1 - y) * 8; // Subtract y from 24 to flip the origin
        // Get the pixels from the list for the given character number and inversion flag
        int index = inverted ? charNum + 128 : charNum;
        Color[] pixels = charPixels[index];

        // Copy the pixels from the list to the screen texture
        c64Screen.SetPixels(destX, destY, 8, 8, pixels);
        c64Screen.Apply();
    }

    // The method that prints a string of characters to the screen
    public void Print(int x, int y, string text, bool inverted = false)
    {
        if (c64Screen == null)
            return;
        // Check if the coordinates are valid
        if (x < 0 || x >= CharactersWidth || y < 0 || y >= CharactersHeight)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.Print] Invalid parameters for Print method, x,y: ({x},{y})");
            return;
        }

        // Loop through all the characters in the text string
        for (int i = 0; i < text.Length; i++)
        {
            // Get the current character from the text string
            char c = text[i];

            // Find the index of the character in the character list order string
            int index = characterListOrder.IndexOf(c);
            if (index == -1)
                index = characterListOrderAlternate.IndexOf(c);
            if (index == -1)
                index = characterPositionForNotFound;
            if (index > 128)
                index = characterPositionForNotFound;
            // Print the character to the screen using PrintChar method with inversion flag
            PrintChar(x + i, y, index, inverted);
            //ConfigManager.WriteConsole($"[ScreenGenerator.Print]char:[{c}] position: {index}");
        }
    }
    // The method that clears the screen with a given color or cyan by default
    public void Clear()
    {
        // Fill the screen texture with the given color
        if (c64Screen == null)
            return;
        c64Screen.SetPixels(colorsBackgroundMatrix);
        c64Screen.Apply();
    }
    // The method that prints a string of characters to the center of the X axis
    public void PrintCentered(int y, string text, bool inverted = false)
    {
        if (c64Screen == null)
            return;
        // Check if the y coordinate is valid
        if (y < 0 || y >= CharactersHeight)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintCentered] Invalid parameters y: ({y})");
            return;
        }

        // Calculate the x coordinate that will center the text
        int x = (CharactersWidth - text.Length) / 2;

        // Print the text using the Print method with the calculated x coordinate
        Print(x, y, text, inverted);
    }


    // The method that prints the same character in a line
    public void PrintLine(int y, bool inverted, char c = '-')
    {
        if (c64Screen == null)
            return;
        // Check if the y coordinate is valid
        if (y < 0 || y >= CharactersHeight)
        {
            ConfigManager.WriteConsoleError($"[ScreenGenerator.PrintLine] Invalid parameters y: ({y})");
            return;
        }

        // Create a string of 40 characters using the given character
        string text = new string(c, CharactersWidth);

        // Print the text using the Print method with the x coordinate of 0
        Print(0, y, text, inverted);
    }

}
