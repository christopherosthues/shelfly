using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ForeignKeyAttribute = System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute;

namespace Shelfly.App.Data.Entities;

[Index(nameof(Url), IsUnique = true)]
public class ServerEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(2048)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? DisplayName { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public ICollection<SavedServerEntry> SavedEntries { get; set; } = [];
}
