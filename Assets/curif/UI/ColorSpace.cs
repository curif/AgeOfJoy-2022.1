
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColorSpaceBase
{
    protected Dictionary<string, Color32> colors;
    protected List<Color32> colorList;
    protected Color32 defaultForeground;
    protected Color32 defaultBackground;

    protected void InitializeColors(Dictionary<string, Color32> colorDict,
                                    Color32 defaultFg, Color32 defaultBg)
    {
        colors = colorDict;
        colorList = new List<Color32>(colors.Values);
        defaultForeground = defaultFg;
        defaultBackground = defaultBg;
    }

    public Color32 GetColorByIndex(int index)
    {
        if (index < 0 || index >= colorList.Count)
        {
            throw new ArgumentOutOfRangeException("index", "Index is out of range.");
        }
        return colorList[index];
    }

    public string GetNameByColor(Color32 color)
    {
        foreach (var pair in colors)
        {
            if (pair.Value.Equals(color))
            {
                return pair.Key;
            }
        }
        return null;
    }

    public Color32 GetColorByName(string name)
    {
        if (colors.ContainsKey(name.ToLower()))
        {
            return colors[name.ToLower()];
        }
        throw new ArgumentException("Color name not found.");
    }

    public Color32 ForegroundDefault()
    {
        return defaultForeground;
    }

    public Color32 BackgroundDefault()
    {
        return defaultBackground;
    }
}

// CPC (color)
public class CPCColorSpace : ColorSpaceBase
{
    public CPCColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "blue", new Color32(0, 0, 102, 255) },
            { "bright_blue", new Color32(0, 0, 255, 255) },
            { "red", new Color32(102, 0, 0, 255) },
            { "magenta", new Color32(102, 0, 102, 255) },
            { "mauve", new Color32(102, 0, 255, 255) },
            { "bright_red", new Color32(255, 0, 0, 255) },
            { "purple", new Color32(255, 0, 102, 255) },
            { "bright_magenta", new Color32(255, 0, 255, 255) },
            { "green", new Color32(0, 102, 0, 255) },
            { "cyan", new Color32(0, 102, 102, 255) },
            { "sky_blue", new Color32(0, 102, 255, 255) },
            { "yellow", new Color32(102, 102, 0, 255) },
            { "white", new Color32(102, 102, 128, 255) },
            { "pastel_blue", new Color32(102, 102, 255, 255) },
            { "orange", new Color32(255, 102, 0, 255) },
            { "pink", new Color32(255, 102, 102, 255) },
            { "pastel_magenta", new Color32(255, 102, 255, 255) },
            { "bright_green", new Color32(0, 255, 0, 255) },
            { "sea_green", new Color32(0, 255, 102, 255) },
            { "bright_cyan", new Color32(0, 255, 255, 255) },
            { "lime", new Color32(102, 255, 0, 255) },
            { "pastel_green", new Color32(102, 255, 102, 255) },
            { "pastel_cyan", new Color32(102, 255, 255, 255) },
            { "bright_yellow", new Color32(255, 255, 0, 255) },
            { "pastel_yellow", new Color32(255, 255, 102, 255) },
            { "bright_white", new Color32(255, 255, 255, 255) }
        }, new Color32(255, 255, 0, 255), new Color32(0, 0, 102, 255));
    }
}

// CPC (green monochrome)
public class CPCMonoColorSpace : ColorSpaceBase
{
    public CPCMonoColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 60, 0, 255) },
            { "blue", new Color32(5, 68, 5, 255) },
            { "bright_blue", new Color32(10, 75, 10, 255) },
            { "red", new Color32(16, 84, 16, 255) },
            { "magenta", new Color32(21, 92, 21, 255) },
            { "mauve", new Color32(26, 99, 26, 255) },
            { "bright_red", new Color32(30, 105, 30, 255) },
            { "purple", new Color32(35, 113, 35, 255) },
            { "bright_magenta", new Color32(40, 120, 40, 255) },
            { "green", new Color32(48, 132, 48, 255) },
            { "cyan", new Color32(53, 140, 53, 255) },
            { "sky_blue", new Color32(58, 147, 58, 255) },
            { "yellow", new Color32(64, 156, 64, 255) },
            { "white", new Color32(69, 164, 69, 255) },
            { "pastel_blue", new Color32(74, 171, 74, 255) },
            { "orange", new Color32(78, 177, 78, 255) },
            { "pink", new Color32(83, 185, 83, 255) },
            { "pastel_magenta", new Color32(88, 192, 88, 255) },
            { "bright_green", new Color32(90, 195, 90, 255) },
            { "sea_green", new Color32(95, 203, 95, 255) },
            { "bright_cyan", new Color32(100, 210, 100, 255) },
            { "lime", new Color32(106, 219, 106, 255) },
            { "pastel_green", new Color32(97, 206, 97, 255) },
            { "pastel_cyan", new Color32(116, 234, 116, 255) },
            { "bright_yellow", new Color32(120, 240, 120, 255) },
            { "pastel_yellow", new Color32(125, 248, 125, 255) },
            { "bright_white", new Color32(130, 255, 130, 255) }
        }, new Color32(120, 240, 120, 255), new Color32(0, 60, 0, 255));
    }
}

