---
description: "Task list for Book Card Gesture Commands implementation"
---

# Tasks: Book Card Gesture Commands

**Input**: Design documents from `/specs/011-book-card-gesture-handling/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing infrastructure is ready for gesture command implementation

- [X] T001 Verify BookCardView.xaml.cs exists with IsSelected BindableProperty and incomplete long press logic at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T002 Verify BookListViewModel.cs exposes EnterSelectionMode, ToggleSelection, ExitSelectionMode commands at `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`
- [X] T003 Verify NavigateToDetailBookCommand exists in BookListViewModel at `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core control infrastructure that MUST be complete before any user story can function

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Add LongPressCommand BindableProperty to BookCardView at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T005 Add TapCommand BindableProperty to BookCardView at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T006 Complete long press detection logic in OnPointerReleased by invoking LongPressCommand with BindingContext parameter at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T007 Wire TapGestureRecognizer to fire TapCommand only when IsSelected is false at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T008 Implement tap-to-deselect logic: when IsSelected is true, normal tap sets IsSelected=false without firing TapCommand at `Shelfly.App/Controls/BookCardView.xaml.cs`

**Checkpoint**: Foundation ready — BookCardView exposes both command properties and gesture detection works correctly

---

## Phase 3: User Story 1 - Long Press Selects Book Card (Priority: P1) 🎯 MVP

**Goal**: Users can long press a book card to toggle selection state; the view model receives the selection command with correct book context.

**Independent Test**: Can be fully tested by long pressing any book card in the library list and verifying that the view model's selection tracking commands are invoked with the correct book context.

### Implementation for User Story 1

- [X] T009 [P] [US1] Wire LongPressCommand to EnterSelectionModeCommand in BookListPage.xaml at `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [X] T010 [P] [US1] Bind IsSelected property to view model selection state (check if book.Id is in SelectedItemIds) in BookListPage.xaml at `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [X] T011 [US1] Ensure long press toggles IsSelected on the card and fires LongPressCommand with BindingContext as parameter at `Shelfly.App/Controls/BookCardView.xaml.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional — long pressing a book card selects it visually and updates view model selection state.

---

## Phase 4: User Story 2 - Normal Tap Navigates Unselected Card (Priority: P1) 🎯 MVP

**Goal**: Users can tap an unselected book card to navigate to the detail page; selected cards deselect on tap without navigation.

**Independent Test**: Can be fully tested by tapping any unselected book card and verifying that the detail page opens with the correct book context passed as a parameter.

### Implementation for User Story 2

- [X] T012 [P] [US2] Wire TapCommand to NavigateToDetailBookCommand in BookListPage.xaml at `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [X] T013 [US2] Replace existing TapGestureRecognizer on BookCardView with TapCommand binding that fires only when unselected at `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [X] T014 [US2] Verify tap on selected card deselects without navigation by checking IsSelected state before firing TapCommand at `Shelfly.App/Controls/BookCardView.xaml.cs`

**Checkpoint**: At this point, both User Stories 1 AND 2 should work independently — long press selects/deselects, normal tap navigates unselected cards.

---

## Phase 5: User Story 3 - Gesture Coexistence (Priority: P2)

**Goal**: Long press and normal tap gestures coexist on the same book card without interference; quick taps trigger navigation immediately while sustained presses trigger selection.

**Independent Test**: Can be tested by performing a rapid tap followed by a long press on different cards, verifying each gesture produces its intended outcome independently.

### Implementation for User Story 3

- [X] T015 [P] [US3] Ensure TapGestureRecognizer and PointerGestureRecognizer coexist without mutual interference in BookCardView constructor at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T016 [US3] Verify tap gesture fires before long press threshold (under 500ms) by checking elapsed time logic at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T017 [US3] Clear pressTime on TapGestureRecognizer completion to prevent false long press detection after a quick tap at `Shelfly.App/Controls/BookCardView.xaml.cs`

**Checkpoint**: All user stories should now be independently functional — gestures coexist correctly with proper discrimination between tap and long press.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T018 [P] Add haptic feedback on long press recognition using HapticFeedback.Default.Perform(HapticFeedbackType.LongPress) at `Shelfly.App/Controls/BookCardView.xaml.cs`
- [X] T019 Verify selection state persists during CollectionView recycling by confirming IsSelected binding survives item template reuse at `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [X] T020 Run quickstart.md validation scenarios to verify all acceptance criteria pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User Story 1 and User Story 2 are both P1 and can proceed in parallel after Phase 2
  - User Story 3 depends on US1 and US2 being complete
- **Polish (Final Phase)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — Shares BookCardView control with US1 but independently testable
- **User Story 3 (P2)**: Depends on US1 and US2 completion — Validates gesture coexistence

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- Foundational tasks T004-T005 (BindableProperty additions) can run in parallel
- Once Foundational phase completes, US1 and US2 can start in parallel
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all bindings for User Story 1 together:
Task: "Wire LongPressCommand to EnterSelectionModeCommand in BookListPage.xaml"
Task: "Bind IsSelected property to view model selection state in BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2)

Both US1 and US2 are P1 priority. They share the same foundational infrastructure but deliver independent value:

1. Complete Phase 1: Setup verification
2. Complete Phase 2: Foundational (add command BindableProperties, complete gesture logic)
3. Complete Phase 3: User Story 1 (wire long press to selection commands)
4. **STOP and VALIDATE**: Test long press selection independently
5. Complete Phase 4: User Story 2 (wire tap to navigation, implement deselection)
6. **STOP and VALIDATE**: Test tap navigation independently

### Incremental Delivery

1. Foundational complete → Control infrastructure ready
2. Add US1 → Long press selects/deselects cards → Deploy/Demo
3. Add US2 → Tap navigates unselected cards → Deploy/Demo
4. Add US3 → Gesture coexistence verified → Final validation
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (long press wiring)
   - Developer B: User Story 2 (tap navigation wiring)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
