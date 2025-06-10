using System.Text.Json;
using ConverterApplication.Database;
using ConverterApplication.Database.Models;
using ConverterApplication.Domain.Models;
using ConverterApplication.S3;
using ConverterApplication.Settings;
using Microsoft.Extensions.Options;

namespace ConverterApplication.Services;

public class ContractConverterService : IContractConverterService
{
    private readonly ILogger<ContractConverterService> _logger;
    private readonly IS3Service _s3Service;
    private readonly S3Settings _s3Settings;

    public ContractConverterService(
        ILogger<ContractConverterService> logger,
        IS3Service s3Service,
        IOptions<S3Settings> s3Settings)
    {
        _logger = logger;
        _s3Service = s3Service;
        _s3Settings = s3Settings.Value;
    }

    public async Task ConvertContractsAsync(List<Contract> contracts)
    {
        foreach (var contract in contracts)
        {
            try
            {
                Asset asset = null; // Get asset from the DB

                if (asset == null)
                {
                    _logger.LogWarning("No asset found for CompanyId: {CompanyId}", contract.CompanyId);
                    continue;
                }

                var outputContract = new OutputContract
                {
                    ContractId = contract.ContractId,
                    CompanyId = contract.CompanyId,
                    SignatureDate = contract.SignatureDate,
                    UserData = contract.UserData,
                    Category = asset.Category
                };

                await _s3Service.SaveOutputContractToS3Async(
                    _s3Settings.OutputBucketName,
                    _s3Settings.OutputFolderName,
                    outputContract);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting contract {ContractId}", contract.ContractId);
            }
        }
    }
}