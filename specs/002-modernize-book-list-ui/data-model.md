# Data Model: Modernize Book List UI

**Feature**: `002-modernize-book-list-ui`
**Date**: 2026-08-24

## Overview

This feature is primarily a UI modernization with no new data entities introduced. Existing data models remain unchanged.

## Affected Entities

### BookEntity (Existing)

**Location**: `Shelfly.Common/Entities/BookEntity.cs` (or similar shared location)

**Changes**: None — entity structure remains identical.

**UI Display Fields**:
- Title: string (bold display in card)
- Author: string (standard display in card)
- Publisher: string (reduced opacity display in card)

### ViewModel Impact

#### BookListViewModel (Existing)

**Location**: `Shelfly.App/Features/Library/ViewModels/BookListViewModel.cs`

**Changes**: None — view model properties and commands remain identical. Layout changes are purely presentational.

## New UI Components

### BookCardView (New Component)

**Type**: ContentView-derived custom control
**Purpose**: Reusable card wrapper for book list items

**Properties**:
- BindingContext: Bound to individual BookEntity
- Margin: 16 units horizontal (left/right), 0 vertical
- Padding: Consistent internal padding (platform-appropriate, ~12 units)
- Border.StrokeShape: RoundRectangle with corner radius
- Border.Shadow: Platform-appropriate elevation effect

**Content Structure**:
```
ContentView
└── Border (rounded corners, shadow)
    └── Grid (RowDefinitions="Auto, Auto, Auto")
        ├── Label (Title - bold)
        ├── Label (Author)
        └── Label (Publisher - reduced opacity)
```

**Interaction**:
- TapGestureRecognizer for navigation to book detail
- SwipeView compatibility maintained (SwipeView wraps BookCardView)

## Data Flow

No changes to data flow. The feature modifies only the presentation layer:

1. BookListViewModel exposes Books collection → unchanged
2. CollectionView binds to Books → unchanged
3. ItemTemplate now uses BookCardView instead of inline Grid → **changed**
4. Navigation commands remain bound identically → unchanged
