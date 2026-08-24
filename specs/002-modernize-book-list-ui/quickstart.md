# Quickstart Validation Guide: Modernize Book List UI

**Feature**: `002-modernize-book-list-ui`
**Date**: 2026-08-24

## Prerequisites

1. .NET 10 SDK installed (prerelease allowed per global.json)
2. Java SDK configured for Android builds (warning may appear during build)
3. Device or emulator available for testing (Android, iOS, Windows, or macOS)

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run API locally (if needed for data population)
dotnet run --project Shelfly.Api
```

## Validation Scenarios

### Scenario 1: Responsive Layout - Wide Screen

**Setup**: Launch app on landscape device or wide-screen emulator.

**Steps**:
1. Open the book list page
2. Observe the top controls area (search bar + sort picker)

**Expected Outcome**:
- Search bar and sort picker appear horizontally aligned side by side at the top of the page
- Both elements are fully visible without truncation
- Layout matches FR-001, FR-002 acceptance criteria

### Scenario 2: Responsive Layout - Narrow Screen

**Setup**: Launch app on portrait device with narrow screen (e.g., small phone).

**Steps**:
1. Open the book list page
2. Observe the top controls area

**Expected Outcome**:
- Sort picker appears beneath the search bar in a stacked layout
- Both elements remain fully functional and visible
- Layout matches FR-003 acceptance criteria

### Scenario 3: Card-Wrapped Book Items

**Setup**: Ensure at least one book exists in the library.

**Steps**:
1. Open the book list page
2. Observe each book entry's visual presentation

**Expected Outcome**:
- Each book entry is displayed within a visually distinct card
- Cards have rounded corners, consistent padding, and 16 units horizontal margin on both sides
- Visual separation exists between adjacent cards via shadow/elevation effects
- Layout matches FR-005, FR-006 acceptance criteria

### Scenario 4: Card Interaction - Tap to Navigate

**Setup**: Ensure at least one book exists in the library.

**Steps**:
1. Open the book list page
2. Tap a card-wrapped book item

**Expected Outcome**:
- Navigation to book detail view occurs immediately
- All book details display correctly in the detail view
- Interaction matches FR-008 acceptance criteria

### Scenario 5: Card Interaction - Swipe to Delete

**Setup**: Ensure at least one book exists on a touch device.

**Steps**:
1. Open the book list page
2. Swipe left on a card-wrapped book item

**Expected Outcome**:
- Swipe-to-delete gesture reveals delete action with red background
- Tapping delete soft-deletes the book and removes it from the visible list
- Gesture functions identically to previous implementation
- Interaction matches FR-009 acceptance criteria

### Scenario 6: Orientation Change Adaptation

**Setup**: Launch app on any device.

**Steps**:
1. Open the book list page in portrait orientation
2. Rotate device to landscape (or vice versa)

**Expected Outcome**:
- Layout automatically reflows to match new screen dimensions
- No data loss occurs during rotation
- Search bar and sort picker adapt position based on available width
- Adaptation matches SC-002 success criterion (<300ms adaptation time)

## Build & Run Commands

```bash
# Build App project (let MSBuild resolve MAUI target frameworks conditionally)
dotnet build Shelfly.App/Shelfly.App.csproj

# Run on connected device or emulator
dotnet run --project Shelfly.App/Shelfly.App.csproj -f net10.0-android
```

## Reference Artifacts

- **Spec**: `specs/002-modernize-book-list-ui/spec.md`
- **Research**: `specs/002-modernize-book-list-ui/research.md`
- **Data Model**: `specs/002-modernize-book-list-ui/data-model.md`
- **Plan**: `specs/002-modernize-book-list-ui/plan.md`
