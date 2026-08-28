# Quickstart: Search Empty Message Validation

## Prerequisites

- .NET 10 SDK installed
- Solution builds cleanly: `dotnet build Shelfly.slnx`
- MAUI client can run on target platform (Android emulator or Windows desktop)

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run the MAUI client (Windows example)
dotnet run --project Shelfly.App
```

## Validation Scenarios

### Scenario 1: Empty Library, No Search Active

**Steps**:
1. Launch the app and navigate to the book list page
2. Ensure the library contains zero books
3. Leave the search bar empty

**Expected Outcome**:
- The `CollectionView` displays its `EmptyView`
- The message reads: **"No books available"** (or localized equivalent)
- This is the standard generic empty state — unchanged from current behavior

### Scenario 2: Search Returns No Results

**Steps**:
1. Ensure the library contains at least one book
2. Enter a search term that matches no books (e.g., "ZzzUniqueTerm")
3. Wait for the 200ms debounce to complete

**Expected Outcome**:
- The `CollectionView` displays its `EmptyView`
- The message reads: **"No books matched your search"** (or localized equivalent)
- This is the new context-aware empty state

### Scenario 3: Clear Search After Empty Results

**Steps**:
1. Start with Scenario 2 active (search showing "no matches")
2. Clear the search bar text

**Expected Outcome**:
- The `CollectionView` reverts to displaying all books
- If the library has books, they appear immediately
- If the library is empty, revert to standard "No books available" message

### Scenario 4: Switch Between Search States Rapidly

**Steps**:
1. Type a non-matching search term → observe context-aware message
2. Clear the search bar → observe standard message or book list
3. Repeat rapidly 5+ times

**Expected Outcome**:
- Message transitions are smooth and immediate
- No stale state persists between transitions
- Debounce period (200ms) does not cause visual flicker

## Verification Checklist

- [ ] Standard empty state displays correctly when library is empty and no search active
- [ ] Search-specific empty state displays when query yields zero results
- [ ] Message switches dynamically as user types/clears search bar
- [ ] Localization keys exist in both en-US and de-DE resource files
- [ ] XAML binds to the new computed property without runtime errors

## References

- **Spec**: [spec.md](./spec.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Research**: [research.md](./research.md)
