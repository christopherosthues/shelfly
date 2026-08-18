# Feature Specification: Local Library Management

**Feature Branch**: `002-local-library`

**Created**: 2026-08-18

**Status**: Draft

**Input**: User description: "Feature Book and bookmark management: The user should be able to manage books and bookmarks locally in a client app without a profile or account."

## Clarifications

### Session 2026-08-18

- Q: Input validation rules for book fields → A: Author, Title, and Publisher must not be empty or whitespace; ISBN must follow valid format (ISBN-10 or ISBN-13)
- Q: Localization support → A: App supports German and English languages
- Q: Note indicator icon interaction → A: Clicking the note indicator icon displays the associated note
- Q: Target platforms for the client app → A: Android, iOS, and Desktop (Windows, MacOS) using .NET MAUI
- Q: Maximum length for bookmark notes → A: At most 1000 characters
- Q: How should the book search handle partial matches and case sensitivity when filtering the library? → A: Case-insensitive substring matching
- Q: How should swipe-to-delete gestures work on desktop platforms where touch input may not be available? → A: Platform-native drag/swipe equivalent (default .NET MAUI functionality)
- Q: What data storage technology is used for local persistence? → A: Local SQLite database with EF Core
- Q: Which ISBN formats are accepted by the system? → A: Both ISBN-10 and ISBN-13 formats
- Q: How does the system handle end page lower than start page in a bookmark range? → A: Validation error is displayed to the user
- Q: Are overlapping bookmarks (same page referenced by multiple bookmarks) allowed for a single book? → A: Overlapping bookmarks are allowed; users may add separate notes for different sections of the same page
- Q: How does the system handle duplicate ISBNs across books? → A: Since ISBNs are unique, a validation error is displayed when adding or editing a book with an existing ISBN (duplicates not allowed)
- Q: What happens when a user swipes to delete a book? → A: The book is always soft-deleted; the associated bookmarks remain in storage (soft delete anyway)
- Q: What are the maximum field lengths for book and bookmark data? → A: Notes max 1000 characters, titles/authors/publishers max 256 characters, ISBN follows standard format including dashes
- Q: What happens when a field exceeds its maximum length during editing? → A: Display validation error on the text field (using .NET MAUI equivalent of Android supporting text)
- Q: Which validation error message appears first when multiple fields fail simultaneously? → A: Display each error inline on the respective text field (using .NET MAUI equivalent of Android supporting text)
- Q: How does the app handle language switching at runtime without data loss? → A: Let .NET MAUI handle it natively
- Q: What happens when a note exceeds 1000 characters during editing? → A: Display validation error on the note field
- Q: How does the bookmark list display overlapping bookmarks (same page referenced by multiple notes)? → A: Separate entries; same page displays range first, then single pages
- Q: Where else should book deletion be possible besides swipe-to-delete? → A: Deletion of a book should also be possible from the detail view
- Q: Should audit values (created at, last modified at) be tracked for Book entities? → A: Yes; CreatedAt is non-nullable DateTime (always set via EF Core interceptor on creation); LastModifiedAt is nullable DateTime (set via interceptor on update)
- Q: Should the app support exporting the local library data (e.g., to JSON or CSV) for backup purposes, and importing books from external sources? → A: Export only (JSON format recommended) — allows backup but no import
- Q: What is the maximum number of books a user can store in their local library before the system displays a capacity warning or enforces limits? → A: No explicit limit — rely on device storage capacity (practical limit ~10,000 books)
- Q: When a soft-deleted book is restored from trash, should its associated bookmarks be automatically restored as well? → A: Yes — restoring a book automatically restores all its soft-deleted bookmarks
- Q: Which Guid version should be used for entity identifiers? → A: Guid version 7 (Guid.CreateVersion7())
- Q: Should audit values (created at, last modified at) also be tracked for Bookmark entities similar to Book entities? → A: Yes; CreatedAt is non-nullable DateTime (always set via EF Core interceptor on creation); LastModifiedAt is nullable DateTime (set via interceptor on update)
- Q: How should loading states be communicated to the user during data operations? → A: Loading indicators displayed when data is being loaded and hidden when loading is finished
- Q: Which error handling pattern should be used instead of custom exceptions? → A: Result pattern (no custom/domain-specific exceptions thrown)
- Q: Should async/await be used for asynchronous operations? → A: Use async await whenever possible and reasonable
- Q: How should database errors be handled during data operations? → A: Database errors are caught, logged via structured logging (NLog), and a toast with an error message is displayed to the user
- Q: What style of validation messages should be used across the app? → A: Consistent, specific validation messages displayed inline on each failing field
- Q: Is restoring soft-deleted books from trash in scope for this feature? → A: Out of scope — soft delete hides books but no restore mechanism is implemented yet
- Q: What is the fallback language when device system language is not German or English? → A: English (default by .NET MAUI)
- Q: Should ISBNs of soft-deleted books be reusable for new books before hard deletion occurs? → A: No — ISBN uniqueness extends to soft-deleted books; validation error if ISBN matches a soft-deleted book (trash management out of scope)
- Q: Are special characters allowed in bookmark notes? → A: Yes — special characters are accepted without restriction in note content
- Q: How should database schema changes be managed across app versions? → A: EF Core migrations with automatic migration on app start
- Q: Do cascade delete and soft delete collide with the constitution's hard delete principle? → A: No — soft delete is first step (in scope); hard delete with cascade delete belongs to trash management (out of scope) where user or background service permanently removes a soft-deleted book after retention period
- Q: How should accessibility be implemented in the app? → A: Semantic properties from .NET MAUI for screen reader support and accessibility features

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View and Browse Book List (Priority: P1)

