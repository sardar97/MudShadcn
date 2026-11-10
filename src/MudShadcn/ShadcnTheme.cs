using MudBlazor;

namespace MudShadcn;

/// <summary>
/// A <see cref="MudTheme"/> made by <see cref="MudShadcnTheme"/>. It is an ordinary MudBlazor theme
/// (change its palette, typography or layout properties as usual) that also carries what a
/// <see cref="MudTheme"/> has no place for: shadcn's tokens, the corner radius and the style.
/// <see cref="MudShadcnProvider"/> reads those when the theme is passed to it.
/// </summary>
public sealed class ShadcnTheme : MudTheme
{
    internal ShadcnTheme(ShadcnThemeOptions options, string radius)
    {
        Options = options;
        Style = options.Style;
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

    /// <summary>shadcn's <c>--radius</c>, emitted as <c>--shadcn-radius</c>. The radius scale is derived from it.</summary>
    public string Radius { get; set; }

    /// <summary>
    /// The light mode's shadcn tokens in OKLCH, by name without the <c>--shadcn-</c> prefix
    /// (<c>ring</c>, <c>sidebar-accent</c>, <c>chart-1</c>, …). Change a value to change the token.
    /// </summary>
    public IDictionary<string, string> TokensLight { get; }

    /// <summary>The dark mode's shadcn tokens. See <see cref="TokensLight"/>.</summary>
    public IDictionary<string, string> TokensDark { get; }

    internal string ToCss(bool dark) => ShadcnTokens.ToCss(dark ? TokensDark : TokensLight, dark, Radius);
}
