
using System;
using System.Collections.Generic;
using System.Linq;
// A class to show a list of numbers to choose from that extends the GenericOptions class
class GenericOptionsInteger : GenericOptions
{
    // The constructor that takes a screen generator, a name, a label text, a range of numbers and the coordinates
    public GenericOptionsInteger(ScreenGenerator screen, string name, string label, int min, int max, int x, int y, bool isSelectable = true) : 
      base(screen, name, label, GenericOptionsInteger.CreateStringListFromInt(min, max), x, y, isSelectable)
    {
    }
    public static List<string>CreateStringListFromInt(int min, int max)
    {
      // Check if the range is valid
      if (min > max)
      {
          throw new ArgumentException("The minimum value must not be greater than the maximum value");
      }
      return Enumerable.Range(min, max - min + 1).Select(i => i.ToString()).ToList();
    }
    
    // A method to get the selected option. Override from GenericOptions.
    public new int GetSelectedOption()
    {
        return int.Parse(options[current]); // Return the current option as an integer
    }
    
    // A method to set the current option by its value. Override from GenericOptions.
    public void SetCurrent(int value) // Change this parameter type
    {
        base.SetCurrent(value.ToString()); // Call the parent method with a string argument
    }
}
