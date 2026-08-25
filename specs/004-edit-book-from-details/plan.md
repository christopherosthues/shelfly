# Implementation Plan: Edit Book from Details Page

**Branch**: `004-edit-book-from-details` | **Date**: 2026-08-25 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/004-edit-book-from-details/spec.md`

## Summary

Add an edit button to the book details page (`BookDetailPage`) that navigates to the existing `BookEditPage`, passing the current book's identifier. The feature reuses all existing editing infrastructure — no new backend endpoints, validation rules, or data models are required. When the user navigates away without saving, edited fields ARE discarded (no draft persistence). Save failures use the Result pattern with user-facing error messages; the app MUST NOT crash.

**Expanded scope**: `BookEditViewModel` currently does not load book data when navigating to edit an existing book. The view model inherits from `ObservableObject` (not `ShelflyViewModelBase`) and lacks `IQueryAttributable` implementation, so navigation query parameters are never consumed. This feature must implement proper book loading in BookEditViewModel following the same pattern as BookDetailViewModel — using `LoadAsync` to fetch existing book data via `LibraryService.GetBookByIdAsync()`.

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: CommunityToolkit.Mvvm (`RelayCommand`), MAUI Shell navigation, existing `BookEditPage` infrastructure

**Storage**: PostgreSQL via EF Core (existing Book entity)

**Testing**: TUnit with Shouldly assertions; TestContainers for integration tests

**Target Platform**: Multi-platform MAUI (Android always, iOS/MacCatalyst on non-Linux, Windows conditionally)

**Project Type**: Mobile/desktop application with API backend

**Performance Goals**: Navigation to edit form completes within one tap; edited data reflected on details page within 1 second after save

**Constraints**: Result pattern for error handling (no exceptions); app MUST NOT crash on save failure; localization via `.resx` files (en-US, de-DE)

**Scale/Scope**: Two-part change: (1) Add ToolbarItem to BookDetailPage and corresponding command in BookDetailViewModel; (2) Implement book loading in BookEditViewModel by inheriting from ShelflyViewModelBase, implementing IQueryAttributable, and adding LoadAsync to fetch existing book data via LibraryService

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | Pass | Common domain models unchanged; UI change isolated to Shelfly.App |
| II. Vertical Slice Architecture | Pass | Change contained within Library and BookEditor feature boundaries |
| III. MVVM Pattern (Client) | Pass | BookEditViewModel refactored to inherit from ShelflyViewModelBase, implement IQueryAttributable, override LoadAsync — follows constitution requirements |
| IV. Coding Standards | Pass | Result pattern used for save failures; explicit types, no var |
| V. Data Management | Pass | Existing Book entity reused; UUID v7 identifiers maintained |
| VI. API Design & Versioning | Pass | No new endpoints; existing update flow sufficient |
| VII. Authentication & User Management | Pass | JWT validation unchanged; same auth context flows through navigation |
| VIII. Localization & Internationalization | Pass | New button text added to AppResources.resx (en-US, de-DE) |
| IX. Asset & Resource Formats | N/A | No new image assets required |
| X. Microsoft Documentation Sourcing | Pass | MAUI Shell navigation patterns verified via official docs |
| XI. IDE-Assisted Refactoring | Consider | Base class change for BookEditViewModel may benefit from Rider MCP refactoring tools |

## Project Structure

### Documentation (this feature)

```text
specs/004-edit-book-from-details/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
Shelfly.App/
└── Features/
    └── Library/
        ├── Pages/
        │   └── BookDetailPage.xaml          # Add ToolbarItem for edit button
        └── ViewModels/
            └── BookDetailViewModel.cs       # Add EditBookCommand

Shelfly.App/
└── Features/
    └── BookEditor/
        └── ViewModels/
            └── BookEditViewModel.cs         # Inherit from ShelflyViewModelBase, implement IQueryAttributable, add LoadAsync to load existing book data

Shelfly.App/
└── Resources/
    └── Strings/
        ├── en-US/AppResources.resx           # Add edit button text key
        └── de-DE/AppResources.resx          # Add German translation
```

**Structure Decision**: Two-project change within Shelfly.App. The feature extends the existing Library vertical slice by adding a ToolbarItem to BookDetailPage and a corresponding command in BookDetailViewModel. Additionally, BookEditViewModel must be refactored to inherit from `ShelflyViewModelBase` (enabling lifecycle hooks), implement `IQueryAttributable` (to receive navigation query parameters), and override `LoadAsync` to fetch existing book data via `LibraryService.GetBookByIdAsync()` — matching the pattern used by BookDetailViewModel. Navigation reuses the established Shell route pattern (`Routes.BookEditPage`) with query parameter passing (book identifier).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| BookEditViewModel base class change | Constitution principle III requires ShelflyViewModelBase inheritance for lifecycle management; current ObservableObject inheritance lacks LoadAsync and IQueryAttributable support | Keeping ObservableObject would require manual parameter extraction and data loading at call site, breaking the established page lifecycle pattern |
