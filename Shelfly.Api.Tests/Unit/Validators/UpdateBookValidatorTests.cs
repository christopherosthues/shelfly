using FluentValidation.Results;
using Shelfly.Api.Authentication.Validators;
using Shelfly.Api.Models;

namespace Shelfly.Api.Tests.Unit.Validators;

public class UpdateBookValidatorTests
{
    [Test]
    public void Validate_WithValidData_ReturnsValid()
    {
        // Arrange
        UpdateBookValidator validator = new();
        UpdateBookRequest request = new("Updated Title", "Author", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validate_WithEmptyTitle_ReturnsError()
    {
        // Arrange
        UpdateBookValidator validator = new();
        UpdateBookRequest request = new("", "Author", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        ValidationResult result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Any(e => e.PropertyName == "Title").ShouldBeTrue();
    }
}
