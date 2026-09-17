# Quickstart Validation Guide: Remote Server Synchronization

**Date**: 2026-09-16 | **Branch**: `014-server-sync`

## Prerequisites

1. **Shelfly API running locally** — The sync server must be accessible at a known URL (e.g., `https://localhost:5001`)
2. **Keycloak instance configured** — Registration and login endpoints require Keycloak backend
3. **PostgreSQL, MongoDB running** — Per existing `compose.yaml` infrastructure

## Setup Commands

```bash
# Start infrastructure services
docker compose up -d

# Run API (in separate terminal)
dotnet run --project Shelfly.Api

# Build client app
dotnet build Shelfly.App
```

## Validation Scenarios

### Scenario 1: Connection Test

**Goal**: Verify the user can test a server URL and receive clear success/failure results.

**Steps**:
1. Launch the MAUI app (Windows or Android)
2. Navigate to Settings → Add Server
3. Enter a valid server URL (e.g., `https://localhost:5001`)
4. Tap "Test Connection"

**Expected outcome**: Success message displayed; URL is recognized as reachable.

**Steps (failure case)**:
1. Enter an invalid URL (e.g., `https://nonexistent.local:9999`)
2. Tap "Test Connection"

**Expected outcome**: Failure message with clear indication of unreachable server.

### Scenario 2: Registration and Sign-In

**Goal**: Verify the user can register a new account and sign in.

**Steps**:
1. From Settings, enter a valid server URL and test connection (success)
2. Tap "Register"
3. Enter username, email, and password
4. Submit registration form

**Expected outcome**: Registration succeeds; user is automatically signed in; server is saved as active entry.

**Steps (existing account)**:
1. From Settings, select a saved server
2. Enter existing credentials
3. Tap "Sign In"

**Expected outcome**: Sign-in succeeds; profile is associated with the server entry.

### Scenario 3: Synchronization Toggle

**Goal**: Verify the user can turn synchronization on/off and state persists.

**Steps**:
1. After signing in, verify synchronization toggle shows "On" (default per assumption)
2. Tap to turn off
3. Restart the app
4. Check Settings screen

**Expected outcome**: Toggle remains "Off"; no data is exchanged with server while off.

### Scenario 4: Multi-Device Sync (Simulated)

**Goal**: Verify books created on one device appear on another after synchronization.

**Setup**: Use two MAUI app instances (e.g., Windows + Android emulator, or two Windows instances via different user profiles).

**Steps**:
1. On Device A: Sign in with Profile X; create a new book
2. Trigger synchronization (event-driven)
3. On Device B: Sign in with same Profile X
4. Trigger synchronization

**Expected outcome**: The book created on Device A appears on Device B after sync. Both devices show identical data for Profile X.

### Scenario 5: Server Switching

**Goal**: Verify the user can switch between saved servers and only one is active at a time.

**Steps**:
1. Register/sign in with Server A (Profile X)
2. Add Server B; register/sign in with Profile Y
3. Switch back to Server A
4. Check Settings screen

**Expected outcome**: Only Server A is marked active; synchronization uses Profile X exclusively. Data from Profile Y is not visible or synced while Profile X is active.

### Scenario 6: Sign-Out Behavior

**Goal**: Verify sign-out clears the profile, stops sync, but preserves local data and saved server entry.

**Steps**:
1. Sign in with a server; create some books locally
2. Synchronize (books uploaded to server)
3. Sign out from Settings
4. Check library view

**Expected outcome**: Local books remain visible and usable; no further data exchange occurs; saved server entry persists for future sign-in.

### Scenario 7: Unreachable Server During Sync

**Goal**: Verify the app handles unreachable servers gracefully without losing local data.

**Steps**:
1. Sign in with a server; synchronization is on
2. Stop the API server (or disconnect network)
3. Create a new book locally
4. Wait for sync attempt (event-driven)

**Expected outcome**: App shows "sync failed" indication in Settings; local book remains intact and available; pending changes are preserved for next successful sync.

## Test Commands

```bash
# Run unit tests
dotnet test Shelfly.App.Tests

# Run all solution tests
dotnet test Shelfly.slnx
```

## Success Indicators

| Criteria | Verification Method |
|----------|-------------------|
| FR-001: Add server by URL | Settings screen accepts URL input and saves it |
| FR-002: Connection test | Test button returns clear success/failure result |
| FR-007: Registration | New account created; user auto-signed in |
| FR-009: Sign in/out | Credentials accepted on sign-in; profile cleared on sign-out |
| FR-015: Two-way sync | Books propagate between devices for same profile |
| FR-016: Sync toggle | Toggle state persists across restarts |
| FR-024: Profile tracking | Book details show synced profiles and last-sync times |

## References

- **Feature spec**: `spec.md` — full requirements and acceptance criteria
- **Data model**: `data-model.md` — entity definitions and relationships
- **API contract**: `contracts/api-contract.md` — server interface assumptions
- **Research notes**: `research.md` — technical decisions and rationale
