---
description: "Task list for modernizing book list UI"
---

# Tasks: Modernize Book List UI

**Input**: Design documents from `/specs/002-modernize-book-list-ui/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to repository root (`D:\home\git\pi-services\shelfly`). MAUI project files live under `Shelfly.App/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create Controls directory at `Shelfly.App/Controls/` for reusable ContentView-based components
- [X] T002 Add Border style with rounded corners (radius 5 units) to `Shelfly.App/Resources/Styles/Styles.xaml` (StrokeShape="RoundRectangle", CornerRadius=5)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T003 Create BookCardView component XAML at `Shelfly.App/Controls/BookCardView.xaml` inheriting from ContentView with Border containing Grid (RowDefinitions="Auto, Auto, Auto") for title, author, and publisher labels
- [X] T004 Create BookCardView code-behind at `Shelfly.App/Controls/BookCardView.xaml.cs` with BindingContext exposed as a property of type BookEntity

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Responsive Layout Adaptation (Priority: P1) 🎯 MVP

**Goal**: Move sort picker from bottom to top alongside search bar with responsive side-by-side/stacked layout based on screen width.

**Independent Test**: Open book list page on wide screen — search bar and sort picker appear side by side at top; rotate to narrow portrait — sort picker moves beneath search bar in stacked layout.

### Implementation for User Story 1

- [X] T005 [P] [US1] Restructure BookListPage root Grid at `Shelfly.App/Features/Library/Pages/BookListPage.xaml` to use FlexLayout or responsive Grid for top controls area (search bar + sort picker side-by-side on wide screens, stacked on narrow screens)
- [X] T006 [US1] Move Picker element from bottom row to new top controls area in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` with appropriate margin and spacing relative to search bar
- [X] T007 [US1] Adjust CollectionView row placement in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` to occupy remaining space below top controls area

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Card-Wrapped Book Items (Priority: P1) 🎯 MVP

**Goal**: Wrap each book list item in a reusable card component with rounded corners, shadow effects, and 16-unit horizontal margins.

**Independent Test**: View populated book list — each entry displays within distinct card with rounded corners, consistent padding, 16-unit left/right margin; tap navigates to detail; swipe-to-delete functions identically.

### Implementation for User Story 2

- [X] T008 [P] [US2] Configure BookCardView styling in `Shelfly.App/Controls/BookCardView.xaml` with Border shadow (Radius=15, Opacity=0.5), rounded corners via StrokeShape="RoundRectangle" and CornerRadius=5, Margin="16, 0" for horizontal spacing, and MinimumHeightRequest="48"
- [X] T009 [US2] Update CollectionView ItemTemplate in `Shelfly.App/Features/Library/Pages/BookListPage.xaml` to use BookCardView inside SwipeView (SwipeView wraps BookCardView) maintaining existing TapGestureRecognizer and binding context
- [X] T010 [US2] Add card-specific styles to `Shelfly.App/Resources/Styles/Styles.xaml` for consistent visual separation between adjacent cards (spacing, elevation effects)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T011 [P] Verify accessibility semantic properties preserved on all card elements in `Shelfly.App/Controls/BookCardView.xaml`
- [ ] T012 [US2] Ensure ActivityIndicator is centered on screen (HorizontalOptions="Center", VerticalOptions="Center") in `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [ ] T013 [P] Verify empty state message remains visible and centered when no books exist in `Shelfly.App/Features/Library/Pages/BookListPage.xaml`
- [ ] T014 Run quickstart.md validation scenarios to confirm all acceptance criteria pass

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P1)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Depends on BookCardView from Phase 2; may integrate with US1 layout changes but independently testable

### Within Each User Story

- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, both user stories can start in parallel (if team capacity allows)
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tasks for User Story 1 together:
Task: "Restructure BookListPage root Grid at Shelfly.App/Features/Library/Pages/BookListPage.xaml"
Task: "Move Picker element from bottom row to new top controls area in Shelfly.App/Features/Library/Pages/BookListPage.xaml"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test responsive layout independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (responsive layout)
   - Developer B: User Story 2 (card wrapping)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
