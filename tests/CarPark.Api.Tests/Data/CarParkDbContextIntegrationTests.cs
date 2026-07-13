using CarPark.Api.Data.Models;
using CarPark.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarPark.Api.Tests.Data;

public class CarParkDbContextIntegrationTests
{
    [Fact]
    public async Task Migrate_CreatesTenNumberedParkingSpaces()
    {
        await using var database = new LocalDbTestDatabase();

        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var spaceNumbers = await context.ParkingSpaces
            .AsNoTracking()
            .OrderBy(space => space.SpaceNumber)
            .Select(space => space.SpaceNumber)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(Enumerable.Range(1, 10), spaceNumbers);
    }

    [Fact]
    public async Task Migrate_WhenDatabaseIsCurrent_DoesNotDuplicateParkingSpaces()
    {
        await using var database = new LocalDbTestDatabase();

        await database.MigrateAsync();

        // Simulate the application starting again against the same database.
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var spaceNumbers = await context.ParkingSpaces
            .AsNoTracking()
            .OrderBy(space => space.SpaceNumber)
            .Select(space => space.SpaceNumber)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(10, spaceNumbers.Count);
        Assert.Equal(Enumerable.Range(1, 10), spaceNumbers);
    }

    [Fact]
    public async Task SaveChanges_WhenParkingSpaceDoesNotExist_ThrowsDbUpdateException()
    {
        await using var database = new LocalDbTestDatabase();

        await database.MigrateAsync();

        await using var context = database.CreateContext();

        context.ParkingSessions.Add(new ParkingSession
        {
            VehicleReg = "AB12CDE",
            VehicleType = VehicleType.SmallCar,
            SpaceNumber = 999,
            TimeIn = DateTimeOffset.UtcNow
        });

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync(TestContext.Current.CancellationToken));
    }
}