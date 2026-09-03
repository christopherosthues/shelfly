# Feature Specification: Trash Management

**Feature Branch**: `[001-trash-management]`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "Trash feature: THe user should be able to select via a flyout menu the library and the trash. the trash displays all soft deleted items. There should be an option to delete all soft deleted entries as a toolbar item. The user should be able to select one or multiple items via long-press and get an additional option for delete selected entries. the user should also be able to restore soft deleted items. also for all items and one or multiple selected items. Additionally to the toolbar items he should also be able to do it with left and right swipes. swipe from left to right (right item) for delete and right to left (left item) for restore. THe user should be able via a click on the item to open a details page which cannot be edited. For the bookmarks he should only be able to display the note, nothing else."

## Clarifications

### Session 2026-09-02

- Q: Which fields should be searchable in the trash view, and which sort criteria should be available? → A: Match existing book list exactly (defer to implementation)
- Q: How should the flyout menu for switching between Library and Trash be implemented? → A: Use Shell flyout


## User Scenarios & Testing *(mandatory)*

### User Story 1 - Navigate between Library and Trash (Priority: P1)

The user can switch between viewing their active library and viewing soft-deleted items in trash using a flyout menu selection.

**Why this priority**: Core navigation prerequisite — without switching to the trash view, no other trash functionality is accessible.

**Independent Test**: Can be fully tested by selecting "Trash" from the flyout menu and verifying that only soft-deleted items appear, while selecting "Library" shows active items.

**Acceptance Scenarios**:

1. **Given** the user is viewing the library, **When** they select "Trash" from the flyout menu, **Then** the trash view displays all soft-deleted books and bookmarks
2. **Given** the user is viewing the trash, **When** they select "Library" from the flyout menu, **Then** the library view displays only active (non-deleted) items
3. **Given** the trash contains no soft-deleted items, **When** the user navigates to trash, **Then** an empty state is displayed

---

### User Story 2 - View Read-Only Item Details in Trash (Priority: P1)

The user can tap a soft-deleted item in the trash to view its read-only details. Books show full book information; bookmarks display only the note content.

**Why this priority**: Essential for users to identify items before deciding to restore or permanently delete them.

**Independent Test**: Can be fully tested by tapping an item in trash and verifying that all fields are displayed but not editable, with bookmarks showing only the note field.

**Acceptance Scenarios**:

1. **Given** a soft-deleted book is in the trash, **When** the user taps it, **Then** a read-only details page opens showing all book information
2. **Given** a soft-deleted bookmark is in the trash, **When** the user taps it, **Then** a read-only details page opens showing only the note content
3. **Given** the user is viewing item details, **When** they attempt to edit any field, **Then** the field remains unchanged

---

### User Story 3 - Restore Individual Items from Trash (Priority: P2)

The user can restore a soft-deleted item back to the active library by swiping right-to-left on the item or selecting it and choosing "Restore."

**Why this priority**: Primary recovery action — allows users to undo accidental deletions.

**Independent Test**: Can be fully tested by swiping an item leftward (or using selection + restore command) and verifying the item reappears in the library with its `DeletedAt` timestamp cleared.

**Acceptance Scenarios**:

1. **Given** a soft-deleted book is in the trash, **When** the user swipes right-to-left on it, **Then** the book is restored to the active library
2. **Given** a soft-deleted bookmark is in the trash, **When** the user selects it and chooses "Restore," **Then** the bookmark is restored to its parent book's bookmarks
3. **Given** an item has been restored, **When** the user navigates to the library, **Then** the restored item appears as active

---

### User Story 4 - Permanently Delete Items from Trash (Priority: P2)

The user can permanently delete a soft-deleted item by swiping left-to-right on the item or selecting it and choosing "Delete."

**Why this priority**: Primary cleanup action — allows users to reclaim storage space for items no longer needed.

**Independent Test**: Can be fully tested by swiping an item rightward (or using selection + delete command) and verifying the item is physically removed from storage.

**Acceptance Scenarios**:

1. **Given** a soft-deleted book is in the trash, **When** the user swipes left-to-right on it, **Then** the book is permanently deleted
2. **Given** a soft-deleted bookmark is in the trash, **When** the user selects it and chooses "Delete," **Then** the bookmark is permanently removed from storage
3. **Given** a book has been permanently deleted, **When** its child bookmarks are checked, **Then** all dependent bookmarks are also permanently deleted

---

### User Story 5 - Multi-Item Selection in Trash (Priority: P3)

The user can long-press items to enter selection mode, then select one or multiple soft-deleted items for batch restore or batch delete operations.

