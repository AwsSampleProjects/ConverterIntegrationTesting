using System.Text.Json;
using ConverterApplication.Database.Models;
using ConverterApplication.Database.Repositories;
using ConverterApplication.Database.Services;
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
    private readonly IAssetRepository _assetRepository;
    private readonly IQueryTrackingService _queryTrackingService;

    public ContractConverterService(
        ILogger<ContractConverterService> logger,
        IS3Service s3Service,
        IOptions<S3Settings> s3Settings,
        IAssetRepository assetRepository,
        IQueryTrackingService queryTrackingService)
    {
        _logger = logger;
        _s3Service = s3Service;
        _s3Settings = s3Settings.Value;
        _assetRepository = assetRepository;
        _queryTrackingService = queryTrackingService;
    }

    public async Task ConvertContractsAsync(List<Contract> contracts)
    {
        var correlationId = Guid.NewGuid();

        foreach (var contract in contracts)
        {
            try
            {
                var asset = await _assetRepository.GetByCompanyIdAsync(contract.CompanyId, correlationId);

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

        await _queryTrackingService.SaveQueriesAsync(correlationId);
    }
}