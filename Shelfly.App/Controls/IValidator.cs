using Shelfly.Common;

namespace Shelfly.App.Controls;

public interface IValidator
{
    Result<bool> Validate(object value, object? parameter = null);
}

public interface IValidator<in T> : IValidator
{
    Result<bool> Validate(T value, object? parameter = null);
}
