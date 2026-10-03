#nullable enable

using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MudShadcn.Showcase.Services;

/// <summary>One change group of a release (<c>### Added</c>, <c>### Fixed</c>, …).</summary>
public sealed record ChangelogGroup(string Heading, IReadOnlyList<string> Items);

/// <summary>One <c>## [version] — date</c> section of CHANGELOG.md.</summary>
public sealed record ChangelogRelease(string Version, string? Date, IReadOnlyList<string> Paragraphs, IReadOnlyList<ChangelogGroup> Groups)
{
    public bool IsUnreleased => Version.Equals("Unreleased", StringComparison.OrdinalIgnoreCase);

    public bool IsPreRelease => Version.Contains('-');

    public string Title => IsUnreleased ? "Unreleased" : $"v{Version}";
}

/// <summary>
/// Reads the repository's CHANGELOG.md, embedded at build time, for the Releases page. It understands
/// what Keep a Changelog uses and nothing more: <c>##</c> release headings, <c>###</c> groups,
/// <c>-</c> items (with indented continuation lines), paragraphs, and inline <c>`code`</c>,
/// <c>**bold**</c>, <c>[links](url)</c> and <c>&lt;url&gt;</c>. Items and paragraphs come out as HTML.
/// </summary>
public static partial class Changelog
{
    public const string FileUrl = "https://github.com/sardar97/MudShadcn/blob/master/CHANGELOG.md";
    public const string ReleasesUrl = "https://github.com/sardar97/MudShadcn/releases";

    public static IReadOnlyList<ChangelogRelease> Releases { get; } = Parse(Read());

    private static string Read()
    {
        using var stream = typeof(Changelog).Assembly.GetManifestResourceStream("CHANGELOG.md");
        return stream is null ? "" : new StreamReader(stream).ReadToEnd();
    }

    internal static IReadOnlyList<ChangelogRelease> Parse(string markdown)
    {
        var releases = new List<ChangelogRelease>();
        string? version = null, date = null, heading = null;
        List<string> paragraphs = [], items = [];
        List<ChangelogGroup> groups = [];
        string? block = null;           // the item or paragraph being collected
        var blockIsItem = false;

        void EndBlock()
        {
            if (block is not null)
            {
                (blockIsItem ? items : paragraphs).Add(Inline(block));
            }
            block = null;
        }

        void EndGroup()
        {
            EndBlock();
            if (heading is not null)
            {
                groups.Add(new ChangelogGroup(heading, items));
            }
            heading = null;
            items = [];
        }

        void EndRelease()
        {
            EndGroup();
            if (version is not null)
            {
                releases.Add(new ChangelogRelease(version, date, paragraphs, groups));
            }
            version = null;
            date = null;
            paragraphs = [];
            groups = [];
        }

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (ReleaseHeading().Match(line) is { Success: true } release)
            {
                EndRelease();
                version = release.Groups["version"].Value;
                date = release.Groups["date"].Success ? release.Groups["date"].Value : null;
            }
            else if (version is null || LinkReference().IsMatch(line))
            {
                // The file's title and introduction, and the link references at the bottom.
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                EndGroup();
                heading = line[4..].Trim();
            }
            else if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                EndBlock();
                block = line[2..];
                blockIsItem = true;
            }
            else if (line.Length == 0)
            {
                EndBlock();
            }
            else if (block is not null)
            {
                block += " " + line.Trim();
            }
            else
            {
                block = line.Trim();
                // A paragraph inside a group (rare) is shown as one of its items.
                blockIsItem = heading is not null;
            }
        }
        EndRelease();
        return releases;
    }

    /// <summary>Markdown inline syntax to HTML, with everything else escaped.</summary>
    private static string Inline(string text)
    {
        var html = WebUtility.HtmlEncode(text);
        html = InlineCode().Replace(html, "<code class=\"docs-code\">$1</code>");
        html = Bold().Replace(html, "<strong>$1</strong>");
        html = Link().Replace(html, "<a href=\"$2\" target=\"_blank\" rel=\"noopener\">$1</a>");
        html = AutoLink().Replace(html, "<a href=\"$1\" target=\"_blank\" rel=\"noopener\">$1</a>");
        return html;
    }

    // "## [1.0.0] — 2026-10-03", "## [1.0.0] - 2026-10-03", "## [Unreleased]"
    [GeneratedRegex(@"^## \[(?<version>[^\]]+)\](?:\s*[—–-]\s*(?<date>.+))?$")]
    private static partial Regex ReleaseHeading();

    [GeneratedRegex(@"^\[[^\]]+\]:\s")]
    private static partial Regex LinkReference();

    [GeneratedRegex("`([^`]+)`")]
    private static partial Regex InlineCode();

    [GeneratedRegex(@"\*\*(.+?)\*\*")]
    private static partial Regex Bold();

    [GeneratedRegex(@"\[([^\]]+)\]\((https?://[^)\s]+)\)")]
    private static partial Regex Link();

    [GeneratedRegex(@"&lt;(https?://[^&\s]+)&gt;")]
    private static partial Regex AutoLink();
}

/// <summary>The versions the site was built with, for the footer and the Releases page.</summary>
public static class BuildInfo
{
    /// <summary>MudShadcn's package version (its &lt;Version&gt;, without the commit suffix).</summary>
    public static string MudShadcnVersion { get; } = Version(typeof(MudShadcnTheme).Assembly);

    /// <summary>The MudBlazor version MudShadcn references.</summary>
    public static string MudBlazorVersion { get; } = typeof(MudBlazor.MudButton).Assembly.GetName().Version?.ToString(3) ?? "";

    private static string Version(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString(3)
            ?? "";
        var plus = version.IndexOf('+');
        return plus < 0 ? version : version[..plus];
    }
}
