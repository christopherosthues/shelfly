# Research: FAB Add Button Implementation

**Date**: 2026-09-02  
**Feature**: specs/008-fab-add-button/spec.md

## Decisions & Findings

### Decision 1: FAB Control Composition

**Decision**: Use `BoxView` + `ImageButton` to create a circular floating action button

**Rationale**: .NET MAUI does not include a native FAB control. The most idiomatic approach is to compose existing controls:
- A `BoxView` with circular shape (using `HeightRequest`, `WidthRequest`, and `HorizontalOptions="End"` / `VerticalOptions="End"`) provides the circular background
- An `ImageButton` centered within the BoxView displays the add icon and handles tap commands

**Alternatives considered**:
- Custom renderer for a native FAB — overkill for a single-page change
- Third-party NuGet package (e.g., MaterialFAB) — requires explicit approval per constitution dependency policy
- AbsoluteLayout with positioned elements — provides precise anchoring control

### Decision 2: Keyboard Repositioning Strategy

**Decision**: Subscribe to `KeyboardShowing`/`KeyboardHidden` events on the page's main Grid to detect keyboard visibility and adjust FAB position accordingly

**Rationale**: MAUI provides keyboard lifecycle events via `Element.KeyboardShowing` and `Element.KeyboardHidden`. By subscribing to these events in the code-behind, we can:
- Calculate the keyboard height from event arguments
- Animate the FAB upward by that amount minus safe area padding
- Restore original position when keyboard dismisses

**Alternatives considered**:
- Hide FAB during keyboard visibility — simpler but less discoverable (user's choice was "move upward")
- Use a `VisualElement` with `InputTransparent` — not relevant to positioning
- Platform-specific handlers — adds complexity without benefit for this use case

### Decision 3: Layout Anchoring Approach

**Decision**: Use `AbsoluteLayout` as the root container to enable precise bottom-right anchoring of the FAB

**Rationale**: AbsoluteLayout allows absolute positioning of child elements regardless of their natural layout behavior. This ensures:
- The FAB stays anchored to bottom-right across all orientations and screen sizes
- The content area (search, picker, collection view) fills remaining space
- Keyboard repositioning only affects the FAB element

**Alternatives considered**:
- Grid with overlay rows — more complex row/column definitions
- FlexLayout — less precise control over absolute positioning
- RelativeLayout — deprecated in favor of AbsoluteLayout for this use case

### Decision 4: Theme Adaptation

**Decision**: Use `AppThemeBinding` on BoxView background color to adapt FAB appearance between light/dark modes

**Rationale**: The constitution requires theme adaptation (User Story 2). AppThemeBinding provides declarative theming without code-behind logic, using the existing Primary/White color resources already defined in the app.

**Alternatives considered**:
- DynamicResource bindings — more verbose, requires additional resource definitions
- Code-behind theme detection — adds runtime overhead for a static property

## Technical References

### MAUI Keyboard Events

Keyboard visibility events are available on `VisualElement`:
```csharp
// In code-behind constructor or OnNavigatedTo
this.KeyboardShowing += (s, e) => { /* reposition FAB upward */ };
this.KeyboardHidden += (s, e) => { /* restore FAB position */ };
```

### AbsoluteLayout Positioning

FAB anchoring uses absolute layout flags:
```xml
<AbsoluteLayout>
    <BoxView 
        AbsoluteLayout.LayoutBounds="0, 0, 56, 56"
        AbsoluteLayout.LayoutFlags="PositionProportional"
        BackgroundColor="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource White}}" />
</AbsoluteLayout>
```

### Existing Resources Available

- Icon: `add_icon.svg` (already used in toolbar)
- Localization keys: `BookListPageAddNewBookDescription`, `BookListPageAddBookButtonText`
- Color resources: `Primary`, `White` (available via AppThemeBinding)
