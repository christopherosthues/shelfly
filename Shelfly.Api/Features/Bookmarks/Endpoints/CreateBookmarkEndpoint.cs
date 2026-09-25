using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Bookmarks.DTOs;
using Shelfly.Api.Features.Bookmarks.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Bookmarks.Endpoints;

public static class CreateBookmarkEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookmarkService bookmarkService,
        HttpContext httpContext,
        BookmarkCreateDto request,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Bookmarks.Create");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
            activity?.SetTag("bookmarks.book.id", request.BookId.ToString());
            activity?.SetTag("bookmarks.outcome", "unauthorized");
            activity?.SetStatus(ActivityStatusCode.Error, "User not authenticated");

            logger.LogWarning("Create bookmark - No user ID found in JWT claims");

            return Json(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "User ID not found in authentication token",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            }, statusCode: 401);
        }

        try
        {
            BookmarkResponseDto bookmark = await bookmarkService.CreateBookmarkAsync(userId.Value, request, cancellationToken);

            activity?.SetTag("bookmarks.user.id", userId.ToString());
            activity?.SetTag("bookmarks.bookmark.id", bookmark.Id.ToString());
            activity?.SetTag("bookmarks.book.id", bookmark.BookId.ToString());
            activity?.SetTag("bookmarks.outcome", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation("Created bookmark {BookmarkId} for book {BookId} by user {UserId}", bookmark.Id, bookmark.BookId, userId);

            return Created("", bookmark);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            activity?.SetTag("bookmarks.user.id", userId);
            activity?.SetTag("bookmarks.book.id", request.BookId.ToString());
            activity?.SetTag("bookmarks.outcome", "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            });
        }
    }
}
