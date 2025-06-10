using ConverterApplication;
using ConverterApplication.Database.Dapper;
using ConverterApplication.Database.Repositories;
using ConverterApplication.Database.Services;
using ConverterApplication.S3;
using ConverterApplication.Sqs;
using ConverterApplication.Services;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddScoped<IDataContext, DataContext>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IContractConverterService, ContractConverterService>();
builder.Services.AddScoped<IQueryTrackingService, QueryTrackingService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<IDataContext>();
    await context.Init();
}

host.Run();