using UnityEngine;

public abstract class ScreenGeneratorFont
{
    // Special characters


    public const char GLYPH_SAVE = (char)224;
    public const char GLYPH_BUTTON_A = (char)225;
    public const char GLYPH_BUTTON_B = (char)226;
    public const char GLYPH_BUTTON_X = (char)227;
    public const char GLYPH_BUTTON_Y = (char)228;
    public const char GLYPH_BUTTON_L = (char)229;
    public const char GLYPH_BUTTON_R = (char)230;
    public const char GLYPH_BUTTON_LT = (char)231;
    public const char GLYPH_BUTTON_RT = (char)232;
    public const char GLYPH_DPAD_UP = (char)233;
    public const char GLYPH_DPAD_DOWN = (char)234;
    public const char GLYPH_DPAD_LEFT = (char)235;
    public const char GLYPH_DPAD_RIGHT = (char)236;
    public const char GLYPH_UP = (char)237;
    public const char GLYPH_DOWN = (char)238;
    public const char GLYPH_LEFT = (char)239;
    public const char GLYPH_RIGHT = (char)240;
    public const char GLYPH_START_1 = (char)241;
    public const char GLYPH_START_2 = (char)242;
    public const char GLYPH_START_3 = (char)243;
    public const char GLYPH_SELECT_1 = (char)244;
    public const char GLYPH_SELECT_2 = (char)245;
    public const char GLYPH_SELECT_3 = (char)246;
    public const char GLYPH_LEFT_UPPER_CORNER = (char)247;
    public const char GLYPH_RIGHT_UPPER_CORNER = (char)248;
    public const char GLYPH_HORIZONTAL_BORDER = (char)249;
    public const char GLYPH_LOWER_LEFT_CORNER = (char)250;
    public const char GLYPH_LOWER_RIGHT_CORNER = (char)251;
    public const char GLYPH_VERTICAL_BORDER = (char)252;
    public const char GLYPH_SQUARE = (char)253;
    public const char GLYPH_GRID = (char)254;
    public const char GLYPH_LINES = (char)255;

    public const char GLYPH_BEGINNING = GLYPH_SAVE;
    public const char GLYPH_END = GLYPH_LINES;

    public static readonly string STR_GLYPH_SAVE = GLYPH_SAVE.ToString();
    public static readonly string STR_GLYPH_BUTTON_A = GLYPH_BUTTON_A.ToString();
    public static readonly string STR_GLYPH_BUTTON_B = GLYPH_BUTTON_B.ToString();
    public static readonly string STR_GLYPH_BUTTON_X = GLYPH_BUTTON_X.ToString();
    public static readonly string STR_GLYPH_BUTTON_Y = GLYPH_BUTTON_Y.ToString();
    public static readonly string STR_GLYPH_BUTTON_L = GLYPH_BUTTON_L.ToString();
    public static readonly string STR_GLYPH_BUTTON_R = GLYPH_BUTTON_R.ToString();
    public static readonly string STR_GLYPH_BUTTON_LT = GLYPH_BUTTON_LT.ToString();
    public static readonly string STR_GLYPH_BUTTON_RT = GLYPH_BUTTON_RT.ToString();
    public static readonly string STR_GLYPH_DPAD_UP = GLYPH_DPAD_UP.ToString();
    public static readonly string STR_GLYPH_DPAD_DOWN = GLYPH_DPAD_DOWN.ToString();
    public static readonly string STR_GLYPH_DPAD_LEFT = GLYPH_DPAD_LEFT.ToString();
    public static readonly string STR_GLYPH_DPAD_RIGHT = GLYPH_DPAD_RIGHT.ToString();
    public static readonly string STR_GLYPH_UP = GLYPH_UP.ToString();
    public static readonly string STR_GLYPH_DOWN = GLYPH_DOWN.ToString();
    public static readonly string STR_GLYPH_LEFT = GLYPH_LEFT.ToString();
    public static readonly string STR_GLYPH_RIGHT = GLYPH_RIGHT.ToString();
    public static readonly string STR_GLYPH_START = GLYPH_START_1.ToString() + GLYPH_START_2.ToString() + GLYPH_START_3.ToString();
    public static readonly string STR_GLYPH_SELECT_1 = GLYPH_SELECT_1.ToString() + GLYPH_SELECT_2.ToString() + GLYPH_SELECT_3.ToString();
    public static readonly string STR_GLYPH_LEFT_UPPER_CORNER = GLYPH_LEFT_UPPER_CORNER.ToString();
    public static readonly string STR_GLYPH_RIGHT_UPPER_CORNER = GLYPH_RIGHT_UPPER_CORNER.ToString();
    public static readonly string STR_GLYPH_HORIZONTAL_BORDER = GLYPH_HORIZONTAL_BORDER.ToString();
    public static readonly string STR_GLYPH_LOWER_LEFT_CORNER = GLYPH_LOWER_LEFT_CORNER.ToString();
    public static readonly string STR_GLYPH_LOWER_RIGHT_CORNER = GLYPH_LOWER_RIGHT_CORNER.ToString();
    public static readonly string STR_GLYPH_VERTICAL_BORDER = GLYPH_VERTICAL_BORDER.ToString();
    public static readonly string STR_GLYPH_SQUARE = GLYPH_SQUARE.ToString();
    public static readonly string STR_GLYPH_GRID = GLYPH_GRID.ToString();
    public static readonly string STR_GLYPH_LINES = GLYPH_LINES.ToString();



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

