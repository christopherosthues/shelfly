# Data Model: Edit Book from Details Page

**Date**: 2026-08-25 | **Status**: Complete

## Entities

### Book (Existing)

**Location**: `Shelfly.Api/Data/Entities/BookEntity` (persistence), `Shelfly.Common` (domain model)

**Attributes**:
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid (UUID v7) | Primary key, time-ordered |
| Title | string | Max 256 characters |
| Author | string? | Nullable |
| ISBN | string? | Validated via IsbnValidator |
| Publisher | string? | Nullable |
| PublishDate | DateTime? | Nullable |
| DeletedAt | DateTime? | Soft deletion marker (null = active) |

**Validation Rules**:
- Title: Required, max 256 characters
- ISBN: Format validated by existing IsbnValidator
- All rules enforced via FluentValidation before persistence

**State Transitions**:
- Active (`DeletedAt == null`) → Editing → Saved (updated fields persisted) or Discarded (navigation away without save)

## Relationships

| Entity | Related To | Type | Notes |
|--------|------------|------|-------|
| BookDetailViewModel | BookEntity | Holds reference | Loaded via LoadAsync during page navigation |
| BookEditPage | BookEntity | Receives Id parameter | Navigated to with BookId query parameter |

## Data Flow

1. **BookDetailPage** loads book data via `LoadAsync` → populates UI
2. User taps edit button → navigates to **BookEditPage** with `BookId` parameter
3. **BookEditPage** receives `BookId` via `IQueryAttributable.ApplyQueryAttributes()` and triggers `LoadAsync`
4. **LoadAsync** calls `LibraryService.GetBookByIdAsync(BookId)` to fetch existing book data → populates form fields
5. User edits fields → saves via Result pattern using `LibraryService.UpdateBookAsync()`
6. On success: returns to BookDetailPage, which refreshes data
7. On failure: displays error message, user remains on edit form

## Schema Changes

**New tables**: None — feature reuses existing infrastructure

**New columns**: None — all required fields already exist

**Migration required**: No
