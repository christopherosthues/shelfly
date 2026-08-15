using FluentValidation.Results;
using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Models;

namespace Shelfly.Api.Tests.Unit.Validators;

public class CreateBookValidatorTests
{
    [Test]
    public void Validate_WithTitle_ReturnsValid()
    {
        // Arrange
        CreateBookValidator validator = new();
        CreateBookRequest request = new("Test Title", "Author", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validate_WithEmptyTitle_ReturnsError()
    {
        // Arrange
        CreateBookValidator validator = new();
        CreateBookRequest request = new("", "Author", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "Title").ShouldBeTrue();
    }

    [Test]
    public void Validate_WithTitleExceeding256Chars_ReturnsError()
    {
        // Arrange
        CreateBookValidator validator = new();
        string longTitle = new('a', 257);
        CreateBookRequest request = new(longTitle, "Author", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "Title").ShouldBeTrue();
    }

    [Test]
    public void Validate_WithISBNExceeding16Chars_ReturnsError()
    {
        // Arrange
        CreateBookValidator validator = new();
        string longIsbn = new('1', 17);
        CreateBookRequest request = new("Title", "Author", longIsbn, new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "ISBN").ShouldBeTrue();
    }
}
