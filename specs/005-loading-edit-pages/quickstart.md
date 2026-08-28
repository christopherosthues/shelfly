# Quickstart Validation Guide: Loading Indicators for Edit Pages

**Date**: 2026-08-28  
**Feature**: specs/005-loading-edit-pages/spec.md

## Prerequisites

1. **Solution builds successfully**: `dotnet build Shelfly.slnx`
2. **MAUI client runs on target platform**: Android emulator or physical device recommended for validation
3. **Test data available**: At least one book in the library to enable edit navigation

## Validation Scenarios

### Scenario 1: Full-Screen Overlay During Book Data Load

**Steps**:
1. Launch the Shelfly app
2. Navigate to the book list page
3. Swipe left on an existing book and tap "Edit" (or use the edit swipe action)
4. Observe the transition to BookEditPage

**Expected Outcome**:
- A full-screen overlay with ActivityIndicator appears immediately upon navigation
- The overlay blocks all user input during data fetch
- Form fields populate when data arrives, overlay disappears
- No stale or empty form state visible during loading

### Scenario 2: Full-Screen Overlay During Bookmark Data Load

**Steps**:
1. Navigate to a book detail page with existing bookmarks
2. Tap the edit icon on an existing bookmark
3. Observe the transition to BookmarkEditPage

**Expected Outcome**:
- A full-screen overlay with ActivityIndicator appears immediately upon navigation
- The overlay blocks all user input during data fetch
- Form fields (StartPage, EndPage, Note) populate when data arrives, overlay disappears

### Scenario 3: Button-Level Indicator During Book Save

**Steps**:
1. Navigate to edit an existing book (or create a new book)
2. Fill in required fields (Title, Author, Publisher)
3. Tap the save button
4. Observe button behavior during save operation

**Expected Outcome**:
- The save button immediately transforms into an inline ActivityIndicator
- The button becomes disabled (no double-tap possible)
- Indicator remains visible for minimum 2 seconds
- On success: indicator disappears, navigation occurs
- On error: button returns to normal state (re-enabled), error message displayed

### Scenario 4: Button-Level Indicator During Bookmark Save

**Steps**:
1. Navigate to edit an existing bookmark (or create a new bookmark)
2. Fill in required fields (StartPage)
3. Tap the save button
4. Observe button behavior during save operation

**Expected Outcome**:
- The save button immediately transforms into an inline ActivityIndicator
- The button becomes disabled (no double-tap possible)
- Indicator remains visible for minimum 2 seconds
- On success: indicator disappears, navigation occurs (back to previous page)
- On error: button returns to normal state (re-enabled), error message displayed

### Scenario 5: Consistency Check

**Steps**:
1. Compare the full-screen overlay on edit pages against BookListPage loading pattern
2. Verify both edit pages use identical visual patterns for their respective indicators

**Expected Outcome**:
- Full-screen overlay matches BookListPage ActivityIndicator styling (color, size, positioning)
- Button-level indicator is consistent between BookEditPage and BookmarkEditPage
- No visual discrepancies between the two edit pages

## Run Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run MAUI client on Android
dotnet run --project Shelfly.App -f net10.0-android
```

## Success Indicators

- [ ] Full-screen overlay appears during data load on both edit pages
- [ ] Overlay blocks all user input during loading
- [ ] Button transforms to indicator during save on both edit pages
- [ ] Button disabling prevents double-tap submissions
- [ ] Minimum 2-second display duration observed for save indicator
- [ ] Visual consistency with existing patterns (BookListPage, BookDetailPage)

## Known Limitations

- **Local development context**: With no server, API calls complete instantly; the 2-second minimum duration ensures visible feedback
- **Network simulation**: To test overlay visibility on slower networks, consider adding artificial delay in ViewModel's LoadAsync method during validation
