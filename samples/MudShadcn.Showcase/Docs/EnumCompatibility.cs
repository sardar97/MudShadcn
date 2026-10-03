#nullable enable

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace MudShadcn.Showcase.Docs;

/// <summary>
/// MudBlazor generates <c>ToStringFast</c> for its enums, but internally; mudblazor.com's examples can
/// call it because the docs project sees MudBlazor's internals. This public equivalent keeps those
/// examples verbatim: with <c>useMetadataAttributes</c> it returns the <see cref="DescriptionAttribute"/>
/// value MudBlazor puts on its enum members (<c>Origin.TopLeft</c> → <c>top-left</c>).
/// </summary>
public static class EnumCompatibility
{
    private static readonly ConcurrentDictionary<Enum, string> Descriptions = new();

    public static string ToStringFast(this Enum value, bool useMetadataAttributes = false) =>
        !useMetadataAttributes
            ? value.ToString()
            : Descriptions.GetOrAdd(value, static v =>
                v.GetType().GetField(v.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? v.ToString());
}
