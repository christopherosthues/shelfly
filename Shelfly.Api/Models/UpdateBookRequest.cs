namespace Shelfly.Api.Models;

public record UpdateBookRequest(
    string Title,
    string Author,
    string ISBN,
    DateTime PublishDate);
