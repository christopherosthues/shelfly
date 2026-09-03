# Data Model: Trash Management

**Date**: 2026-09-02  
**Feature**: [spec.md](./spec.md) | [plan.md](./plan.md)

## Entities

### BookEntity (Existing — Extended for Trash)

**File**: `Shelfly.App.Data/Entities/BookEntity.cs`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid | Key, UUIDv7 | Primary identifier |
| Title | string | Required, MaxLength(256) | Searchable field |
| Author | string | Required, MaxLength(256) | Searchable field |
| ISBN | string | Required, Unique | Searchable field |
| Publisher | string | Required, MaxLength(256) | Searchable field |
| PublishDate | DateTime? | Nullable | Sort criterion |
| DeletedAt | DateTime? | Nullable | **Soft deletion marker**: `null` = active, non-null = soft-deleted |
| CreatedAt | DateTime | Required | Audit timestamp |
| LastModifiedAt | DateTime? | Nullable | Audit timestamp |

**Global Query Filter**: `DeletedAt == null` (excludes soft-deleted books from normal queries)

### BookmarkEntity (Existing — No Changes)

**File**: `Shelfly.App.Data/Entities/BookmarkEntity.cs`

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | Guid | Key, UUIDv7 | Primary identifier |
| BookId | Guid | Required, ForeignKey | Links to parent book |
| StartPage | int | Required | Page reference |
| EndPage | int? | Nullable | Optional end page |
| Note | string? | MaxLength(1000) | Displayed in trash detail view |
| CreatedAt | DateTime | Required | Audit timestamp |
| LastModifiedAt | DateTime? | Nullable | Audit timestamp |

**Relationship**: Cascade delete on `BookId` FK — when parent book is hard-deleted, bookmarks are physically removed

### TrashService State (New)

The trash service maintains no persistent state. It operates as a query layer over existing entities:

| Operation | Query Pattern | Result |
|-----------|---------------|--------|
| List trash items | `Books.Where(b => b.DeletedAt != null)` with `.IgnoreQueryFilters()` | Collection of soft-deleted books + their bookmarks |
| Restore book | `SET DeletedAt = NULL` on target book | Returns to active library |
| Hard delete book | `Remove(book)` from DbContext (cascades to bookmarks) | Physical row deletion |
| Search trash | LIKE pattern on Title, Author, Publisher, ISBN filtered by `DeletedAt != null` | Filtered soft-deleted items |
| Sort trash | ORDER BY on sort criterion with direction | Ordered result set |

## State Transitions

### Book Lifecycle

```
[Active] ──(SoftDelete)──> [Soft-Deleted/Trash]
                                    │
                                    ├─(Restore)──> [Active]
                                    │
                                    └─(HardDelete)──> [Gone]
                                                       │
                                                       └─(Cascade)──> Bookmarks [Gone]
```

**Transition Details**:

| Transition | Trigger | Data Change | Query Filter Impact |
|------------|---------|-------------|---------------------|
| Active → Soft-Deleted | User swipes right or selects "Delete" in library | `DeletedAt = DateTime.UtcNow` | Excluded from global filter (`DeletedAt != null`) |
| Soft-Deleted → Active | User swipes left or selects "Restore" in trash | `DeletedAt = NULL` | Included in global filter again |
| Soft-Deleted → Gone | User swipes right or selects "Delete" in trash | Physical row removal | Removed from database entirely |

### Bookmark Lifecycle (Inherited)

```
[Active] ──(Parent SoftDeleted)──> [Visible in Trash]
                                      │
                                      ├─(Parent Restored)──> [Active]
                                      │
                                      └─(Parent HardDeleted)──> [Gone - Cascade]
```

**Key Constraint**: Bookmarks have no independent lifecycle. They appear in trash only when their parent book is soft-deleted, and are permanently removed when the parent book is hard-deleted via FK cascade.

## Validation Rules

| Rule | Entity | Description |
|------|--------|-------------|
| Restore requires DeletedAt | BookEntity | Only soft-deleted books (`DeletedAt != null`) can be restored |
| Hard delete cascades bookmarks | BookEntity → BookmarkEntity | Removing a book physically deletes all associated bookmarks |
| Trash query uses IgnoreQueryFilters | DbContext | Must bypass global filter to include soft-deleted items |

## Index Considerations

Existing indexes from `BookEntity`:
- Composite: `(Title, Author, Publisher)` — supports search queries
- Unique: `(ISBN)` — ensures uniqueness across active and soft-deleted books

No new indexes required for trash operations. The existing composite index covers the LIKE-based search patterns used in both library and trash views.
