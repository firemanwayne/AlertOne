namespace UserManagement.Infrastructure.Configurations;

/// <summary>
/// Configuration settings for database connection
/// </summary>
public sealed class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the database provider name.
    /// </summary>
    public string Provider { get; set; } = "SqlServer"; // SqlServer or Sqlite
}
