#nullable enable

using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// What a visitor chose for the site: the look (a shadcn style, or MudBlazor's own) and everything
/// the Theme Generator offers. It is written the way it travels, as a query string
/// (<c>style=nova&amp;base=zinc&amp;theme=blue</c>): in a link to share, and in localStorage.
/// </summary>
public sealed record ShowcaseDesign
{
    /// <summary><c>false</c> for MudBlazor's own look (<c>MudShadcnProvider.Enabled</c>).</summary>
    public bool Shadcn { get; init; } = true;

    public ShadcnStyle Style { get; init; } = ShadcnStyle.NewYork;

    public ShadcnBaseColor BaseColor { get; init; } = ShadcnBaseColor.Neutral;

    public ShadcnColor Theme { get; init; } = ShadcnColor.Base;

    /// <summary><c>null</c>: the five colours of shadcn's original theme.</summary>
    public ShadcnColor? ChartColor { get; init; }

    public ShadcnRadius Radius { get; init; } = ShadcnRadius.Default;

    /// <summary>A font of <see cref="ShowcaseFonts"/>, by its shadcn name.</summary>
    public string Font { get; init; } = ShowcaseFonts.Default;

    /// <summary>A font of <see cref="ShowcaseFonts"/>, or <c>inherit</c> for <see cref="Font"/>.</summary>
    public string HeadingFont { get; init; } = ShowcaseFonts.Inherit;

    public ShadcnMenuAccent MenuAccent { get; init; } = ShadcnMenuAccent.Subtle;

    public ShadcnMenuColor MenuColor { get; init; } = ShadcnMenuColor.Default;

    public static string StyleTitle(ShadcnStyle style) => style == ShadcnStyle.NewYork ? "New York" : style.ToString();

    /// <summary>The name of the look, as the top bar shows it.</summary>
    public string LookName => Shadcn ? StyleTitle(Style) : "MudBlazor";

    public ShadcnThemeOptions ToOptions() => new()
    {
        Style = Style,
        BaseColor = BaseColor,
        Theme = Theme,
        ChartColor = ChartColor,
        Radius = Radius,
        FontFamily = ShowcaseFonts.Stack(Font),
        HeadingFontFamily = HeadingFont == ShowcaseFonts.Inherit || HeadingFont == Font ? null : ShowcaseFonts.Stack(HeadingFont),
        MenuAccent = MenuAccent,
        MenuColor = MenuColor,
    };

    /// <summary>The choices that differ from the defaults, as <c>key=value</c> pairs joined by <c>&amp;</c>.</summary>
    public string ToQuery()
    {
        var defaults = new ShowcaseDesign();
        var parts = new List<string>();
        void Add(string key, string value, bool isDefault)
        {
            if (!isDefault)
            {
                parts.Add($"{key}={value}");
            }
        }

        Add("look", "mudblazor", Shadcn);
        Add("style", Slug(Style), Style == defaults.Style);
        Add("base", Slug(BaseColor), BaseColor == defaults.BaseColor);
        Add("theme", Slug(Theme), Theme == defaults.Theme);
        Add("chart", ChartColor is { } chart ? Slug(chart) : "", ChartColor is null);
        Add("radius", Slug(Radius), Radius == defaults.Radius);
        Add("font", Font, Font == defaults.Font);
        Add("heading", HeadingFont, HeadingFont == defaults.HeadingFont);
        Add("accent", Slug(MenuAccent), MenuAccent == defaults.MenuAccent);
        Add("menu", Slug(MenuColor), MenuColor == defaults.MenuColor);
        return string.Join('&', parts);
    }

