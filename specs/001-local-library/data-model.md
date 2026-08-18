# Data Model: Local Library Management

**Date**: 2026-08-18 | **Feature**: 002-local-library

## Entities

### Book (Domain Model — Shelfly.Common)

Framework-agnostic domain model representing a physical book in the user's library.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` | Required, Primary Key | Auto-generated on creation |
| `Title` | `string` | Required, Max 256 chars, Not whitespace-only | Validated on add/edit |
| `Author` | `string` | Required, Max 256 chars, Not whitespace-only | Validated on add/edit |
| `ISBN` | `string` | Required, Unique across all books, ISBN-10 or ISBN-13 format | Dashes included; validated against standard conventions |
| `Publisher` | `string` | Required, Max 256 chars, Not whitespace-only | Validated on add/edit |
| `PublishDate` | `DateTime?` | Optional | Nullable — not all books have a known publish date |
| `DeletedAt` | `DateTime?` | Nullable | Soft delete marker: `null` = active, non-null = soft-deleted |
| `CreatedAt` | `DateTime` | Required, Non-nullable | Audit timestamp — always set via EF Core interceptor on creation |
| `LastModifiedAt` | `DateTime?` | Nullable | Audit timestamp — populated automatically via EF Core interceptor on update |

**Validation Rules**:
- Title, Author, Publisher must not be empty or whitespace-only
- ISBN must follow valid ISBN-10 or ISBN-13 format (including dashes)
- ISBN uniqueness enforced across all books (case-insensitive comparison after normalization)
- Field length limits: Title/Author/Publisher ≤256 characters

**State Transitions**:
- `Active` → `SoftDeleted`: Set `DeletedAt` to current timestamp via swipe-to-delete or detail view delete button; associated bookmarks remain in storage but are hidden from view
- `SoftDeleted` → `Active`: Clear `DeletedAt` (restoration — out of scope for this feature but implied by soft delete); restoring a book automatically restores all its associated bookmarks, making them visible again

### Bookmark (Domain Model — Shelfly.Common)

Framework-agnostic domain model representing a saved page reference within a specific book.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` | Required, Primary Key | Auto-generated on creation |
| `BookId` | `Guid` | Required, Foreign Key to Book | Each bookmark belongs to exactly one book |
| `StartPage` | `int` | Required, Positive integer | Start of page range (or single page if EndPage is null) |
| `EndPage` | `int?` | Optional, ≥ StartPage when set | Null = single page; non-null = page range |
| `Note` | `string?` | Optional, Max 1000 chars | Nullable — bookmarks may have no note |
| `CreatedAt` | `DateTime` | Required, Non-nullable | Audit timestamp — always set via EF Core interceptor on creation |
| `LastModifiedAt` | `DateTime?` | Nullable | Audit timestamp — populated automatically via EF Core interceptor on update |

**Validation Rules**:
- StartPage must be a positive integer (> 0)
- When EndPage is set, it must be ≥ StartPage (validation error if lower)
- Note length ≤ 1000 characters when present
- Overlapping pages allowed: multiple bookmarks may reference the same page(s)

**State Transitions**:
- `Active` → `Deleted`: Hard delete — physically removed from storage (part of trash management, out of scope for this feature)
- Cascade delete: When parent book is hard-deleted, all associated bookmarks cascade-delete automatically (per constitution Principle V; part of trash management, out of scope for this feature)

## Persistence Entities (Shelfly.App/Data/Entities/)

### BookEntity

EF Core persistence entity mapping to domain model. Includes data annotations or FluentAPI configurations for SQLite.

| Field | DB Type | Constraints |
|-------|---------|-------------|
| `Id` | TEXT (GUID) | PRIMARY KEY |
| `Title` | TEXT | NOT NULL, MAX 256 chars |
| `Author` | TEXT | NOT NULL, MAX 256 chars |
| `ISBN` | TEXT | UNIQUE INDEX, NOT NULL |
| `Publisher` | TEXT | NOT NULL, MAX 256 chars |
| `PublishDate` | INTEGER (DateTime) | NULLABLE |
| `DeletedAt` | INTEGER (DateTime) | NULLABLE |
| `CreatedAt` | INTEGER (DateTime) | NOT NULL |
| `LastModifiedAt` | INTEGER (DateTime) | NULLABLE |

**Indexes**:
- Unique index on `ISBN` (case-insensitive collation for uniqueness)
- Composite index on `(Title, Author, Publisher)` for search optimization

### BookmarkEntity

EF Core persistence entity mapping to domain model.

| Field | DB Type | Constraints |
|-------|---------|-------------|
| `Id` | TEXT (GUID) | PRIMARY KEY |
| `BookId` | TEXT (GUID) | FOREIGN KEY → BookEntity.Id, ON DELETE CASCADE |
| `StartPage` | INTEGER | NOT NULL, CHECK > 0 |
| `EndPage` | INTEGER | NULLABLE, CHECK ≥ StartPage when non-null |
| `Note` | TEXT | NULLABLE, MAX 1000 chars |
| `CreatedAt` | INTEGER (DateTime) | NOT NULL |
| `LastModifiedAt` | INTEGER (DateTime) | NULLABLE |

**Indexes**:
- Index on `BookId` for efficient bookmark list retrieval per book
- Composite index on `(BookId, StartPage)` for overlapping page queries

## Relationships

### Book → Bookmark (One-to-Many)

- One Book has many Bookmarks
- Each Bookmark belongs to exactly one Book
- **Cascade Delete**: When a Book is hard-deleted, all associated Bookmarks are physically removed (part of trash management, out of scope for this feature)
- **Soft Delete Behavior**: When a Book is soft-deleted (`DeletedAt` set), associated Bookmarks remain in storage but are hidden from view (in scope for this feature)
- **Restoration Behavior**: When a soft-deleted book is restored (clearing `DeletedAt`), all associated bookmarks are automatically restored and become visible again (part of trash management, out of scope for this feature)

## Capacity Notes

- No explicit upper bound on book or bookmark count
- Practical capacity bounded by device storage (~10,000 books estimated)
- SQLite handles typical personal library sizes without performance degradation

## LocalDbContext (Shelfly.App/Data/)

EF Core DbContext for local SQLite storage.

| DbSet | Entity | Notes |
|-------|--------|-------|
| `Books` | `BookEntity` | Filtered by `DeletedAt == null` in default queries |
| `Bookmarks` | `BookmarkEntity` | Cascade delete on Book hard deletion (trash management, out of scope); soft delete hides bookmarks from view when book is soft-deleted (in scope) |

**Configuration**:
- SQLite database file stored in app's local storage directory
- Database created and migrated on first app launch (EF Core migrations)
- Connection string uses embedded SQLite provider (`Data Source={local_path};`)
