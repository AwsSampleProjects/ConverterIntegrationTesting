using System.Text.Json;
using ConverterApplication.Domain.Models;
using ConverterApplication.Tests.Fixtures;
using ConverterApplication.Tests.Tools;
using Microsoft.Extensions.Hosting;
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
        // Load recorded test queries
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        const string contract1001Name = "Contract_1001_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json";
        const string contract1002Name = "Contract_1002_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json";
        const string folderName = "Contracts";
        
        // Init
        // Upload test input file to S3
        var result = await _s3Helper.UploadFile("converter-input-bucket-test",
            "Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json",
            "./TestFiles/ContractTest_1/Input/Contracts_5deaa247-1e8c-437c-9c22-f4d164fae0f1.json");
        
        // Expected
        var contract1001Expected = TestsHelper.Deserialize<Contract>($"./TestFiles/ContractTest_1/Output/{contract1001Name}");
        var contract1002Expected = TestsHelper.Deserialize<Contract>($"./TestFiles/ContractTest_1/Output/{contract1002Name}");
        
        // Wait
        await Task.Delay(3000);
        
        // Actual
        // Download result files from output bucket
        var contract1001Actual = await _s3Helper.DownloadFile<Contract>("converter-output-bucket-test",
            $"{folderName}/{contract1001Name}");
        var contract1002Actual = await _s3Helper.DownloadFile<Contract>("converter-output-bucket-test",
            $"{folderName}/{contract1002Name}");

        // Assert
        var expected1001 = JsonSerializer.Serialize(contract1001Expected, options);
        var actual1001 = JsonSerializer.Serialize(contract1001Actual, options);
        Assert.Equal(expected1001, actual1001);
        
        var expected1002 = JsonSerializer.Serialize(contract1002Expected, options);
        var actual1002 = JsonSerializer.Serialize(contract1002Actual, options);
        Assert.Equal(expected1002, actual1002);
    }
}