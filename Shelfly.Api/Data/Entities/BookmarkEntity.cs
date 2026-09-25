using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shelfly.Api.Data.Entities;

[Index(nameof(UserId))]
[Index(nameof(BookId))]
[Index(nameof(UserId), nameof(BookId))]
[Index(nameof(Id), nameof(UserId))]
[Index(nameof(BookId), nameof(StartPage))]
public class BookmarkEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Range(1, int.MaxValue)]
    public int StartPage { get; set; }

    public int? EndPage { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [ForeignKey(nameof(BookId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public BookEntity? Book { get; set; }
}
