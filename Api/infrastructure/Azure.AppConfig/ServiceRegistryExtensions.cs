using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.AppConfig;

public static class ServiceRegistryExtensions
{
    public static IConfigurationBuilder AddAppConfiguration(this IConfigurationBuilder builder)
    {
        var credential = new DefaultAzureCredential();

        builder.AddAzureAppConfiguration(options =>
        {
            options
            .Connect(new Uri("https://alert-one-app-config-dev.azconfig.io"), credential)
            // Load all keys that start with `AzureNotificationHubs:` and have no label
            .Select(keyFilter: "AzureNotificationHubs:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "AzureCommunications:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "AzureSqlServer:*", labelFilter: LabelFilter.Null)
            .Select(keyFilter: "JwtSettings:*", labelFilter: LabelFilter.Null)
            // Configure Key Vault reference resolution
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(credential);
            })
            .ConfigureRefresh(refreshOptions =>
            {
                // Trigger full configuration refresh only if the `AzureNotificationHubs` changes.
                refreshOptions
                .Register("AzureNotificationHubs", refreshAll: true)
                .Register("AzureCommunications", refreshAll: true)
                .Register("AzureSqlServer", refreshAll: true)
                .Register("JwtSettings", refreshAll: true);
            });
        });

        return builder;
    }
}
