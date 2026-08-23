using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Shelfly.App.Data.Entities;

[Index(nameof(Title), nameof(Author), nameof(Publisher))]
[Index(nameof(ISBN), IsUnique = true)]
public class BookEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Author { get; set; } = string.Empty;

    [Required]
    public string ISBN { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string Publisher { get; set; } = string.Empty;

    public DateTime? PublishDate { get; set; }

    public DateTime? DeletedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }
}
