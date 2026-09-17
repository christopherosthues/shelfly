# Feature Specification: Remote Server Synchronization

**Feature Branch**: `014-server-sync`
**Created**: 2026-09-16
**Status**: Draft
**Input**: User description — "The user should be able to register and login into a remote server to synchronize between multiple devices. The books and bookmarks should be stored on the server. The URL should be specified by the user with an option to test the connection. It should be possible to switch between multiple servers but only one is active in the app at a time. On a settings screen the user should be able to logout, login, register, turn on/off synchronization. Each book should track the information for which profile it was synchronized (list of profiles) and when the last synchronization for that profile was."

## User Scenarios and Testing

### Scenario 1: Register an account on a new server (Happy Path)

The user opens the server section of Settings, enters the URL of a sync server, and taps "Test Connection". A clear success or failure message is shown. The user then registers an account with a username, email address, and password. After successful registration the user is automatically signed in, the server is added to the list of saved servers, and it becomes the active server.

**Acceptance Criteria**

1. The user can enter an arbitrary server URL and receive an immediate, understandable success or failure result from a connection test.
2. The registration form collects username, email, and password (with password confirmation).
3. After a successful registration the user is signed in, the server is saved, and it is marked active.
4. A failed registration (e.g., username already taken) produces a clear message and leaves the user not signed in.
5. No registration or login is possible against a server that cannot be reached.

### Scenario 2: Sign in to an existing account

The user enters (or selects) a server URL, tests the connection if desired, and signs in with existing credentials. The app confirms the sign-in and begins (or continues) synchronization with that profile's data.

**Acceptance Criteria**

1. Sign in succeeds with valid credentials and fails with a clear, generic error for invalid ones.
2. After sign in, the active server and signed-in profile are shown in Settings.
3. Sign in works from any saved server entry as well as a freshly entered URL.

### Scenario 3: Switch between multiple saved servers

The user has two or more saved servers (each a URL with its associated profile). From Settings the user sees the full list, selects a different one, and the app switches the active server. From that moment on, all synchronization uses the newly active server.

**Acceptance Criteria**

1. All saved servers are listed with their URL, profile, and active-state indicator.
2. Selecting a different server changes the active server immediately and the choice survives an app restart.
3. Only one server is ever active at a time.
4. Data belonging to another profile is never shown or synced while a profile is active.

### Scenario 4: Sign out

The user signs out of the active server in Settings. The signed-in profile is cleared, synchronization stops, and the app continues to work with the local books and bookmarks already on the device. The saved server entry remains available for a later sign in.

**Acceptance Criteria**

1. After sign out, no data is exchanged with any server.
2. Local books and bookmarks remain fully usable on the device.
3. The saved server and its profile can be selected again to sign in.

### Scenario 5: Turn synchronization on and off

The user toggles synchronization in Settings. While off, the app performs no uploads or downloads and the local library is unaffected. While on, the device's books and bookmarks are synchronized with the active profile on the active server.

**Acceptance Criteria**

1. The toggle state is visible in Settings and persists across app restarts.
2. Turning synchronization off never deletes or alters local data.
3. Turning synchronization on triggers a synchronization of the local library with the server.

### Scenario 6: Synchronization between two devices

The user signs in with the same profile on two devices. A book (with its bookmarks) created on the first device appears on the second device after synchronization, and a book edited on the second device reflects the edit on the first device after synchronization.

**Acceptance Criteria**

1. New books and bookmarks propagate from the creating device to the other device for the same profile.
2. Edits to a book or its bookmarks propagate the same way.
3. After both devices have synchronized, both devices show identical data for that profile.
4. If the same item was changed on both devices before the next sync, the most recent change is kept on both devices.

### Scenario 7: Unreachable server during synchronization

The active server is temporarily unreachable (no network, server down). The app keeps working with local data, clearly reports that synchronization could not happen, and completes the pending synchronization once the connection is restored.

**Acceptance Criteria**

1. The app never crashes or loses local data when the server is unreachable.
2. The user sees a clear "sync failed / last successful sync" indication in Settings.
3. Changes made while offline are not lost and are uploaded on the next successful synchronization.

### Edge Cases

