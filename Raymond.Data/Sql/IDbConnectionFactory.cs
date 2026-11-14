using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Raymond.Data.Sql;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}

public sealed class SqlConnectionFactory(IConfiguration config, ILogger<SqlConnectionFactory> logger) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        try
        {
            var connectionString = config.GetConnectionString("raymond-sql");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("raymond-sql connection string is not configured. Please check configuration settings.");
            }
            
            return new SqlConnection(connectionString);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create SQL connection");
            throw;
        }
    }
}