using Microsoft.EntityFrameworkCore;

namespace ConverterApplication.Database;

public class TestApplicationDbContext : ApplicationDbContext
{
    public QueryInterceptor QueryInterceptor { get; }

    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        QueryInterceptor = new QueryInterceptor();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(QueryInterceptor);
    }
} 