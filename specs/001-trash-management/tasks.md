---

description: "Task list for Trash Management feature implementation"

---

# Tasks: Trash Management

**Input**: Design documents from `/specs/001-trash-management/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`). The project uses a three-project structure:
- `Shelfly.App/` — MAUI client (primary work location)
- `Shelfly.App.Data/` — Client-side data access layer
- `Shelfly.Common/` — Shared domain models

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and structural changes to support Shell flyout navigation

- [ ] T001 Convert AppShell.xaml from single-content to FlyoutItem-based structure with Library and Trash entries in `Shelfly.App/AppShell.xaml`
- [ ] T002 Add route constants for trash pages (TrashListPage, TrashBookDetailPage, TrashBookmarkDetailPage) in `Shelfly.App/Routes.cs`
- [ ] T003 Create feature directory structure: `Shelfly.App/Features/Trash/Pages/`, `Shelfly.App/Features/Trash/ViewModels/`, `Shelfly.App/Features/Trash/Services/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 Implement TrashService with GetAllTrashBooksAsync() using `.IgnoreQueryFilters()` to query soft-deleted books in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T005 Register TrashListPage and associated ViewModels via AddScopedWithShellRoute in `Shelfly.App/MauiProgram.cs`
- [ ] T006 Add localization keys for trash UI text (trash title, empty state, restore/delete labels) to both en-US and de-DE resource files in `Shelfly.App/Resources/AppResources.resx`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Navigate between Library and Trash (Priority: P1) 🎯 MVP

**Goal**: The user can switch between viewing their active library and viewing soft-deleted items in trash using a flyout menu selection.

**Independent Test**: Can be fully tested by selecting "Trash" from the flyout menu and verifying that only soft-deleted items appear, while selecting "Library" shows active items.

### Implementation for User Story 1

- [ ] T007 [P] [US1] Create TrashListViewModel with ObservableCollection of soft-deleted books implementing LoadAsync to query via TrashService in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T008 [P] [US1] Create TrashBookCardView control displaying book title, author, and deletion date as a read-only card view in `Shelfly.App/Controls/TrashBookCardView.xaml`
- [ ] T009 [US1] Implement TrashListPage with CollectionView bound to trash items, empty state handling, and toolbar header in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T010 [US1] Wire up Shell flyout navigation: verify Library item navigates to BookListPage and Trash item navigates to TrashListPage in `Shelfly.App/AppShell.xaml`

**Checkpoint**: At this point, User Story 1 should be fully functional — users can navigate between Library and Trash views via flyout menu.

---

## Phase 4: User Story 2 - View Read-Only Item Details in Trash (Priority: P1) 🎯 MVP

**Goal**: The user can tap a soft-deleted item in the trash to view its read-only details. Books show full book information; bookmarks display only the note content.

**Independent Test**: Can be fully tested by tapping an item in trash and verifying that all fields are displayed but not editable, with bookmarks showing only the note field.

### Implementation for User Story 2

- [ ] T011 [P] [US2] Create TrashBookDetailViewModel inheriting ShelflyViewModelBase implementing IQueryAttributable to receive bookId parameter in `Shelfly.App/Features/Trash/ViewModels/TrashBookDetailViewModel.cs`
- [ ] T012 [P] [US2] Create TrashBookmarkDetailViewModel displaying only the Note field with LoadAsync fetching bookmark data by ID in `Shelfly.App/Features/Trash/ViewModels/TrashBookmarkDetailViewModel.cs`
- [ ] T013 [US2] Implement TrashBookDetailPage showing all book fields as read-only labels (no edit controls) in `Shelfly.App/Features/Trash/Pages/TrashBookDetailPage.xaml`
- [ ] T014 [US2] Implement TrashBookmarkDetailPage displaying only the note content with a back navigation button in `Shelfly.App/Features/Trash/Pages/TrashBookmarkDetailPage.xaml`
- [ ] T015 [US2] Add tap gesture handler on TrashListPage items to navigate to appropriate detail page (book or bookmark) using Shell.Current.GoToAsync with query parameters in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`

**Checkpoint**: At this point, Users Stories 1 AND 2 should both work independently — users can navigate to trash and view read-only details for any item.

---

## Phase 5: User Story 3 - Restore Individual Items from Trash (Priority: P2)

