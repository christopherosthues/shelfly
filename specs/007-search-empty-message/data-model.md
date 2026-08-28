# Data Model: Search Empty Message

## Entities

No new database entities introduced. The feature operates entirely on existing view model state.

### View Model State Changes

| Property | Type | Source | Description |
|----------|------|--------|-------------|
| `SearchQuery` | `string` | Existing `[ObservableProperty]` | User-provided search text; empty/null means no active search |
| `Books` | `ObservableCollection<BookEntity>` | Existing `[ObservableProperty]` | Filtered book list; zero items triggers EmptyView display |

### Computed Property (New)

| Property | Type | Logic | Description |
|----------|------|-------|-------------|
| `EmptyStateMessage` | `string` | Derived from `SearchQuery` + `Books.Count` | Returns localized message: search-specific when query is active, standard "no books" otherwise |

**Logic**:
```
IF Books.Count == 0 AND SearchQuery is not null/whitespace:
    RETURN localization key for "search returned no results"
ELSE IF Books.Count == 0:
    RETURN localization key for "standard no books available"
ELSE:
    RETURN empty string (or default)
```

## Localization Keys (New)

| Key | en-US Value | de-DE Value | Used By |
|-----|-------------|-------------|---------|
| `BookListPageSearchEmptyMessage` | "No books matched your search" | [TBD - German translation] | EmptyView when search active |
| `BookListPageEmptyStateMessage` | (existing) "No books available. Add one with +" | (existing) | EmptyView when no search active |

## Validation Rules

- `SearchQuery` whitespace-only is treated as "no active search"
- Message switch must occur immediately upon property change (reactive binding)
