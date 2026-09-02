# Quickstart Validation: FAB Add Button

**Date**: 2026-09-02  
**Feature**: specs/008-fab-add-button/spec.md

## Prerequisites

- .NET 10 SDK installed (with MAUI workloads for target platform)
- Android emulator or physical device connected (primary test target)
- Solution builds successfully: `dotnet build Shelfly.slnx`

## Validation Scenarios

### Scenario 1: FAB Visibility and Positioning

**Setup**: Run the MAUI app on an Android device/emulator

**Steps**:
1. Navigate to the book list page (Library home)
2. Verify a circular button with an add icon appears at the bottom-right corner
3. Verify the toolbar no longer shows the add action

**Expected Outcome**: FAB is visible, circular in shape, positioned at bottom-right; toolbar contains only the export item

### Scenario 2: FAB Navigation

**Setup**: App running on book list page with FAB visible

**Steps**:
1. Tap the floating action button
2. Observe navigation behavior

**Expected Outcome**: App navigates to "Add New Book" page (BookEditPage) without parameters

### Scenario 3: Keyboard Repositioning

**Setup**: App running on book list page with FAB visible

**Steps**:
1. Tap the search bar to bring up the keyboard
2. Observe FAB position change
3. Dismiss the keyboard
4. Observe FAB returns to original position

**Expected Outcome**: FAB moves upward when keyboard appears (no overlap); FAB returns to bottom-right when keyboard dismisses

### Scenario 4: Screen Orientation and Size

**Setup**: App running on book list page with FAB visible

**Steps**:
1. Rotate device to landscape orientation
2. Verify FAB remains at bottom-right corner
3. Return to portrait orientation
4. Verify FAB returns to original position

**Expected Outcome**: FAB maintains bottom-right anchoring across all orientations and screen sizes

### Scenario 5: Accessibility

**Setup**: Screen reader enabled (TalkBack on Android, VoiceOver on iOS)

**Steps**:
1. Navigate to book list page
2. Focus on the floating action button using screen reader controls
3. Verify announced description matches localized text

**Expected Outcome**: Screen reader announces the FAB's semantic description using localized resource (`BookListPageAddNewBookDescription`)

### Scenario 6: Theme Adaptation

**Setup**: App running with light theme enabled

**Steps**:
1. Observe FAB appearance in light mode
2. Switch device to dark mode
3. Observe FAB adapts color scheme

**Expected Outcome**: FAB background and icon colors adapt between light/dark themes using AppThemeBinding resources

## Run Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run MAUI app (Android)
dotnet run --project Shelfly.App -f net10.0-android

# Run on Windows desktop (for quick layout verification)
dotnet run --project Shelfly.App -f net10.0-windows
```

## References

- **Spec**: [spec.md](./spec.md)
- **Research**: [research.md](./research.md)
- **Data Model**: [data-model.md](./data-model.md)