**Goal**: The user can restore a soft-deleted item back to the active library by swiping right-to-left on the item or selecting it and choosing "Restore."

**Independent Test**: Can be fully tested by swiping an item leftward (or using selection + restore command) and verifying the item reappears in the library with its `DeletedAt` timestamp cleared.

### Implementation for User Story 3

- [ ] T016 [P] [US3] Add RestoreBookAsync method to TrashService that sets DeletedAt = NULL on target book in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T017 [US3] Implement SwipeView with LeftItems (Restore action) and RightItems (Delete placeholder) in TrashListPage item template, binding to RestoreCommand in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T018 [US3] Add RestoreCommand to TrashListViewModel that calls TrashService.RestoreBookAsync, removes item from ObservableCollection, and refreshes the list in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T019 [P] [US3] Add SVG icons for restore action (restore_icon.svg) to `Shelfly.App/Resources/Raw/`

**Checkpoint**: At this point, users can restore individual items from trash via swipe gesture or toolbar command.

---

## Phase 6: User Story 4 - Permanently Delete Items from Trash (Priority: P2)

**Goal**: The user can permanently delete a soft-deleted item by swiping left-to-right on the item or selecting it and choosing "Delete."

**Independent Test**: Can be fully tested by swiping an item rightward (or using selection + delete command) and verifying the item is physically removed from storage.

### Implementation for User Story 4

- [ ] T020 [P] [US4] Add HardDeleteBookAsync method to TrashService that removes book via DbContext.Bookmarks.Remove() with cascade deletion in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T021 [US4] Complete SwipeView RightItems (Delete action) binding to HardDeleteCommand in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T022 [US4] Add HardDeleteCommand to TrashListViewModel that calls TrashService.HardDeleteBookAsync, removes item from ObservableCollection, and refreshes the list in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T023 [P] [US4] Add SVG icons for delete action (delete_icon.svg) to `Shelfly.App/Resources/Raw/`

**Checkpoint**: At this point, users can permanently delete individual items from trash via swipe gesture or toolbar command.

---

## Phase 7: User Story 5 - Multi-Item Selection in Trash (Priority: P3)

**Goal**: The user can long-press items to enter selection mode, then select one or multiple soft-deleted items for batch restore or batch delete operations.

**Independent Test**: Can be fully tested by long- pressing an item, selecting additional items, and executing a batch action to verify all selected items are affected.

### Implementation for User Story 5

- [ ] T024 [P] [US5] Add selection state properties to TrashListViewModel: IsSelectionMode (bool), SelectedItemIds (ObservableCollection<Guid>), ToggleSelectionCommand in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T025 [US5] Implement long-press gesture handler on trash items that sets IsSelectionMode = true and adds item to selection in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T026 [P] [US5] Add value converter to visually highlight selected items (checkmark overlay or background color change) in `Shelfly.App/Converters/SelectionStateConverter.cs`
- [ ] T027 [US5] Implement toolbar conditional visibility: show "Restore Selected", "Delete Selected", and "Done" buttons when IsSelectionMode is true in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T028 [US5] Add RestoreSelectedCommand to TrashListViewModel iterating over SelectedItemIds calling RestoreBookAsync for each in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T029 [US5] Add DeleteSelectedCommand to TrashListViewModel iterating over SelectedItemIds calling HardDeleteBookAsync for each in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`

**Checkpoint**: At this point, users can select multiple items via long-press and execute batch restore/delete operations.

---

## Phase 8: User Story 6 - Bulk Operations on All Trash Items (Priority: P3)

**Goal**: The user can restore all soft-deleted items or delete all soft-deleted entries via a single toolbar action.

**Independent Test**: Can be fully tested by tapping "Restore All" or "Delete All" in the toolbar and verifying all trash items are affected accordingly.

### Implementation for User Story 6

- [ ] T030 [P] [US6] Add RestoreAllAsync method to TrashService that sets DeletedAt = NULL on all soft-deleted books in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T031 [P] [US6] Add DeleteAllAsync method to TrashService that removes all soft-deleted books with cascade bookmark deletion in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T032 [US6] Implement RestoreAllCommand and DeleteAllCommand in TrashListViewModel calling respective TrashService methods and refreshing the list in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T033 [US6] Add "Restore All" and "Delete All" toolbar buttons with confirmation dialogs to TrashListPage in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`

