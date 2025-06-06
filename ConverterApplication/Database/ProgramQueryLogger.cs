using System.Text.Json;

namespace ConverterApplication.Database;

public class ProgramQueryLogger
{
    private static readonly Lazy<ProgramQueryLogger> _instance = new(() => new ProgramQueryLogger());
    private readonly List<QueryLog> _queryLogs = new();
    private readonly string _logFilePath;
    private readonly object _lockObject = new();

    public static ProgramQueryLogger Instance => _instance.Value;

    private ProgramQueryLogger()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(logsDirectory);
        _logFilePath = Path.Combine(logsDirectory, $"SqlOutput_{timestamp}.json");
    }

    public void LogQuery(QueryLog log)
    {
        lock (_lockObject)
        {
            _queryLogs.Add(log);
            SaveLogs();
        }
    }

    private void SaveLogs()
    {
        var output = new
        {
            Timestamp = DateTime.UtcNow,
            Queries = _queryLogs.Select(log => new
            {
                Query = log.Query,
                Parameters = log.Parameters,
                Results = log.Results,
                Duration = $"{log.Duration}ms",
                ExecutionTime = log.Timestamp
            }).ToList()
        };

        var json = JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(_logFilePath, json);
    }

    public List<QueryLog> GetLogs()
    {
        lock (_lockObject)
        {
            return _queryLogs.ToList();
        }
    }

    public void ClearLogs()
    {
        lock (_lockObject)
        {
            _queryLogs.Clear();
            SaveLogs();
        }
    }
} 