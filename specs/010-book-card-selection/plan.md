# Implementation Plan: Book Card Selection

**Branch**: `010-book-card-selection` | **Date**: 2026-09-04 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/010-book-card-selection/spec.md`

## Summary

Add long press selection capability to `BookCardView`. Long pressing a book card toggles its selection state, displaying an animated checkmark icon inside a circular indicator at the leading edge of the card. The space for the indicator is reserved when unselected (visually empty) to prevent text movement during selection/deselection transitions. Selection state is managed client-side within the list session; no batch action UI is included in this scope.

## Technical Context

**Language/Version**: C# 14 / .NET 10 (MAUI)

**Primary Dependencies**: CommunityToolkit.Mvvm, MAUI LongPressGestureRecognizer, MAUI VisualElement animation APIs

**Storage**: Client-side selection state only (`ObservableCollection<Guid>` in ViewModel); no server synchronization required

**Testing**: TUnit + Shouldly for unit tests; visual validation via MAUI client on target platforms

**Target Platform**: Android always, iOS/MacCatalyst on non-Linux, Windows conditionally

**Project Type**: Mobile app (.NET MAUI client)

**Performance Goals**: Selection confirmed within 100ms of gesture completion; animation completes within 300ms with ease-in-out easing

**Constraints**: Long press must coexist with existing tap (navigate to details) and swipe gestures; text position variance below 2 pixels during state transitions

**Scale/Scope**: Single feature addition to existing book list UI; modifies BookCardView control and BookListViewModel; no new entities or API endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Compliance |
|-----------|------------|
| **I. SOLID & Separation of Concerns** | ✅ MVVM pattern maintained; ViewModel manages selection state, BookCardView handles gesture recognition and visual presentation |
| **II. Vertical Slice Architecture** | ✅ Feature code co-located in `Features/Library/` alongside existing book list; control modifications in `Controls/` |
| **III. MVVM Pattern (Client)** | ✅ Commands bound via `RelayCommand`; selection state uses `ObservableProperty`; follows TrashListViewModel pattern for multi-select |
| **IV. Coding Standards** | ✅ Explicit types, collection expressions, primary constructors, nullable enabled; animation timing documented |
| **VIII. Localization & Internationalization** | ✅ Semantic accessibility properties localized via `.resx` (English + German) |
| **IX. Asset & Resource Formats** | ✅ Checkmark icon uses existing SVG asset (`check_icon.svg`) with theme-adaptive coloring |
| **X. Microsoft Documentation Sourcing** | ✅ LongPressGestureRecognizer and animation APIs grounded in official MAUI documentation |

## Project Structure

### Documentation (this feature)

```text
specs/010-book-card-selection/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Controls/
│   ├── BookCardView.xaml          # Modified: add selection indicator with reserved space, long press gesture
│   └── BookCardView.xaml.cs       # Modified: add selection state binding and animation logic
├── Features/
│   └── Library/
│       ├── Pages/
│       │   └── BookListPage.xaml  # Unchanged: existing CollectionView with BookCardView items
│       └── ViewModels/
│           └── BookListViewModel.cs    # Modified: add selection state management (IsSelectionMode, SelectedItemIds)
├── Resources/
│   ├── Localization/
│   │   ├── AppResources.resx         # Added: semantic accessibility strings for selection states
│   │   └── AppResources.de.resx      # Added: German translations for new keys
│   └── Images/
│       └── check_icon.svg            # Existing: SVG icon for selected state indicator
```

**Structure Decision**: The feature modifies the existing `BookCardView` control and adds selection state management to `BookListViewModel`, following the proven pattern from `TrashListViewModel`. No new files are created outside these boundaries except localization strings. This follows the Vertical Slice Architecture principle by keeping all changes co-located with the existing book list feature while reusing shared controls.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | Feature is a straightforward addition to existing gesture infrastructure; follows established selection patterns | — |
