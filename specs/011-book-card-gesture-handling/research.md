# Research: Book Card Gesture Commands

**Date**: 2026-09-04  
**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Decisions

### Decision 1: Long Press Detection Approach (.NET 10)

**Chosen**: Use `PointerGestureRecognizer` with timing logic (press/release timestamp comparison)

**Rationale**: 
- `LongPressGestureRecognizer` is available in .NET 11 only, not .NET 10
- The existing implementation already uses this pattern with a 500ms threshold
- `PointerGestureRecognizer` provides `PointerPressed` and `PointerReleased` events to detect press duration
- No additional NuGet package required

**Alternatives considered**:
- **CommunityToolkit TouchBehavior**: Provides `LongPressCommand` and `LongPressDuration` properties, but requires adding a behavior to the control's Behaviors collection. Less flexible for custom control scenarios where gesture state must be managed internally.
- **DispatcherTimer-based approach**: More complex; requires timer start/stop logic on press/release. Pointer timing is simpler and more reliable.
- **Upgrade to .NET 11**: Would enable `LongPressGestureRecognizer` directly, but introduces framework migration risk beyond current scope.

**References**:
- [Tap Gesture (.NET MAUI)](https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/tap?view=net-maui-10.0)
- [Pointer Gesture (.NET MAUI)](https://learn.microsoft.com/dotnet/maui/fundamentals/gestures/pointer?view=net-maui-10.0)
- [LongPressGestureRecognizer Class (.NET 11)](https://learn.microsoft.com/dotnet/api/microsoft.maui.controls.longpressgesturerecognizer?view=net-maui-11.0)

### Decision 2: Command BindableProperty Pattern

**Chosen**: Add `ICommand` BindableProperties on BookCardView (`LongPressCommand`, `TapCommand`) that fire with `BindingContext` as parameter

**Rationale**:
- Follows established MAUI pattern (same as `TapGestureRecognizer.Command`)
- Enables XAML binding from page-level templates to view model commands
- Maintains separation of concerns: control handles gesture detection, view model handles business logic
- Consistent with existing `IsSelected` BindableProperty pattern

**Alternatives considered**:
- **Event-based approach**: Raising custom events (`LongPressed`, `Tapped`) instead of commands. Less idiomatic for MAUI/XAML binding scenarios where ICommand is the standard.
- **Behavior-based approach**: Using CommunityToolkit behaviors. Adds external dependency and complexity to a simple control.

### Decision 3: Tap Gesture Coexistence with Long Press

**Chosen**: The existing `TapGestureRecognizer` in BookCardView constructor fires only when card is unselected; selected cards use tap for deselection only

**Rationale**:
- Matches user expectation: tap on unselected = navigate, tap on selected = deselect
- Prevents navigation interference during selection mode
- Long press toggles selection regardless of current state (per clarification)

**Alternatives considered**:
- **Always fire tap command**: Would cause unwanted navigation when user intends to deselect. Requires page-level logic to filter.
- **Separate commands for selected/unselected tap**: Adds complexity; single `TapCommand` with conditional firing is simpler.

### Decision 4: Gesture Threshold Timing

**Chosen**: Keep existing 500ms threshold constant (`LongPressThreshold = 500`)

**Rationale**:
- Matches platform default long press duration (approximately 400-600ms range)
- Existing implementation already uses this value
- Provides clear distinction between tap (< 500ms) and long press (>= 500ms)

**Alternatives considered**:
- **Platform-specific thresholds**: Would require conditional logic per platform. Adds complexity for marginal UX improvement.
- **Configurable threshold via BindableProperty**: Over-engineered for current scope; fixed value is sufficient.

## Dependencies Verified

| Dependency | Status | Notes |
|------------|--------|-------|
| `PointerGestureRecognizer` | Available (.NET 10) | Built into Microsoft.Maui.Controls |
| `TapGestureRecognizer` | Available (.NET 10) | Replaces deprecated ClickGestureRecognizer |
| `ICommand` BindableProperty pattern | Standard MAUI | Used by TapGestureRecognizer, SwipeView, etc. |
| View model commands (`EnterSelectionMode`, `ToggleSelection`) | Already exist | BookListViewModel.cs lines 82-106 |
| Navigation command (`NavigateToDetailBookCommand`) | Already exists | BookListViewModel.cs lines 69-73 |

## Platform Compatibility Notes

| Platform | Pointer Events | Tap Gesture | Long Press Detection |
|----------|----------------|-------------|---------------------|
| Android | Full support | Supported | Timing-based (500ms) |
| iOS/MacCatalyst | Full support | Supported | Timing-based (500ms) |
| Windows | Full support | Supported | Timing-based (500ms) |

**Secondary button behavior note**: On iOS/MacCatalyst, secondary pointer press may fire `PointerPressed` followed immediately by `PointerReleased`. The timing logic should account for this edge case (per Microsoft docs).
