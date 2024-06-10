
using System;

// A class to draw character windows in the screen
class GenericWindow : GenericWidget
{
    // Constants for the default title
    private const string DEFAULT_TITLE = "Window";
    private const bool DEFAULT_INVERTED = true;

    // Fields for the window properties
    private int width; // The width of the window in characters
    private int height; // The height of the window in characters
    private string title; // The title of the window
    private bool inverted; // Whether the title is inverted or not


    // A constructor that takes the screen generator, the x and y coordinates, the width and height of the window and an optional title and inverted flag
    public GenericWindow(ScreenGenerator screen, int x, int y, string name, int width, int height, string title = DEFAULT_TITLE, bool inverted = DEFAULT_INVERTED) :
      base(screen, x, y, name, false)
    {
        // Check if the screen generator is valid
        if (screen == null)
        {
            throw new ArgumentNullException("The screen generator must not be null");
        }

        // Check if the x and y coordinates are valid
        if (x < 0 || y < 0)
        {
            throw new ArgumentException("The x and y coordinates of the window must not be negative");
        }

        if (x + width > screen.CharactersXCount || y + height > screen.CharactersYCount)
        {
            throw new ArgumentException("The window must fit within the screen size");
        }

        // Check if the width and height are valid
        if (width < 3 || height < 3)
        {
            throw new ArgumentException("The width and height of the window must be at least 3");
        }

        // Check if the title is valid
        if (title.Length > width - 2)
        {
            throw new ArgumentException("The title must not be longer than the window width minus 2");
        }

        this.width = width;
        this.height = height;
        this.title = title;
        this.inverted = inverted;
    }

    // A method to draw the window on the screen
    public override void Draw()
    {
        if (!enabled)
            return;

        // Draw the top border with the title
        screen.PrintChar(x, y, ScreenGeneratorFont.GLYPH_LEFT_UPPER_CORNER);

        for (int i = 1; i < width - 1; i++)
        {
            screen.PrintChar(x + i, y, ScreenGeneratorFont.GLYPH_HORIZONTAL_BORDER);
        }

        screen.PrintChar(x + width - 1, y, ScreenGeneratorFont.GLYPH_RIGHT_UPPER_CORNER);
        screen.Print(x + 1, y, title, inverted);

        // Draw the side borders
        for (int i = 1; i < height - 1; i++)
        {
            screen.PrintChar(x, y + i, ScreenGeneratorFont.GLYPH_VERTICAL_BORDER);
            screen.PrintChar(x + width - 1, y + i, ScreenGeneratorFont.GLYPH_VERTICAL_BORDER);
        }

        // Draw the bottom border
        screen.PrintChar(x, y + height - 1, ScreenGeneratorFont.GLYPH_LOWER_LEFT_CORNER);

        for (int i = 1; i < width - 1; i++)
        {
            screen.PrintChar(x + i, y + height - 1, ScreenGeneratorFont.GLYPH_HORIZONTAL_BORDER);
        }

        screen.PrintChar(x + width - 1, y + height - 1, ScreenGeneratorFont.GLYPH_LOWER_RIGHT_CORNER);
    }
}
