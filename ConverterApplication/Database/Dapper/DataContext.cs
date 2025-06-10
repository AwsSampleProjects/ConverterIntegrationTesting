using ConverterApplication.Database.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConverterApplication.Database.Dapper
{
    public class DataContext : IDataContext
    {
        private readonly IConfiguration _configuration;

        public DataContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task Init()
        {
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            var tableExists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'Asset');");

            if (!tableExists)
            {
                await connection.ExecuteAsync(@"
                    CREATE TABLE ""Asset"" (
                        ""Id"" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                        ""CompanyId"" INTEGER NOT NULL,
                        ""ContractId"" INTEGER NOT NULL,
                        ""Category"" TEXT NOT NULL
                    );");

                var assets = new[]
                {
                    new { CompanyId = 123, ContractId = 1001, Category = "Health" },
                    new { CompanyId = 456, ContractId = 1002, Category = "Glass" },
                    new { CompanyId = 678, ContractId = 1003, Category = "Health" },
                    new { CompanyId = 111, ContractId = 1004, Category = "Life" }
                };

                await connection.ExecuteAsync(
                    @"INSERT INTO ""Asset"" (""CompanyId"", ""ContractId"", ""Category"") 
                      VALUES (@CompanyId, @ContractId, @Category);", 
                    assets);
            }
        }
    }
} 