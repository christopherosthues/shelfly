using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Shelfly.App.Data.Entities;

[Index(nameof(ServerId))]
[Index(nameof(IsActive))]
public class SavedServerEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ServerId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ProfileUsername { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string ProfileEmail { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(ServerId))]
    [DeleteBehavior(DeleteBehavior.Cascade)]
    public ServerEntity? Server { get; set; }
}
