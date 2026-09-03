# Research: Trash Management

**Date**: 2026-09-02  
**Feature**: [spec.md](./spec.md) | [plan.md](./plan.md)

## Decisions

### Decision 1: Shell Flyout Navigation Pattern

**Rationale**: The spec requires switching between Library and Trash via flyout menu. Constitution Principle III mandates Shell navigation with `AddScopedWithShellRoute` registration.

**Chosen Approach**: Convert the current single-content AppShell to a flyout-based structure with two `FlyoutItem` entries:
- "Library" → BookListPage (existing)
- "Trash" → TrashListPage (new)

**Implementation Details**:
1. Modify `AppShell.xaml` to wrap content in `<Shell>` with `<FlyoutItem>` children
2. Each flyout item contains a `ShellContent` pointing to the respective page route
3. Add route constant for `TrashListPage` in `Routes.cs`
4. Register TrashListPage/ViewModel via `AddScopedWithShellRoute<TrashListPage, TrashListViewModel>(Routes.TrashListPage)`

**Alternatives Considered**:
- **TabBar navigation**: Less suitable — trash is a secondary view, not a primary tab
- **Modal presentation**: Requires explicit dismiss; flyout provides persistent access
- **Push navigation from BookListPage**: Loses the global Library/Trash context switching

### Decision 2: BookmarkEntity Soft Deletion Strategy

**Rationale**: The spec requires displaying soft-deleted bookmarks in trash. Currently `BookmarkEntity` lacks a `DeletedAt` field and follows parent book deletion via FK cascade.

**Chosen Approach**: Bookmarks inherit the parent book's soft-deletion state. When a book is soft-deleted (`DeletedAt != null`), all its bookmarks appear in the trash view alongside it. No new `DeletedAt` column added to `BookmarkEntity`.

**Query Logic**:
- Trash query: `Books.Where(b => b.DeletedAt != null).SelectMany(b => b.Bookmarks)` — loads soft-deleted books and their associated bookmarks
- This avoids schema changes and maintains the cascade relationship defined in Constitution Principle V

**Alternatives Considered**:
- **Add DeletedAt to BookmarkEntity**: Requires migration, new global query filter, and independent lifecycle management — overkill for inherited state
- **Separate bookmark trash endpoint**: Adds API complexity; bookmarks are always tied to a parent book

### Decision 3: Multi-Item Selection Pattern in MAUI CollectionView

**Rationale**: The spec requires long-press selection mode with batch restore/delete operations.

**Chosen Approach**: Implement selection state tracking in the ViewModel using an `ObservableCollection<Guid>` for selected item IDs. Long-press triggers a boolean flag (`IsSelectionMode`) that toggles the toolbar and enables multi-selection.

**UI Pattern**:
1. CollectionView items bind to a converter that checks if the item's ID is in the selected collection
2. Long-press on an item adds its ID to the selection and sets `IsSelectionMode = true`
3. Subsequent taps toggle selection state for individual items
4. Toolbar displays "Restore Selected" / "Delete Selected" when items are selected
5. A "Done" button exits selection mode

**Alternatives Considered**:
- **MAUI built-in SelectionMode**: Limited customization; requires `SelectionMode="Multiple"` which lacks long-press activation
- **SwipeView-only gestures**: Insufficient for batch operations on many items
- **Third-party selection library**: Adds dependency without explicit approval (Constitution Dependency Policy)

### Decision 4: Swipe Gesture Direction Mapping

**Rationale**: The spec defines left-to-right swipe for delete and right-to-left swipe for restore. This conflicts with the existing BookListPage where right swipe = soft delete and left swipe = edit.

**Chosen Approach**: Trash-specific swipe mapping:
- **SwipeView.RightItems** → Delete (permanent removal) — triggered by swiping left-to-right
- **SwipeView.LeftItems** → Restore (return to library) — triggered by swiping right-to-left

This matches the spec's directional description and creates a consistent mental model: "swipe toward delete = remove, swipe toward restore = recover."

**Alternatives Considered**:
- **Reuse BookListPage gesture mapping**: Inconsistent with trash semantics; edit is irrelevant in read-only trash view
- **Single-direction swipe only**: Reduces discoverability of both actions

### Decision 5: Search and Sort Reuse Strategy

**Rationale**: The spec requires trash list to support searching and sorting "like the book list does." Clarification confirmed: match existing book list exactly.

**Chosen Approach**: Reuse the search/sort infrastructure from the Library feature:
- `SortCriterion` enum (Title, Author, Publisher, PublishDate) — reused as-is
- `SortDirection` enum (Ascending, Descending) — reused as-is
- Search logic uses `EF.Functions.Like` for SQL LIKE pattern matching across Title, Author, Publisher, ISBN
- The TrashService wraps the same query patterns but targets soft-deleted items (`DeletedAt != null`)

**Alternatives Considered**:
- **Abstract base class for search/sort**: Adds indirection; direct reuse is simpler and matches Constitution Principle II (vertical slice independence)
- **Shared service layer**: Requires cross-feature dependency; vertical slices prefer co-located code

## Open Questions Deferred to Planning

None — all technical unknowns resolved above.
