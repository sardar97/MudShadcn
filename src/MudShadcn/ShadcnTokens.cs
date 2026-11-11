namespace MudShadcn;

/// <summary>
/// The shadcn/ui default ("neutral") theme tokens, taken verbatim from the shadcn/ui theming
/// docs (Default Theme CSS). <see cref="MudShadcnProvider"/> emits a theme's tokens as
/// <c>--shadcn-*</c> CSS custom properties so MudShadcn.css — and your own CSS — can use the real
/// OKLCH values, including the ones MudBlazor's palette has no slot for (ring, input, sidebar).
/// These are the tokens of the default theme; a theme made with other
/// <see cref="ShadcnThemeOptions"/> carries its own (<see cref="ShadcnTheme.TokensLight"/>,
/// <see cref="ShadcnTheme.TokensDark"/>).
/// </summary>
public static class ShadcnTokens
{
    /// <summary>shadcn's <c>--radius</c>. The radius scale is derived from it in MudShadcn.css.</summary>
    public const string Radius = "0.625rem";

    /// <summary>Tokens under shadcn's <c>:root</c>.</summary>
    public static IReadOnlyDictionary<string, string> Light { get; } = new Dictionary<string, string>
    {
        ["background"] = "oklch(1 0 0)",
        ["foreground"] = "oklch(0.145 0 0)",
        ["card"] = "oklch(1 0 0)",
        ["card-foreground"] = "oklch(0.145 0 0)",
        ["popover"] = "oklch(1 0 0)",
        ["popover-foreground"] = "oklch(0.145 0 0)",
        ["primary"] = "oklch(0.205 0 0)",
        ["primary-foreground"] = "oklch(0.985 0 0)",
        ["secondary"] = "oklch(0.97 0 0)",
        ["secondary-foreground"] = "oklch(0.205 0 0)",
        ["muted"] = "oklch(0.97 0 0)",
        ["muted-foreground"] = "oklch(0.556 0 0)",
        ["accent"] = "oklch(0.97 0 0)",
        ["accent-foreground"] = "oklch(0.205 0 0)",
        ["destructive"] = "oklch(0.577 0.245 27.325)",
        ["border"] = "oklch(0.922 0 0)",
        ["input"] = "oklch(0.922 0 0)",
        ["ring"] = "oklch(0.708 0 0)",
        ["chart-1"] = "oklch(0.646 0.222 41.116)",
        ["chart-2"] = "oklch(0.6 0.118 184.704)",
        ["chart-3"] = "oklch(0.398 0.07 227.392)",
        ["chart-4"] = "oklch(0.828 0.189 84.429)",
        ["chart-5"] = "oklch(0.769 0.188 70.08)",
        ["sidebar"] = "oklch(0.985 0 0)",
        ["sidebar-foreground"] = "oklch(0.145 0 0)",
        ["sidebar-primary"] = "oklch(0.205 0 0)",
        ["sidebar-primary-foreground"] = "oklch(0.985 0 0)",
        ["sidebar-accent"] = "oklch(0.97 0 0)",
        ["sidebar-accent-foreground"] = "oklch(0.205 0 0)",
        ["sidebar-border"] = "oklch(0.922 0 0)",
        ["sidebar-ring"] = "oklch(0.708 0 0)",
        // Not shadcn tokens: per-mode values shadcn expresses with `dark:` utility variants.
        ["input-background"] = "transparent",                       // bg-transparent
        ["destructive-background"] = "oklch(0.577 0.245 27.325)",   // bg-destructive
    };

