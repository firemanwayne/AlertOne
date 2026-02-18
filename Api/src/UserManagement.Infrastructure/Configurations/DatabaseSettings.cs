namespace UserManagement.Infrastructure.Configurations;

/// <summary>
/// Configuration settings for database connection
/// </summary>
public sealed class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = "SqlServer"; // SqlServer or Sqlite
}
