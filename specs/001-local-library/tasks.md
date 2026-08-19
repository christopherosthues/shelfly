---

description: "Task list template for feature implementation"
---

# Tasks: Local Library Management

**Input**: Design documents from `/specs/002-local-library/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Optional — TUnit framework with Shouldly assertions per constitution; included where they reduce implementation risk.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Mobile/Desktop**: `Shelfly.App/`, `Shelfly.Common/` at repository root
- Paths shown below follow plan.md structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create feature directory structure per implementation plan in `Shelfly.App/Features/Library/`, `Shelfly.App/Features/BookEditor/`, `Shelfly.App/Features/BookmarkEditor/`
- [X] T002 Add NLog dependency to `Directory.Packages.props` and reference it in `Shelfly.App/Shelfly.App.csproj` for structured logging
- [X] T003 Configure NLog with local file output in `Shelfly.App/App.xaml.cs` or startup initialization

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Create Book domain model in `Shelfly.Common/Book.cs` with Guid v7 Id, Title (max 256), Author (max 256), ISBN (unique), Publisher (max 256), PublishDate?, DeletedAt?, CreatedAt, LastModifiedAt?
- [X] T005 Create Bookmark domain model in `Shelfly.Common/Bookmark.cs` with Guid v7 Id, BookId, StartPage (positive int), EndPage? (≥ StartPage), Note? (max 1000 chars), CreatedAt, LastModifiedAt?
- [X] T006 Implement Result pattern base types in `Shelfly.Common/Result.cs` for success/failure outcomes without exceptions
- [X] T007 Create BookEntity persistence entity in `Shelfly.App/Data/Entities/BookEntity.cs` with SQLite mappings, unique ISBN index, and FluentAPI configuration
- [X] T008 Create BookmarkEntity persistence entity in `Shelfly.App/Data/Entities/BookmarkEntity.cs` with foreign key to BookEntity, page validation checks, and ON DELETE CASCADE
- [X] T009 Implement LocalDbContext in `Shelfly.App/Data/LocalDbContext.cs` with DbSet<BookEntity>, DbSet<BookmarkEntity>, SQLite connection string using app local storage path, and auto-migration on startup
- [X] T010 Create EF Core interceptor for audit timestamps in `Shelfly.App/Data/AuditTimestampInterceptor.cs` that sets CreatedAt (non-null) on creation and LastModifiedAt? (nullable) on update for both BookEntity and BookmarkEntity
- [X] T011 Implement Guid v7 helper utility in `Shelfly.Common/IdGenerator.cs` using `Guid.CreateVersion7()` for entity identifier generation
- [X] T012 Register LocalDbContext as singleton service in `Shelfly.App/App.xaml.cs` with migration execution on first launch
- [X] T013 Configure Shell routes in `Shelfly.App/AppShell.xaml` for BookListPage, BookEditPage, BookDetailPage, BookmarkEditPage using `AddScopedWithShellRoute<TPage, TViewModel>` pattern
- [X] T014 Create test project `Shelfly.App.Tests` with .NET 10 target, referencing Shelfly.App and Shelfly.Common; add TUnit and Shouldly NuGet packages via `Directory.Packages.props` per constitution Testing Strategy mandate
- [X] T015 Implement scoped DI registration in `Shelfly.App/App.xaml.cs` using `AddScopedWithShellRoute<TPage, TViewModel>` for BookListViewModel, BookEditViewModel, BookDetailViewModel, BookmarkEditViewModel per constitution Principle III

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View and Browse Book List (Priority: P1) 🎯 MVP

**Goal**: Display all added books in a list showing title, author, and publisher with search, sort, empty state, and swipe-to-delete soft deletion.

**Independent Test**: Open the app, verify book list displays correctly with all required information, searching returns matching results, sorting reorders items correctly, and empty state shows when no books exist.

### Implementation for User Story 1

- [X] T016 [P] [US1] Create English localization resource file in `Shelfly.App/Resources/Strings/en-US/AppResources.resx` with strings for book list UI (empty state message, search placeholder, sort options)
- [X] T017 [P] [US1] Create German localization resource file in `Shelfly.App/Resources/Strings/de-DE/AppResources.resx` with corresponding translations for all book list UI strings
- [X] T018 [P] [US1] Implement LibraryService in `Shelfly.App/Features/Library/Services/LibraryService.cs` with methods: GetAllBooks (filtered by DeletedAt == null), SearchBooks (case-insensitive substring match on Title, Author, Publisher, ISBN), SortBooks (by title, author, publisher, publish date)
- [X] T019 [US1] Implement BookListViewModel in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` with ObservableProperty for book list, search query, sort criterion, RelayCommand for search/sort actions, and loading state management (FR-032)
- [X] T020 [US1] Add debounced search input handler in `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs` using CommunityToolkit.Maui debounce extension (≤500ms delay) to meet SC-002 performance target; ensure SearchBooks method executes only after debounce completes
- [X] T021 [US1] Create BookListPage XAML in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` with CollectionView displaying book title, author, publisher; empty state view; search bar; sort selector; SwipeView for soft deletion gesture (mobile) and platform-native drag/swipe equivalent (desktop)
- [X] T022 [US1] Implement swipe-to-delete logic in `Shelfly.App/Features/Library/Pages/BookListPage.xaml.cs` that sets DeletedAt on the swiped book via LibraryService, removes it from visible list within 200ms with visual feedback (SC-005)
- [X] T023 [US1] Implement platform-specific swipe gesture handlers in `Shelfly.App/Features/Library/Pages/BookListPage.xaml.cs`: use SwipeView for Android/iOS; implement drag/swipe equivalent via pointer/touch events on Windows/MacOS desktop platforms; unify visual feedback across all platforms per FR-022
- [X] T024 [US1] Add accessibility semantic properties to `Shelfly.App/Features/Library/Pages/BookListPage.xaml` for screen reader support on list items, search bar, and sort controls

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Add and Edit Books (Priority: P1) 🎯 MVP

**Goal**: Navigate to add/edit screen for a book with fields for title, author, publisher, ISBN, publish date; validate all inputs inline; save new or updated books.

**Independent Test**: Navigate to the add/edit screen, enter valid data for all fields, save, and verify the book appears in the list with correct details. Editing an existing book and verifying changes persist also validates this story.

### Implementation for User Story 2

- [ ] T025 [P] [US2] Add English localization strings to `Shelfly.App/Resources/Strings/en-US/AppResources.resx` for book editor UI (field labels, validation messages: empty field errors, ISBN format error, duplicate ISBN error, max length exceeded)
- [ ] T026 [P] [US2] Add German localization strings to `Shelfly.App/Resources/Strings/de-DE/AppResources.resx` for corresponding book editor translations and validation messages
- [ ] T027 [P] [US2] Implement ISBN validation utility in `Shelfly.Common/IsbnValidator.cs` supporting both ISBN-10 and ISBN-13 formats (including dashes) with checksum verification
- [ ] T028 [US2] Extend LibraryService in `Shelfly.App/Features/Library/Services/LibraryService.cs` with methods: AddBook (with ISBN uniqueness check across all books including soft-deleted), UpdateBook, SoftDeleteBook — returning Result pattern outcomes
- [ ] T029 [US2] Implement BookEditViewModel in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` with ObservableProperty for Title, Author, Publisher, ISBN, PublishDate?; inline validation errors using .NET MAUI equivalent of Android supporting text (FR-017, FR-018, FR-025, FR-026); RelayCommand for save action
- [ ] T030 [US2] Create BookEditPage XAML in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` with text fields for title (max 256), author (max 256), publisher (max 256), ISBN, DatePicker for publish date; inline validation error display on each field using supporting text pattern
- [ ] T031 [US2] Implement navigation from BookListPage to BookEditPage in `Shelfly.App/Features/Library/Pages/BookListPage.xaml.cs` with book data passing for edit mode (existing book) or add mode (new book)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Manage Bookmarks within a Book (Priority: P2)

**Goal**: Open book detail view showing all book details and associated bookmarks; add/edit/delete bookmarks with page ranges and notes; display note indicator icons; handle overlapping bookmark ordering.

**Independent Test**: Open a book detail view, verify all book details display correctly, add a new bookmark with page range and note, edit an existing bookmark, delete a bookmark, and confirm each action updates the bookmark list accordingly.

### Implementation for User Story 3

- [ ] T032 [P] [US3] Add English localization strings to `Shelfly.App/Resources/Strings/en-US/AppResources.resx` for bookmark editor UI (field labels, validation messages: page range error, note length exceeded) and detail view elements
- [ ] T033 [P] [US3] Add German localization strings to `Shelfly.App/Resources/Strings/de-DE/AppResources.resx` for corresponding bookmark editor translations and validation messages
- [ ] T034 [US3] Extend LibraryService in `Shelfly.App/Features/Library/Services/LibraryService.cs` with methods: GetBookById, AddBookmark (with page range validation), UpdateBookmark, DeleteBookmark, SoftDeleteBookWithBookmarks — returning Result pattern outcomes; bookmark list ordered by range-first then single-page for overlapping pages (FR-029)
- [ ] T035 [US3] Implement BookDetailViewModel in `Shelfly.App/Features/Library/ViewModels/BookDetailViewModel.cs` with ObservableProperty for book details, bookmark list, loading state; RelayCommand for delete book action (soft-delete entire book including associated bookmarks per FR-030)
- [ ] T036 [US3] Create BookDetailPage XAML in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` displaying full book details (title, author, ISBN, publisher, publish date), bookmark list with page/range display, note indicator icons for bookmarks with notes (FR-011), edit/delete icons per bookmark entry (FR-012), and delete button in detail view
- [ ] T037 [US3] Implement BookmarkEditViewModel in `Shelfly.App/Features/BookmarkEditor/ViewModels/BookmarkEditViewModel.cs` with ObservableProperty for StartPage, EndPage?, Note?; inline validation errors (end page ≥ start page per FR-023, note max 1000 chars per FR-021); RelayCommand for save action
- [ ] T038 [US3] Create BookmarkEditPage XAML in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` with fields for start page (positive int), end page (optional, ≥ start page), note text (max 1000 chars); inline validation error display on each field
- [ ] T039 [US3] Implement note indicator icon interaction in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml.cs` — clicking the note indicator displays associated note content to user (FR-020)
- [ ] T040 [US3] Add accessibility semantic properties to `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` and `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` for screen reader support

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T041 Implement LibraryExportService in `Shelfly.App/Services/LibraryExportService.cs` for JSON export of library data (books and bookmarks) to device storage file (FR-031)
- [ ] T042 Add export button to BookListPage toolbar/menu with file picker integration; on selection, invoke LibraryExportService to write JSON backup containing all active books and their bookmarks to device storage path (FR-031)
- [ ] T043 Add loading indicator UI components across all pages (`BookListPage.xaml`, `BookDetailPage.xaml`) showing/hiding during async data operations (FR-032)
- [ ] T044 Implement database error handling wrapper in `Shelfly.App/Services/LocalStorageService.cs` that catches SQLite errors, logs via NLog structured logging using Result pattern return types, and displays toast with error message to user
- [ ] T045 [P] Add icon fonts for note indicator, edit, delete icons in `Shelfly.App/Resources/Fonts/` with platform-specific configuration
- [ ] T046 Verify solution builds successfully: `dotnet build Shelfly.slnx`
- [ ] T047 Run quickstart.md validation scenarios to confirm all acceptance criteria pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - US1 and US2 are both P1 priority; US1 should complete first as it provides the primary entry point, but they can proceed in parallel if staffed
  - US3 depends on US1 (needs book list navigation to detail view) and US2 (needs books populated)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — Integrates with US1 via LibraryService but independently testable
