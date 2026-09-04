# Research: Book Card Info & Sorting Enhancements

**Date**: 2026-09-04  
**Feature**: 012-book-card-info-sorting

## Decision: Null LastModifiedAt Sorting Behavior

**Decision**: Defer to EF Core / PostgreSQL default null sorting behavior (nulls first in ascending, nulls last in descending)

**Rationale**: The constitution specifies that the API uses PostgreSQL via Npgsql. PostgreSQL's default ORDER BY behavior places NULL values first in ASC order and last in DESC order. This aligns with user expectations: when sorting by "most recently modified" (DESC), books without modifications appear at the bottom; when sorting by "oldest modification" (ASC), they appear at the top.

**Alternatives considered**:
- Explicit null-coalescing fallback (like PublishDate uses `DateTime.MinValue`/`DateTime.MaxValue`) — adds complexity and may produce unexpected ordering for unmodified books
- Custom comparer in client-side sort — loses database-level optimization benefits

## Decision: Bookmark Count Computation Strategy

**Decision**: Compute bookmark count client-side via EF Core COUNT query per book during list loading

**Rationale**: The current architecture loads full BookEntity lists with no pre-computed bookmark counts. Adding a server-side computed field would require API changes (new endpoint or GraphQL resolver modification). Client-side computation is sufficient because:
1. Bookmark count is only needed for display in list views
2. EF Core can efficiently compute COUNT via `dbContext.Bookmarks.Count(b => b.BookId == book.Id)` 
3. For performance, a single grouped query can retrieve counts for all books in one round-trip

**Alternatives considered**:
- Server-side computation with API endpoint — requires backend changes, increases coupling
- Navigation property on BookEntity (`ICollection<BookmarkEntity>`) — loads full bookmark entities into memory unnecessarily
- Denormalized BookmarkCount column on Books table — adds write-path complexity (increment/decrement on every bookmark CRUD)

**Selected approach**: Use a single EF Core query to retrieve bookmark counts for all books in the current list via `GroupBy` + `Count`, then merge results client-side. This minimizes database round-trips while avoiding full entity loading.

```csharp
// Example: Efficient count retrieval
var counts = await dbContext.Bookmarks
    .Where(b => bookIds.Contains(b.BookId))
    .GroupBy(b => b.BookId)
    .Select(g => new { BookId = g.Key, Count = g.Count() })
    .ToDictionaryAsync(x => x.BookId, x => x.Count);
```

## Decision: Last Modified Date Display Fallback

**Decision**: Display CreatedAt when LastModifiedAt is null (creation date serves as last modified date for unmodified books)

**Rationale**: Clarified during `/speckit.clarify` — users expect to see a temporal indicator on every card. Showing the creation date provides consistent information and avoids empty space or placeholder text. This matches the constitution's guidance that LastModifiedAt is nullable and represents "most recent modification."

**Alternatives considered**:
- Empty display for null LastModifiedAt — creates visual inconsistency across cards
- "Never modified" text — adds localization burden and visual clutter

## Decision: Sort Criterion Implementation Pattern

**Decision**: Extend existing switch-expression pattern in LibraryService/TrashService with new cases for CreatedAt and LastModifiedAt

**Rationale**: The current sorting implementation uses a switch expression on SortCriterion enum. Adding two new cases follows the established pattern without structural changes. For nullable LastModifiedAt, use null-coalescing fallback to CreatedAt during sorting (consistent with PublishDate's approach using DateTime.MinValue/DateTime.MaxValue).

**Alternatives considered**:
- Expression tree builder for dynamic sorting — over-engineered for 2 new criteria
- Separate sort method per criterion — increases code duplication

## Decision: UI Binding Strategy for BookCardView

**Decision**: Add computed properties to BookEntity (or a view model wrapper) that provide bookmark count and display-ready last modified date

**Rationale**: XAML bindings require direct property access. Adding computed properties avoids complex converters or multi-bindings. The properties can be:
- `BookmarkCount` — populated via the efficient COUNT query described above
- `DisplayLastModifiedAt` — returns LastModifiedAt ?? CreatedAt

**Alternatives considered**:
- Value converter in XAML — adds indirection and testing complexity
- Code-behind computed properties on BookCardView — couples control to data logic

## Dependencies & Best Practices

### EF Core Null Sorting (PostgreSQL)

PostgreSQL default behavior confirmed:
- `ORDER BY column ASC` → NULLs first
- `ORDER BY column DESC` → NULLs last

For LastModifiedAt sorting with CreatedAt fallback, use:
```csharp
// Ascending: nulls first (unmodified books appear first)
query.OrderBy(b => b.LastModifiedAt ?? b.CreatedAt)

// Descending: nulls last (unmodified books appear last)  
query.OrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt)
```

### MAUI XAML Binding for Dates

Date display should use string formatting via binding:
```xml
<Label Text="{Binding DisplayLastModifiedAt, StringFormat='{0:yyyy-MM-dd}'}" />
```

For localization, consider using a date format resource or platform-specific formatting.

### CollectionView Performance

Displaying bookmark count and dates on every card requires efficient data loading. The recommended approach batches the COUNT query to avoid N+1 queries when rendering large lists.
