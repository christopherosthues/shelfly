# Specification: Context-Aware Empty State Messages

## Feature Description

When a user searches for books and the search returns no results, the empty state message should display an appropriate, context-aware message indicating that no books matched the search query — rather than showing the generic "no books available, add one with +" message.

## User Scenarios & Testing

### Scenario 1: Search Returns No Results
1. User opens the book list page
2. User enters a search term in the search bar (e.g., "NonExistentBook")
3. The search completes with zero matching books
4. **Expected**: The empty state displays a message indicating no books matched the search query

### Scenario 2: Search Cleared Returns to Normal State
1. User has an active search with no results showing the context-aware empty message
2. User clears the search bar (or enters a term that matches existing books)
3. **Expected**: The view returns to normal — either displaying matching books or reverting to the standard "no books" empty state if the library is genuinely empty

### Scenario 3: Library Initially Empty, No Search Active
1. User opens the book list page with an empty library and no active search
2. **Expected**: The standard generic empty state message is displayed (e.g., "No books available")

## Functional Requirements

### FR-1: Context-Aware Empty Message Display
- When a search query is active AND the filtered result set is empty, the empty view MUST display a message indicating that no books matched the current search.
- The message SHOULD reference the user's search context to clarify why the list is empty.

### FR-2: Standard Empty State Preservation
- When NO search query is active AND the library contains zero books, the empty view MUST display the standard generic "no books" message.
- This preserves existing behavior for users with genuinely empty libraries.

### FR-3: Dynamic Message Switching
- The empty state message MUST update dynamically as the user types or clears the search bar.
- Transition from "search no results" to "standard empty" (or vice versa) must be seamless without requiring a page reload.

## Success Criteria

- **Verifiability**: User can visually distinguish between "no books in library" and "no matching search results" based on the displayed message.
- **Completeness**: Both empty state conditions (search vs. no-search) display distinct, appropriate messages 100% of the time.
- **User Experience**: The context-aware message reduces user confusion by clearly indicating why the list is empty during a search operation.

## Key Entities

| Entity | Description |
|--------|-------------|
| Search Query | User-provided text input used to filter the book list |
| Empty State Message | Dynamic UI text displayed when the book collection view has zero items |
| Book List View | The primary display area showing filtered or unfiltered books |

## Assumptions

- The localization system (`.resx` resource files) supports adding new message keys for both languages (en-US and de-DE).
- The empty state is rendered via the `CollectionView.EmptyView` mechanism in MAUI.
- The view model can distinguish between "search active" and "no search" states based on whether the search query property has content.

## Edge Cases

- Search term consists only of whitespace — treated as no active search (standard empty message).
- Very long search terms with no results — message remains readable and concise.
- Rapid typing/deleting during debounce period — final state reflects the last committed query.
