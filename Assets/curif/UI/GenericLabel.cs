
class GenericLabel : GenericWidget // Inherit from GenericWidget
{
    // The label to show
    public string label;

    // The inverted property
    private bool inverted; // Add this field

    // The constructor that takes the label, the coordinates, the screen generator and the inverted property
    public GenericLabel(ScreenGenerator screen, string name, string label, int x, int y, bool inverted = false, bool isSelectable = false) : 
      base(screen, x, y, name, isSelectable) // Call the base constructor with the screen, x and y parameters
    {
        this.label = label; // Set the label field
        this.inverted = inverted; // Set the inverted field
    }

    // A method to print the label
    public override void Draw()
    {
      if (!enabled)
        return;
      screen.Print(x, y, label, inverted); // Print the label with the inverted property
    }
}
