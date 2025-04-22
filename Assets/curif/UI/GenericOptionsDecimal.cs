
using System;
using System.Collections.Generic;
using System.Linq;
// A class to show a list of numbers to choose from that extends the GenericOptions class
class GenericOptionsDecimal : GenericOptions
{
    private string numberFormat;

    // The constructor that takes a screen generator, a name, a label text, a range of numbers and the coordinates
    public GenericOptionsDecimal(ScreenGenerator screen, string name,
                                string label,
                                double min, double max, double increment,
                                int x = 0, int y = 0,
                                bool isSelectable = true,
                                string numberFormat = "N2") :
      base(screen, name, label,
            GenericOptionsDecimal.CreateStringListFromDouble(min, max, increment, numberFormat),
            x, y, isSelectable)
    {
        this.numberFormat = numberFormat;
    }
    public static List<string> CreateStringListFromDouble(double min, double max, double increment, string numberFormat)
    {
        // Check if the range is valid
        if (min > max)
        {
            throw new ArgumentException("The minimum value must not be greater than the maximum value");
        }
        return GenerateDecimalSequence(min, max, increment, numberFormat);
    }

    // A method to get the selected option. Override from GenericOptions.
    public new int GetSelectedOption()
    {
        return int.Parse(options[current]); // Return the current option as an integer
    }

    // A method to set the current option by its value. Override from GenericOptions.
    public void SetCurrent(int value) // Change this parameter type
    {
        base.SetCurrent(value.ToString(numberFormat)); // Call the parent method with a string argument
    }

    private static List<string> GenerateDecimalSequence(double min, double max, double increment, string numberFormat)
    {

        var sequence = new List<string>();
        for (double i = min; i <= max; i += increment)
        {
            sequence.Add(i.ToString(numberFormat));
        }
        return sequence;
    }
}
