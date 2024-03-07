using OpenTK.Mathematics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace GraphicsPlayground.Util;

/// <summary> Color helper class that primarily uses Vector3 for OpenTK compatibility. </summary>
public static class ColorHelper
{
    public static List<ColoredString> ConvertTextToColoredStrings(string text)
    {
        string pattern = @"<(\w+)>([^<]*)";
        List<ColoredString> coloredStrings = new();
        MatchCollection matches = Regex.Matches(text, pattern);
        foreach (Match match in matches.Cast<Match>())
        {
            string color = match.Groups[1].Value;
            string str = match.Groups[2].Value;
            if (str.StartsWith(" "))
            {
                str = str[1..];
            }
            KnownColor? knownColor = KnownColorFromString(color);
            if (knownColor.HasValue)
            {
                coloredStrings.Add(new ColoredString(str, ColorFromKnownColor(knownColor.Value)));
                continue;
            }
            // Check if color is rgb
            string[] rgb = color.Split(',');
            if (rgb.Length == 3)
            {
                coloredStrings.Add(new ColoredString(str, ColorFromRgb(float.Parse(rgb[0]), float.Parse(rgb[1]), float.Parse(rgb[2]))));
                continue;
            }
            // Check if color is hex
            if (color.Length == 6)
            {
                coloredStrings.Add(new ColoredString(str, ColorFromHex(color)));
                continue;
            }
            // Default to white if color is not recognized
            coloredStrings.Add(new ColoredString(str, ColorFromRgb(255, 255, 255)));
        }
        if (matches.Count == 0)
        {
            coloredStrings.Add(new ColoredString(text, ColorFromRgb(255, 255, 255)));
        }
        return coloredStrings;
    }

    public struct ColoredString(string str, Vector3 color)
    {
        public string String { get; set; } = str;
        public Vector3 Color { get; set; } = color;
    }

    public static KnownColor? KnownColorFromString(string color)
    {
        foreach (KnownColor knownColor in Enum.GetValues(typeof(KnownColor)))
        {
            if (knownColor.ToString().Equals(color, StringComparison.OrdinalIgnoreCase))
            {
                return knownColor;
            }
        }
        return null;
    }

    public static Color DefaultNormalMapColor => Color.FromArgb(127, 127, 255);

    public static Vector3 ColorFromKnownColor(KnownColor color) => ColorFromColorClass(Color.FromKnownColor(color));

    public static Vector3 ColorFromColorClass(Color color) => ColorFromRgb(color.R, color.G, color.B);

    public static Vector3 ColorFromRgb(float r, float g, float b) => new(r / 255f, g / 255f, b / 255f);

    public static Vector3 ColorFromHex(string hex)
    {
        if (hex.Length != 6)
        {
            throw new ArgumentException("Hex string must be 6 characters long.");
        }

        float r = Convert.ToInt32(hex[..2], 16);
        float g = Convert.ToInt32(hex.Substring(2, 2), 16);
        float b = Convert.ToInt32(hex.Substring(4, 2), 16);

        return ColorFromRgb(r, g, b);
    }

    public static Vector3 ColorFromHex(int hex) => ColorFromHex(hex.ToString("X"));

    public static Vector3 ColorFromHex(uint hex) => ColorFromHex(hex.ToString("X"));

    public static Vector3 ColorFromHex(byte r, byte g, byte b) => ColorFromRgb(r, g, b);

    public static System.Numerics.Vector3 ToSystemVector3(Vector3 color) => new(color.X, color.Y, color.Z);
}
