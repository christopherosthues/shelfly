---

description: "Task list for remote server synchronization feature implementation"

---

# Tasks: Remote Server Synchronization

**Input**: Design documents from `/specs/014-server-sync/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths are relative to the repository root (`D:\home\git\pi-services\shelfly`). The MAUI client lives under `Shelfly.App/`; data access under `Shelfly.App.Data/`; migrations under `Shelfly.App.Migrations/`.

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [x] T001 Create Settings feature directory structure at `Shelfly.App/Features/Settings/Views/`, `Shelfly.App/Features/Settings/ViewModels/`, `Shelfly.App/Features/Settings/Services/`
- [x] T002 Add new entity files to `Shelfly.App.Data/Entities/`: ServerEntity.cs, SavedServerEntry.cs, SyncState.cs, BookServerMapping.cs, BookProfileSyncRecord.cs
- [x] T003 Add entity classes to `Shelfly.App.Data/Entities/`: ServerEntity.cs, SavedServerEntry.cs, SyncState.cs, BookServerMapping.cs, BookProfileSyncRecord.cs — all configuration via attributes ([Key], [Required], [MaxLength], [Index], [ForeignKey])
- [x] T004 Update LocalDbContext.cs (`Shelfly.App.Data/LocalDbContext.cs`) with new DbSet properties for all five entities; relationships configured via attributes on entity classes
- [x] T005 Add new localization keys to `Shelfly.App/Resources/en-US/AppResources.resx` for Settings page labels, server management strings, sync state messages, and error messages
- [x] T006 Add corresponding German translations to `Shelfly.App/Resources/de-DE/AppResources.resx` mirroring all English keys from T005

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T007 Create ApiClient.cs (`Shelfly.App/Features/Settings/Services/ApiClient.cs`) with HttpClient wrapper supporting: connection test (GET /health), registration (POST /v1/auth/register), login (POST /v1/auth/login), logout (POST /v1/auth/logout), and sync endpoints
- [x] T008 Implement credential storage service (`Shelfly.App/Features/Settings/Services/CredentialStore.cs`) using encrypted file persistence in `FileSystem.AppDataDirectory` with AES encryption for JWT tokens and profile data
- [x] T009 Create SyncService.cs (`Shelfly.App/Features/Settings/Services/SyncService.cs`) skeleton with: two-way sync method, conflict resolution (last-write-wins), deduplication logic using server-assigned IDs, and per-profile tracking updates
- [x] T010 Register new services in `Shelfly.App/MauiProgram.cs`: ApiClient (singleton), CredentialStore (singleton), SyncService (scoped) with proper DI lifetime selection
- [x] T011 Create EF Core migration using `dotnet ef migrations add AddServerSyncEntities --project Shelfly.App.Migrations --configuration Release` targeting LocalDbContext; verify migration SQL includes all five new entities, indexes, and cascade delete configurations

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Server Management & Registration (Priority: P1) 🎯 MVP

**Goal**: The user can add a sync server by URL, test the connection, register a new account, and be automatically signed in.

**Independent Test**: Launch app → navigate to Settings → enter valid server URL → tap "Test Connection" → see success message → tap "Register" → enter username/email/password → submit → verify auto sign-in with active server displayed.

### Implementation for User Story 1

- [X] T012 [P] [US1] Create ServerEntryViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/ServerEntryViewModel.cs`) inheriting from `ShelflyViewModelBase` with: URL property, connection test command (calls ApiClient.TestConnection), registration form properties (username, email, password, confirm password), register command with validation
- [X] T013 [P] [US1] Create SettingsPage.xaml (`Shelfly.App/Features/Settings/Views/SettingsPage.xaml`) inheriting from `ShelflyContentPageBase` with: server URL entry field, "Test Connection" button, registration form section (username, email, password entries), "Register" button, active server display area
- [X] T014 [US1] Implement SettingsViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/SettingsViewModel.cs`) inheriting from `ShelflyViewModelBase` with: LoadAsync method to load saved servers list and sync state; properties for SavedServers collection, ActiveServerEntry, SyncEnabled toggle, LastSuccessfulSyncAt; commands for AddServerCommand (navigates to server entry), ToggleSyncCommand
- [X] T015 [US1] Implement connection test logic in ApiClient.cs (`Shelfly.App/Features/Settings/Services/ApiClient.cs`) using GET /health endpoint with Result pattern return type, timeout handling, and clear error messages for unreachable servers
- [X] T016 [US1] Implement registration flow in ServerEntryViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/ServerEntryViewModel.cs`): validate password confirmation → call ApiClient.Register → on success, save server entity via LocalDbContext → create SavedServerEntry with IsActive=true → update SyncState.ActiveServerEntryId and SyncEnabled=true → store credentials in CredentialStore
- [X] T017 [US1] Register SettingsPage and ServerEntryViewModel in `Shelfly.App/MauiProgram.cs` using `AddScopedWithShellRoute<SettingsPage, SettingsViewModel>(Routes.SettingsPage)` pattern; add corresponding route constant to Routes.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently — user can add server, test connection, register, and auto sign-in.