- Signing in with wrong credentials: clear generic error; user remains not signed in; repeated attempts are not penalized beyond the server's own rules.
- Same username already exists on the server: registration fails with a message suggesting to sign in instead.
- User registers a second account on the same server URL: it is stored as a separate saved-server entry with its own data.
- Deletion of a book while synchronization is on: the deletion is part of synchronization and is reflected on the other devices for that profile.
- User changes the active server mid-session: pending synchronization for the previous profile is discarded or completed before switching; no data crosses between profiles.
- Server URL is entered with a typo or wrong scheme: the connection test and sign in fail with an understandable message, and the URL is never saved as a working server.
- Synchronization toggle is turned off and on again: no duplicate data is created on re-enable.
- Device clock is wrong: last-synchronization timestamps remain consistent (server time is authoritative).

## Requirements

### Server Management

- **FR-001**: The user can add a sync server by entering its URL in the app's settings.
- **FR-002**: The user can test the connection to an entered server URL and receive an immediate, unambiguous success or failure result before the server is used.
- **FR-003**: The user can save multiple servers; each saved server records its URL and the profile (account) associated with it.
- **FR-004**: The user can switch the active server at any time from the saved list; exactly one server is active at any time; the active choice persists across app restarts.
- **FR-005**: The user can remove a saved server from the list; removal affects only the device and never deletes data stored on the server.
- **FR-006**: A server can only be used for registration, sign in, or synchronization after a successful connection test or a successful authenticated request to it.

### Authentication and Accounts

- **FR-007**: The user can register a new account on a reachable server by providing a username, an email address, and a password.
- **FR-008**: A successful registration automatically signs the user in.
- **FR-009**: The user can sign in with existing credentials and sign out at any time from Settings.
- **FR-010**: Signing out clears the active profile and stops synchronization, but keeps the saved server entry and all local data.
- **FR-011**: Credentials are stored on the device in an encrypted (non-plain-text) form only.
- **FR-012**: All communication between the app and a server happens over an encrypted connection; the app must not transmit credentials or data over an unencrypted connection.
- **FR-013**: Invalid credentials produce a single generic error message that does not reveal which part of the credentials was wrong.
- **FR-014**: Each profile's data is isolated: data belonging to one profile is never visible, downloadable, or modifiable while a different profile is active.

### Synchronization

- **FR-015**: While synchronization is on and a profile is signed in, books and bookmarks on the device are synchronized with that profile's data on the active server in both directions (local changes go up; server changes come down).
- **FR-016**: The user can turn synchronization on and off from Settings; the toggle state is visible and persists across app restarts.
- **FR-017**: While synchronization is off, no data is uploaded or downloaded and local data is never modified by the app's synchronization behavior.
- **FR-018**: Synchronization is scoped to the active profile on the active server; switching profiles or servers never mixes data between them.
- **FR-019**: When the same book or bookmark is modified on two devices before the next synchronization, the most recent modification wins and both devices converge to the same state after synchronization.
- **FR-020**: If the server is unreachable when synchronization is attempted, the app continues to work with local data, reports the failure to the user, and preserves all local changes until the next successful synchronization.
- **FR-021**: After a successful synchronization the user can see the time of the last successful synchronization.
- **FR-022**: Re-enabling synchronization never creates duplicate books or bookmarks on the device or on the server.
- **FR-023**: Synchronization is event-driven with a cooldown period; changes trigger immediate sync, but a minimum interval must elapse between consecutive sync attempts to prevent excessive network usage.

### Per-Profile Synchronization Tracking

- **FR-024**: Every book records the list of profiles it has been synchronized with (device-local only).
- **FR-025**: For every profile in that list, the book records when the last synchronization for that profile happened.
- **FR-026**: A book becomes part of a profile's list only after a successful synchronization of that book with that profile.
- **FR-027**: The recorded profile list and last-synchronization times are visible to the user in the app (at least per book).

### Settings Screen

- **FR-028**: A single settings area provides: the list of saved servers, adding a server with URL and connection test, sign in, register, sign out, and the synchronization toggle.
- **FR-029**: The settings area always shows the active server, the signed-in profile (if any), the synchronization state (on/off), and the last successful synchronization time (if any).
- **FR-030**: The app remains fully usable for local books and bookmarks at all times, including when no server is configured, signed in, or reachable.

## Clarifications

### Session 2026-09-16

