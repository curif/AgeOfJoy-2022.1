using System;
// A child class of GenericLabel that draws for some seconds and then clears the space
class GenericTimedLabel : GenericLabel
{
    // The start time field
    private DateTime startTime;

    // The seconds field
    private double seconds;

    // The constructor that takes the label, the coordinates, the screen generator and the inverted property
    public GenericTimedLabel(ScreenGenerator screen, string name, string label, int x, int y, bool inverted = false, bool isSelectable = false) :
    base(screen, name, label, x, y, inverted, isSelectable) // Call the base constructor with the same parameters
    {
        Clear(); // Call the Clear method to start cleared
    }

    // A method to print the label and set the start time if not set
    public override void Draw()
    {
        if (!enabled)
            return;

        if (CheckTime())
        {
            Clear(); //disable
            return;
        }

        base.Draw(); // Call the base Draw method
    }

    // A method to clear the space and stop drawing
    public void Clear()
    {
        screen.Print(x, y, new string(' ', label.Length), false); // Print spaces with the same length as the label
        enabled = false; // Set the enabled property to false
        startTime = default; // Reset the start time to default
    }

    // A method to set the seconds and start drawing again
    public void SetSecondsAndDraw(double seconds)
    {
        Start(seconds);
        Draw(); // Call the Draw method
    }

    public void Start(double seconds)
    {
        startTime = DateTime.Now; // Set the start time to the current time
        this.seconds = seconds; // Set the seconds field
        enabled = true;
    }

    // A method to check if the time has elapsed and clear if so
    public bool CheckTime()
    {
        var currentTime = DateTime.Now; // Get the current time
        var elapsedTime = currentTime - startTime; // Calculate the elapsed time
        return (elapsedTime.TotalSeconds >= seconds); // If the elapsed time is greater than or equal to the seconds field
    }
}
