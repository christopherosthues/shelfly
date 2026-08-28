---

description: "Task list for loading indicators on edit pages"

---

# Tasks: Loading Indicators for Edit Pages

**Input**: Design documents from `/specs/005-loading-edit-pages/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`). MAUI client project is `Shelfly.App/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Verify solution builds successfully with `dotnet build Shelfly.slnx`
- [x] T002 Confirm existing ActivityIndicator global style in `Shelfly.App/Resources/Styles/Styles.xaml` is suitable for reuse
- [x] T003 Verify CommunityToolkit.Maui package reference exists in `Directory.Packages.props` and includes converter support

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T004 Add `IsSaving` observable property to `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` (`[ObservableProperty] public partial bool IsSaving { get; set; } = false;`)
- [x] T005 Add `IsSaving` observable property to `Shelfly.App/Features/BookmarkEditor/ViewModels/BookmarkEditViewModel.cs` (`[ObservableProperty] public partial bool IsSaving { get; set; } = false;`)
- [x] T006 Update `SaveAsync` method in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` to toggle `IsSaving` (set `true` before try block, reset to `false` in finally block)
- [x] T007 Update `SaveAsync` method in `Shelfly.App/Features/BookmarkEditor/ViewModels/BookmarkEditViewModel.cs` to toggle `IsSaving` (set `true` before try block, reset to `false` in finally block)
- [x] T008 Remove computed property `public bool IsNotLoading => !IsLoading;` from `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` (replaced by InvertedBoolConverter in XAML)
- [x] T009 Remove computed property `public bool IsNotLoading => !IsLoading;` from `Shelfly.App/Features/BookmarkEditor/ViewModels/BookmarkEditViewModel.cs` (replaced by InvertedBoolConverter in XAML)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Button Feedback During Save (Priority: P1) 🎯 MVP

**Goal**: Save button transforms into an inline loading indicator and becomes disabled during save operations on both edit pages.

**Independent Test**: Can be fully tested by opening an edit page, tapping save, and verifying the button transforms into a loading indicator and becomes disabled before navigation completes. Delivers reduced perceived wait time and prevents duplicate submissions.

### Implementation for User Story 1

- [x] T010 [P] [US1] Add toolkit namespace declaration (`xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"`) and ResourceDictionary with `InvertedBoolConverter` to `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml`
- [x] T011 [P] [US1] Add toolkit namespace declaration (`xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"`) and ResourceDictionary with `InvertedBoolConverter` to `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml`
- [x] T012 [P] [US1] Replace save button in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` with a Grid containing both the original Button (`IsEnabled="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`, `IsVisible="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`) and an ActivityIndicator (`IsRunning="{Binding IsSaving}"`, `IsVisible="{Binding IsSaving}"`) positioned at the same location
- [x] T013 [P] [US1] Replace save button in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` with a Grid containing both the original Button (`IsEnabled="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`, `IsVisible="{Binding IsSaving, Converter={StaticResource InvertedBoolConverter}}"`) and an ActivityIndicator (`IsRunning="{Binding IsSaving}"`, `IsVisible="{Binding IsSaving}"`) positioned at the same location
- [x] T014 [US1] Add minimum 2-second display duration logic to save operations in both ViewModels (use `await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken)` after successful save before navigation)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently — tapping save on either edit page shows button-level loading feedback with minimum duration.

---

## Phase 4: User Story 2 - Full-Screen Feedback During Load (Priority: P2)

**Goal**: A full-screen overlay loading indicator appears while data is being fetched from the API during navigation to edit an existing book or bookmark. The overlay blocks all user input until form fields are populated.

**Independent Test**: Can be fully tested by navigating to an existing book or bookmark edit page and verifying a full-screen overlay appears during data fetch and disappears when fields are populated.

### Implementation for User Story 2

