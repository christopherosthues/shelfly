---

description: "Task list for FAB Edit & Save UI implementation"

---

# Tasks: FAB Edit & Save UI

**Input**: Design documents from `/specs/009-fab-edit-save/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Optional — no test tasks generated unless explicitly requested in feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root `D:\home\git\pi-services\shelfly\`

<!--
  ============================================================================
  Tasks generated based on:
  - User stories from spec.md (with priorities P1, P2, P3)
  - Feature requirements from plan.md
  - FAB pattern details from research.md
  - Pure UI change confirmed by data-model.md
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing resources and prepare for implementation

- [x] T001 Verify edit_icon.svg exists in `Shelfly.App/Resources/Images/edit_icon.svg`
- [x] T002 Verify check_icon.svg or suitable save icon exists; if absent, add check_icon.svg to `Shelfly.App/Resources/Images/`
- [x] T003 Confirm AppThemeBinding color resources (Primary, Secondary, White) exist in `Shelfly.App/Resources/Styles/Colors.xaml`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Localization keys for FAB SemanticProperties.Description — MUST complete before any XAML changes

- [x] T004 Add localization key `BookDetailPageEditFabDescription` to `Shelfly.App/Resources/Localization/AppResources.resx` and `AppResources.de.resx`
- [x] T005 Add localization key `BookEditPageSaveFabDescription` to `Shelfly.App/Resources/Localization/AppResources.resx` and `AppResources.de.resx`
- [x] T006 Add localization key `BookmarkEditPageSaveFabDescription` to `Shelfly.App/Resources/Localization/AppResources.resx` and `AppResources.de.resx`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Replace Toolbar Edit Button with FAB on Book Detail (Priority: P1) 🎯 MVP

**Goal**: Replace the edit ToolbarItem on BookDetailPage with a floating action button matching the BookListPage FAB pattern. The delete toolbar item remains unchanged.

**Independent Test**: Navigate to any book detail page; verify FAB at bottom-right navigates to edit page, and toolbar shows only delete icon (no edit icon).

### Implementation for User Story 1

- [x] T007 [US1] Remove edit ToolbarItem from `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml`
- [x] T008 [US1] Add FAB Grid container to outer layout in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` using BookListPage pattern (BoxView 64x64 CornerRadius 32 + ImageButton 48x48 with edit_icon.svg, bound to EditBookCommand)
- [x] T009 [US1] Verify `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` builds successfully and FAB is visible at bottom-right

**Checkpoint**: At this point, User Story 1 should be fully functional — book detail page shows FAB for edit, toolbar retains delete only.

---

## Phase 4: User Story 2 - Replace Inline Save Button with FAB on Book Edit (Priority: P1)

**Goal**: Replace the inline save button at the bottom of the book edit form with a floating action button matching the established FAB pattern. Include loading state visualization (ActivityIndicator + reduced opacity).

**Independent Test**: Navigate to book edit page; verify FAB at bottom-right saves changes, and no inline save button appears in form body. During save, FAB shows spinner with reduced opacity.

### Implementation for User Story 2

- [x] T010 [US2] Remove inline save button Grid from `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml`
- [x] T011 [US2] Restructure outer layout in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` to use Grid with RowDefinitions="*, Auto" to accommodate FAB container at bottom-right
- [x] T012 [US2] Add FAB Grid container to `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` using BookListPage pattern (BoxView 64x64 CornerRadius 32 + ImageButton 48x48 with check_icon.svg, bound to SaveCommand)
- [x] T013 [US2] Add ActivityIndicator overlay inside FAB container in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` bound to IsSaving property (visible during save operations)
- [x] T014 [US2] Apply reduced opacity to ImageButton in `Shelfly.App/Features/BookEditor/Pages/BookEditPage.xaml` using InvertedBoolConverter on IsSaving

**Checkpoint**: At this point, User Story 2 should be fully functional — book edit page shows FAB for save with loading state.

---

## Phase 5: User Story 3 - Apply FAB Pattern to Bookmark Edit Page (Priority: P2)

**Goal**: Replace the inline save button on bookmark edit page with a floating action button matching the established FAB pattern, including loading state visualization.

