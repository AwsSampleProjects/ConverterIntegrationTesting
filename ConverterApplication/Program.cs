using ConverterApplication;
using ConverterApplication.Extensions;
using ConverterApplication.Services;
using Microsoft.EntityFrameworkCore;
using ConverterApplication.Settings;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSqsMessaging(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddScoped<DatabaseInitializer>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

host.Run();