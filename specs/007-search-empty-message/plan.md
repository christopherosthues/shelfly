# Implementation Plan: Search Empty Message

**Branch**: `007-search-empty-message` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/007-search-empty-message/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command; its definition describes the execution workflow.

## Summary

The book list page displays a generic "no books available" empty state regardless of whether the library is truly empty or a search returned zero results. The feature adds context-aware messaging: when a search query is active and yields no results, the empty view displays a message referencing the search context; otherwise, the standard "no books" message shows for genuinely empty libraries.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm, CommunityToolkit.Maui (EventToCommandBehavior), NLog

**Storage**: Local EF Core SQLite database (`LocalDbContext`) for book entities

**Testing**: TUnit framework with Shouldly assertions

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Mobile/desktop application (.NET MAUI client)

**Performance Goals**: Search debounce at 200ms; empty state transition must be immediate (no perceptible delay)

**Constraints**: All user-facing text MUST use `.resx` localization keys (en-US and de-DE). XAML source generation enabled (`MauiXamlInflator=SourceGen`). MVVM pattern with `ShelflyContentPageBase` / `ShelflyViewModelBase` inheritance.

**Scale/Scope**: Single page modification (BookListPage.xaml + BookListViewModel.cs); two new localization keys

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **III. MVVM Pattern** | ✓ Pass | Changes confined to BookListPage (View) and BookListViewModel (ViewModel). No direct Keycloak calls. Uses `ObservableProperty` for state binding. |
| **IV. Coding Standards** | ✓ Pass | Explicit types, collection expressions, nullable reference types enforced. No custom exceptions — Result pattern used where applicable. |
| **VIII. Localization** | ✓ Pass | Two new `.resx` keys required: one for "no books matched search" and one for standard "no books". Both en-US and de-DE translations needed. |
| **X. Microsoft Documentation** | ✓ Pass | MAUI `CollectionView.EmptyView` behavior well-established; no speculative APIs used. |

No violations detected. Feature is a focused UI enhancement with minimal architectural impact.

## Project Structure

### Documentation (this feature)

```text
specs/007-search-empty-message/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (NEEDS CLARIFICATION resolved)
├── data-model.md        # Phase 1 output (view model state + localization keys)
├── quickstart.md        # Phase 1 output (validation scenarios)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/Library/
│   ├── Pages/BookListPage.xaml          # EmptyView conditional binding update
│   └── ViewModels/BookListViewModel.cs  # Search state tracking property
├── Resources/Localization/
│   ├── en-US/AppResources.resx          # New key: BookListPageSearchEmptyMessage
│   └── de-DE/AppResources.resx          # New key: BookListPageSearchEmptyMessage (German)
```

**Structure Decision**: Single-project modification within the existing MAUI client (`Shelfly.App`). The feature touches only the Library feature's book list page and its view model, plus localization resources. No new files or directories created beyond updated resource files.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No constitution violations detected. Feature scope is minimal — single page UI modification with localization updates.
