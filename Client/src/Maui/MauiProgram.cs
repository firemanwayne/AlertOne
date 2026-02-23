using AlertOne.Core.Interfaces;
using AlertOne.Maui.Services;
using AlertOne.Shared.Auth;
using AlertOne.Shared.Handlers;
using AlertOne.Shared.States;

using Blazored.LocalStorage;

using Domains.AlertManagement;
using Domains.UserManagement;

using Microsoft.AspNetCore.Components.Authorization;

#if DEBUG
using Microsoft.Extensions.Logging;
#endif

using MudBlazor.Services;

namespace AlertOne.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // MudBlazor
        builder
            .Services
            .AddMudServices();

        // Blazored LocalStorage
        builder
            .Services
            .AddBlazoredLocalStorage();

        // Global state
        builder
            .Services
            .AddSingleton<AppState>();

        // Deep link service
        builder
            .Services
            .AddSingleton<DeepLinkService>();

        // Push notification service
        builder
            .Services
            .AddSingleton<PushNotificationRegistrationService>();

        builder
            .Services
            .AddSingleton<IPushNotificationRegistrationService>(sp => sp.GetRequiredService<PushNotificationRegistrationService>());

        // Blazor WebView
        builder
            .Services
            .AddMauiBlazorWebView();

        // Authorization services for AuthorizeView components
        builder
            .Services
            .AddAuthorizationCore();

#if DEBUG
        builder
            .Services
            .AddBlazorWebViewDeveloperTools();

        builder
            .Logging
            .AddDebug();
#endif

        // HttpClient with JWT handler
        builder
            .Services
            .AddTransient<JwtAuthHeaderHandler>();

        // Platform-specific token storage
        builder
            .Services
            .AddSingleton<ITokenStorageService, MauiTokenStorageService>();

        // Authentication state provider
        builder
            .Services
            .AddScoped<JwtAuthenticationStateProvider>();

        builder
            .Services
            .AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

        builder
            .Services
            .AddUserManagement()
            .AddAlertManagement();

        return builder.Build();
    }
}
