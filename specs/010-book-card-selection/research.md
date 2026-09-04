# Research: Book Card Selection Implementation

**Date**: 2026-09-04
**Branch**: `010-book-card-selection`

## Decisions

### Decision: Use LongPressGestureRecognizer for Selection Toggle

**Rationale**: The spec requires long press to toggle selection state. MAUI provides `LongPressGestureRecognizer` with configurable `MinimumPressDuration` (default 500ms) and `Command` binding support. This is the standard gesture recognizer for this interaction pattern across all supported platforms.

**Alternatives considered**:
- Using `PointerGestureRecognizer` with custom timing logic — adds complexity; LongPressGestureRecognizer handles platform-specific behavior automatically
- Adding a tap-based selection toggle — conflicts with existing tap-to-navigate behavior; long press provides clear distinction between navigation and selection
- Using CollectionView's built-in `SelectionMode` — designed for single-item selection with automatic highlighting; lacks per-item visual customization needed for checkmark indicators

### Decision: Follow TrashListViewModel Selection Pattern

**Rationale**: The existing `TrashListViewModel` already implements multi-select state management with `IsSelectionMode`, `SelectedItemIds` collection, and commands like `ToggleSelection`, `EnterSelectionMode`, and `ExitSelectionMode`. Reusing this pattern ensures consistency across the app and leverages proven selection mechanics.

**Alternatives considered**:
- Creating a new base class for selection — premature abstraction; BookListViewModel can adopt the same properties directly
- Using a wrapper ViewModel for each item — adds unnecessary indirection; direct binding to ViewModel collection is simpler and more testable

### Decision: Reserve Space with Invisible Indicator When Unselected

**Rationale**: The spec requires text stability during selection/deselection. Reserving space for the indicator when unselected (visually empty) prevents layout shifts. A Grid column definition with fixed width provides consistent spacing regardless of selection state.

**Alternatives considered**:
- Using margin/padding adjustments — less predictable across platforms; Grid columns provide deterministic layout
- Collapsing/expanding the indicator — causes text movement as required by spec ("text should not move")

### Decision: Use Existing check_icon.svg for Selection Indicator

**Rationale**: The asset already exists in `Resources/Images/check_icon.svg` and follows SVG format requirements. Using it avoids adding new dependencies and ensures theme adaptability via `AppThemeBinding`.

**Alternatives considered**:
- Creating a custom icon — unnecessary when existing asset meets requirements
- Using a font-based checkmark (e.g., FontAwesome) — adds dependency; SVG provides better scalability and theme support

## Technical Findings

### LongPressGestureRecognizer API (Official MAUI Documentation)

| Property | Type | Default | Purpose |
|----------|------|---------|---------|
| `Command` | `ICommand` | — | Executed when long press is recognized |
| `CommandParameter` | `object` | — | Parameter passed to Command |
| `MinimumPressDuration` | `int` | 500ms | Minimum hold duration (ignored on Android) |

**Key Behavior**:
- Fires `LongPressed` event when user releases after minimum duration
- Compatible with other gesture recognizers (TapGestureRecognizer, SwipeView) — gestures are evaluated in priority order
- Works within ContentView and Grid contexts without gesture conflicts

### Y-Axis Rotation Animation (Official MAUI Documentation)

| Method | Description |
|--------|-------------|
| `RotateYToAsync(element, rotation, length, easing)` | Animates to target Y-rotation value with optional duration and easing |
| `VisualElement.RotationY` | Static property for bindable Y-axis rotation in degrees |

**Implementation Approach**:
```csharp
// Animate checkmark appearance (0 → 360 degrees over 300ms)
await checkmarkImage.RotateYToAsync(360, 300, Easing.SinInOut);

// Animate checkmark disappearance (current → 0 degrees over 300ms)
await checkmarkImage.RotateYToAsync(0, 300, Easing.SinInOut);
```

**Easing Options**: `Easing.SinInOut` provides smooth ease-in-out curve matching the spec requirement. Duration of 300ms aligns with SC-005 success criteria.

### Selection State Binding Pattern (Codebase Analysis)

The TrashListViewModel pattern for multi-select:

| Property | Type | Purpose |
|----------|------|---------|
| `IsSelectionMode` | `bool` | Indicates whether selection mode is active |
| `SelectedItemIds` | `ObservableCollection<Guid>` | Tracks selected item identifiers |
| `ToggleSelection(BookEntity)` | `void` | Toggles individual item selection state |

**Binding in DataTemplate**:
```xaml
<!-- Bind to ViewModel collection via RelativeSource -->
<Grid>
    <Grid.GestureRecognizers>
        <LongPressGestureRecognizer
            Command="{Binding ToggleSelectionCommand, Source={x:RelativeSource AncestorType={x:Type local:BookListViewModel}}}"
            CommandParameter="{Binding .}" />
    </Grid.GestureRecognizers>
</Grid>
```

### Platform Considerations

| Platform | LongPressGestureRecognizer Support | Notes |
|----------|-----------------------------------|-------|
| Android | Full touch support | Primary target platform; MinimumPressDuration ignored (uses native timing) |
| iOS/MacCatalyst | Full touch support | Available on non-Linux hosts; respects NumberOfTouchesRequired |
| Windows | Touch and pointer support | Pointer devices trigger long press after duration threshold |

## Resolved Clarifications

All NEEDS CLARIFICATION items resolved:
- ✅ LongPressGestureRecognizer API confirmed via Microsoft Learn documentation
- ✅ Y-axis rotation animation approach validated with `RotateYToAsync` and `Easing.SinInOut`
- ✅ Selection state management pattern verified against existing TrashListViewModel implementation
- ✅ check_icon.svg asset location identified in `Resources/Images/`
- ✅ Gesture coexistence confirmed: LongPressGestureRecognizer works alongside TapGestureRecognizer without conflicts
