---

description: "Task list for book card selection implementation"

---

# Tasks: Book Card Selection

**Input**: Design documents from `/specs/010-book-card-selection/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to the repository root (`D:\home\git\pi-services\shelfly`). The MAUI client project is `Shelfly.App/`. Feature code lives under `Features/Library/` and `Resources/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Verify LongPressGestureRecognizer API availability in current .NET MAUI target frameworks per research.md
- [ ] T002 Confirm existing check_icon.svg asset in `Shelfly.App/Resources/Images/check_icon.svg` meets selection indicator requirements (circular, theme-adaptive)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Add localized string key `BookListPageSelectedDescription` to `Shelfly.App/Resources/Localization/AppResources.resx` (English)
- [ ] T004 [P] Add localized string key `BookListPageSelectedDescription` to `Shelfly.App/Resources/Localization/AppResources.de.resx` (German)
- [ ] T005 Add localized string key `BookListPageUnselectedDescription` to `Shelfly.App/Resources/Localization/AppResources.resx` (English)
- [ ] T006 [P] Add localized string key `BookListPageUnselectedDescription` to `Shelfly.App/Resources/Localization/AppResources.de.resx` (German)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Long Press to Select Book Card (Priority: P1) 🎯 MVP

**Goal**: Enable users to long press on a book card to toggle its selection state, displaying an animated checkmark icon inside a circular indicator at the leading edge of the card.

**Independent Test**: Long press any book card in the library list → verify checkmark appears with y-axis rotation animation → tap selected card → verify checkmark disappears with animation.

### Implementation for User Story 1

- [ ] T007 [US1] Add `IsSelectionMode` property (bool, ObservableProperty) to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`
- [ ] T008 [US1] Add `SelectedItemIds` property (`ObservableCollection<Guid>`, ObservableProperty) to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`
- [ ] T009 [US1] Add `ToggleSelectionCommand` (RelayCommand taking BookEntity parameter) to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` that toggles book ID in/out of SelectedItemIds and sets IsSelectionMode accordingly
- [ ] T010 [US1] Modify `BookCardView.xaml` to add a Grid with two columns: first column for selection indicator (fixed width), second column (`*`) for existing content
- [ ] T011 [US1] Add circular BoxView element in the first column of BookCardView.xaml as the selection indicator background, using AppThemeBinding for theme-adaptive coloring
- [ ] T012 [P] [US1] Add Image element with `Source="check_icon.svg"` inside the BoxView in `BookCardView.xaml`, bound to visibility based on selection state
- [ ] T013 [US1] Implement y-axis rotation animation logic in `BookCardView.xaml.cs` using `RotateYToAsync(360, 300, Easing.SinInOut)` for selection and `RotateYToAsync(0, 300, Easing.SinInOut)` for deselection
- [ ] T014 [US1] Add LongPressGestureRecognizer to BookCardView.xaml binding Command to ViewModel's ToggleSelectionCommand via RelativeSource AncestorType pattern

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Visual Stability During Selection (Priority: P1)

**Goal**: Ensure text content maintains consistent screen coordinates during selection/deselection transitions by reserving space for the indicator when unselected.

**Independent Test**: Observe text position before long press → long press to select → verify text position unchanged → tap to deselect → verify text position still unchanged.

### Implementation for User Story 2

- [ ] T015 [US2] Configure fixed-width first column in BookCardView.xaml Grid (e.g., `ColumnDefinitions="32, *"`) to reserve indicator space regardless of selection state
- [ ] T016 [P] [US2] Set consistent WidthRequest and HeightRequest on the circular BoxView in `BookCardView.xaml` matching the reserved column width
- [ ] T017 [US2] Verify that existing content (Title, Author, Publisher labels) occupies only the second column of the Grid in `BookCardView.xaml`, preventing layout shifts during selection state changes

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Multi-Card Selection (Priority: P2)

**Goal**: Enable users to select multiple book cards by long pressing each one individually, with independent selection state tracking per card.

**Independent Test**: Long press Book A → verify checkmark appears → long press Book B → verify both cards show checkmarks → tap Book A → verify only Book A's checkmark disappears while Book B remains selected.

### Implementation for User Story 3

- [ ] T018 [US3] Implement `OnNavigatingFrom` override in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` to clear IsSelectionMode and SelectedItemIds when navigating away from the page
- [ ] T019 [P] [US3] Add binding logic in `BookCardView.xaml` that checks whether the current book's ID exists in ViewModel's SelectedItemIds collection, using a converter or computed property to determine selection state
- [ ] T020 [US3] Verify that rapid successive long presses on different cards correctly toggle each card's independent selection state without affecting other selections

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T021 Run quickstart.md validation scenarios to verify end-to-end functionality
- [ ] T022 [P] Verify LongPressGestureRecognizer touch-only behavior on Windows platform per research findings
- [ ] T023 [P] Validate theme adaptation (light/dark mode) for selection indicator in `BookCardView.xaml`
- [ ] T024 Code cleanup and verify no trailing whitespace or formatting issues in modified files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — Depends on US1 Grid structure in BookCardView.xaml for column configuration
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) — Depends on US1 selection infrastructure; extends with multi-card tracking

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all foundational tasks together:
Task: "Add localized string key to AppResources.resx (English)"
Task: "Add localized string key to AppResources.de.resx (German)"

# Once foundation is ready, launch US1 implementation:
Task: "Add IsSelectionMode and SelectedItemIds properties to BookListViewModel.cs"
Task: "Modify BookCardView.xaml with Grid columns for selection indicator"
Task: "Implement y-axis rotation animation in BookCardView.xaml.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test long press selection independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (BookCardView.xaml modifications, animation logic)
   - Developer B: User Story 2 (Grid column configuration, visual stability verification)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
