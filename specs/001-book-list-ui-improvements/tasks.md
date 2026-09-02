---

description: "Task list template for feature implementation"

---

# Tasks: Book List UI Improvements

**Input**: Design documents from `/specs/001-book-list-ui-improvements/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`). All changes are confined to `Shelfly.App/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Verify solution builds successfully with `dotnet build Shelfly.slnx`
- [X] T002 Confirm existing localization infrastructure in `Shelfly.App/Resources/Localization/AppResources.resx` and `AppResources.de.resx`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Add `BookListPageTitle` resource key to `Shelfly.App/Resources/Localization/AppResources.resx` with value "My Library"
- [X] T004 Add `BookListPageTitle` resource key to `Shelfly.App/Resources/Localization/AppResources.de.resx` with German translation
- [X] T005 Add `SortDirectionAscending` resource key to `Shelfly.App/Resources/Localization/AppResources.resx` with value "Ascending"
- [X] T006 Add `SortDirectionAscending` resource key to `Shelfly.App/Resources/Localization/AppResources.de.resx` with German translation
- [X] T007 Add `SortDirectionDescending` resource key to `Shelfly.App/Resources/Localization/AppResources.resx` with value "Descending"
- [X] T008 Add `SortDirectionDescending` resource key to `Shelfly.App/Resources/Localization/AppResources.de.resx` with German translation
- [X] T009 Create SVG icon file `sort_asc.svg` in `Shelfly.App/Resources/Images/` (upward arrow matching existing icon style)
- [X] T010 Create SVG icon file `sort_desc.svg` in `Shelfly.App/Resources/Images/` (downward arrow matching existing icon style)

**Checkpoint**: Foundation ready — all resource keys and icons available for user story implementation

---

## Phase 3: User Story 1 - Correct Page Title Display (Priority: P1) 🎯 MVP

**Goal**: Replace the incorrect page title ("Title" from sort picker label) with a proper localized page title identifying the screen as "My Library"

**Independent Test**: Open the book list page and verify the displayed title reads "My Library" or equivalent localized text in device language

### Implementation for User Story 1

- [X] T011 [US1] Update `Shelfly.App/Features/Library/Pages/BookListPage.xaml` line 13: change Title binding from `{x:Static resx:AppResources.BookListPageSortByTitle}` to `{x:Static resx:AppResources.BookListPageTitle}`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently — page displays correct localized title

---

## Phase 4: User Story 2 - Localized Sort Options in Picker (Priority: P1) 🎯 MVP

**Goal**: All sort option values displayed in the picker are properly localized to match the device language, replacing raw enum names with localized strings from AppResources

**Independent Test**: Change device language and verify all sort options display in the correct language when opening the sort picker

### Implementation for User Story 2

- [X] T012 [US1] Create `SortOptionDisplay` class in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` with properties: `Criterion` (SortCriterion) and `DisplayName` (string from AppResources based on criterion value)
- [X] T013 [US1] Update `SortOptions` property in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` line 36: change return type from `List<SortCriterion>` to `List<SortOptionDisplay>` and populate with localized display names mapping each enum value to its corresponding AppResources key
- [X] T014 [US1] Update `Shelfly.App/Features/Library/Pages/BookListPage.xaml` lines 51-52: change Picker ItemsSource binding from raw enum list to `SortOptionDisplay.DisplayName` property and update SelectedItem binding to extract the underlying Criterion value
- [X] T015 [US1] Update `SortAsync` method in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` line 80: change parameter type from `SortCriterion` to `SortOptionDisplay` and extract Criterion for service call

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — page title is correct and sort options are localized

---

## Phase 5: User Story 3 - Ascending and Descending Sort Direction (Priority: P2)

**Goal**: Users can toggle between ascending and descending order for any selected sort criterion via a single-tap toggle arrow icon adjacent to the sort picker

**Independent Test**: Select a sort option and verify the list order can be reversed by tapping the direction toggle icon; direction persists when changing criteria

### Implementation for User Story 3

- [X] T016 [US2] Add `SortDirection` enum in `Shelfly.App/Features/Library/Services/LibraryService.cs` with values: `Ascending`, `Descending`
- [X] T017 [US2] Add `SortDirection` observable property to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`: default value `SortDirection.Ascending`; maintained in-memory during session
- [X] T018 [US2] Add computed property `SortIconSource` to `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`: returns "sort_asc.svg" or "sort_desc.svg" based on current SortDirection value
- [X] T019 [US2] Update `LoadAsync` method in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` line 38: pass both SortCriterion and SortDirection to `libraryService.SortBooksAsync` call
- [X] T020 [US2] Update `SortCommand` handler in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`: preserve current SortDirection when criterion changes, then call refresh with both parameters
- [X] T021 [US2] Add `ToggleSortDirectionCommand` relay command to `Shelfly.App/Features/Library/ViewModels/ViewModels/BookListViewModel.cs`: toggles between Ascending and Descending, then calls refresh
- [X] T022 [US2] Update `RefreshBooksAsync` method in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` line 174: pass both SortCriterion and SortDirection to service call
- [X] T023 [US2] Add ImageButton for sort direction toggle in `Shelfly.App/Features/Library/Pages/BookListPage.xaml`: place adjacent to the Picker (within FlexLayout row), bind Source to `SortIconSource`, bind Command to `ToggleSortDirectionCommand`, set SemanticProperties.Description based on current direction
- [X] T024 [US2] Update `LibraryService.SortBooksAsync` method signature in `Shelfly.App/Features/Library/Services/LibraryService.cs`: add SortDirection parameter; implement conditional logic to use OrderBy or OrderByDescending based on direction value

**Checkpoint**: All three user stories should now be independently functional — page title correct, sort options localized, and sort direction toggleable

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T025 Verify XAML source generation compiles successfully with `dotnet build Shelfly.App/Shelfly.App.csproj`
- [ ] T026 Run quickstart.md validation scenarios for all three user stories
- [ ] T027 Confirm German localization renders correctly by testing with de-DE device language
- [ ] T028 Verify sort direction persists across page navigation (navigate away and back to book list)

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
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — Shares ViewModel file with US3 but independently testable
- **User Story 3 (P2)**: Depends on US2 completion (Picker binding changes in T014 must be complete before adding toggle button); can integrate with US1/US2 but is independently testable

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Foundational resource key additions (T003-T008) marked [P] can run in parallel — different lines in same file, no dependency conflicts
- Icon creation tasks (T009, T010) are independent of each other and from resource keys
- Once Foundational phase completes, US1 and US2 can start in parallel if team capacity allows
- US3 depends on US2 completion (Picker binding changes must be done first)

---

## Parallel Example: User Story 2

```bash
# Launch all foundational tasks together:
Task: "Add BookListPageTitle resource to AppResources.resx"
Task: "Add BookListPageTitle resource to AppResources.de.resx"
Task: "Create sort_asc.svg icon"
Task: "Create sort_desc.svg icon"

# Launch US2 implementation tasks (after T012 completes):
Task: "Update SortOptions property in BookListViewModel.cs"
Task: "Update Picker binding in BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 + User Story 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (page title fix)
4. Complete Phase 4: User Story 2 (localized sort options)
5. **STOP and VALIDATE**: Test both stories independently via quickstart.md
6. Deploy/demo if ready

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
   - Developer A: User Story 1 (simple binding change)
   - Developer B: User Story 2 (ViewModel + XAML changes)
3. After US2 completes, Developer C takes User Story 3

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
