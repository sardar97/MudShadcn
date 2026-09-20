using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace MudShadcn;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers MudBlazor's services (this calls <c>AddMudServices</c> for you) with
    /// shadcn-flavoured defaults: sonner-style snackbars, bottom-right, without the
    /// coloured Material backgrounds.
    /// </summary>
    public static IServiceCollection AddMudShadcn(this IServiceCollection services, Action<MudServicesConfiguration>? configure = null)
    {
        return services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
            config.SnackbarConfiguration.HideTransitionDuration = 150;
            config.SnackbarConfiguration.ShowTransitionDuration = 150;

            configure?.Invoke(config);
        });
    }
}
