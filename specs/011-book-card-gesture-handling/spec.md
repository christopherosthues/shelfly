# Feature Specification: Book Card Gesture Commands

**Feature Branch**: `011-book-card-gesture-handling`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "The BookCardView should support long press and normal taps. When long press recognized the card should be marked as selected. Then a normal tap should only deselect the card again. When unselected a normal tap should trigger the user command e.g. navigate to detail page. Therefore add a command for the normal unselected tap. The long press is implemented rudimentally but not fully. finish it. .NET 10, no LongPressGestureRecognizer available."

## Clarifications

### Session 2026-09-04

- Q: When the user long presses a book card that is already selected, should the selection be removed or remain unchanged? → A: Toggle off — long press toggles selection state regardless of current state (unselected → select, selected → deselect)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Long Press Selects Book Card (Priority: P1)

While viewing the library list, the user presses and holds on a book card for approximately 500 milliseconds. Upon gesture recognition, the card's selection state toggles: unselected cards become selected with visual feedback (checkmark indicator), and already-selected cards become deselected. The selection state change is communicated to the page's view model so it can track selected items. A normal tap on an already-selected card also deselects it without triggering navigation.

**Why this priority**: This completes the core multi-selection interaction established in the previous feature scope. Without a functioning long press command, users cannot select cards for batch operations.

**Independent Test**: Can be fully tested by long pressing any book card in the library list and verifying that the view model's selection tracking commands are invoked with the correct book context.

**Acceptance Scenarios**:

1. **Given** the user is viewing the library list, **When** they long press on an unselected book card, **Then** the card becomes selected and the view model receives a selection toggle command
2. **Given** a book card is currently selected, **When** the user taps it normally, **Then** only the selection is removed without triggering navigation to detail
3. **Given** a book card is unselected, **When** the user long presses it, **Then** the card becomes selected with visual feedback
4. **Given** a book card is currently selected, **When** the user long presses it again, **Then** the selection is removed and the view model receives a deselection command

---

### User Story 2 - Normal Tap Navigates Unselected Card (Priority: P1)

While viewing the library list, the user taps an unselected book card. The tap gesture triggers a navigation command that opens the book detail page for the tapped item. This allows users to quickly drill into book information without entering selection mode first.

**Why this priority**: Normal tap navigation is the primary interaction pattern for exploring the library. Users expect immediate navigation on tap when not in selection mode.

**Independent Test**: Can be fully tested by tapping any unselected book card and verifying that the detail page opens with the correct book context passed as a parameter.

**Acceptance Scenarios**:

1. **Given** a book card is unselected, **When** the user taps it normally, **Then** the navigation command fires and the detail page opens for that book
2. **Given** a book card is selected, **When** the user taps it normally, **Then** only deselection occurs — no navigation to detail

---

### User Story 3 - Gesture Coexistence (Priority: P2)

The long press and normal tap gestures coexist on the same book card without interference. A quick tap triggers navigation immediately; a sustained press (500ms+) triggers selection. The user can switch between modes fluidly by alternating gestures.

**Why this priority**: Dual-gesture support enables both quick exploration (tap to navigate) and batch operations (long press to select) from the same UI surface without requiring separate mode toggles.

**Independent Test**: Can be tested by performing a rapid tap followed by a long press on different cards, verifying each gesture produces its intended outcome independently.

**Acceptance Scenarios**:

1. **Given** the user taps quickly (under 500ms), **When** the finger lifts, **Then** navigation fires for unselected cards
2. **Given** the user presses and holds (over 500ms), **When** the finger lifts, **Then** selection toggles for that card without navigation

---

### Edge Cases

- What happens when the user long presses a card at the very top or bottom of the list during scroll?
- How does gesture recognition behave during rapid successive taps and long presses on different cards?
- Does the 500ms threshold adapt to platform-specific defaults (Android vs iOS vs Windows)?
- What happens if the user's finger drifts slightly during a long press — is it still recognized?
- How does selection state persist when scrolling causes cards to recycle in the CollectionView?
- Does the gesture system correctly handle accessibility interactions (e.g., switch control hold)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: BookCardView MUST expose an `ICommand` BindableProperty for long press gestures that fires when a press exceeds 500 milliseconds and toggles the card's selection state (unselected → selected, selected → unselected)
- **FR-002**: BookCardView MUST expose an `ICommand` BindableProperty for normal tap gestures on unselected cards
- **FR-003**: The long press command MUST receive the card's `BindingContext` (book entity) as its command parameter
- **FR-004**: The tap command MUST receive the card's `BindingContext` (book entity) as its command parameter when the card is unselected
- **FR-005**: When a book card is selected, a normal tap MUST deselect it without firing the navigation command
- **FR-006**: When a book card is unselected, a normal tap MUST fire the exposed tap command to allow page-level binding (e.g., navigation)
- **FR-007**: The long press detection logic MUST complete the existing rudimentary implementation by actually invoking the bound command upon threshold recognition to toggle selection state
- **FR-008**: Both gesture recognizers (tap and pointer-based long press) MUST coexist without mutual interference on the same control

### Key Entities

- **BookCardView**: A ContentView-derived custom control representing a book in list form. It manages selection state via `IsSelected` property and exposes commands for user interaction gestures.
- **Long Press Command**: An `ICommand` BindableProperty on BookCardView that page-level view models bind to their selection toggle logic (e.g., `EnterSelectionMode`).
- **Tap Command**: An `ICommand` BindableProperty on BookCardView that page-level view models bind to navigation commands for unselected cards.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can select a book card within 600ms of initiating a long press gesture
- **SC-002**: Unselected cards navigate to detail within 200ms of a normal tap
- **SC-003**: Selected cards deselect within 150ms of a normal tap without navigation delay
- **SC-004**: Gesture recognition accuracy exceeds 95% — false positives (tap misfiring as long press or vice versa) occur less than 5% of the time

## Assumptions

- The existing `IsSelected` BindableProperty and visual selection indicator remain functional from the previous feature scope
- View models (`BookListViewModel`, `TrashListViewModel`) already expose selection commands (`EnterSelectionMode`, `ToggleSelection`) ready to bind
- The 500ms long press threshold matches platform defaults sufficiently for user expectations
- Page-level XAML will wire the new command properties to appropriate view model commands (navigation for tap, selection for long press)
- The pointer gesture approach works across all target platforms (Android, iOS/MacCatalyst, Windows) as a .NET 10 compatible alternative to `LongPressGestureRecognizer`
