# Data Model: FAB Edit & Save UI

**Date**: 2026-09-02 | **Feature**: [spec.md](./spec.md)

## Overview

This feature is a pure UI change — no data model modifications are required. The FAB pattern affects only XAML view structure and command binding, with no changes to domain models, entity classes, or database schemas.

## Affected Components (UI Only)

### BookDetailPage
- **Change**: Replace edit ToolbarItem with FAB Grid container
- **Command bound**: `EditBookCommand` (existing RelayCommand in BookDetailViewModel)
- **Kept**: Delete toolbar item (`DeleteBookCommand`) remains unchanged

### BookEditPage
- **Change**: Replace inline save button at form bottom with FAB Grid container
- **Command bound**: `SaveCommand` (existing RelayCommand in BookEditViewModel)
- **Loading state**: FAB displays ActivityIndicator + reduced opacity when `IsSaving` is true

### BookmarkEditPage
- **Change**: Replace inline save button at form bottom with FAB Grid container
- **Command bound**: `SaveCommand` (existing RelayCommand in BookmarkEditViewModel)
- **Loading state**: FAB displays ActivityIndicator + reduced opacity when `IsSaving` is true

## No New Entities

No new database entities, DTOs, or domain models introduced. All commands and view model properties remain unchanged — only the XAML presentation layer is modified.
