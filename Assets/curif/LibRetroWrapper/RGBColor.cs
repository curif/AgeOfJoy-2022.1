using UnityEngine;

public class RGBColor
{
    public byte r = 255;
    public byte g = 255;
    public byte b = 255;
    public byte a = 255; // Alpha channel as byte

    public float intensity = 0; // Additional intensity parameter

    /// <summary>
    /// Initializes a new instance of the RGBColor class with default values (white, opaque, no intensity).
    /// </summary>
    public RGBColor() { }

    /// <summary>
    /// Initializes a new instance of the RGBColor class from a UnityEngine.Color and an intensity value.
    /// Converts the float Color components (0-1) to byte (0-255).
    /// </summary>
    /// <param name="c">The UnityEngine.Color struct (with float components 0-1).</param>
    /// <param name="intensity">The intensity value for this color.</param>
    public RGBColor(Color c, float intensity)
    {
        // Safely convert Color (float 0-1) to Color32 (byte 0-255)
        // Color32 constructor handles the conversion from float components
        Color32 c32 = new Color32((byte)(c.r * 255f), (byte)(c.g * 255f), (byte)(c.b * 255f), (byte)(c.a * 255f));

        r = c32.r;
        g = c32.g;
        b = c32.b;
        a = c32.a;
        this.intensity = intensity;
    }

    /// <summary>
    /// Initializes a new instance of the RGBColor class from a UnityEngine.Color32 and an intensity value.
    /// </summary>
    /// <param name="c">The UnityEngine.Color32 struct (with byte components 0-255).</param>
    /// <param name="intensity">The intensity value for this color.</param>
    public RGBColor(Color32 c, float intensity)
    {
        r = c.r;
        g = c.g;
        b = c.b;
        a = c.a;
        this.intensity = intensity;
    }

    /// <summary>
    /// Initializes a new instance of the RGBColor class from byte RGB components and an intensity value,
    /// assuming full opacity (alpha=255).
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="intensity">The intensity value for this color.</param>
    public RGBColor(byte r, byte g, byte b, float intensity)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 255; // Default to opaque if only RGB is provided
        this.intensity = intensity;
    }

    /// <summary>
    /// Initializes a new instance of the RGBColor class from byte RGBA components and an intensity value.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-255).</param>
    /// <param name="intensity">The intensity value for this color.</param>
    public RGBColor(byte r, byte g, byte b, byte a, float intensity)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
        this.intensity = intensity;
    }


    /// <summary>
    /// Creates a new RGBColor instance from a UnityEngine.Color and an intensity value.
    /// This is a static factory method.
    /// </summary>
    /// <param name="unityColor">The UnityEngine.Color to convert (uses float 0-1 values).</param>
    /// <param name="intensity">The desired intensity for the resulting RGBColor.</param>
    /// <returns>A new RGBColor instance with byte components and the specified intensity.</returns>
    public static RGBColor FromColor(UnityEngine.Color unityColor, float intensity)
    {
        // UnityEngine.Color uses float values (0-1), RGBColor uses byte values (0-255).
        // We need to convert these. Color32 is a convenient struct in Unity for this.
        // The Color32 constructor takes float components and converts them to byte.
        Color32 color32 = new Color32(
            (byte)(unityColor.r * 255f),
            (byte)(unityColor.g * 255f),
            (byte)(unityColor.b * 255f),
            (byte)(unityColor.a * 255f)
        );

        // Now we can use the existing constructor that takes Color32
        return new RGBColor(color32, intensity);
    }


    /// <summary>
    /// Converts this RGBColor back to a UnityEngine.Color struct, applying the stored intensity value.
    /// The intensity factor is calculated as Math.Pow(2, intensity). The result is in linear color space.
    /// </summary>
    /// <returns>A UnityEngine.Color struct representing this RGBColor with intensity applied.</returns>
    public virtual Color getColor()
    {
        // Factor to apply based on intensity (e.g., intensity 1 means factor 2)
        float factor = Mathf.Pow(2, intensity);

        // Convert byte components to float (0-1) using Color32 intermediary
        Color baseColor = new Color32(r, g, b, a);

        // Apply the intensity factor to the RGB channels
        Color finalColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);

        return finalColor.linear;
    }
    public virtual Color getColorNoLinear()
    {
        // Factor to apply based on intensity (e.g., intensity 1 means factor 2)
        float factor = Mathf.Pow(2, intensity);

        // Convert byte components to float (0-1) using Color32 intermediary
        Color baseColor = new Color32(r, g, b, a);

        // Apply the intensity factor to the RGB channels
        Color finalColor = new Color(baseColor.r * factor, baseColor.g * factor, baseColor.b * factor, baseColor.a);

        return finalColor;
    }

    /// <summary>
    /// Converts this RGBColor back to a UnityEngine.Color struct without applying the stored intensity value.
    /// The result is in linear color space.
    /// </summary>
    /// <returns>A UnityEngine.Color struct representing this RGBColor without intensity applied.</returns>
    public virtual Color getColorNoIntensity()
    {
        // Convert byte components directly to float (0-1) using Color32 intermediary
        Color color = new Color32(r, g, b, a);

        // Returning linear color space
        return color.linear;
    }

    /// <summary>
    /// Returns a string that represents the current RGBColor object, including RGBA and intensity.
    /// </summary>
    /// <returns>A string representation of the object.</returns>
    public override string ToString()
    {
        return $"RGBColor(r:{r}, g:{g}, b:{b}, a:{a}, intensity:{intensity:F2})";
    }

    /// <summary>
    /// Formats the RGBColor (excluding alpha) and intensity into a basic list string using a specified separator.
    /// Useful for simple data output.
    /// </summary>
    /// <param name="separator">The string to use between the R, G, B, and intensity values.</param>
    /// <returns>A string formatted as "r{separator}g{separator}b{separator}intensity".</returns>
    public string ToAGEBasicList(string separator)
    {
        return $"{r}{separator}{g}{separator}{b}{separator}{intensity:F2}";
    }
}