// MSX
public class MSXColorSpace : ColorSpaceBase
{
    public MSXColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
           { "transparent", new Color32(0, 0, 0, 255) },
           { "black", new Color32(1, 1, 1, 255) },
           { "green", new Color32(62, 184, 73, 255) },
           { "light green", new Color32(116, 208, 125, 255) },
           { "blue", new Color32(89, 85, 224, 255) },
           { "light blue", new Color32(128, 118, 241, 255) },
           { "dark red", new Color32(185, 94, 81, 255) },
           { "cyan", new Color32(101, 219, 239, 255) },
           { "red", new Color32(219, 101, 89, 255) },
           { "light red", new Color32(255, 137, 125, 255) },
           { "yellow", new Color32(204, 195, 94, 255) },
           { "light yellow", new Color32(222, 208, 135, 255) },
           { "dark green", new Color32(58, 162, 65, 255) },
           { "magenta", new Color32(183, 102, 181, 255) },
           { "gray", new Color32(204, 204, 204, 255) },
           { "white", new Color32(255, 255, 255, 255) },
        }, new Color32(255, 255, 255, 255), new Color32(89, 85, 224, 255));
    }
}

// MSX (green monochrome)
public class MSXMonoColorSpace : ColorSpaceBase
{
    public MSXMonoColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
           { "transparent", new Color32(0, 60, 0, 255) },
           { "black", new Color32(5, 68, 5, 255) },
           { "green", new Color32(10, 75, 10, 255) },
           { "light green", new Color32(16, 84, 16, 255) },
           { "blue", new Color32(21, 92, 21, 255) },
           { "light blue", new Color32(26, 99, 26, 255) },
           { "dark red", new Color32(30, 105, 30, 255) },
           { "cyan", new Color32(35, 113, 35, 255) },
           { "red", new Color32(40, 120, 40, 255) },
           { "light red", new Color32(48, 132, 48, 255) },
           { "yellow", new Color32(53, 140, 53, 255) },
           { "light yellow", new Color32(58, 147, 58, 255) },
           { "dark green", new Color32(64, 156, 64, 255) },
           { "magenta", new Color32(69, 164, 69, 255) },
           { "gray", new Color32(74, 171, 74, 255) },
           { "white", new Color32(78, 177, 78, 255) },
        }, new Color32(78, 177, 78, 255), new Color32(0, 60, 0, 255));
    }
}

// IBM PC CGA graphics mode
public class IMBPCColorSpace : ColorSpaceBase
{
    public IMBPCColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "blue", new Color32(0, 0, 170, 255) },
            { "green", new Color32(0, 170, 0, 255) },
            { "cyan", new Color32(0, 170, 170, 255) },
            { "red", new Color32(170, 0, 0, 255) },
            { "magenta", new Color32(170, 0, 170, 255) },
            { "brown", new Color32(170, 85, 0, 255) },
            { "light_gray", new Color32(170, 170, 170, 255) },
            { "dark_gray", new Color32(85, 85, 85, 255) },
            { "light_blue", new Color32(85, 85, 255, 255) },
            { "light_green", new Color32(85, 255, 85, 255) },
            { "light_cyan", new Color32(85, 255, 255, 255) },
            { "light_red", new Color32(255, 85, 85, 255) },
            { "light_magenta", new Color32(255, 85, 255, 255) },
            { "yellow", new Color32(255, 255, 85, 255) },
            { "white", new Color32(255, 255, 255, 255) }
        }, new Color32(255, 255, 255, 255), new Color32(0, 0, 0, 255));
    }
}

public class C64ColorSpace : ColorSpaceBase
{
    public C64ColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "white", new Color32(255, 255, 255, 255) },
            { "red", new Color32(136, 0, 0, 255) },
            { "cyan", new Color32(170, 255, 238, 255) },
            { "violet", new Color32(204, 68, 204, 255) },
            { "green", new Color32(0, 204, 85, 255) },
            { "blue", new Color32(0, 0, 170, 255) },
            { "yellow", new Color32(238, 238, 119, 255) },
            { "orange", new Color32(221, 136, 85, 255) },
            { "brown", new Color32(102, 68, 0, 255) },
            { "light_red", new Color32(255, 119, 119, 255) },
            { "darkgrey", new Color32(51, 51, 51, 255) },
            { "grey", new Color32(119, 119, 119, 255) },
            { "light_green", new Color32(170, 255, 102, 255) },
            { "light_blue", new Color32(0, 136, 255, 255) },
            { "light_grey", new Color32(187, 187, 187, 255) }
        }, new Color32(255, 255, 255, 255), new Color32(13, 58, 219, 255));
    }
}


