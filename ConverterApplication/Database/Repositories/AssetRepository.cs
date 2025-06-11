using ConverterApplication.Database.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using QueryLogger;

namespace ConverterApplication.Database.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly string _connectionString;
    private readonly IQueryTrackingService _queryTrackingService;
    private readonly bool _recordQueries;

    public const string Query = @"SELECT * FROM ""Asset"" WHERE ""CompanyId"" = @CompanyId";

    public AssetRepository(IConfiguration configuration, IQueryTrackingService queryTrackingService)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _queryTrackingService = queryTrackingService;
        _recordQueries = configuration.GetValue<bool>("RecordQueries");
    }

    public async Task<Asset?> GetByCompanyIdAsync(int companyId, Guid correlationId)
    {
        var parameters = new { CompanyId = companyId };

        DatabaseQuery databaseQuery = null!;
        if(_recordQueries)
            databaseQuery = _queryTrackingService.StartQuery(correlationId, Query, parameters);

        await using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QueryFirstOrDefaultAsync<Asset>(Query, parameters);
        
        if(_recordQueries)
            _queryTrackingService.CompleteQuery(databaseQuery, result);
        
        return result;
    }
} 