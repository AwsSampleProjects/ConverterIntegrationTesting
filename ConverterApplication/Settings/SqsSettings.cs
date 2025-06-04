namespace ConverterApplication.Settings;

public class SqsSettings
{
    public string QueueName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int MaxNumberOfMessages { get; set; } = 10;
    public int WaitTimeSeconds { get; set; } = 20;
}