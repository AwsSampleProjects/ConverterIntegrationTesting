using ConverterApplication.Database.Dapper;
using ConverterApplication.Database.Repositories;
using ConverterApplication.S3;
using ConverterApplication.Sqs;
using ConverterApplication.Services;
using ConverterApplication.Tests.Database;
using ConverterApplication.Tests.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QueryLogger;

namespace ConverterApplication.Tests;

public class TestInitializer
{
    public IHost Host { get; }
    private IServiceProvider ServiceProvider { get; }

    public TestInitializer(string queryResponsePath)
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Development"
        });

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        builder.Configuration.AddConfiguration(configuration);

        builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
        builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

        builder.Services.AddScoped<IDataContext, FakeDataContext>();
        builder.Services.AddScoped<IAssetRepository>(_ => new FakeAssetRepository(queryResponsePath));
        builder.Services.AddScoped<IContractConverterService, ContractConverterService>();
        builder.Services.AddScoped<IQueryTrackingService, QueryTrackingService>();
        builder.Services.AddHostedService<Worker>();

        Host = builder.Build();
        ServiceProvider = Host.Services;
    }

    public async Task InitializeAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IDataContext>();
        await context.Init();
    }
} 