using CarPark.Api.Domain;

namespace CarPark.Api.Services;

public class ParkingChargeCalculator : IParkingChargeCalculator
{
    /// <inheritdoc/>
    public decimal Calculate(
        VehicleType vehicleType,
        DateTimeOffset timeIn,
        DateTimeOffset timeOut)
    {
        if (timeOut < timeIn)
            throw new ArgumentOutOfRangeException(
                nameof(timeOut),
                timeOut,
                "Time out cannot be earlier than time in.");

        var rate = vehicleType switch
        {
            VehicleType.SmallCar => 0.10m,
            VehicleType.MediumCar => 0.20m,
            VehicleType.LargeCar => 0.40m,
            _ => throw new ArgumentOutOfRangeException(nameof(vehicleType), vehicleType, "Unsupported vehicle type")
        };

        var elapsedMinutes = (decimal)(timeOut - timeIn).TotalMinutes;
        var billableMinutes = Math.Max(1m, Math.Ceiling(elapsedMinutes));
        var baseCharge = billableMinutes * rate;
        var surcharge = Math.Floor(billableMinutes / 5m);

        var total = baseCharge + surcharge;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}