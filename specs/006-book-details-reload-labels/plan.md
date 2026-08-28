# Implementation Plan: Book Details Reload and Field Labels

**Branch**: `006-book-details-reload-labels` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/006-book-details-reload-labels/spec.md`

## Summary

Add visible field labels to BookDetailPage, BookEditPage, and BookmarkEditPage so users can clearly identify each data attribute. Fix navigation after book edit save to return to the previous page (BookDetailPage) instead of always going to BookListPage, ensuring edited changes are immediately visible on the details page.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm, XAML source generation

**Storage**: SQLite (via Shelfly.App.Data), no new storage needed

**Testing**: TUnit with Shouldly assertions

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Mobile/desktop application (.NET MAUI client)

**Performance Goals**: Details page reload within 1 second after returning from edit

**Constraints**: Labels must use localized `.resx` resource strings; no hardcoded text in XAML. Labels apply only to input fields on edit pages (helper text, validation messages, section headers out of scope).

**Scale/Scope**: 3 pages affected (BookDetailPage, BookEditPage, BookmarkEditPage); ~10 new localization keys required

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & SoC | ✅ Pass | Changes isolated to MAUI client layer; no domain model changes |
| II. Vertical Slice | ✅ Pass | Each feature (Library, BookEditor, BookmarkEditor) self-contained |
| III. MVVM Pattern | ✅ Pass | XAML changes only; ViewModels unchanged except navigation fix |
| IV. Coding Standards | ✅ Pass | Explicit types, collection expressions, nullable enabled |
| V. Data Management | ✅ Pass | No data model changes; uses existing entities |
| VI. API Design | ✅ Pass | Client-side only; no new endpoints |
| VII. Auth & User Mgmt | ✅ Pass | No auth flow changes |
| VIII. Localization | ✅ Pass | New keys added to both `.resx` files (en-US + de-DE) |
| IX. Asset Formats | ✅ Pass | SVG icons used where applicable |
| X. MS Documentation | ⚠️ Deferred | MAUI label patterns verified via existing codebase conventions |
| XI. IDE Refactoring | ✅ Pass | Rider MCP tools available for consistent updates |

**Testing Strategy**: Unit tests for ViewModel navigation logic; UI verification for label visibility

## Project Structure

### Documentation (this feature)

```text
specs/006-book-details-reload-labels/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (if applicable)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Resources/
│   └── Localization/
│       ├── AppResources.resx           # English labels (new keys added)
│       ├── AppResources.Designer.cs    # Auto-generated accessor
│       ├── AppResources.de.resx        # German translations (new keys added)
│       └── AppResources.de.Designer.cs # Auto-generated accessor
├── Features/
│   ├── Library/
│   │   └── Pages/
│   │       └── BookDetailPage.xaml     # Add visible field labels
│   ├── BookEditor/
│   │   ├── Pages/
│   │   │   └── BookEditPage.xaml      # Add explicit Label elements above fields
│   │   └── ViewModels/
│   │       └── BookEditViewModel.cs    # Fix navigation to return to previous page
│   └── BookmarkEditor/
│       └── Pages/
│           └── BookmarkEditPage.xaml  # Add explicit Label elements above fields
```

**Structure Decision**: All changes confined to the MAUI client project (`Shelfly.App`). No API or Common library changes required. The feature touches three existing vertical slices (Library, BookEditor, BookmarkEditor) within their respective XAML views and one ViewModel for navigation logic.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Navigation change in BookEditViewModel | Users expect to return to details page after editing; current `//BookListPage` navigation loses context | Using `..` (back) alone is insufficient when navigating from non-details contexts (e.g., swipe-edit from list); must track source page or use Shell modal navigation |