public class ZXColorSpace : ColorSpaceBase
{
    public ZXColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "blue", new Color32(0, 0, 215, 255) },
            { "red", new Color32(215, 0, 0, 255) },
            { "magenta", new Color32(215, 0, 215, 255) },
            { "green", new Color32(0, 215, 0, 255) },
            { "cyan", new Color32(0, 215, 215, 255) },
            { "yellow", new Color32(215, 215, 0, 255) },
            { "white", new Color32(215, 215, 215, 255) },
            { "bright_black", new Color32(0, 0, 0, 255) },
            { "bright_blue", new Color32(0, 0, 255, 255) },
            { "bright_red", new Color32(255, 0, 0, 255) },
            { "bright_magenta", new Color32(255, 0, 255, 255) },
            { "bright_green", new Color32(0, 255, 0, 255) },
            { "bright_cyan", new Color32(0, 255, 255, 255) },
            { "bright_yellow", new Color32(255, 255, 0, 255) },
            { "bright_white", new Color32(255, 255, 255, 255) }
        }, new Color32(0, 0, 0, 255), new Color32(215, 215, 215, 255));
    }
}


// Apple II color space
public class AppleIIColorSpace : ColorSpaceBase
{
    public AppleIIColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "red", new Color32(153, 3, 95, 255) },
            { "blue", new Color32(55, 4, 225, 255) },
            { "purple", new Color32(0, 0, 0, 255) },
            { "green", new Color32(0, 116, 16, 255) },
            { "gray", new Color32(127, 127, 127, 255) },
            { "medium_blue", new Color32(36, 151, 255, 255) },
            { "light_blue", new Color32(170, 162, 255, 255) },
            { "brown", new Color32(79, 81, 1, 255) },
            { "orange", new Color32(240, 92, 0, 255) },
            { "light_gray", new Color32(190, 190, 190, 255) },
            { "pink", new Color32(255, 133, 225, 255) },
            { "light_green", new Color32(18, 202, 7, 255) },
            { "yellow", new Color32(206, 212, 19, 255) },
            { "aqua", new Color32(81, 245, 149, 255) },
            { "white", new Color32(255, 255, 254, 255) },
        }, new Color32(81, 245, 149, 255), new Color32(0, 0, 0, 255));
    }
}

// Atari 2600 color space
public class Atari2600ColorSpace : ColorSpaceBase
{
    public Atari2600ColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "white", new Color32(195, 195, 195, 255) },
            { "red", new Color32(228, 0, 88, 255) },
            { "cyan", new Color32(0, 255, 255, 255) },
            { "purple", new Color32(177, 73, 224, 255) },
            { "green", new Color32(0, 144, 0, 255) },
            { "blue", new Color32(0, 0, 168, 255) },
            { "yellow", new Color32(248, 252, 112, 255) }
        }, new Color32(195, 195, 195, 255), new Color32(0, 0, 0, 255));
    }
}

// TO7 color space
public class TO7ColorSpace : ColorSpaceBase
{
    public TO7ColorSpace()
    {
        InitializeColors(new Dictionary<string, Color32>
        {
            { "black", new Color32(0, 0, 0, 255) },
            { "red", new Color32(255, 42, 42, 255) },
            { "green", new Color32(0, 255, 0, 255) },
            { "yellow", new Color32(255, 255, 0, 255) },
            { "blue", new Color32(42, 42, 255, 255) },
            { "pink", new Color32(255, 255, 0, 255) },
            { "cyan", new Color32(0, 255, 255, 255) },
            { "white", new Color32(255, 255, 255, 255) }
        }, new Color32(42, 42, 255, 255), new Color32(0, 255, 255, 255));
    }
}

public static class ColorSpaceManager
{
    private static Dictionary<string, ColorSpaceBase> colorSpaces = new Dictionary<string, ColorSpaceBase>()
    {
        { "ibmpc", new IMBPCColorSpace() },
        { "appleii", new AppleIIColorSpace() },
        { "atari2600", new Atari2600ColorSpace() },
        { "zx", new ZXColorSpace() },
        { "c64", new C64ColorSpace() },
        { "cpc", new CPCColorSpace() }
    };

    public static ColorSpaceBase GetColorSpace(string name)
    {
        if (colorSpaces.ContainsKey(name))
        {
            return colorSpaces[name];
        }
        throw new ArgumentException($"Color space not found: {name}.");
    }

    public static Color32 GetColorByName(string colorSpaceName, string colorName)
    {
        var colorSpace = GetColorSpace(colorSpaceName);
        return colorSpace.GetColorByName(colorName);
    }

    public static string GetNameByColor(string colorSpaceName, Color32 color)
    {
        var colorSpace = GetColorSpace(colorSpaceName);
        return colorSpace.GetNameByColor(color);
    }

    public static Color32 ForegroundDefault(string colorSpaceName)
    {
        var colorSpace = GetColorSpace(colorSpaceName);
        return colorSpace.ForegroundDefault();
    }

    public static Color32 BackgroundDefault(string colorSpaceName)
    {
        var colorSpace = GetColorSpace(colorSpaceName);
        return colorSpace.BackgroundDefault();
    }

    public static Color32 GetColorByIndex(string colorSpaceName, int index)
    {
        var colorSpace = GetColorSpace(colorSpaceName);
        return colorSpace.GetColorByIndex(index);
    }
}
