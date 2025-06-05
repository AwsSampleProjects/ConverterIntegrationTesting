using ConverterApplication;
using ConverterApplication.Database;
using ConverterApplication.S3;
using Microsoft.EntityFrameworkCore;
using ConverterApplication.Settings;
using ConverterApplication.Sqs;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

host.Run();