The user opens the application and sees a list of all their added books. Each book displays its title, author, and publisher. If no books exist, the user sees an empty state message prompting them to add a book first. The user can search through the list by typing keywords that match title, author, publisher, or ISBN. The user can sort the list by title, author, publisher, or publish date.

**Why this priority**: This is the primary entry point and core navigation experience. Without it, the user cannot discover or access any other functionality.

**Independent Test**: Can be fully tested by opening the app, verifying the book list displays correctly with all required information, searching returns matching results, sorting reorders items correctly, and empty state shows when no books exist.

**Acceptance Scenarios**:

1. **Given** the user has added at least one book, **When** they open the app, **Then** they see a list showing each book's title, author, and publisher
2. **Given** no books have been added, **When** the user opens the app, **Then** they see an empty state message prompting them to add a book
3. **Given** the user has multiple books, **When** they type a search query matching any book's title, author, publisher, or ISBN, **Then** only matching books appear in the list
4. **Given** the user has multiple books, **When** they select a sort criterion (title, author, publisher, or publish date), **Then** the list reorders according to the selected field
5. **Given** the user is viewing the book list, **When** they swipe left on a book item, **Then** the book is soft-deleted and removed from the visible list

---

### User Story 2 - Add and Edit Books (Priority: P1)

The user navigates to an add or edit screen for a book. They fill in text fields for title, author, publisher, and ISBN, and select a publish date using a date selector. Upon saving, the new or updated book appears in the book list with all provided details.

**Why this priority**: Essential data entry flow that populates the library. Without it, the user cannot add any content to manage.

**Independent Test**: Can be fully tested by navigating to the add/edit screen, entering valid data for all fields, saving, and verifying the book appears in the list with correct details. Editing an existing book and verifying changes persist also validates this story.

**Acceptance Scenarios**:

1. **Given** the user is on the add book screen, **When** they enter title, author, publisher, ISBN, and publish date, then save, **Then** a new book appears in the library with all entered details
2. **Given** a book exists in the library, **When** the user opens edit mode for that book and modifies one or more fields, then saves, **Then** the book reflects the updated values
3. **Given** the user is on the add/edit book screen, **When** they leave Title, Author, or Publisher empty or whitespace-only and attempt to save, **Then** validation feedback indicates each field requires a non-empty value
4. **Given** the user is on the add/edit book screen, **When** they enter an ISBN in invalid format (not ISBN-10 or ISBN-13) and attempt to save, **Then** validation feedback indicates the ISBN format is incorrect

---

### User Story 3 - Manage Bookmarks within a Book (Priority: P2)

The user opens a specific book's detail view showing all book details and its associated bookmarks. Each bookmark displays the page number or page range, an optional icon indicating a note exists, an edit icon, and a delete icon. The user can add new bookmarks by specifying a single page or page range (start and end page) along with an optional short note. The user can edit existing bookmarks to modify pages or notes, and delete bookmarks individually.

**Why this priority**: Core value proposition — bookmarking passages is the primary reason users interact with their library after adding books.

**Independent Test**: Can be fully tested by opening a book detail view, verifying all book details display correctly, adding a new bookmark with page range and note, editing an existing bookmark, deleting a bookmark, and confirming each action updates the bookmark list accordingly.

**Acceptance Scenarios**:

