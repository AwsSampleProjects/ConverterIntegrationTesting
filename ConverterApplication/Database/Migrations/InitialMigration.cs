using FluentMigrator;

namespace ConverterApplication.Database.Migrations;

[Migration(1)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");

        Create.Table("Asset")
            .WithColumn("Id").AsGuid().PrimaryKey().WithDefault(SystemMethods.NewGuid)
            .WithColumn("CompanyId").AsInt32().NotNullable()
            .WithColumn("ContractId").AsInt32().NotNullable()
            .WithColumn("Category").AsString().NotNullable();

        var assets = new[]
        {
            new { CompanyId = 123, ContractId = 1001, Category = "Health" },
            new { CompanyId = 456, ContractId = 1002, Category = "Glass" },
            new { CompanyId = 678, ContractId = 1003, Category = "Health" },
            new { CompanyId = 111, ContractId = 1004, Category = "Life" }
        };

        foreach (var asset in assets)
        {
            Insert.IntoTable("Asset")
                .Row(new
                {
                    CompanyId = asset.CompanyId,
                    ContractId = asset.ContractId,
                    Category = asset.Category
                });
        }
    }

    public override void Down()
    {
        Delete.Table("Asset");
        Execute.Sql("DROP EXTENSION IF EXISTS \"uuid-ossp\";");
    }
} 