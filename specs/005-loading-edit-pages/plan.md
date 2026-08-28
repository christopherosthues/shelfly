# Implementation Plan: Loading Indicators for Edit Pages

**Branch**: `005-loading-edit-pages` | **Date**: 2026-08-28 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/005-loading-edit-pages/spec.md` + user refinement: "Use separate loading properties for button and full screen loading. The loading indicator should be displayed in the button, use converters from the community toolkit for data conversion e.g. bool negation"

## Summary

Add loading indicators to both edit pages (BookEditPage and BookmarkEditPage) in the MAUI client. The feature requires two distinct visual patterns: a full-screen overlay for data loading operations (matching BookListPage style) and an inline button-level indicator with button disabling during save operations. Each pattern uses a separate observable property (`IsLoading` for full-screen, `IsSaving` for button-level) to enable independent control. CommunityToolkit.Maui's `InvertedBoolConverter` replaces computed negation properties in XAML bindings.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: 
- CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`)
- CommunityToolkit.Maui (`InvertedBoolConverter` for XAML boolean negation)
- XAML source generation (`MauiXamlInflator=SourceGen`)

**Storage**: N/A (client-side UI feature, no data model changes)

**Testing**: TUnit + Shouldly (per constitution)

**Target Platform**: Android always; iOS/MacCatalyst on non-Linux; Windows conditionally

**Project Type**: Mobile app (.NET MAUI cross-platform client)

**Performance Goals**: Overlay visible within 100ms of load initiation; button transforms within 50ms of tap; minimum display duration of 2 seconds for save indicator

**Constraints**: Must follow MVVM pattern (Constitution III); must use XAML source generation; all user-facing text localized via `.resx` files (Constitution VIII)

**Scale/Scope**: Two edit pages modified (BookEditPage, BookmarkEditPage); two ViewModels updated (BookEditViewModel, BookmarkEditViewModel)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | ✅ Pass | Separate loading properties (`IsLoading`, `IsSaving`) isolate concerns; XAML converters handle boolean negation without ViewModel computed properties |
| II. Vertical Slice Architecture | ✅ Pass | Changes co-located within BookEditor and BookmarkEditor feature directories |
| III. MVVM Pattern (Client) | ✅ Pass | Pages inherit from ShelflyContentPageBase; ViewModels use ObservableProperty/RelayCommand; loading flows through LoadAsync; separate properties enable independent lifecycle management |
| IV. Coding Standards | ✅ Pass | Explicit types, collection expressions, nullable reference types enforced; CommunityToolkit converters used per established patterns |
| V. Data Management | ✅ N/A | No entity model changes |
| VI. API Design & Versioning | ✅ N/A | Client-side only feature |
| VII. Authentication & User Management | ✅ N/A | No auth flow changes |
| VIII. Localization & Internationalization | ⚠️ Watch | Any new user-facing text must be added to AppResources.resx (en-US + de-DE) |
| IX. Asset & Resource Formats | ✅ Pass | SVG icons used where applicable |
| X. Microsoft Documentation Sourcing | ✅ N/A | MAUI patterns well-established for .NET 10 |
| XI. IDE-Assisted Refactoring | ✅ N/A | No structural refactoring required |

## Project Structure

### Documentation (this feature)

```text
specs/005-loading-edit-pages/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (N/A - no data model changes)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/
│   ├── BookEditor/
│   │   ├── Pages/
│   │   │   └── BookEditPage.xaml          # Modified: add overlay + button indicator with InvertedBoolConverter
│   │   └── ViewModels/
│   │       └── BookEditViewModel.cs        # Modified: add IsSaving property, wire to save command
│   └── BookmarkEditor/
│       ├── Pages/
│       │   └── BookmarkEditPage.xaml      # Modified: add overlay + button indicator with InvertedBoolConverter
│       └── ViewModels/
│           └── BookmarkEditViewModel.cs    # Modified: add IsSaving property, wire to save command
├── Resources/
│   └── Styles/
│       └── Styles.xaml                     # Potentially modified: new loading overlay style
```

**Structure Decision**: Changes are confined to the two existing feature directories (BookEditor and BookmarkEditor). No new files or directories created. 

**Separate Loading Properties**: Each ViewModel will have two distinct observable properties:
- `IsLoading` — Controls full-screen overlay visibility during data load operations (`LoadAsync`)
- `IsSaving` — Controls button-level indicator visibility during save operations (`SaveAsync`)

This separation allows independent lifecycle management and prevents state conflicts between loading and saving operations.

**XAML Binding Strategy**: Use CommunityToolkit.Maui's `InvertedBoolConverter` for boolean negation in XAML bindings:
- Full-screen overlay: `IsVisible="{Binding IsLoading}"` (direct binding)
- Form content visibility: `IsVisible="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}"` (inverted binding)
- Button visibility/enabled: `IsVisible="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`, `IsEnabled="{Binding ISaving, Converter={StaticResource InvertedBoolConverter}}"`
- ActivityIndicator in button: `IsRunning="{Binding IsSaving}"`, `IsVisible="{Binding IsSaving}"`

This eliminates the need for computed negation properties (`IsNotLoading`) in ViewModels, keeping ViewModel logic focused on state management while XAML handles display logic.

## Complexity Tracking

> **No violations — all changes follow established patterns from Constitution III (MVVM Pattern)**

**Design Decisions**:
1. **Separate loading properties** (`IsLoading` vs `IsSaving`) provide clear separation of concerns and enable independent lifecycle management for data load vs save operations
2. **CommunityToolkit.Maui converters** replace computed ViewModel properties, keeping display logic in XAML where it belongs (per MVVM principles)
3. **InvertedBoolConverter usage** follows existing patterns in the codebase (BookDetailPage already uses `IsNotNullConverter` from the same toolkit namespace)