- Q: How should books be matched across devices during synchronization to ensure deduplication? → A: Server-assigned ID per server; each book maintains a list of IDs, one for each server it is synced with.
- Q: Should the "list of profiles a book was synchronized with" be stored on the server or remain device-local only? → A: Device-local only; each device tracks its own sync history per profile independently.
- Q: What synchronization frequency should be used to balance data freshness with resource consumption? → A: Event-driven with cooldown; immediate sync but minimum interval between attempts.
- Q: How should each server-assigned ID stored on a book be linked back to the server that issued it? → A: Server entity table; separate table with server ID and URL, book references server by ID and stores server-assigned ID.
- Q: Is any server-side code in-scope for this feature? → A: No; all server-side code is out-of-scope; only client-side (MAUI app) implementation is targeted.

## Key Entities

- **Server entity**: unique ID, URL, optional display name; serves as the canonical reference for all saved-server entries and book-to-server mappings.
- **Saved server entry**: references a server entity by ID, stores associated profile and active flag.
- **Profile (account)**: identity on a server (username, email); owns that profile's books and bookmarks on the server.
- **Book**: existing book record; carries a list of mappings (server entity ID → server-assigned ID) for cross-server matching; additionally carries device-local list of profiles it was synchronized with and the last-synchronization time per profile.
- **Bookmark**: existing bookmark record; travels with its parent book during synchronization.
- **Synchronization state (per device)**: active server, active profile, synchronization on/off, last successful synchronization time, last synchronization result.

## Success Criteria

1. A new user can complete the flow "enter URL → test connection → register → first successful synchronization" in under 3 minutes without assistance.
2. A book (with its bookmarks) created on one device is visible on a second device signed in with the same profile after at most one synchronization on the second device.
3. A user can keep 5 or more saved servers and switch between them; the app always synchronizes with exactly the one active server, and data is never mixed between profiles.
4. 100% of synchronization failures (unreachable server, wrong credentials, wrong URL, offline) produce a clear, understandable message, and in none of these cases is local data lost or the app left unusable.
5. All data exchanged with servers is encrypted in transit, and credentials are stored encrypted at rest on the device (verifiable by inspection of what the app stores and transmits).
6. After every successful synchronization, 100% of synchronized books show the correct profile list and a last-synchronization time for every profile in that list.
7. Turning synchronization off and back on never produces duplicate books or bookmarks (0 duplicates in repeated on/off cycles).

## Assumptions

- Synchronization is two-way: the server holds the profile's data, the device holds local data, and both are merged; the server is the shared store that makes multi-device consistency possible.
- For conflicts, "most recent change wins" is the v1 rule; no field-level merge is required.
- Each book maintains a list of mappings (server entity ID → server-assigned ID) for cross-server identity matching; the server entity table stores the URL and serves as the canonical reference.
- Profile sync history (which profiles synced which books and when) is device-local only; it does not travel to the server or other devices.
- Synchronization is on by default after a successful registration or sign in; the user can turn it off at any time.
- Bookmarks are synchronized as part of their parent book; per-profile synchronization tracking is recorded on the book (as the user description states), not separately on bookmarks.
- The existing Shelfly API is assumed to support registration, login, and synchronization operations without modification in this feature's scope.
- The server is the Shelfly sync service; "test connection" verifies that the URL is reachable and speaks the expected protocol. Arbitrary third-party servers are not a supported target.
- A saved server entry is "URL + profile". A user may register several accounts on the same URL; each appears as a separate entry.
- Removing a saved server is a device-local action; it never deletes account data on the server.
- The app is fully usable offline; synchronization happens when the app is open and the connection allows, and pending changes are kept until then.
- Only one profile is active in the app at a time (consistent with "only one active server").
- Server-side data is retained until the user deletes it through a future account-management feature (out of scope here).

## Out of Scope

- Server-side code: all implementation work targets the client-side (MAUI app) only; the existing Shelfly API is treated as an external dependency with assumed support for registration, login, and synchronization operations.
- Sharing books or bookmarks between different profiles or with other users.
- Account deletion or password change on the server (future feature).
- Synchronizing app settings, themes, or anything other than books and bookmarks.
- User self-hosting of the sync server or supporting arbitrary external storage services.
- Real-time push synchronization (synchronization is event-driven with cooldown; it occurs on app start, on demand, and when changes occur — not continuously in the background).
- Field-level or content-aware conflict resolution (beyond most-recent-wins).

## Open Questions

None. All unspecified aspects were resolved with the documented assumptions above and can be revisited during clarification or planning.