    /// <summary>
    /// Reads <see cref="ToQuery"/>'s format (a leading <c>?</c> is fine) on top of
    /// <paramref name="start"/>. Unknown keys and values are skipped, so a page's other parameters
    /// and a link from an older version do no harm.
    /// </summary>
    public static ShowcaseDesign Parse(string? query, ShowcaseDesign? start = null)
    {
        var design = start ?? new ShowcaseDesign();
        foreach (var pair in (query ?? "").TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var equals = pair.IndexOf('=');
            if (equals <= 0)
            {
                continue;
            }

            var value = Uri.UnescapeDataString(pair[(equals + 1)..]).Trim().ToLowerInvariant();
            design = pair[..equals].ToLowerInvariant() switch
            {
                "look" => design with { Shadcn = value != "mudblazor" },
                "style" when Value<ShadcnStyle>(value) is { } style => design with { Style = style, Shadcn = true },
                "base" when Value<ShadcnBaseColor>(value) is { } color => design with { BaseColor = color },
                "theme" when Value<ShadcnColor>(value) is { } color => design with { Theme = color },
                "chart" when value == "classic" => design with { ChartColor = null },
                "chart" when Value<ShadcnColor>(value) is { } color => design with { ChartColor = color },
                "radius" when Value<ShadcnRadius>(value) is { } radius => design with { Radius = radius },
                "font" when ShowcaseFonts.Find(value) is not null => design with { Font = value },
                "heading" when value == ShowcaseFonts.Inherit || ShowcaseFonts.Find(value) is not null => design with { HeadingFont = value },
                "accent" when Value<ShadcnMenuAccent>(value) is { } accent => design with { MenuAccent = accent },
                "menu" when Value<ShadcnMenuColor>(value) is { } menu => design with { MenuColor = menu },
                _ => design,
            };
        }

        return design;
    }

    // "new-york" and "default-translucent" are shadcn's spellings of NewYork and DefaultTranslucent.
    private static T? Value<T>(string text) where T : struct, Enum =>
        Enum.TryParse<T>(text.Replace("-", ""), ignoreCase: true, out var value) && Enum.IsDefined(value) ? value : null;

    private static string Slug<T>(T value) where T : struct, Enum =>
        string.Concat(value.ToString().Select((c, i) => char.IsUpper(c) && i > 0 ? "-" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}

/// <summary>
/// The site's theme: the visitor's <see cref="ShowcaseDesign"/> as the theme
/// <c>MudShadcnProvider</c> gets. One instance per app, so the layouts and the Theme Generator see
/// the same choice. A design in the page's address applies for the visit and is not saved; one set
/// from the site is.
/// </summary>
public sealed class ShowcaseTheme(ThemePreference preference, NavigationManager navigation)
{
    private Task? _loading;

    public ShowcaseDesign Design { get; private set; } = new();

    public ShadcnTheme Theme { get; private set; } = MudShadcnTheme.Create(new ShowcaseDesign().ToOptions());

    /// <summary><c>mode=dark</c> or <c>mode=light</c> in the address: the mode for this visit.</summary>
    public bool? ModeFromAddress { get; private set; }

    /// <summary>
    /// Whether the site runs right-to-left (<c>MudRTLProvider.RightToLeft</c>): the top bar's
    /// switch, or <c>dir=rtl</c> / <c>dir=ltr</c> in the address for this visit.
    /// </summary>
    public bool RightToLeft { get; private set; }

    /// <summary>Raised after <see cref="Design"/>, <see cref="Theme"/> or <see cref="RightToLeft"/> changed.</summary>
    public event Action? Changed;

    /// <summary>Reads the saved design, then the address. Safe to call from every layout.</summary>
    public Task LoadAsync() => _loading ??= LoadCoreAsync();

    private async Task LoadCoreAsync()
    {
        var saved = ShowcaseDesign.Parse(await preference.GetDesignAsync());
        var query = new Uri(navigation.Uri).Query;
        Apply(ShowcaseDesign.Parse(query, saved));
        RightToLeft = await preference.GetRightToLeftAsync();

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            ModeFromAddress = pair.ToLowerInvariant() switch
            {
                "mode=dark" => true,
                "mode=light" => false,
                _ => ModeFromAddress,
            };
            RightToLeft = pair.ToLowerInvariant() switch
            {
                "dir=rtl" => true,
                "dir=ltr" => false,
                _ => RightToLeft,
            };
        }
    }

    /// <summary>Switches the site's direction and remembers it.</summary>
    public async Task SetRightToLeftAsync(bool rightToLeft)
    {
        if (rightToLeft == RightToLeft)
        {
            return;
        }

        RightToLeft = rightToLeft;
        Changed?.Invoke();
        await preference.SetRightToLeftAsync(rightToLeft);
    }

    /// <summary>Applies <paramref name="design"/> to the site and remembers it.</summary>
    public async Task SetAsync(ShowcaseDesign design)
    {
        if (design == Design)
        {
            return;
        }

        Apply(design);
        Changed?.Invoke();
        await preference.SetDesignAsync(design.ToQuery());
    }

    private void Apply(ShowcaseDesign design)
    {
        Design = design;
        Theme = MudShadcnTheme.Create(design.ToOptions());
    }
}
