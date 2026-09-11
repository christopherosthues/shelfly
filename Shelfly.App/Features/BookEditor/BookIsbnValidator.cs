using Shelfly.App.Controls;
using Shelfly.App.Features.Library;
using Shelfly.App.Resources.Localization;
using Shelfly.Common;

namespace Shelfly.App.Features.BookEditor;

public class BookIsbnValidator : IValidator<string>
{
    // XAML instantiates this resource directly (no DI), so it needs a parameterless
    // constructor. The service provider is injected afterwards by the page once its
    // Handler is attached (see BookEditPage.xaml.cs, Handler.MauiContext.Services).
    public IServiceProvider? ServiceProvider { get; set; }

    public Result<bool> Validate(string value, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<bool>.Failure(AppResources.BookEditPageIsbnEmptyError);
        }

        if (!IsbnValidator.IsValid(value))
        {
            return Result<bool>.Failure(AppResources.BookEditPageIsbnFormatError);
        }

        if (ServiceProvider is null)
        {
            return Result<bool>.Success(true);
        }

        if (parameter is not Guid bookId)
        {
            return Result<bool>.Failure(AppResources.BookEditPageIsbnFormatError);
        }

        using IServiceScope scope = ServiceProvider.CreateScope();
        LibraryService libraryService = scope.ServiceProvider.GetRequiredService<LibraryService>();
        Result<bool> result = libraryService.IsbnExistsAsync(value, bookId).Result;
        if (result is { IsSuccess: true, Value: true })
        {
            return Result<bool>.Failure(AppResources.BookEditPageIsbnDuplicateError);
        }

        return Result<bool>.Success(true);
    }

    public Result<bool> Validate(object value, object? parameter = null)
    {
        if (value is not string text)
        {
            return Result<bool>.Failure(AppResources.BookEditPagePublisherEmptyError);
        }

        return Validate(text, parameter);
    }
}
