using ConverterApplication.Database.Models;

namespace ConverterApplication.Database.Repositories;

public interface IAssetRepository
{
    Task<Asset> GetByCompanyIdAsync(int companyId, Guid correlationId);
} 