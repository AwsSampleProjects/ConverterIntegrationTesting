using ConverterApplication.Database.Models;
using ConverterApplication.Database.Services;
using Dapper;
using Npgsql;

namespace ConverterApplication.Database.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly IConfiguration _configuration;
    private readonly IQueryTrackingService _queryTrackingService;

    public AssetRepository(IConfiguration configuration, IQueryTrackingService queryTrackingService)
    {
        _configuration = configuration;
        _queryTrackingService = queryTrackingService;
    }

    public async Task<Asset> GetByCompanyIdAsync(int companyId, Guid correlationId)
    {
        const string query = @"SELECT * FROM ""Asset"" WHERE ""CompanyId"" = @CompanyId";
        var parameters = new { CompanyId = companyId };
        
        var databaseQuery = _queryTrackingService.StartQuery(correlationId, query, parameters);
        
        using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var result = await connection.QueryFirstOrDefaultAsync<Asset>(query, parameters);
        
        _queryTrackingService.CompleteQuery(databaseQuery, result);
        
        return result;
    }
} 