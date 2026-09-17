namespace Shelfly.App.Features.Settings.Services;

public class ApiResult<TValue>
{
    public bool IsSuccess { get; init; }
    public TValue? Value { get; init; }
    public string? ErrorMessage { get; init; }

    public static ApiResult<TValue> Success(TValue value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static ApiResult<TValue> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}