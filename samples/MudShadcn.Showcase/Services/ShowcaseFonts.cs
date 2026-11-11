#nullable enable

namespace MudShadcn.Showcase.Services;

/// <summary>A font the Theme Generator offers: shadcn/create's list (apps/v4/lib/font-definitions.ts).</summary>
/// <param name="Name">shadcn's name for it, as in a design's address (<c>font=inter</c>).</param>
/// <param name="Title">The font's name, which is also its CSS family name.</param>
/// <param name="Kind"><c>sans</c>, <c>serif</c> or <c>mono</c>.</param>
/// <param name="GoogleFamily">
/// The <c>family=</c> value of the Google Fonts stylesheet that loads it.
/// </param>
/// <param name="InIndexHtml"><c>true</c> when the site's index.html already loads the font (Geist, Geist Mono).</param>
public sealed record ShowcaseFont(string Name, string Title, string Kind, string GoogleFamily, bool InIndexHtml = false);

public static class ShowcaseFonts
{
    /// <summary>Geist, the font ui.shadcn.com uses. index.html loads it.</summary>
    public const string Default = "geist";

    /// <summary>A heading font of <c>inherit</c> is the body font.</summary>
    public const string Inherit = "inherit";

    // shadcn's order. Every GoogleFamily value was checked against fonts.googleapis.com (HTTP 200),
    // and asks for the font's whole weight range (Instrument Serif only has 400).
    public static IReadOnlyList<ShowcaseFont> All { get; } =
    [
        new("geist", "Geist", "sans", "Geist:wght@100..900", true),
        new("inter", "Inter", "sans", "Inter:wght@100..900"),
        new("noto-sans", "Noto Sans", "sans", "Noto+Sans:wght@100..900"),
        new("nunito-sans", "Nunito Sans", "sans", "Nunito+Sans:wght@200..1000"),
        new("figtree", "Figtree", "sans", "Figtree:wght@300..900"),
        new("roboto", "Roboto", "sans", "Roboto:wght@100..900"),
        new("raleway", "Raleway", "sans", "Raleway:wght@100..900"),
        new("dm-sans", "DM Sans", "sans", "DM+Sans:wght@100..1000"),
        new("public-sans", "Public Sans", "sans", "Public+Sans:wght@100..900"),
        new("outfit", "Outfit", "sans", "Outfit:wght@100..900"),
        new("oxanium", "Oxanium", "sans", "Oxanium:wght@200..800"),
        new("manrope", "Manrope", "sans", "Manrope:wght@200..800"),
        new("space-grotesk", "Space Grotesk", "sans", "Space+Grotesk:wght@300..700"),
        new("montserrat", "Montserrat", "sans", "Montserrat:wght@100..900"),
        new("ibm-plex-sans", "IBM Plex Sans", "sans", "IBM+Plex+Sans:wght@100..700"),
        new("source-sans-3", "Source Sans 3", "sans", "Source+Sans+3:wght@200..900"),
        new("instrument-sans", "Instrument Sans", "sans", "Instrument+Sans:wght@400..700"),
        new("jetbrains-mono", "JetBrains Mono", "mono", "JetBrains+Mono:wght@100..800"),
        new("geist-mono", "Geist Mono", "mono", "Geist+Mono:wght@100..900", true),
        new("noto-serif", "Noto Serif", "serif", "Noto+Serif:wght@100..900"),
        new("roboto-slab", "Roboto Slab", "serif", "Roboto+Slab:wght@100..900"),
        new("merriweather", "Merriweather", "serif", "Merriweather:wght@300..900"),
        new("lora", "Lora", "serif", "Lora:wght@400..700"),
        new("playfair-display", "Playfair Display", "serif", "Playfair+Display:wght@400..900"),
        new("eb-garamond", "EB Garamond", "serif", "EB+Garamond:wght@400..800"),
        new("instrument-serif", "Instrument Serif", "serif", "Instrument+Serif:wght@400"),
    ];

    public static ShowcaseFont? Find(string name) => All.FirstOrDefault(font => font.Name == name);

    /// <summary>The font in front of the stack a stock shadcn/ui project falls back to.</summary>
    public static string[] Stack(string name)
    {
        var font = Find(name) ?? Find(Default)!;
        return font.Kind switch
        {
            "mono" => [font.Title, "ui-monospace", "SFMono-Regular", "Menlo", "Monaco", "Consolas", "Liberation Mono", "Courier New", "monospace"],
            "serif" => [font.Title, "ui-serif", "Georgia", "Cambria", "Times New Roman", "Times", "serif"],
            _ => [font.Title, .. MudShadcnTheme.DefaultFontFamily],
        };
    }

    /// <summary>
    /// The Google Fonts stylesheet that loads the given fonts, or <c>null</c> when none is needed.
    /// Only the fonts in use are requested. The site passes <paramref name="skipLoaded"/> because
    /// index.html already loads Geist and Geist Mono; the code a visitor copies does not.
    /// </summary>
    public static string? Href(bool skipLoaded, params string[] names)
    {
        var families = names
            .Select(Find)
            .Where(font => font is not null && !(skipLoaded && font.InIndexHtml))
            .Select(font => font!.GoogleFamily)
            .Distinct()
            .ToArray();
        return families.Length == 0 ? null : $"https://fonts.googleapis.com/css2?family={string.Join("&family=", families)}&display=swap";
    }
}
