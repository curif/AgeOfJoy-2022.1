
using System;
using System.Threading;

class BootScreen
{
    private ScreenGenerator screen;
    private string[] messages = { };
    private int currentLine = 0;
    private int yPosition = 0;

    public BootScreen(ScreenGenerator screen)
    {
        this.screen = screen;
    }

    public bool PrintNextLine()
    {
        if (currentLine >= messages.Length)
        {
            return true;
        }
        yPosition += screen.Print(0, yPosition, messages[currentLine++]);
        return false;
    }

    public void Reset()
    {
        messages = screen.Skin.BootMessages;
        currentLine = 0;
        yPosition = 0;
    }
}
