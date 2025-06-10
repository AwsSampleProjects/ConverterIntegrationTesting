using FluentMigrator.Runner;

namespace ConverterApplication.Database.Dapper
{
    public class DataContext : IDataContext
    {
        private readonly IConfiguration _configuration;
        private readonly IMigrationRunner _migrationRunner;

        public DataContext(IConfiguration configuration, IMigrationRunner migrationRunner)
        {
            _configuration = configuration;
            _migrationRunner = migrationRunner;
        }

        public async Task Init()
        {
            _migrationRunner.MigrateUp();
        }
    }
} 