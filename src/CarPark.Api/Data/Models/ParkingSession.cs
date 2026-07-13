using System.ComponentModel.DataAnnotations;
using CarPark.Api.Domain;

namespace CarPark.Api.Data.Models;

public class ParkingSession
{
    /// <summary>
    ///     Gets or sets the unique identifier for the parking session.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     Gets or sets the vehicle registration number associated with the parking session.
    /// </summary>
    public required string VehicleReg { get; set; }

    /// <summary>
    ///     Gets or sets the time when the vehicle entered the parking space.
    /// </summary>
    public DateTimeOffset TimeIn { get; set; }

    /// <summary>
    ///     Gets or sets the type of vehicle associated with the parking session.
    /// </summary>
    public VehicleType VehicleType { get; set; }

    /// <summary>
    ///     Gets or sets the parking space number associated with the parking session.
    /// </summary>
    public int SpaceNumber { get; set; }

    /// <summary>
    ///     Gets or sets the time when the vehicle exited the parking space. This property is nullable to indicate that the
    ///     vehicle may not have exited yet.
    /// </summary>
    public DateTimeOffset? TimeOut { get; set; }

    /// <summary>
    ///     Gets or sets the charge for the parking session. This property is nullable to indicate that the charge may not have
    ///     been calculated yet.
    /// </summary>
    public decimal? VehicleCharge { get; set; }
}