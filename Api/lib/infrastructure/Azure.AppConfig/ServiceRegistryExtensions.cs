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
            .Select(keyFilter: "AzureNotificationHubs:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "AzureCommunications:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "CosmosDb:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "JwtSettings:*", labelFilter: LabelFilter.Null)
            // Configure Key Vault reference resolution
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credential);
            })
            .ConfigureRefresh(refreshOptions =>
            {
                refreshOptions
                .Register("AzureNotificationHubs", refreshAll: true)
                .Register("AzureCommunications", refreshAll: true)
                .Register("CosmosDb", refreshAll: true)
                .Register("JwtSettings", refreshAll: true);
            });
        });

        return builder;
    }
}
