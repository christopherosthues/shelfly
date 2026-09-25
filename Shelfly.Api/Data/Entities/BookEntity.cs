using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shelfly.Api.Data.Entities;

[Index(nameof(UserId), nameof(ISBN), IsUnique = true)]
[Index(nameof(UserId), nameof(Id))]
[Index(nameof(UserId))]
[Index(nameof(Title), nameof(Author), nameof(Publisher))]
public class BookEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Author { get; set; } = null!;

    [Required]
    [MaxLength(13)]
    public string ISBN { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string Publisher { get; set; } = null!;

    public DateTime? PublishDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    [InverseProperty(nameof(BookmarkEntity.Book))]
    public ICollection<BookmarkEntity> Bookmarks { get; set; } = [];
}
