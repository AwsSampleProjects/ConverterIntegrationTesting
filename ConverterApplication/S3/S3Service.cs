using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;
using ConverterApplication.Domain.Models;

namespace ConverterApplication.S3;


public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<List<Contract>> GetContractsFromS3Async(string bucketName, string key)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            await using var responseStream = response.ResponseStream;
            using var reader = new StreamReader(responseStream);
            var content = await reader.ReadToEndAsync();

            var contracts = JsonSerializer.Deserialize<List<Contract>>(content);
            return contracts ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts from S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            throw;
        }
    }

    public async Task SaveOutputContractToS3Async(string bucketName, string folderName, OutputContract contract, Guid correlationId)
    {
        try
        {
            var json = JsonSerializer.Serialize(contract, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var key = $"{folderName}/Contract_{contract.ContractId}_{correlationId}.json";
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = json,
                ContentType = "application/json"
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("Saved output contract to S3. Bucket: {Bucket}, Key: {Key}", bucketName, key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving output contract to S3. Bucket: {Bucket}, ContractId: {ContractId}, CorrelationId: {CorrelationId}", bucketName, contract.ContractId, correlationId);
            throw;
        }
    }
} 