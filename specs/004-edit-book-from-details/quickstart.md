# Quickstart: Edit Book from Details Page

**Date**: 2026-08-25 | **Status**: Complete

## Prerequisites

1. API service running locally (`dotnet run --project Shelfly.Api`)
2. PostgreSQL, MongoDB, and Keycloak containers available via `docker compose up`
3. At least one book exists in the database (use existing seeding or create via BookEditPage)
4. MAUI client built successfully (`dotnet build Shelfly.App/Shelfly.App.csproj`)

## Validation Scenarios

### Scenario 1: Navigate to Edit Page from Details

**Steps**:
1. Launch the Shelfly app and navigate to the book list
2. Tap any book card to open BookDetailPage
3. Verify an edit button appears in the toolbar (alongside delete)
4. Tap the edit button

**Expected Outcome**:
- App navigates to BookEditPage
- Form fields are pre-populated with current book data loaded via `LibraryService.GetBookByIdAsync()` (title, author, ISBN, publisher, publish date)
- Page title indicates "Edit" mode (not "Add")
- Loading indicator displays while book data is fetched

### Scenario 2: Save Changes Successfully

**Steps**:
1. From the edit page (reached via Scenario 1), modify one or more fields
2. Tap the save button

**Expected Outcome**:
- Changes persist to database via `LibraryService.UpdateBookAsync()`
- App navigates back to BookDetailPage
- Updated field values are displayed in book details within 1 second

### Scenario 3: Navigate Away Without Saving

**Steps**:
1. From the edit page, modify one or more fields
2. Use device back button (or equivalent) to navigate away from edit form

**Expected Outcome**:
- Edited fields ARE discarded (no draft persistence)
- App returns to BookDetailPage showing original data
- Re-entering edit mode reloads fresh data from server

### Scenario 4: Save Failure Handling

**Steps**:
1. From the edit page, modify a field with invalid data (e.g., malformed ISBN)
2. Tap the save button

**Expected Outcome**:
- Error message displayed to user via Result pattern
- App remains on edit form with data preserved
- App does NOT crash
- User can correct the error and retry saving

### Scenario 5: Localization Verification

**Steps**:
1. Set app language to German (de-DE)
2. Navigate to BookDetailPage and locate the edit button

**Expected Outcome**:
- Edit button text is displayed in German
- All related UI elements use localized strings from AppResources.resx

## Test Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run API locally (requires Keycloak config)
dotnet run --project Shelfly.Api

# Run MAUI client on target platform
# Android: dotnet run --project Shelfly.App -f net10.0-android
# Windows: dotnet run --project Shelfly.App -f net10.0-windows
```

## References

- **Spec**: [spec.md](./spec.md) — Full feature specification with acceptance criteria
- **Research**: [research.md](./research.md) — Technical decisions and dependency verification
- **Data Model**: [data-model.md](./data-model.md) — Entity details and data flow