**Checkpoint**: At this point, users can execute bulk restore/delete operations on all trash items.

---

## Phase 9: Search and Sort (Cross-Cutting Enhancement)

**Goal**: The trash list supports searching and sorting with the same fields and criteria as the book list.

### Implementation for Search and Sort

- [ ] T034 [P] Add search query method to TrashService using EF.Functions.Like on Title, Author, Publisher, ISBN filtered by DeletedAt != null in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T035 [P] Add sort query method to TrashService supporting SortCriterion and SortDirection enums (reused from Library feature) in `Shelfly.App/Features/Trash/Services/TrashService.cs`
- [ ] T036 [US1] Implement SearchBar binding with EventToCommandBehavior on TextChanged triggering search command in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T037 [US1] Add SortPicker and sort direction toggle ImageButton to TrashListPage toolbar matching BookListPage patterns in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T038 [US1] Wire search/sort properties and commands into TrashListViewModel (SearchQuery, SelectedSortOptionIndex, SortCommand) reusing Library feature logic in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T039 [P] Add trash-specific styles (SwipeView styling, selection highlight colors) to `Shelfly.App/Resources/Styles/Styles.xaml`
- [ ] T040 Implement OnNavigatingFrom override in TrashListViewModel to cancel active commands and clear selection state per Constitution Principle III in `Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs`
- [ ] T041 Add empty state handling for search results (no matching items) with localized message in `Shelfly.App/Features/Trash/Pages/TrashListPage.xaml`
- [ ] T042 Run quickstart.md validation scenarios 1-10 to verify end-to-end functionality

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - US1 and US2 are P1 priority — implement sequentially or in parallel if staffed
  - US3 and US4 are P2 priority — can start after US1/US2 complete
  - US5 and US6 are P3 priority — can start after US3/US4 complete
- **Search/Sort (Phase 9)**: Depends on US1 completion (needs TrashListPage structure)
- **Polish (Phase 10)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P1)**: Can start after US1 completes — Needs TrashListPage item tap navigation
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) — Depends on TrashService from Phase 2
- **User Story 4 (P2)**: Can start after US3 completes — Reuses swipe infrastructure from US3
- **User Story 5 (P3)**: Can start after US3/US4 complete — Needs RestoreCommand and HardDeleteCommand
- **User Story 6 (P3)**: Can start after US3/US4 complete — Needs TrashService bulk methods

### Within Each User Story

- Models before services
- Services before UI pages
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- T011 and T012 (detail ViewModels) can run in parallel within US2
- T016 and T019 (restore service + icons) can run in parallel within US3
- T020 and T023 (delete service + icons) can run in parallel within US4
- T024 and T026 (selection state + converter) can run in parallel within US5
- T030 and T031 (bulk methods) can run in parallel within US6
- T034 and T035 (search/sort service methods) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all models for User Story 1 together:
Task: "Create TrashListViewModel with ObservableCollection of soft-deleted books in Shelfly.App/Features/Trash/ViewModels/TrashListViewModel.cs"
Task: "Create TrashBookCardView control displaying book info as read-only card in Shelfly.App/Controls/TrashBookCardView.xaml"

# Sequential (depends on above):
Task: "Implement TrashListPage with CollectionView bound to trash items in Shelfly.App/Features/Trash/Pages/TrashListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (Shell flyout structure, routes, directory creation)
2. Complete Phase 2: Foundational (TrashService registration, DI wiring, localization keys)
3. Complete Phase 3: User Story 1 (Navigate between Library and Trash)
4. **STOP and VALIDATE**: Test flyout navigation independently per quickstart Scenario 1
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Navigation) → Test independently → MVP achieved
3. Add US2 (Read-Only Details) → Test independently → Demo
4. Add US3+US4 (Restore + Delete) → Test independently → Demo
5. Add US5+US6 (Selection + Bulk Ops) → Test independently → Demo
6. Add Search/Sort → Test independently → Feature complete
7. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (Navigation) + US2 (Details)
   - Developer B: US3 (Restore) + US4 (Delete)
   - Developer C: US5 (Selection) + US6 (Bulk Ops) + Search/Sort
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Constitution Principle III requires OnNavigatingFrom override to cancel active commands
- Constitution Principle VIII requires all new UI text in both en-US and de-DE resource files
