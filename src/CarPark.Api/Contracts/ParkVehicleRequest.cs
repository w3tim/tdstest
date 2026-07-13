using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Contracts;

public sealed class ParkVehicleRequest
{
    /// <summary>
    ///     Gets or sets the vehicle registration number.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string VehicleReg { get; init; }

    /// <summary>
    ///     Gets or sets the type of vehicle being parked.
    /// </summary>
    [Range(1, 3)]
    public int VehicleType { get; init; }
}