using System.Globalization;

namespace MudShadcn;

/// <summary>
/// A CSS <c>oklch()</c> colour, to give MudBlazor's palette the sRGB hex value a browser shows for
/// a shadcn token (MudBlazor's <c>MudColor</c> does not read OKLCH).
/// </summary>
internal readonly record struct OklchColor(double L, double C, double H, double Alpha)
{
    /// <summary>
    /// Reads <c>oklch(0.577 0.245 27.325)</c>, <c>oklch(1 0 0 / 10%)</c> and Tailwind's
    /// <c>oklch(98.5% 0 none)</c>.
    /// </summary>
    public static OklchColor Parse(string css)
    {
        var open = css.IndexOf('(');
        var close = css.LastIndexOf(')');
        if (!css.StartsWith("oklch", StringComparison.OrdinalIgnoreCase) || open < 0 || close < open)
        {
            throw new FormatException($"Not an oklch() colour: '{css}'.");
        }

        var parts = css[(open + 1)..close].Split('/');
        var channels = parts[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (channels.Length != 3)
        {
            throw new FormatException($"Not an oklch() colour: '{css}'.");
        }

        return new OklchColor(
            Number(channels[0], percentOf: 1),
            Number(channels[1], percentOf: 0.4),
            Number(channels[2], percentOf: 1),
            parts.Length > 1 ? Number(parts[1].Trim(), percentOf: 1) : 1);
    }

    private static double Number(string text, double percentOf)
    {
        if (text == "none")
        {
            return 0;
        }

        return text.EndsWith('%')
            ? double.Parse(text[..^1], CultureInfo.InvariantCulture) / 100 * percentOf
            : double.Parse(text, CultureInfo.InvariantCulture);
    }

    /// <summary>The colour in sRGB, each channel clipped to the gamut as a browser on an sRGB display does.</summary>
    public (byte R, byte G, byte B) ToRgb()
    {
        var hue = H * Math.PI / 180;
        var a = C * Math.Cos(hue);
        var b = C * Math.Sin(hue);

        // OKLab to linear sRGB (Björn Ottosson's matrices).
        var l = Math.Pow(L + 0.3963377774 * a + 0.2158037573 * b, 3);
        var m = Math.Pow(L - 0.1055613458 * a - 0.0638541728 * b, 3);
        var s = Math.Pow(L - 0.0894841775 * a - 1.2914855480 * b, 3);

        return (
            Channel(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s),
            Channel(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s),
            Channel(-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s));
    }

    private static byte Channel(double linear)
    {
        var encoded = linear <= 0.0031308 ? 12.92 * linear : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;
        return (byte)Math.Round(Math.Clamp(encoded, 0, 1) * 255, MidpointRounding.AwayFromZero);
    }

    /// <summary><c>#rrggbb</c>, or <c>#rrggbbaa</c> when the colour is not opaque.</summary>
    public string ToHex() => ToHex(1);

    /// <summary>As <see cref="ToHex()"/>, with the colour's alpha multiplied by <paramref name="opacity"/> (Tailwind's <c>/50</c>).</summary>
    public string ToHex(double opacity)
    {
        var (r, g, b) = ToRgb();
        var alpha = Alpha * opacity;
        return alpha >= 1
            ? $"#{r:x2}{g:x2}{b:x2}"
            : $"#{r:x2}{g:x2}{b:x2}{(byte)Math.Round(alpha * 255, MidpointRounding.AwayFromZero):x2}";
    }

    /// <summary>This colour at <paramref name="opacity"/> painted over an opaque <paramref name="background"/>.</summary>
    public string Over(OklchColor background, double opacity)
    {
        var (r, g, b) = ToRgb();
        var (br, bg, bb) = background.ToRgb();
        return Mix((r, g, b), (br, bg, bb), opacity);
    }

    /// <summary><paramref name="rgb"/> at <paramref name="opacity"/> painted over this colour.</summary>
    public string Under((byte R, byte G, byte B) rgb, double opacity) => Mix(rgb, ToRgb(), opacity);

    private static string Mix((byte R, byte G, byte B) top, (byte R, byte G, byte B) bottom, double opacity)
    {
        static byte Blend(byte top, byte bottom, double opacity) =>
            (byte)Math.Round(top * opacity + bottom * (1 - opacity), MidpointRounding.AwayFromZero);

        return $"#{Blend(top.R, bottom.R, opacity):x2}{Blend(top.G, bottom.G, opacity):x2}{Blend(top.B, bottom.B, opacity):x2}";
    }

    /// <summary>
    /// The CSS colour <paramref name="css"/> with its alpha multiplied by <paramref name="opacity"/>,
    /// still in OKLCH: Tailwind's <c>bg-input/30</c> for a token that already has an alpha.
    /// </summary>
    public static string WithOpacity(string css, double opacity)
    {
        var color = Parse(css);
        var slash = css.IndexOf('/');
        var channels = (slash < 0 ? css[(css.IndexOf('(') + 1)..css.LastIndexOf(')')] : css[(css.IndexOf('(') + 1)..slash]).Trim();
        var percent = Math.Round(color.Alpha * opacity * 100, 4);
        return $"oklch({channels} / {percent.ToString(CultureInfo.InvariantCulture)}%)";
    }
}
