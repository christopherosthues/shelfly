namespace Shelfly.Api.Books;

public static class BookEndpoints
{
    extension(WebApplication app) {
        public async Task MapBookEndpoints()
        {
            app.MapGet("/books", (HttpContext httpContext) =>
            {

            }).RequireAuthorization();

            app.MapGet("/books/{id}", (HttpContext httpContext) =>
            {

            }).RequireAuthorization();
        }
    }
}