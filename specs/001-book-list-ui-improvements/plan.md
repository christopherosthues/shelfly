# Implementation Plan: Book List UI Improvements

**Branch**: `001-book-list-ui-improvements` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-book-list-ui-improvements/spec.md`

## Summary

Three improvements to the book list page in the MAUI client:
1. Replace the incorrect page title (currently shows "Title" — the sort picker's label) with a proper localized page title
2. Localize all sort option values displayed in the picker (currently raw enum names appear regardless of device language)
3. Add ascending/descending sort direction toggle via a toggle arrow icon adjacent to the sort picker

All changes are client-side only (`Shelfly.App`). No API or Common library changes required.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm (ObservableProperty, RelayCommand), Microsoft.Maui.Controls (Picker, ImageButton)

**Storage**: N/A — client-side UI state only; sort direction maintained in-memory during session per clarification

**Testing**: TUnit framework with Shouldly assertions (per constitution Testing Strategy)

**Target Platform**: .NET MAUI multi-target (Android always; iOS/MacCatalyst on non-Linux; Windows conditionally)

**Project Type**: Mobile/desktop application (.NET MAUI client)

**Performance Goals**: Sort direction toggle completes within one interaction (single tap); list reorder is imperceptible to user

**Constraints**: All UI text must be localized via `.resx` files (Constitution VIII); XAML source generation enabled (`MauiXamlInflator=SourceGen`); ViewModels inherit from `ShelflyViewModelBase`; Pages inherit from `ShelflyContentPageBase`

**Scale/Scope**: Single page modification (BookListPage); affects BookListViewModel and localization resources only

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | ✅ Pass | Changes isolated to Shelfly.App; no domain model changes |
| II. Vertical Slice Architecture | ✅ Pass | All changes within `Features/Library/` boundary |
| III. MVVM Pattern (Client) | ✅ Pass | ViewModel property for sort direction; XAML binding for toggle icon |
| IV. Coding Standards | ✅ Pass | Explicit types, collection expressions, primary constructors |
| VIII. Localization & Internationalization | ✅ Pass | New resources added to both `AppResources.resx` and `AppResources.de.resx` simultaneously |
| IX. Asset & Resource Formats | ⚠️ Watch | New sort direction icons must be SVG format |

## Project Structure

### Documentation (this feature)

```text
specs/001-book-list-ui-improvements/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (N/A — client-only feature, no external contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/Library/
│   ├── Pages/
│   │   └── BookListPage.xaml          # Title binding fix + sort direction toggle UI
│   └── ViewModels/
│       └── BookListViewModel.cs       # SortDirection property + localized sort options
├── Resources/
│   ├── Localization/
│   │   ├── AppResources.resx          # New: page title, sort option labels, sort direction labels
│   │   └── AppResources.de.resx       # German translations for new resources
│   └── Images/
│       ├── sort_asc.svg               # Ascending sort arrow icon (↑)
│       └── sort_desc.svg              # Descending sort arrow icon (↓)
```

**Structure Decision**: All changes confined to `Shelfly.App` within the existing Library feature boundary. No new projects or modules created. The feature modifies existing files rather than adding new components, following the principle of minimal structural change for UI improvements. SVG icons added per Constitution IX.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | All changes follow existing patterns (MVVM binding, .resx localization) | — |
