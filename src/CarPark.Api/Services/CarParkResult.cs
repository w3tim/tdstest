namespace CarPark.Api.Services;

public sealed class CarParkResult<T>
{
    private CarParkResult(
        T? value,
        CarParkFailure? failure,
        string? message)
    {
        Value = value;
        Failure = failure;
        Message = message;
    }

    public bool IsSuccess => Failure is null;

    public T? Value { get; }

    public CarParkFailure? Failure { get; }

    public string? Message { get; }

    public static CarParkResult<T> Success(T value)
    {
        return new CarParkResult<T>(value, null, null);
    }

    public static CarParkResult<T> Failed(
        CarParkFailure failure,
        string message)
    {
        return new CarParkResult<T>(default, failure, message);
    }
}