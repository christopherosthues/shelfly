---

description: "Task list template for feature implementation"
---

# Tasks: About Page Licenses

**Input**: Design documents from `/specs/013-about-page-licenses/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, quickstart.md

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to the repository root (`D:\home\git\pi-services\shelfly`). All source changes are within `Shelfly.App/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and build configuration for JSON generation

- [x] T001 Add pre-build MSBuild target to `Shelfly.App/Shelfly.App.csproj` that runs `nuget-license -i Shelfly.App.csproj -o JsonPretty --include-transitive -fo licenses-release.json` conditioned on Release configuration
- [x] T002 Add `<None Include="Resources\Raw\licenses-release.json" CopyToOutputDirectory="Always" />` to `Shelfly.App/Shelfly.App.csproj` to bundle the generated JSON file with the application output

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core data model and service layer that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T003 Create `Shelfly.App/Models/DependencyPackage.cs` — record type with properties: PackageId (string), PackageVersion (string), Authors (string?), License (string?), LicenseUrl (Uri?)
- [x] T004 Implement `Shelfly.App/Services/LicenseDataService.cs` — service that reads the bundled JSON file from application output directory, parses it using System.Text.Json, and returns a List<DependencyPackage> with fallback defaults for null fields

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Third-Party Dependencies List (Priority: P1) 🎯 MVP

**Goal**: Display all third-party NuGet dependencies as a scrollable list on the About page, showing package name, version, author(s), and license type for each entry.

**Independent Test**: Navigate to the About page and verify that a CollectionView displays dependency entries with correct package names, versions, authors, and license types loaded from the bundled JSON file.

### Implementation for User Story 1

- [x] T005 [P] [US1] Add `LicenseDataService` registration to `Shelfly.App/MauiProgram.cs` DI container (AddScoped)
- [x] T006 [US1] Modify `Shelfly.App/Features/About/AboutViewModel.cs` — add `[ObservableProperty]` for `List<DependencyPackage> Dependencies`, inject `LicenseDataService` via primary constructor, and load dependencies in `LoadAsync` by calling the service with error handling
- [x] T007 [US1] Modify `Shelfly.App/Features/About/AboutPage.xaml` — replace the "View Licenses" button (`ShowLibrariesDialogCommand`) with a CollectionView bound to `Dependencies`, using an ItemTemplate that displays package name, version, authors, and license type in a Grid layout
- [x] T008 [US1] Add error state handling to `Shelfly.App/Features/About/AboutViewModel.cs` — when JSON loading fails, set Dependencies to a single entry with PackageId "Error" and Authors "Failed to load dependency data"

**Checkpoint**: At this point, User Story 1 should be fully functional — the About page displays all dependencies in a scrollable list

---

## Phase 4: User Story 2 - Navigate to License Details (Priority: P2)

**Goal**: Allow users to tap on a dependency's license type to open the corresponding license text URL in an external browser.

**Independent Test**: Tap any license type text in the dependency list and verify that the correct license URL opens in an external browser viewer.

### Implementation for User Story 2

- [x] T009 [P] [US2] Add `[RelayCommand] OpenLicenseUrlAsync(Uri? url)` to `Shelfly.App/Features/About/AboutViewModel.cs` — uses `CommunityToolkit.Maui.Core.Extensions.Launcher.OpenAsync(url)` with fallback for null URLs
- [x] T010 [US2] Modify `Shelfly.App/Features/About/AboutPage.xaml` CollectionView ItemTemplate — make the license type Label tappable by adding a TapGestureRecognizer bound to `OpenLicenseUrlCommand` passing the LicenseUrl parameter

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [x] T011 Update `Shelfly.App/Resources/Localization/AppResources.resx` — add localized strings for dependency list header, error message text, and license navigation labels
- [x] T012 [P] Verify build succeeds with `dotnet build Shelfly.App/Shelfly.App.csproj -c Release` and that `licenses-release.json` is generated and copied to output directory
- [x] T013 Run quickstart.md validation scenarios — launch app, navigate to About page, verify dependency list displays correctly, test license URL navigation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Depends on US1 UI structure (CollectionView must exist before adding tap gesture)

### Within Each User Story

- Models before services
- Services before ViewModel integration
- ViewModel before XAML binding
- Core implementation before error handling
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- T003 (DependencyPackage record) and T004 (LicenseDataService) can run in parallel within Phase 2
- Once Foundational phase completes, User Story 1 can begin immediately
- Polish tasks T011 (localization strings) and T012 (build verification) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all foundational tasks together:
Task: "Create DependencyPackage record in Shelfly.App/Models/DependencyPackage.cs"
Task: "Implement LicenseDataService in Shelfly.App/Services/LicenseDataService.cs"

# After foundation, launch US1 implementation:
Task: "Register LicenseDataService in MauiProgram.cs DI container"
Task: "Modify AboutViewModel to load and expose Dependencies list"
Task: "Modify AboutPage.xaml to display CollectionView with dependency entries"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (csproj changes for JSON generation)
2. Complete Phase 2: Foundational (DependencyPackage model + LicenseDataService)
3. Complete Phase 3: User Story 1 (About page displays dependency list)
4. **STOP and VALIDATE**: Test that the About page shows all dependencies correctly
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
   - Developer A: User Story 1 (ViewModel + XAML)
   - Developer B: Localization strings (T011)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- The nuget-license tool must be installed globally before building in Release mode
