# Data Model: Book Card Info & Sorting Enhancements

**Date**: 2026-09-04  
**Feature**: 012-book-card-info-sorting

## Entity Changes

### SortCriterion Enum (Extended)

**File**: `Shelfly.App/Enums/SortCriterion.cs`

**Changes**: Add two new enum values for date-based sorting.

```csharp
public enum SortCriterion
{
    Title,
    Author,
    Publisher,
    PublishDate,
    CreatedAt,       // NEW: sort by book creation timestamp
    LastModifiedAt   // NEW: sort by last modification timestamp (null → CreatedAt fallback)
}
```

**Validation Rules**:
- All enum values must have corresponding display names in localization resources
- SortCriterion values are used exclusively with SortDirection (Ascending/Descending)

### BookEntity (Computed Properties)

**File**: `Shelfly.App.Data/Entities/BookEntity.cs`

**Changes**: Add computed properties for UI binding. No database schema changes required — these are client-side computed fields populated during list loading.

```csharp
// NEW: Computed bookmark count (populated via EF Core COUNT query)
public int BookmarkCount { get; set; }

// NEW: Display-ready last modified date (null-coalesced to CreatedAt)
public DateTime DisplayLastModifiedAt => LastModifiedAt ?? CreatedAt;
```

**Notes**:
- `BookmarkCount` is not persisted — it reflects the current bookmark count at query time
- `DisplayLastModifiedAt` provides a consistent non-null value for XAML binding
- Both properties are populated during list loading in LibraryService/TrashService

### BookmarkEntity (No Changes)

**File**: `Shelfly.App.Data/Entities/BookmarkEntity.cs`

**Status**: Existing structure sufficient. Foreign key relationship to BookEntity via BookId enables efficient COUNT queries.

## Query Patterns

### Efficient Bookmark Count Retrieval

To avoid N+1 queries when loading book lists, use a single grouped query:

```csharp
// Retrieve bookmark counts for all books in one round-trip
var countQuery = dbContext.Bookmarks
    .Where(b => bookIds.Contains(b.BookId))
    .GroupBy(b => b.BookId)
    .Select(g => new { BookId = g.Key, Count = g.Count() });

var counts = await countQuery.ToDictionaryAsync(x => x.BookId, x => x.Count);

// Merge results into book entities
foreach (var book in books)
{
    book.BookmarkCount = counts.GetValueOrDefault(book.Id, 0);
}
```

### Date Sorting with Null Fallback

For LastModifiedAt sorting, use null-coalescing to CreatedAt:

```csharp
// Ascending sort (nulls first via PostgreSQL default)
query.OrderBy(b => b.LastModifiedAt ?? b.CreatedAt)

// Descending sort (nulls last via PostgreSQL default)
query.OrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt)
```

**Note**: The null-coalescing fallback ensures consistent ordering for books without modifications, while respecting the clarified requirement that CreatedAt serves as the display value when LastModifiedAt is null.

## State Transitions

### Bookmark Count Updates

When bookmarks are added, edited, or deleted:
1. The affected book's `BookmarkCount` must be refreshed on next list load
2. No immediate UI update required — count reflects current database state at query time
3. For real-time updates, the ViewModel should reload the list after bookmark operations complete

### Last Modified Date Updates

When a book or its bookmarks are modified:
1. `LastModifiedAt` is updated server-side (or client-side for local DB)
2. The card displays the new timestamp on next list load
3. For real-time updates, the ViewModel should reload the list after modification operations complete
