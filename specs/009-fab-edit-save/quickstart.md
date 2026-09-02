# Quickstart Validation: FAB Edit & Save UI

**Date**: 2026-09-02 | **Feature**: [spec.md](./spec.md)

## Prerequisites

- .NET 10 SDK installed (with MAUI workloads for target platform)
- Shelfly solution builds successfully: `dotnet build Shelfly.slnx`
- API service running locally or reachable at configured server URL
- Device or emulator connected and capable of running the MAUI app

## Setup Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run API (requires Keycloak config in appsettings.json)
dotnet run --project Shelfly.Api

# Launch MAUI client on target platform
# Android: dotnet run --project Shelfly.App -f net10.0-android
# Windows: dotnet run --project Shelfly.App -f net10.0-windows
```

## Validation Scenarios

### Scenario 1: Book Detail FAB Navigation

**Goal**: Verify FAB replaces edit toolbar item and navigates to book edit page.

1. Launch the app and navigate to any existing book's detail page
2. **Check**: Toolbar shows only delete icon (no edit icon)
3. **Check**: Floating action button visible at bottom-right corner with edit icon
4. **Action**: Tap the FAB
5. **Expected**: Navigate to BookEditPage pre-populated with current book data

### Scenario 2: Book Edit FAB Save

**Goal**: Verify FAB replaces inline save button and saves changes correctly.

1. From book detail, tap the edit FAB to enter edit mode
2. **Check**: Form body shows no full-width save button at bottom
3. **Check**: Floating action button visible at bottom-right corner with save icon
4. **Action**: Make a change (e.g., modify title) and tap the FAB
5. **Expected**: Save operation executes; loading spinner appears in FAB during save; return to book detail page showing updated data

### Scenario 3: Bookmark Edit FAB Save

**Goal**: Verify FAB pattern applied consistently on bookmark edit page.

1. From book detail, add a new bookmark or edit an existing one
2. **Check**: Form body shows no inline save button at bottom
3. **Check**: Floating action button visible at bottom-right corner with save icon
4. **Action**: Make a change (e.g., modify note) and tap the FAB
5. **Expected**: Save operation executes; loading spinner appears in FAB during save; return to book detail page showing updated bookmark

### Scenario 4: Delete Toolbar Item Preserved

**Goal**: Verify delete action remains accessible via toolbar item on book detail.

1. Navigate to any existing book's detail page
2. **Check**: Delete icon visible in toolbar area (top-right)
3. **Action**: Tap the delete toolbar item
4. **Expected**: Confirmation alert appears; tapping OK soft-deletes the book and returns to library list

### Scenario 5: FAB Theme Consistency

**Goal**: Verify FAB adapts to light/dark theme correctly.

1. Navigate to any page with a FAB (book detail, book edit, or bookmark edit)
2. **Check**: FAB background circle uses Primary color in light mode, White in dark mode
3. **Check**: FAB icon tint uses Secondary color in light mode, Primary in dark mode
4. **Action**: Toggle system theme between light and dark
5. **Expected**: FAB colors adapt immediately to match active theme

## Expected Outcomes

All scenarios complete without errors:
- FAB visible and tappable on all three affected pages
- Toolbar edit item removed from book detail; delete item preserved
- Inline save buttons removed from both edit forms
- Loading state (spinner + reduced opacity) displayed during save operations
- Theme-aware colors applied correctly via AppThemeBinding

## References

- [Research decisions](./research.md) — FAB pattern details and rationale
- [Data model](./data-model.md) — confirms no data changes required
- [Feature spec](./spec.md) — full acceptance criteria
