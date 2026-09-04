# Contract: BookCardView UI Bindings

**Date**: 2026-09-04  
**Feature**: 012-book-card-info-sorting

## Binding Requirements

### Bookmark Count Display

| Property | Type | Position | Binding Source |
|----------|------|----------|----------------|
| `BookmarkCount` | `int` | Top right corner of card | `BookEntity.BookmarkCount` (computed) |

**Display Rules**:
- Always visible — displays "0" for books with no bookmarks
- Positioned at top right of the card layout
- Uses existing label styling consistent with other card metadata

### Last Modified Date Display

| Property | Type | Position | Binding Source |
|----------|------|----------|----------------|
| `DisplayLastModifiedAt` | `DateTime` | Bottom right corner of card | `BookEntity.DisplayLastModifiedAt` (computed: `LastModifiedAt ?? CreatedAt`) |

**Display Rules**:
- Always visible — displays a formatted date string
- Positioned at bottom right of the card layout
- Date format follows localization conventions (platform-specific or resource-defined)
- When LastModifiedAt is null, displays CreatedAt value

## Layout Contract

The BookCardView grid must accommodate two new label elements:

```
┌─────────────────────────────────────┐
│ [Selection]  Title         [Count] │ ← Top right: Bookmark count
│              Author                        │
│              Publisher   [Date]    │ ← Bottom right: Last modified date
└─────────────────────────────────────┘
```

**Grid Structure Changes**:
- Add new row/column definitions to accommodate the two new labels
- Maintain existing content positioning (Title, Author, Publisher)
- Ensure minimum height request accommodates additional content

## XAML Binding Contract

Both properties must be bindable from BookEntity DataContext:

```xml
<!-- Bookmark count label -->
<Label Text="{Binding BookmarkCount}" 
       Grid.Column="..." Grid.Row="..." />

<!-- Last modified date label -->
<Label Text="{Binding DisplayLastModifiedAt, StringFormat='{0:yyyy-MM-dd}'}"
       Grid.Column="..." Grid.Row="..." />
```

**Compiled Binding**: Use `x:DataType="entities:BookEntity"` to enable compile-time validation via XAML source generation.
