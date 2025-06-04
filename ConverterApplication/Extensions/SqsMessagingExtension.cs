using Amazon.Runtime;
using Amazon.SQS;
using ConverterApplication.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConverterApplication.Extensions;

public static class SqsMessagingExtension
{
    public static IServiceCollection AddSqsMessaging(this IServiceCollection services, IConfiguration configuration,
        string environmentName)
    {
        services.Configure<SqsSettings>(configuration.GetSection("Sqs"));
        services.Configure<LocalStackSettings>(configuration.GetSection("LocalStack"));

        services.AddTransient<IAmazonSQS>(sp =>
        {
            var localstackOptions = sp.GetRequiredService<IOptions<LocalStackSettings>>().Value;

            return environmentName switch
            {
                "Development" => CreateLocalSqsClient(localstackOptions),
                _ => new AmazonSQSClient()
            };
        });

        return services;
    }

    private static AmazonSQSClient CreateLocalSqsClient(LocalStackSettings settings) => new(
        new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
        new AmazonSQSConfig { ServiceURL = settings.ServiceUrl, AuthenticationRegion = settings.Region });
} 