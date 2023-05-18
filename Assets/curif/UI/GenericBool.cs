using System;

// A class to show a bool value with a label that extends the widget class
class GenericBool : GenericWidget
{
    // A field to store the label text
    private string label;

    // A field to store the bool value
    public bool value;

    // A constructor that takes a ScreenGenerator object, a name, a label text and an initial bool value
    public GenericBool(ScreenGenerator screen, string name, string label, bool value, int x, int y, bool isSelectable = true) : 
      base(screen, x, y, name, isSelectable)
    {
      this.label = label + " ( )"; // Add the square brackets to the label text
      this.value = value;
    }
    public GenericBool SetValue(bool value)
    {
      this.value = value;
      return this;
    }

    // A method to toggle the bool value
    public GenericBool Toggle()
    {
      if (enabled)
      {
        value = !value;
        Draw(); // Show the new value
      }
      return this;
    }
    public override void Action()
    {
      Toggle();
    }

    // A method to draw this widget on screen. Override from GenericWidget.
    public override void Draw()
    {
      if (!enabled)
        return;

      // Print the label text with the square brackets
      screen.Print(x, y, label);

      // Print an X or a space depending on the bool value
      if (value)
      {
        screen.Print(x + label.Length - 2, y, "*", true);
      }
      else
      {
        screen.Print(x + label.Length - 2, y, " ", false);
      }
    }
}
