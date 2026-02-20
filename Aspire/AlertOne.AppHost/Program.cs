var builder = DistributedApplication.CreateBuilder(args);

// Azure Functions isolated-worker API running as plain .NET (Kestrel via AspNetCore integration)
// Pin to port 7071 so the Blazor WASM wwwroot/appsettings.json and MAUI fallback stay correct
var userApi = builder
    .AddAzureFunctionsProject<Projects.UserManagement_Client>("api")
    .WithEndpoint("http", e => e.Port = 7071);

// Alert management API on port 7072
var alertsApi = builder
    .AddAzureFunctionsProject<Projects.AlertManagement_Client>("alerts-api")
    .WithEndpoint("http", e => e.Port = 7072);

// Blazor WebAssembly dev server
// API URL is read from wwwroot/appsettings.json (browser can't read process env vars)

var blazor = builder
    .AddStandaloneBlazorWebAssemblyProject<Projects.AlertOne_Web>("blazor")
    .WithReference(userApi)
    .WithReference(alertsApi)
    .WaitFor(userApi)
    .WaitFor(alertsApi);

userApi.WithReference(blazor);

alertsApi.WithReference(blazor);

// MAUI Windows app
// Aspire injects services__api__http__0 as an env var; MauiApp.CreateBuilder reads it via IConfiguration
// Register your MAUI app

// Add Windows device (uses localhost directly)
// Windows
//mauiapp.AddWindowsDevice()
//    .WithReference(api);

//// macOS Catalyst
//mauiapp.AddMacCatalystDevice()
//    .WithReference(api);

//// iOS Simulator
//mauiapp.AddiOSSimulator()
//     .WithReference(api);

//// iOS Device
//mauiapp.AddiOSDevice()
//     .WithReference(api);

//// Android Device
//mauiapp.AddAndroidDevice()
//        .WithReference(api);

//// Android Emulator
//mauiapp.AddAndroidEmulator()
//    .WithReference(api);

builder
    .Build()
    .Run();
