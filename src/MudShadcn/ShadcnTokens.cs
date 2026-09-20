namespace MudShadcn;

/// <summary>
/// The shadcn/ui default ("neutral") theme tokens, taken verbatim from the shadcn/ui theming
/// docs (Default Theme CSS). These are emitted as <c>--shadcn-*</c> CSS custom properties by
/// <see cref="MudShadcnProvider"/> so MudShadcn.css — and your own CSS — can use the real
/// OKLCH values, including the ones MudBlazor's palette has no slot for (ring, input, sidebar).
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

    internal static string ToCss(bool dark)
    {
        var sb = new System.Text.StringBuilder(":root{");
        foreach (var (name, value) in dark ? Dark : Light)
        {
            sb.Append("--shadcn-").Append(name).Append(':').Append(value).Append(';');
        }
        return sb.Append('}').ToString();
    }
}
