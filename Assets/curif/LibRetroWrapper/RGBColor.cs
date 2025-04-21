using UnityEngine;

public class RGBColor
{
    public byte r = 255;
    public byte g = 255;
    public byte b = 255;
    public byte a = 255;

    public float intensity = 0;

    public RGBColor() { }

    public RGBColor(Color c, float intensity)
    {
        /*
        r = (byte)(c.r * 255);
        g = (byte)(c.g * 255);
        b = (byte)(c.b * 255);
        a = (byte)(c.a * 255);
        */
        r = (byte)c.r;
        g = (byte)c.g;
        b = (byte)c.b;
        a = (byte)c.a;
        this.intensity = intensity; // Assign the passed intensity
    }

    public RGBColor(Color32 c, float intensity)
    {
        r = c.r;
        g = c.g;
        b = c.b;
        a = c.a;
        this.intensity = intensity; // Assign the passed intensity
    }

    public RGBColor(byte r, byte g, byte b, float intensity)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 1;
        this.intensity = intensity; // Assign the passed intensity
    }

    public virtual Color getColor()
    {
        float factor = Mathf.Pow(2, intensity);
        Color c = new Color32(r, g, b, a);
        Color cf = new Color(c.r * factor, c.g * factor, c.b * factor, a);
        return cf.linear;
    }

    public virtual Color getColorNoIntensity()
    {
        Color color = new Color32(r, g, b, a);
        return color.linear;
    }

    public override string ToString()
    {
        return $"RGBColor(r:{r}, g:{g}, b:{b}, a:{a}, intensity:{intensity:F2})";
    }

    public string ToAGEBasicList(string separator)
    {
        return $"{r}{separator}{g}{separator}{b}{separator}{intensity:F2}";
    }
}