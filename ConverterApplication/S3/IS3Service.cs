using ConverterApplication.Domain.Models;

namespace ConverterApplication.S3;

public interface IS3Service
{
    Task<List<Contract>> GetContractsFromS3Async(string bucketName, string key);
    Task SaveOutputContractToS3Async(string bucketName, string folderName, OutputContract contract, Guid correlationId);
}
