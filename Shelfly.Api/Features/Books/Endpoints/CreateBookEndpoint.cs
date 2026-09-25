using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shelfly.Api.Extensions;
using Shelfly.Api.Features.Books.DTOs;
using Shelfly.Api.Features.Books.Services;
using static Microsoft.AspNetCore.Http.Results;

namespace Shelfly.Api.Features.Books.Endpoints;

public static class CreateBookEndpoint
{
    private static readonly ActivitySource ActivitySource = new("shelfly-api");

    public static async Task<IResult> Handle(
        IBookService bookService,
        HttpContext httpContext,
        BookCreateDto request,
        ILogger logger = null!,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("Books.Create");

        Guid? userId = httpContext.GetUserId();

        if (userId is null)
        {
            activity?.SetTag("books.isbn", request.ISBN);
            activity?.SetTag("books.outcome", "unauthorized");
            activity?.SetStatus(ActivityStatusCode.Error, "User not authenticated");

            logger.LogWarning("Create book - No user ID found in JWT claims");

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
            BookResponseDto book = await bookService.CreateBookAsync(userId.Value, request, cancellationToken);

            activity?.SetTag("books.user.id", userId.ToString());
            activity?.SetTag("books.book.id", book.Id.ToString());
            activity?.SetTag("books.isbn", book.ISBN);
            activity?.SetTag("books.outcome", "success");
            activity?.SetStatus(ActivityStatusCode.Ok);

            logger.LogInformation("Created book {BookId} with ISBN {ISBN} for user {UserId}", book.Id, book.ISBN, userId);

            return Created("", book);
        }
        catch (Exception ex) when (ex.Message.Contains("already exists"))
        {
            activity?.SetTag("books.user.id", userId);
            activity?.SetTag("books.isbn", request.ISBN);
            activity?.SetTag("books.outcome", "conflict");
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            return Conflict(new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = ex.Message,
                Type = "https://tools.ietf.org/html/rfc7807#section-2.1"
            });
        }
    }
}
