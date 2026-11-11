#nullable enable

using System.Text;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// The code the Theme Generator shows for a <see cref="ShowcaseDesign"/>: what a developer pastes
/// into an app to get the same look. It is written against the real API
/// (<see cref="ShadcnThemeOptions"/>), and only names the options that differ from its defaults.
/// </summary>
public static class ThemeCode
{
    /// <summary>The <c>&lt;link&gt;</c> that loads the fonts in use, or <c>null</c> when none is needed.</summary>
    public static string? FontLink(ShowcaseDesign design) =>
        ShowcaseFonts.Href(skipLoaded: false, design.Font, design.HeadingFont) is { } href
            ? $"<link rel=\"preconnect\" href=\"https://fonts.googleapis.com\" />\n<link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin />\n<link rel=\"stylesheet\" href=\"{href}\" />"
            : null;

    /// <summary>The provider line and the theme it gets.</summary>
    public static string Provider(ShowcaseDesign design)
    {
        var defaults = new ShadcnThemeOptions();
        var options = design.ToOptions();
        var lines = new List<string>();

        void Add(string name, object value, object @default)
        {
            if (!Equals(value, @default))
            {
                lines.Add($"    {name} = {value.GetType().Name}.{value},");
            }
        }

        Add(nameof(ShadcnThemeOptions.Style), options.Style, defaults.Style);
        Add(nameof(ShadcnThemeOptions.BaseColor), options.BaseColor, defaults.BaseColor);
        Add(nameof(ShadcnThemeOptions.Theme), options.Theme, defaults.Theme);
        if (options.ChartColor is { } chart)
        {
            lines.Add($"    {nameof(ShadcnThemeOptions.ChartColor)} = ShadcnColor.{chart},");
        }

        Add(nameof(ShadcnThemeOptions.Radius), options.Radius, defaults.Radius);

        // The package's own font is the system stack, so the chosen font is always spelled out.
        lines.Add($"    {nameof(ShadcnThemeOptions.FontFamily)} = {Stack(design.Font)},");
        if (options.HeadingFontFamily is not null)
        {
            lines.Add($"    {nameof(ShadcnThemeOptions.HeadingFontFamily)} = {Stack(design.HeadingFont)},");
        }

        Add(nameof(ShadcnThemeOptions.MenuAccent), options.MenuAccent, defaults.MenuAccent);
        Add(nameof(ShadcnThemeOptions.MenuColor), options.MenuColor, defaults.MenuColor);

        var code = new StringBuilder();
        code.AppendLine("<MudShadcnProvider Theme=\"_theme\" />");
        code.AppendLine();
        code.AppendLine("@code {");
        code.AppendLine("    private readonly ShadcnTheme _theme = MudShadcnTheme.Create(new ShadcnThemeOptions");
        code.AppendLine("    {");
        foreach (var line in lines)
        {
            code.AppendLine("    " + line);
        }

        code.AppendLine("    });");
        code.Append('}');
        return code.ToString();
    }

    // ["Inter", .. MudShadcnTheme.DefaultFontFamily] for a sans font, the whole stack for the others.
    private static string Stack(string font)
    {
        var stack = ShowcaseFonts.Stack(font);
        return ShowcaseFonts.Find(font)?.Kind is "mono" or "serif"
            ? "[" + string.Join(", ", stack.Select(name => $"\"{name}\"")) + "]"
            : $"[\"{stack[0]}\", .. MudShadcnTheme.DefaultFontFamily]";
    }
}
