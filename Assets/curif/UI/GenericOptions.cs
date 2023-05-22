using System;
using System.Collections.Generic;
class GenericOptions : GenericWidget // derived class (child)
{
    // A list of options to choose from
    protected List<string> options;

    // The index of the current option
    protected int current;

    // The label to show before the options
    private string label;
    // The maximum length of the options
    public int MaxLength { get; set; } = 0; // auto-implemented property with default value

    // The constructor that takes a list of options, the coordinates, the screen generator and the maximum length
    public GenericOptions(ScreenGenerator screen, string name, string label, List<string> options, int x, int y, bool isSelectable = true, int maxLength = 0) :
    base(screen, x, y, name, isSelectable) // call the base class constructor
    {
        this.options = options == null? new List<string>() : options;
        this.label = label;
        this.current = 0; // Start with the first option
        this.MaxLength = maxLength; // assign the parameter to the property
        if (this.MaxLength == 0) // if zero, calculate based on options, label, brackets and x position
        {
            foreach (string option in options) // Loop through the options
            {
                if (option.Length > this.MaxLength) // If the current option is longer than the previous maximum
                {
                    this.MaxLength = option.Length; // Update the maximum length
                }
            }
            this.MaxLength += label.Length + 4; // add the length of label and brackets
            if (this.MaxLength + x > screen.CharactersWidth) // if exceeds screen width
            {
                this.MaxLength = screen.CharactersWidth - x; // adjust to fit screen width
            }
        }
    }

    // A method to draw this widget on screen. Override from GenericWidget.
    public override void Draw() // use override keyword to indicate that this method replaces the base class method
    {
        if (!enabled)
            return;
        string paddedOption = options[current].PadRight(MaxLength - label.Length - 4); // Add spaces to the right of the current option to make it fit within maxLength
        if (paddedOption.Length > MaxLength - label.Length - 4) // if still too long
        {
            paddedOption = paddedOption.Substring(0, MaxLength - label.Length - 4); // truncate it
        }
        screen.Print(x, y, label + " <", false); // Print the label with normal colors
        screen.Print(x + label.Length + 2, y, paddedOption + " ", true); // Print the option with inverted colors and a trailing space
        screen.Print(x + MaxLength - 1, y, ">", false); // Print the closing bracket with normal colors
    }

    // A method to move to the next option
    public override void NextOption() // use override keyword to indicate that this method replaces the base class method
    {
        current = (current + 1) % options.Count; // Increment the index and wrap around if needed
        Draw(); // Show the new option
    }

    // A method to move to the previous option
    public override void PreviousOption() // use override keyword to indicate that this method replaces the base class method
    {
        current = (current - 1 + options.Count) % options.Count; // Decrement the index and wrap around if needed
        Draw(); // Show the new option
    }

    // A method to get the selected option
    public string GetSelectedOption()
    {
        return options[current]; // Return the current option
    }

    // A method to set the current option by its value
    public void SetCurrent(string value) // Add this parameter
    {
        int index = options.IndexOf(value); // Find the index of the new option
        if (index != -1) // If found
        {
            current = index; // Set the current field to that index
            Draw(); // Show the new option
        }
    }
}
