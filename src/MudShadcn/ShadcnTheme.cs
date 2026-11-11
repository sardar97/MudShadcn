using MudBlazor;

namespace MudShadcn;

/// <summary>
/// A <see cref="MudTheme"/> made by <see cref="MudShadcnTheme"/>. It is an ordinary MudBlazor theme
/// (change its palette, typography or layout properties as usual) that also carries what a
/// <see cref="MudTheme"/> has no place for: shadcn's tokens, the corner radius, the style and the
/// icon library.
/// <see cref="MudShadcnProvider"/> reads those when the theme is passed to it.
/// </summary>
public sealed class ShadcnTheme : MudTheme
{
    internal ShadcnTheme(ShadcnThemeOptions options, string radius)
    {
        Options = options;
        Style = options.Style;
        MenuAccent = options.MenuAccent;
        MenuColor = options.MenuColor;
        IconLibrary = options.IconLibrary;
        Radius = radius;
        TokensLight = ShadcnTokens.Build(options, dark: false);
        TokensDark = ShadcnTokens.Build(options, dark: true);
    }

    /// <summary>The options the theme was created from.</summary>
    public ShadcnThemeOptions Options { get; }

    /// <summary>
    /// The style <see cref="MudShadcnProvider"/> links a stylesheet for.
    /// <see cref="ShadcnStyle.NewYork"/> needs none.
    /// </summary>
    public ShadcnStyle Style { get; set; }

    /// <summary>
    /// How the item under the pointer stands out in menus, selects and autocompletes. Bold is a
    /// stylesheet of its own (<c>styles/menu-bold.css</c>) that <see cref="MudShadcnProvider"/> links.
    /// </summary>
    public ShadcnMenuAccent MenuAccent { get; set; }

    /// <summary>
    /// The surface of menus, selects and autocompletes. Inverted and translucent are stylesheets of
    /// their own (<c>styles/menu-inverted.css</c>, <c>styles/menu-translucent.css</c>) that
    /// <see cref="MudShadcnProvider"/> links.
    /// </summary>
    public ShadcnMenuColor MenuColor { get; set; }

    /// <summary>
    /// The icon library MudBlazor's built-in icons are drawn with, or <c>null</c> for MudBlazor's
    /// own Material icons. Each library is a stylesheet of its own (<c>styles/icons-lucide.css</c>,
    /// …) that <see cref="MudShadcnProvider"/> links.
    /// </summary>
    public ShadcnIconLibrary? IconLibrary { get; set; }

    /// <summary>shadcn's <c>--radius</c>, emitted as <c>--shadcn-radius</c>. The radius scale is derived from it.</summary>
    public string Radius { get; set; }

    /// <summary>
    /// The light mode's shadcn tokens in OKLCH, by name without the <c>--shadcn-</c> prefix
    /// (<c>ring</c>, <c>sidebar-accent</c>, <c>chart-1</c>, …). Change a value to change the token.
    /// </summary>
    public IDictionary<string, string> TokensLight { get; }

    /// <summary>The dark mode's shadcn tokens. See <see cref="TokensLight"/>.</summary>
    public IDictionary<string, string> TokensDark { get; }

    internal bool MenuInverted => MenuColor is ShadcnMenuColor.Inverted or ShadcnMenuColor.InvertedTranslucent;

    internal bool MenuTranslucent => MenuColor is ShadcnMenuColor.DefaultTranslucent or ShadcnMenuColor.InvertedTranslucent;

    // An inverted menu is dark on a light page; on a dark page it is what it would be anyway.
    internal string ToCss(bool dark) =>
        ShadcnTokens.ToCss(dark ? TokensDark : TokensLight, dark, Radius, MenuInverted && !dark ? TokensDark : null);

    /// <summary>
    /// The stylesheets the theme needs besides MudShadcn.css, by name and in the order they have to
    /// load: the style, then the menu options, then the icon library.
    /// </summary>
    internal IEnumerable<string> StyleSheets(bool dark)
    {
        if (Style != ShadcnStyle.NewYork)
        {
            yield return Style.ToString().ToLowerInvariant();   // NewYork is MudShadcn.css itself
        }

        // A translucent menu has its own highlight (the foreground at 10%), as in shadcn.
        if (MenuAccent == ShadcnMenuAccent.Bold && !MenuTranslucent)
        {
            yield return "menu-bold";
        }

        if (MenuInverted && !dark)
        {
            yield return "menu-inverted";
        }

        if (MenuTranslucent)
        {
            yield return "menu-translucent";
        }

        if (IconLibrary is { } library)
        {
            yield return "icons-" + library.ToString().ToLowerInvariant();
        }
    }
}
