---

description: "Task list for context-aware empty state messages"

---

# Tasks: Search Empty Message

**Input**: Design documents from `/specs/007-search-empty-message/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root `D:\home\git\pi-services\shelfly`. The MAUI client lives under `Shelfly.App/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

No setup tasks required — the project is already initialized with all necessary infrastructure.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T001 Add localization key `BookListPageSearchEmptyMessage` to `Shelfly.App/Resources/Localization/AppResources.resx` with value "No books matched your search"
- [X] T001 [P] Add localization key `BookListPageSearchEmptyMessage` to `Shelfly.App/Resources/Localization/AppResources.de.resx` with German translation

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Context-Aware Empty Message (Priority: P1) 🎯 MVP

**Goal**: When a search query is active and yields zero results, display a context-aware empty state message indicating no books matched the search.

**Independent Test**: Enter a non-matching search term in BookListPage → verify the EmptyView displays "No books matched your search" (or localized equivalent) instead of the generic "No books yet. Tap + to add your first book."

### Implementation for User Story 1

- [X] T002 [US1] Add computed property `EmptyStateMessage` to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` that returns localized message based on search state: when `SearchQuery` is not null/whitespace AND `Books.Count == 0`, return `AppResources.BookListPageSearchEmptyMessage`; otherwise return `AppResources.BookListPageEmptyStateMessage`
- [X] T003 [US1] Update EmptyView binding in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` to bind `Label.Text` to `{Binding EmptyStateMessage}` instead of the static resource reference

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Standard Empty State Preservation (Priority: P2)

**Goal**: When NO search query is active AND the library contains zero books, display the standard generic "no books" message.

**Independent Test**: Open BookListPage with an empty library and no active search → verify the EmptyView displays "No books yet. Tap + to add your first book." (or localized equivalent)

### Implementation for User Story 2

- [X] T004 [US2] Verify in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` that `EmptyStateMessage` returns `AppResources.BookListPageEmptyStateMessage` when `SearchQuery` is null or whitespace and `Books.Count == 0`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Dynamic Message Switching (Priority: P3)

**Goal**: The empty state message updates dynamically as the user types or clears the search bar without requiring a page reload.

**Independent Test**: With an active non-matching search showing "no books matched" → clear the search bar → verify message switches to standard "no books yet" (if library is empty) or books appear (if library has content)

### Implementation for User Story 3

- [X] T005 [US3] Ensure `EmptyStateMessage` property in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` raises PropertyChanged when either `SearchQuery` or `Books` changes, by implementing the partial method pattern or using `[ObservableProperty]` with dependency tracking

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T006 Run quickstart.md validation scenarios to verify all empty state conditions display correctly
- [X] T007 [P] Verify XAML source generation compiles without errors by running `dotnet build Shelfly.App/Shelfly.App.csproj`

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
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Validates US1 logic for non-search state
- **User Story 3 (P3)**: Depends on US1 completion — Verifies reactive binding behavior

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Different localization files can be edited in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all models for User Story 1 together:
Task: "Add computed property EmptyStateMessage to BookListViewModel.cs"
Task: "Update EmptyView binding in BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 2: Foundational (localization keys added)
2. Complete Phase 3: User Story 1 (computed property + XAML binding update)
3. **STOP and VALIDATE**: Test that search empty state displays context-aware message
4. Deploy/demo if ready

### Incremental Delivery

1. Complete Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Verify standard empty state preserved
4. Add User Story 3 → Verify dynamic switching works
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (computed property + XAML binding)
   - Developer B: User Story 2 (validation of standard state)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
