using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ForeignKeyAttribute = System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute;

namespace Shelfly.App.Data.Entities;

[Index(nameof(BookId), nameof(ProfileUsername), IsUnique = true)]
[Index(nameof(ProfileUsername))]
public class BookProfileSyncRecord
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ProfileUsername { get; set; } = string.Empty;

    public DateTime LastSyncAt { get; set; }

    [ForeignKey(nameof(BookId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public BookEntity? Book { get; set; }
}
