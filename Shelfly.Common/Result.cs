namespace Shelfly.Common;

public class Result(bool isSuccess, string? error = null)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? Error { get; } = error;

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T>
{
    public T Value { get; }
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(T value, bool isSuccess, string? error = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, true);
    public static Result<T> Failure(string error) => new(default!, false, error);
}
