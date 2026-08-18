namespace Shelfly.Common;

public class Result(bool isSuccess, string? error = null)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? Error { get; } = error;

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T>(T value) : Result(true)
{
    public T Value { get; } = value;

    public static Result<T> Success(T value) => new(value);
}
