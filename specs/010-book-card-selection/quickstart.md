# Quickstart: Book Card Selection Validation Guide

**Date**: 2026-09-04
**Branch**: `010-book-card-selection`

## Prerequisites

1. **Solution built successfully**: `dotnet build Shelfly.slnx`
2. **API running locally**: `dotnet run --project Shelfly.Api` (requires Keycloak config in appsettings.json)
3. **MAUI client configured**: Target platform selected (Android recommended for initial validation)

## Validation Scenarios

### Scenario 1: Long Press Selects Book Card

**Steps**:
1. Launch the MAUI application and navigate to the book list page
2. Ensure at least one book exists in the library
3. Place finger on a book card and hold for approximately 500ms (standard long press duration)
4. Observe that:
   - A checkmark icon appears inside a circular indicator at the leading edge of the card
   - The checkmark animates with a y-axis rotation (ease-in-out easing, completes within 300ms)
   - Text content (title, author, publisher) remains in the same position

**Expected Outcome**: Checkmark is visible with smooth rotation animation; text position unchanged despite visual change

### Scenario 2: Tap Deselects Selected Card

**Steps**:
1. With a book card selected (showing checkmark from Scenario 1), tap the same card
2. Observe that:
   - The checkmark disappears with a y-axis rotation animation
   - Text content remains in the same position
   - The space previously occupied by the checkmark becomes visually empty

**Expected Outcome**: Checkmark animates out; text position stable; indicator space reserved but invisible

### Scenario 3: Multi-Card Selection

**Steps**:
1. Long press on Book A to select it (checkmark appears)
2. Long press on Book B to select it (checkmark appears)
3. Verify both cards display checkmarks simultaneously
4. Tap Book A to deselect it
5. Verify only Book B's checkmark remains

**Expected Outcome**: Both selections tracked independently; deselection affects only the tapped card

### Scenario 4: Visual Stability During Selection

**Steps**:
1. Observe text position of a book card before selection
2. Long press to select the card
3. Compare text position after selection
4. Tap to deselect and compare again

**Expected Outcome**: Text position variance remains below 2 pixels throughout all state transitions; no adjacent cards shift position

### Scenario 5: Gesture Coexistence

**Steps**:
1. With a book card unselected, tap it briefly (not long press)
2. Observe navigation to book detail page
3. Navigate back to list
4. Long press the same card to select it
5. Verify selection works independently of tap navigation

**Expected Outcome**: Tap navigates to details; long press toggles selection; both gestures function without conflict

### Scenario 6: Scroll Persistence

**Steps**:
1. Select one or more book cards via long press
2. Scroll the list up or down (moving selected cards out of view)
3. Scroll back to the originally selected cards
4. Verify checkmarks are still visible

**Expected Outcome**: Selection state persists during scrolling; checkmarks remain visible when cards return to viewport

### Scenario 7: Theme Adaptation

**Steps**:
1. With a card selected, switch app theme (light/dark mode) via system settings or app toggle
2. Observe that the checkmark indicator adapts to the new theme colors

**Expected Outcome**: Checkmark icon and circular background use theme-appropriate colors via `AppThemeBinding`

### Scenario 8: Accessibility Verification

**Steps**:
1. Enable screen reader (TalkBack on Android, VoiceOver on iOS)
2. Long press a book card to select it
3. Verify the selection state is announced via semantic properties

**Expected Outcome**: Screen reader announces selection state change using localized text from resource files

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
