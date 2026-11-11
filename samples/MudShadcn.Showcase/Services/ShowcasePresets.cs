#nullable enable

namespace MudShadcn.Showcase.Services;

/// <summary>A preset of the Theme Generator: shadcn/create's Preset list, a design chosen in one click.</summary>
public sealed record ShowcasePreset(string Name, string Title, string Description, ShowcaseDesign Design)
{
    /// <summary>Shown as "Custom" when the design matches none of <see cref="All"/>.</summary>
    public const string Custom = "custom";

    /// <summary>
    /// MudShadcn's own default, then shadcn's <c>radix-*</c> presets (PRESETS in
    /// apps/v4/registry/config.ts) minus the icon library, which MudBlazor has no choice about.
    /// shadcn's chart colour <c>neutral</c> is "the theme's own", which is <see cref="ShadcnColor.Base"/> here.
    /// </summary>
    public static IReadOnlyList<ShowcasePreset> All { get; } =
    [
        new("mudshadcn", "MudShadcn", "New York / Neutral / Geist", new ShowcaseDesign()),
        Shadcn("vega", ShadcnStyle.Vega, "Inter"),
        Shadcn("nova", ShadcnStyle.Nova, "Geist"),
        Shadcn("maia", ShadcnStyle.Maia, "Figtree"),
        Shadcn("lyra", ShadcnStyle.Lyra, "JetBrains Mono"),
        Shadcn("mira", ShadcnStyle.Mira, "Inter"),
        Shadcn("luma", ShadcnStyle.Luma, "Inter"),
        Shadcn("rhea", ShadcnStyle.Rhea, "Inter"),
        new("sera", "Sera", "Sera / Taupe / Noto Sans + Playfair Display", new ShowcaseDesign
        {
            Style = ShadcnStyle.Sera,
            BaseColor = ShadcnBaseColor.Taupe,
            Theme = ShadcnColor.Base,
            ChartColor = ShadcnColor.Base,
            Font = "noto-sans",
            HeadingFont = "playfair-display",
        }),
    ];

    private static ShowcasePreset Shadcn(string name, ShadcnStyle style, string fontTitle) => new(
        name,
        ShowcaseDesign.StyleTitle(style),
        $"{ShowcaseDesign.StyleTitle(style)} / Neutral / {fontTitle}",
        new ShowcaseDesign
        {
            Style = style,
            ChartColor = ShadcnColor.Base,
            Font = ShowcaseFonts.All.First(f => f.Title == fontTitle).Name,
        });
}
