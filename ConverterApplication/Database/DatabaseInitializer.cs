using ConverterApplication.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverterApplication.Database;

public class DatabaseInitializer
{
    private readonly ApplicationDbContext _context;

    public DatabaseInitializer(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await _context.Database.EnsureCreatedAsync();

        if (!await _context.Assets.AnyAsync())
        {
            var assets = new List<Asset>
            {
                new() { CompanyId = 123, ContractId = 1001, Category = "Health" },
                new() { CompanyId = 456, ContractId = 1002, Category = "Glass" },
                new() { CompanyId = 678, ContractId = 1003, Category = "Health" },
                new() { CompanyId = 111, ContractId = 1004, Category = "Life" }
            };

            await _context.Assets.AddRangeAsync(assets);
            await _context.SaveChangesAsync();
        }
    }
} 