**Independent Test**: Navigate to bookmark edit page; verify FAB at bottom-right saves changes, and no inline save button appears in form body. During save, FAB shows spinner with reduced opacity.

### Implementation for User Story 3

- [x] T015 [US3] Remove inline save button Grid from `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml`
- [x] T016 [US3] Restructure outer layout in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` to use Grid with RowDefinitions="*, Auto" to accommodate FAB container at bottom-right
- [x] T017 [US3] Add FAB Grid container to `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` using BookListPage pattern (BoxView 64x64 CornerRadius 32 + ImageButton 48x48 with check_icon.svg, bound to SaveCommand)
- [x] T018 [US3] Add ActivityIndicator overlay inside FAB container in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` bound to IsSaving property (visible during save operations)
- [x] T019 [US3] Apply reduced opacity to ImageButton in `Shelfly.App/Features/BookmarkEditor/Pages/BookmarkEditPage.xaml` using InvertedBoolConverter on IsSaving

**Checkpoint**: At this point, User Stories 1 AND 2 AND 3 should all be functional — all three pages use FAB pattern.

---

## Phase 6: User Story 4 - Keep Delete Button in Toolbar on Book Detail (Priority: P3)

**Goal**: Verify the delete toolbar item remains visible and functional after edit migration to FAB. This is a verification task confirming no regression.

**Independent Test**: Navigate to book detail page; verify delete icon still appears in toolbar area and soft-delete command works correctly.

### Implementation for User Story 4

- [x] T020 [US4] Verify delete ToolbarItem remains present in `Shelfly.App/Features/Library/Pages/BookDetailPage.xaml` after edit migration
- [x] T021 [US4] Test soft-delete flow: tap delete toolbar item → confirmation alert appears → book removed and navigation to list page occurs

**Checkpoint**: All user stories should now be independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and consistency checks across all affected pages

- [x] T022 [P] Run `dotnet build Shelfly.slnx` to verify solution builds without errors
- [x] T023 [P] Verify FAB colors match BookListPage by comparing AppThemeBinding usage in all three modified XAML files
- [x] T024 Run quickstart.md validation scenarios for all 5 test cases
- [x] T025 [P] Verify localization keys exist in both `AppResources.resx` (en-US) and `AppResources.de.resx` (de-DE)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) — Independent of US1, same priority
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) — Independent of US1/US2
- **User Story 4 (P3)**: Can start after US1 completes — Verification task confirming no regression

### Within Each User Story

- Localization keys before XAML changes
- Layout restructuring before FAB container addition
- FAB container before loading state overlay

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational localization tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, US1 and US2 (both P1) can start in parallel
- US3 (P2) can begin independently once Foundational is complete
- Polish tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1 & 2 (Both P1)

```bash
# Launch both P1 stories together after Phase 2 completes:

# Developer A - US1 (Book Detail):
Task: "Remove edit ToolbarItem from BookDetailPage.xaml"
Task: "Add FAB Grid container to BookDetailPage.xaml"

# Developer B - US2 (Book Edit):
Task: "Restructure outer layout in BookEditPage.xaml"
Task: "Add FAB Grid container to BookEditPage.xaml"
Task: "Add ActivityIndicator overlay for loading state"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (verify resources)
2. Complete Phase 2: Foundational (localization keys)
3. Complete Phase 3: User Story 1 (Book Detail FAB)
4. **STOP and VALIDATE**: Test book detail page independently — FAB navigates to edit, delete remains in toolbar
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → MVP delivered (book detail uses FAB)
3. Add User Story 2 → Test independently → Book edit page uses FAB with loading state
4. Add User Story 3 → Test independently → Bookmark edit page uses FAB with loading state
5. Add User Story 4 → Verify no regression on delete toolbar item
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Book Detail FAB)
   - Developer B: User Story 2 (Book Edit FAB)
3. After P1 stories complete:
   - Developer C: User Story 3 (Bookmark Edit FAB)
4. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- FAB pattern must exactly match BookListPage: Grid + BoxView (64x64, CornerRadius 32) + ImageButton (48x48) with AppThemeBinding colors
- Save FABs include ActivityIndicator overlay and reduced opacity during IsSaving state
