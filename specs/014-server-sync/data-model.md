# Data Model: Remote Server Synchronization

**Date**: 2026-09-16 | **Branch**: `014-server-sync`

## New Entities (Device-Local Storage)

All new entities are stored in the local SQLite database via `LocalDbContext`. They are device-local only and do not travel to the server.

### ServerEntity

Represents a unique sync server as the canonical reference for all saved-server entries and book-to-server mappings.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` (v7) | `[Key]`, generated via `Guid.CreateVersion7()` | Canonical identifier |
| `Url` | `string` | `[Required]`, `[MaxLength(2048)]` | Server URL; validated as valid URI |
| `DisplayName` | `string?` | nullable, `[MaxLength(64)]` | Optional user-defined display name |
| `CreatedAt` | `DateTime` | Auto-populated by `AuditTimestampInterceptor` | Entity creation time |

**Indexes**:
- Unique on `(Url)` — prevents duplicate server entries for the same URL

**Query Filter**: None (hard deletion; removed servers are physically deleted)

### SavedServerEntry

Represents a saved server entry with an associated profile. Multiple entries may reference the same ServerEntity (e.g., different accounts on the same URL).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` (v7) | `[Key]`, generated via `Guid.CreateVersion7()` | Entry identifier |
| `ServerId` | `Guid` | `[Required]`, FK to ServerEntity | Reference to server entity |
| `ProfileUsername` | `string` | `[Required]`, `[MaxLength(128)]` | Profile username on the server |
| `ProfileEmail` | `string` | `[Required]`, `[MaxLength(256)]` | Profile email address |
| `IsActive` | `bool` | Default `false` | Active flag; exactly one entry is active at a time |
| `CreatedAt` | `DateTime` | Auto-populated by `AuditTimestampInterceptor` | Entry creation time |

**Indexes**:
- On `(ServerId)` — for join performance
- On `(IsActive)` — for quick lookup of active entry

**Relationships**:
- Many-to-one with ServerEntity (multiple entries can reference the same server)
- Cascade delete: when ServerEntity is deleted, all referencing SavedServerEntry records are deleted

### SyncState

Tracks per-device synchronization state.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` (v7) | `[Key]`, singleton record | Single row; ID is fixed constant |
| `ActiveServerEntryId` | `Guid?` | nullable, FK to SavedServerEntry | Currently active server entry |
| `SyncEnabled` | `bool` | Default `true` | Synchronization toggle state |
| `LastSuccessfulSyncAt` | `DateTime?` | nullable | Timestamp of last successful sync (server time) |
| `LastSyncResult` | `string?` | nullable, `[MaxLength(512)]` | Human-readable result message |

**Indexes**: None (singleton record)

**Query Filter**: None

### BookServerMapping

Maps a book to its server-assigned ID for a specific server. One row per (book, server) pair.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` (v7) | `[Key]`, generated via `Guid.CreateVersion7()` | Mapping identifier |
| `BookId` | `Guid` | `[Required]`, FK to BookEntity | Reference to the book |
| `ServerId` | `Guid` | `[Required]`, FK to ServerEntity | Reference to the server entity |
| `ServerAssignedId` | `string` | `[Required]`, `[MaxLength(256)]` | ID assigned by the server for this book |

**Indexes**:
- Unique on `(BookId, ServerId)` — one mapping per book-server pair
- On `(ServerAssignedId, ServerId)` — for deduplication during sync

**Relationships**:
- Many-to-one with BookEntity (cascade delete: when book is hard-deleted, mappings are deleted)
- Many-to-one with ServerEntity (cascade delete: when server entity is deleted, mappings are deleted)

### BookProfileSyncRecord

