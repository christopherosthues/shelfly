# Feature Specification: Loading Indicators for Edit Pages

**Feature Branch**: `005-loading-edit-pages`

**Created**: 2026-08-28

**Status**: Draft

**Input**: User description: "Add loading indicators to the edit pages"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Button Feedback During Save (Priority: P1)

When a user taps the save button on either the book editor or bookmark editor, the button immediately transforms into an inline loading indicator and becomes disabled. The indicator remains visible until the save completes and the page navigates away.

**Why this priority**: Provides immediate confirmation to the user that their action was received at the exact interaction point, preventing double-tap submissions during network latency or slow server response. This is the most frequent interaction on edit pages.

**Independent Test**: Can be fully tested by opening an edit page, tapping save, and verifying the button transforms into a loading indicator and becomes disabled before navigation completes. Delivers reduced perceived wait time and prevents duplicate submissions.

**Acceptance Scenarios**:

1. **Given** the user is editing a book or bookmark, **When** they tap the save button, **Then** the button immediately displays an inline loading indicator and becomes disabled
2. **Given** a save operation is in progress, **When** the server responds successfully, **Then** the button indicator disappears and navigation occurs
3. **Given** a save operation is in progress, **When** the server responds with an error, **Then** the button returns to its normal state (re-enabled) and an error message is shown

---

### User Story 2 - Full-Screen Feedback During Load (Priority: P2)

When navigating to edit an existing book or bookmark, a full-screen overlay loading indicator appears while the data is being fetched from the API. The indicator covers all content and remains visible until the form fields are populated with the retrieved data.

**Why this priority**: Ensures users understand the delay between navigation and form population, particularly important on slower networks where API response may take longer than expected. The full-screen overlay signals that the page is actively loading.

**Independent Test**: Can be fully tested by navigating to an existing book or bookmark edit page and verifying a full-screen overlay appears during data fetch and disappears when fields are populated.

**Acceptance Scenarios**:

1. **Given** the user navigates to edit an existing book, **When** the API request is sent, **Then** a full-screen overlay loading indicator becomes visible
2. **Given** a load operation is in progress for a book or bookmark, **When** the data arrives successfully, **Then** the overlay disappears and form fields are populated

---

### User Story 3 - Consistent Loading Experience Across Edit Pages (Priority: P3)

Both edit pages display loading indicators using consistent visual patterns — full-screen overlay for data loading matching the book list page style, and button-level inline indicator for save operations — creating a predictable user experience throughout the application.

**Why this priority**: Maintains UI consistency and reduces cognitive load — users recognize both loading patterns from other screens and know what to expect on edit pages.

**Independent Test**: Can be tested by comparing the full-screen overlay on edit pages against the book list page pattern, and verifying button-level indicators behave consistently between both edit pages.

**Acceptance Scenarios**:

1. **Given** the user has seen a loading indicator on the book list page, **When** they navigate to an edit page with active data loading, **Then** the full-screen overlay visual style matches the established pattern
2. **Given** both edit pages are used in sequence, **When** save operations occur on each, **Then** the button-level indicator behavior is consistent between them

---

### Edge Cases

- What happens when the save completes so quickly that the button indicator flashes briefly? — The indicator should remain visible for a minimum of 2 seconds to avoid flicker during local development.
- How does the system handle rapid successive taps on the save button? — The second tap should be ignored while the first operation is in progress due to button disabling.
- What happens if the user navigates away mid-operation? — The loading indicator and associated async operation should cancel cleanly without errors.
- How is error feedback presented after a failed save? — An error message must appear below the form fields, and the save button returns to its normal (enabled) state.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a full-screen overlay loading indicator that blocks all user input on the book edit page during data load operations (editing existing items)
- **FR-002**: System MUST display a full-screen overlay loading indicator that blocks all user input on the bookmark edit page during data load operations (editing existing items)
- **FR-003**: System MUST replace the save button with an inline loading indicator and disable the button during save operations on the book edit page
- **FR-004**: System MUST replace the save button with an inline loading indicator and disable the button during save operations on the bookmark edit page
- **FR-005**: The full-screen overlay loading indicator MUST use the same visual style as established in the book list page

### Key Entities *(include if feature involves data)*

- **BookEditPage**: XAML view that edits book metadata; currently lacks visual loading feedback despite having `IsLoading` property in ViewModel
- **BookmarkEditPage**: XAML view that edits bookmark data (page ranges, notes); currently lacks visual loading feedback despite having `IsLoading` property in ViewModel

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Full-screen overlay loading indicator appears within 100ms of initiating a data load operation
- **SC-002**: Save button transforms into an inline loading indicator, remains visible for a minimum of 2 seconds, and the button stays disabled during that period
- **SC-003**: Users perceive no ambiguity about whether their save action was received — button-level indicator provides immediate visual confirmation at the interaction point
- **SC-004**: Full-screen overlay behavior matches the book list page pattern as verified by visual inspection

## Clarifications

### Session 2026-08-28

- Q: Which loading indicator visual pattern should the edit pages use? → A: Full screen for data loading, button indicator with disabling during save
- Q: Should the full-screen overlay allow interaction behind it or block all input? → A: Block all input during load
- Q: What is the minimum display duration for the button-level loading indicator? → A: 2 seconds

## Assumptions

- The existing `IsLoading` observable property on both ViewModels will be wired to the XAML binding for visibility control
- Loading indicators are needed only during save operations for create mode (new item), since load mode has no data to fetch initially
- The minimum display duration requirement applies only where network latency is extremely low (local development); production networks naturally exceed this threshold
