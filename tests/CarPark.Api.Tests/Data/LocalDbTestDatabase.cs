using CarPark.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Tests.Data;

internal sealed class LocalDbTestDatabase : IAsyncDisposable
{
    private readonly string _connectionString;

    public LocalDbTestDatabase()
    {
        var databaseName = $"CarParkAssessmentTests_{Guid.NewGuid():N}";

        _connectionString =
            $"""
             Server=(localdb)\MSSQLLocalDB;
             Database={databaseName};
             Trusted_Connection=True;
             TrustServerCertificate=True;
             Pooling=False
             """;
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
    }

    public CarParkDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CarParkDbContext>()
            .UseSqlServer(
                _connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsAssembly(
                    typeof(CarParkDbContext).Assembly.FullName))
            .Options;

        return new CarParkDbContext(options);
    }

    public async Task MigrateAsync()
    {
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }
}