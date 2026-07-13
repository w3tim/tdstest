using CarPark.Api.Contracts;

namespace CarPark.Api.Services;

public interface ICarParkService
{
    /// <summary>
    ///     Attempts to park a vehicle in the car park. If successful, returns the space number and time of entry.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CarParkResult<ParkVehicleResponse>> ParkVehicleAsync(
        ParkVehicleRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves the current status of the car park.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ParkingStatusResponse> GetStatusAsync(
        CancellationToken cancellationToken);

    /// <summary>
    ///     Attempts to exit a vehicle from the car park. If successful, returns the vehicle registration, charge, and time of
    ///     exit.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<CarParkResult<ExitVehicleResponse>> ExitVehicleAsync(
        ExitVehicleRequest request,
        CancellationToken cancellationToken);
}