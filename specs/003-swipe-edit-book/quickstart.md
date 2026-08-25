# Quickstart: Swipe-to-Edit Book Validation Guide

**Date**: 2026-08-25
**Branch**: `003-swipe-edit-book`

## Prerequisites

1. **Solution built successfully**: `dotnet build Shelfly.slnx`
2. **API running locally**: `dotnet run --project Shelfly.Api` (requires Keycloak config in appsettings.json)
3. **MAUI client configured**: Target platform selected (Android recommended for initial validation)

## Validation Scenarios

### Scenario 1: Swipe Left Reveals Edit Action

**Steps**:
1. Launch the MAUI application and navigate to the book list page
2. Ensure at least one book exists in the library
3. Place finger on a book card and swipe left (toward the screen edge)
4. Observe that an action element appears with:
   - An edit icon (SVG)
   - Localized text matching current app language

**Expected Outcome**: Action element is visible, displays correct icon and localized text for the active locale

### Scenario 2: Tap Edit Action Navigates to BookEditPage

**Steps**:
1. With the edit action element visible (from Scenario 1), tap it
2. Observe navigation to the book edit page
3. Verify the page loads with existing book data pre-filled in all fields

**Expected Outcome**: Navigation succeeds; `BookEditPage` displays current book data for author, title, publisher, ISBN, and publication year

### Scenario 3: Edit Book and Navigate Back

**Steps**:
1. On the edit page (from Scenario 2), modify a field (e.g., change the title)
2. Tap the Save button
3. Observe navigation back to the book list
4. Verify the modified data is reflected in the list view

**Expected Outcome**: Changes are persisted; list view shows updated information without requiring manual refresh

### Scenario 4: Swipe Cancellation

**Steps**:
1. Start a left swipe on a book item but release before reaching activation threshold
2. Observe the card animates back to resting position
3. Verify no navigation occurred and no side effects (e.g., no API calls)

**Expected Outcome**: Card returns smoothly; edit page not opened; original data unchanged

### Scenario 5: Localization Verification

**Steps**:
1. Switch app language to German (`de-DE`) via settings or resource file selection
2. Swipe left on a book item
3. Verify the action element text is in German

**Expected Outcome**: Text matches `AppResources.BookListPageSwipeToEditCommand` value for `de-DE` locale

## Test Commands

```bash
# Build solution with all projects
dotnet build Shelfly.slnx

# Run API service (required for data persistence)
dotnet run --project Shelfly.Api

# Launch MAUI client on Android
dotnet run --project Shelfly.App -f net10.0-android
```

## References

- **Feature Spec**: [spec.md](./spec.md)
- **Implementation Plan**: [plan.md](./plan.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Research Notes**: [research.md](./research.md)
