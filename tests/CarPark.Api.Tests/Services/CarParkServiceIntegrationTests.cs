using CarPark.Api.Contracts;
using CarPark.Api.Data;
using CarPark.Api.Data.Models;
using CarPark.Api.Domain;
using CarPark.Api.Services;
using CarPark.Api.Tests.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarPark.Api.Tests.Services;

public class CarParkServiceIntegrationTests
{
    private static readonly DateTimeOffset InitialTime =
        new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ParkVehicle_AllocatesLowestAvailableSpacesAndNormalisesRegistration()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var timeProvider = new TestTimeProvider(InitialTime);
        var service = CreateService(context, timeProvider);

        var firstResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = " ab12 cde ",
                VehicleType = (int)VehicleType.SmallCar
            },
            TestContext.Current.CancellationToken);

        var secondResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = "xy99 zzz",
                VehicleType = (int)VehicleType.MediumCar
            },
            TestContext.Current.CancellationToken);

        Assert.True(firstResult.IsSuccess);
        Assert.True(secondResult.IsSuccess);

        var firstResponse =
            Assert.IsType<ParkVehicleResponse>(firstResult.Value);

        var secondResponse =
            Assert.IsType<ParkVehicleResponse>(secondResult.Value);

        Assert.Equal("AB12CDE", firstResponse.VehicleReg);
        Assert.Equal(1, firstResponse.SpaceNumber);
        Assert.Equal(InitialTime.UtcDateTime, firstResponse.TimeIn);
        Assert.Equal(DateTimeKind.Utc, firstResponse.TimeIn.Kind);

        Assert.Equal("XY99ZZZ", secondResponse.VehicleReg);
        Assert.Equal(2, secondResponse.SpaceNumber);

        var sessions = await context.ParkingSessions
            .AsNoTracking()
            .OrderBy(session => session.SpaceNumber)
            .ToListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, sessions.Count);
        Assert.Equal("AB12CDE", sessions[0].VehicleReg);
        Assert.Equal("XY99ZZZ", sessions[1].VehicleReg);
    }

    [Fact]
    public async Task ParkVehicle_WhenEquivalentRegistrationIsAlreadyActive_ReturnsVehicleAlreadyParked()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var service = CreateService(
            context,
            new TestTimeProvider(InitialTime));

        var firstResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = "ab12 cde",
                VehicleType = (int)VehicleType.SmallCar
            },
            TestContext.Current.CancellationToken);

        var duplicateResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = " AB12CDE ",
                VehicleType = (int)VehicleType.LargeCar
            },
            TestContext.Current.CancellationToken);

        Assert.True(firstResult.IsSuccess);

        Assert.False(duplicateResult.IsSuccess);
        Assert.Equal(
            CarParkFailure.VehicleAlreadyParked,
            duplicateResult.Failure);
        Assert.Null(duplicateResult.Value);

        var activeSessionCount = await context.ParkingSessions
            .CountAsync(
                session => session.TimeOut == null,
                TestContext.Current.CancellationToken);

        Assert.Equal(1, activeSessionCount);
    }

    [Fact]
    public async Task ParkVehicle_WhenRequestIsInvalid_ReturnsValidationFailures()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var service = CreateService(
            context,
            new TestTimeProvider(InitialTime));

        var invalidRegistrationResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = "    ",
                VehicleType = (int)VehicleType.SmallCar
            },
            TestContext.Current.CancellationToken);

        var invalidVehicleTypeResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = "AB12CDE",
                VehicleType = 999
            },
            TestContext.Current.CancellationToken);

        Assert.False(invalidRegistrationResult.IsSuccess);
        Assert.Equal(
            CarParkFailure.InvalidVehicleRegistration,
            invalidRegistrationResult.Failure);

        Assert.False(invalidVehicleTypeResult.IsSuccess);
        Assert.Equal(
            CarParkFailure.InvalidVehicleType,
            invalidVehicleTypeResult.Failure);

        Assert.Empty(context.ParkingSessions);
    }

    [Fact]
    public async Task ParkVehicle_WhenAllSpacesAreOccupied_ReturnsCarParkFull()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        context.ParkingSessions.AddRange(
            Enumerable.Range(1, 10)
                .Select(spaceNumber => new ParkingSession
                {
                    VehicleReg = $"REG{spaceNumber:00}",
                    VehicleType = VehicleType.SmallCar,
                    SpaceNumber = spaceNumber,
                    TimeIn = InitialTime.AddMinutes(-spaceNumber)
                }));

        await context.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var service = CreateService(
            context,
            new TestTimeProvider(InitialTime));

        var result = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = "NEW123",
                VehicleType = (int)VehicleType.SmallCar
            },
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(CarParkFailure.CarParkFull, result.Failure);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task GetStatus_CountsOnlyActiveParkingSessions()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        context.ParkingSessions.AddRange(
            new ParkingSession
            {
                VehicleReg = "ACTIVE1",
                VehicleType = VehicleType.SmallCar,
                SpaceNumber = 1,
                TimeIn = InitialTime.AddMinutes(-10)
            },
            new ParkingSession
            {
                VehicleReg = "COMPLETE1",
                VehicleType = VehicleType.MediumCar,
                SpaceNumber = 2,
                TimeIn = InitialTime.AddHours(-1),
                TimeOut = InitialTime.AddMinutes(-30),
                VehicleCharge = 7.00m
            });

        await context.SaveChangesAsync(
            TestContext.Current.CancellationToken);

        var service = CreateService(
            context,
            new TestTimeProvider(InitialTime));

        var result = await service.GetStatusAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(9, result.AvailableSpaces);
        Assert.Equal(1, result.OccupiedSpaces);
    }

    [Fact]
    public async Task ExitVehicle_CompletesSessionCalculatesChargeAndReleasesSpace()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var timeProvider = new TestTimeProvider(InitialTime);
        var service = CreateService(context, timeProvider);

        var parkResult = await service.ParkVehicleAsync(
            new ParkVehicleRequest
            {
                VehicleReg = " ab12 cde ",
                VehicleType = (int)VehicleType.MediumCar
            },
            TestContext.Current.CancellationToken);

        Assert.True(parkResult.IsSuccess);

        timeProvider.UtcNow = InitialTime.AddMinutes(5);

        var exitResult = await service.ExitVehicleAsync(
            new ExitVehicleRequest
            {
                VehicleReg = "AB12 CDE"
            },
            TestContext.Current.CancellationToken);

        Assert.True(exitResult.IsSuccess);

        var response =
            Assert.IsType<ExitVehicleResponse>(exitResult.Value);

        Assert.Equal("AB12CDE", response.VehicleReg);
        Assert.Equal(2.00d, response.VehicleCharge);
        Assert.Equal(InitialTime.UtcDateTime, response.TimeIn);
        Assert.Equal(
            timeProvider.UtcNow.UtcDateTime,
            response.TimeOut);
        Assert.Equal(DateTimeKind.Utc, response.TimeIn.Kind);
        Assert.Equal(DateTimeKind.Utc, response.TimeOut.Kind);

        var persistedSession = await context.ParkingSessions
            .AsNoTracking()
            .SingleAsync(
                TestContext.Current.CancellationToken);

        Assert.Equal(timeProvider.UtcNow, persistedSession.TimeOut);
        Assert.Equal(2.00m, persistedSession.VehicleCharge);

        var status = await service.GetStatusAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(10, status.AvailableSpaces);
        Assert.Equal(0, status.OccupiedSpaces);
    }

    [Fact]
    public async Task ExitVehicle_WhenVehicleIsNotActive_ReturnsVehicleNotParked()
    {
        await using var database = new LocalDbTestDatabase();
        await database.MigrateAsync();

        await using var context = database.CreateContext();

        var service = CreateService(
            context,
            new TestTimeProvider(InitialTime));

        var result = await service.ExitVehicleAsync(
            new ExitVehicleRequest
            {
                VehicleReg = "AB12CDE"
            },
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            CarParkFailure.VehicleNotParked,
            result.Failure);
        Assert.Null(result.Value);
    }

    private static CarParkService CreateService(
        CarParkDbContext context,
        TimeProvider timeProvider)
    {
        return new CarParkService(
            context,
            new ParkingChargeCalculator(),
            timeProvider);
    }
}