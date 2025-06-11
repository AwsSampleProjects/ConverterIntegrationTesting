using Amazon.S3;

namespace ConverterApplication.Tests.Fixtures;

using System;
using System.Threading.Tasks;
using Testcontainers.LocalStack;
using Xunit;

public class LocalStackFixture : IAsyncLifetime
{
    private readonly LocalStackContainer _localStackContainer;

    public string EndpointUrl => _localStackContainer.GetConnectionString();
    public string AccessKey => "test";
    public string SecretKey => "test";
    public string Region => "eu-central-1";

    public LocalStackFixture()
    {
        string initScript = GetStartScriptFilePath();
        
        _localStackContainer = new LocalStackBuilder()
            .WithImage("localstack/localstack:4.3.0")
            .WithEnvironment("DEFAULT_REGION", Region)
            .WithEnvironment("AWS_ACCESS_KEY_ID", AccessKey)
            .WithEnvironment("AWS_SECRET_ACCESS_KEY", SecretKey)
            .WithEnvironment("SERVICES", "s3,sqs")
            .WithCleanUp(true)
            .WithPortBinding(45660, 4566)
            .WithBindMount(initScript, "/etc/localstack/init/ready.d/init-localstack.sh")
            .Build();
    }

    private static string GetStartScriptFilePath()
    {
        var currentDirectory = AppContext.BaseDirectory;
        var initScriptPath = Path.Combine(currentDirectory, "./Scripts/init-localstack.sh");
        
        if (!File.Exists(initScriptPath))
        {
            throw new FileNotFoundException($"LocalStack init script not found at: {initScriptPath}");
        }

        return initScriptPath;
    }

    public AmazonS3Client GetS3Client()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = EndpointUrl,
            AuthenticationRegion = Region,
            ForcePathStyle = true
        };
        return new AmazonS3Client(config);
    }

    public async Task InitializeAsync()
    {
        await _localStackContainer.StartAsync();
        await Task.Delay(2000);
    }

    public async Task DisposeAsync()
    {
        await _localStackContainer.DisposeAsync();
    }
}