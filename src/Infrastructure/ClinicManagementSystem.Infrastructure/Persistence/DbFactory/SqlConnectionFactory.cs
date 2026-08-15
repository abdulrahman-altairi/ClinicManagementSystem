using System.Data;
using ClinicManagementSystem.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ClinicManagementSystem.Infrastructure.Persistence.DbFactory;


public sealed class SqlConnectionOptions
{
    public string DefaultConnection { get; init; } = string.Empty;
}

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string                     _connectionString;
    private readonly ILogger<SqlConnectionFactory> _logger;

    public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured in appsettings.json.");
        _logger = logger;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("SQL connection opened. Server: {Server}", connection.DataSource);
            return connection;
        }
        catch (Exception ex)
        {
            connection.Dispose();
            _logger.LogError(ex, "Failed to open SQL Server connection.");
            throw;
        }
    }
}