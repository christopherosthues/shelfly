# Feature Specification: Edit Book from Details Page

**Feature Branch**: `004-edit-book-from-details`

**Created**: 2026-08-25

**Status**: Draft

**Input**: User description: "The user should be able to also edit the book from the details page via an edit button to open the edit page."

## Clarifications

### Session 2026-08-25

- Q: What happens when the user navigates away from the edit page without saving? → A: Edited fields ARE discarded and do NOT remain available for re-editing.
- Q: How should save failures be handled? → A: Caught, wrapped in Result pattern, error message displayed to user; app MUST NOT crash.
- Q: Does the edit page load existing book data when navigating from details? → A: Yes — edit page must load the book like the detail page does via async service call (currently not implemented).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Edit Book from Details Page (Priority: P1)

While viewing a book's details, the user taps an edit button on the details page and is taken to the book editing form pre-populated with the current book's information. The user can modify any field and save changes.

**Why this priority**: This provides a natural, discoverable entry point for editing books directly from where users review book information, reducing navigation friction compared to the existing swipe-to-edit gesture on the list page.

**Independent Test**: Can be fully tested by navigating to any book's details page, tapping the edit button, verifying the edit form appears with correct data, making a change, saving, and confirming the updated data persists and displays correctly on return to the details page.

**Acceptance Scenarios**:

1. **Given** the user is viewing a book's details page, **When** they tap the edit button, **Then** the book editing form opens with all current book fields pre-populated
2. **Given** the user has edited one or more book fields on the edit form, **When** they save changes and return to the details page, **Then** the updated field values are displayed in the book details

---

### Edge Cases

- When the user navigates away from the edit page without saving, the edited fields ARE discarded and do NOT remain available for re-editing.
- Save failures are caught and wrapped in a Result pattern; an appropriate error message is displayed to the user.
- The app MUST NOT crash on save failure — the user remains on the edit form with data preserved.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display an edit button on the book details page that is visible and tappable at all times for any loaded book
- **FR-002**: System MUST navigate to the book editing form when the user taps the edit button, passing the current book's identifier
- **FR-003**: Users MUST be able to modify any editable book field (title, author, ISBN, publisher, publish date) on the editing form
- **FR-004**: System MUST persist saved changes and reflect them immediately upon returning to the details page
- **FR-005**: System MUST validate all edited fields using existing validation rules before saving
- **FR-006**: When the user navigates away from the edit form without saving, the edited field values ARE discarded and do NOT remain available for re-editing
- **FR-007**: Save failures MUST be caught and wrapped in a Result pattern with an appropriate error message displayed to the user
- **FR-008**: The app MUST NOT crash on save failure; the user remains on the edit form with data preserved
- **FR-009**: When navigating to the edit page for an existing book, the system MUST load current book data asynchronously and pre-populate all form fields before displaying the editing interface

### Key Entities *(include if feature involves data)*

- **Book**: Represents a physical book record with editable attributes (title, author, ISBN, publisher, publish date). The book entity already exists and is managed by the BookEditPage.
- **Navigation Context**: The relationship between the details page and edit page — the edit page receives the book identifier to load existing data for editing.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can navigate from a book's details page to its edit form in a single tap
- **SC-002**: 95% of users successfully complete an edit operation on first attempt without errors
- **SC-003**: Edited book data is reflected on the details page within one second after save completes
- **SC-004**: Save failures display an error message to the user within two seconds, with no app crash

## Assumptions

- The existing book editing form (BookEditPage) and its validation logic are reused as-is for this feature
- BookEditViewModel must be refactored to inherit from ShelflyViewModelBase, implement IQueryAttributable, and override LoadAsync to load existing book data — matching the pattern used by BookDetailViewModel
- Users have stable connectivity to the API when saving changes
- The edit button will be placed in a standard, discoverable location consistent with other action buttons on the details page (e.g., toolbar or inline button)
- No new backend endpoints are required; existing book update functionality is sufficient
