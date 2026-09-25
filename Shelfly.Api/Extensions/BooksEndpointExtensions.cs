using Shelfly.Api.Features.Books.Endpoints;

namespace Shelfly.Api.Extensions;

public static class BooksEndpointExtensions
{
    extension(IEndpointRouteBuilder routes)
    {
        public IEndpointRouteBuilder MapBooksEndpoints()
        {
            RouteGroupBuilder group = routes.MapGroup("v1/books");

            group.MapGet("/", ListBooksEndpoint.Handle);
            group.MapPost("/", CreateBookEndpoint.Handle);
            group.MapGet("/{id}", GetBookByIdEndpoint.Handle);
            group.MapPut("/{id}", UpdateBookEndpoint.Handle);
            group.MapPatch("/{id}", PatchBookEndpoint.Handle);
            group.MapDelete("/{id}", DeleteBookEndpoint.Handle);

            return routes;
        }
    }
}
