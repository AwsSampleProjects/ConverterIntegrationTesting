using ConverterApplication.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ConverterApplication.Tests.Integration;

public class AssetQueryTests : IDisposable
{
    private readonly TestDatabaseHelper _dbHelper;
    private readonly IConfiguration _configuration;

    public AssetQueryTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Development.json")
            .Build();
        _dbHelper = new TestDatabaseHelper(_configuration);
    }

    [Fact]
    public async Task Should_Log_Queries_And_Load_Assets()
    {
        await _dbHelper.InitializeAsync();
        _dbHelper.ClearQueryLogs();

        var asset = await _dbHelper.Context.Assets
            .FirstOrDefaultAsync(a => a.CompanyId == 123);

        var assetsFromLogs = await _dbHelper.LoadAssetsFromQueryLogsAsync();

        Assert.NotNull(asset);
        Assert.Equal("Health", asset.Category);
        Assert.Contains(assetsFromLogs, a => a.CompanyId == 123 && a.Category == "Health");
    }

    [Fact]
    public async Task Should_Log_Multiple_Queries()
    {
        await _dbHelper.InitializeAsync();
        _dbHelper.ClearQueryLogs();

        var healthAssets = await _dbHelper.Context.Assets
            .Where(a => a.Category == "Health")
            .ToListAsync();

        var glassAssets = await _dbHelper.Context.Assets
            .Where(a => a.Category == "Glass")
            .ToListAsync();

        var assetsFromLogs = await _dbHelper.LoadAssetsFromQueryLogsAsync();

        Assert.Equal(2, healthAssets.Count);
        Assert.Single(glassAssets);
        Assert.Equal(3, assetsFromLogs.Count);
    }

    [Fact]
    public async Task Should_Load_Assets_By_Specific_Query()
    {
        await _dbHelper.InitializeAsync();
        _dbHelper.ClearQueryLogs();

        var asset = await _dbHelper.Context.Assets
            .FirstOrDefaultAsync(a => a.CompanyId == 123);

        var parameters = new Dictionary<string, object>
        {
            { "@__p_0", 123 }
        };

        var assetsFromLogs = await _dbHelper.LoadAssetsFromQueryLogsByQueryAsync("CompanyId", parameters);

        Assert.NotNull(asset);
        Assert.Single(assetsFromLogs);
        Assert.Equal(123, assetsFromLogs[0].CompanyId);
        Assert.Equal("Health", assetsFromLogs[0].Category);
    }

    public void Dispose()
    {
        _dbHelper.Dispose();
    }
} 