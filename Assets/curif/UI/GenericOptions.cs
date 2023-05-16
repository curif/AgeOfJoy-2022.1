
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

    // The constructor that takes a list of options, the coordinates and the screen generator
    public GenericOptions(ScreenGenerator screen, string label, List<string> options, int x, int y)
    {
        this.options = options;
        this.x = x;
        this.y = y;
        this.screen = screen;
        this.label = label;
        this.current = 0; // Start with the first option
    }

    // A method to show the current option
    public void Print()
    {
        screen.Print(x, y, label + " <" + options[current] + ">", true); // Print the label and the option with inverted colors
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
