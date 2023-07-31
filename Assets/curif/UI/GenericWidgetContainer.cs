
using System;
using System.Collections.Generic;

// A class to store and show the widgets that extends the widget class
class GenericWidgetContainer : GenericWidget
{
    // A field for the list of widgets
    private List<GenericWidget> widgets; // The list of widgets

    // A field for the current index of the widget
    private int index; // The index of the current widget

    public int lastYAdded = 0;

    // A constructor that takes the screen generator, the x and y coordinates and the name of the container and initializes the list and the index
    public GenericWidgetContainer(ScreenGenerator screen, string name) : base(screen, 0, 0, name)
    {
        widgets = new List<GenericWidget>();
        index = -1;
    }
    public int Count()
    {
        return widgets.Count;
    }

    // A method to add a widget to the list
    public GenericWidgetContainer Add(GenericWidget widget, int x = -1, int y = -1)
    {
        // Check if the widget is valid
        if (widget == null)
        {
            throw new ArgumentNullException("The widget must not be null");
        }

        // Add the widget to the list
        widgets.Add(widget);
        if (x != -1)
            widget.x = x;
        if (y != -1)
            widget.y = y;
        
        lastYAdded = widget.y;

        // If this is the first widget, set it as the current one
        if (index == -1 && widget.isSelectable && widget.enabled)
        {
            index = widgets.Count - 1;
        }
        return this;
    }

    // A method to move to the next widget in the list
    public override void NextOption()
    {
        // Check if there are any widgets in the list
        if (widgets.Count == 0)
            return;

        for (int i = index + 1; i < widgets.Count; i++)
        {
            if (widgets[i].isSelectable && widgets[i].enabled)
            {
                index = i;
                //widgets[index].Draw();
                DrawAll();
                ConfigManager.WriteConsole($"[GenericWidgetContainer.NextOption] {widgets[i].name} idx: {index} enabled: {widgets[i].enabled}");
                return;
            }
        }
    }

    public void SetOption(int idx)
    {
        // Check if there are any widgets in the list
        if (widgets.Count == 0 || idx < 0 || idx > widgets.Count - 1)
        {
            return;
        }
        if (widgets[idx].isSelectable && widgets[idx].enabled)
        {
            index = idx;
        }
        DrawAll();
    }

    // A method to move to the previous widget in the list
    public override void PreviousOption()
    {
        // Check if there are any widgets in the list
        if (widgets.Count == 0)
        {
            return;
        }

        // Decrement the index and wrap around if needed
        for (int i = index - 1; i >= 0; i--)
        {
            if (widgets[i].isSelectable && widgets[i].enabled)
            {
                index = i;
                //widgets[index].Draw();
                DrawAll();
                ConfigManager.WriteConsole($"[GenericWidgetContainer.PreviousOption] {widgets[i].name} idx: {index} enabled: {widgets[i].enabled}");
                return;
            }
        }

    }

    // A method to recover a widget from the container
    public GenericWidget GetWidget(string name)
    {
        // Check if the name is valid
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("The name must not be null or empty");
        }

        // Loop through the list of widgets
        foreach (GenericWidget widget in widgets)
        {
            // If the name matches, return the widget
            if (widget.name == name)
            {
                return widget;
            }
        }

        // If no widget is found, return null
        return null;
    }
    // A method to recover a widget from the container
    public GenericWidget GetSelectedWidget()
    {
        if (index == -1)
            return null;
        return widgets[index];
    }

    // A method to draw all the widgets
    public void DrawAll()
    {
        if (!enabled)
            return;

        for (int i = 0; i < widgets.Count; i++) // Loop through the widgets
        {
            GenericWidget widget = widgets[i]; // Get the current widget
            if (widget.enabled)
            {
                int xpos = widget.x > 1 ? widget.x - 1 : 0;
                if (i == index) // If it is the selected widget
                    screen.Print(xpos, widget.y, ">", true); // Print a ">" character before its x coordinate with normal colors
                else
                    screen.Print(xpos, widget.y, " ", false); // Print a space character before its x coordinate with normal colors
                widget.Draw(); // Print the widget itself
            }
        }
    }

    // A method to draw only this container on screen. Override from GenericWidget.
    public override void Draw()
    {
        // Call DrawAll() to draw all the widgets in this container
        DrawAll();
    }
}
