using System;
using UnityEngine;

public class ScreenGeneratorSkin
{
    private const string DEFAULT_SKIN = "c64";

    // Supported skins
    public const string C64 = "c64";
    public const string CPC = "cpc";
    public const string ZX = "zx";

    public string Name;
    public ScreenGeneratorFont Font;
    public ColorSpaceBase ColorSpace;
    public Color32 BorderColor;
    public string[] BootMessages;

    public static ScreenGeneratorSkin GetSkin(string name, ScreenGenerator screenGenerator)
    {
        switch (name)
        {
            case "c64":
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new C64ScreenGeneratorFont(screenGenerator),
                    ColorSpace = new C64ColorSpace(),
                    BorderColor = new Color32(134, 122, 222, 255),
                    BootMessages = new string[]{
                          "\n"
                        + "    **** COMMODORE 64 BASIC V2 ****\n"
                        + "\n"
                        + " 64K RAM SYSTEM 38911 BASIC BYTES FREE\n"
                        + "\n"
                        + "READY.",
                        "LOAD '*.*', 8, 1"
                    }
                };
            case "cpc":
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new CPCScreenGeneratorFont(screenGenerator),
                    ColorSpace = new CPCColorSpace(),
                    BorderColor = new Color32(0, 0, 102, 255),
                    BootMessages = new string[]{
                          "\n"
                        + " Amstrad 128K Microcomputer  (f3)\n"
                        + "\n"
                        + " ©1985 Amstrad Consumer Electronics plc\n"
                        + "           and Locomotive Software Ltd.\n"
                        + " BASIC 1.1\n"
                        + "\n"
                        + "Ready",
                        "RUN\"DISC"
                    }
                };
            case "zx":
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new ZXScreenGeneratorFont(screenGenerator),
                    ColorSpace = new ZXColorSpace(),
                    BorderColor = new Color32(215, 215, 215, 255),
                    BootMessages = new string[]{
                          "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
                        + "© 1982 Sinclair Research Ltd",
                        "RUN\""
                    }
                };
            default:
                ConfigManager.WriteConsoleError($"[ScreenGeneratorSkin.GetSkin] Unknown skin, defaulting to {DEFAULT_SKIN}: ({name})");
                return GetSkin(DEFAULT_SKIN, screenGenerator);
        }
    }
}
