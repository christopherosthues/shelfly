using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using KeyAttribute = System.ComponentModel.DataAnnotations.KeyAttribute;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;
using MaxLengthAttribute = System.ComponentModel.DataAnnotations.MaxLengthAttribute;
using IndexAttribute = Microsoft.EntityFrameworkCore.IndexAttribute;

namespace Shelfly.App.Data.Entities;

[Index(nameof(BookId))]
[Index(nameof(BookId), nameof(StartPage))]
public class BookmarkEntity
{
    [Key] public Guid Id { get; set; }

    [Required] public Guid BookId { get; set; }

    [Required] public int StartPage { get; set; }

    public int? EndPage { get; set; }

    [MaxLength(1000)] public string? Note { get; set; }

    [Required] public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    [ForeignKey(nameof(BookId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public BookEntity? Book { get; set; }
}