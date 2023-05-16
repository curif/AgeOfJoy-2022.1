
using System;
using System.Threading;

class C64BootScreen {
    private ScreenGenerator screen;
    private string[] messages = {
        "**** COMMODORE 64 BASIC V2 ****",
        "64K RAM SYSTEM 38911 BASIC BYTES FREE",
        "READY.",
    };
    private int currentLine = 0;

    public C64BootScreen(ScreenGenerator screen) {
        this.screen = screen;
    }

    public bool PrintNextLine() {
        if (currentLine >= messages.Length) {
            return true;
        }

        screen.Print(0, currentLine + 5, messages[currentLine], false);
        currentLine++;

        return false;
    }
}
