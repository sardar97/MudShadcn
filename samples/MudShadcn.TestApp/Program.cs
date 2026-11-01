using System.Globalization;
using MudShadcn;
using MudShadcn.TestApp.Components;

var builder = WebApplication.CreateBuilder(args);

// The sample data is shadcn's, in US dollars. Without this, a host with no locale (a container,
// CI) formats the amounts with the invariant culture's "¤" sign.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Registers MudBlazor's services with shadcn-flavoured defaults. MudBlazor itself arrives
// transitively through the MudShadcn reference: this project has no MudBlazor PackageReference.
builder.Services.AddMudShadcn();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