- **User Story 3 (P2)**: Depends on US1 completion (navigation from list to detail) and US2 completion (books must exist); independently testable once dependencies met

### Within Each User Story

- Models before services
- Services before ViewModels
- ViewModels before Pages
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- T016 and T017 (localization files) can run in parallel within Phase 3
- T025 and T026 (editor localization files) can run in parallel within Phase 4
- T032 and T033 (bookmark localization files) can run in parallel within Phase 5
- Once Foundational phase completes, US1 and US2 can start in parallel (if team capacity allows)

---

## Parallel Example: User Story 1

```text
# Launch all localization tasks together:
Task: "Create English localization resource file in Shelfly.App/Resources/Strings/en-US/AppResources.resx"
Task: "Create German localization resource file in Shelfly.App/Resources/Strings/de-DE/AppResources.resx"

# Launch service and ViewModel creation (after localization):
Task: "Implement LibraryService in Shelfly.App/Features/Library/Services/LibraryService.cs"
Task: "Implement BookListViewModel in Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1 (View and Browse Book List)
4. Complete Phase 4: User Story 2 (Add and Edit Books)
5. **STOP and VALIDATE**: Test US1 + US2 independently using quickstart.md scenarios 1-6
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
   - Developer A: User Story 1 (Book List)
   - Developer B: User Story 2 (Book Editor)
3. After US1 and US2 complete:
   - Developer C: User Story 3 (Bookmark Management)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
