
using System;
using System.Collections.Generic;

class GenericOptions
{
    // A list of options to choose from
    private List<string> options;

    // The index of the current option
    private int current;

    // The coordinates to show the option
    private int x, y;

    // The screen generator to use
    private ScreenGenerator screen;

    // The label to show before the options
    private string label; 
    // The maximum length of the options
    private int maxLength;
    // The constructor that takes a list of options, the coordinates and the screen generator
    public GenericOptions(ScreenGenerator screen, string label, List<string> options, int x, int y)
    {
        this.options = options;
        this.x = x;
        this.y = y;
        this.screen = screen;
        this.label = label;
        this.current = 0; // Start with the first option
        this.maxLength = 0; // Initialize the maximum length to zero
        foreach (string option in options) // Loop through the options
        {
            if (option.Length > maxLength) // If the current option is longer than the previous maximum
            {
                maxLength = option.Length; // Update the maximum length
            }
        }  
    }

    // A method to show the current option
    public void Print()
    {
        string paddedOption = options[current].PadLeft(maxLength / 2 + 1).PadRight(maxLength + 1); // Add spaces to the left and right of the current option to make it the same length as the longest option
        screen.Print(x, y, label + " <", false); // Print the label with normal colors
        screen.Print(x + label.Length + 2, y, paddedOption, true); // Print the option with inverted colors
        screen.Print(x + label.Length + 2 + paddedOption.Length, y, ">", false); // Print the closing bracket with normal colors
    }
    // A method to move to the next option
    public void NextOption()
    {
        current = (current + 1) % options.Count; // Increment the index and wrap around if needed
        Print(); // Show the new option
    }

    // A method to move to the previous option
    public void PreviousOption()
    {
        current = (current - 1 + options.Count) % options.Count; // Decrement the index and wrap around if needed
        Print(); // Show the new option
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
            Print(); // Show the new option
        }
    }
}
