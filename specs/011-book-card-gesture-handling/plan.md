# Implementation Plan: Book Card Gesture Commands

**Branch**: `011-book-card-gesture-handling` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/011-book-card-gesture-handling/spec.md`

## Summary

Complete the long press gesture detection in `BookCardView` by adding command BindableProperties (`LongPressCommand`, `TapCommand`) and wiring them to the existing pointer event handlers. Enable page-level XAML to bind selection commands (long press) and navigation commands (tap on unselected cards). The control must support both gestures coexisting without interference.

## Technical Context

**Language/Version**: C# / .NET 10 (.NET MAUI)

**Primary Dependencies**: 
- Microsoft.Maui.Controls (core framework)
- CommunityToolkit.Mvvm (ObservableObject, RelayCommand patterns in view models)
- Microsoft.Maui.Essentials (HapticFeedback for gesture feedback - optional)

**Storage**: N/A (client-side state only)

**Testing**: TUnit + Shouldly (per constitution), XAML unit tests for gesture behavior

**Target Platform**: Android (always), iOS/MacCatalyst (non-Linux), Windows (conditional)

**Project Type**: Mobile/desktop application (.NET MAUI client)

**Performance Goals**: Gesture recognition within 600ms; visual feedback within 100ms; navigation within 200ms

**Constraints**: 
- .NET 10 has no `LongPressGestureRecognizer` (available in .NET 11 only)
- Must use `PointerGestureRecognizer` for long press detection via timing logic
- Both tap and pointer gestures must coexist on the same control without mutual interference
- Selection state must be communicated to view model commands

**Scale/Scope**: Single custom control (`BookCardView`) modification; consumed by BookListPage (primary) and potentially TrashListPage (future adoption)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & SoC | PASS | Control exposes commands; view models handle selection logic |
| II. Vertical Slice | N/A | Client-side feature, not API layer |
| III. MVVM Pattern | PASS | Commands follow ICommand pattern; XAML binds to view model commands |
| IV. Coding Standards | PASS | Explicit types, nullable enabled, primary constructors where applicable |
| V. Data Management | N/A | No data persistence changes |
| VI. API Design | N/A | Client-side only |
| VII. Auth & User Mgmt | N/A | No auth flow changes |
| VIII. Localization | PASS | Semantic properties will use resource keys (per constitution) |
| IX. Asset Formats | PASS | Checkmark icon remains SVG |
| X. MS Documentation | PASS | .NET 10 gesture APIs verified via Microsoft Learn MCP |
| XI. IDE Refactoring | N/A | Implementation phase concern |

## Project Structure

### Documentation (this feature)

```text
specs/011-book-card-gesture-handling/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── contracts/           # Phase 1 output (control interface contract)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Controls/
│   ├── BookCardView.xaml          # XAML layout (unchanged)
│   └── BookCardView.xaml.cs       # Add LongPressCommand, TapCommand BindableProperties + wire events
├── Features/
│   └── Library/
│       ├── Pages/
│       │   └── BookListPage.xaml  # Wire LongPressCommand→EnterSelectionMode, TapCommand→NavigateToDetailBook
│       └── ViewModels/
│           └── BookListViewModel.cs # Existing commands ready to bind (no changes needed)
└── Features/
    └── Trash/
        ├── Pages/
        │   └── TrashListPage.xaml  # Future adoption of BookCardView with gesture commands
        └── ViewModels/
            └── TrashListViewModel.cs # Existing selection commands ready to bind
```

**Structure Decision**: The feature modifies the existing `BookCardView` control in place. No new files are added to the Controls directory. Page-level XAML (BookListPage) will wire the new command properties to view model commands. View models already expose the necessary commands (`EnterSelectionMode`, `ToggleSelection`, `NavigateToDetailBookCommand`).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Pointer timing logic for long press | .NET 10 lacks LongPressGestureRecognizer (available in .NET 11) | CommunityToolkit TouchBehavior considered but requires behavior package; pointer events are built-in and more control-friendly |
