# Contract: Sorting Service Interface

**Date**: 2026-09-04  
**Feature**: 012-book-card-info-sorting

## SortCriterion Extension

### New Enum Values

| Value | Description | Null Handling |
|-------|-------------|---------------|
| `CreatedAt` | Sort by book creation timestamp | Non-null (always has value) |
| `LastModifiedAt` | Sort by last modification timestamp | Null → CreatedAt fallback during sort |

## Service Method Contract

### LibraryService.SearchSortedBooksAsync

**Signature**:
```csharp
Task<List<BookEntity>> SearchSortedBooksAsync(
    string query, 
    SortCriterion criterion, 
    SortDirection direction, 
    CancellationToken cancellationToken = default)
```

**Behavior for New Criteria**:

| Criterion | Ascending Behavior | Descending Behavior |
|-----------|-------------------|---------------------|
| `CreatedAt` | Oldest books first | Newest books first |
| `LastModifiedAt` | Nulls first (unmodified books), then by modification date | Most recently modified first, nulls last |

**Null Handling**: When sorting by LastModifiedAt, use `b.LastModifiedAt ?? b.CreatedAt` as the sort key to ensure consistent ordering for unmodified books.

### TrashService.SearchSortedTrashBooksAsync

**Signature**:
```csharp
Task<List<BookEntity>> SearchSortedTrashBooksAsync(
    string query, 
    SortCriterion criterion, 
    SortDirection direction, 
    CancellationToken cancellationToken = default)
```

**Behavior**: Identical to LibraryService for new criteria, with `IgnoreQueryFilters()` applied to include soft-deleted books.

## ViewModel Contract

### SortableListViewModelBase.SortOptions

The `SortOptions` collection must include entries for the new sort criteria:

```csharp
public List<SortOptionDisplay> SortOptions { get; } =
[
    // Existing options
    new SortOptionDisplay(SortCriterion.Title, AppResources.BookListPageSortByTitle),
    new SortOptionDisplay(SortCriterion.Author, AppResources.BookListPageSortByAuthor),
    new SortOptionDisplay(SortCriterion.Publisher, AppResources.BookListPageSortByPublisher),
    new SortOptionDisplay(SortCriterion.PublishDate, AppResources.BookListPageSortByPublishDate),
    
    // NEW options
    new SortOptionDisplay(SortCriterion.CreatedAt, AppResources.BookListPageSortByCreatedAt),
    new SortOptionDisplay(SortCriterion.LastModifiedAt, AppResources.BookListPageSortByLastModifiedAt)
];
```

**Localization Keys Required**:
- `BookListPageSortByCreatedAt` — English: "Created Date", German: (TBD)
- `BookListPageSortByLastModifiedAt` — English: "Last Modified", German: (TBD)

## Error Handling

| Scenario | Behavior |
|----------|----------|
| Unknown SortCriterion value | Fall back to default sort (Title, Ascending) |
| Null LastModifiedAt during sort | Use CreatedAt as fallback sort key |
| Empty list | Return empty collection — no error thrown |
