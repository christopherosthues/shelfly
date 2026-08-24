# Research: Modernize Book List UI

**Feature**: `002-modernize-book-list-ui`
**Date**: 2026-08-24

## Technical Decisions

### Decision 1: Card Component Structure

**Decision**: Create a separate ContentView-based component (`BookCardView`) using a Grid inside a Border.

**Rationale**: 
- Provides reusable, testable card abstraction
- Separation of concerns between page layout and item presentation
- ContentView allows flexible content hosting while maintaining consistent styling
- Grid inside Border enables precise control over rounded corners (via Border.StrokeShape) and internal layout

**Alternatives considered**:
- Inline XAML in DataTemplate: Less reusable, harder to maintain consistent styling
- Frame element: Provides shadow/elevation but less customizable than Border+Grid combination
- FlexLayout: Good for responsive layouts but less precise control over card boundaries

### Decision 2: Responsive Layout Container

**Decision**: Use a Grid with conditional row/column definitions or FlexLayout for the top controls area (search bar + sort picker).

**Rationale**:
- .NET MAUI's FlexLayout supports `Wrap` and `Direction` properties ideal for responsive side-by-side/stacked behavior
- Grid provides explicit control over element placement but requires manual breakpoint handling
- FlexLayout is simpler for automatic reflow based on available space

**Alternatives considered**:
- Pure Grid with fixed breakpoints: More control but requires code-behind or converters for layout switching
- VisualStateManager: Overhead for simple responsive behavior
- Custom LayoutManager: Unnecessary complexity for this use case

### Decision 3: Shadow/Elevation Implementation

**Decision**: Use Border.Shadow property with platform-appropriate shadow styling.

**Rationale**:
- MAUI's Shadow element supports Radius, Opacity, Brush, and Offset properties
- Provides consistent cross-platform elevation effects
- Aligns with existing Shadow style defined in Styles.xaml (Radius=15, Opacity=0.5)

**Alternatives considered**:
- Frame.Shadow: Similar capability but Frame is a higher-level container
- Platform-specific renderers: More control but increased maintenance burden
- CSS-like styling via resources: Less direct control over shadow parameters

## MAUI-Specific Findings

### ContentView Inheritance Pattern

Based on existing codebase patterns:
- `ShelflyContentPageBase` demonstrates the inheritance pattern used for pages
- Custom views should follow similar conventions (code-behind + XAML)
- No existing ContentView-based components found — this will be the first reusable view component

### Border Styling Capabilities

From Styles.xaml analysis:
- Default Border style sets `Stroke`, `StrokeShape="Rectangle"`, and `StrokeThickness=1`
- For rounded corners, use `StrokeShape="RoundRectangle"` or custom Shape
- MAUI 10 supports advanced shape options including corner radius control

### SwipeView Compatibility with ContentView

Verified from BookListPage.xaml:
- SwipeView wraps the item content directly in DataTemplate
- A ContentView can be placed inside SwipeView as the visual container
- GestureRecognizers and bindings remain functional when wrapped in ContentView

## Unknowns Resolved

| Unknown | Resolution |
|---------|------------|
| Card component structure | ContentView inheriting from Grid inside Border |
| Responsive layout approach | FlexLayout for automatic reflow |
| Shadow implementation | Border.Shadow with existing style conventions |
| Horizontal margin specification | 16 units on left and right sides (FR-005) |
