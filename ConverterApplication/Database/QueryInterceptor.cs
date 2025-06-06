using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;

namespace ConverterApplication.Database;

public class QueryInterceptor : DbCommandInterceptor
{
    private readonly List<QueryLog> _queryLogs = new();
    private readonly Stopwatch _stopwatch = new();
    private readonly bool _isTestMode;

    public IReadOnlyList<QueryLog> QueryLogs => _queryLogs.AsReadOnly();

    public QueryInterceptor(bool isTestMode = false)
    {
        _isTestMode = isTestMode;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        _stopwatch.Restart();
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        _stopwatch.Stop();
        var queryLog = new QueryLog
        {
            Query = command.CommandText,
            Parameters = command.Parameters.Cast<DbParameter>()
                .ToDictionary(p => p.ParameterName, p => p.Value),
            Duration = _stopwatch.ElapsedMilliseconds,
            Timestamp = DateTime.UtcNow
        };

        if (result != null)
        {
            var results = new List<Dictionary<string, object>>();
            while (await result.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < result.FieldCount; i++)
                {
                    row[result.GetName(i)] = result.GetValue(i);
                }
                results.Add(row);
            }
            queryLog.Results = results;
        }

        if (_isTestMode)
        {
            _queryLogs.Add(queryLog);
        }
        else
        {
            ProgramQueryLogger.Instance.LogQuery(queryLog);
        }

        return result;
    }

    public void ClearLogs()
    {
        _queryLogs.Clear();
    }

    public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result);
    }

    public ValueTask<InterceptionResult<object>> ScalarExecutedAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        object? value,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result);
    }

    public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result);
    }

    public ValueTask<InterceptionResult<int>> NonQueryExecutedAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        int rowsAffected,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(result);
    }
}

public class QueryLog
{
    public string Query { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public List<Dictionary<string, object>> Results { get; set; }
    public long Duration { get; set; }
    public DateTime Timestamp { get; set; }
} 