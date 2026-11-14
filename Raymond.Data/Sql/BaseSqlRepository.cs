using Dapper;
using Microsoft.Extensions.Logging;

namespace Raymond.Data.Sql;

public abstract class BaseSqlRepository(ILogger<BaseSqlRepository> logger, IDbConnectionFactory factory)
{
    protected async Task<T?> GetFirstAsync<T>(string query, object? param = null)
    {
        try
        {
            using var connection = factory.CreateConnection();

            logger.LogDebug("Executing query: {Query}", query);
            return await connection.QueryFirstOrDefaultAsync<T>(query, param);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error executing query: {Query}", query);
            throw;
        }
    }
    
    protected async Task<List<T>> GetAllAsync<T>(string query, object? param = null)
    {
        try
        {
            using var connection = factory.CreateConnection();
            logger.LogDebug("Executing query: {Query}", query);
            return (await connection.QueryAsync<T>(query, param)).ToList();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error executing query: {Query}", query);
            throw;
        }
    }
    
    protected async Task ExecuteAsync(string query, object? param = null)
    {
        try
        {
            using var connection = factory.CreateConnection();
            logger.LogDebug("Executing query: {Query}", query);
            await connection.ExecuteAsync(query, param);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error executing query: {Query}", query);
            throw;
        }
    }
}