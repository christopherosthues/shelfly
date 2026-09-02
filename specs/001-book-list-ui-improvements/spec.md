# Feature Specification: Book List UI Improvements

**Feature Branch**: `001-book-list-ui-improvements`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "The sort options should be localized also for the picker. Also add a correct title to the book list page. The sorting should support ascending and descending."

## Clarifications

### Session 2026-09-02

- Q: How should the system persist the user's sort direction preference across page reloads and app restarts? → A: Session-only (in-memory via ViewModel; resets on app restart, survives page navigation)
- Q: What visual indicator should the UI use to show whether the current sort order is ascending or descending? → A: Toggle arrow icon (small ↑/↓ icon next to the picker; tapping reverses direction)
- Q: Should each sort criterion have its own default direction or should all criteria uniformly default to ascending? → A: Uniform ascending — all criteria default to ascending regardless of field type

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Correct Page Title Display (Priority: P1)

When viewing the book list, the user sees a meaningful page title that identifies the purpose of the screen, rather than seeing "Title" (which is currently the sort picker's label masquerading as the page title).

**Why this priority**: The page title is the primary identifier for the screen. Without it, users are confused about what they're viewing, especially in navigation history and device status bars.

**Independent Test**: [Can be tested by opening the book list page and verifying the displayed title reads "My Library" or equivalent localized text]

**Acceptance Scenarios**:

1. **Given** the user opens the book list page, **When** the page loads, **Then** the page title displays a meaningful name (e.g., "My Library")
2. **Given** the device language is set to German, **When** the user views the book list page, **Then** the page title displays the localized equivalent

---

### User Story 2 - Localized Sort Options in Picker (Priority: P1)

When using the sort picker on the book list page, all sort option values displayed in the picker are properly localized to match the device language. Currently, the enum values (Title, Author, Publisher, Publish Date) appear as raw English text regardless of locale.

**Why this priority**: Non-English speaking users see inconsistent language mixing — localized UI elements mixed with hardcoded English enum names in the picker.

**Independent Test**: [Can be tested by changing device language and verifying all sort options display in the correct language]

**Acceptance Scenarios**:

1. **Given** the device language is set to German, **When** the user opens the sort picker, **Then** all sort options display their localized names
2. **Given** the device language is set to English, **When** the user opens the sort picker, **Then** all sort options display correctly in English

---

### User Story 3 - Ascending and Descending Sort Direction (Priority: P2)

After selecting a sort criterion from the picker, the user can toggle between ascending and descending order for that criterion. The current implementation only sorts in one direction per criterion.

**Why this priority**: Users have different preferences — some want newest first, others oldest first; some prefer A-Z, others Z-A. This flexibility significantly improves usability.

**Independent Test**: [Can be tested by selecting a sort option and verifying the list order can be reversed]

**Acceptance Scenarios**:

1. **Given** books are sorted by title ascending (A to Z), **When** the user toggles sort direction, **Then** the list reorders to descending (Z to A)
2. **Given** books are sorted by publish date descending (newest first), **When** the user toggles sort direction, **Then** the list reorders to ascending (oldest first)
3. **Given** the user changes sort criterion while in descending mode, **When** the new criterion is selected, **Then** the sort direction persists as descending for the new criterion

---

### Edge Cases

- What happens when the user toggles sort direction on an empty book list?
- Sort direction persists in-memory during session; what is the state on app restart? (Answer: resets to default ascending)
- Default sort direction on first load: ascending for all criteria

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The book list page MUST display a meaningful, localized page title that identifies the screen purpose
- **FR-002**: All sort option values displayed in the picker MUST be fully localized to match the device language
- **FR-003**: Users MUST be able to toggle between ascending and descending order for any selected sort criterion
- **FR-004**: The system MUST maintain the user's current sort direction preference in-memory during the session (survives page navigation, resets on app restart)
- **FR-005**: The UI MUST display a toggle arrow icon (↑/↓) adjacent to the sort picker that indicates and toggles between ascending and descending order on tap

### Key Entities *(include if feature involves data)*

- **Sort Criterion**: Represents a sortable field on books (Title, Author, Publisher, Publish Date). Each criterion must support both ascending and descending order
- **Page Title**: A localized string resource identifying the book list page purpose

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of UI text on the book list page is properly localized across all supported languages
- **SC-002**: Users can reverse sort order with a single interaction (tap/click)
- **SC-003**: The page title is immediately recognizable as identifying the user's personal library

## Assumptions

- The existing localization infrastructure (AppResources.resx and .de.resx files) is used for all UI text
- All sort criteria uniformly default to ascending order regardless of field type (text or date)
- Visual indicators for sort direction use a toggle arrow icon (↑/↓) adjacent to the sort picker
- The feature scope is limited to the book list page; other list views are not affected
