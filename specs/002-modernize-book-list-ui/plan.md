# Implementation Plan: Modernize Book List UI

**Branch**: `002-modernize-book-list-ui` | **Date**: 2026-08-24 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-modernize-book-list-ui/spec.md`

## Summary

Modernize the book list page UI by (1) moving the sort picker from the bottom to the top alongside the search bar with responsive layout adaptation, and (2) wrapping each book list item in a reusable card component (`BookCardView`) with rounded corners, shadow effects, and 16-unit horizontal margins.

## Technical Context

**Language/Version**: C# / .NET 10.0 (MAUI Controls 10.0.100)

**Primary Dependencies**: 
- Microsoft.Maui.Controls
- CommunityToolkit.Maui (EventToCommandBehavior, converters)
- CommunityToolkit.Mvvm (ObservableObject, RelayCommand)

**Storage**: SQLite with EF Core (no schema changes for this feature)

**Testing**: TUnit framework + Shouldly assertions (per AGENTS.md testing stack)

**Target Platform**: Android, iOS, Windows, macOS (cross-platform via .NET MAUI)

**Project Type**: Mobile/Desktop application (.NET MAUI client)

**Performance Goals**: Layout adaptation within 300ms of screen dimension change; smooth scrolling with card-wrapped items

**Constraints**: 
- Maintain existing swipe-to-delete functionality
- Preserve accessibility semantic properties
- Cards must have exactly 16 units horizontal margin on left and right sides
- Card component must inherit from ContentView using Grid inside Border

**Scale/Scope**: Single page UI modernization (BookListPage); one new reusable component (BookCardView)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Based on project principles from AGENTS.md:

| Principle | Status | Notes |
|-----------|--------|-------|
| Nullable reference types enabled | ✅ Pass | New component will use explicit `?` annotations |
| No `var` preference | ✅ Pass | Code style maintained in new files |
| Primary constructors preferred | ✅ Pass | Component code-behind will follow convention |
| Collection expressions syntax | ✅ Pass | Where applicable |
| XAML source generation enabled | ✅ Pass | New component uses XAML + code-behind pattern |
| FluentValidation for requests | ✅ N/A | UI-only feature, no request validation changes |
| Result pattern (no custom exceptions) | ✅ Pass | Consistent with existing error handling |

**Post-Design Re-evaluation**: All gates remain satisfied. No constitution violations introduced by the design decisions in research.md.

## Project Structure

### Documentation (this feature)

```text
specs/002-modernize-book-list-ui/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Controls/                    # NEW directory for reusable components
│   └── BookCardView.xaml        # Card component (ContentView + Border + Grid)
│   └── BookCardView.xaml.cs     # Component code-behind
├── Features/
│   └── Library/
│       ├── Pages/
│       │   └── BookListPage.xaml      # MODIFIED: responsive layout, card usage
│       │   └── BookListPage.xaml.cs   # Potentially modified for layout logic
│       └── ViewModels/
│           └── BookListViewModel.cs   # Likely unchanged (purely presentational)
├── Resources/
│   ├── Styles/
│   │   └── Styles.xaml              # MODIFIED: card styles, shadow configuration
│   └── Images/                      # Existing icons (add_icon.svg, export_icon.svg)
```

**Structure Decision**: New `Controls/` directory created at the app root level to house reusable ContentView-based components. This follows MAUI conventions for custom controls and keeps them discoverable alongside existing Resources and Features directories. The BookCardView component is placed here because it may be reused in other list views in the future (e.g., bookmark lists, trash management).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New Controls directory | First reusable ContentView component; establishes pattern for future UI components | Inline XAML in DataTemplate would work but lacks reusability and testability |
