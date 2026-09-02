# Data Model: FAB Add Button

**Date**: 2026-09-02  
**Feature**: specs/008-fab-add-button/spec.md

## Entities

### No New Data Entities Required

This feature is a UI layout change only. The following existing components are reused without modification:

| Component | Role | Changes |
|-----------|------|---------|
| `BookListViewModel` | Provides navigation command | Existing `NavigateToAddBookCommand` bound to FAB tap |
| `BookEntity` | Book data model | No changes — used by CollectionView item template |
| `Routes.BookEditPage` | Navigation target | No changes — same route as toolbar add button |

## Validation Rules

No new validation rules introduced. The feature relies on existing navigation infrastructure and command binding patterns established in the Library feature slice.

## State Transitions

The FAB introduces a single interaction state:

| Trigger | Action | Result |
|---------|--------|--------|
| User taps FAB | `NavigateToAddBookCommand` executes | Shell navigates to BookEditPage via `Routes.BookEditPage` |
