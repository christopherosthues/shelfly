# Research: Remote Server Synchronization

**Date**: 2026-09-16 | **Branch**: `014-server-sync`

## Technical Context Decisions

### Decision: Client-Side Scope Boundary

**Decision**: All implementation work targets the MAUI client (`Shelfly.App`) only; server-side code is out-of-scope. The existing Shelfly API is treated as an external dependency with assumed support for registration, login, and synchronization operations.

**Rationale**: User clarification explicitly states "any server side code is out-of-scope for this feature." This means the implementation focuses on:
- New local data models (server entity table, saved server entries) in `Shelfly.App.Data`
- API client infrastructure (HttpClient wrapper or StrawberryShake GraphQL client)
- Settings UI (new feature page) and authentication UI updates
- Synchronization logic (merge, conflict resolution, deduplication)

**Alternatives considered**: Including server-side implementation alongside client — rejected per user scope clarification.

### Decision: API Communication Pattern for Client

**Decision**: Use HttpClient with REST endpoints for initial synchronization communication; StrawberryShake GraphQL remains available but not yet wired in the app.

**Rationale**: The current MAUI app has no active network layer (StrawberryShake packages referenced but unused). Using HttpClient provides a simpler, more controllable integration path for:
- Connection testing (simple GET/POST to verify reachability)
- Registration and login (REST POST endpoints)
- Synchronization data exchange (upload/download books and bookmarks)

The API uses Keycloak for auth and expects JWT tokens. The client will communicate exclusively with the API (per constitution boundary rule), never calling Keycloak directly.

**Alternatives considered**:
- StrawberryShake GraphQL: available but requires schema files, code generation, and is not yet configured in the app — adds complexity for v1
- Direct Keycloak WebView auth: ruled out by constitution ("client only talks to API")

### Decision: Server Entity Storage Approach

**Decision**: Use a server entity table (separate from saved-server entries) as the canonical reference for all server URLs; books maintain mappings (server entity ID → server-assigned ID).

**Rationale**: Normalizes the relationship, avoids URL duplication across books, and enables future server metadata extensions without schema changes to the book record. This was clarified during specification.

**Alternatives considered**:
- Direct URL linkage in book records — simpler but duplicates URLs and couples book data to server identity
- Hybrid approach (URL for active, entity table for historical) — adds unnecessary complexity

### Decision: Settings Page Integration

**Decision**: Create a new Settings feature page accessible via the Shell flyout menu; consolidate server management, authentication actions, and synchronization toggle in one location.

**Rationale**: The spec requires a single settings area providing all sync-related controls (FR-028). Currently no settings page exists; the About page is the closest analog but serves a different purpose. A dedicated Settings feature aligns with the vertical slice architecture principle.

**Alternatives considered**:
- Extending the About page — mixes concerns and dilutes its license/version purpose
- Adding to existing Authentication pages — scatters sync controls across multiple screens

### Decision: Credential Storage Mechanism

**Decision**: Use `Microsoft.Extensions.Options.IOptions<T>` with encrypted persistence via `FileSystem.AppDataDirectory` for storing credentials on the device.

**Rationale**: FR-011 requires credentials stored in "encrypted (non-plain-text) form." The MAUI app already uses `FileSystem.AppDataDirectory` for the SQLite database. Using a similar approach with encryption (e.g., AES via `System.Security.Cryptography`) provides:
- Device-local storage without external dependencies
- Encrypted at rest (per FR-011)
- Survives app restarts

**Alternatives considered**:
- Secure Storage API (`Microsoft.Maui.Authentication`) — platform-specific, limited capacity, good for single tokens but not full credential sets
- SQLite encrypted database — adds complexity; credentials are small data better suited to file storage

### Decision: Synchronization Merge Strategy

**Decision**: Implement two-way sync with "most recent change wins" conflict resolution based on `LastModifiedAt` timestamps (per constitution Principle V).

**Rationale**: The spec requires bidirectional synchronization (FR-015) and the constitution establishes last-write-wins as the v1 rule. This provides:
- Predictable conflict resolution
- Simple implementation (compare timestamps, apply most recent)
- Convergence guarantee (both devices reach identical state after sync)

**Alternatives considered**:
- Field-level merge — deferred to future versions (per spec out-of-scope)
- Client-side edit tracking with server arbitration — adds complexity for v1

### Decision: Migration Project Configuration

**Decision**: Use `Shelfly.App.Migrations` project in Release configuration with net10.0 framework for all new EF Core migrations targeting LocalDbContext.

**Rationale**: User clarification specifies this as the migration project. The existing pattern uses DesignTimeDbContextFactory to create LocalDbContext instances for EF tooling. New entities (server entity, saved server entry) will be added to LocalDbContext and migrated using:
```bash
dotnet ef migrations add <Name> --project Shelfly.App.Migrations --configuration Release
```

**Alternatives considered**: N/A — user-specified constraint.

## Existing Infrastructure Summary

| Component | Current State | Relevance to Feature |
|-----------|---------------|---------------------|
| `LocalDbContext` | Manages BookEntity, BookmarkEntity; soft-delete filter on Books | New entity sets needed (server entity, saved server entry) |
| `Shelfly.App.Migrations` | DesignTimeDbContextFactory pattern; 2 existing migrations | Target for new migration(s) |
| Authentication feature | Skeleton pages/ViewModels in Features/Authentication | Needs functional implementation for login/registration |
| Settings feature | Non-existent | New feature to be created |
| API client layer | HttpClient not yet wired; StrawberryShake packages referenced | New infrastructure needed |
| `AuditTimestampInterceptor` | Auto-populates CreatedAt, LastModifiedAt on entities | Will extend to new entities |

## Key Clarifications Resolved

All NEEDS CLARIFICATION items from the specification have been resolved through prior clarification sessions:

1. **Book matching across devices** → Server-assigned ID per server; each book maintains a list of IDs
2. **Profile sync history storage** → Device-local only; each device tracks independently
3. **Synchronization frequency** → Event-driven with cooldown; immediate sync but minimum interval between attempts
4. **Server ID linkage** → Server entity table (separate table with server ID and URL)
5. **Server-side scope** → Out-of-scope; client-only implementation
