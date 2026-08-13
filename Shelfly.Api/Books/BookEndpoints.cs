namespace Shelfly.Api.Books;

public static class BookEndpoints
{
    extension(WebApplication app) {
        public async Task MapBookEndpoints()
        {
            app.MapGet("/books",
                    async (HttpContext httpContext, BookService bookService) =>
                    {
                        return await bookService.GetBooksAsync(Guid.CreateVersion7());
                    })
                .RequireAuthorization();

            app.MapGet("/books/{id}",
                async (Guid id, HttpContext httpContext, BookService bookService) =>
                {
                    return await bookService.GetBookAsync(Guid.CreateVersion7(), id);
                }).RequireAuthorization();
        }
    }
}