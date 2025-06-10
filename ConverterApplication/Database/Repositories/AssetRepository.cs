using ConverterApplication.Database.Models;
using Dapper;
using Npgsql;

namespace ConverterApplication.Database.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly IConfiguration _configuration;

    public AssetRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<Asset> GetByCompanyIdAsync(int companyId)
    {
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        return await connection.QueryFirstOrDefaultAsync<Asset>(
            @"SELECT * FROM ""Asset"" WHERE ""CompanyId"" = @CompanyId",
            new { CompanyId = companyId });
    }
} 