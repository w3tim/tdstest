using CarPark.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarPark.Api.Controllers;

[ApiController]
[Route("parking")]
public class CarParkController : ControllerBase
{
    [HttpGet(Name = "GetParkingStatus")]
    public ParkingStatusResponse Get()
    {
        throw new NotImplementedException();
    }

    [HttpPost(Name = "ParkVehicle")]
    public ParkVehicleResponse ParkVehicle(ParkVehicleRequest request)
    {
        throw new NotImplementedException();
    }

    [HttpPost("exit", Name = "ExitVehicle")]
    public ExitVehicleResponse ExitVehicle(ExitVehicleRequest request)
    {
        throw new NotImplementedException();
    }
}