    /// <summary>Tokens under shadcn's <c>.dark</c>.</summary>
    public static IReadOnlyDictionary<string, string> Dark { get; } = new Dictionary<string, string>
    {
        ["background"] = "oklch(0.145 0 0)",
        ["foreground"] = "oklch(0.985 0 0)",
        ["card"] = "oklch(0.205 0 0)",
        ["card-foreground"] = "oklch(0.985 0 0)",
        ["popover"] = "oklch(0.205 0 0)",
        ["popover-foreground"] = "oklch(0.985 0 0)",
        ["primary"] = "oklch(0.922 0 0)",
        ["primary-foreground"] = "oklch(0.205 0 0)",
        ["secondary"] = "oklch(0.269 0 0)",
        ["secondary-foreground"] = "oklch(0.985 0 0)",
        ["muted"] = "oklch(0.269 0 0)",
        ["muted-foreground"] = "oklch(0.708 0 0)",
        ["accent"] = "oklch(0.269 0 0)",
        ["accent-foreground"] = "oklch(0.985 0 0)",
        ["destructive"] = "oklch(0.704 0.191 22.216)",
        ["border"] = "oklch(1 0 0 / 10%)",
        ["input"] = "oklch(1 0 0 / 15%)",
        ["ring"] = "oklch(0.556 0 0)",
        ["chart-1"] = "oklch(0.488 0.243 264.376)",
        ["chart-2"] = "oklch(0.696 0.17 162.48)",
        ["chart-3"] = "oklch(0.769 0.188 70.08)",
        ["chart-4"] = "oklch(0.627 0.265 303.9)",
        ["chart-5"] = "oklch(0.645 0.246 16.439)",
        ["sidebar"] = "oklch(0.205 0 0)",
        ["sidebar-foreground"] = "oklch(0.985 0 0)",
        ["sidebar-primary"] = "oklch(0.488 0.243 264.376)",
        ["sidebar-primary-foreground"] = "oklch(0.985 0 0)",
        ["sidebar-accent"] = "oklch(0.269 0 0)",
        ["sidebar-accent-foreground"] = "oklch(0.985 0 0)",
        ["sidebar-border"] = "oklch(1 0 0 / 10%)",
        ["sidebar-ring"] = "oklch(0.556 0 0)",
        ["input-background"] = "oklch(1 0 0 / 4.5%)",                       // dark:bg-input/30 → 15% × 30%
        ["destructive-background"] = "oklch(0.704 0.191 22.216 / 60%)",     // dark:bg-destructive/60
    };

    /// <summary>
    /// The tokens for <paramref name="options"/>, put together the way shadcn/create does it: the base
    /// colour's set, then what the theme colour overrides, then the chart colours.
    /// </summary>
    internal static Dictionary<string, string> Build(ShadcnThemeOptions options, bool dark)
    {
        var tokens = ShadcnThemeData.Base(options.BaseColor, dark);
        if (options.Theme != ShadcnColor.Base)
        {
            foreach (var (name, value) in ShadcnThemeData.Accent(options.Theme, dark))
            {
                tokens[name] = value;
            }
        }

        // null: the five colours of shadcn's original theme, which the registry's themes no longer have.
        IReadOnlyDictionary<string, string> charts = options.ChartColor switch
        {
            null => dark ? Dark : Light,
            ShadcnColor.Base => ShadcnThemeData.Base(options.BaseColor, dark),
            { } color => ShadcnThemeData.Accent(color, dark),
        };
        for (var i = 1; i <= 5; i++)
        {
            tokens[$"chart-{i}"] = charts[$"chart-{i}"];
        }

        // Not shadcn tokens: per-mode values shadcn expresses with `dark:` utility variants.
        tokens["input-background"] = dark ? OklchColor.WithOpacity(tokens["input"], 0.3) : "transparent";   // dark:bg-input/30
        tokens["destructive-background"] = dark ? OklchColor.WithOpacity(tokens["destructive"], 0.6) : tokens["destructive"];   // dark:bg-destructive/60

        return tokens;
    }

    internal static string ToCss(bool dark) => ToCss(dark ? Dark : Light, dark, Radius);

    // inverted: the other mode's tokens, emitted as --shadcn-inverted-* for the parts of the page a
    // theme option draws in that mode (styles/menu-inverted.css), or null.
    internal static string ToCss(IEnumerable<KeyValuePair<string, string>> tokens, bool dark, string radius,
        IEnumerable<KeyValuePair<string, string>>? inverted = null)
    {
        // color-scheme as next-themes sets it for shadcn: native scrollbars and form controls, and
        // CSS light-dark(), follow the active mode.
        var sb = new System.Text.StringBuilder(":root{color-scheme:").Append(dark ? "dark" : "light").Append(';');
        if (radius != Radius)
        {
            sb.Append("--shadcn-radius:").Append(radius).Append(';');
        }
        foreach (var (name, value) in tokens)
        {
            sb.Append("--shadcn-").Append(name).Append(':').Append(value).Append(';');
        }
        foreach (var (name, value) in inverted ?? [])
        {
            sb.Append("--shadcn-inverted-").Append(name).Append(':').Append(value).Append(';');
        }
        return sb.Append('}').ToString();
    }
}
