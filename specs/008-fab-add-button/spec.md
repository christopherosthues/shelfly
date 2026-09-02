# Feature Specification: FAB Add Button

**Feature Branch**: `[008-fab-add-button]`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "The add button on the list page should be more like a FAB known from Android. Remove it from the toolbar and move it to the bottom right of the page where a normal FAB would be located."

## Clarifications

### Session 2026-09-02

- Q: When the on-screen keyboard appears (e.g., during search), should the floating action button hide automatically, move upward to avoid overlap, or remain in its original position? → A: Move FAB upward to avoid overlapping with the keyboard
- Q: On landscape-oriented screens or tablets with wide displays, should the FAB maintain its bottom-right corner position relative to the content area, or should it adapt its placement based on available screen real estate? → A: Maintain bottom-right position regardless of orientation and screen size

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add Book via Floating Action Button (Priority: P1)

The user views their book list and taps a floating action button positioned at the bottom-right corner of the screen to navigate to the "Add New Book" page. The FAB is always visible, uses a circular icon design consistent with Android Material Design conventions, and provides clear visual affordance for the primary "add" action.

**Why this priority**: This is the core user journey — replacing the toolbar add button with a more discoverable, platform-standard FAB improves the primary entry point for adding books to the library.

**Independent Test**: Can be fully tested by opening the book list page, verifying the FAB appears at the bottom-right corner, tapping it, and confirming navigation to the "Add New Book" page.

**Acceptance Scenarios**:

1. **Given** the user is on the book list page, **When** they view the screen, **Then** a circular floating action button with an add icon is visible at the bottom-right corner
2. **Given** the FAB is visible, **When** the user taps it, **Then** the app navigates to the "Add New Book" page
3. **Given** the user was previously using the toolbar add button, **When** they view the book list page, **Then** the add action no longer appears in the toolbar

---

### User Story 2 - FAB Accessibility and Localization (Priority: P2)

The floating action button includes proper accessibility descriptions and localized text so screen reader users understand its purpose. The FAB respects system theme (light/dark mode) with appropriate icon coloring.

**Why this priority**: Ensures the FAB is usable by all users, including those relying on assistive technology, and maintains consistency with existing localization patterns.

**Independent Test**: Can be tested by enabling a screen reader, navigating to the book list page, and verifying the FAB announces its purpose using localized text.

**Acceptance Scenarios**:

1. **Given** the user has a screen reader enabled, **When** they focus on the FAB, **Then** the screen reader announces the button's localized description
2. **Given** the device is in dark mode, **When** the user views the book list page, **Then** the FAB icon color adapts to maintain contrast

---

### Edge Cases

- When the keyboard is visible (e.g., during search), the FAB MUST reposition upward to avoid overlapping with the keyboard input area
- The FAB MUST maintain bottom-right corner anchoring on all screen sizes and orientations, including landscape mode and tablet displays

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a floating action button (FAB) at the bottom-right corner of the book list page
- **FR-002**: The FAB MUST use a circular shape with an add icon, consistent with Android Material Design conventions
- **FR-003**: Users MUST be able to tap the FAB to navigate to the "Add New Book" page
- **FR-004**: System MUST remove the add action from the toolbar items on the book list page
- **FR-005**: The FAB MUST include localized semantic accessibility properties matching existing resource patterns
- **FR-006**: When the on-screen keyboard appears, the FAB MUST reposition upward to avoid overlapping with the keyboard

### Key Entities *(include if feature involves data)*

- **BookListPage UI Layout**: The page layout must accommodate a bottom-right positioned FAB without overlapping content, search controls, or the collection view

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can initiate "Add New Book" navigation with a single tap on the FAB
- **SC-002**: The FAB maintains bottom-right anchoring across all screen sizes, orientations, and device form factors without layout overlap
- **SC-003**: When the keyboard appears, the FAB repositions upward within 100ms to avoid overlapping with the input area
- **SC-004**: Screen reader users receive clear, localized descriptions when focusing the FAB

## Assumptions

- The existing "Add New Book" navigation command (`NavigateToAddBookCommand`) remains functional and unchanged
- The export toolbar item (if present) continues to function in its current location unless explicitly moved
- The FAB design follows standard Material Design conventions for floating action buttons on Android, adapted for cross-platform MAUI rendering
- Localization keys already exist or can be reused from the previous toolbar implementation
