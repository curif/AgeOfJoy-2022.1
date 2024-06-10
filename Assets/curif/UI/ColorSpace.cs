
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
            { "green", new Color32(0, 255, 0, 255) },
            { "purple", new Color32(255, 0, 255, 255) },
            { "white", new Color32(255, 255, 255, 255) },
            { "orange", new Color32(255, 128, 0, 255) },
            { "blue", new Color32(0, 0, 255, 255) },
            { "yellow", new Color32(255, 255, 0, 255) },
            { "cyan", new Color32(0, 255, 255, 255) }
        }, new Color32(255, 255, 255, 255), new Color32(0, 0, 0, 255));
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