- [x] T015 [P] [US2] Restructure `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` layout to use a Grid with two rows (`RowDefinitions="Auto, *"`) where Row 1 contains both the existing ScrollView form content AND an ActivityIndicator overlay (`Grid.Row="1"`, `IsRunning="{Binding IsLoading}"`, `IsVisible="{Binding IsLoading}"`, centered horizontally and vertically)
- [x] T016 [P] [US2] Restructure `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` layout to use a Grid with two rows (`RowDefinitions="Auto, *"`) where Row 1 contains both the existing ScrollView form content AND an ActivityIndicator overlay (`Grid.Row="1"`, `IsRunning="{Binding IsLoading}"`, `IsVisible="{Binding IsLoading}"`, centered horizontally and vertically)
- [x] T017 [US2] Add semi-transparent background to loading overlay in both edit pages (use a BoxView with `BackgroundColor="#80000000"` or theme-appropriate dimming color, `Opacity="0.5"`, `IsVisible="{Binding IsLoading}"` positioned behind the ActivityIndicator)
- [x] T018 [US2] Update ScrollView visibility in both edit pages to use InvertedBoolConverter (`IsVisible="{Binding IsLoading, Converter={StaticResource InvertedBoolConverter}}"`) instead of computed `IsNotLoading` property

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — full-screen overlay blocks input during load, and button-level indicator works during save.

---

## Phase 5: User Story 3 - Consistent Loading Experience Across Edit Pages (Priority: P3)

**Goal**: Both edit pages display loading indicators using consistent visual patterns — full-screen overlay for data loading matching the book list page style, and button-level inline indicator for save operations.

**Independent Test**: Can be tested by comparing the full-screen overlay on edit pages against the book list page pattern, and verifying button-level indicators behave consistently between both edit pages.

### Implementation for User Story 3

- [x] T019 [P] [US3] Verify ActivityIndicator styling in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` matches the visual style used in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` (same color theming via global Styles.xaml, same sizing approach)
- [x] T020 [P] [US3] Verify ActivityIndicator styling in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` matches the visual style used in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` (same color theming via global Styles.xaml, same sizing approach)
- [x] T021 [US3] Verify button-level loading indicator behavior is identical between BookEditPage and BookmarkEditPage (same Grid structure, same binding patterns using InvertedBoolConverter, same minimum duration logic)

**Checkpoint**: All user stories should now be independently functional — both edit pages have consistent loading indicators matching established patterns.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [x] T022 [P] Run quickstart validation scenarios from `specs/005-loading-edit-pages/quickstart.md` to verify all acceptance criteria
- [x] T023 Verify cancellation token handling: confirm loading indicator and async operation cancel cleanly when navigating away mid-operation (validate in both ViewModels' `OnNavigatingFrom` behavior)
- [x] T024 [P] Verify error feedback after failed save: button returns to normal state (re-enabled) and error message is displayed (test on both edit pages)
- [x] T025 Code cleanup: ensure no duplicate ActivityIndicator patterns exist, verify all `IsLoading` and `IsSaving` bindings are correct across modified XAML files

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Depends on US1 and US2 completion - verification of consistency between both patterns

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, US1 and US2 can start in parallel (if team capacity allows)
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch both XAML modifications together:
Task: "Add InvertedBoolConverter to BookEditPage.xaml"
Task: "Add InvertedBoolConverter to BookmarkEditPage.xaml"
Task: "Replace save button in BookEditPage.xaml with Grid containing Button + ActivityIndicator"
Task: "Replace save button in BookmarkEditPage.xaml with Grid containing Button + ActivityIndicator"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test button-level loading feedback on both edit pages independently
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
   - Developer A: User Story 1 (button feedback)
   - Developer B: User Story 2 (full-screen overlay)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

## Design Decisions (from updated plan)

1. **Separate loading properties**: `IsLoading` controls full-screen overlay during data load; `IsSaving` controls button-level indicator during save operations
2. **InvertedBoolConverter**: Replaces computed ViewModel properties (`IsNotLoading`) for boolean negation in XAML bindings
3. **Toolkit namespace**: Both edit pages must declare `xmlns:toolkit="http://schemas.microsoft.com/dotnet/2022/maui/toolkit"` and register the converter in ResourceDictionary
