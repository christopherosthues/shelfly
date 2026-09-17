using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ForeignKeyAttribute = System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute;

namespace Shelfly.App.Data.Entities;

public class SyncState
{
    public static readonly Guid DefaultId = new("550e8400-e29b-41d4-a716-446655440000");

    [Key]
    public Guid Id { get; set; } = DefaultId;

    public Guid? ActiveServerEntryId { get; set; }

    public bool SyncEnabled { get; set; } = true;

    public DateTime? LastSuccessfulSyncAt { get; set; }

    [MaxLength(512)]
    public string? LastSyncResult { get; set; }

    [ForeignKey(nameof(ActiveServerEntryId))]
    [DeleteBehavior(DeleteBehavior.SetNull)]
    public SavedServerEntry? ActiveServerEntry { get; set; }
}
