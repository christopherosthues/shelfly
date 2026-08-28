---

description: "Task list for Book Details Reload and Field Labels feature implementation"

---

# Tasks: Book Details Reload and Field Labels

**Input**: Design documents from `/specs/006-book-details-reload-labels/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Mobile**: `Shelfly.App/` for MAUI client code
- Paths shown below assume the three-project solution structure from plan.md

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create Controls directory at `Shelfly.App/Controls/` if not existing
- [ ] T002 Add localization keys to `Shelfly.App/Resources/Localization/AppResources.resx` (en-US): FloatingLabelEntryTitle, FloatingLabelEntryAuthor, FloatingLabelEntryPublisher, FloatingLabelEntryISBN, FloatingLabelEntryStartPage, FloatingLabelEntryEndPage, FloatingLabelEntryNote
- [ ] T002 [P] Add localization keys to `Shelfly.App/Resources/Localization/AppResources.de.resx` (de-DE): German translations for all keys from T001

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Create FloatingLabelEntry XAML view at `Shelfly.App/Controls/FloatingLabelEntry.xaml` with Grid layout containing Label, Entry, and Border elements
- [ ] T004 Implement FloatingLabelEntry code-behind at `Shelfly.App/Controls/FloatingLabelEntry.xaml.cs` with bindable properties (LabelText, Text) and focus/text event handlers
- [ ] T005 Implement floating label animation logic in `Shelfly.App/Controls/FloatingLabelEntry.xaml.cs` using TranslateTo for vertical movement and FadeTo for opacity transitions with Easing.CubicInOut (200ms duration)
- [ ] T006 Add FloatingLabelEntry styles to `Shelfly.App/Resources/Styles/Styles.xaml` with AppThemeBinding support for light/dark mode colors

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Details Page Refreshes After Edit (Priority: P1) 🎯 MVP

**Goal**: After saving edits to a book, the user returns to the page they came from (list or detail). The detail page reloads fresh data automatically via OnNavigatedTo lifecycle. Navigation preserves the visited path — no pages added that were not visited by the user.

**Independent Test**: Navigate `detail -> edit -> save` and verify return lands on detail page with updated data. Also verify `list -> edit -> save` still returns to list page correctly.

### Implementation for User Story 1

- [ ] T007 [US1] Change post-save navigation in `Shelfly.App/Features/BookEditor/ViewModels/BookEditViewModel.cs` from absolute `//BookListPage` to relative `..` (back) so user returns to previous page regardless of entry point
- [ ] T008 [US1] Verify BookDetailPage reloads data on navigation return via existing OnNavigatedTo -> LoadAsync lifecycle in `Shelfly.App/Features/Library/ViewModels/BookDetailViewModel.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Clear Field Labels on Details Page (Priority: P1)

**Goal**: The book details page displays clear, unambiguous labels for each field so the user can immediately identify which value corresponds to which attribute.

**Independent Test**: View any book's details page and verify each data field has a visible, descriptive label that clearly identifies the attribute being shown.

### Implementation for User Story 2

- [ ] T009 [P] [US2] Add explicit Label elements above each data field in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` with bindings to localized resource strings (Title, Author, Publisher, ISBN)
- [ ] T010 [P] [US2] Verify BookDetailPage uses correct localization keys from AppResources for all field labels

**Checkpoint**: At this point, User Story 2 should be fully functional and testable independently

---

## Phase 5: User Story 3 - Clear Field Labels on Edit Pages (Priority: P1)

**Goal**: The book edit page and bookmark edit page display clear, unambiguous labels for each input field using the FloatingLabelEntry control with Material Design animation.

**Independent Test**: Open either the book edit page or bookmark edit page and verify each input field has a visible, descriptive label that clearly identifies the expected data type, with floating label animation on focus/text entry.

### Implementation for User Story 3

- [ ] T011 [P] [US3] Replace Entry/Editor controls in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` with FloatingLabelEntry controls bound to Title, Author, Publisher, and ISBN properties
- [ ] T012 [P] [US3] Replace Entry/Editor controls in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` with FloatingLabelEntry controls bound to StartPage, EndPage, and Note properties
- [ ] T013 [US3] Update BookEditPage layout in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` to accommodate FloatingLabelEntry controls (adjust Grid row definitions or switch to VerticalStackLayout)
- [ ] T014 [US3] Verify BookmarkEditPage layout compatibility with FloatingLabelEntry controls in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T015 [P] Verify solution builds successfully with `dotnet build Shelfly.slnx`
- [ ] T016 Run quickstart.md validation scenarios on Android device/emulator
- [ ] T017 [P] Verify localization works correctly in both en-US and de-DE locales
- [ ] T018 Final visual review of FloatingLabelEntry animation smoothness and consistency across all edit pages

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

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories. Changes post-save navigation from absolute `//BookListPage` to relative `..` (back), preserving the user's visited path. Detail page reloads via existing OnNavigatedTo lifecycle.
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Adds labels to details page; independent of edit pages.
- **User Story 3 (P1)**: Depends on Foundational phase completion (FloatingLabelEntry control must exist). Independent of US1 and US2 implementation.

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T002 localization keys for both languages)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 3

```bash
# Launch all XAML updates for User Story 3 together:
Task: "Replace Entry controls in BookEditPage.xaml with FloatingLabelEntry"
Task: "Replace Entry controls in BookmarkEditPage.xaml with FloatingLabelEntry"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (localization keys)
2. Complete Phase 2: Foundational (FloatingLabelEntry control)
3. Complete Phase 3: User Story 1 (relative navigation fix + reload verification)
4. **STOP and VALIDATE**: Test both paths: `list -> edit -> save` returns to list; `detail -> edit -> save` returns to detail with updated data
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
   - Developer A: User Story 1 (navigation fix)
   - Developer B: User Story 2 (details page labels)
   - Developer C: User Story 3 (edit page floating labels)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
