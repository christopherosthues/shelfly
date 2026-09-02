# Implementation Plan: FAB Add Button

**Branch**: `[008-fab-add-button]` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/008-fab-add-button/spec.md`

## Summary

Replace the toolbar add button on the book list page with a floating action button (FAB) positioned at the bottom-right corner. Since .NET MAUI lacks a native FAB control, implement it using an `ImageButton` inside a circular `BoxView` container anchored to the bottom-right of the page layout. The FAB must reposition upward when the keyboard appears and maintain positioning across all screen sizes and orientations.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm (ObservableObject, RelayCommand), XAML source generation

**Storage**: N/A — UI-only change, no new data persistence

**Testing**: TUnit framework with Shouldly assertions (per constitution)

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Cross-platform mobile/desktop MAUI application

**Performance Goals**: FAB reposition within 100ms of keyboard appearance; tap navigation completes without perceptible delay

**Constraints**: Must follow MVVM pattern with ShelflyContentPageBase inheritance; all text localized via .resx resources; XAML source generation enabled

**Scale/Scope**: Single page modification (BookListPage) — layout restructuring and FAB control addition

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | Pass | UI change isolated to BookListPage; no domain model impact |
| II. Vertical Slice Architecture | Pass | Feature code stays within `Shelfly.App/Features/Library/Pages/` |
| III. MVVM Pattern (Client) | Pass | FAB command binds to existing `NavigateToAddBookCommand`; page inherits ShelflyContentPageBase |
| IV. Coding Standards | Pass | Explicit types, collection expressions, nullable enabled |
| VIII. Localization & Internationalization | Pass | FAB semantic properties use existing resource keys (`BookListPageAddNewBookDescription`) |
| IX. Asset & Resource Formats | Pass | Icon asset `add_icon.svg` already exists in SVG format |

## Project Structure

### Documentation (this feature)

```text
specs/008-fab-add-button/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/
│   └── Library/
│       └── Pages/
│           ├── BookListPage.xaml          # Modified: remove ToolbarItem, add FAB layout
│           └── BookListPage.xaml.cs      # Modified: keyboard visibility handling
└── Resources/
    └── Localization/
        ├── AppResources.resx              # Existing keys reused for FAB semantics
        └── AppResources.de.resx           # Existing keys reused for FAB semantics
```

**Structure Decision**: Single-page modification within the existing Library feature slice. The FAB is implemented directly in BookListPage XAML using a `BoxView` + `ImageButton` composition anchored via absolute layout positioning. No new files or controls required — the existing `add_icon.svg` asset and localization keys are reused from the toolbar implementation.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | All constitution principles satisfied without deviation | — |
