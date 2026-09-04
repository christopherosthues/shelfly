# Contract: BookCardView Gesture Interface

**Date**: 2026-09-04  
**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)

## Overview

This contract defines the gesture command interface exposed by `BookCardView`. Page-level XAML templates bind to these commands to wire selection and navigation behavior.

## BindableProperties

### LongPressCommand

| Attribute | Value |
|-----------|-------|
| **Property Name** | `LongPressCommand` |
| **Type** | `ICommand?` |
| **Default Value** | `null` |
| **Parameter** | `BindingContext` (BookEntity instance) |
| **Fired When** | Pointer release occurs after >= 500ms hold duration |

**XAML Binding Example**:
```xml
<controls:BookCardView>
    <controls:BookCardView.LongPressCommand>
        {Binding EnterSelectionModeCommand, 
                 Source={x:RelativeSource AncestorType={x:Type local:BookListViewModel}}}
    </controls:BookCardView.LongPressCommand>
</controls:BookCardView>
```

### TapCommand

| Attribute | Value |
|-----------|-------|
| **Property Name** | `TapCommand` |
| **Type** | `ICommand?` |
| **Default Value** | `null` |
| **Parameter** | `BindingContext` (BookEntity instance) |
| **Fired When** | Normal tap on unselected card (`IsSelected == false`) |

**XAML Binding Example**:
```xml
<controls:BookCardView>
    <controls:BookCardView.TapCommand>
        {Binding NavigateToDetailBookCommand, 
                 Source={x:RelativeSource AncestorType={x:Type local:BookListViewModel}}}
    </controls:BookCardView.TapCommand>
</controls:BookCardView>
```

## Existing Properties (Unchanged)

### IsSelected

| Attribute | Value |
|-----------|-------|
| **Property Name** | `IsSelected` |
| **Type** | `bool` |
| **Default Value** | `false` |
| **Purpose** | Drives visual selection indicator; set by view model after command execution |

## Command Execution Flow

### Long Press Sequence

```
1. User presses and holds (PointerPressed event)
   → pressTime = DateTime.Now

2. User releases finger (PointerReleased event)
   → elapsed = DateTime.Now - pressTime.Value
   
3. If elapsed >= 500ms:
   → IsSelected = !IsSelected (toggle state)
   → LongPressCommand?.Execute(BindingContext)
   
4. View model receives BookEntity parameter
   → Executes selection logic (add/remove from SelectedItemIds)
   → Updates IsSelectionMode flag
   
5. Property changed callback animates SelectionIndicator
```

### Tap Sequence (Unselected Card)

```
1. User taps card quickly (< 500ms)
   
2. If IsSelected == false:
   → TapCommand?.Execute(BindingContext)
   
3. View model receives BookEntity parameter
   → Executes navigation logic
   
4. Detail page opens with book ID
```

### Tap Sequence (Selected Card)

```
1. User taps card quickly (< 500ms)
   
2. If IsSelected == true:
   → IsSelected = false (deselection only)
   → TapCommand NOT fired
   
3. View model selection tracking updated via LongPressCommand binding
   (or page-level logic handles deselection)
```

## Binding Requirements for Consumers

### Required Bindings

Page-level XAML must bind:
1. **LongPressCommand** → Selection command (`EnterSelectionMode` or `ToggleSelection`)
2. **TapCommand** → Navigation command (`NavigateToDetailBookCommand`)
3. **IsSelected** → View model selection state (two-way binding to reflect card state)

### Recommended Bindings

- **HapticFeedback**: Optional vibration on long press recognition for tactile feedback
- **SemanticProperties.Description**: Accessibility description reflecting current selection state

## Compatibility Notes

| Platform | Long Press Detection | Tap Gesture | Command Binding |
|----------|---------------------|-------------|-----------------|
| Android | Pointer events (timing) | Supported | Full XAML binding support |
| iOS/MacCatalyst | Pointer events (timing) | Supported | Full XAML binding support |
| Windows | Pointer events (timing) | Supported | Full XAML binding support |

**Secondary button caveat**: On iOS/MacCatalyst, secondary pointer press may fire `PointerPressed` followed immediately by `PointerReleased`. The timing logic should handle this edge case.
