using Blazored.LocalStorage;
using Domains.AlertManagement;
using Domains.AlertManagement.Services;
using Domains.UserManagement;
using Domains.UserManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagementApps.Core.Interfaces;
using UserManagementApps.Shared.Handlers;
using UserManagementApps.Shared.States;
using UserManagementApps.Web;
using UserManagementApps.Web.Auth;
using UserManagementApps.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");

builder.RootComponents.Add<HeadOutlet>("head::after");

builder.AddServiceDefaults();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Global state
builder.Services.AddSingleton<AppState>();

// HttpClient with JWT handler
builder.Services.AddTransient<JwtAuthHeaderHandler>();

// Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Browser-specific token storage
builder.Services.AddScoped<ITokenStorageService, BrowserTokenStorageService>();

// Blazor authentication — require auth on every route by default
builder.Services.AddAuthorizationCore(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder
    .Services
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

await builder
    .Build()
    .RunAsync();
