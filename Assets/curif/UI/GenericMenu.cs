using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericMenu
{
    // A reference to the existing class that writes text over a texture
    public ScreenGenerator screen;

    // The title of the menu
    public string title;

    // The options of the menu
    public List<string> options;
    // The help text for each option
    public List<string> helpTexts;
    // The index of the currently selected option
    public int selectedIndex = 0;
    public int userSelectedIndex = -1;

    // The character to use for highlighting the selected option
    public char highlightChar = '*';

    // The constructor of the class
    public GenericMenu(ScreenGenerator screen, string title, string[] options = null, string[] helpText = null)
    {
        // Assign the screen reference
        this.screen = screen;

        // Assign the title and the options
        this.title = title;
        this.options = options != null ? new List<string>(options) : new List<string>();
        this.helpTexts = helpText != null ? new List<string>(helpText) : new List<string>();
        Deselect();
    }

    // A method to draw the menu on the screen
    public void DrawMenu()
    {
        // Print the title in the center of the first row
        screen.PrintCentered(0, title, false);

        // Print a horizontal line below the title
        screen.PrintLine(1, false, '-');

        // Calculate the vertical center of the screen
        int centerY = screen.CharactersHeight / 2;

        // Calculate the top row of the menu
        int topRow = centerY - options.Count / 2;

        // Print the options in the center of the screen
        int row = topRow + 2;
        for (int i = 0; i < options.Count; i++)
        {
            if (i == selectedIndex)
            {
                // Highlight the selected option with an inverted text and a character on both sides
                screen.PrintCentered(row, highlightChar + " " + options[i] + " " + highlightChar, true);
            }
            else
            {
                screen.PrintCentered(row, "  " + options[i] + "  ", false);
            }
            row++;
        }
        row++;

        // Print the help text if it is available
        if (selectedIndex >= 0 && selectedIndex < helpTexts.Count && !string.IsNullOrEmpty(helpTexts[selectedIndex]))
        {
            screen.PrintCentered(row, helpTexts[selectedIndex], false);
        }
    }

    // A method to move to the next option
    public void NextOption()
    {
        // Move down one option if possible
        if (selectedIndex < options.Count - 1)
        {
            selectedIndex++;
        }

        // Draw the updated menu
        DrawMenu();
    }

    // A method to move to the previous option
    public void PreviousOption()
    {
        // Move up one option if possible
        if (selectedIndex > 0)
        {
            selectedIndex--;
        }

        // Draw the updated menu
        DrawMenu();
    }

    // A method to return the selected option as a string
    public string GetSelectedOption()
    {
        if (IsSelected())
        {
            return options[userSelectedIndex];
        }
        return "";
    }

    public string GetHighlightedOption()
    {
        return options[selectedIndex];
    }

    public void Select()
    {
        userSelectedIndex = selectedIndex;
    }

    public bool IsSelected()
    {
        return userSelectedIndex != -1;
    }

    public void Deselect()
    {
        userSelectedIndex = -1;
    }

    // A method to add an option and its corresponding help text
    public void AddOption(string option, string helpText = "")
    {
        options.Add(option);
        helpTexts.Add(helpText);
    }
}
