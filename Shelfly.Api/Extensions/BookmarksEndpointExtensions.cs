using Shelfly.Api.Features.Bookmarks.Endpoints;

namespace Shelfly.Api.Extensions;

public static class BookmarksEndpointExtensions
{
    extension(IEndpointRouteBuilder routes)
    {
        public IEndpointRouteBuilder MapBookmarksEndpoints()
        {
            RouteGroupBuilder group = routes.MapGroup("v1/bookmarks");

            group.MapGet("/", ListBookmarksEndpoint.Handle);
            group.MapPost("/", CreateBookmarkEndpoint.Handle);
            group.MapGet("/{id}", GetBookmarkByIdEndpoint.Handle);
            group.MapPut("/{id}", UpdateBookmarkEndpoint.Handle);
            group.MapPatch("/{id}", PatchBookmarkEndpoint.Handle);
            group.MapDelete("/{id}", DeleteBookmarkEndpoint.Handle);

            return routes;
        }
    }
}
