using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace AlertManagement.Client.OpenApi;

/// <summary>
/// OpenAPI document settings for the AlertOne Alert Management API. The OpenAPI extension discovers this class by type,
/// so it must be public. The document is served at <c>/api/swagger.json</c>, <c>/api/openapi/v3.json</c>
/// and the Swagger UI at <c>/api/swagger/ui</c>.
/// </summary>
public sealed class OpenApiConfiguration : DefaultOpenApiConfigurationOptions
{
    /// <inheritdoc />
    public override OpenApiInfo Info { get; set; } = new()
    {
        Version = "v1",
        Title = "AlertOne Alert Management API",
        Description = "Alerts, alert types, alert status levels and push notifications for AlertOne."
    };

    /// <inheritdoc />
    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