    /*
    public void PrintChar(Texture2D screenTexture, int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        int destX = offsetX + (x * CharactersWidth);
        int destY = offsetY + ((screenGenerator.CharactersYCount - (y + 1)) * CharactersHeight);

        // Copy the pixels from the list to the screen texture
        int charIndex = translate ? TranslateCharacter(charNum) : charNum;
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
    */
    public void PrintChar(Texture2D screenTexture, int x, int y, char charNum, Color32 fgColor, Color32 bgColor, bool translate = true)
    {
        int destX = offsetX + (x * CharactersWidth);
        int destY = offsetY + ((screenGenerator.CharactersYCount - (y + 1)) * CharactersHeight);

        int charIndex = translate ? TranslateCharacter(charNum) : charNum;
        bool[] pixelData = characters[charIndex];

        Color32[] buffer = new Color32[CharactersWidth * CharactersHeight];
        for (int i = 0; i < CharactersHeight; i++)
        {
            for (int j = 0; j < CharactersWidth; j++)
            {
                buffer[i * CharactersWidth + j] = pixelData[i * CharactersWidth + j] ? fgColor : bgColor;
            }
        }

        screenTexture.SetPixels32(destX, destY, CharactersWidth, CharactersHeight, buffer);
    }


    protected virtual int TranslateCharacter(char charNum)
    {
        if (charNum >= GLYPH_BEGINNING && charNum <= GLYPH_END)
            return charNum;
        if (charNum < 0 || charNum > 255)
            return ' ';
        int index = characterListOrder.IndexOf(charNum);
        if (index < 0 || index > 255)
            return ' ';

        return index;
    }
}

public class AppleIIScreenGeneratorFont : ScreenGeneratorFont
{
    public AppleIIScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/apple2Font",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§_"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
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
}

public class CPCScreenGeneratorFont : ScreenGeneratorFont
{
    public CPCScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/cpcFont",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§_"
            + "`abcdefghijklmnopqrstuvwxyz{|}~§"
            + "§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§§"
            + "^§§£©§§§§§§§§§§§§§§§§§§§§§§§§§§§",

            8, 8, screenGenerator)
    {
    }
}
public class MSXScreenGeneratorFont : ScreenGeneratorFont
{
    public MSXScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/msxFont",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§_"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
    }
}

public class TO7ScreenGeneratorFont : ScreenGeneratorFont
{
    public TO7ScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/to7Font",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§_"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
    }
}

public class ZXScreenGeneratorFont : ScreenGeneratorFont
{
    public ZXScreenGeneratorFont(ScreenGenerator screenGenerator) : base(
            "UICabinet/Screen/zxFont",

              " !\"#$%&'()*+,-./0123456789:;<=>?"
            + "§ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]§_"
            + "£abcdefghijklmnopqrstuvwxyz{|}~©",

            8, 8, screenGenerator)
    {
    }
}
