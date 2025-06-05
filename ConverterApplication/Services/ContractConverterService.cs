using System.Text.Json;
using ConverterApplication.Database;
using ConverterApplication.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverterApplication.Services;

public class ContractConverterService : IContractConverterService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ContractConverterService> _logger;

    public ContractConverterService(ApplicationDbContext dbContext, ILogger<ContractConverterService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ConvertContractsAsync(List<Contract> contracts)
    {
        foreach (var contract in contracts)
        {
            try
            {
                var asset = await _dbContext.Assets
                    .FirstOrDefaultAsync(a => a.CompanyId == contract.CompanyId);

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

                var outputPath = $"Contract_{contract.ContractId}.json";
                var json = JsonSerializer.Serialize(outputContract, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(outputPath, json);
                _logger.LogInformation("Created output file: {Path}", outputPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting contract {ContractId}", contract.ContractId);
            }
        }
    }
}