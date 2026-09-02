# Data Model: Book List UI Improvements

**Date**: 2026-09-02 | **Branch**: `001-book-list-ui-improvements`

## Overview

This feature introduces client-side state changes only. No database schema or API contract modifications are required. The data model covers ViewModel properties and display objects used for presentation purposes.

## Entities

### SortDirection (Enum)

Represents the sort order direction for book list sorting.

| Field | Type | Description |
|-------|------|-------------|
| `Ascending` | enum value | Items sorted in ascending order (A-Z, oldest-first) |
| `Descending` | enum value | Items sorted in descending order (Z-A, newest-first) |

**Validation Rules**:
- Default value: `Ascending` for all sort criteria (per clarification)
- Toggling between values reverses the current direction

### SortOptionDisplay (Class)

Presentation wrapper around `SortCriterion` enum that provides localized display text for Picker items.

| Field | Type | Description |
|-------|------|-------------|
| `Criterion` | `SortCriterion` | The underlying sort criterion enum value |
| `DisplayName` | `string` | Localized string for display in the picker (from AppResources) |

**Validation Rules**:
- `DisplayName` must be non-null and non-empty
- Mapping from `SortCriterion` to resource key is deterministic:
  - `Title` → `AppResources.BookListPageSortByTitle`
  - `Author` → `AppResources.BookListPageSortByAuthor`
  - `Publisher` → `AppResources.BookListPageSortByPublisher`
  - `PublishDate` → `AppResources.BookListPageSortByPublishDate`

**Relationships**:
- Wraps `SortCriterion` enum (defined in `LibraryService.cs`)
- Used by `BookListViewModel.SortOptions` property instead of raw enum values

### BookListViewModel State Changes

New properties added to existing ViewModel:

| Property | Type | Description |
|----------|------|-------------|
| `SortDirection` | `SortDirection` | Current sort direction; defaults to `Ascending`; maintained in-memory during session |
| `SortOptions` | `List<SortOptionDisplay>` | Localized display options for the Picker (replaces raw enum list) |

**State Transitions**:
- Sort direction toggle: `Ascending ↔ Descending` on each tap of the toggle button
- Criterion change: Direction persists across criterion changes (per User Story 3 acceptance scenario #3)
- Session reset: On app restart, both properties revert to defaults (`Title`, `Ascending`)

## Localization Resources

### New Resource Keys

| Key | Purpose | Used By |
|-----|---------|---------|
| `BookListPageTitle` | Page title text | BookListPage.xaml Title binding |
| `SortDirectionAscending` | Accessibility description for ascending state | Toggle button SemanticProperties.Description (ascending) |
| `SortDirectionDescending` | Accessibility description for descending state | Toggle button SemanticProperties.Description (descending) |

### Existing Resources Reused

| Key | Current Value | New Usage |
|-----|---------------|-----------|
| `BookListPageSortByTitle` | "Title" | SortOptionDisplay.DisplayName for Title criterion |
| `BookListPageSortByAuthor` | "Author" | SortOptionDisplay.DisplayName for Author criterion |
| `BookListPageSortByPublisher` | "Publisher" | SortOptionDisplay.DisplayName for Publisher criterion |
| `BookListPageSortByPublishDate` | "Publish Date" | SortOptionDisplay.DisplayName for PublishDate criterion |

## Assumptions

- The `SortCriterion` enum in `LibraryService.cs` remains unchanged (no new sort criteria added)
- German translations follow the same pattern as existing resources (mirrored structure in `.de.resx`)
- SVG icons match the visual style of existing action icons (`add_icon.svg`, etc.)
