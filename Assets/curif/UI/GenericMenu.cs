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

    /// <summary>When true, options start under the title (top) instead of vertically centered.</summary>
    public bool alignTop;

    /// <summary>When true with alignTop, options are left-aligned with &gt;&gt; cursor (MR home style).</summary>
    public bool leftAlign;

    /// <summary>Blinking selection cursor for leftAlign menus (">>" or "  ").</summary>
    public string selectionCursor = ">>";

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
        screen.BackgroundColorString = "green";
        screen.PrintCentered(0, title, false);
        screen.ResetColors();

        // Print a horizontal line below the title
        screen.PrintLine(1, false, '-');

        int row = alignTop ? 3 : (screen.CharactersYCount / 2 - options.Count / 2) + 2;
        if (row < 2)
            row = 2;

        for (int i = 0; i < options.Count; i++)
        {
            if (i == selectedIndex)
                DrawSelectedOption(row, options[i]);
            else
                DrawUnselectedOption(row, options[i]);
            row++;
        }
        row++;

        // Print the help text if it is available
        if (selectedIndex >= 0 && selectedIndex < helpTexts.Count && !string.IsNullOrEmpty(helpTexts[selectedIndex]))
        {
            screen.BackgroundColorString = "blue";
            if (alignTop && leftAlign)
                screen.Print(1, row, helpTexts[selectedIndex], false);
            else
                screen.PrintCentered(row, helpTexts[selectedIndex], false);
            screen.ResetColors();
        }
    }

    void DrawSelectedOption(int row, string option)
    {
        int width = screen.CharactersXCount;
        string inner;
        string label;

        if (alignTop && leftAlign)
        {
            string cursor = string.IsNullOrEmpty(selectionCursor) ? ">>" : selectionCursor;
            if (cursor.Length < 2)
                cursor = cursor.PadRight(2);
            inner = cursor + option;
            label = (" " + inner).PadRight(width);
            if (label.Length > width)
                label = label.Substring(0, width);

            screen.ForegroundColorString = "black";
            screen.BackgroundColorString = "yellow";
            screen.Print(0, row, label, false);
            screen.ResetColors();
            return;
        }

        inner = highlightChar + " " + option + " " + highlightChar;
        int pad = Mathf.Max(0, (width - inner.Length) / 2);
        label = new string(' ', pad) + inner;
        if (label.Length < width)
            label = label.PadRight(width);
        else if (label.Length > width)
            label = label.Substring(0, width);

        screen.ForegroundColorString = "black";
        screen.BackgroundColorString = "yellow";
        screen.Print(0, row, label, false);
        screen.ResetColors();
    }

    void DrawUnselectedOption(int row, string option)
    {
        int width = screen.CharactersXCount;
        screen.ResetColors();

        if (alignTop && leftAlign)
        {
            string label = ("  " + option).PadRight(width);
            if (label.Length > width)
                label = label.Substring(0, width);
            screen.Print(0, row, label, false);
        }
        else
        {
            string inner = "  " + option + "  ";
            int pad = Mathf.Max(0, (width - inner.Length) / 2);
            string label = new string(' ', pad) + inner;
            if (label.Length < width)
                label = label.PadRight(width);
            else if (label.Length > width)
                label = label.Substring(0, width);
            screen.Print(0, row, label, false);
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
