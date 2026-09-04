# Quickstart Validation Guide: Book Card Info & Sorting Enhancements

**Date**: 2026-09-04  
**Feature**: 012-book-card-info-sorting

## Prerequisites

1. **Solution built successfully**: `dotnet build Shelfly.slnx`
2. **Local database initialized**: Ensure SQLite local storage is created via `LocalDbContext.EnsureDatabaseCreatedAsync()`
3. **Test data available**: At least 3 books with varying:
   - Creation dates (CreatedAt)
   - Modification dates (LastModifiedAt — some null, some populated)
   - Bookmark counts (0, 1, and multiple bookmarks per book)

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run the MAUI client (Android emulator or Windows desktop)
dotnet run --project Shelfly.App
```

## Validation Scenarios

### Scenario 1: Bookmark Count Display

**Goal**: Verify bookmark count appears at top right of every book card.

**Steps**:
1. Navigate to Library list view
2. Observe each book card displays a numeric value at the top right corner
3. For a book with zero bookmarks, verify "0" is displayed
4. For a book with one or more bookmarks, verify the correct count is shown
5. Add a new bookmark to a book via BookDetailPage → return to list → verify count incremented

**Expected Outcome**: All cards display accurate bookmark counts; updates reflect after bookmark operations.

### Scenario 2: Last Modified Date Display

**Goal**: Verify last modified date appears at bottom right of every book card.

**Steps**:
1. Navigate to Library list view
2. Observe each book card displays a formatted date at the bottom right corner
3. For a newly created book (no modifications), verify CreatedAt is displayed
4. For a modified book, verify LastModifiedAt is displayed
5. Edit a bookmark → return to list → verify updated timestamp appears

**Expected Outcome**: All cards display dates; null LastModifiedAt falls back to CreatedAt.

### Scenario 3: Sort by Creation Date

**Goal**: Verify sorting by CreatedAt works in both directions.

**Steps**:
1. Navigate to Library list view
2. Open sort picker → select "Created Date" (or localized equivalent)
3. Verify books are ordered oldest to newest (ascending default)
4. Toggle sort direction → verify order reverses (newest to oldest)
5. Repeat for Trash list view

**Expected Outcome**: Books sorted correctly by creation timestamp; direction toggle works.

### Scenario 4: Sort by Last Modified Date

**Goal**: Verify sorting by LastModifiedAt works with null handling.

**Steps**:
1. Navigate to Library list view
2. Open sort picker → select "Last Modified" (or localized equivalent)
3. Verify books with modifications appear first (descending default or ascending based on initial state)
4. Verify unmodified books (null LastModifiedAt) are positioned according to null-coalescing fallback behavior
5. Toggle sort direction → verify order reverses consistently
6. Repeat for Trash list view

**Expected Outcome**: Books sorted by modification date; null values handled via CreatedAt fallback.

### Scenario 5: Trash List Parity

**Goal**: Verify all features work identically in trash list view.

**Steps**:
1. Soft-delete a book from Library → navigate to Trash
2. Verify bookmark count and last modified date display on trashed books
3. Verify sorting options include new criteria
4. Test sort direction toggle for both new criteria

**Expected Outcome**: Trash list mirrors library list functionality for card info and sorting.

## Localization Validation

**Goal**: Verify new sort option strings appear in both languages.

**Steps**:
1. Run app with English locale → open sort picker → verify "Created Date" and "Last Modified" options
2. Switch device/app locale to German → open sort picker → verify translated equivalents appear
3. Verify no resource key missing errors in application output

**Expected Outcome**: Sort options display correctly in both en-US and de-DE locales.

## Success Indicators

- [ ] Bookmark count visible on all cards (library + trash)
- [ ] Last modified date visible on all cards (with CreatedAt fallback for null)
- [ ] Sort picker includes "Created Date" option
- [ ] Sort picker includes "Last Modified" option  
- [ ] Direction toggle works for both new criteria
- [ ] Localization strings present in both languages
- [ ] No runtime binding errors in XAML output

## Known Limitations

- Bookmark count reflects database state at query time — not real-time during session
- Date formatting uses platform-specific conventions; consider resource-defined format strings for consistency across locales
