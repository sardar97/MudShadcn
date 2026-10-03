using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudShadcn;
using MudShadcn.Showcase;
using MudShadcn.Showcase.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The examples fetch sample data from mudblazor.com's web API. ExamplesApiHandler answers those
// requests in the browser, so the showcase stays a static site that any static host can serve.
builder.Services.AddScoped(_ => new HttpClient(new ExamplesApiHandler { InnerHandler = new HttpClientHandler() })
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
});

// MudBlazor's services with MudShadcn's defaults. MudBlazor itself comes in through MudShadcn.
builder.Services.AddMudShadcn();
builder.Services.AddSingleton<MenuService>();
builder.Services.AddScoped<ThemePreference>();

await builder.Build().RunAsync();
