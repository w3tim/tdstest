using System.ComponentModel.DataAnnotations;

namespace CarPark.Api.Data.Models;

public class ParkingSpace
{
    /// <summary>
    ///     Gets or sets the unique identifier for the parking space.
    /// </summary>
    public int SpaceNumber { get; set; }

    /// <summary>
    ///     Gets or sets the collection of parking sessions associated with this parking space.
    /// </summary>
    public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
}