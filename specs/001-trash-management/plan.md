# Implementation Plan: Trash Management

**Branch**: `[001-trash-management]` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-trash-management/spec.md`

## Summary

Add a Trash view accessible via Shell flyout navigation, displaying all soft-deleted books and their associated bookmarks. Users can restore items (clear `DeletedAt`) or permanently delete them via swipe gestures, toolbar actions, or multi-item selection. The trash list mirrors the book list's search and sorting capabilities.

## Technical Context

**Language/Version**: C# / .NET 10.0

**Primary Dependencies**: 
- `.NET MAUI` (client UI)
- `CommunityToolkit.Mvvm` (MVVM patterns)
- `EF Core` with `Npgsql` (data access)
- `HotChocolate` (GraphQL server - API side, minimal currently)

**Storage**: SQLite on client (`LocalDbContext`), PostgreSQL on server (future)

**Testing**: TUnit framework with Shouldly assertions

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Mobile + API (three-project solution: Shelfly.App, Shelfly.Api, Shelfly.Common)

**Performance Goals**: Search results within 1 second; sort changes within 500ms; batch operations on 50 items within 3 seconds

**Constraints**: 
- Shell flyout navigation required for Library/Trash switching
- Soft deletion via `DeletedAt` timestamp (Constitution Principle V)
- MVVM pattern with `ShelflyViewModelBase` and `ShelflyContentPageBase`
- Result pattern for error handling (no exceptions)

**Scale/Scope**: Single-user local database; trash view displays all soft-deleted items without pagination requirements

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|------------|-------|
| I. SOLID & SoC | Pass | Trash logic isolated in new service; domain models remain framework-agnostic |
| II. Vertical Slice | Pass | New feature directory under `Features/Trash/` with page, ViewModel, service co-located |
| III. MVVM Pattern | Pass | Pages inherit `ShelflyContentPageBase`; ViewModels inherit `ShelflyViewModelBase`; Shell navigation via flyout |
| IV. Coding Standards | Pass | Explicit types, collection expressions, primary constructors, Result pattern |
| V. Data Management | Pass | Soft deletion via `DeletedAt`; global query filters; `IgnoreQueryFilters` for trash queries; cascade delete on hard removal |
| VI. API Design | N/A | API project minimal; endpoints deferred to future implementation |
| VII. Auth & User Mgmt | N/A | Single-user local app; no auth changes required |
| VIII. Localization | Pass | New UI text added to `AppResources.resx` (en-US, de-DE) |
| IX. Asset Formats | Pass | SVG icons for toolbar and swipe actions |
| X. Microsoft Docs | Pass | MAUI Shell flyout patterns verified via microsoft-learn MCP |
| XI. IDE Refactoring | N/A | Applicable during implementation phase |

## Project Structure

### Documentation (this feature)

```text
specs/001-trash-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/
│   ├── Library/                    # Existing feature (reference for patterns)
│   │   ├── Pages/
│   │   │   └── BookListPage.xaml   # Template for trash list UI
│   │   ├── ViewModels/
│   │   │   └── BookListViewModel.cs # Template for search/sort logic
│   │   └── Services/
│   │       ├── LibraryService.cs    # Extended with trash methods
│   │       ├── SortCriterion.cs     # Reused for trash sorting
│   │       └── SortDirection.cs     # Reused for trash sorting
│   └── Trash/                      # NEW feature directory
│       ├── Pages/
│       │   ├── TrashListPage.xaml        # Main trash list view
│       │   ├── TrashBookDetailPage.xaml  # Read-only book details
│       │   └── TrashBookmarkDetailPage.xaml # Read-only bookmark note display
│       ├── ViewModels/
│       │   ├── TrashListViewModel.cs     # Search, sort, selection, batch ops
│       │   ├── TrashBookDetailViewModel.cs # Read-only book info
│       │   └── TrashBookmarkDetailViewModel.cs # Note-only display
│       └── Services/
│           └── TrashService.cs         # Restore, hard delete, cascade logic
├── Pages/
│   └── ShelflyContentPageBase.cs     # Base page lifecycle (extended)
├── ViewModels/
│   └── ShelflyViewModelBase.cs       # Base ViewModel (extended for selection state)
├── Controls/
│   ├── BookCardView.xaml             # Reused in trash list
│   └── TrashBookCardView.xaml        # NEW: Read-only variant with swipe gestures
├── Resources/
│   ├── Styles/Styles.xaml            # Extended with trash-specific styles
│   └── Raw/                          # SVG icons for restore/delete actions
├── AppShell.xaml                     # Modified to add flyout items
├── Routes.cs                         # Extended with trash routes
└── MauiProgram.cs                    # Extended with DI registrations

Shelfly.App.Data/
├── Entities/
│   ├── BookEntity.cs                 # DeletedAt field (existing)
│   └── BookmarkEntity.cs            # No change needed (follows parent book)
└── LocalDbContext.cs                # Global query filter on BookEntity.DeletedAt

Shelfly.Common/
├── Book.cs                           # Domain model with DeletedAt
└── Bookmark.cs                       # Domain model (no DeletedAt)
```

**Structure Decision**: The feature follows vertical slice architecture (Constitution Principle II). All trash-related code lives under `Features/Trash/` with pages, ViewModels, and services co-located. The existing Library feature serves as the reference implementation for search/sort patterns. Shell flyout navigation replaces the current single-content AppShell to support Library/Trash switching.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New read-only detail pages | Trash items need distinct UI from editable library items | Reusing existing detail pages would require conditional edit/disable logic, violating Principle III (MVVM clarity) |
| Separate TrashService | Isolates trash-specific lifecycle (restore vs. soft-delete) from LibraryService | Adding methods to LibraryService would blur the boundary between active management and recovery operations |
