# Implementation Plan: Remote Server Synchronization

**Branch**: `014-server-sync` | **Date**: 2026-09-16 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/014-server-sync/spec.md`

## Summary

The MAUI client gains the ability to register accounts on a remote sync server, sign in/out, and synchronize books and bookmarks across multiple devices. The user manages servers via a new Settings page: adding URLs, testing connections, switching between saved servers (one active at a time), toggling synchronization on/off, and signing in/out. All implementation work targets the client-side only; server-side code is out-of-scope.

## Technical Context

**Language/Version**: C# 14 / .NET 10.0 (MAUI multi-target: net10.0-android, net10.0-ios, net10.0-maccatalyst, net10.0-windows)

**Primary Dependencies**: 
- `Microsoft.Maui.Controls` (UI framework)
- `CommunityToolkit.Mvvm` (MVVM patterns)
- `Microsoft.EntityFrameworkCore.Sqlite` (local persistence)
- `System.Net.Http` (API communication — no external HTTP client libraries added per Dependency Policy)

**Storage**: SQLite via EF Core (`LocalDbContext`) for device-local data; new entities: ServerEntity, SavedServerEntry, SyncState, BookServerMapping, BookProfileSyncRecord

**Testing**: TUnit framework with Shouldly assertions; integration tests use TestContainers (PostgreSQL, MongoDB, Keycloak) in `Shelfly.Api.Tests`; client unit tests in `Shelfly.App.Tests`

**Target Platform**: Cross-platform MAUI app (Android always, iOS/MacCatalyst on non-Linux, Windows conditionally); primary development target is Windows for rapid iteration

**Project Type**: Mobile/desktop application with local-first data storage and optional server synchronization

**Performance Goals**: Connection test completes in under 5 seconds; initial sync of library up to 1000 books completes in under 30 seconds on typical network conditions

**Constraints**: 
- Client-side only (server code out-of-scope)
- Exactly one active server at a time
- Credentials stored encrypted at rest on device
- All communication over HTTPS (encrypted in transit)
- App remains fully usable offline/local when no server is configured or reachable

**Scale/Scope**: Supports 5+ saved servers per device; library scale up to 10,000 books with bookmarks; synchronization handles bidirectional merge with last-write-wins conflict resolution

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|------------|-------|
| **I. SOLID & SoC** | ✓ Pass | New entities in `Shelfly.App.Data/Entities/`; domain models remain framework-agnostic; sync logic separated from UI concerns |
| **II. Vertical Slice** | ✓ Pass | New "Settings" feature created under `Features/Settings/` with all concerns co-located (page, ViewModel, services) |
| **III. MVVM Pattern** | ✓ Pass | SettingsPage inherits `ShelflyContentPageBase`; SettingsViewModel inherits `ShelflyViewModelBase`; registered via `AddScopedWithShellRoute`; uses `ObservableProperty`, `RelayCommand` |
| **IV. Coding Standards** | ✓ Pass | Explicit types (no `var`); collection expressions; primary constructors; nullable reference types; Result pattern for error handling |
| **V. Data Management** | ✓ Pass | New entities use UUID v7 (`Guid.CreateVersion7()`); soft-delete filter preserved on BookEntity; cascade delete configured for child entities; last-write-wins per constitution |
| **VI. API Design** | N/A (client-side) | Client communicates with existing REST endpoints; no new server endpoints created in this feature |
| **VII. Auth & User Mgmt** | ✓ Pass | Client talks to API only (never Keycloak directly); JWT tokens stored encrypted on device; profile data isolated per active entry |
| **VIII. Localization** | ✓ Pass | All user-facing text uses `.resx` resource files; new keys added to both `en-US` and `de-DE` simultaneously |
| **IX. Asset Formats** | N/A (no new assets) | Settings page uses existing iconography patterns |
| **X. Microsoft Docs** | ✓ Pass | EF Core migration patterns, MAUI HttpClient usage, CommunityToolkit patterns verified via microsoft-learn MCP before implementation |
| **XI. IDE Refactoring** | ✓ Pass | Rider MCP tools used for structural changes (renaming, interface extraction) during implementation phase |

## Project Structure

### Documentation (this feature)

```text
specs/014-server-sync/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── api-contract.md  # Server interface assumptions
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/
│   ├── Settings/                    # NEW feature
│   │   ├── Views/
│   │   │   ├── SettingsPage.xaml
│   │   │   └── ServerListPage.xaml  # Optional: dedicated server management view
│   │   ├── ViewModels/
│   │   │   ├── SettingsViewModel.cs
│   │   │   └── ServerEntryViewModel.cs
│   │   └── Services/
│   │       ├── SyncService.cs       # Synchronization logic
│   │       └── ApiClient.cs         # HTTP client wrapper for API communication
│   └── Authentication/              # UPDATED feature
│       ├── Views/
│       │   ├── LoginPage.xaml       # Functional implementation added
│       │   └── RegistrationPage.xaml # Functional implementation added
│       └── ViewModels/
│           ├── LoginViewModel.cs    # Commands, validation, API calls
│           └── RegistrationViewModel.cs

Shelfly.App.Data/
├── Entities/
│   ├── ServerEntity.cs              # NEW: canonical server reference (config via attributes)
│   ├── SavedServerEntry.cs          # NEW: saved entry with profile (config via attributes)
│   ├── SyncState.cs                 # NEW: per-device sync state (config via attributes)
│   ├── BookServerMapping.cs         # NEW: book-to-server ID mapping (config via attributes)
│   └── BookProfileSyncRecord.cs     # NEW: per-profile sync history (config via attributes)
└── LocalDbContext.cs                # UPDATED: new DbSet properties; relationships configured on entities

Shelfly.App.Migrations/              # NEW migration(s)
├── <Timestamp>_AddServerSyncEntities.cs  # Migration file
└── (DesignTimeDbContextFactory unchanged)

Shelfly.App/Resources/               # Localization updates
├── en-US/AppResources.resx          # NEW keys: settings labels, sync messages
└── de-DE/AppResources.resx          # NEW keys: German translations

Directory.Packages.props             # UPDATED: if new NuGet packages needed (approved per Dependency Policy)
```

**Structure Decision**: Feature-based organization under `Shelfly.App/Features/Settings/` follows Vertical Slice Architecture (Principle II). The Settings feature contains all concerns: views, ViewModels, and services. Data entities live in `Shelfly.App.Data/Entities/` with configuration via attributes directly on entity classes ([Key], [Required], [MaxLength], [Index], [ForeignKey]). Migrations target `Shelfly.App.Migrations` per user specification. Authentication feature is updated in-place (existing skeleton pages become functional).

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A — all principles satisfied | Client-side scope keeps complexity contained; no new projects or dependencies required beyond HttpClient | Single-feature addition with clear boundaries |
