// using System.Security.Claims;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Logging;
// using NSubstitute;
// using Shelfly.Api.Authentication;
//
// namespace Shelfly.Api.Tests.Integration;
// TODO
// public class AudienceValidatorTest
// {
//     [Test]
//     public void AudienceMismatch_Returns401Unauthorized()
//     {
//         // Arrange
//         JwtAudienceValidator validator = new("shelfly-api", Substitute.For<ILogger<JwtAudienceValidator>>());
//         AuthenticationTicket ticket = new AuthenticationTicket(
//             new ClaimsPrincipal(),
//             "scheme",
//             new[] { new Claim(ClaimTypes.Name, "test-user") });
//
//         // Act - simulate audience mismatch
//         Task<(bool IsValid, string? ErrorMessage)> result = validator.ValidateAsync(ticket);
//
//         // Assert
//         result.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
//     }
//
//     [Test]
//     public void AudienceMatch_Returns200Ok()
//     {
//         // Arrange
//         JwtAudienceValidator validator = new("shelfly-api", Substitute.For<ILogger<JwtAudienceValidator>>());
//         AuthenticationTicket ticket = new AuthenticationTicket(
//             new ClaimsPrincipal(),
//             "scheme",
//             new[] { new Claim(ClaimTypes.Name, "test-user") });
//
//         // Act - simulate audience match
//         Task<(bool IsValid, string? ErrorMessage)> result = validator.ValidateAsync(ticket);
//
//         // Assert
//         result.StatusCode.ShouldBe(StatusCodes.Status200OK);
//     }
// }
