using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace QueryLogger;

public interface IQueryTrackingService
{
    DatabaseQuery StartQuery(Guid correlationId, string query, object? parameters);
    void CompleteQuery(DatabaseQuery query, object? response);
    Task SaveQueriesAsync(Guid correlationId);
}

public class QueryTrackingService : IQueryTrackingService
{
    private readonly ILogger<QueryTrackingService> _logger;
    private readonly string _queryLogsDirectory;
    private readonly Dictionary<Guid, List<DatabaseQuery>> _queries = new();

    public QueryTrackingService(ILogger<QueryTrackingService> logger)
    {
        _logger = logger;
        _queryLogsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QueryLogs");
        Directory.CreateDirectory(_queryLogsDirectory);
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

        if (!_queries.TryGetValue(correlationId, out var value))
        {
            value = [];
            _queries[correlationId] = value;
        }

        value.Add(databaseQuery);

        return databaseQuery;
    }

    public void CompleteQuery(DatabaseQuery query, object? response)
    {
        query.Response = JsonSerializer.Serialize(response);
        query.Duration = DateTime.UtcNow - query.Timestamp;
    }

    public async Task SaveQueriesAsync(Guid correlationId)
    {
        try
        {
            if (!_queries.TryGetValue(correlationId, out var queries))
            {
                return;
            }

            var fileName = $"Queries_{correlationId}.json";
            var filePath = Path.Combine(_queryLogsDirectory, fileName);

            var queryLog = new
            {
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Queries = queries
            };

            var json = JsonSerializer.Serialize(queryLog, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);
            _queries.Remove(correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving queries to file. CorrelationId: {CorrelationId}", correlationId);
        }
    }
} 