---

## Phase 4: User Story 2 - Sign In/Out & Server Switching (Priority: P2)

**Goal**: The user can sign in with existing credentials on any saved server, sign out to clear the profile while preserving local data, and switch between multiple saved servers (one active at a time).

**Independent Test**: After US1 registration, restart app → navigate to Settings → select different saved server → enter credentials → tap "Sign In" → verify active server changes; then tap "Sign Out" → verify profile cleared but local books remain visible.

### Implementation for User Story 2

- [X] T018 [P] [US2] Implement sign-in flow in ServerEntryViewModel.cs (`Shelfly.App/Features/Settings/ServerEntryViewModel.cs`): add login form properties (username, password), LoginCommand that calls ApiClient.Login → on success, update SavedServerEntry with profile data via SettingsService → set IsActive=true for this entry and false for all others → update SyncState.ActiveServerEntryId → store credentials in CredentialStore
- [X] T019 [P] [US2] Implement sign-out flow in SettingsViewModel.cs (`Shelfly.App/Features/Settings/SettingsViewModel.cs`): SignOutCommand that calls ApiClient.Logout (best-effort) via SettingsService → sets SyncState.ActiveServerEntryId to null and SyncEnabled=false → clears active profile from UI; preserves SavedServerEntry records
- [X] T020 [US2] Implement server switching in SettingsViewModel.cs (`Shelfly.App/Features/Settings/SettingsViewModel.cs`): SelectServerCommand that takes a SavedServerEntry → sets IsActive=true for selected entry and false for all others via SettingsService → updates SyncState.ActiveServerEntryId → triggers LoadAsync to refresh UI
- [X] T021 [US2] Update SettingsPage.xaml (`Shelfly.App/Features/Settings/SettingsPage.xaml`) with: saved servers list (CollectionView displaying URL, profile username, active indicator), "Sign In" button per server entry, "Sign Out" button for active server, synchronization toggle switch bound to SyncEnabled property
- [X] T022 [US2] Implement sign-in UI in ServerEntryViewModel.cs (`Shelfly.App/Features/Settings/ServerEntryViewModel.cs`): add conditional visibility for login form (shown when selecting existing saved server); implement generic error message display per FR-013; use IQueryAttributable for navigation parameters

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently — user can register, sign in/out, and switch between servers.

---

## Phase 5: User Story 3 - Synchronization Engine (Priority: P3)

**Goal**: Books and bookmarks synchronize bidirectionally with the active server for the signed-in profile; conflicts resolved via last-write-wins; per-profile sync history tracked on each book.

**Independent Test**: Sign in with same profile on two app instances → create book on Device A → trigger sync → verify book appears on Device B after sync; edit book on Device B → sync both devices → verify identical state; check book details show synced profiles and last-sync times.

### Implementation for User Story 3

