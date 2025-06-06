using ConverterApplication;
using ConverterApplication.Database;
using ConverterApplication.S3;
using Microsoft.EntityFrameworkCore;
using ConverterApplication.Sqs;
using ConverterApplication.Services;

Console.WriteLine("### Starting Converter Application ###");

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
    options.AddInterceptors(new QueryInterceptor(isTestMode: false));
});

builder.Services.AddSqsService(builder.Configuration, builder.Environment.EnvironmentName);
builder.Services.AddS3Service(builder.Configuration, builder.Environment.EnvironmentName);

builder.Services.AddScoped<IContractConverterService, ContractConverterService>();
builder.Services.AddScoped<DatabaseInitializer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

try
{
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();
        
        var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await initializer.InitializeAsync();
    }

    host.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Error during startup: {ex}");
    throw;
}