using FluentValidation.Results;
using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Models;

namespace Shelfly.Api.Tests.Unit.Validators;

public class CreateBookmarkValidatorTests
{
    [Test]
    public void Validate_WithValidData_ReturnsValid()
    {
        // Arrange
        CreateBookmarkValidator validator = new();
        CreateBookmarkRequest request = new(1, 5, "Test note");

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validate_WithStartPageZero_ReturnsError()
    {
        // Arrange
        CreateBookmarkValidator validator = new();
        CreateBookmarkRequest request = new(0, 5, "Test note");

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "StartPage").ShouldBeTrue();
    }

    [Test]
    public void Validate_WithEndPageLessThanStartPage_ReturnsError()
    {
        // Arrange
        CreateBookmarkValidator validator = new();
        CreateBookmarkRequest request = new(10, 5, "Test note");

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "EndPage").ShouldBeTrue();
    }

    [Test]
    public void Validate_WithNullEndPage_ReturnsValid()
    {
        // Arrange
        CreateBookmarkValidator validator = new();
        CreateBookmarkRequest request = new(1, null, "Test note");

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }
}
