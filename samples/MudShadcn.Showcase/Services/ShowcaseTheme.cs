#nullable enable

using MudBlazor;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// MudShadcn's default theme with Geist, the font ui.shadcn.com uses (loaded in index.html). This is
/// the customisation the README describes; everything else is MudShadcnTheme's defaults.
/// </summary>
public static class ShowcaseTheme
{
    public static MudTheme Theme { get; } = MudShadcnTheme.Create(["Geist", .. MudShadcnTheme.DefaultFontFamily]);
}
