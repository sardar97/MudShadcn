namespace MudShadcn;

/// <summary>
/// What <see href="https://ui.shadcn.com/create">shadcn/create</see> lets you choose, for
/// <see cref="MudShadcnTheme.Create(ShadcnThemeOptions)"/>. Every property has shadcn's name and
/// values; the defaults give MudShadcn's original look.
/// </summary>
/// <example>
/// <code>
/// var theme = MudShadcnTheme.Create(new ShadcnThemeOptions
/// {
///     Style = ShadcnStyle.Nova,
///     BaseColor = ShadcnBaseColor.Zinc,
///     Theme = ShadcnColor.Blue,
///     Radius = ShadcnRadius.Small,
/// });
/// </code>
/// </example>
public sealed record ShadcnThemeOptions
{
    /// <summary>
    /// The shape of the components: sizes, spacing, corners, focus rings. Each style other than
    /// <see cref="ShadcnStyle.NewYork"/> is a small stylesheet of its own that
    /// <see cref="MudShadcnProvider"/> links when the theme is applied.
    /// </summary>
    public ShadcnStyle Style { get; init; } = ShadcnStyle.NewYork;

    /// <summary>The grey everything is drawn in: backgrounds, borders, muted text.</summary>
    public ShadcnBaseColor BaseColor { get; init; } = ShadcnBaseColor.Neutral;

    /// <summary>
    /// The colour of primary buttons, selected controls and the sidebar's active item.
    /// <see cref="ShadcnColor.Base"/> (the default) uses the base colour itself, as shadcn's default theme does.
    /// </summary>
    public ShadcnColor Theme { get; init; } = ShadcnColor.Base;

    /// <summary>
    /// The colour the five chart colours are shades of. <c>null</c> (the default) keeps the five
    /// different colours of shadcn's original theme; shadcn/create uses the same value as
    /// <see cref="Theme"/>.
    /// </summary>
    public ShadcnColor? ChartColor { get; init; }

    /// <summary>The corner radius every rounded corner is derived from.</summary>
    public ShadcnRadius Radius { get; init; } = ShadcnRadius.Default;

    /// <summary>
    /// The font stack. <c>null</c> uses <see cref="MudShadcnTheme.DefaultFontFamily"/>. A web font
    /// has to be loaded by the page, as usual.
    /// </summary>
    public string[]? FontFamily { get; init; }

    /// <summary>The font stack of headings (h1 to h6). <c>null</c> uses <see cref="FontFamily"/>.</summary>
    public string[]? HeadingFontFamily { get; init; }

    /// <summary>How strongly the item under the pointer stands out in menus, selects and lists of options.</summary>
    public ShadcnMenuAccent MenuAccent { get; init; } = ShadcnMenuAccent.Subtle;

    /// <summary>The surface of menus: the page's own colours, or the opposite mode's, solid or translucent.</summary>
    public ShadcnMenuColor MenuColor { get; init; } = ShadcnMenuColor.Default;
}

/// <summary>shadcn/ui's styles. The names are shadcn's.</summary>
public enum ShadcnStyle
{
    /// <summary>MudShadcn's original look, from shadcn's <c>new-york</c> style. Needs no extra stylesheet.</summary>
    NewYork,

    /// <summary>Clean, neutral and familiar: the classic shadcn/ui look.</summary>
    Vega,

    /// <summary>Reduced padding and margins, for compact layouts.</summary>
    Nova,

    /// <summary>Soft and rounded, with generous spacing.</summary>
    Maia,

    /// <summary>Boxy and sharp. Pairs well with mono fonts.</summary>
    Lyra,

    /// <summary>Compact, made for dense interfaces.</summary>
    Mira,

    /// <summary>Rounded geometry, soft elevation, breathable layouts.</summary>
    Luma,

    /// <summary>Editorial and typographic: square corners, underlined controls, uppercase headings.</summary>
    Sera,

    /// <summary>Like Luma, but compact.</summary>
    Rhea,
}

/// <summary>shadcn/ui's base colours: the greys of Tailwind's palette.</summary>
public enum ShadcnBaseColor
{
    Neutral,
    Stone,
    Zinc,
    Mauve,
    Olive,
    Mist,
    Taupe,
}

/// <summary>shadcn/ui's theme colours, for <see cref="ShadcnThemeOptions.Theme"/> and <see cref="ShadcnThemeOptions.ChartColor"/>.</summary>
public enum ShadcnColor
{
    /// <summary>The base colour itself (<see cref="ShadcnThemeOptions.BaseColor"/>).</summary>
    Base,
    Amber,
    Blue,
    Cyan,
    Emerald,
    Fuchsia,
    Green,
    Indigo,
    Lime,
    Orange,
    Pink,
    Purple,
    Red,
    Rose,
    Sky,
    Teal,
    Violet,
    Yellow,
}

/// <summary>shadcn/create's radius choices, as the value of <c>--radius</c>.</summary>
public enum ShadcnRadius
{
    /// <summary>0.625rem, shadcn's default.</summary>
    Default,

    /// <summary>Square corners.</summary>
    None,

    /// <summary>0.45rem.</summary>
    Small,

    /// <summary>0.625rem.</summary>
    Medium,

    /// <summary>0.875rem.</summary>
    Large,
}

/// <summary>shadcn/create's menu accent.</summary>
public enum ShadcnMenuAccent
{
    /// <summary>The item under the pointer gets the muted accent surface.</summary>
    Subtle,

    /// <summary>The item under the pointer gets the primary colour.</summary>
    Bold,
}

/// <summary>shadcn/create's menu colour.</summary>
public enum ShadcnMenuColor
{
    /// <summary>Menus have the page's colours.</summary>
    Default,

    /// <summary>Menus are dark on a light page.</summary>
    Inverted,

    /// <summary>Menus have the page's colours and let the page show through, blurred.</summary>
    DefaultTranslucent,

    /// <summary>Menus are dark on a light page and let the page show through, blurred.</summary>
    InvertedTranslucent,
}
