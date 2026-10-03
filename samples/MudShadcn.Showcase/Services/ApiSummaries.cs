#nullable enable

namespace MudShadcn.Showcase.Services;

/// <summary>
/// The <c>&lt;summary&gt;</c> of every documented MudBlazor type, which mudblazor.com shows under
/// each page title. The build extracts them from the MudBlazor.xml that ships in MudBlazor's
/// package (target <c>ExtractMudBlazorSummaries</c> in the csproj), so they always match the
/// MudBlazor version MudShadcn references.
/// </summary>
public static class ApiSummaries
{
    private static readonly Lazy<Dictionary<string, string>> Summaries = new(Load);

    /// <summary>Gets the summary for a type name such as <c>MudAlert</c> or <c>MudChip</c>.</summary>
    public static string? Get(string? typeName) =>
        typeName is not null && Summaries.Value.TryGetValue(typeName, out var summary) ? summary : null;

    private static Dictionary<string, string> Load()
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        using var stream = typeof(ApiSummaries).Assembly.GetManifestResourceStream("ApiSummaries.tsv");
        if (stream is null)
        {
            return result;
        }

        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            var tab = line.IndexOf('\t');
            if (tab > 0)
            {
                // First wins: MudBlazor.MudChip`1 is listed before any nested or same-named type.
                result.TryAdd(line[..tab], line[(tab + 1)..]);
            }
        }

        return result;
    }
}
