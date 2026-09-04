# Quickstart Validation: Book Card Gesture Commands

**Date**: 2026-09-04  
**Feature**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Prerequisites

1. **Environment**: .NET 10 SDK installed with MAUI workloads
2. **Database**: PostgreSQL running (via Docker/Podman compose)
3. **Keycloak**: Authentication service configured and accessible
4. **API**: Shelfly.Api running locally or via container
5. **Data**: Library contains at least 3 book entries for testing

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run API (if not already running)
dotnet run --project Shelfly.Api

# Launch MAUI client (Android emulator or desktop)
dotnet run --project Shelfly.App
```

## Validation Scenarios

### Scenario 1: Long Press Selects Card

**Goal**: Verify long press gesture fires selection command and updates visual state.

**Steps**:
1. Navigate to the library list page (BookListPage)
2. Locate a book card in the visible area
3. Press and hold on the card for >= 500ms
4. Release finger

**Expected Outcome**:
- Checkmark indicator appears at leading edge of card with Y-axis rotation animation
- Card text position remains stable (no layout shift)
- View model's `IsSelectionMode` becomes `true`
- Book ID is added to `SelectedItemIds` collection

**Validation Points**:
- [ ] Visual selection indicator displays correctly
- [ ] Animation completes within 300ms
- [ ] Text content position unchanged
- [ ] Selection state tracked in view model

---

### Scenario 2: Long Press Deselects Selected Card

**Goal**: Verify long press on already-selected card toggles selection off.

**Steps**:
1. Navigate to the library list page
2. Long press a book card to select it (checkmark visible)
3. Long press the same card again for >= 500ms
4. Release finger

**Expected Outcome**:
- Checkmark indicator disappears with Y-axis rotation animation
- Card text position remains stable
- View model's `SelectedItemIds` no longer contains this book ID
- If this was the only selected item, `IsSelectionMode` becomes `false`

**Validation Points**:
- [ ] Visual deselection indicator animates correctly
- [ ] Selection state removed from view model
- [ ] Text content position unchanged

---

### Scenario 3: Normal Tap Navigates Unselected Card

**Goal**: Verify normal tap on unselected card fires navigation command.

**Steps**:
1. Navigate to the library list page
2. Locate an unselected book card (no checkmark visible)
3. Tap the card quickly (< 500ms press duration)

**Expected Outcome**:
- Navigation occurs to BookDetailPage with correct book ID parameter
- Card remains in unselected state (`IsSelected = false`)
- No selection indicator appears

**Validation Points**:
- [ ] Detail page opens for tapped book
- [ ] Correct book data displayed on detail page
- [ ] Selection state unchanged (still unselected)

---

### Scenario 4: Normal Tap Deselects Selected Card

**Goal**: Verify normal tap on selected card deselects without navigation.

**Steps**:
1. Navigate to the library list page
2. Long press a book card to select it
3. Tap the same card quickly (< 500ms)

**Expected Outcome**:
- Checkmark indicator disappears (deselected)
- No navigation occurs (remain on BookListPage)
- View model's `SelectedItemIds` no longer contains this book ID

**Validation Points**:
- [ ] Card becomes unselected visually
- [ ] Navigation did NOT occur
- [ ] Selection state removed from view model

---

### Scenario 5: Multi-Card Selection

**Goal**: Verify multiple cards can be selected via long press.

**Steps**:
1. Navigate to the library list page
2. Long press first book card (selects it)
3. Long press second book card (selects it)
4. Long press third book card (selects it)
5. Tap one of the selected cards (deselects only that card)

**Expected Outcome**:
- Three checkmark indicators visible simultaneously
- After tap, two checkmarks remain; tapped card is unselected
- View model's `SelectedItemIds` contains exactly 2 IDs after deselection

**Validation Points**:
- [ ] Multiple selections persist independently
- [ ] Tap deselects only the tapped card
- [ ] Other selected cards remain selected
- [ ] No text position shifts during any selection change

---

### Scenario 6: Gesture Coexistence

**Goal**: Verify tap and long press gestures do not interfere with each other.

**Steps**:
1. Navigate to the library list page
2. Tap card A quickly (navigates to detail)
3. Return to list page
4. Long press card B for >= 500ms (selects it)
5. Tap card C quickly (navigates to detail)

**Expected Outcome**:
- Card A navigated correctly on first tap
- Card B selected via long press without navigation interference
- Card C navigated correctly despite card B being selected elsewhere
- All gestures produce intended outcomes independently

**Validation Points**:
- [ ] Tap gesture fires navigation command for unselected cards
- [ ] Long press gesture fires selection command
- [ ] Gestures do not interfere with each other across different cards

## Rollback Criteria

If any validation point fails:
1. Check `BookCardView.xaml.cs` for correct BindableProperty definitions
2. Verify pointer event handlers fire commands with correct parameters
3. Confirm XAML bindings in BookListPage.xaml wire to view model commands
4. Review gesture coexistence logic (tap vs long press discrimination)

## References

- [Control Interface Contract](./contracts/bookcardview-gesture-contract.md) - Command signatures and binding expectations
- [Data Model](./data-model.md) - State transitions and validation rules
- [Research Notes](./research.md) - Technical decisions and platform compatibility
