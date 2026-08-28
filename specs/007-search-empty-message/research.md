# Research: Search Empty Message

## Decisions

### Decision 1: Conditional EmptyView Binding Approach

**Rationale**: The `CollectionView.EmptyView` in MAUI supports data binding. By introducing a computed property on the view model that returns the appropriate localized message based on search state, the XAML can bind to this single property without requiring complex converters or triggers.

**Alternatives considered**:
- **DataTrigger in XAML**: Would require CommunityToolkit.Maui's `DataTrigger` to switch between two `Label` elements based on a boolean condition. More verbose and harder to maintain localization keys inline.
- **Two separate EmptyView templates**: MAUI doesn't natively support conditional template selection without third-party libraries.
- **Chosen approach**: Single bound property returning the correct localized string — simplest, most idiomatic MVVM pattern.

### Decision 2: Search State Detection

**Rationale**: The view model already tracks `SearchQuery` via `[ObservableProperty]`. A simple computed check (`string.IsNullOrWhiteSpace(SearchQuery)`) determines whether a search is active. No additional state tracking required.

**Alternatives considered**:
- **Dedicated boolean flag `IsSearching`**: Adds redundant state that must be kept in sync with `SearchQuery`. Unnecessary complexity for this feature.
- **Chosen approach**: Derive search state directly from the existing `SearchQuery` property.

## Dependencies

| Dependency | Status | Notes |
|------------|--------|-------|
| CommunityToolkit.Maui | Already present | Used for `EventToCommandBehavior`, `IsNotNullConverter` — no new packages needed |
| Localization (.resx) | Already present | Standard MAUI resource file pattern; keys added to both en-US and de-DE |

## Summary

No NEEDS CLARIFICATION items remain. The feature is a focused UI change with:
1. One new view model property (computed empty state message)
2. Two new localization keys (search-empty vs. standard-empty messages)
3. One XAML binding update (EmptyView Label bound to the new property)
