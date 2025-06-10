using System.Text.Json;
using ConverterApplication.Database.Models;
using Microsoft.Extensions.Logging;

namespace ConverterApplication.Database.Services;

public interface IQueryTrackingService
{
    DatabaseQuery StartQuery(Guid correlationId, string query, object? parameters);
    void CompleteQuery(DatabaseQuery query, object? response);
}

public class QueryTrackingService : IQueryTrackingService
{
    private readonly ILogger<QueryTrackingService> _logger;

    public QueryTrackingService(ILogger<QueryTrackingService> logger)
    {
        _logger = logger;
    }

    public DatabaseQuery StartQuery(Guid correlationId, string query, object? parameters)
    {
        var databaseQuery = new DatabaseQuery
        {
            CorrelationId = correlationId,
            Query = query,
            Parameters = parameters,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Starting database query. CorrelationId: {CorrelationId}, Query: {Query}, Parameters: {Parameters}",
            databaseQuery.CorrelationId, query, JsonSerializer.Serialize(parameters));

        return databaseQuery;
    }

    public void CompleteQuery(DatabaseQuery query, object? response)
    {
        query.Response = JsonSerializer.Serialize(response);
        query.Duration = DateTime.UtcNow - query.Timestamp;

        _logger.LogInformation("Completed database query. CorrelationId: {CorrelationId}, Duration: {Duration}ms, Response: {Response}",
            query.CorrelationId, query.Duration.TotalMilliseconds, query.Response);
    }
} 