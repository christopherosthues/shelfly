# Quickstart: Trash Management Validation Guide

**Date**: 2026-09-02  
**Feature**: [spec.md](./spec.md) | [plan.md](./plan.md)

## Prerequisites

1. **Solution builds successfully**: `dotnet build Shelfly.slnx`
2. **Local database is initialized**: The `LocalDbContext` migrations have been applied
3. **Test data exists**: At least 5 books with varied titles/authors in the library

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run the MAUI app (Android emulator recommended)
dotnet run --project Shelfly.App
```

## Validation Scenarios

### Scenario 1: Shell Flyout Navigation

**Goal**: Verify Library/Trash switching via flyout menu

**Steps**:
1. Launch the app — confirm BookListPage displays as the default view
2. Open the Shell flyout (hamburger icon or swipe from left edge)
3. Verify two options appear: "Library" and "Trash"
4. Select "Trash" — navigate to TrashListPage
5. Select "Library" — return to BookListPage

**Expected Outcome**: Flyout menu displays both Library and Trash entries; navigation between them is immediate with no errors.

### Scenario 2: Soft-Deleted Items Appear in Trash

**Goal**: Verify soft-deleted books appear in the trash view

**Steps**:
1. In Library view, swipe right on a book to trigger soft delete (existing functionality)
2. Open flyout → select "Trash"
3. Verify the soft-deleted book appears in the trash list
4. Verify associated bookmarks of that book also appear grouped under it

**Expected Outcome**: The book with `DeletedAt != null` is visible in trash; its bookmarks are displayed alongside it.

### Scenario 3: Restore Item from Trash (Swipe)

**Goal**: Verify right-to-left swipe restores a soft-deleted item

**Steps**:
1. In Trash view, locate a soft-deleted book
2. Swipe the item from right to left
3. Observe the "Restore" swipe action triggers
4. Navigate back to Library view
5. Verify the restored book appears in the library list

**Expected Outcome**: The book's `DeletedAt` is cleared (`NULL`); it reappears in the active library with all data intact.

### Scenario 4: Permanently Delete Item from Trash (Swipe)

**Goal**: Verify left-to-right swipe permanently removes an item

**Steps**:
1. In Trash view, locate a soft-deleted book
2. Swipe the item from left to right
3. Observe the "Delete" swipe action triggers
4. Confirm the item disappears from the trash list
5. Navigate to Library — verify the item is absent

**Expected Outcome**: The book row is physically removed from the database; associated bookmarks are cascade-deleted.

### Scenario 5: Read-Only Detail View

**Goal**: Verify tapping a trash item opens a read-only details page

**Steps**:
1. In Trash view, tap a soft-deleted book
2. Verify BookDetailPage opens with all fields displayed but not editable
3. Tap any field — confirm no keyboard appears and value remains unchanged
4. Navigate back to trash list

**Expected Outcome**: All book information is visible; edit controls are disabled or hidden; the user cannot modify data.

### Scenario 6: Bookmark Note-Only Display

**Goal**: Verify bookmark details in trash show only the note content

**Steps**:
1. In Trash view, tap a soft-deleted bookmark (child of a soft-deleted book)
2. Verify the detail page displays only the `Note` field
3. Verify other bookmark properties (StartPage, EndPage, BookId) are hidden

**Expected Outcome**: Only the note text is visible; no editing controls appear.

### Scenario 7: Search in Trash View

**Goal**: Verify search functionality matches book list behavior

**Steps**:
1. In Trash view, enter a search query in the SearchBar
2. Verify results filter to matching soft-deleted items (LIKE pattern on Title, Author, Publisher, ISBN)
3. Clear the search — verify full trash list reappears
4. Test with no results — verify empty state displays

**Expected Outcome**: Search filters trash items using the same fields and patterns as the book list; empty states are handled gracefully.

### Scenario 8: Sort in Trash View

**Goal**: Verify sorting functionality matches book list behavior

**Steps**:
1. In Trash view, open the sort Picker
2. Select a sort criterion (Title, Author, Publisher, PublishDate)
3. Toggle sort direction using the ImageButton
4. Verify the trash list reorders accordingly

**Expected Outcome**: Items reorder based on selected criterion and direction; sorting persists across navigation within the app.

### Scenario 9: Multi-Item Selection

**Goal**: Verify long-press selection mode with batch operations

**Steps**:
1. In Trash view, long-press an item to enter selection mode
2. Tap additional items — verify they become visually selected
3. Select "Delete Selected" from the toolbar
4. Confirm all selected items are permanently removed
5. Repeat with "Restore Selected" on remaining trash items

**Expected Outcome**: Selection state tracks multiple items; batch operations affect all selected entries simultaneously.

### Scenario 10: Bulk Operations

**Goal**: Verify "Restore All" and "Delete All" toolbar actions

**Steps**:
1. In Trash view, populate with at least 3 soft-deleted books
2. Tap "Restore All" — verify all items return to library
3. Soft-delete the same books again
4. Tap "Delete All" — confirm all items are permanently removed
5. Verify trash is empty

**Expected Outcome**: Bulk operations affect every trash item; cascade deletion removes dependent bookmarks.

## Edge Case Validation

| Edge Case | Verification Steps | Expected Result |
|-----------|-------------------|-----------------|
| Empty trash | Navigate to Trash with no soft-deleted items | Empty state message displays |
| Search no results | Enter query matching no trash items | "No results" message displays |
| Restore bookmark without parent | Parent book hard-deleted, then restore bookmark | Bookmark restored only if parent exists; otherwise orphaned or hidden |
| Swipe mid-navigation | Swipe item while navigating away from trash | Gesture completes or cancels gracefully |

## Success Indicators

- [ ] All 10 validation scenarios pass without errors
- [ ] Shell flyout navigation is responsive and stable
- [ ] Search/sort performance meets criteria (search < 1s, sort < 500ms)
- [ ] Swipe gestures trigger correct actions consistently
- [ ] Multi-item selection state persists during interaction
- [ ] Read-only detail pages prevent all edits

## Rollback Steps

If validation fails:

```bash
# Revert database to pre-feature state (SQLite backup restore)
# Or run initial migration again
dotnet ef migrations revert --project Shelfly.App.Data
```