1. **Given** a book exists with at least one bookmark, **When** the user opens the book detail view, **Then** they see all book details and a list of bookmarks showing page/range information
2. **Given** a bookmark has an associated note, **When** viewing the bookmark in the list, **Then** an icon indicates that a note exists for this bookmark
3. **Given** the user is on a book detail view, **When** they tap the add button and enter a single page number with an optional note, then save, **Then** the new bookmark appears in the bookmark list
4. **Given** the user is adding a bookmark, **When** they specify both a start page and end page (page range) along with an optional note, then save, **Then** the bookmark displays the full page range
5. **Given** a bookmark exists, **When** the user taps the edit icon and modifies the page(s) or note, then saves, **Then** the bookmark reflects the updated values
6. **Given** a bookmark exists, **When** the user taps the delete icon, **Then** the bookmark is removed from the list
7. **Given** a bookmark has an associated note and displays a note indicator icon, **When** the user clicks the note indicator icon, **Then** the note content is displayed to the user
8. **Given** the user is adding or editing a bookmark with a page range, **When** they enter an end page lower than the start page and attempt to save, **Then** a validation error is displayed indicating the end page must be greater than or equal to the start page
9. **Given** the user is viewing a book detail view, **When** they tap the delete button in the detail view, **Then** the book (including all associated bookmarks) is soft-deleted and removed from the visible list

---

### Edge Cases

- When end page is lower than start page, a validation error is displayed to the user
- Duplicate ISBNs trigger a validation error (ISBN uniqueness enforced across all books including soft-deleted ones)
- When adding or editing a book, the ISBN is checked against both active and soft-deleted books; if a match exists, a validation error is displayed
- When a field exceeds its maximum length during editing, a validation error is displayed inline on the text field
- What happens when the user swipes to delete a book that has existing bookmarks? → The book is soft-deleted; associated bookmarks remain in storage and are hidden from view
- Search uses case-insensitive substring matching (e.g., "harry" finds "Harry Potter")
- When multiple fields fail validation simultaneously, each error is displayed inline on the respective text field
- Notes have a maximum length of 1000 characters; titles/authors/publishers max 256 characters; ISBN follows standard format including dashes
- Language switching at runtime is handled natively by .NET MAUI without data loss
- When a note exceeds 1000 characters during editing, a validation error is displayed on the note field
- Desktop platforms use platform-native drag/swipe equivalent for soft deletion
- Overlapping bookmarks (same page referenced by multiple notes) appear as separate entries; same page displays range first, then single pages
- Loading indicators are displayed during data operations and hidden when loading completes
- When a database error occurs, the error is caught, logged via structured logging (NLog), and a toast with an error message is displayed to the user
- Validation messages are consistent and specific across all input fields; each failing field displays its own inline error message using standardized wording
- Bookmark notes accept special characters without restriction (no character filtering applied)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow users to add a new book with title, author, publisher, ISBN, and publish date
- **FR-002**: System MUST display all added books in a list showing title, author, and publisher
- **FR-003**: System MUST show an empty state message when no books exist, prompting the user to add a book
- **FR-004**: Users MUST be able to search the book list by title, author, publisher, or ISBN using case-insensitive substring matching
- **FR-005**: Users MUST be able to sort the book list by title, author, publisher, or publish date
- **FR-006**: System MUST support swipe-to-delete (left swipe gesture) on book list items for soft deletion
- **FR-007**: System MUST allow users to edit an existing book's details (title, author, publisher, ISBN, publish date)
- **FR-008**: System MUST display full book details when the user opens a book detail view
- **FR-009**: System MUST display all bookmarks associated with a book in the book detail view
- **FR-010**: Each bookmark entry MUST show page number or page range (start and end page)
- **FR-011**: Bookmarks with notes MUST display an icon indicating note presence
- **FR-012**: Each bookmark entry MUST provide edit and delete icons for quick actions
- **FR-013**: Users MUST be able to add a new bookmark specifying either a single page or a page range (start and end)
- **FR-014**: Users MUST be able to attach an optional short note to a bookmark
- **FR-015**: System MUST allow users to edit an existing bookmark's page(s) and note
- **FR-016**: System MUST allow users to delete individual bookmarks
- **FR-017**: System MUST validate that Title, Author, and Publisher fields are not empty or whitespace-only when adding or editing a book
- **FR-018**: System MUST accept both ISBN-10 and ISBN-13 formats (including dashes) and validate the entered ISBN against the corresponding standard conventions when adding or editing a book
- **FR-025**: System MUST enforce ISBN uniqueness across all books — displaying a validation error when a duplicate ISBN is detected during add or edit operations
- **FR-026**: System MUST enforce maximum field lengths: notes up to 1000 characters, titles/authors/publishers up to 256 characters each
- **FR-019**: System MUST display all UI text in the user's selected language (German or English)
- **FR-020**: Clicking the note indicator icon on a bookmark entry MUST display the associated note content
- **FR-021**: System MUST enforce a maximum note length of 1000 characters when adding or editing a bookmark
- **FR-022**: System MUST provide a consistent user experience across Android, iOS, Windows, and MacOS platforms
- **FR-023**: System MUST display a validation error when the end page of a bookmark range is lower than the start page
- **FR-024**: System MUST allow multiple bookmarks to reference the same page(s) within a single book (overlapping pages are permitted)
- **FR-027**: When a field exceeds its maximum length, the validation error MUST be displayed inline on the text field using .NET MAUI's equivalent of Android supporting text
- **FR-028**: When multiple fields fail validation simultaneously, each error MUST be displayed inline on the respective text field
- **FR-029**: The bookmark list MUST display overlapping bookmarks as separate entries; when referencing the same page, range-based bookmarks appear first, followed by single-page bookmarks
- **FR-030**: System MUST provide a delete button in the book detail view that soft-deletes the entire book (including all associated bookmarks)
- **FR-031**: System MUST allow users to export their local library data as JSON for backup purposes
- **FR-032**: System MUST display loading indicators when data is being loaded and hide them when loading is finished

