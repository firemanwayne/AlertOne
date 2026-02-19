
using AlertOne.Core.Interfaces;
using AlertOne.Shared.Handlers;
using AlertOne.Shared.States;
using Domains.AlertManagement;
using Domains.AlertManagement.Services;
using Domains.UserManagement;
using Domains.UserManagement.Services;
using Microsoft.Extensions.Logging;
using UserManagementApps.Maui.Services;

namespace UserManagementApps.Maui;

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

        // Global state
        builder.Services.AddSingleton<AppState>();

        // Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        // HttpClient with JWT handler
        builder.Services.AddTransient<JwtAuthHeaderHandler>();

        // Platform-specific token storage
        builder.Services.AddSingleton<ITokenStorageService, MauiTokenStorageService>();

        // Add service defaults
        //builder.AddServiceDefaults();

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://api") });

        builder.Services
            .AddUserManagement()
            .AddAlertManagement();

        builder
            .Services
            .AddHttpClient<UserManagementApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://api");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        builder
            .Services
            .AddHttpClient<AlertManagementApiClient>(client =>
            {
                client.BaseAddress = new Uri("http://alerts-api");
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddHttpMessageHandler<JwtAuthHeaderHandler>();

        return builder.Build();
    }
}
