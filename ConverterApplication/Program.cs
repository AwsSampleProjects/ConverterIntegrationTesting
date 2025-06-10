using ConverterApplication;
using ConverterApplication.Database.Dapper;
using ConverterApplication.Database.Migrations;
using ConverterApplication.S3;
using ConverterApplication.Sqs;
using ConverterApplication.Services;
using FluentMigrator.Runner;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ScanIn(typeof(InitialMigration).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

builder.Services.AddScoped<IDataContext, DataContext>();

builder.Services.AddScoped<IContractConverterService, ContractConverterService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<IDataContext>();
    await context.Init();
}

host.Run();