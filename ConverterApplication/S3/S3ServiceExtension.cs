using Amazon.Runtime;
using Amazon.S3;
using ConverterApplication.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConverterApplication.S3;

public static class S3ServiceExtension
{
    public static IServiceCollection AddS3Service(this IServiceCollection services, IConfiguration configuration,
        string environmentName)
    {
        services.Configure<S3Settings>(configuration.GetSection("S3"));
        services.Configure<LocalStackSettings>(configuration.GetSection("LocalStack"));

        services.AddTransient<IAmazonS3>(sp =>
        {
            var localstackOptions = sp.GetRequiredService<IOptions<LocalStackSettings>>().Value;

            return environmentName switch
            {
                "Development" => CreateLocalS3Client(localstackOptions),
                _ => new AmazonS3Client()
            };
        });
        services.AddTransient<IS3Service, S3Service>();

        return services;
    }

    private static AmazonS3Client CreateLocalS3Client(LocalStackSettings settings) => new(
        new BasicAWSCredentials(settings.AccessKeyId, settings.SecretAccessKey),
        new AmazonS3Config
        {
            ServiceURL = settings.ServiceUrl,
            ForcePathStyle = true,
            AuthenticationRegion = settings.Region
        });
} 