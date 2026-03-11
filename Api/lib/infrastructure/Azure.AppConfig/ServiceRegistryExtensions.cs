using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.AppConfig;

public static class ServiceRegistryExtensions
{
    public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder builder)
    {
        // Build preliminary config to get App Configuration endpoint
        var preliminaryConfig = builder.Build();
        var appConfigBaseUri = preliminaryConfig["AppConfig:BaseUri"];

        // Skip App Configuration for local development when BaseUri is not configured
        if (string.IsNullOrWhiteSpace(appConfigBaseUri))
        {
            return builder;
        }

        var credential = new DefaultAzureCredential();

        builder.AddAzureAppConfiguration(options =>
        {
            options
            .Connect(new Uri(appConfigBaseUri), credential)
            // Load all keys with no label
            .Select(KeyFilter.Any, LabelFilter.Null)
            // Configure Key Vault reference resolution
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credential);
            })
            .ConfigureRefresh(refreshOptions =>
            {
                // Use a sentinel key to trigger refresh of all values
                refreshOptions
                .Register("Sentinel", refreshAll: true)
                .SetRefreshInterval(TimeSpan.FromMinutes(5));
            });
        });

        return builder;
    }
}
