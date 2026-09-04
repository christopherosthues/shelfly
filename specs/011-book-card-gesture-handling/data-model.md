# Data Model: Book Card Gesture Commands

**Date**: 2026-09-04  
**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Overview

This feature modifies the `BookCardView` control to expose gesture commands. No new database entities or domain models are introduced. The data model focuses on control state and command interface contracts.

## Control State Model

### BookCardView Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsSelected` | `bool` | `false` | BindableProperty tracking selection state; drives visual indicator visibility |
| `LongPressCommand` | `ICommand?` | `null` | **NEW** Command fired when long press threshold (500ms) is exceeded |
| `TapCommand` | `ICommand?` | `null` | **NEW** Command fired on normal tap when card is unselected |

### Internal State Fields

| Field | Type | Description |
|-------|------|-------------|
| `pressTime` | `DateTime?` | Records pointer press timestamp for long press detection |
| `LongPressThreshold` | `const int` | Fixed at 500 milliseconds; threshold for distinguishing tap from long press |

## Command Interface Contract

### LongPressCommand

**Parameter**: `BindingContext` (BookEntity instance)  
**Fired when**: Pointer release occurs after >= 500ms hold duration  
**Expected binding target**: View model selection command (`EnterSelectionMode`, `ToggleSelection`)

```csharp
// Example view model command signature
void EnterSelectionMode(BookEntity book);
void ToggleSelection(BookEntity book);
```

### TapCommand

**Parameter**: `BindingContext` (BookEntity instance)  
**Fired when**: Normal tap on unselected card (`IsSelected == false`)  
**Expected binding target**: View model navigation command (`NavigateToDetailBookCommand`)

```csharp
// Example view model command signature
Task NavigateToDetailBookAsync(Guid bookId);
```

## Selection State Flow

### Data Flow Diagram

```
User Gesture          BookCardView              Page ViewModel
─────────────        ───────────────           ────────────────
                    IsSelected = false
                    │
Long Press (≥500ms) │
    │               ▼
    ├──── LongPressCommand.Execute(BindingContext)
    │               │
    │               ├──── ToggleSelection(book) or EnterSelectionMode(book)
    │               │         │
    │               │         ├──── Add book.Id to SelectedItemIds
    │               │         └──── Set IsSelectionMode = true
    │               ▼
    │           IsSelected = true (visual update via animation)
    │
Normal Tap          │
    │               ▼ (IsSelected == false)
    ├──── TapCommand.Execute(BindingContext)
    │               │
    │               └──── NavigateToDetailBookAsync(book.Id)
```

### State Transitions

| Current State | Gesture | Next State | Command Fired |
|---------------|---------|------------|---------------|
| Unselected (`IsSelected = false`) | Long Press | Selected (`IsSelected = true`) | `LongPressCommand` → view model selection command |
| Selected (`IsSelected = true`) | Long Press | Unselected (`IsSelected = false`) | `LongPressCommand` → view model deselection command |
| Selected (`IsSelected = true`) | Normal Tap | Unselected (`IsSelected = false`) | None (deselection only) |
| Unselected (`IsSelected = false`) | Normal Tap | Unselected (`IsSelected = false`) | `TapCommand` → navigation command |

## Validation Rules

- **LongPressCommand parameter**: Must be non-null BookEntity; control passes `BindingContext` directly
- **TapCommand condition**: Only fires when `IsSelected == false`; selected cards use tap for deselection
- **Selection indicator animation**: Y-axis rotation (360° when selected, 0° when unselected) with 300ms duration and SinInOut easing

## Existing View Model Infrastructure

### BookListViewModel Selection Commands

| Command | Signature | Behavior |
|---------|-----------|----------|
| `ToggleSelectionCommand` | `void ToggleSelection(BookEntity book)` | Adds/removes book.Id from SelectedItemIds |
| `EnterSelectionModeCommand` | `void EnterSelectionMode(BookEntity book)` | Sets IsSelectionMode=true, adds book.Id to SelectedItemIds |
| `ExitSelectionModeCommand` | `void ExitSelectionMode()` | Sets IsSelectionMode=false, clears SelectedItemIds |

### BookListViewModel Navigation Commands

| Command | Signature | Behavior |
|---------|-----------|----------|
| `NavigateToDetailBookCommand` | `Task NavigateToDetailBookAsync(Guid bookId)` | Navigates to Routes.BookDetailPage with book ID parameter |
