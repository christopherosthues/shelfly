# Quickstart Validation: Book List UI Improvements

**Date**: 2026-09-02 | **Branch**: `001-book-list-ui-improvements`

## Prerequisites

- .NET 10 SDK installed (prerelease allowed per `global.json`)
- MAUI workloads installed for target platform(s)
- Solution builds successfully: `dotnet build Shelfly.slnx`

## Validation Scenarios

### Scenario 1: Page Title Display

**Goal**: Verify the book list page displays a proper localized title instead of "Title" (the sort picker's label).

**Steps**:
1. Run the MAUI app (`dotnet run --project Shelfly.App` or launch via IDE)
2. Navigate to the book list page (main library view)
3. Observe the page title in the navigation bar / status bar

**Expected Outcome**:
- English device: Page title displays "My Library" (or equivalent meaningful name from `AppResources.BookListPageTitle`)
- German device: Page title displays localized equivalent
- Title is distinct from the sort picker's label ("Sort books by")

### Scenario 2: Sort Options Localization

**Goal**: Verify all sort option values in the picker are properly localized.

**Steps**:
1. Run the MAUI app
2. Navigate to the book list page
3. Tap the sort picker to open the dropdown
4. Observe all displayed option texts

**Expected Outcome**:
- English device: Options display "Title", "Author", "Publisher", "Publish Date" (from localized resources)
- German device: Options display German equivalents from `AppResources.de.resx`
- No raw enum names visible (e.g., no "SortCriterion.Title" or similar technical identifiers)

### Scenario 3: Sort Direction Toggle

**Goal**: Verify ascending/descending sort direction toggle works correctly.

**Steps**:
1. Run the MAUI app with books present in the library
2. Navigate to the book list page
3. Note the initial sort order (ascending by default — A-Z for text fields)
4. Tap the sort direction toggle icon adjacent to the picker
5. Observe the list reorder and icon change

**Expected Outcome**:
- Initial state: Sort arrow shows ascending indicator (↑), books sorted A-Z / oldest-first
- After first tap: Arrow changes to descending (↓), books reverse order (Z-A / newest-first)
- After second tap: Returns to ascending, books restore original order
- Toggle completes within one interaction (single tap)

### Scenario 4: Direction Persistence Across Criterion Change

**Goal**: Verify sort direction persists when changing sort criterion.

**Steps**:
1. Run the MAUI app with books present
2. Set sort direction to descending via toggle
3. Change sort criterion from Title to Author (or any other option)
4. Observe the new list order

**Expected Outcome**:
- List re-sorts by the new criterion while maintaining descending direction
- Sort arrow icon remains showing descending state (↓)
- Direction only resets on app restart (session-only persistence per clarification)

## Setup Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run API locally (if needed for data loading)
dotnet run --project Shelfly.Api

# Run MAUI client (platform-specific)
# Windows:
dotnet run --project Shelfly.App -f net10.0-windows
# Android:
dotnet run --project Shelfly.App -f net10.0-android
```

## References

- **Spec**: [spec.md](./spec.md) — Full acceptance scenarios and success criteria
- **Data Model**: [data-model.md](./data-model.md) — ViewModel properties and resource keys
- **Research**: [research.md](./research.md) — Technical decisions and rationale
