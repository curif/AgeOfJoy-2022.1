
class GenericLabelOnOff : GenericLabel // Inherit from GenericWidget
{
    public bool isOn;

    // The constructor that takes the label, the coordinates, the screen generator and the inverted property
    public GenericLabelOnOff(ScreenGenerator screen, string name, string label, int x, int y, 
                            bool inverted = false, bool isOn = false) :
      base(screen, name, label, x, y, inverted, false) // Call the base constructor with the screen, x and y parameters
    {
        this.isOn = isOn;
    }

    public override void Draw()
    {
        if (isOn)
            base.Draw();
        else if (enabled)
            screen.Print(x, y, new string(' ', label.Length)); // Print the label with the inverted property

        return;
    }

}