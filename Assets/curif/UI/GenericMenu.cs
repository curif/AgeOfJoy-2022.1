using UnityEngine;
using System.Collections;

public class GenericMenu {

    // A reference to the existing class that writes text over a texture
    public ScreenGenerator screen;

    // The title of the menu
    public string title;

    // The options of the menu
    public string[] options;

    // The index of the currently selected option
    public int selectedIndex = 0;

    // The character to use for highlighting the selected option
    public char highlightChar = '*';

    // The constructor of the class
    public GenericMenu(ScreenGenerator screen, string title, string[] options) {
        // Assign the screen reference
        this.screen = screen;

        // Assign the title and the options
        this.title = title;
        this.options = options;
    }

    // A method to draw the menu on the screen
    public void DrawMenu() {
        // Clear the screen
        screen.Clear();

        // Print the title in the center of the first row
        screen.PrintCentered(0, title, false);

        // Print a horizontal line below the title
        screen.PrintLine(1, false, '-');

        // Print the options in the center of the screen, starting from row 10
        int row = 10;
        foreach (string option in options) {
            screen.PrintCentered(row, option, false);
            row++;
        }

        // Highlight the selected option with an inverted text and a character on both sides
        screen.PrintCentered(selectedIndex + 10, highlightChar + " " + options[selectedIndex] + " " + highlightChar, true);
    }

    // A method to move to the next option
    public void NextOption() {
        // Move down one option if possible
        if (selectedIndex < options.Length - 1) {
            selectedIndex++;
        }

        // Draw the updated menu
        DrawMenu();
    }

    // A method to move to the previous option
    public void PreviousOption() {
        // Move up one option if possible
        if (selectedIndex > 0) {
            selectedIndex--;
        }

        // Draw the updated menu
        DrawMenu();
    }

    // A method to return the selected option as a string
    public string GetSelectedOption() {
        return options[selectedIndex];
    }
}
