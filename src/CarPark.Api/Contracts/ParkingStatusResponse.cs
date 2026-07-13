namespace CarPark.Api.Contracts;

public sealed class ParkingStatusResponse
{
    /// <summary>
    ///     Gets or sets the number of available parking spaces in the car park.
    /// </summary>
    public int AvailableSpaces { get; init; }

    /// <summary>
    ///     Gets or sets the number of occupied parking spaces in the car park.
    /// </summary>
    public int OccupiedSpaces { get; init; }
}