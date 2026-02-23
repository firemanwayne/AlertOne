using AlertOne.Core.Interfaces;
using AlertOne.Shared.Auth;
using AlertOne.Shared.Handlers;
using AlertOne.Shared.States;
using AlertOne.Web;
using AlertOne.Web.Services;
using Blazored.LocalStorage;
using Domains.AlertManagement;
using Domains.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.AddServiceDefaults();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// MudBlazor
builder.Services.AddMudServices();

// Global state
builder.Services.AddSingleton<AppState>();

// HttpClient with JWT handler
builder.Services.AddTransient<JwtAuthHeaderHandler>();

// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Browser-specific token storage
builder.Services.AddScoped<ITokenStorageService, BrowserTokenStorageService>();

// Push notifications (stub for WASM - not supported in browser)
builder.Services.AddSingleton<IPushNotificationRegistrationService, WebPushNotificationService>();

// Blazor authentication — require auth on every route by default
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped<JwtAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder
    .Services
    .AddUserManagement()
    .AddAlertManagement();

await builder
    .Build()
    .RunAsync();
