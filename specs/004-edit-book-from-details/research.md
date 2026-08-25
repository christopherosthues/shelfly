# Research: Edit Book from Details Page

**Date**: 2026-08-25 | **Status**: Complete

## Decisions

### Decision: Navigation Pattern for Edit Button

**Decision**: Use Shell navigation via `Shell.Current.GoToAsync(Routes.BookEditPage, parameters)` with book identifier passed as query parameter.

**Rationale**: Matches existing navigation patterns used by `AddBookmarkCommand` and `EditBookmarkCommand` in BookDetailViewModel. The route constant `Routes.BookEditPage` is already defined and the target page accepts `BookId` via `IQueryAttributable.ApplyQueryAttributes()`.

**Alternatives considered**:
- Direct page instantiation — less consistent with Shell-based navigation
- Message-based navigation — adds indirection without benefit for this simple case

### Decision: Edit Button Placement

**Decision**: Add ToolbarItem inside existing `<ContentPage.ToolbarItems>` block alongside the delete button.

**Rationale**: Follows established UI pattern in BookDetailPage where action buttons are placed in the toolbar. Consistent with MAUI conventions and provides discoverable access to editing functionality.

**Alternatives considered**:
- Inline button in page content — would require layout changes and visual redesign
- Context menu — less discoverable for primary edit action

### Decision: Draft Persistence on Navigation-Away

**Decision**: Edited fields ARE discarded when user navigates away without saving (no draft state).

**Rationale**: Simpler implementation; matches standard mobile app behavior. User can re-enter the edit page and data reloads from server. Clarified by user during specification phase.

**Alternatives considered**:
- Persist draft locally — adds complexity with local storage, conflict resolution on next save
- Show confirmation dialog — adds UX friction for simple navigation

### Decision: Error Handling Pattern

**Decision**: Use Result pattern to wrap save failures; display error message via existing UI mechanisms (toast/alert). App MUST NOT crash.

**Rationale**: Constitution principle IV mandates Result pattern over exceptions. Provides explicit success/failure outcomes at call site and enables compile-time verification of failure paths.

**Alternatives considered**:
- Exception-based handling — less testable, hidden control flow
- Silent retry — may mask underlying issues from user

### Decision: BookEditViewModel Loading Pattern

**Decision**: Refactor BookEditViewModel to inherit from `ShelflyViewModelBase`, implement `IQueryAttributable`, and override `LoadAsync` to fetch existing book data via `LibraryService.GetBookByIdAsync()` when `BookId != Guid.Empty`.

**Rationale**: BookDetailViewModel uses this exact pattern successfully. Constitution principle III requires ViewModels to inherit from ShelflyViewModelBase for lifecycle management, and IQueryAttributable for receiving navigation query parameters. The existing `LoadBook(BookEntity)` method in BookEditViewModel is never called — replacing it with async loading via LoadAsync ensures data freshness and follows project conventions.

**Alternatives considered**:
- Keep manual LoadBook method — requires explicit invocation at call site, breaks lifecycle management
- Pass book entity directly via navigation — couples pages tightly, bypasses service layer

## Dependencies Verified

| Dependency | Status | Notes |
|------------|--------|-------|
| `Routes.BookEditPage` route constant | Available | Defined in Routes.cs |
| BookEditViewModel accepts BookId parameter | Confirmed | Uses IQueryAttributable.ApplyQueryAttributes() |
| Existing save command with Result pattern | Present | BookEditViewModel.SaveCommand uses Result pattern |
| Localization infrastructure | Ready | AppResources.resx exists for en-US and de-DE |
