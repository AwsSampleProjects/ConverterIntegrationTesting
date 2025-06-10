using ConverterApplication.Domain.Models;

namespace ConverterApplication.Services;

public interface IContractConverterService
{
    Task ConvertContractsAsync(List<Contract> contracts, Guid correlationId);
}