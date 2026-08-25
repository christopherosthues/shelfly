---

description: "Task list template for feature implementation"

---

# Tasks: Edit Book from Details Page

**Input**: Design documents from `/specs/004-edit-book-from-details/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

All paths are relative to the repository root (`D:\home\git\pi-services\shelfly`).

---

<!--
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.

  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/

  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment

  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Verify existing project builds successfully (`dotnet build Shelfly.slnx`)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T002 Refactor BookEditViewModel to inherit from ShelflyViewModelBase in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs`
- [X] T003 Implement IQueryAttributable interface on BookEditViewModel in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` to receive BookId from navigation query parameters
- [X] T004 Override LoadAsync method in BookEditViewModel in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` to fetch existing book data via LibraryService.GetBookByIdAsync() when BookId is not Guid.Empty, and populate form fields (Title, Author, ISBN, Publisher, PublishDate)
- [X] T005 Override OnNavigatingFrom in BookEditViewModel in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` to cancel active operations per constitution principle III

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Edit Book from Details Page (Priority: P1) 🎯 MVP

**Goal**: Add an edit button to the book details page that navigates to the existing BookEditPage, passing the current book's identifier. The edit form loads existing book data and allows modifications with Result-pattern error handling.

**Independent Test**: Can be fully tested by navigating to any book's details page, tapping the edit button, verifying the edit form appears with correct pre-populated data loaded via LoadAsync, making a change, saving, and confirming the updated data persists and displays correctly on return to the details page.

### Implementation for User Story 1

- [X] T006 [P] Add localization keys for edit button text (`BookDetailPageEditBookButtonText`) in `Shelfly.App/Resources/Localization/AppResources.resx` (en-US)
- [X] T007 [P] Add localization keys for edit button description (`BookDetailPageEditBookDescription`) in `Shelfly.App/Resources/Localization/AppResources.de.resx` (de-DE)
- [X] T008 Add ToolbarItem to BookDetailPage.xaml in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` inside existing `<ContentPage.ToolbarItems>` block, binding Command to EditBookCommand and using localization keys for Text and SemanticProperties.Description
- [X] T009 Implement EditBookAsync command method in BookDetailViewModel in `Shelfly.App/Features/Library/ViewModels/BookDetailViewModel.cs` using `[RelayCommand]` attribute that navigates to Routes.BookEditPage with BookId query parameter (passing current book's Id)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. The user can:
1. Navigate to a book's details page
2. Tap the edit button in the toolbar
3. See the edit form pre-populated with existing book data (loaded via LoadAsync)
4. Modify fields and save changes using Result pattern error handling
5. Return to details page seeing updated data

---

## Phase 4: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T010 Run quickstart.md validation scenarios for edit button navigation, book loading, save success/failure, and localization
- [X] T011 Verify all constitution principles are satisfied (especially III. MVVM Pattern for BookEditViewModel lifecycle management)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3)**: All depend on Foundational phase completion
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- T006 and T007 (localization keys for en-US and de-DE) can run in parallel
- Once Foundational phase completes, User Story 1 can begin immediately

---

## Parallel Example: User Story 1

```bash
# Launch all localization tasks together:
Task: "Add edit button text key in AppResources.resx (en-US)"
Task: "Add edit button description key in AppResources.de.resx (de-DE)"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
   - Refactor BookEditViewModel to inherit from ShelflyViewModelBase
   - Implement IQueryAttributable for query parameter reception
   - Add LoadAsync for book data loading
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently using quickstart.md scenarios
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Each story adds value without breaking previous stories

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
