using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using ConverterApplication.S3;
using ConverterApplication.Settings;
using ConverterApplication.Sqs.Models;
using ConverterApplication.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConverterApplication;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqsClient;
    private readonly SqsSettings _sqsSettings;
    private readonly IS3Service _s3Service;
    private readonly IServiceScopeFactory _scopeFactory;
    private string? _queueUrl;

    public Worker(
        ILogger<Worker> logger, 
        IAmazonSQS sqsClient, 
        IOptions<SqsSettings> sqsSettings,
        IS3Service s3Service,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _sqsClient = sqsClient;
        _sqsSettings = sqsSettings.Value;
        _s3Service = s3Service;
        _scopeFactory = scopeFactory;
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
                
                if (response.Messages is null) continue;
                
                foreach (var message in response.Messages)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var contractConverterService = scope.ServiceProvider.GetRequiredService<IContractConverterService>();
                        
                        var messageBody = JsonSerializer.Deserialize<MessageBody>(message.Body);
                        if (messageBody?.Records != null && messageBody.Records.Any())
                        {
                            var s3Event = messageBody.Records[0];
                            var bucketName = s3Event.S3.Bucket.Name;
                            var objectKey = s3Event.S3.Object.Key;
                            
                            _logger.LogInformation("Processing S3 object. Bucket: {Bucket}, Key: {Key}", bucketName, objectKey);
                            
                            var contracts = await _s3Service.GetContractsFromS3Async(bucketName, objectKey);
                            _logger.LogInformation("Retrieved {Count} contracts from S3", contracts.Count);
                            
                            await contractConverterService.ConvertContractsAsync(contracts);
                            _logger.LogInformation("Successfully processed {Count} contracts", contracts.Count);
                            
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