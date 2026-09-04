---

description: "Task list template for feature implementation"
---

# Tasks: Book Card Info & Sorting Enhancements

**Input**: Design documents from `/specs/012-book-card-info-sorting/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`). All changes are within `Shelfly.App/` project.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Verify solution builds successfully with `dotnet build Shelfly.slnx`
- [ ] T002 Confirm existing BookCardView.xaml grid layout and binding context for reference

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Add `BookmarkCount` property to BookEntity in `Shelfly.App.Data/Entities/BookEntity.cs`
- [ ] T004 Add `DisplayLastModifiedAt` computed property to BookEntity in `Shelfly.App.Data/Entities/BookEntity.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Bookmark Count on Book Cards (Priority: P1) 🎯 MVP

**Goal**: Display the total number of bookmarks associated with each book at the top right corner of every book card in both library and trash list views.

**Independent Test**: Navigate to either the library list or trash list, verify that every book card displays a numeric bookmark count in the top right corner, and confirm the count matches the actual number of bookmarks stored for each book (including zero-bookmark books showing "0").

### Implementation for User Story 1

- [ ] T005 [P] [US1] Add efficient bookmark COUNT query method to LibraryService in `Shelfly.App/Features/Library/Services/LibraryService.cs` — use grouped EF Core query (`GroupBy` + `Count`) to retrieve counts for all books in a single round-trip
- [ ] T006 [P] [US1] Add efficient bookmark COUNT query method to TrashService in `Shelfly.App/Features/Trash/Services/TrashService.cs` — use grouped EF Core query with `IgnoreQueryFilters()` for soft-deleted books
- [ ] T007 [US1] Update `SearchSortedBooksAsync` in LibraryService (`Shelfly.App/Features/Library/Services/LibraryService.cs`) to populate `BookmarkCount` on each returned BookEntity using the COUNT query results
- [ ] T008 [US1] Update `SearchSortedTrashBooksAsync` in TrashService (`Shelfly.App/Features/Trash/Services/TrashService.cs`) to populate `BookmarkCount` on each returned BookEntity using the COUNT query results
- [ ] T009 [US1] Add bookmark count Label element to BookCardView.xaml (`Shelfly.App/Controls/BookCardView.xaml`) — bind to `BookmarkCount`, position at top right corner of card grid, use existing label styling

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently — all book cards in library and trash views display accurate bookmark counts.

---

## Phase 4: User Story 2 - View Last Modified Date on Book Cards (Priority: P1) 🎯 MVP

**Goal**: Display the last modified date for each book at the bottom right corner of every book card; when LastModifiedAt is null, display CreatedAt as a fallback.

**Independent Test**: View any list (library or trash), verify that every card displays a last modified date at the bottom right corner, and confirm the date matches the most recent modification timestamp for that book (or CreatedAt for unmodified books).

### Implementation for User Story 2

- [ ] T010 [P] [US2] Add localization resource key `BookListPageLastModifiedLabel` to AppResources.resx (`Shelfly.App/Resources/Localization/AppResources.resx`) — English: "Last Modified"
- [ ] T011 [P] [US2] Add localization resource key `BookListPageLastModifiedLabel` to AppResources.de.resx (`Shelfly.App/Resources/Localization/AppResources.de.resx`) — German translation
- [ ] T012 [US2] Add last modified date Label element to BookCardView.xaml (`Shelfly.App/Controls/BookCardView.xaml`) — bind to `DisplayLastModifiedAt` with date string format, position at bottom right corner of card grid, use reduced opacity styling consistent with Publisher label

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — all book cards display bookmark count (top right) and last modified date (bottom right).

---

## Phase 5: User Story 3 - Sort Books by Creation and Modification Dates (Priority: P2)

**Goal**: Extend library and trash list views with sorting by creation date (CreatedAt) and last modification date (LastModifiedAt), supporting both ascending and descending directions.

**Independent Test**: Open either list view, select "Created At" or "Last Modified At" from the sort picker, toggle ascending/descending direction, and verify the book order changes correctly based on the selected criterion.

### Implementation for User Story 3

- [ ] T013 [P] [US3] Add `CreatedAt` and `LastModifiedAt` values to SortCriterion enum in `Shelfly.App/Enums/SortCriterion.cs`
- [ ] T014 [P] [US3] Add localization resource key `BookListPageSortByCreatedAt` to AppResources.resx (`Shelfly.App/Resources/Localization/AppResources.resx`) — English: "Created Date"
- [ ] T015 [P] [US3] Add localization resource key `BookListPageSortByLastModifiedAt` to AppResources.resx (`Shelfly.App/Resources/Localization/AppResources.resx`) — English: "Last Modified"
- [ ] T016 [P] [US3] Add localization resource key `BookListPageSortByCreatedAt` to AppResources.de.resx (`Shelfly.App/Resources/Localization/AppResources.de.resx`) — German translation
- [ ] T017 [P] [US3] Add localization resource key `BookListPageSortByLastModifiedAt` to AppResources.de.resx (`Shelfly.App/Resources/Localization/AppResources.de.resx`) — German translation
- [ ] T018 [US3] Extend `SortOptions` collection in SortableListViewModelBase (`Shelfly.App/ViewModels/SortableListViewModelBase.cs`) — add two new SortOptionDisplay entries for CreatedAt and LastModifiedAt using the new localization keys
- [ ] T019 [US3] Add sorting cases for `CreatedAt` to switch expression in `SearchSortedBooksAsync` (LibraryService, `Shelfly.App/Features/Library/Services/LibraryService.cs`) — use `OrderBy(b => b.CreatedAt)` / `OrderByDescending(b => b.CreatedAt)`
- [ ] T020 [US3] Add sorting cases for `LastModifiedAt` to switch expression in `SearchSortedBooksAsync` (LibraryService, `Shelfly.App/Features/Library/Services/LibraryService.cs`) — use null-coalescing fallback: `OrderBy(b => b.LastModifiedAt ?? b.CreatedAt)` / `OrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt)`
- [ ] T021 [US3] Add sorting cases for `CreatedAt` to switch expression in `SearchSortedTrashBooksAsync` (TrashService, `Shelfly.App/Features/Trash/Services/TrashService.cs`) — use `OrderBy(b => b.CreatedAt)` / `OrderByDescending(b => b.CreatedAt)`
- [ ] T022 [US3] Add sorting cases for `LastModifiedAt` to switch expression in `SearchSortedTrashBooksAsync` (TrashService, `Shelfly.App/Features/Trash/Services/TrashService.cs`) — use null-coalescing fallback: `OrderBy(b => b.LastModifiedAt ?? b.CreatedAt)` / `OrderByDescending(b => b.LastModifiedAt ?? b.CreatedAt)`

**Checkpoint**: All user stories should now be independently functional — cards display bookmark count and dates, and lists can be sorted by creation/modification dates.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T023 [P] Verify BookCardView.xaml grid layout accommodates new labels without overlapping existing content
- [ ] T024 Run quickstart.md validation scenarios for all three user stories
- [ ] T025 Confirm solution builds successfully with `dotnet build Shelfly.slnx` after all changes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - US1 and US2 are both P1 priority and can proceed in parallel after Phase 2
  - US3 depends on SortCriterion enum extension (T013) which is independent of US1/US2
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories. Requires T003 (BookmarkCount property).
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories. Requires T004 (DisplayLastModifiedAt property).
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) — Independent of US1/US2, but requires SortCriterion enum extension (T013).

### Within Each User Story

- Models before services
- Services before UI bindings
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- T003 and T004 (Foundational) are independent — can run in parallel
- Once Foundational phase completes, US1 and US2 can start in parallel (both P1)
- US3 sorting localization tasks (T014–T017) are all [P] and independent
- Sorting service updates (T019–T022) depend on T013 (enum extension) but T019/T020 can run in parallel with T021/T022

---

## Parallel Example: User Story 1

```bash
# Launch all COUNT query methods together:
Task: "Add efficient bookmark COUNT query method to LibraryService"
Task: "Add efficient bookmark COUNT query method to TrashService"

# After COUNT queries complete, launch service updates and UI in parallel:
Task: "Update SearchSortedBooksAsync to populate BookmarkCount (LibraryService)"
Task: "Update SearchSortedTrashBooksAsync to populate BookmarkCount (TrashService)"
Task: "Add bookmark count Label element to BookCardView.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 2)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (Bookmark count display)
4. Complete Phase 4: User Story 2 (Last modified date display)
5. **STOP and VALIDATE**: Test both stories independently using quickstart.md scenarios 1–2
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add US1 (Bookmark count) → Test independently → Deploy/Demo
3. Add US2 (Last modified date) → Test independently → Deploy/Demo
4. Add US3 (Date sorting) → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Bookmark count — LibraryService, TrashService, BookCardView)
   - Developer B: User Story 2 (Last modified date — localization, BookCardView)
   - Developer C: User Story 3 (Date sorting — enum, services, localization)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
