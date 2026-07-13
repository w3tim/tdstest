using CarPark.Api.Domain;

namespace CarPark.Api.Services;

public interface IParkingChargeCalculator
{
    /// <summary>
    ///     Calculates the parking charge based on the vehicle type and the time spent in the car park.
    /// </summary>
    /// <param name="vehicleType"></param>
    /// <param name="timeIn"></param>
    /// <param name="timeOut"></param>
    /// <returns></returns>
    public decimal Calculate(
        VehicleType vehicleType,
        DateTimeOffset timeIn,
        DateTimeOffset timeOut);
}