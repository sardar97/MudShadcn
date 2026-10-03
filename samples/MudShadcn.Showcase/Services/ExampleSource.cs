#nullable enable

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using ColorCode;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// The source of each example, embedded at build time (see the csproj), highlighted the way
/// MudBlazor.Docs.Compiler does it for mudblazor.com: markup with ColorCode's HTML grammar, the
/// <c>@code</c> block with its C# grammar. Done at runtime and cached, so the code shown under an
/// example is always the code that ran.
/// </summary>
public static partial class ExampleSource
{
    private const string AtSign = "<span class=\"atSign\">&#64;</span>";
    private static readonly ConcurrentDictionary<string, string?> HtmlCache = new();
    private static readonly ConcurrentDictionary<string, string?> RawCache = new();

    /// <summary>The example's source as a reader would copy it, without <c>@namespace</c> lines.</summary>
    public static string? GetRaw(string name) => RawCache.GetOrAdd(name, static n =>
    {
        using var stream = typeof(ExampleSource).Assembly.GetManifestResourceStream($"Examples/{n}.razor");
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var source = reader.ReadToEnd().Replace("\r\n", "\n").Replace("\t", "    ");
        return NamespaceLayoutOrPageRegex().Replace(source, string.Empty).Trim();
    });

    /// <summary>Highlighted HTML for the example, or <c>null</c> if there is no such example.</summary>
    public static string? GetHtml(string name) => HtmlCache.GetOrAdd(name, static n =>
    {
        var source = GetRaw(n);
        if (source is null)
        {
            return null;
        }

        var formatter = new HtmlClassFormatter();
        var blocks = source.Split("@code");

        // ColorCode trips over '@', so it is swapped out for a placeholder and restored afterwards.
        var markup = formatter.GetHtmlString(blocks[0].Replace("@", "PlaceholdeR").Trim(), Languages.Html)
            .Replace("PlaceholdeR", "@");
        markup = AttributeValueRegex().Replace(markup, m =>
            $"<span class=\"quot\">&quot;</span>{AttributeValue(m.Groups["value"].Value)}<span class=\"quot\">&quot;</span>");

        var html = "<div class=\"mud-codeblock\">" + markup.Replace("@", AtSign);
        if (blocks.Length > 1)
        {
            html += formatter.GetHtmlString("@code" + string.Join("@code", blocks[1..]), Languages.CSharp).Replace("@", AtSign);
        }

        return html + "</div>";
    });

    private static string AttributeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        if (value is "true" or "false")
        {
            return $"<span class=\"keyword\">{value}</span>";
        }

        if (EnumValueRegex().IsMatch(value))
        {
            var tokens = value.Split('.');
            return $"<span class=\"enum\">{tokens[0]}</span><span class=\"enumValue\">.{tokens[1]}</span>";
        }

        if (VariableRegex().IsMatch(value))
        {
            return $"<span class=\"sharpVariable\">{value}</span>";
        }

        return $"<span class=\"htmlAttributeValue\">{value}</span>";
    }

    [GeneratedRegex("@(namespace|layout|page) .+?\n")]
    private static partial Regex NamespaceLayoutOrPageRegex();

    [GeneratedRegex(@"<span class=""htmlAttributeValue"">&quot;(?'value'.*?)&quot;</span>")]
    private static partial Regex AttributeValueRegex();

    [GeneratedRegex("^[A-Z][A-Za-z0-9]+[.][A-Za-z][A-Za-z0-9]+$")]
    private static partial Regex EnumValueRegex();

    [GeneratedRegex("^@[A-Za-z0-9]+$")]
    private static partial Regex VariableRegex();
}
