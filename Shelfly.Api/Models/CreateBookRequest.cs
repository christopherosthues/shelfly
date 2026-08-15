namespace Shelfly.Api.Models;

public record CreateBookRequest(
    string Title,
    string Author,
    string ISBN,
    DateTime PublishDate);
