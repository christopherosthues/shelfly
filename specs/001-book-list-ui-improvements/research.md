# Research: Book List UI Improvements

**Date**: 2026-09-02 | **Branch**: `001-book-list-ui-improvements`

## Decisions

### Decision 1: Page Title Binding Approach

**Chosen**: Bind directly to localized resource in XAML using `{x:Static resx:AppResources.BookListPageTitle}`

**Rationale**: Follows the existing pattern used by `BookDetailPage.xaml` (line 13), which binds title to a static resource. Simpler than creating a ViewModel property for a constant string, and keeps the page self-contained. The BookEditPage uses a dynamic ViewModel property only because the title changes based on edit mode — not applicable here.

**Alternatives considered**:
- ViewModel `PageTitle` property (used by BookEditPage) — unnecessary complexity for a static title
- Hardcoded string in XAML — violates Constitution VIII (localization requirement)

### Decision 2: Sort Option Localization Pattern

**Chosen**: Create a display class (`SortOptionDisplay`) with localized string property; ViewModel exposes `List<SortOptionDisplay>` instead of raw enum values. The Picker binds to this list and displays the localized strings, while internally mapping back to the `SortCriterion` enum for service calls.

**Rationale**: Maintains type safety (enum still used for sorting logic) while providing localized display text. Follows the MVVM pattern — ViewModel transforms domain data into presentation-friendly format. No ValueConverter needed, keeping XAML simple and compile-time validated via source generation.

**Alternatives considered**:
- `IValueConverter` to convert enum → localized string — adds runtime overhead and harder to test; breaks XAML source generation validation
- Dictionary mapping in ViewModel — less type-safe than a dedicated class
- Extending the enum with display attributes — requires reflection or custom attribute processing at runtime

### Decision 3: Sort Direction Toggle UI

**Chosen**: ImageButton placed adjacent to the sort picker, displaying SVG arrow icons (↑ for ascending, ↓ for descending). Tapping toggles direction and refreshes the book list. The icon source binds to a ViewModel property that returns the appropriate SVG filename based on current direction.

**Rationale**: Consistent with existing UI patterns — `ImageButton` is already used for FAB actions in BookListPage (lines 132-143). SVG icons follow Constitution IX. Single-tap interaction meets SC-002 success criterion. Visual state clearly indicates current sort direction via icon change.

**Alternatives considered**:
- Switch control — less intuitive for sort direction; typically used for boolean settings
- Direction labels appended to picker items (Option B from clarification) — clutters the picker and requires longer text per option
- Separate button labeled "Reverse Sort" — takes more screen space than an icon-only ImageButton

### Decision 4: Icon Design for Sort Arrows

**Chosen**: Simple arrow SVG icons following existing icon style in `Resources/Images/`. Icons named `sort_asc.svg` (upward arrow) and `sort_desc.svg` (downward arrow). Both use the same visual style as existing action icons (`add_icon.svg`, `edit_icon.svg`, etc.).

**Rationale**: SVG format per Constitution IX. Consistent naming convention with existing icons. Minimal design — arrows are universally recognized sort direction indicators across languages, reducing localization burden.

**Alternatives considered**:
- Text-based labels (A→Z / Z→A) — requires localization and takes more space
- System glyph icons — less consistent with project's SVG icon approach

## Dependencies

### Existing Code References

| File | Line | Relevance |
|------|------|-----------|
| `BookListPage.xaml` | 13 | Current (incorrect) title binding to `AppResources.BookListPageSortByTitle` |
| `BookListPage.xaml` | 45-59 | Sort Picker definition — needs ItemsSource change and adjacent toggle button |
| `BookListViewModel.cs` | 25 | `SortCriterion` property — needs companion `SortDirection` property |
| `BookListViewModel.cs` | 36 | `SortOptions` returns raw enum values — needs localization wrapper |
| `LibraryService.cs` | 244-250 | `SortCriterion` enum definition — no changes needed, but referenced by display class |
| `AppResources.resx` | 60-71 | Existing sort label resources (`SortByTitle`, etc.) — available for picker item localization |

### New Resources Required

| Resource Key | English Value | German Value (placeholder) | Purpose |
|--------------|---------------|----------------------------|---------|
| `BookListPageTitle` | "My Library" | [TBD] | Page title replacement |
| `SortDirectionAscending` | "Ascending" | [TBD] | Accessibility description for ascending state |
| `SortDirectionDescending` | "Descending" | [TBD] | Accessibility description for descending state |

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| German translations missing | Partial localization coverage | Add placeholder values; mark for translation review |
| Icon visual inconsistency | UX quality | Match existing icon style (stroke width, color scheme) |
| Picker item display regression | Sort options show wrong text | Unit test verifies localized strings are returned correctly |