### Key Entities *(include if feature involves data)*

- **Book**: Represents a physical book in the user's library. Attributes include title, author, ISBN (unique across all books), publisher, publish date, an associated list of bookmarks, and audit timestamps (CreatedAt — non-nullable, always set via interceptor; LastModifiedAt — nullable). A soft-deleted book remains in storage but is hidden from the visible list. Entity identifiers use Guid version 7 (`Guid.CreateVersion7()`).
- **Bookmark**: Represents a saved page reference within a specific book. Attributes include either a single page number or a page range (start page and end page), plus an optional short note, and audit timestamps (CreatedAt — non-nullable, always set via interceptor; LastModifiedAt — nullable). Each bookmark belongs to exactly one book. Multiple bookmarks may reference the same page(s) — overlapping pages are allowed so users can add separate notes for different sections of the same page. Entity identifiers use Guid version 7 (`Guid.CreateVersion7()`).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can add a new book in under 30 seconds
- **SC-002**: Search results appear within 500 milliseconds of typing a query
- **SC-003**: Users can navigate from the book list to a specific book's detail view in no more than two taps
- **SC-004**: Users can add a new bookmark in under 15 seconds
- **SC-005**: Swipe-to-delete gesture completes within 200 milliseconds and provides visual feedback

## Assumptions

- The client app targets Android, iOS, and Desktop (Windows, MacOS) using .NET MAUI; swipe gestures are supported on all platforms
- Data persists locally on the device using SQLite with EF Core; database schema changes managed via EF Core migrations
- Database is automatically migrated to latest schema on app start (EF Core migration applied at startup)
- Entity identifiers (Book and Bookmark) use Guid version 7 (`Guid.CreateVersion7()`) for time-ordered generation
- The local library has no explicit book count limit; practical capacity is bounded by device storage (~10,000 books)
- ISBN validation follows standard ISBN-10 or ISBN-13 format conventions
- Page numbers are positive integers; page ranges require start page less than or equal to end page
- Soft-deleted books remain in local storage but are hidden from the visible list; ISBN uniqueness extends to soft-deleted books (ISBN cannot be reused until hard deletion)
- Trash management for restoration or hard deletion of soft-deleted books is out of scope for this feature
- This feature performs only soft delete first (hides book from view); hard delete with cascade delete belongs to trash management where the user manually hard deletes a soft-deleted book or a background service hard deletes after a retention period — both scenarios are out of scope
- When a book with bookmarks is swiped-to-delete, the entire book (including all associated bookmarks) is soft-deleted as a unit; no individual bookmark deletion occurs during swipe
- Notes have a maximum length of 1000 characters
- Title, Author, and Publisher fields have a maximum length of 256 characters each
- ISBN follows standard format including dashes (ISBN-10 or ISBN-13)
- Swipe-to-delete always performs a soft delete; the book remains in storage but is hidden from the visible list
- Physical deletion (hard delete) with cascade delete of associated bookmarks is part of trash management and out of scope for this feature; the constitution's hard delete principle applies to trash management scenarios where a user or background service permanently removes a soft-deleted book
- The app defaults to the device's system language if German or English is available; otherwise falls back to English (default by .NET MAUI)
- All UI strings, labels, messages, and validation feedback are fully translatable between German and English
- Language switching at runtime is handled natively by .NET MAUI without data loss
- Asynchronous operations use async/await pattern whenever possible and reasonable (per constitution Principle IV)
- Error handling uses Result pattern instead of custom/domain-specific exceptions (per constitution Principle IV)
- Database errors are caught, logged via structured logging (NLog per constitution Logging & Observability section), and a toast with an error message is displayed to the user
- Accessibility implemented using semantic properties from .NET MAUI for screen reader support and accessibility features
