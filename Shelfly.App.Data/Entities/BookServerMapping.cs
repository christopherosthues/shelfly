using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ForeignKeyAttribute = System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute;

namespace Shelfly.App.Data.Entities;

[Index(nameof(BookId), nameof(ServerId), IsUnique = true)]
[Index(nameof(ServerAssignedId), nameof(ServerId))]
public class BookServerMapping
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BookId { get; set; }

    [Required]
    public Guid ServerId { get; set; }

    [Required]
    [MaxLength(256)]
    public string ServerAssignedId { get; set; } = string.Empty;

    [ForeignKey(nameof(BookId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public BookEntity? Book { get; set; }

    [ForeignKey(nameof(ServerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public ServerEntity? Server { get; set; }
}
