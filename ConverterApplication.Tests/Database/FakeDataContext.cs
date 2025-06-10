using ConverterApplication.Database.Dapper;

namespace ConverterApplication.Tests.Database;

public class FakeDataContext : IDataContext
{
    public Task Init()
    {
        return Task.CompletedTask;
    }
} 