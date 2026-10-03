#nullable enable

namespace MudShadcn.Showcase.Docs;

/// <summary>Links to MudBlazor's API reference, which this site does not duplicate.</summary>
public static class ApiLink
{
    public static string GetApiLinkFor(Type type) =>
        $"https://mudblazor.com/api/{type.Name.Replace("`1", "").Replace("`2", "").ToLowerInvariant()}";
}
