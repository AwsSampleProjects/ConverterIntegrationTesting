using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using ConverterApplication.Settings;
using ConverterApplication.SqsModels;
using Microsoft.Extensions.Options;

namespace ConverterApplication;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsSettings _sqsSettings;
    private string? _queueUrl;

    public Worker(ILogger<Worker> logger, IAmazonSQS sqsClient, IOptions<SqsSettings> sqsSettings)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _sqsSettings = sqsSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _queueUrl = await GetQueueUrlAsync();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var receiveMessageRequest = new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = _sqsSettings.MaxNumberOfMessages,
                    WaitTimeSeconds = _sqsSettings.WaitTimeSeconds
                };

                var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

                foreach (var message in response.Messages)
                {
                    try
                    {
                        var messageBody = JsonSerializer.Deserialize<MessageBody>(message.Body);
                        if (messageBody?.Records != null && messageBody.Records.Any())
                        {
                            var s3Event = messageBody.Records[0];
                            var s3ObjectKey = s3Event.S3.Object.Key;
                            
                            _logger.LogInformation("Received S3 object key: {Key}", s3ObjectKey);
                            
                            await _sqsClient.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving messages from SQS");
            }
            
            await Task.Delay(3000, stoppingToken);
        }
    }

    private async Task<string> GetQueueUrlAsync()
    {
        var request = new GetQueueUrlRequest
        {
            QueueName = _sqsSettings.QueueName
        };

        var response = await _sqsClient.GetQueueUrlAsync(request);
        return response.QueueUrl;
    }
}