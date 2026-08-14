using Microsoft.AspNetCore.Authentication;
using FluentValidation;

namespace Shelfly.Api.Authentication;

public class JwtAudienceValidator(string expectedAudience, ILogger<JwtAudienceValidator> logger)
{
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(AuthenticationTicket ticket)
    {
        string? audienceClaim = ticket.Principal.FindFirst("aud")?.Value;

        if (audienceClaim != expectedAudience)
        {
            logger.LogWarning(
                "JWT audience mismatch: expected '{Expected}', received '{Actual}'",
                expectedAudience,
                audienceClaim);

            return (false, "401");
        }

        logger.LogInformation(
            "JWT audience validated successfully: '{Audience}'",
            audienceClaim);

        return (true, null);
    }

    public void ValidateClaims(AuthenticationTicket ticket)
    {
        // TODO: use it?
        string? subClaim = ticket.Principal.FindFirst("sub")?.Value;
        string? audClaim = ticket.Principal.FindFirst("aud")?.Value;
        string? issClaim = ticket.Principal.FindFirst("iss")?.Value;

        if (string.IsNullOrEmpty(subClaim))
        {
            logger.LogWarning("JWT sub claim is required");
            throw new ValidationException("JWT sub claim is required");
        }

        if (string.IsNullOrEmpty(audClaim))
        {
            logger.LogWarning("JWT aud claim is required");
            throw new ValidationException("JWT aud claim is required");
        }

        if (string.IsNullOrEmpty(issClaim))
        {
            logger.LogWarning("JWT iss claim is required");
            throw new ValidationException("JWT iss claim is required");
        }

        logger.LogInformation("JWT claims validated successfully");
    }
}
