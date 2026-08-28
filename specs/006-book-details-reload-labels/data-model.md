# Data Model: Floating Label Entry Control

**Date**: 2026-08-28 | **Branch**: `006-book-details-reload-labels`

## Summary

This feature is primarily a UI control enhancement. No database schema or domain model changes are required. The "data model" here refers to the control's bindable properties and localization keys.

## Control Properties

### FloatingLabelEntry Bindable Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LabelText` | `string` | `string.Empty` | The label text displayed when focused or text is present |
| `Text` | `string` (TwoWay) | `string.Empty` | Bound to the internal Entry's Text property |

## Localization Keys Required

### New Keys for AppResources.resx (en-US) and AppResources.de.resx (de-DE)

| Key | English (en-US) | German (de-DE) | Usage |
|-----|-----------------|----------------|-------|
| `FloatingLabelEntryTitle` | Title | Titel | BookEditPage - Title field label |
| `FloatingLabelEntryAuthor` | Author | Autor | BookEditPage - Author field label |
| `FloatingLabelEntryPublisher` | Publisher | Verlag | BookEditPage - Publisher field label |
| `FloatingLabelEntryISBN` | ISBN | ISBN | BookEditPage - ISBN field label |
| `FloatingLabelEntryStartPage` | Start Page | Startseite | BookmarkEditPage - Start page label |
| `FloatingLabelEntryEndPage` | End Page | Ende Seite | BookmarkEditPage - End page label |
| `FloatingLabelEntryNote` | Note | Notiz | BookmarkEditPage - Note field label |

## Validation Rules

No new validation rules required. Existing FluentValidation rules in the ViewModels remain unchanged. The control simply provides a better visual presentation of existing fields with clear labels.

## State Transitions

### Floating Label States

| State | Condition | Visual Behavior |
|-------|-----------|-----------------|
| **Placeholder** | Unfocused + no text | Label hidden (opacity=0), placeholder visible in Entry |
| **Floating** | Focused OR has text | Label visible above input, animated upward with fade-in |
| **Reset** | Unfocused + no text | Label animates back to hidden state |

## Relationships

The control integrates with existing infrastructure:
- Inherits from `ContentView` (consistent with `BookCardView`)
- Uses standard MAUI Entry for text input
- Compatible with existing ViewModel binding patterns via TwoWay bindable properties
- Follows localization pattern established in Constitution principle VIII
