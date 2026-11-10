#nullable enable

namespace MudShadcn.Showcase.Services;

/// <summary>A font the Theme Generator offers: shadcn/create's list (apps/v4/lib/font-definitions.ts).</summary>
/// <param name="Name">shadcn's name for it, as in a design's address (<c>font=inter</c>).</param>
/// <param name="Title">The font's name, which is also its CSS family name.</param>
/// <param name="Kind"><c>sans</c>, <c>serif</c> or <c>mono</c>.</param>
/// <param name="GoogleFamily">
/// The <c>family=</c> value of the Google Fonts stylesheet that loads it, or <c>null</c> when
/// index.html already loads the font.
/// </param>
public sealed record ShowcaseFont(string Name, string Title, string Kind, string? GoogleFamily);

public static class ShowcaseFonts
{
    /// <summary>Geist, the font ui.shadcn.com uses. index.html loads it.</summary>
    public const string Default = "geist";

    /// <summary>A heading font of <c>inherit</c> is the body font.</summary>
    public const string Inherit = "inherit";

    public static IReadOnlyList<ShowcaseFont> All { get; } =
    [
        new("geist", "Geist", "sans", null),
        new("geist-mono", "Geist Mono", "mono", null),
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
}