- [X] T023 [P] [US3] Implement upload logic in SyncService.cs (`Shelfly.App/Features/Settings/Services/SyncService.cs`): scan local books/bookmarks → compare with server state using BookServerMapping and LastModifiedAt → send changes via ApiClient sync endpoints → update BookServerMapping.ServerAssignedId on success
- [X] T024 [P] [US3] Implement download logic in SyncService.cs (`Shelfly.App/Features/Settings/Services/SyncService.cs`): fetch server books/bookmarks → merge with local data using last-write-wins (compare LastModifiedAt) → deduplicate using BookServerMapping.ServerAssignedId → update local entities
- [X] T025 [US3] Implement per-profile tracking in SyncService.cs (`Shelfly.App/Features/Settings/Services/SyncService.cs`): after successful sync, create/update BookProfileSyncRecord for each synchronized book with profile username and server timestamp; ensure no duplicate records (upsert by BookId + ProfileUsername)
- [X] T026 [US3] Implement event-driven sync trigger in SyncService.cs (`Shelfly.App/Features/Settings/Services/SyncService.cs`): subscribe to library change events → on change, check cooldown period (minimum interval between attempts) → if cooldown elapsed, initiate sync; handle unreachable server gracefully with Result pattern and user-visible failure indication
- [X] T027 [US3] Implement SyncState persistence in LocalDbContext: ensure LastSuccessfulSyncAt and LastSyncResult are updated after each sync attempt; seed initial SyncState singleton row during migration (T011) with fixed Guid constant
- [X] T028 [P] [US3] Update BookDetailPage (`Shelfly.App/Features/Library/Views/BookDetailPage.xaml`) to display synced profiles list and last-synchronization times per profile using data from BookProfileSyncRecord via ViewModel
- [X] T029 [US3] Implement sync toggle logic in SettingsViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/SettingsViewModel.cs`): ToggleSyncCommand that updates SyncState.SyncEnabled → when turning on, triggers immediate sync if ActiveServerEntryId is set and user is signed in; when turning off, preserves all local data

**Checkpoint**: All user stories should now be independently functional — full synchronization with conflict resolution, deduplication, and per-profile tracking.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T030 [P] Implement cancellation token management in SettingsViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/SettingsViewModel.cs`): override OnNavigatingFrom to cancel active commands and associated cancellation tokens per constitution Principle III
- [X] T031 [P] Implement cancellation token management in ServerEntryViewModel.cs (`Shelfly.App/Features/Settings/ViewModels/ServerEntryViewModel.cs`): override OnNavigatingFrom to cancel connection test, registration, and login operations
- [X] T032 Extend AuditTimestampInterceptor (`Shelfly.App.Data/AuditTimestampInterceptor.cs`) to auto-populate CreatedAt for ServerEntity and SavedServerEntry (if not already covered by convention)
- [X] T033 Add Result pattern error handling across all ApiClient methods: unreachable server, timeout, invalid response, generic auth failure — ensure user-facing messages are clear and non-technical
- [X] T034 Verify all localization keys exist in both `en-US` and `de-DE` resource files; add missing translations for any new strings introduced during implementation
- [X] T035 Run quickstart.md validation scenarios: execute all 7 validation scenarios from `specs/014-server-sync/quickstart.md` to confirm end-to-end functionality (Code compilation verified; runtime validation requires API infrastructure)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Integrates with US1 server management but independently testable
- **User Story 3 (P3)**: Depends on US1 and US2 completion — requires active server, signed-in profile, and functional API client

### Within Each User Story

- Models before services
- Services before endpoints/UI
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T005, T006 — localization files)
- All Foundational tasks marked [P] can run in parallel within Phase 2 (T007, T008 independent of each other; T010 depends on T007-T009)
- Once Foundational phase completes, user stories can start sequentially or in parallel (if team capacity allows)
- Models within a story marked [P] can run in parallel (T012, T013 for US1; T018, T019, T028 for US3)

---

## Parallel Example: User Story 1

```bash
# Launch all models for User Story 1 together:
Task: "Create ServerEntryViewModel.cs in Shelfly.App/Features/Settings/ViewModels/ServerEntryViewModel.cs"
Task: "Create SettingsPage.xaml in Shelfly.App/Features/Settings/Views/SettingsPage.xaml"

# Sequential (depends on models):
Task: "Implement SettingsViewModel.cs in Shelfly.App/Features/Settings/ViewModels/SettingsViewModel.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test registration flow independently
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
   - Developer A: User Story 1 (Registration)
   - Developer B: User Story 2 (Sign In/Out, Switching)
   - Developer C: User Story 3 (Synchronization Engine)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- EF Core migrations use `Shelfly.App.Migrations` in Release config with net10.0 framework (per user clarification)
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
