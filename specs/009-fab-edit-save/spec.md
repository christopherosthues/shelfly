# Feature Specification: FAB Edit & Save UI

**Feature Branch**: `[009-fab-edit-save]`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "Use FAB for edit and saving on the detail and edit pages instead of normal buttons and toolbar items"

## Clarifications

### Session 2026-09-02

- Q: How should the delete action be presented on the book detail page? → A: FAB for edit only; delete remains as a toolbar item
- Q: Should Add Bookmark button also become a FAB? → A: Keep as inline image button next to Bookmarks title
- Q: How should FAB behave when keyboard is open on edit pages? → A: FAB stays fixed at bottom-right; user dismisses keyboard to tap it
- Q: How should FAB loading state be visualized? → A: Both spinner icon and reduced opacity during save operations

### Session 2026-09-02 (Implementation Reference)

- Q: What FAB implementation pattern should be used? → A: Implement exactly like BookListPage does — Grid container with BoxView background circle + ImageButton overlay, using AppThemeBinding for colors

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Replace Toolbar Edit Button with FAB on Book Detail (Priority: P1)

When viewing a book's details, the user taps a floating action button positioned at the bottom-right corner to enter edit mode. The FAB provides a prominent, thumb-friendly target for the most common action on the detail page.

**Why this priority**: This is the primary interaction change — replacing toolbar items with FAB establishes the new UI pattern used across all pages.

**Independent Test**: Can be fully tested by navigating to any book detail page and verifying that tapping the floating action button navigates to the edit screen, while the toolbar no longer shows an edit icon.

**Acceptance Scenarios**:

1. **Given** I am viewing a book's detail page, **When** I tap the floating action button at the bottom-right corner, **Then** I navigate to the book edit page
2. **Given** I am viewing a book's detail page, **When** I look at the toolbar area, **Then** the edit icon is no longer visible as a toolbar item

---

### User Story 2 - Replace Inline Save Button with FAB on Book Edit (Priority: P1)

When editing a book, the user taps a floating action button to save changes. The FAB replaces the full-width inline save button at the bottom of the form, providing consistent thumb-friendly interaction across all edit pages.

**Why this priority**: Consistent with Story 1 — establishes the FAB pattern for saving actions on edit pages.

**Independent Test**: Can be fully tested by navigating to a book edit page and verifying that tapping the floating action button saves the book data, while the inline save button is no longer visible in the form body.

**Acceptance Scenarios**:

1. **Given** I am editing a book with changes made, **When** I tap the floating action button at the bottom-right corner, **Then** the changes are saved and I return to the detail page
2. **Given** I am on a book edit page, **When** I look at the form body, **Then** the inline save button is no longer visible

---

### User Story 3 - Apply FAB Pattern to Bookmark Edit Page (Priority: P2)

The bookmark edit page uses the same floating action button pattern for saving changes, maintaining visual consistency across all editing experiences.

**Why this priority**: Extends the established FAB pattern to the second edit page in the application.

**Independent Test**: Can be fully tested by navigating to a bookmark edit page and verifying that tapping the floating action button saves the bookmark data.

**Acceptance Scenarios**:

1. **Given** I am editing a bookmark with changes made, **When** I tap the floating action button at the bottom-right corner, **Then** the changes are saved and I return to the book detail page
2. **Given** I am on a bookmark edit page, **When** I look at the form body, **Then** the inline save button is no longer visible

---

### User Story 4 - Keep Delete Button in Toolbar on Book Detail (Priority: P3)

The delete action on book detail remains as a toolbar item while the edit action migrates to a FAB. This preserves quick access to destructive actions without visual clutter from multiple FABs.

**Why this priority**: Completes the migration scope by confirming which toolbar items are replaced — only edit moves to FAB, delete stays in toolbar.

**Independent Test**: Can be fully tested by navigating to a book detail page and verifying that the delete icon is still visible as a toolbar item while the edit action is now available via FAB.

**Acceptance Scenarios**:

1. **Given** I am viewing a book's detail page, **When** I look at the toolbar area, **Then** the delete icon is still visible as a toolbar item
2. **Given** I am viewing a book's detail page, **When** I tap the floating action button, **Then** I navigate to the edit page (not delete)

---

### Edge Cases

- When the keyboard is open on edit pages, the FAB remains fixed at the bottom-right corner; users must dismiss the keyboard to tap it
- During save operations, the FAB displays a spinner icon with reduced opacity to indicate loading state
- Does the FAB adapt to different screen sizes and orientations?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a floating action button on book detail pages for initiating edit mode
- **FR-002**: System MUST display a floating action button on book edit pages for saving changes
- **FR-003**: System MUST display a floating action button on bookmark edit pages for saving changes
- **FR-004**: System MUST remove the edit toolbar item from the detail page; the delete toolbar item remains visible
- **FR-005**: System MUST remove inline save buttons from the bottom of edit forms
- **FR-006**: Floating action buttons MUST display a spinner icon with reduced opacity during active save operations to indicate loading state
- **FR-007**: Floating action buttons MUST remain fixed at the bottom-right corner regardless of keyboard visibility or scroll position

### Key Entities *(include if feature involves data)*

- **FAB Control**: A floating action button implemented as a Grid container with BoxView background circle (64x64, CornerRadius 32) and ImageButton overlay (48x48), positioned at bottom-right with Margin 16. Uses AppThemeBinding for light/dark theme colors matching the BookListPage FAB pattern. Supports icon display, loading states via ActivityIndicator, and command binding.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can initiate book editing with a single tap on the floating action button
- **SC-002**: Users can save changes with a single tap on the floating action button without scrolling to find an inline button
- **SC-003**: Edit and save actions across detail and edit pages use a consistent floating action button pattern; delete remains as a toolbar item on the detail page
- **SC-004**: The floating action button remains fixed at the bottom-right corner; users dismiss the keyboard to tap it on edit pages

## Assumptions

- The FAB implementation follows the BookListPage pattern: Grid container with BoxView background circle and ImageButton overlay, using AppThemeBinding for theme-aware colors
- Existing localization resources for button text will be reused or extended for FAB accessibility labels
- The FAB pattern applies to page-level navigation actions on detail pages (edit book) and save actions on edit pages; section-level actions like Add Bookmark remain as inline buttons
- Users expect a single-tap interaction for primary actions; no long-press or swipe gestures are required
