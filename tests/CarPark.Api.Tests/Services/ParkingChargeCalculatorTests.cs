using CarPark.Api.Domain;
using CarPark.Api.Services;
using Xunit;

namespace CarPark.Api.Tests.Services;

public class ParkingChargeCalculatorTests
{
    private static readonly DateTimeOffset TimeIn =
        new(2026, 7, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly ParkingChargeCalculator _calculator = new();

    public static TheoryData<VehicleType, TimeSpan, decimal> ChargeCases =>
        new()
        {
            {
                VehicleType.SmallCar,
                TimeSpan.FromSeconds(30),
                0.10m
            },
            {
                VehicleType.MediumCar,
                TimeSpan.FromMinutes(1),
                0.20m
            },
            {
                VehicleType.MediumCar,
                TimeSpan.FromMinutes(4),
                0.80m
            },
            {
                VehicleType.MediumCar,
                TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(1),
                2.00m
            },
            {
                VehicleType.MediumCar,
                TimeSpan.FromMinutes(5),
                2.00m
            },
            {
                VehicleType.MediumCar,
                TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1),
                2.20m
            },
            {
                VehicleType.LargeCar,
                TimeSpan.FromMinutes(10),
                6.00m
            }
        };

    [Theory]
    [MemberData(nameof(ChargeCases))]
    public void Calculate_ReturnsExpectedCharge(
        VehicleType vehicleType,
        TimeSpan duration,
        decimal expectedCharge)
    {
        var result = _calculator.Calculate(
            vehicleType,
            TimeIn,
            TimeIn.Add(duration));

        Assert.Equal(expectedCharge, result);
    }

    [Fact]
    public void Calculate_WhenTimeOutIsBeforeTimeIn_ThrowsArgumentOutOfRangeException()
    {
        var timeOut = TimeIn.AddMinutes(-1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.Calculate(
                VehicleType.SmallCar,
                TimeIn,
                timeOut));
    }

    [Fact]
    public void Calculate_WhenVehicleTypeIsUnsupported_ThrowsArgumentOutOfRangeException()
    {
        var unsupportedVehicleType = (VehicleType)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _calculator.Calculate(
                unsupportedVehicleType,
                TimeIn,
                TimeIn.AddMinutes(1)));
    }
}