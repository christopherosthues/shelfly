# Feature Specification: Book Card Info & Sorting Enhancements

**Feature Branch**: `[012-book-card-info-sorting]`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "The number of bookmarks should be displayed in the list views at the top right of the card. Also the last modified date should be displayed at the bottom right corner of each card. The sorting should support createAt and lastmodifiedat dates for both list views."

## Clarifications

### Session 2026-09-04

- Q: When sorting by LastModifiedAt, where should books with no modification date (null value) appear in the list? → A: Defer to EF Core / database default null sorting behavior
- Q: For a newly created book with no modifications yet, what should be displayed in the last modified date field? → A: Display CreatedAt — creation date serves as the last modified date for unmodified books

## User Scenarios & *(mandatory)*

### User Story 1 - View Bookmark Count on Book Cards (Priority: P1)

As a reader, when browsing my library or trash, I want to immediately see how many bookmarks each book has so I can identify which books contain the most saved passages.

**Why this priority**: This is the primary visual enhancement requested — bookmark count provides at-a-glance insight into reading engagement and helps users prioritize which books to revisit.

**Independent Test**: Can be fully tested by navigating to either the library list or trash list, verifying that every book card displays a numeric bookmark count in the top right corner, and confirming the count matches the actual number of bookmarks stored for each book.

**Acceptance Scenarios**:

1. **Given** a book has zero bookmarks, **When** viewing its card in any list, **Then** the card displays "0" as the bookmark count
2. **Given** a book has one or more bookmarks, **When** viewing its card in any list, **Then** the card displays the correct numeric count at the top right of the card
3. **Given** a user adds a new bookmark to a book, **When** returning to the list view, **Then** the updated bookmark count is reflected on the card

---

### User Story 2 - View Last Modified Date on Book Cards (Priority: P1)

As a reader, I want to see when each book was last modified so I can track my recent reading activity and identify books I've been actively working with.

**Why this priority**: The last modified date complements the bookmark count by providing temporal context — together they give users a complete picture of their engagement with each book.

**Independent Test**: Can be fully tested by viewing any list (library or trash), verifying that every card displays a last modified date at the bottom right corner, and confirming the date matches the most recent modification timestamp for that book.

**Acceptance Scenarios**:

1. **Given** a book has been modified (bookmarks added/edited), **When** viewing its card in any list, **Then** the card displays the last modified date at the bottom right corner
2. **Given** a newly created book with no modifications yet (LastModifiedAt is null), **When** viewing its card, **Then** the card displays the CreatedAt value as the last modified date
3. **Given** a user edits a bookmark for a book, **When** returning to the list view, **Then** the updated last modified date is reflected on the card

---

### User Story 3 - Sort Books by Creation and Modification Dates (Priority: P2)

As a reader, I want to sort my library and trash lists by creation date or last modification date so I can quickly find recently added books or recently active reading material.

**Why this priority**: Date-based sorting extends the existing sorting capabilities (title, author, publisher, publish date) with temporal dimensions that are critical for managing a growing library.

**Independent Test**: Can be fully tested by opening either list view, selecting "Created At" or "Last Modified At" from the sort picker, toggling ascending/descending direction, and verifying the book order changes correctly based on the selected criterion.

**Acceptance Scenarios**:

1. **Given** a library with multiple books created on different dates, **When** sorting by creation date (ascending), **Then** books are ordered from oldest to newest
2. **Given** a library with multiple books modified on different dates, **When** sorting by last modification date (descending), **Then** books are ordered from most recently modified to least recently modified
3. **Given** the sort direction toggle is pressed, **When** switching between ascending and descending, **Then** the list order reverses accordingly

---

### Edge Cases

- When LastModifiedAt is null, CreatedAt is displayed as the fallback value
- How does the system handle books with identical creation or modification timestamps?
- What is displayed for bookmark count when network data is stale during offline mode?
- Null LastModifiedAt values follow EF Core / PostgreSQL default: nulls first in ascending, nulls last in descending

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display the total number of bookmarks associated with each book on every book card in both library and trash list views
- **FR-002**: The bookmark count MUST be positioned at the top right corner of each book card
- **FR-003**: System MUST display the last modified date for each book on every book card in both library and trash list views; when LastModifiedAt is null, display CreatedAt as a fallback
- **FR-004**: The last modified date MUST be positioned at the bottom right corner of each book card
- **FR-005**: Both library and trash list views MUST support sorting by creation date (CreatedAt)
- **FR-006**: Both library and trash list views MUST support sorting by last modification date (LastModifiedAt)
- **FR-007**: The sort picker in both list views MUST include the new date-based sorting options alongside existing criteria
- **FR-008**: System MUST defer to EF Core / database default null sorting behavior for LastModifiedAt (PostgreSQL: nulls first in ascending, nulls last in descending)
- **FR-009**: Bookmark count and last modified date MUST update automatically when bookmarks are added, edited, or deleted

### Key Entities *(include if feature involves data)*

- **Book**: Represents a physical book in the user's library. Relevant attributes include Id, Title, Author, CreatedAt, LastModifiedAt, and an implicit relationship to associated bookmarks
- **Bookmark**: Represents a saved page reference within a book. Each bookmark is linked to a parent Book via BookId. The count of bookmarks per book must be queryable efficiently

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can identify the bookmark count for any book at a glance without navigating to detail views
- **SC-002**: Users can determine when a book was last modified by viewing its card in list mode
- **SC-003**: Users can sort their library or trash by creation date or modification date with a single picker selection
- **SC-004**: Bookmark count and last modified date display accurately for 100% of books in the list, including edge cases (zero bookmarks, null dates)

## Assumptions

- The API already provides LastModifiedAt data as part of book entity responses; if not, a backend change is required to include this field
- Bookmark count must be computed server-side and returned with each book entity, or calculated client-side from loaded bookmark collections
- Date formatting follows the existing localization patterns using resource files (`.resx`) for consistent display across supported languages
- The feature applies to both the library list view (BookListPage) and trash list view (TrashListPage), as both use BookCardView
- When LastModifiedAt is null, CreatedAt serves as the displayed value (creation date = last modified date for unmodified books)
- Null LastModifiedAt values follow EF Core / PostgreSQL default null sorting: nulls first in ascending order, nulls last in descending order
