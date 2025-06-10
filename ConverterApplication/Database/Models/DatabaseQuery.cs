using System;

namespace ConverterApplication.Database.Models;

public class DatabaseQuery
{
    public Guid CorrelationId { get; set; }
    public string Query { get; set; } = string.Empty;
    public object? Parameters { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Response { get; set; }
    public TimeSpan Duration { get; set; }
} 