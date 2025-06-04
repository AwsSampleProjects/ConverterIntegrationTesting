using System.Text.Json.Serialization;

namespace ConverterApplication.SqsModels;

public class MessageBody
{
    [JsonPropertyName("Records")]
    public List<S3EventRecord> Records { get; set; } = new();
}

public class S3EventRecord
{
    [JsonPropertyName("eventVersion")]
    public string EventVersion { get; set; } = string.Empty;

    [JsonPropertyName("eventSource")]
    public string EventSource { get; set; } = string.Empty;

    [JsonPropertyName("awsRegion")]
    public string AwsRegion { get; set; } = string.Empty;

    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = string.Empty;

    [JsonPropertyName("s3")]
    public S3Event S3 { get; set; } = new();
}

public class S3Event
{
    [JsonPropertyName("bucket")]
    public S3Bucket Bucket { get; set; } = new();

    [JsonPropertyName("object")]
    public S3Object Object { get; set; } = new();
}

public class S3Bucket
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class S3Object
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;
}