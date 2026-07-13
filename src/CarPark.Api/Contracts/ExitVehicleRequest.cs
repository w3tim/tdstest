using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Contracts;

public sealed class ExitVehicleRequest
{
    /// <summary>
    ///     Gets or sets the vehicle registration number of the vehicle to be exited from the car park.
    /// </summary>
    [Required]
    [MaxLength(20)]
    public required string VehicleReg { get; init; }
}