using System.Text.Json;
using ConverterApplication.Database.Models;
using ConverterApplication.Database.Repositories;
using QueryLogger;

namespace ConverterApplication.Tests.Repositories;

public class FakeAssetRepository : IAssetRepository
{
    private readonly List<DatabaseQuery> _queries;

    public FakeAssetRepository(string queryResponsePath)
    {
        if (!File.Exists(queryResponsePath))
            throw new FileNotFoundException(queryResponsePath);

        var json = File.ReadAllText(queryResponsePath);
        var response = JsonSerializer.Deserialize<QueryLog>(json) ??
                       throw new InvalidOperationException("Failed to deserialize query response");
        _queries = response.Queries;
    }

    public async Task<Asset?> GetByCompanyIdAsync(int companyId, Guid correlationId)
    {
        const string query = AssetRepository.Query;

        // TODO: Do better comparision for parameters
        var matchingQuery = _queries.FirstOrDefault(q => q.Query == query && q.Parameters.ToString().Contains(companyId.ToString()));

        if (matchingQuery == null)
        {
            throw new InvalidOperationException($"No matching query found for CompanyId: {companyId}");
        }

        var result = JsonSerializer.Deserialize<Asset>(matchingQuery.Response);
        if (result == null)
        {
            throw new InvalidOperationException("Failed to deserialize asset response");
        }

        return result;
    }
}

public class QueryLog
{
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; }
    public List<DatabaseQuery> Queries { get; set; } = new();
}