**Why this priority**: Efficiency enhancement — reduces repetitive actions when managing multiple trash entries.

**Independent Test**: Can be fully tested by long- pressing an item, selecting additional items, and executing a batch action to verify all selected items are affected.

**Acceptance Scenarios**:

1. **Given** the user is in selection mode via long-press, **When** they tap multiple items, **Then** all tapped items become visually selected
2. **Given** multiple items are selected, **When** the user chooses "Restore Selected," **Then** all selected items are restored to the active library
3. **Given** multiple items are selected, **When** the user chooses "Delete Selected," **Then** all selected items are permanently removed

---

### User Story 6 - Bulk Operations on All Trash Items (Priority: P3)

The user can restore all soft-deleted items or delete all soft-deleted entries via a single toolbar action.

**Why this priority**: Convenience for full trash management — useful when the user wants to reset the entire trash state.

**Independent Test**: Can be fully tested by tapping "Restore All" or "Delete All" in the toolbar and verifying all trash items are affected accordingly.

**Acceptance Scenarios**:

1. **Given** the trash contains multiple soft-deleted items, **When** the user taps "Restore All," **Then** all items are restored to their respective active locations
2. **Given** the trash contains multiple soft-deleted items, **When** the user taps "Delete All," **Then** all items and their dependents are permanently removed
3. **Given** the trash is empty, **When** the user taps a bulk action, **Then** a confirmation or no-op message is shown

---

### Edge Cases

- What happens when a soft-deleted book's parent category was also soft-deleted?
- How does the system handle restoring a bookmark whose parent book was permanently deleted?
- What occurs when the user swipes an item mid-animation then navigates away?
- How is selection state preserved during orientation changes or app backgrounding?
- What happens when search returns no results in the trash view?
- Does sorting persist across navigation away and back to the trash view?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST use Shell flyout navigation to allow the user to switch between Library view (active items) and Trash view (soft-deleted items)
- **FR-002**: The trash view MUST display all soft-deleted books and bookmarks, grouped by entity type
- **FR-003**: The system MUST allow the user to permanently delete a single soft-deleted item via swipe gesture (left-to-right on the item) or toolbar action
- **FR-004**: The system MUST allow the user to restore a single soft-deleted item via swipe gesture (right-to-left on the item) or toolbar action
- **FR-005**: The system MUST support multi-item selection via long-press, enabling batch restore and batch delete operations
- **FR-006**: The system MUST provide toolbar actions to restore all trash items and delete all trash items simultaneously
- **FR-007**: The system MUST display a read-only details page when the user taps an item in the trash
- **FR-008**: Bookmark details in trash MUST display only the note content, excluding other bookmark properties
- **FR-009**: When a book is permanently deleted from trash, all dependent bookmarks of that book MUST also be permanently deleted (cascade)
- **FR-010**: The system MUST visually distinguish selection mode with clear indicators for selected items
- **FR-011**: The trash view MUST support searching and sorting with the same fields and criteria as the book list

### Key Entities

- **Soft-Deleted Book**: A book record marked as soft-deleted via a `DeletedAt` timestamp; appears in trash until restored or permanently removed
- **Soft-Deleted Bookmark**: A bookmark record associated with a soft-deleted parent book; follows the same lifecycle as its parent
- **Trash View**: The collection of all soft-deleted items available for restoration or permanent deletion
- **Selection State**: Temporary UI state tracking which items are selected for batch operations

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate from library to trash and back in under 2 seconds
- **SC-002**: Users can restore or permanently delete a single item with one gesture or tap action
- **SC-003**: Batch operations on up to 50 selected items complete within 3 seconds
- **SC-004**: 95% of users successfully identify whether an item is soft-deleted or active based on visual cues alone
- **SC-005**: Users can distinguish between restore and delete swipe directions without error after a single use
- **SC-006**: Search results in the trash view appear within 1 second of entering query text
- **SC-007**: Sort order changes apply to the visible list within 500 milliseconds

## Assumptions

- Soft deletion uses a nullable `DeletedAt` timestamp where `null` indicates an active item and non-null indicates soft-deleted
- Shell flyout navigation is used for switching between Library and Trash views; this feature extends the existing Shell configuration with a Trash route
- Swipe gestures are available on all target platforms (Android, iOS/MacCatalyst, Windows) via the MAUI framework
- Bookmarks inherit the deletion state of their parent book — a soft-deleted book's bookmarks appear in trash alongside it
- The read-only details page reuses existing detail page infrastructure with edit controls disabled
