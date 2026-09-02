# Implementation Plan: FAB Edit & Save UI

**Branch**: `009-fab-edit-save` | **Date**: 2026-09-02 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/009-fab-edit-save/spec.md`

## Summary

Replace toolbar edit button and inline save buttons on detail/edit pages with floating action buttons (FABs) matching the BookListPage FAB pattern. The FAB uses a Grid container with BoxView background circle and ImageButton overlay, positioned at bottom-right with AppThemeBinding for theme-aware colors. Delete remains as a toolbar item on BookDetailPage; Add Bookmark remains as an inline button.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm, CommunityToolkit.Maui (IconTintColorBehavior), XAML source generation

**Storage**: N/A — pure UI change, no data model modifications

**Testing**: TUnit + Shouldly for unit tests; visual verification on device/emulator

**Target Platform**: Android always, iOS/MacCatalyst on non-Linux, Windows conditionally

**Project Type**: Mobile app (.NET MAUI) with Shell navigation

**Performance Goals**: Standard mobile app expectations — FAB tap response under 200ms

**Constraints**: MVVM pattern enforced; XAML source generation enabled; localization via .resx required for all user-facing text

**Scale/Scope**: 3 pages affected (BookDetailPage, BookEditPage, BookmarkEditPage); no new dependencies added

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | Pass | FAB is pure UI concern in Shelfly.App; no domain model changes |
| II. Vertical Slice Architecture | Pass | Changes confined to existing feature boundaries (Library, BookEditor, BookmarkEditor) |
| III. MVVM Pattern (Client) | Pass | FAB binds to existing RelayCommands; no ViewModel changes required |
| IV. Coding Standards | Pass | XAML source generation maintained; explicit types used in code-behind |
| V. Data Management | Pass | No data model changes |
| VI. API Design & Versioning | Pass | Client-side only change |
| VII. Authentication & User Management | Pass | No auth flow changes |
| VIII. Localization & Internationalization | Pass | FAB SemanticProperties.Description uses existing localization resources |
| IX. Asset & Resource Formats | Pass | Reuses existing SVG icons (edit_icon.svg, check_icon.svg) |
| X. Microsoft Documentation Sourcing | Pass | MAUI Grid/ImageButton/BoxView patterns well-established |

## Project Structure

### Documentation (this feature)

```text
specs/009-fab-edit-save/
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
│   ├── Library/
│   │   └── Pages/
│   │       └── BookDetailPage.xaml          # Replace edit ToolbarItem with FAB
│   ├── BookEditor/
│   │   └── Pages/
│   │       └── BookEditPage.xaml            # Replace inline save button with FAB
│   └── BookmarkEditor/
│       └── Pages/
│           └── BookmarkEditPage.xaml        # Replace inline save button with FAB
├── Resources/
│   └── Styles/
│       ├── Colors.xaml                        # AppThemeBinding color resources (Primary, Secondary, White)
```

**Structure Decision**: All changes are confined to existing XAML pages within the three feature directories. No new files or directories created. The FAB pattern is copied from BookListPage and adapted for each target page. Loading state on edit pages uses an ActivityIndicator overlay inside the FAB Grid container.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | No constitution violations detected for this UI-only change | — |
