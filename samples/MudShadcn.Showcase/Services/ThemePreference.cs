#nullable enable

using Microsoft.JSInterop;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// Remembers the light/dark choice and the look (shadcn or MudBlazor's own) in localStorage, so
/// they survive reloads and reach the examples that mudblazor.com renders in an iframe (they run
/// as a separate app instance).
/// </summary>
public sealed class ThemePreference(IJSRuntime js)
{
    private const string Key = "mudshadcn-showcase-theme";
    private const string LookKey = "mudshadcn-showcase-look";

    /// <summary><c>false</c> if the visitor switched to MudBlazor's own look; otherwise <c>true</c>.</summary>
    public async Task<bool> GetShadcnAsync()
    {
        try
        {
            return await js.InvokeAsync<string?>("localStorage.getItem", LookKey) != "mudblazor";
        }
        catch (JSException)
        {
            return true; // storage blocked
        }
    }

    public async Task SetShadcnAsync(bool shadcn)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", LookKey, shadcn ? "shadcn" : "mudblazor");
        }
        catch (JSException)
        {
            // storage blocked: the choice lasts for this session only
        }
    }

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
