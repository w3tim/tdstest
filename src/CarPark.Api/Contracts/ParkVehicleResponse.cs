namespace CarPark.Api.Contracts;

public sealed class ParkVehicleResponse
{
    /// <summary>
    ///     Gets or sets the vehicle registration number.
    /// </summary>
    public required string VehicleReg { get; init; }

    /// <summary>
    ///     Gets or sets the allocated parking space number for the vehicle.
    /// </summary>
    public int SpaceNumber { get; init; }

    /// <summary>
    ///     Gets or sets the time when the vehicle entered the car park.
    /// </summary>
    public DateTime TimeIn { get; init; }
}