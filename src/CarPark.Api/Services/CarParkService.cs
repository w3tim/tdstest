using CarPark.Api.Contracts;
using CarPark.Api.Data;
using CarPark.Api.Data.Models;
using CarPark.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CarPark.Api.Services;

public class CarParkService(CarParkDbContext dbContext, IParkingChargeCalculator calculator, TimeProvider timeProvider)
    : ICarParkService
{
    /// <inheritdoc />
    public async Task<CarParkResult<ParkVehicleResponse>> ParkVehicleAsync(ParkVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var reg = NormaliseReg(request.VehicleReg);

        if (string.IsNullOrWhiteSpace(reg) || reg.Length > 20)
            return CarParkResult<ParkVehicleResponse>.Failed(CarParkFailure.InvalidVehicleRegistration,
                "A valid vehicle registration must be provided.");

        if (!Enum.IsDefined(typeof(VehicleType), request.VehicleType))
            return CarParkResult<ParkVehicleResponse>.Failed(
                CarParkFailure.InvalidVehicleType,
                "Vehicle type must be 1, 2 or 3.");

        var isParked = await dbContext.ParkingSessions.AnyAsync(
            session =>
                session.VehicleReg == reg &&
                session.TimeOut == null,
            cancellationToken);

        if (isParked)
            return CarParkResult<ParkVehicleResponse>.Failed(
                CarParkFailure.VehicleAlreadyParked,
                "The vehicle is already parked.");

        var spaceNumber = await dbContext.ParkingSpaces
            .Where(space => !dbContext.ParkingSessions.Any(session =>
                session.SpaceNumber == space.SpaceNumber &&
                session.TimeOut == null))
            .OrderBy(space => space.SpaceNumber)
            .Select(space => (int?)space.SpaceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (spaceNumber is null)
            return CarParkResult<ParkVehicleResponse>.Failed(
                CarParkFailure.CarParkFull,
                "The car park is full.");

        var session = new ParkingSession
        {
            VehicleReg = reg,
            VehicleType = (VehicleType)request.VehicleType,
            SpaceNumber = spaceNumber.Value,
            TimeIn = timeProvider.GetUtcNow()
        };

        dbContext.ParkingSessions.Add(session);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CarParkResult<ParkVehicleResponse>.Success(new ParkVehicleResponse
        {
            SpaceNumber = session.SpaceNumber,
            VehicleReg = session.VehicleReg,
            TimeIn = session.TimeIn.UtcDateTime
        });
    }

    /// <inheritdoc />
    public async Task<ParkingStatusResponse> GetStatusAsync(CancellationToken cancellationToken)
    {
        var totalSpaces = await dbContext.ParkingSpaces.CountAsync(cancellationToken);
        var occupiedSpaces = await dbContext.ParkingSessions
            .Where(session => session.TimeOut == null)
            .Select(session => session.SpaceNumber)
            .Distinct()
            .CountAsync(cancellationToken);
        var availableSpaces = totalSpaces - occupiedSpaces;

        return new ParkingStatusResponse
        {
            AvailableSpaces = availableSpaces,
            OccupiedSpaces = occupiedSpaces
        };
    }

    /// <inheritdoc />
    public async Task<CarParkResult<ExitVehicleResponse>> ExitVehicleAsync(ExitVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var reg = NormaliseReg(request.VehicleReg);

        if (string.IsNullOrEmpty(reg) || reg.Length > 20)
            return CarParkResult<ExitVehicleResponse>.Failed(
                CarParkFailure.InvalidVehicleRegistration,
                "Valid vehicle registration is required.");

        var session = await dbContext.ParkingSessions
            .SingleOrDefaultAsync(
                parkingSession =>
                    parkingSession.VehicleReg == reg &&
                    parkingSession.TimeOut == null,
                cancellationToken);

        if (session is null)
            return CarParkResult<ExitVehicleResponse>.Failed(
                CarParkFailure.VehicleNotParked,
                "The vehicle is not currently parked.");

        var timeOut = timeProvider.GetUtcNow();
        var charge = calculator.Calculate(
            session.VehicleType,
            session.TimeIn,
            timeOut);

        session.TimeOut = timeOut;
        session.VehicleCharge = charge;

        await dbContext.SaveChangesAsync(cancellationToken);

        return CarParkResult<ExitVehicleResponse>.Success(new ExitVehicleResponse
        {
            VehicleReg = session.VehicleReg,
            VehicleCharge = decimal.ToDouble(charge),
            TimeIn = session.TimeIn.UtcDateTime,
            TimeOut = timeOut.UtcDateTime
        });
    }

    private static string NormaliseReg(string reg)
    {
        return string.IsNullOrWhiteSpace(reg) ? 
            string.Empty : 
            string.Concat(reg.Where(c => !char.IsWhiteSpace(c))).ToUpperInvariant();
    }
}