
using System;
using System.Collections.Generic;

// An abstract class to implement widget characters components
abstract class GenericWidget
{
    // Fields for the widget properties
    public int x; // The x coordinate of the widget in characters
    public int y; // The y coordinate of the widget in characters
    public string name; // The name of the widget
    public bool isSelectable = true; //labels arent isSelectable for example
    public bool enabled = true;

    // A field for the screen generator
    protected ScreenGenerator screen; // The screen generator to use

    // A constructor that takes the screen generator, the x and y coordinates and the name of the widget
    public GenericWidget(ScreenGenerator screen, int x, int y, string name, bool isSelectable = true)
    {
        // Check if the screen generator is valid
        if (screen == null)
        {
            throw new ArgumentNullException("The screen generator must not be null");
        }

        // Check if the x and y coordinates are valid
        if (x < 0 || y < 0)
        {
            throw new ArgumentException("The x and y coordinates of the widget must not be negative");
        }

        if (x >= screen.CharactersWidth || y >= screen.CharactersHeight)
        {
            throw new ArgumentException("The widget must fit within the screen size");
        }

        // Check if the name is valid
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The name must not be null or empty");
        }

        // Assign the fields
        this.screen = screen;
        this.x = x;
        this.y = y;
        this.name = name;
        this.isSelectable = isSelectable;

    }

    // A method to draw the widget on the screen
    public virtual void Draw()
    {
        // Do nothing by default, override in subclasses
    }
    // A method to move to the next option
    public virtual void NextOption()
    {
        // Do nothing by default, override in subclasses
    }

    // A method to move to the previous option
    public virtual void PreviousOption()
    {
        // Do nothing by default, override in subclasses
    }
    // A method to do something 
    public virtual void Action()
    {
        // Do nothing by default, override in subclasses
    }
}
