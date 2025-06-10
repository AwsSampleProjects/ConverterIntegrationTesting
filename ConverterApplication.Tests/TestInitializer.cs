using ConverterApplication;
using ConverterApplication.Database.Dapper;
using ConverterApplication.Database.Repositories;
using ConverterApplication.Database.Services;
using ConverterApplication.S3;
using ConverterApplication.Sqs;
using ConverterApplication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConverterApplication.Tests;

public class TestInitializer
{
    public IHost Host { get; }
    public IServiceProvider ServiceProvider { get; }

    public TestInitializer()
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

        builder.Services.AddScoped<IDataContext, DataContext>();
        builder.Services.AddScoped<IAssetRepository, AssetRepository>();
        builder.Services.AddScoped<IContractConverterService, ContractConverterService>();
        builder.Services.AddScoped<IQueryTrackingService, QueryTrackingService>();

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