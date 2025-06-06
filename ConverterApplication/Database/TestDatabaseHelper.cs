using ConverterApplication.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConverterApplication.Database;

public class TestDatabaseHelper : IDisposable
{
    private readonly TestApplicationDbContext _context;
    private readonly string _connectionString;

    public TestDatabaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        _context = new TestApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        var initializer = new DatabaseInitializer(_context);
        await initializer.InitializeAsync();
    }

    public async Task<List<Asset>> LoadAssetsFromQueryLogsAsync()
    {
        var assets = new List<Asset>();
        foreach (var log in _context.QueryInterceptor.QueryLogs)
        {
            if (log.Results == null) continue;

            foreach (var result in log.Results)
            {
                if (result.TryGetValue("Id", out var id) &&
                    result.TryGetValue("CompanyId", out var companyId) &&
                    result.TryGetValue("ContractId", out var contractId) &&
                    result.TryGetValue("Category", out var category))
                {
                    assets.Add(new Asset
                    {
                        Id = (Guid)id,
                        CompanyId = Convert.ToInt32(companyId),
                        ContractId = Convert.ToInt32(contractId),
                        Category = category.ToString()
                    });
                }
            }
        }
        return assets;
    }

    public async Task<List<Asset>> LoadAssetsFromQueryLogsByQueryAsync(string query, Dictionary<string, object> parameters)
    {
        var assets = new List<Asset>();
        foreach (var log in _context.QueryInterceptor.QueryLogs)
        {
            if (log.Results == null || !log.Query.Contains(query)) continue;

            var matchesParameters = true;
            foreach (var param in parameters)
            {
                if (!log.Parameters.TryGetValue(param.Key, out var value) || 
                    !value?.ToString()?.Equals(param.Value.ToString(), StringComparison.OrdinalIgnoreCase) == true)
                {
                    matchesParameters = false;
                    break;
                }
            }

            if (!matchesParameters) continue;

            foreach (var result in log.Results)
            {
                if (result.TryGetValue("Id", out var id) &&
                    result.TryGetValue("CompanyId", out var companyId) &&
                    result.TryGetValue("ContractId", out var contractId) &&
                    result.TryGetValue("Category", out var category))
                {
                    assets.Add(new Asset
                    {
                        Id = (Guid)id,
                        CompanyId = Convert.ToInt32(companyId),
                        ContractId = Convert.ToInt32(contractId),
                        Category = category.ToString()
                    });
                }
            }
        }
        return assets;
    }

    public void ClearQueryLogs()
    {
        _context.QueryInterceptor.ClearLogs();
    }

    public TestApplicationDbContext Context => _context;

    public void Dispose()
    {
        _context.Dispose();
    }
} 