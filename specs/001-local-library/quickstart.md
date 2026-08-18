# Quickstart: Local Library Management

**Date**: 2026-08-18 | **Feature**: 002-local-library

## Prerequisites

- .NET 10 SDK installed (prerelease allowed via `global.json`)
- Target platform selected (Android, iOS, Windows, or MacOS)
- Solution builds successfully: `dotnet build Shelfly.slnx`

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run the MAUI app on target platform
# Android example:
dotnet run --project Shelfly.App --framework net10.0-android

# Windows example (on Windows host):
dotnet run --project Shelfly.App --framework net10.0-windows
```

## Validation Scenarios

### Scenario 1: Empty State Display

**Goal**: Verify the app shows an empty state message when no books exist.

1. Launch the app on target platform
2. Observe the main screen (BookListPage)
3. **Expected**: Empty state message displayed prompting user to add a book first

### Scenario 2: Add Book Flow

**Goal**: Verify users can add a new book with all required fields.

1. From empty state or list view, navigate to add book screen (BookEditPage)
2. Enter valid data for Title, Author, Publisher, ISBN (valid format), and Publish Date
3. Save the book
4. **Expected**: Book appears in library list showing title, author, and publisher

### Scenario 3: Inline Validation Errors

**Goal**: Verify field validation errors display inline on text fields.

1. Navigate to add/edit book screen
2. Leave Title empty, enter invalid ISBN format, exceed Publisher character limit (>256)
3. Attempt to save
4. **Expected**: Each error displayed inline on the respective text field (using .NET MAUI equivalent of Android supporting text)

### Scenario 4: Search and Sort

**Goal**: Verify search filtering and sorting work correctly.

1. Add at least 3 books with distinct titles, authors, publishers
2. Type a search query matching one book's title
3. **Expected**: Only matching books appear in the list (case-insensitive substring match)
4. Select sort by author
5. **Expected**: List reorders alphabetically by author

### Scenario 5: Swipe-to-Delete (Mobile) / Drag-Swipe (Desktop)

**Goal**: Verify soft deletion via gesture works on target platform.

1. With a book in the list, perform swipe-left gesture (mobile) or drag/swipe equivalent (desktop)
2. **Expected**: Book is soft-deleted and removed from visible list within 200ms with visual feedback

### Scenario 6: Book Detail View

**Goal**: Verify full book details display correctly.

1. Tap/click on a book in the list
2. Navigate to book detail view (BookDetailPage)
3. **Expected**: All book details displayed (title, author, ISBN, publisher, publish date) plus bookmark list

### Scenario 7: Add Bookmark with Page Range

**Goal**: Verify bookmarks can be added with page ranges and notes.

1. From book detail view, tap add button for new bookmark
2. Enter start page (e.g., 50), end page (e.g., 60), and a note
3. Save the bookmark
4. **Expected**: Bookmark appears in list showing full page range with note indicator icon

### Scenario 8: Overlapping Bookmarks Display

**Goal**: Verify overlapping bookmarks display correctly with proper ordering.

1. Add two bookmarks for the same book: one with page range (e.g., pages 10-20) and one with single page (page 15)
2. View bookmark list in detail view
3. **Expected**: Both bookmarks appear as separate entries; range-based bookmark appears first, followed by single-page bookmark

### Scenario 9: Bookmark Validation Error

**Goal**: Verify end page lower than start page triggers validation error.

1. Add/edit a bookmark with page range
2. Enter start page (e.g., 50) and end page (e.g., 40 — lower than start)
3. Attempt to save
4. **Expected**: Validation error displayed on the text field indicating end page must be ≥ start page

### Scenario 10: Note Character Limit

**Goal**: Verify note exceeding 1000 characters triggers validation error.

1. Add/edit a bookmark with a note
2. Enter a note exceeding 1000 characters
3. Attempt to save
4. **Expected**: Validation error displayed on the note field

### Scenario 11: Delete Book from Detail View

**Goal**: Verify book deletion is possible from detail view.

1. Open a book's detail view (with or without bookmarks)
2. Tap the delete button in the detail view
3. **Expected**: Book (including all associated bookmarks) is soft-deleted and removed from visible list; user returns to book list

### Scenario 12: Localization Switching

**Goal**: Verify language switching works at runtime without data loss.

1. Add a book with German text in title/author fields
2. Switch app language (via device settings or in-app toggle) between English and German
3. **Expected**: All UI strings, labels, messages, and validation feedback display in selected language; existing book data persists unchanged

## Expected Outcomes

All scenarios complete successfully with:
- Books added in under 30 seconds (SC-001)
- Search results appear within 500ms of typing query (SC-002)
- Navigation from list to detail view in no more than two taps (SC-003)
- Bookmarks added in under 15 seconds (SC-004)
- Swipe-to-delete gesture completes within 200ms with visual feedback (SC-005)

### Scenario 13: Library Export to JSON

**Goal**: Verify the app can export library data as a JSON file for backup.

1. Add at least one book with associated bookmarks
2. Navigate to the export function (settings or menu option)
3. Trigger the export action
4. **Expected**: A JSON file is generated containing all books and their bookmarks; file is saved to device storage and accessible via file manager

## References

- [Data Model](./data-model.md) — Entity definitions and relationships
- [Research](./research.md) — Technical decisions and rationale
- [Spec](./spec.md) — Full feature specification with acceptance criteria
