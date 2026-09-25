using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Books.DTOs;
using Shelfly.Api.Features.Books.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Books.Endpoints;

public static class ListBooksEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookService bookService,
        HttpContext httpContext,
        int page = 1,
        int pageSize = 20,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Books.List");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
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

        PagedBookListDto result = await bookService.GetBooksAsync(userId.Value, page, pageSize, cancellationToken);

        activity?.SetTag("books.user.id", userId.ToString());
        activity?.SetTag("books.page", page);
        activity?.SetTag("books.page_size", pageSize);
        activity?.SetTag("books.total_count", result.TotalCount);
        activity?.SetStatus(ActivityStatusCode.Ok);

        logger?.LogInformation("Listed {Count} books for user {UserId}", result.Items.Count(), userId);

        return Ok(result);
    }
}
