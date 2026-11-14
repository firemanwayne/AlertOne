var builder = DistributedApplication.CreateBuilder(args);

// Local Cosmos DB emulator (preview/vnext emulator serves plain HTTP, so no certificate trust is needed).
// The APIs create their databases and containers on startup (EnsureCreated).
var cosmos = builder
    .AddAzureCosmosDB("cosmos")
    .RunAsPreviewEmulator(emulator => emulator
        .WithLifetime(ContainerLifetime.Persistent)
        .WithDataExplorer());

// Well-known, publicly documented Cosmos emulator account key (not a secret)
const string CosmosEmulatorKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

// Azure Functions isolated-worker API running as plain .NET (Kestrel via AspNetCore integration)
// Pin to port 7071 so the Blazor WASM wwwroot/appsettings.json and MAUI fallback stay correct
var userApi = builder
    .AddAzureFunctionsProject<Projects.UserManagement_Client>("usrm-api")
    .WithEndpoint("http", e => e.Port = 7071)
    .WaitForStart(cosmos)
    .WithEnvironment("CosmosDb__Endpoint", cosmos.Resource.UriExpression)
    .WithEnvironment("CosmosDb__AccountKey", CosmosEmulatorKey)
    .WithEnvironment("CosmosDb__DatabaseName", "alertone-usrm")
    // Local-only JWT settings (previously supplied by Azure App Configuration)
    .WithEnvironment("JwtSettings__usrm__Secret", "local-dev-only-jwt-secret-key-change-me-0123456789")
    .WithEnvironment("JwtSettings__usrm__Issuer", "AlertOne.Local")
    .WithEnvironment("JwtSettings__usrm__Audience", "AlertOne.Local")
    .WithEnvironment("JwtSettings__usrm__ExpiryMinutes", "60");

// Alert management API on port 7072
var alertsApi = builder
    .AddAzureFunctionsProject<Projects.AlertManagement_Client>("alerts-api")
    .WithEndpoint("http", e => e.Port = 7072)
    .WaitForStart(cosmos)
    .WithEnvironment("CosmosDb__Endpoint", cosmos.Resource.UriExpression)
    .WithEnvironment("CosmosDb__AccountKey", CosmosEmulatorKey)
    .WithEnvironment("CosmosDb__DatabaseName", "alertone-alerts");

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

builder
    .Build()
    .Run();
