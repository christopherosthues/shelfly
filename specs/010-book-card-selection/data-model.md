# Data Model: Book Card Selection

**Date**: 2026-09-04
**Branch**: `010-book-card-selection`

## Entities Affected

### BookEntity (Existing)

No structural changes required. The entity already contains the identifier needed for selection tracking:

| Field | Type | Role in Selection | Notes |
|-------|------|-------------------|-------|
| `Id` | `Guid` (UUID v7) | Primary key used as selection identifier | Used to track selected items in ViewModel collection |

### BookListViewModel Properties (New)

Selection state is managed client-side within the ViewModel:

| Property | Type | Purpose |
|----------|------|---------|
| `IsSelectionMode` | `bool` (`ObservableProperty`) | Indicates whether selection mode is active; set to true on first long press |
| `SelectedItemIds` | `ObservableCollection<Guid>` (`ObservableProperty`) | Collection of selected book identifiers; cleared on navigation away from page |

### Selection Indicator (UI Element)

The checkmark indicator is a visual element within BookCardView:

| Property | Value | Source |
|----------|-------|--------|
| `IconImageSource` | `"check_icon.svg"` | SVG asset in `Resources/Images/` |
| `RotationY` | Animated via `RotateYToAsync` | 0° (unselected) → 360° (selected), with ease-in-out easing over 300ms |
| `IsVisible` | Always true (space reserved) | Visibility controlled by RotationY animation; space occupied even when unselected |
| `WidthRequest` | Fixed dimension | Reserved to prevent text movement during state transitions |
| `HeightRequest` | Fixed dimension | Matches WidthRequest for circular appearance |

## Validation Rules

Selection state management follows these rules:

- **Multi-select**: Multiple books can be selected simultaneously via individual long presses
- **Toggle behavior**: Long pressing a selected book deselects it; long pressing an unselected book selects it
- **Tap coexistence**: Regular tap navigates to book details regardless of selection state
- **Session persistence**: Selection persists during scrolling and within the current list session
- **Navigation cleanup**: `OnNavigatingFrom` clears all selections and exits selection mode

## State Transitions

```
Initial State:
  IsSelectionMode = false
  SelectedItemIds = []

First Long Press (Book A):
  IsSelectionMode = true
  SelectedItemIds = [A.Id]
  BookCardView displays animated checkmark for Book A

Subsequent Long Press (Book B):
  IsSelectionMode = true (unchanged)
  SelectedItemIds = [A.Id, B.Id]
  Both cards display checkmarks

Tap on Selected Card (Book A):
  SelectedItemIds = [B.Id]
  Book A's checkmark animates out

Scroll or Navigate Away:
  OnNavigatingFrom() called
  IsSelectionMode = false
  SelectedItemIds = []
```

## Gesture Priority

When multiple gesture recognizers are present on the same element, MAUI evaluates them in this order:

1. **LongPressGestureRecognizer** — fires after minimum press duration (500ms default)
2. **TapGestureRecognizer** — fires on tap release (immediate)
3. **SwipeView gestures** — handled at parent level (existing swipe-to-edit/delete)

The long press gesture does not interfere with tap navigation or swipe actions because:
- Long press requires sustained hold; tap is immediate
- SwipeView operates at the CollectionView item wrapper level, not within BookCardView
- Gesture recognizers are evaluated independently without mutual exclusion
