---

description: "Task list for FAB Add Button implementation"

---

# Tasks: FAB Add Button

**Input**: Design documents from `/specs/008-fab-add-button/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: Optional — no explicit test tasks requested in feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths use the existing three-project MAUI solution structure:
- `Shelfly.App/Features/Library/Pages/` — Page XAML and code-behind files
- `Shelfly.App/Resources/Localization/` — Resource files for localization

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify existing project readiness — no new infrastructure needed

- [X] T001 Confirm `add_icon.svg` asset exists in Shelfly.App/Resources/Images/ and is accessible via IconImageSource
- [X] T002 [P] Verify localization keys `BookListPageAddNewBookDescription` exist in both AppResources.resx (en-US) and AppResources.de.resx (de-DE)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Confirm `NavigateToAddBookCommand` exists and is functional in BookListViewModel.cs
- [X] T004 Verify existing AppThemeBinding resources (`Primary`, `White`) are defined for FAB theming

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 - Add Book via Floating Action Button (Priority: P1) 🎯 MVP

**Goal**: Replace the toolbar add button with a circular floating action button anchored at the bottom-right corner of the book list page

**Independent Test**: Open the book list page, verify FAB appears at bottom-right as a circular button with add icon; tap FAB and confirm navigation to "Add New Book" page; verify toolbar no longer shows the add action

### Implementation for User Story 1

- [X] T005 [US1] Remove `ToolbarItem` for add book from ContentPage.ToolbarItems in Shelfly.App/Features/Library/Pages/BookListPage.xaml
- [X] T006 [US1] Use Grid with RowDefinitions="Auto, *" to enable bottom-right anchoring of FAB via HorizontalOptions="End" and VerticalOptions="End" in BookListPage.xaml
- [X] T007 [US1] Add BoxView element styled as circular FAB (HeightRequest="56", WidthRequest="56", CornerRadius="28") positioned at bottom-right using Grid.Row="1" with HorizontalOptions="End" and VerticalOptions="End" in BookListPage.xaml
- [X] T008 [US1] Add ImageButton inside the BoxView container with Source="add_icon.svg" bound to NavigateToAddBookCommand in BookListPage.xaml
- [X] T009 [US1] Ensure existing search bar, sort picker, and CollectionView content area fill remaining space without FAB overlap in BookListPage.xaml

**Checkpoint**: At this point, User Story 1 should be fully functional — FAB visible at bottom-right, tap navigates to Add New Book page, toolbar add button removed

---

## Phase 4: User Story 2 - FAB Accessibility and Localization (Priority: P2)

**Goal**: The floating action button includes proper accessibility descriptions and adapts to system theme (light/dark mode)

**Independent Test**: Enable screen reader on device, navigate to book list page, focus on FAB and verify it announces localized description; switch between light/dark themes and verify FAB colors adapt appropriately

### Implementation for User Story 2

- [X] T010 [P] [US2] Add SemanticProperties.Description binding using `BookListPageAddNewBookDescription` resource key to the FAB ImageButton in BookListPage.xaml
- [X] T011 [P] [US2] Apply AppThemeBinding to BoxView BackgroundColor property using existing theme resources (`Primary` for light, `White` for dark) in BookListPage.xaml

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — FAB is accessible and theme-adaptive

---

## Phase 5: Keyboard Repositioning (Edge Case Handling)

**Goal**: When the on-screen keyboard appears, the FAB repositions upward to avoid overlapping with the input area; when dismissed, it returns to original position

**Independent Test**: Open book list page, tap search bar to show keyboard — verify FAB moves upward without overlap; dismiss keyboard — verify FAB returns to bottom-right corner

### Implementation for Phase 5

- [ ] T012 [US1] Subscribe to KeyboardShowing event in BookListPage.xaml.cs and calculate keyboard height from event arguments
- [ ] T013 [US1] Implement upward repositioning logic using AbsoluteLayout.SetBounds or translation animation when keyboard appears in BookListPage.xaml.cs
- [ ] T014 [US1] Subscribe to KeyboardHidden event in BookListPage.xaml.cs and restore FAB to original bottom-right position

**Checkpoint**: FAB correctly avoids keyboard overlap and returns to normal position on dismissal

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation and edge case coverage

- [ ] T015 Verify FAB maintains bottom-right anchoring across landscape/portrait orientation changes in BookListPage.xaml
- [ ] T016 Run quickstart.md validation scenarios to confirm all acceptance criteria are met
- [ ] T017 Confirm export ToolbarItem continues to function correctly after layout restructuring in BookListPage.xaml

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational phase completion
- **User Story 2 (Phase 4)**: Depends on Phase 3 completion (FAB must exist before adding accessibility)
- **Keyboard Repositioning (Phase 5)**: Depends on Phase 3 completion (FAB must exist before repositioning logic)
- **Polish (Phase 6)**: Depends on all previous phases being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Depends on US1 completion — FAB must exist before adding accessibility properties
- **Keyboard Repositioning**: Depends on US1 completion — FAB must exist before repositioning logic

### Within Each User Story

- Layout changes before control additions
- Control additions before command bindings
- Core implementation before edge case handling
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T001, T002)
- All Foundational verification tasks marked [P] can run in parallel (T003, T004)
- Accessibility and theming tasks within US2 can run in parallel (T010, T011)

---

## Parallel Example: User Story 2

```bash
# Launch all accessibility/theming tasks for User Story 2 together:
Task: "Add SemanticProperties.Description binding using BookListPageAddNewBookDescription resource key to the FAB ImageButton in BookListPage.xaml"
Task: "Apply AppThemeBinding to BoxView BackgroundColor property using existing theme resources in BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup verification
2. Complete Phase 2: Foundational checks
3. Complete Phase 3: User Story 1 — FAB creation and navigation
4. **STOP and VALIDATE**: Test FAB visibility, positioning, and navigation independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation verified
2. Add User Story 1 → Test independently → MVP delivered (FAB functional)
3. Add User Story 2 → Test independently → Accessibility and theming complete
4. Add Keyboard Repositioning → Test independently → Edge cases handled
5. Each phase adds value without breaking previous functionality

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (FAB layout and navigation)
   - Developer B: Phase 5 (Keyboard repositioning logic in code-behind)
3. Developer C can work on User Story 2 after US1 completes

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
