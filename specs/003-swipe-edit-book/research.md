# Research: Swipe-to-Edit Book Implementation

**Date**: 2026-08-25
**Branch**: `003-swipe-edit-book`

## Decisions

### Decision: Use SwipeView.LeftItems for Edit Action

**Rationale**: The existing swipe-to-delete pattern uses `SwipeView.RightItems`. Using `LeftItems` provides a symmetric dual-action swipe experience (left = edit, right = delete) without conflicting gestures. This is the standard MAUI pattern for multi-action swipe lists.

**Alternatives considered**:
- Adding an edit button directly on BookCardView — increases visual clutter and requires tap rather than gesture discovery
- Using `RightItems` with multiple SwipeItem entries — works but less intuitive; left/right separation maps naturally to distinct actions

### Decision: Navigate to Existing BookEditPage (No Inline Editing)

**Rationale**: The spec clarifies that edit opens a full page, not inline editing. The existing `BookEditPage` already handles both create and edit operations via the optional `BookId` parameter. Reusing it avoids duplication.

**Alternatives considered**:
- Creating a new dedicated edit page — unnecessary overhead; current page is fully capable
- Inline editing within SwipeView using `SwipeItemView` — breaks MVVM separation and complicates validation

### Decision: Follow Existing Swipe-to-Delete Binding Pattern

**Rationale**: The current implementation binds commands via `{Binding Command, Source={x:RelativeSource AncestorType={x:Type local:BookListViewModel}}}`. Using the same pattern ensures consistency and leverages proven navigation mechanics.

**Alternatives considered**:
- Direct `Invoked` event handler — less testable and harder to mock in unit tests
- Behavior-based binding — adds unnecessary abstraction layer

## Technical Findings

### SwipeView.LeftItems API (Official MAUI Documentation)

| Property | Type | Purpose |
|----------|------|---------|
| `LeftItems` | `SwipeItems` | Collection of swipe items revealed on left swipe |
| `Mode` | `SwipeItemInvokeMode` | Controls execution: `Reveal` (tap to execute, default) or `Execute` (auto-execute on swipe) |

### Key Implementation Details

- **Command binding**: Use `{Binding Command}` with `Source={x:RelativeSource AncestorType=...}` to bind view model commands
- **Navigation parameter**: Pass book ID via `CommandParameter="{Binding Id}"` where `Id` is the BookEntity's identifier
- **Localization**: Text property must use `{x:Static resx:AppResources.BookListPageSwipeToEditCommand}` pattern matching existing delete command localization
- **Icon format**: SVG icon asset placed in `Resources/Raw/` directory, referenced via `IconImageSource="edit_icon.svg"`

### Platform Considerations

| Platform | SwipeView Support | Notes |
|----------|-------------------|-------|
| Android | Full touch support | Primary target platform |
| iOS/MacCatalyst | Full touch support | Available on non-Linux hosts |
| Windows | Touch-only | Mouse/pointer devices do not trigger swipes per official docs |

## Resolved Clarifications

All NEEDS CLARIFICATION items resolved:
- ✅ SwipeView API confirmed via Microsoft Learn documentation
- ✅ LeftItems pattern validated against existing RightItems implementation
- ✅ Navigation route and parameter passing mechanism verified in codebase
- ✅ Localization resource file locations identified (`Resources/Strings/en-US/AppResources.resx` and `de-DE/AppResources.resx`)
