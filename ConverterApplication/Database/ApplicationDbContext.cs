using ConverterApplication.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ConverterApplication.Database;

public class ApplicationDbContext : DbContext
{
    public DbSet<Asset> Assets { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CompanyId).IsRequired();
            entity.Property(e => e.ContractId).IsRequired();
            entity.Property(e => e.Category).IsRequired();
        });
    }
} 