---

description: "Task list for swipe-to-edit book implementation"

---

# Tasks: Swipe-to-Edit Book

**Input**: Design documents from `/specs/003-swipe-edit-book/`

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

- [X] T001 Verify SwipeView.LeftItems API availability in current MAUI target frameworks per research.md
- [X] T002 Add edit icon SVG asset to `Shelfly.App/Resources/Raw/edit_icon.svg`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add localized string key `BookListPageSwipeToEditCommand` to `Shelfly.App/Resources/Strings/en-US/AppResources.resx`
- [X] T004 [P] Add localized string key `BookListPageSwipeToEditCommand` to `Shelfly.App/Resources/Strings/de-DE/AppResources.resx`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Swipe Right to Reveal Edit Action (Priority: P1) 🎯 MVP

**Goal**: Enable users to swipe right on a book item to reveal an edit action element, then tap it to navigate to the existing BookEditPage with the book's ID.

**Independent Test**: Swipe right on any book item in the library list → verify action element appears with icon and localized text → tap element → confirm navigation to BookEditPage pre-filled with that book's data.

### Implementation for User Story 1

- [X] T005 [US1] Add `NavigateToEditBookCommand` (RelayCommand taking Guid parameter) to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`
- [X] T006 [US1] Implement navigation handler in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` that calls `Shell.Current.GoToAsync(Routes.BookEditPage, new Dictionary<string, object> { [nameof(BookEditViewModel.BookId)] = bookId })`
- [X] T007 [US1] Add `SwipeView.LeftItems` with a single `SwipeItem` to the DataTemplate in `Shelfly.App/Features/Library/Pages/BookListPage.xaml`, binding Command to `NavigateToEditBookCommand` and CommandParameter to `{Binding Id}`
- [X] T008 [US1] Configure SwipeItem properties: set `IconImageSource="edit_icon.svg"`, bind `Text="{x:Static resx:AppResources.BookListPageSwipeToEditCommand}"`, and set an appropriate `BackgroundColor` matching existing UI design language

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Visual Feedback During Swipe (Priority: P2)

**Goal**: Ensure smooth visual feedback during the swipe gesture with proper animations and localized text display.

**Independent Test**: Swipe right on a book item → observe smooth animation revealing action element → verify icon and localized text are correctly displayed for active locale.

### Implementation for User Story 2

- [X] T009 [P] [US2] Verify SwipeView global style in `Shelfly.App/Resources/Styles/Styles.xaml` provides appropriate margin/padding for the new LeftItems action element
- [X] T010 [P] [US2] Validate that localized text renders correctly by testing both English (`en-US`) and German (`de-DE`) locales on the swipe action element in `Shelfly.App/Features/Library/Pages/BookListPage.xaml`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Swipe Cancellation (Priority: P3)

**Goal**: Ensure graceful cancellation when user releases before activation threshold or taps outside the action element.

**Independent Test**: Start right swipe and release before threshold → verify item animates back to resting state with no navigation. Tap outside action element area → verify element hides without side effects.

### Implementation for User Story 3

- [X] T011 [US3] Verify SwipeView default `SwipeBehaviorOnInvoked` behavior in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` ensures the list item resets after navigation or cancellation
- [X] T012 [US3] Confirm that back-navigation from BookEditPage resets the swiped book item to its resting state (verify via `OnNavigatingFrom` in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`)

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T013 Run quickstart.md validation scenarios to verify end-to-end functionality
- [X] T014 [P] Verify SwipeView touch-only behavior on Windows platform per research findings
- [X] T015 Code cleanup and verify no trailing whitespace or formatting issues in modified files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Depends on US1 XAML structure being in place for style validation
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) — Depends on US1 swipe infrastructure; verifies cancellation behavior

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
Task: "Add localized string key to en-US/AppResources.resx"
Task: "Add localized string key to de-DE/AppResources.resx"

# Once foundation is ready, launch US1 implementation:
Task: "Add NavigateToEditBookCommand to BookListViewModel.cs"
Task: "Add SwipeView.LeftItems to BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test swipe-to-edit independently
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
   - Developer A: User Story 1 (XAML + ViewModel changes)
   - Developer B: User Story 2 (Style validation + localization testing)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

---

## Phase 7: Convergence

- [X] T016 Move edit SwipeItem from `SwipeView.LeftItems` to `SwipeView.RightItems` in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` per FR-001 (contradicts) — **Resolved**: Rightward swipe drags left-to-right, revealing content on the left side (`LeftItems`)
