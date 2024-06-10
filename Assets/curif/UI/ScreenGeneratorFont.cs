using UnityEngine;

public abstract class ScreenGeneratorFont
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
    private string characterListOrder;

    // The texture that represents a matrix of characters, each character is 8x8 pixels
    private Texture2D fontTexture;
    private ScreenGenerator screenGenerator;
    private int offsetX;
    private int offsetY;

    public int CharactersWidth;
    public int CharactersHeight;

    public ScreenGeneratorFont(string fontFile, string characterListOrder, int characterWidth, int characterHeight, ScreenGenerator screenGenerator)
    {
        this.screenGenerator = screenGenerator;
        this.fontTexture = Resources.Load<Texture2D>(fontFile);
        this.characterListOrder = characterListOrder;
        this.CharactersWidth = characterWidth;
        this.CharactersHeight = characterHeight;
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

    protected virtual int TranslateCharacter(char charNum)
    {
        if (charNum < 0 || charNum > 255)
            return ' ';
        int index = characterListOrder.IndexOf(charNum);
        if (index < 0 || index > 255)
            return ' ';

        return index;
    }
}
public class C64ScreenGeneratorFont : ScreenGeneratorFont
{
    public C64ScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/c64Font",

            "@abcdefghijklmnopqrstuvwxyz[£]§§"
            + " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "©ABCDEFGHIJKLMNOPQRSTUVWXYZ+§§§§",

            8, 8, screenGenerator)
    { }

    protected override int TranslateCharacter(char charNum)
    {
        switch (charNum)
        {
            case LEFT_UPPER_CORNER:
                return 112;
            case RIGHT_UPPER_CORNER:
                return 110;
            case HORIZONTAL_BORDER:
                return 64;
            case LOWER_LEFT_CORNER:
                return 109;
            case LOWER_RIGHT_CORNER:
                return 125;
            case VERTICAL_BORDER:
                return 93;
            default:
                return base.TranslateCharacter(charNum);
        }
    }
}
public class CPCScreenGeneratorFont : ScreenGeneratorFont
{
    public CPCScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/cpcFont",

              "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§§"
            + "`abcdefghijklmnopqrstuvwxyz{|}~§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + "^§§£©§§§§§§§§§§§§§§§§§§§§§§§§§§§",

            8, 8, screenGenerator)
    {
    }

    protected override int TranslateCharacter(char charNum)
    {
        switch (charNum)
        {
            case LEFT_UPPER_CORNER:
                return 150;
            case RIGHT_UPPER_CORNER:
                return 156;
            case HORIZONTAL_BORDER:
                return 154;
            case LOWER_LEFT_CORNER:
                return 147;
            case LOWER_RIGHT_CORNER:
                return 153;
            case VERTICAL_BORDER:
                return 149;
            default:
                return base.TranslateCharacter(charNum);
        }
    }
}

public class TO7ScreenGeneratorFont : ScreenGeneratorFont
{
    public TO7ScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/to7Font",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§§"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
    }

    protected override int TranslateCharacter(char charNum)
    {
        switch (charNum)
        {
            case LEFT_UPPER_CORNER:
                return 96;
            case RIGHT_UPPER_CORNER:
                return 97;
            case HORIZONTAL_BORDER:
                return 98;
            case LOWER_LEFT_CORNER:
                return 99;
            case LOWER_RIGHT_CORNER:
                return 100;
            case VERTICAL_BORDER:
                return 101;
            default:
                return base.TranslateCharacter(charNum);
        }
    }
}

public class ZXScreenGeneratorFont : ScreenGeneratorFont
{
    public ZXScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/zxFont",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§§"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
    }

    protected override int TranslateCharacter(char charNum)
    {
        switch (charNum)
        {
            case LEFT_UPPER_CORNER:
                return 96;
            case RIGHT_UPPER_CORNER:
                return 97;
            case HORIZONTAL_BORDER:
                return 98;
            case LOWER_LEFT_CORNER:
                return 99;
            case LOWER_RIGHT_CORNER:
                return 100;
            case VERTICAL_BORDER:
                return 101;
            default:
                return base.TranslateCharacter(charNum);
        }
    }
}
