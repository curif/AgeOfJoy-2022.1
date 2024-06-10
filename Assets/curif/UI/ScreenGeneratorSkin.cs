using System;
using UnityEngine;

public class ScreenGeneratorSkin
{
    private const string DEFAULT_SKIN = "c64";

    // Supported skins
    public const string APPLE2 = "apple2";
    public const string C64 = "c64";
    public const string CPC = "cpc";
    public const string MSX = "msx";
    public const string MSX_MONO = "msx_mono";
    public const string TO7 = "to7";
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
            case APPLE2:
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new AppleIIScreenGeneratorFont(screenGenerator),
                    ColorSpace = new AppleIIColorSpace(),
                    BorderColor = new Color32(0, 0, 0, 255),
                    BootMessages = new string[]{
                          "\n"
                        + "\n"
                        + "               APPLE II\n"
                        + "    DOS VERSION 3.3  SYSTEM MASTER\n"
                        + "\n"
                        + "            JANUARY 1, 1933\n"
                        + "\n"
                        + "COPYRIGHT APPLE COMPUTER,INC. 1980,1982"
                        + "\n",
                        "]"
                    }
                };
            case C64:
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
                        "LOAD\"*.*\",8,1"
                    }
                };
            case CPC:
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
                        + " \u00A91985 Amstrad Consumer Electronics plc\n"
                        + "           and Locomotive Software Ltd.\n"
                        + " BASIC 1.1\n"
                        + "\n"
                        + "Ready",
                        "RUN\"DISC"
                    }
                };
            case MSX:
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new MSXScreenGeneratorFont(screenGenerator),
                    ColorSpace = new MSXColorSpace(),
                    BorderColor = new Color32(89, 85, 224, 255),
                    BootMessages = new string[]{
                          "MSX BASIC version 1.0\n"
                        + "Copyright 1983 by Microsoft\n"
                        + "28815 Bytes free\n"
                        + "Ok\n"
                        + ScreenGeneratorFont.STR_GLYPH_SQUARE + "\n"
                        + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
                        + "color  auto   goto   list   run"
                    }
                };
            case MSX_MONO:
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new MSXScreenGeneratorFont(screenGenerator),
                    ColorSpace = new MSXMonoColorSpace(),
                    BorderColor = new Color32(0, 60, 0, 255),
                    BootMessages = new string[]{
                          "MSX BASIC version 1.0\n"
                        + "Copyright 1983 by Microsoft\n"
                        + "28815 Bytes free\n"
                        + "Ok\n"
                        + ScreenGeneratorFont.STR_GLYPH_SQUARE + "\n"
                        + "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
                        + "color  auto   goto   list   run"
                    }
                };
            case TO7:
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new TO7ScreenGeneratorFont(screenGenerator),
                    ColorSpace = new TO7ColorSpace(),
                    BorderColor = new Color32(0, 255, 255, 255),
                    BootMessages = new string[]{
                          "BASIC Version 1.0\n"
                        + "(c) Microsoft 1982\n"
                        + "\n"
                        + "OK",
                        "RUN\""
                    }
                };
            case ZX:
                return new ScreenGeneratorSkin
                {
                    Name = name,
                    Font = new ZXScreenGeneratorFont(screenGenerator),
                    ColorSpace = new ZXColorSpace(),
                    BorderColor = new Color32(215, 215, 215, 255),
                    BootMessages = new string[]{
                          "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n"
                        + "\u00A9 1982 Sinclair Research Ltd",
                        "RUN\""
                    }
                };
            default:
                ConfigManager.WriteConsoleError($"[ScreenGeneratorSkin.GetSkin] Unknown skin, defaulting to {DEFAULT_SKIN}: ({name})");
                return GetSkin(DEFAULT_SKIN, screenGenerator);
        }
    }
}
