using UnityEngine;

public class ScreenGeneratorFont
{
    // Special characters
    public const char LEFT_UPPER_CORNER = (char)0;
    public const char RIGHT_UPPER_CORNER = (char)1;
    public const char HORIZONTAL_BORDER = (char)2;
    public const char LOWER_LEFT_CORNER = (char)3;
    public const char LOWER_RIGHT_CORNER = (char)4;
    public const char VERTICAL_BORDER = (char)5;

    // The list that stores the pixels for each character
    private bool[][] characters;

    // The string that contains the list of characters in the same order as the font texture
    // the arroba means "no replace"
    private string characterListOrder =
              "@abcdefghijklmnopqrstuvwxyz[£]§§"
            + " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ+§§§§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§";

    // The texture that represents a matrix of characters, each character is 8x8 pixels
    private Texture2D fontTexture;
    private ScreenGenerator screenGenerator;
    private int offsetX;
    private int offsetY;

    public int CharactersWidth { get { return 8; } }
    public int CharactersHeight { get { return 8; } }

    public ScreenGeneratorFont(Texture2D fontTexture, ScreenGenerator screenGenerator)
    {
        this.fontTexture = fontTexture;
        this.screenGenerator = screenGenerator;
        Init();
    }

    public void setOffsets(int offsetX, int offsetY)
    {
        this.offsetX = offsetX;
        this.offsetY = offsetY;
    }

    private void Init()
    {
        // Initialize the character data
        characters = new bool[256][];

        // Retrieve all pixels from the texture
        Color32[] allPixels = fontTexture.GetPixels32();
        Color32 bgColor = new Color32(255, 255, 255, 255);  // Pure white is background color

        // Loop through all the characters in the font texture
        for (int i = 0; i < 256; i++)
        {
            // Calculate the pixel coordinates for the source texture
            int srcX = (i % 32) * CharactersWidth;
            int srcY = (7 - i / 32) * CharactersHeight;

            bool[] charPixelsArray = new bool[CharactersWidth * CharactersHeight]; // 8x8 characters

            // Extract the pixels for the current character
            for (int row = 0; row < CharactersHeight; row++)
            {
                for (int col = 0; col < CharactersWidth; col++)
                {
                    int pixelIndex = ((srcY + row) * fontTexture.width + (srcX + col));
                    charPixelsArray[row * CharactersHeight + col] = !allPixels[pixelIndex].Equals(bgColor);
                }
            }

            characters[i] = charPixelsArray;
        }
    }

    public void PrintChar(Texture2D screenTexture, int x, int y, char charNum, Color32 fgColor, Color32 bgColor)
    {
        int destX = offsetX + (x * CharactersWidth);
        int destY = offsetY + ((screenGenerator.CharactersYCount - (y + 1)) * CharactersHeight);

        // Copy the pixels from the list to the screen texture
        int charIndex = TranslateCharacter(charNum);
        bool[] pixelData = characters[charIndex];

        // Draw to texture. This could be improved by writing directly to the texture data
        for (y = 0; y < CharactersHeight; y++)
        {
            for (x = 0; x < CharactersWidth; x++)
            {
                screenTexture.SetPixel(destX + x, destY + y, pixelData[y * CharactersHeight + x] ? fgColor : bgColor);
            }
        }
    }

    private int TranslateCharacter(char charNum)
    {
        switch (charNum)
        {
            case LEFT_UPPER_CORNER:
                return 112; // Specific to the C64 font, to be adjusted for other fonts
            case RIGHT_UPPER_CORNER:
                return 110; // Specific to the C64 font, to be adjusted for other fonts
            case HORIZONTAL_BORDER:
                return 64; // Specific to the C64 font, to be adjusted for other fonts
            case LOWER_LEFT_CORNER:
                return 109; // Specific to the C64 font, to be adjusted for other fonts
            case LOWER_RIGHT_CORNER:
                return 125; // Specific to the C64 font, to be adjusted for other fonts
            case VERTICAL_BORDER:
                return 93; // Specific to the C64 font, to be adjusted for other fonts
            default:
                if (charNum < 0 || charNum > 255)
                    return ' ';
                int index = characterListOrder.IndexOf(charNum);
                if (index == -1 || index > 128)
                    return ' ';

                return index;
        }
    }
}
