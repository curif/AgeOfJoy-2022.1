
class GenericButton: GenericLabel // Inherit from GenericWidget
{
    // The constructor that takes the label, the coordinates, the screen generator and the inverted property
    public GenericButton(ScreenGenerator screen, string name, string label, int x, int y, bool inverted, bool isSelectable = true) : 
      base(screen, name, label, x, y, inverted, isSelectable) // Call the base constructor with the screen, x and y parameters
    {
    }
}
