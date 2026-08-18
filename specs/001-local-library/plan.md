# Implementation Plan: Local Library Management

**Branch**: `002-local-library` | **Date**: 2026-08-18 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/002-local-library/spec.md`

## Summary

The user manages books and bookmarks locally in the MAUI client app without requiring a profile or account. The feature provides book CRUD operations, bookmark management within each book, search/sort capabilities, swipe-to-delete soft deletion, inline validation errors, German/English localization, and JSON export for library backup. Data persists via local SQLite with EF Core.

## Technical Context

**Language/Version**: C# 12 / .NET 10 (MAUI multi-target: Android always, iOS/MacCatalyst on non-Linux, Windows conditionally)

**Primary Dependencies**: 
- `Microsoft.EntityFrameworkCore.Sqlite` (v10.0.11 — already in Directory.Packages.props)
- `CommunityToolkit.Mvvm` (v8.4.2 — already referenced)
- `CommunityToolkit.Maui` (already referenced for MAUI utilities)

**Storage**: Local SQLite database via EF Core (`Microsoft.Data.Sqlite` v10.0.11)

**Testing**: TUnit framework with Shouldly assertions (per constitution); unit tests in new test project

**Target Platform**: Android, iOS, Windows, MacOS (via .NET MAUI conditional targeting)

**Project Type**: Mobile + Desktop application (client-only; no server/networking for this feature)

**Performance Goals**: Search results within 500ms of typing query; swipe-to-delete gesture completes within 200ms with visual feedback

**Constraints**: Offline-capable; field length limits (notes ≤1000 chars, titles/authors/publishers ≤256 chars); ISBN uniqueness enforced locally; JSON export for backup purposes

**Scale/Scope**: Single-user local library; no explicit upper bound on book/bookmark count (~10,000 books practical limit via device storage)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|------------|-------|
| **I. SOLID & Separation of Concerns** | ✅ PASS | Domain models in `Shelfly.Common` remain framework-agnostic; App layer handles persistence via EF Core |
| **II. Vertical Slice Architecture** | ⚠️ N/A (Client) | Applies to API project; client follows MVVM pattern per Principle III |
| **III. MVVM Pattern (Client)** | ✅ PASS | Pages/ViewModels registered via `AddScopedWithShellRoute`; CommunityToolkit patterns used |
| **IV. Coding Standards** | ✅ PASS | Explicit types, collection expressions, primary constructors, nullable enabled |
| **V. Data Management** | ✅ PASS | Books use soft deletion (`DeletedAt` timestamp) in this feature; Bookmarks remain in storage but hidden when book is soft-deleted; hard delete with cascade delete deferred to trash management (out of scope) |
| **VI. API Design & Versioning** | ⚠️ N/A (Client-only) | No REST/GraphQL endpoints for this feature |
| **VII. Authentication** | ✅ PASS | Feature works without profile/account — no Keycloak dependency |

## Project Structure

### Documentation (this feature)

```text
specs/002-local-library/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.Common/
├── Book.cs              # Domain model: title, author, ISBN, publisher, publish date
└── Bookmark.cs          # Domain model: page/range, note, book reference

Shelfly.App/
├── Data/
│   ├── LocalDbContext.cs        # EF Core DbContext for SQLite
│   └── Entities/
│       ├── BookEntity.cs        # Persistence entity (maps to Common.Book)
│       └── BookmarkEntity.cs    # Persistence entity (maps to Common.Bookmark)
├── Features/
│   ├── Library/
│   │   ├── Pages/
│   │   │   ├── BookListPage.xaml          # List view with search, sort, swipe-to-delete
│   │   │   └── BookDetailPage.xaml        # Detail view with bookmarks and delete button
│   │   ├── ViewModels/
│   │   │   ├── BookListViewModel.cs       # Search, sort, soft delete logic
│   │   │   └── BookDetailViewModel.cs     # Bookmark list, book deletion
│   │   └── Services/
│   │       └── LibraryService.cs          # CRUD operations for books/bookmarks
│   ├── BookEditor/
│   │   ├── Pages/
│   │   │   └── BookEditPage.xaml          # Add/edit book form with inline validation
│   │   └── ViewModels/
│   │       └── BookEditViewModel.cs       # Field validation, save logic
│   └── BookmarkEditor/
│       ├── Pages/
│       │   └── BookmarkEditPage.xaml      # Add/edit bookmark form with inline validation
│       └── ViewModels/
│           └── BookmarkEditViewModel.cs   # Page range validation, note limits
├── Resources/
│   ├── Strings/
│   │   ├── en-US/
│   │   │   └── AppResources.resx          # English localization
│   │   └── de-DE/
│   │       └── AppResources.resx          # German localization
│   └── Fonts/                       # Icon fonts for note indicator, edit, delete
├── Services/
│   ├── LocalStorageService.cs       # DbContext initialization and migration
│   ├── LocalizationService.cs       # Language switching wrapper (MAUI native)
│   └── LibraryExportService.cs      # JSON export for library backup (FR-031)
└── AppShell.xaml                    # Shell routes: BookListPage, BookEditPage, BookDetailPage, BookmarkEditPage
```

**Structure Decision**: Feature-based organization within `Shelfly.App/Features/` following MVVM pattern. Domain models added to `Shelfly.Common` (framework-agnostic). Persistence entities in `Shelfly.App/Data/Entities/`. Localization via .NET MAUI resource files (`AppResources.resx`). No API or server components needed — feature is entirely client-side. Library export service provides JSON backup capability without requiring external dependencies.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | All principles satisfied without deviation | — |
