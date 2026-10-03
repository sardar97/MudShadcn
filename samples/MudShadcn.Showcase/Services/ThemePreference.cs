#nullable enable

using Microsoft.JSInterop;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// Remembers the light/dark choice in localStorage, so it survives reloads and reaches the
/// examples that mudblazor.com renders in an iframe (they run as a separate app instance).
/// </summary>
public sealed class ThemePreference(IJSRuntime js)
{
    private const string Key = "mudshadcn-showcase-theme";

    /// <summary><c>true</c> for dark, <c>false</c> for light, <c>null</c> to follow the OS.</summary>
    public async Task<bool?> GetAsync()
    {
        try
        {
            return await js.InvokeAsync<string?>("localStorage.getItem", Key) switch
            {
                "dark" => true,
                "light" => false,
                _ => null,
            };
        }
        catch (JSException)
        {
            return null; // storage blocked
        }
    }

    public async Task SetAsync(bool dark)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", Key, dark ? "dark" : "light");
        }
        catch (JSException)
        {
            // storage blocked: the choice lasts for this session only
        }
    }
}
