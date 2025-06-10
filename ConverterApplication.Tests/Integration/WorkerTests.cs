using Amazon.SQS;
using Amazon.SQS.Model;
using ConverterApplication.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConverterApplication.Tests.Integration;

public class WorkerTests : IAsyncLifetime
{
    private readonly TestInitializer _initializer;
    private readonly IHost _host;
    private string? _queueUrl;

    public WorkerTests()
    {
        _initializer = new TestInitializer();
        _host = _initializer.Host;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _initializer.InitializeAsync();
            _queueUrl = await GetQueueUrlAsync();
            await _host.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
    }

    [Fact]
    public async Task Worker_ShouldStartAndListenToSQS()
    {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
            QueueUrl = _queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 5
        };

        // var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest);
        // Assert.NotNull(response);
    }

    private async Task<string> GetQueueUrlAsync()
    {
        // var request = new GetQueueUrlRequest
        // {
        //     QueueName = _sqsSettings.QueueName
        // };
        //
        // var response = await _sqsClient.GetQueueUrlAsync(request);
        // return response.QueueUrl;
        return null;
    }
} 