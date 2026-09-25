using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Bookmarks.DTOs;
using Shelfly.Api.Features.Bookmarks.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Bookmarks.Endpoints;

public static class GetBookmarkByIdEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookmarkService bookmarkService,
        HttpContext httpContext,
        Guid id,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Bookmarks.GetById");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
            activity?.SetTag("bookmarks.bookmark.id", id.ToString());
            activity?.SetTag("bookmarks.outcome", "unauthorized");
            activity?.SetStatus(ActivityStatusCode.Error, "User not authenticated");

            logger.LogWarning("Bookmark {BookmarkId} - No user ID found in JWT claims", id);

            return Json(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "User ID not found in authentication token",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            }, statusCode: 401);
        }

        BookmarkResponseDto? bookmark = await bookmarkService.GetBookmarkByIdAsync(userId.Value, id, cancellationToken);

        if (bookmark is null)
        {
            activity?.SetTag("bookmarks.user.id", userId.ToString());
            activity?.SetTag("bookmarks.bookmark.id", id.ToString());
            activity?.SetTag("bookmarks.outcome", "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Bookmark not found");

            logger.LogWarning("Bookmark {BookmarkId} not found for user {UserId}", id, userId);

            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = $"Bookmark '{id}' not found",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            });
        }

        activity?.SetTag("bookmarks.user.id", userId);
        activity?.SetTag("bookmarks.bookmark.id", id.ToString());
        activity?.SetTag("bookmarks.outcome", "success");
        activity?.SetStatus(ActivityStatusCode.Ok);

        logger.LogInformation("Retrieved bookmark {BookmarkId} for user {UserId}", id, userId);

        return Ok(bookmark);
    }
}
