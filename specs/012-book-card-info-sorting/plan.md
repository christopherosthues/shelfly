# Implementation Plan: Book Card Info & Sorting Enhancements

**Branch**: `012-book-card-info-sorting` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/012-book-card-info-sorting/spec.md`

## Summary

Add bookmark count display (top right of card), last modified date display (bottom right of card with CreatedAt fallback for null LastModifiedAt), and two new sorting criteria (CreatedAt, LastModifiedAt) to both library and trash list views. The implementation spans the MAUI client layer: updating BookCardView XAML, extending SortCriterion enum, modifying LibraryService/TrashService sorting logic, and adding localization resources.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI multi-target)

**Primary Dependencies**: Entity Framework Core (Npgsql), CommunityToolkit.Mvvm, FluentValidation

**Storage**: SQLite via EF Core LocalDbContext (client-side); PostgreSQL via ShelflyDbContext (server-side)

**Testing**: TUnit framework with Shouldly assertions; TestContainers for integration tests

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Mobile + API (MAUI client communicates exclusively with ASP.NET Core Minimal API)

**Performance Goals**: List views render smoothly with 60fps scrolling; sorting operations complete within 200ms for libraries up to 10,000 books

**Constraints**: Nullable reference types enabled; XAML source generation (`MauiXamlInflator=SourceGen`); Result pattern for error handling; localization via `.resx` files (en-US and de-DE minimum)

**Scale/Scope**: Client-side feature affecting BookCardView, SortCriterion enum, LibraryService, TrashService, and localization resources. No API changes required unless bookmark count needs server-side computation.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

### Pre-Design Gates

| Principle | Compliance | Notes |
|-----------|------------|-------|
| **I. SOLID & Separation of Concerns** | ✅ Pass | Common domain models remain framework-agnostic; entity models in Api/Data/Entities handle persistence mapping |
| **II. Vertical Slice Architecture** | ✅ Pass | Feature code co-located within Library and Trash feature boundaries |
| **III. MVVM Pattern (Client)** | ✅ Pass | BookCardView is a ContentView control; ViewModels inherit from ShelflyViewModelBase; sorting flows through SortableListViewModelBase |
| **IV. Coding Standards** | ✅ Pass | Explicit types, collection expressions, primary constructors, nullable enforcement |
| **V. Data Management** | ✅ Pass | Soft deletion via DeletedAt with global query filters; UUID v7 for identifiers; LastModifiedAt null-coalescing to CreatedAt follows constitution guidance |
| **VI. API Design & Versioning** | ⚠️ Deferred | Feature is client-side only; no new API endpoints required unless bookmark count needs server computation |
| **VIII. Localization** | ✅ Pass | New sort option strings added to both AppResources.resx (en-US) and AppResources.de.resx (de-DE) |

### Post-Design Re-Evaluation

| Principle | Compliance | Notes |
|-----------|------------|-------|
| **I. SOLID & Separation of Concerns** | ✅ Confirmed | Computed properties on BookEntity are client-side only; no framework attributes added to Common domain models |
| **II. Vertical Slice Architecture** | ✅ Confirmed | All changes confined to Library and Trash feature directories; shared SortCriterion enum updated centrally |
| **III. MVVM Pattern (Client)** | ✅ Confirmed | XAML bindings use compiled data types; ViewModel sort options follow existing patterns via SortableListViewModelBase |
| **IV. Coding Standards** | ✅ Confirmed | Null-coalescing operator used for LastModifiedAt fallback; collection expressions for SortOptions initialization |
| **V. Data Management** | ✅ Confirmed | EF Core COUNT query with GroupBy avoids N+1 problem; null handling respects PostgreSQL default sorting behavior |
| **VI. API Design & Versioning** | ✅ Resolved | Client-side computation eliminates need for new API endpoints; bookmark count retrieved via efficient grouped query |
| **VIII. Localization** | ✅ Confirmed | Six new resource keys required (2 sort options × 2 locales + 2 fallback strings); keys added to both .resx files simultaneously |

**Result**: All gates pass. Feature design aligns with constitution principles. No violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/012-book-card-info-sorting/
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
├── Controls/
│   └── BookCardView.xaml          # Updated: add bookmark count and last modified date labels
├── Enums/
│   └── SortCriterion.cs           # Extended: add CreatedAt, LastModifiedAt values
├── Features/
│   ├── Library/
│   │   ├── Services/
│   │   │   └── LibraryService.cs  # Updated: add sorting cases for new criteria
│   │   └── ViewModels/
│   │       └── BookListViewModel.cs # Extended: add sort options to SortOptions collection
│   └── Trash/
│       ├── Services/
│       │   └── TrashService.cs    # Updated: add sorting cases for new criteria
│       └── ViewModels/
│           └── TrashListViewModel.cs # Extended: add sort options to SortOptions collection
├── Resources/
│   └── Localization/
│       ├── AppResources.resx      # Added: new sort option strings (en-US)
│       └── AppResources.de.resx   # Added: new sort option strings (de-DE)
└── ViewModels/
    ├── SortableListViewModelBase.cs # Extended: add sort options for date criteria
    └── SortOptionDisplay.cs         # No changes required

Shelfly.Api/
├── Data/
│   └── Entities/                  # Optional: add BookmarkCount property if server-side computation needed
└── (No immediate changes unless API provides bookmark count)
```

**Structure Decision**: Client-only implementation. The feature modifies existing controls, enums, services, and resources within the MAUI client project (`Shelfly.App`). No new directories or projects required. Sorting logic follows the established pattern in LibraryService/TrashService using switch expressions on SortCriterion. BookCardView receives two new bound properties (BookmarkCount, DisplayLastModifiedAt) computed from existing entity data.

## Complexity Tracking

> **No constitution violations requiring justification**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | Client-side feature uses existing patterns | No new dependencies or architectural changes |
