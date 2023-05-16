
using System;

class GenericBool
{
    // A field to store the ScreenGenerator object
    private ScreenGenerator screen;

    // A field to store the label text
    private string label;

    // A field to store the bool value
    private bool value;

    // A constructor that takes a ScreenGenerator object, a label text and an initial bool value
    public GenericBool(ScreenGenerator screen, string label, bool value)
    {
      this.screen = screen;
      this.label = label + " [ ]"; // Add the square brackets to the label text
      this.value = value;
    }

    // A method to toggle the bool value
    public void Toggle()
    {
      value = !value;
    }

    // A method to draw the GenericBool object on the screen
    public void Print(int x, int y)
    {
      // Print the label text with the square brackets
      screen.Print(x, y, label);

      // Print an X or a space depending on the bool value
      if (value)
      {
        screen.Print(x + label.Length - 2, y, "X", true);
      }
      else
      {
        screen.Print(x + label.Length - 2, y, " ", false);
      }
    }
}
