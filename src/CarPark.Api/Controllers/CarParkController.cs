using CarPark.Api.Contracts;
using CarPark.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarPark.Api.Controllers;

[ApiController]
[Route("parking")]
public class CarParkController(ICarParkService carParkService) : ControllerBase
{
    [HttpGet(Name = "GetParkingStatus")]
    public async Task<ActionResult<ParkingStatusResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await carParkService.GetStatusAsync(cancellationToken);

        return Ok(response);
    }

    [HttpPost(Name = "ParkVehicle")]
    public async Task<ActionResult<ParkVehicleResponse>> ParkVehicle(ParkVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await carParkService.ParkVehicleAsync(
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [HttpPost("exit", Name = "ExitVehicle")]
    public async Task<ActionResult<ExitVehicleResponse>> ExitVehicle(ExitVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await carParkService.ExitVehicleAsync(
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    private ActionResult<T> ToActionResult<T>(CarParkResult<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);

        var error = new
        {
            result.Message
        };

        return result.Failure switch
        {
            CarParkFailure.InvalidVehicleRegistration => BadRequest(error),
            CarParkFailure.InvalidVehicleType => BadRequest(error),
            CarParkFailure.VehicleAlreadyParked => Conflict(error),
            CarParkFailure.CarParkFull => Conflict(error),
            CarParkFailure.VehicleNotParked => NotFound(error),

            _ => throw new InvalidOperationException(
                $"Unsupported car park failure: {result.Failure}")
        };
    }
}