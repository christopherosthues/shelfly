using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Bookmarks.DTOs;
using Shelfly.Api.Features.Bookmarks.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Bookmarks.Endpoints;

public static class ListBookmarksEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookmarkService bookmarkService,
        HttpContext httpContext,
        Guid? bookId = null,
        int page = 1,
        int pageSize = 20,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Bookmarks.List");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
            activity?.SetTag("bookmarks.book.id", bookId?.ToString());
            activity?.SetTag("bookmarks.outcome", "unauthorized");
            activity?.SetStatus(ActivityStatusCode.Error, "User not authenticated");

            logger?.LogWarning("No user ID found in JWT claims");

            return Json(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "User ID not found in authentication token",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            }, statusCode: 401);
        }

        PagedBookmarkListDto result = await bookmarkService.GetBookmarksAsync(userId.Value, bookId, page, pageSize, cancellationToken);

        activity?.SetTag("bookmarks.user.id", userId.ToString());
        activity?.SetTag("bookmarks.book.id", bookId?.ToString());
        activity?.SetTag("bookmarks.page", page);
        activity?.SetTag("bookmarks.page_size", pageSize);
        activity?.SetTag("bookmarks.total_count", result.TotalCount);
        activity?.SetStatus(ActivityStatusCode.Ok);

        logger?.LogInformation("Listed {Count} bookmarks for user {UserId}", result.Items.Count(), userId);

        return Ok(result);
    }
}
