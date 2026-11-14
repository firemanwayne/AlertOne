using AlertOne.Core.Interfaces;
using AlertOne.Maui.Services;
using AlertOne.Shared.Blazor.Auth;
using AlertOne.Shared.Blazor.Handlers;
using AlertOne.Shared.Blazor.States;

using Blazored.LocalStorage;

using Domains.AlertManagement;
using Domains.UserManagement;

using Microsoft.AspNetCore.Components.Authorization;

#if DEBUG
using Microsoft.Extensions.Logging;
#endif

using MudBlazor.Services;

namespace AlertOne.Maui;

/// <summary>
/// Entry point that builds and configures the MAUI app.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds the MAUI app: fonts, MudBlazor, the Blazor web view, shared services and the domain services.
    /// </summary>
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
            .AddMudServices()
            .AddAuthorizationCore()
            .AddMauiBlazorWebView(); // Blazor WebView 

        // Blazored LocalStorage
        builder
            .Services
            .AddBlazoredLocalStorage();

        // Singleton Services
        builder
            .Services
            .AddSingleton<AppState>() // Global state
            .AddSingleton<DeepLinkService>() // Deep link service
            .AddSingleton<NotificationState>()
            .AddSingleton<PushNotificationRegistrationService>()
            .AddSingleton<ITokenStorageService, MauiTokenStorageService>() // Platform-specific token storage
            .AddSingleton<IPushNotificationRegistrationService>(sp => sp.GetRequiredService<PushNotificationRegistrationService>()) // Push notification service
            .AddSingleton<IAppVersionService, MauiAppVersionService>(); // App version service

        // Scoped Services
        builder
            .Services
            .AddScoped<JwtAuthenticationStateProvider>()
            .AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

        // Transient Services
        builder
            .Services
            .AddTransient<JwtAuthHeaderHandler>();

        // Domain services
        builder
            .Services
            .AddUserManagement(builder.Configuration)
            .AddAlertManagement(builder.Configuration);

#if DEBUG
        builder
            .Services
            .AddBlazorWebViewDeveloperTools();

        builder
            .Logging
            .AddDebug();
#endif

        return builder.Build();
    }
}
