namespace CarPark.Api.Contracts;

public sealed class ExitVehicleResponse
{
    /// <summary>
    ///     Gets or sets the vehicle registration number of the vehicle that has exited the car park.
    /// </summary>
    public required string VehicleReg { get; init; }

    /// <summary>
    ///     Gets or sets the total charge for the vehicle's stay in the car park.
    /// </summary>
    public double VehicleCharge { get; init; }

    /// <summary>
    ///     Gets or sets the time when the vehicle entered the car park.
    /// </summary>
    public DateTime TimeIn { get; init; }

    /// <summary>
    ///     Gets or sets the time when the vehicle exited the car park.
    /// </summary>
    public DateTime TimeOut { get; init; }
}