# Quickstart Validation Guide

**Date**: 2026-08-28 | **Branch**: `006-book-details-reload-labels`

## Prerequisites

1. .NET 10 SDK installed (prerelease allowed per `global.json`)
2. Android emulator or physical device with USB debugging enabled
3. API service running locally (`dotnet run --project Shelfly.Api`)

## Setup Commands

```bash
# Build the solution
dotnet build Shelfly.slnx

# Run the MAUI client on Android
dotnet run --project Shelfly.App -f net10.0-android
```

## Validation Scenarios

### Scenario 1: Floating Label Animation on Focus

**Goal**: Verify that the placeholder text animates to become a floating label when the user focuses an input field.

**Steps**:
1. Launch the Shelfly app on Android
2. Navigate to any book's details page
3. Tap the edit button to open `BookEditPage`
4. Focus on any text input field (Title, Author, Publisher, or ISBN)
5. Observe the animation: placeholder should smoothly move upward and become a visible label inside the top border

**Expected Outcome**:
- Label appears above the input with smooth animation (~200ms duration)
- Label text matches the localized resource string for that field
- Animation uses cubic easing for natural motion feel

### Scenario 2: Label Persists When Text is Entered

**Goal**: Verify that the label remains visible after entering text and unfocusing.

**Steps**:
1. On `BookEditPage`, enter text in any field
2. Tap outside the field to unfocus
3. Observe the label state

**Expected Outcome**:
- Label remains floating above the input (does not disappear)
- Visual consistency with focused state

### Scenario 3: Label Resets on Empty Unfocus

**Goal**: Verify that the label returns to placeholder mode when the field is empty and unfocused.

**Steps**:
1. On `BookEditPage`, focus a field then clear all text
2. Tap outside to unfocus while field is empty
3. Observe the label state

**Expected Outcome**:
- Label fades out and returns to hidden placeholder mode
- Entry displays placeholder text again

### Scenario 4: Localization Works Correctly

**Goal**: Verify that labels display correctly in both English and German locales.

**Steps**:
1. Set device locale to German (de-DE)
2. Navigate to edit pages and verify all field labels are in German
3. Switch back to English (en-US) and verify labels switch accordingly

**Expected Outcome**:
- All labels use correct localized strings from `.resx` resources
- No hardcoded text visible in the UI

### Scenario 5: Bookmark Edit Page Integration

**Goal**: Verify the floating label control works correctly on `BookmarkEditPage`.

**Steps**:
1. Navigate to a bookmark's edit page
2. Test focus/unfocus animation on StartPage, EndPage, and Note fields
3. Verify numeric keyboard appears for page number fields

**Expected Outcome**:
- Floating labels work identically to BookEditPage
- Numeric input fields show appropriate keyboard

## Build Verification

After implementing the control, verify:

```bash
# Solution builds successfully
dotnet build Shelfly.slnx

# Check for XAML compilation errors (source generation enabled)
# MauiXamlInflator=SourceGen catches binding errors at compile time
```

## Reference Artifacts

- **Research**: [research.md](./research.md) - Technical decisions and implementation approach
- **Data Model**: [data-model.md](./data-model.md) - Control properties and localization keys
- **Plan**: [plan.md](./plan.md) - Full implementation plan with constitution checks
