using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Books.DTOs;
using Shelfly.Api.Features.Books.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Books.Endpoints;

public static class PatchBookEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookService bookService,
        HttpContext httpContext,
        Guid id,
        BookPatchDto request,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Books.Patch");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
            activity?.SetTag("books.book.id", id.ToString());
            activity?.SetTag("books.outcome", "unauthorized");
            activity?.SetStatus(ActivityStatusCode.Error, "User not authenticated");

            logger.LogWarning("Book {BookId} - No user ID found in JWT claims", id);

            return Json(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = "User ID not found in authentication token",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            }, statusCode: 401);
        }

        BookResponseDto? book = await bookService.PatchBookAsync(userId.Value, id, request, cancellationToken);

        if (book is null)
        {
            activity?.SetTag("books.user.id", userId.ToString());
            activity?.SetTag("books.book.id", id.ToString());
            activity?.SetTag("books.outcome", "not_found");
            activity?.SetStatus(ActivityStatusCode.Error, "Book not found");

            logger.LogWarning("Book {BookId} not found for user {UserId}", id, userId);

            return NotFound(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = $"Book '{id}' not found",
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            });
        }

        activity?.SetTag("books.user.id", userId);
        activity?.SetTag("books.book.id", id.ToString());
        activity?.SetTag("books.outcome", "success");
        activity?.SetStatus(ActivityStatusCode.Ok);

        logger.LogInformation("Patched book {BookId} for user {UserId}", id, userId);

        return Ok(book);
    }
}
