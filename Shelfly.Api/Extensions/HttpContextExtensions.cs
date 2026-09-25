namespace Shelfly.Api.Extensions;

public static class HttpContextExtensions
{
    public static Guid? GetUserId(this HttpContext httpContext) =>
        httpContext.User.FindFirst("sub")?.Value is { } sub && Guid.TryParse(sub, out var userId) ? userId : default;
}
