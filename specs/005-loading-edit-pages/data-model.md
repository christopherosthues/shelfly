# Data Model: Loading Indicators for Edit Pages

**Date**: 2026-08-28  
**Feature**: specs/005-loading-edit-pages/spec.md

## Summary

This feature is **client-side UI only** — no new entities, database changes, or API contracts are introduced. The existing data models remain unchanged. Two separate observable properties will manage loading states independently.

## Existing Entities (Modified)

### BookEditViewModel

| Property | Type | Notes |
|----------|------|-------|
| `IsLoading` | `bool` | Already declared via `[ObservableProperty]`; toggled during `LoadAsync` for full-screen overlay |
| `IsSaving` | `bool` | **NEW** — Declared via `[ObservableProperty]`; toggled during `SaveAsync` for button-level indicator |
| `BookId` | `Guid` | Empty Guid indicates create mode; non-empty indicates edit mode |

### BookmarkEditViewModel

| Property | Type | Notes |
|----------|------|-------|
| `IsLoading` | `bool` | Already declared via `[ObservableProperty]`; toggled during `LoadAsync` for full-screen overlay |
| `IsSaving` | `bool` | **NEW** — Declared via `[ObservableProperty]`; toggled during `SaveAsync` for button-level indicator |
| `BookmarkId` | `Guid` | Empty Guid indicates create mode; non-empty indicates edit mode |

## Changes Required

### Both ViewModels — Add IsSaving Property

Add a new observable property to control button-level loading feedback independently from full-screen overlay.

**Declaration**:
```csharp
[ObservableProperty]
public partial bool IsLoading { get; set; } = false;  // Full-screen overlay (existing)

[ObservableProperty]
public partial bool IsSaving { get; set; } = false;   // Button-level indicator (new)
```

### Both ViewModels — Update SaveAsync Method

Wrap save operations with `IsSaving` toggle pattern.

**Before**:
```csharp
[RelayCommand]
private async Task SaveAsync(CancellationToken cancellationToken = default)
{
    // validate...
    await service.AddBookmarkAsync(...) or UpdateBookmarkAsync(...);
}
```

**After**:
```csharp
[RelayCommand]
private async Task SaveAsync(CancellationToken cancellationToken = default)
{
    IsSaving = true;
    try
    {
        // validate...
        await service.AddBookmarkAsync(...) or UpdateBookmarkAsync(...);
    }
    finally
    {
        IsSaving = false;
    }
}
```

### Both ViewModels — Remove Computed IsNotLoading Property

The computed property `public bool IsNotLoading => !IsLoading;` is no longer needed as XAML will use `InvertedBoolConverter` for boolean negation.

## Validation Rules (Unchanged)

### BookEditViewModel
- Title: Required, max 256 characters
- Author: Required, max 256 characters
- Publisher: Required, max 256 characters
- ISBN: Validated format
- PublishDate: Optional

### BookmarkEditViewModel
- StartPage: Required, must be > 0
- EndPage: Optional, must be >= StartPage if provided
- Note: Optional, max 1000 characters
