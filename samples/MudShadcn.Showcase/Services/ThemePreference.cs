#nullable enable

using Microsoft.JSInterop;

namespace MudShadcn.Showcase.Services;

/// <summary>
/// Remembers the light/dark choice and the design (the look and the Theme Generator's choices) in
/// localStorage, so they survive reloads and reach the examples that mudblazor.com renders in an
/// iframe (they run as a separate app instance).
/// </summary>
public sealed class ThemePreference(IJSRuntime js)
{
    private const string Key = "mudshadcn-showcase-theme";
    private const string LookKey = "mudshadcn-showcase-look";
    private const string DesignKey = "mudshadcn-showcase-design";

    /// <summary>The saved <see cref="ShowcaseDesign"/> in its query-string form, or <c>null</c>.</summary>
    public async Task<string?> GetDesignAsync()
    {
        try
        {
            var design = await js.InvokeAsync<string?>("localStorage.getItem", DesignKey);
            if (design is not null)
            {
                return design;
            }

            // Before the styles there was one choice, the look, under a key of its own.
            return await js.InvokeAsync<string?>("localStorage.getItem", LookKey) == "mudblazor" ? "look=mudblazor" : null;
        }
        catch (JSException)
        {
            return null; // storage blocked
        }
    }

    public async Task SetDesignAsync(string design)
    {
        try
        {
            await js.InvokeVoidAsync("localStorage.setItem", DesignKey, design);
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
