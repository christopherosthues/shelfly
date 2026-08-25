# Data Model: Swipe-to-Edit Book

**Date**: 2026-08-25
**Branch**: `003-swipe-edit-book`

## Entities Affected

### BookEntity (Existing)

No structural changes required. The entity already contains all fields needed for editing:

| Field | Type | Editable | Notes |
|-------|------|----------|-------|
| `Id` | `Guid` (UUID v7) | No | Primary key, auto-generated via `Guid.CreateVersion7()` |
| `Title` | `string?` | Yes | Max 256 characters, validated by FluentValidation |
| `Author` | `string?` | Yes | Max 256 characters |
| `Publisher` | `string?` | Yes | Max 256 characters |
| `ISBN` | `string?` | Yes | Unique constraint enforced at database level |
| `PublicationYear` | `int?` | Yes | Nullable integer for publication date |
| `DeletedAt` | `DateTimeOffset?` | No | Soft deletion timestamp; null = active |

### SwipeItem (MAUI Framework Type)

The edit action element is a `SwipeItem` instance configured at runtime:

| Property | Value | Source |
|----------|-------|--------|
| `Text` | Localized string | `AppResources.BookListPageSwipeToEditCommand` |
| `IconImageSource` | `"edit_icon.svg"` | SVG asset in `Resources/Raw/` |
| `BackgroundColor` | Theme-appropriate color | Matches existing UI design language |
| `Command` | `NavigateToEditBookCommand` | Bound from `BookListViewModel` |
| `CommandParameter` | Book entity's `Id` | `{Binding Id}` on the BookEntity data context |

## Validation Rules (Existing)

The edit page (`BookEditPage`) uses existing FluentValidation rules:

- Title: Required, max 256 characters
- Author: Optional, max 256 characters
- Publisher: Optional, max 256 characters
- ISBN: Unique constraint checked against database; duplicate warning displayed if found
- PublicationYear: Optional integer

## State Transitions

No new state transitions introduced. The feature leverages existing navigation flow:

```
BookListPage → (swipe left + tap edit) → BookEditPage(bookId) → (save) → Back to BookListPage
```

The book entity's data is fetched from the API and displayed in the edit page. Upon saving, changes are persisted via the existing library service and the list view refreshes with updated data.
