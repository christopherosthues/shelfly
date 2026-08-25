# Implementation Plan: Swipe-to-Edit Book

**Branch**: `003-swipe-edit-book` | **Date**: 2026-08-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/003-swipe-edit-book/spec.md`

## Summary

Add a swipe-to-edit action to the book list view using `SwipeView.LeftItems`. Swiping left on a book item reveals an edit action element (icon + localized text). Tapping this element navigates to the existing `BookEditPage` with the book's ID as a parameter. The implementation mirrors the existing swipe-to-delete pattern which uses `SwipeView.RightItems`.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm, MAUI SwipeView control

**Storage**: PostgreSQL via EF Core (existing Book entity)

**Testing**: TUnit + Shouldly for unit tests; TestContainers for integration tests

**Target Platform**: Android always, iOS/MacCatalyst on non-Linux, Windows conditionally

**Project Type**: Mobile app (.NET MAUI client)

**Performance Goals**: Swipe action element revealed within 500ms of gesture completion

**Constraints**: Touch-only input required on Windows; swipe must not conflict with horizontal page navigation

**Scale/Scope**: Single feature addition to existing book list UI; no new entities or API endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance |
|-----------|------------|
| **I. SOLID & Separation of Concerns** | ✅ MVVM pattern maintained; view model handles navigation command, XAML defines swipe UI |
| **II. Vertical Slice Architecture** | ✅ Feature code co-located in `Features/Library/` alongside existing book list |
| **III. MVVM Pattern (Client)** | ✅ Command bound via `RelayCommand`; page inherits from base classes; DI registered via Shell routes |
| **IV. Coding Standards** | ✅ Explicit types, collection expressions, primary constructors, nullable enabled |
| **VIII. Localization & Internationalization** | ✅ Action element text localized via `.resx` (English + German) |
| **X. Microsoft Documentation Sourcing** | ✅ SwipeView usage grounded in official MAUI documentation |

## Project Structure

### Documentation (this feature)

```text
specs/003-swipe-edit-book/
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
│       ├── Pages/
│       │   └── BookListPage.xaml          # Modified: add SwipeView.LeftItems with edit action
│       └── ViewModels/
│           └── BookListViewModel.cs        # Modified: add NavigateToEditBookCommand
├── Resources/
│   ├── Strings/
│   │   ├── en-US/AppResources.resx         # Added: swipe-to-edit localized text
│   │   └── de-DE/AppResources.resx         # Added: swipe-to-edit localized text (German)
│   └── Raw/
│       └── edit_icon.svg                   # Added: SVG icon for edit action element
├── Controls/
│   └── BookCardView.xaml                   # Unchanged: existing card control
```

**Structure Decision**: The feature modifies the existing `BookListPage` and its view model within the Library vertical slice. No new files are created outside this boundary except localization strings and an SVG icon asset. This follows the Vertical Slice Architecture principle by keeping all changes co-located with the existing book list feature.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | Feature is a straightforward addition to existing SwipeView infrastructure | — |
