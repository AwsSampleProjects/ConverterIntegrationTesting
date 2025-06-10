using ConverterApplication;
using ConverterApplication.Database;
using ConverterApplication.S3;
using Microsoft.EntityFrameworkCore;
using ConverterApplication.Sqs;
using ConverterApplication.Services;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddScoped<IContractConverterService, ContractConverterService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();