Tracks per-profile synchronization history for a book (device-local only).

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `Guid` (v7) | `[Key]`, generated via `Guid.CreateVersion7()` | Record identifier |
| `BookId` | `Guid` | `[Required]`, FK to BookEntity | Reference to the book |
| `ProfileUsername` | `string` | `[Required]`, `[MaxLength(128)]` | Profile username (from SavedServerEntry) |
| `LastSyncAt` | `DateTime` | Server time of last successful sync for this profile |

**Indexes**:
- Unique on `(BookId, ProfileUsername)` — one record per book-profile pair
- On `(ProfileUsername)` — for listing books synced with a profile

**Relationships**:
- Many-to-one with BookEntity (cascade delete: when book is hard-deleted, records are deleted)

## Modified Existing Entities

### BookEntity (Extensions)

No new fields added to BookEntity itself. The synchronization tracking is handled through the new `BookServerMapping` and `BookProfileSyncRecord` entities, keeping the core book model clean and decoupled from sync concerns.

**Navigation properties** (added):
- `ICollection<BookServerMapping> ServerMappings` — one-to-many relationship
- `ICollection<BookProfileSyncRecord> ProfileSyncRecords` — one-to-many relationship

### BookmarkEntity (No Changes)

Bookmarks continue to travel with their parent book during synchronization. No new fields or relationships required. Per-profile tracking is recorded on the book level (per spec).

## Entity Relationships Diagram

```
ServerEntity (1) ←→ (*) SavedServerEntry
    ↑                    ↓
    |                    |
    |                    | (ActiveServerEntryId FK)
    |                    ↓
    |                  SyncState (singleton)
    |
    ↓
BookServerMapping (*) → BookEntity (1)
    ↑
    |
BookProfileSyncRecord (*) → BookEntity (1)
```

## Validation Rules

| Rule | Entity | Details |
|------|--------|---------|
| URL format | ServerEntity | Must be valid URI; scheme must be `https://` (per FR-012 encrypted connection requirement) |
| Unique URL | ServerEntity | No two server entities share the same URL (case-insensitive comparison) |
| Single active entry | SavedServerEntry | Exactly one entry has `IsActive = true`; activating another auto-deactivates the previous |
| Profile uniqueness per server | SavedServerEntry | Username + ServerId combination is unique; prevents duplicate profiles on the same server |
| SyncState singleton | SyncState | Only one row exists; enforced by application logic (fixed Guid constant) |
| Mapping uniqueness | BookServerMapping | One mapping per (BookId, ServerId) pair |
| Profile record uniqueness | BookProfileSyncRecord | One record per (BookId, ProfileUsername) pair |

## State Transitions

### SavedServerEntry Activation Flow

1. User selects a saved server entry from the list
2. The previously active entry is set to `IsActive = false`
3. The selected entry is set to `IsActive = true`
4. SyncState.ActiveServerEntryId is updated to point to the new active entry
5. Synchronization begins with the newly active profile

### Sign-Out Flow

1. Active SavedServerEntry remains in database (not deleted)
2. SyncState.ActiveServerEntryId is set to `null`
3. SyncState.SyncEnabled is set to `false`
4. No data is exchanged with any server

### Synchronization Toggle Flow

- **On → Off**: SyncState.SyncEnabled = false; no immediate data loss; pending changes preserved locally
- **Off → On**: SyncState.SyncEnabled = true; triggers synchronization if ActiveServerEntryId is set and user is signed in

## Audit Timestamp Interceptor Extension

The existing `AuditTimestampInterceptor` will be extended to auto-populate `CreatedAt` for new entities (ServerEntity, SavedServerEntry). The interceptor already handles BookEntity and BookmarkEntity. No changes required to the interceptor logic — it applies to all entities with a `CreatedAt` property by convention.

## Migration Notes

- New entities require a migration via `Shelfly.App.Migrations` in Release configuration
- Existing BookEntity and BookmarkEntity require navigation properties added (no data loss)
- The migration should be small and focused; consider splitting into multiple migrations if needed
- SyncState singleton row should be seeded during the migration with a fixed Guid constant
