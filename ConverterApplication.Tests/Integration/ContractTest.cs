using Amazon.S3;
using Amazon.SQS;
using Amazon.SQS.Model;
using ConverterApplication.Settings;
using ConverterApplication.Tests.Fixtures;
using ConverterApplication.Tests.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace ConverterApplication.Tests.Integration;

public class ContractTest : IAsyncLifetime, IClassFixture<LocalStackFixture>
{
    private readonly TestInitializer _initializer;
    private readonly IHost _host;

    private readonly LocalStackFixture _localstack;
    private S3Helper _s3Helper;

    public ContractTest(LocalStackFixture localstack)
    {
        var queryResponsePath = Path.Combine(AppContext.BaseDirectory,
            "./TestFiles/ContractTest_1/Queries/Queries_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json");
        
        _initializer = new TestInitializer(queryResponsePath);
        _host = _initializer.Host;

        _localstack = localstack;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _initializer.InitializeAsync();
            await _localstack.InitializeAsync();
            
            await _host.StartAsync();
            

            _s3Helper = new S3Helper(_localstack.GetS3Client());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public async Task DisposeAsync()
    {
        await _host.StopAsync();
        await _localstack.DisposeAsync();
    }

    [Fact]
    public async Task ContractsFile_ProperlyUploaded_ResultProperlyGenerated()
    {
        // Upload test input file to S3
        var result = await _s3Helper.UploadFile("converter-input-bucket-test",
            "Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json",
            "./TestFiles/ContractTest_1/Input/Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json");

        await Task.Delay(1000000);
        
        
        
        
        
        
    }
}