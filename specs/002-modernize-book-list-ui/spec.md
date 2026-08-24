# Feature Specification: Modernize Book List UI

**Feature Branch**: `002-modernize-book-list-ui`

**Created**: 2026-08-24

**Status**: Draft

**Input**: User description: "The UI of the book list page should be modernized. The sort picker should be moved from the bottom to the top to the right of the search bar if there is enough space. When both elements are too small the picker should move beneath the search bar. The items should be wrapped into a card (name from android use a similar element from .NET MAUI)"

## Clarifications

### Session 2026-08-24

- Q: How should the sort picker behave on very narrow screens (e.g., small phones in portrait)? → A: Picker moves beneath the search bar when combined width exceeds available space
- Q: Should the card styling apply to all book list items or only specific ones? → A: All book list items should be wrapped in a card with consistent styling
- Q: What visual properties should the cards have (shadow, border, radius)? → A: Use platform-native card styling with subtle shadow, rounded corners, and consistent spacing
- Q: What horizontal margin should be applied to book list cards? → A: Cards must have a default margin of 16 units on both left and right sides
- Q: How should the card component be structured technically? → A: Create a separate component inheriting from ContentView, using a Grid inside a Border
- Q: What MinimumHeightRequest should BookCardView have for accessibility compliance? → A: 48 units to meet Android accessibility minimums
- Q: What corner radius value should cards use? → A: 5 units for rounded corners
- Q: How should the ActivityIndicator be positioned in the new layout? → A: Centered on the screen (HorizontalOptions="Center", VerticalOptions="Center")
- Q: How is BindingContext handled on BookCardView? → A: BindingContext is a property, not a constructor parameter

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Responsive Layout Adaptation (Priority: P1)

The user opens the book list page on various screen sizes. On wider screens, the search bar and sort picker appear side by side at the top of the page. On narrower screens where both elements would be cramped, the sort picker automatically moves beneath the search bar. The layout adapts seamlessly without requiring user interaction or manual resizing.

**Why this priority**: This is the core visual improvement that affects all users immediately upon opening the book list page. It establishes the modernized UI foundation for subsequent enhancements.

**Independent Test**: Can be fully tested by rotating device orientation, testing on multiple screen sizes (small phone, large phone, tablet), and verifying the layout adapts correctly: side-by-side on wide screens, stacked on narrow screens.

**Acceptance Scenarios**:

1. **Given** the user is viewing the book list page on a landscape or wide-screen device, **When** they view the top controls area, **Then** the search bar and sort picker appear horizontally aligned side by side
2. **Given** the user is viewing the book list page on a narrow portrait screen where combined element width exceeds available space, **When** they view the top controls area, **Then** the sort picker appears beneath the search bar in a stacked layout
3. **Given** the user rotates their device from portrait to landscape (or vice versa), **When** the orientation changes, **Then** the layout automatically reflows to match the new screen dimensions without data loss

---

### User Story 2 - Card-Wrapped Book Items (Priority: P1)

The user views their book list and sees each book entry displayed within a visually distinct card. Each card provides clear visual separation between items with consistent padding, rounded corners, and subtle elevation/shadow effects. The cards maintain the existing information display (title, author, publisher) while improving visual hierarchy and touch target clarity.

**Why this priority**: Card wrapping is the primary visual modernization that improves readability, touch interaction, and overall aesthetic quality of the book list. It directly impacts user experience for every list interaction.

**Independent Test**: Can be fully tested by viewing a populated book list, verifying each item is wrapped in a card with consistent styling (padding, corners, shadow), comparing before/after visual appearance, and confirming existing functionality (tap to detail, swipe to delete) remains intact within the new card structure.

**Acceptance Scenarios**:

1. **Given** the user has added at least one book, **When** they view the book list, **Then** each book entry is displayed within a visually distinct card with rounded corners (radius 5 units), consistent padding, and 16 units of horizontal margin on both left and right sides
2. **Given** the user is viewing card-wrapped book items, **When** they tap a card, **Then** navigation to the book detail view occurs as before
3. **Given** the user is viewing card-wrapped book items on a touch device, **When** they swipe left on a card, **Then** the swipe-to-delete gesture functions identically to the previous implementation
4. **Given** the user has multiple books in their library, **When** scrolling through the list, **Then** consistent visual spacing and separation exists between adjacent cards

---

### Edge Cases

- When screen width is exactly at the threshold where side-by-side layout becomes cramped, the system should prefer stacked layout to ensure usability
- Cards must maintain existing accessibility semantic properties for screen reader support
- Card styling should adapt to platform-native design guidelines (Android Material Design, iOS Human Interface Guidelines) while maintaining visual consistency across platforms
- Empty state message should remain visible and centered when no books exist, unaffected by card styling changes
- Loading indicator MUST be centered on the screen (HorizontalOptions="Center", VerticalOptions="Center") relative to the new layout structure

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display search bar and sort picker at the top of the book list page
- **FR-002**: On screens with sufficient width, the search bar and sort picker MUST appear horizontally aligned side by side
- **FR-003**: When combined element width exceeds available screen space, the sort picker MUST reflow beneath the search bar in a stacked layout
- **FR-004**: System MUST automatically adapt layout based on screen dimensions without requiring user interaction
- **FR-005**: Each book list item MUST be wrapped in a card with rounded corners (radius 5 units), consistent padding, and a default margin of 16 units on both left and right sides
- **FR-011**: BookCardView MUST have MinimumHeightRequest of 48 units to meet platform accessibility minimums
- **FR-006**: Cards MUST provide visual separation between items using platform-appropriate shadow or elevation effects
- **FR-007**: Card styling MUST maintain existing information display (title, author, publisher) within each card
- **FR-008**: Existing tap-to-navigate functionality MUST remain functional within the new card structure
- **FR-009**: Swipe-to-delete gesture MUST continue to function identically within card-wrapped items
- **FR-010**: System MUST preserve accessibility semantic properties for screen reader support on all card elements

### Key Entities *(include if feature involves data)*

- **BookListPage Layout**: The page layout structure changes from a vertical stack (search bar → book list → sort picker) to a responsive top section (search bar + sort picker, either side-by-side or stacked) followed by the card-wrapped book list
- **Card Component**: A visual container wrapping each book list item providing rounded corners (radius 5 units), padding, shadow/elevation, consistent spacing between items, a default horizontal margin of 16 units on both left and right sides, and MinimumHeightRequest of 48 units for accessibility compliance. BindingContext is exposed as a property.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can locate and interact with search and sort controls within 2 seconds of opening the page
- **SC-002**: Layout adaptation occurs within 300 milliseconds of screen dimension change (orientation, resize)
- **SC-003**: Card-wrapped items improve perceived visual hierarchy — users can distinguish individual book entries at a glance without scrolling ambiguity
- **SC-004**: Touch target area for each card meets platform accessibility minimums (MinimumHeightRequest of 48 units)

## Assumptions

- The app uses .NET MAUI's built-in layout containers (FlexLayout or Grid) to achieve responsive side-by-side/stacked behavior
- Card styling leverages platform-native design patterns while maintaining cross-platform visual consistency
- Existing book data display (title, author, publisher) remains unchanged within cards — only container styling is modified
- The sort picker continues to use the same sorting options and functionality; only its position changes
- Search bar functionality (case-insensitive substring matching) remains unchanged
- Layout thresholds for side-by-side vs. stacked are determined by available screen width relative to combined element minimum widths
- BookCardView uses MinimumHeightRequest of 48 units, corner radius of 5 units, and BindingContext as a property
