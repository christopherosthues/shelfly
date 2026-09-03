# Contract: Trash Management API Endpoints

**Date**: 2026-09-02  
**Feature**: [spec.md](./spec.md) | [plan.md](./plan.md)

## Overview

This contract defines the REST and GraphQL endpoints required to support trash management operations. The current implementation is client-side (MAUI app with local SQLite), but these contracts establish the API surface for future server-side synchronization.

## REST Endpoints

### List Trash Items

**GET** `/v1/trash`

Returns all soft-deleted books and their associated bookmarks.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| search | string | No | Filter by title, author, publisher, or ISBN (LIKE pattern) |
| sortBy | string | No | Sort criterion: `title`, `author`, `publisher`, `publishDate` (default: `title`) |
| sortDir | string | No | Sort direction: `asc`, `desc` (default: `asc`) |

**Response**: Array of trash items containing books and their bookmarks.

### Restore Book from Trash

**PATCH** `/v1/books/{bookId}/restore`

Clears the `DeletedAt` timestamp, returning the book to active status.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| bookId | Guid | Yes | The UUID of the soft-deleted book |

**Response**: 200 OK with restored book object

### Permanently Delete Book from Trash

**DELETE** `/v1/books/{bookId}`

Physically removes a soft-deleted book and cascades deletion to all associated bookmarks.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| bookId | Guid | Yes | The UUID of the soft-deleted book |

**Response**: 204 No Content

### Restore All Trash Items

**POST** `/v1/trash/restore-all`

Clears `DeletedAt` on all soft-deleted books.

**Response**: 200 OK with count of restored items

### Delete All Trash Items

**DELETE** `/v1/trash`

Physically removes all soft-deleted books and their cascading bookmarks.

**Response**: 200 OK with count of deleted items

## GraphQL Operations

### Query: trashItems

```graphql
query TrashItems(
  $search: String,
  $sortBy: SortCriterion,
  $sortDir: SortDirection
) {
  trashItems(search: $search, sortBy: $sortBy, sortDir: $sortDir) {
    id
    title
    author
    isbn
    publisher
    publishDate
    deletedAt
    bookmarks {
      id
      startPage
      endPage
      note
    }
  }
}
```

### Mutation: restoreBook

```graphql
mutation RestoreBook($bookId: ID!) {
  restoreBook(bookId: $bookId) {
    success
    message
  }
}
```

### Mutation: hardDeleteBook

```graphql
mutation HardDeleteBook($bookId: ID!) {
  hardDeleteBook(bookId: $bookId) {
    success
    message
  }
}
```

### Mutation: restoreAllTrash

```graphql
mutation RestoreAllTrash {
  restoreAllTrash {
    count
    message
  }
}
```

### Mutation: deleteAllTrash

```graphql
mutation DeleteAllTrash {
  deleteAllTrash {
    count
    message
  }
}
```

## Enums

### SortCriterion

| Value | Description |
|-------|-------------|
| Title | Sort by book title (alphabetical) |
| Author | Sort by author name (alphabetical) |
| Publisher | Sort by publisher name (alphabetical) |
| PublishDate | Sort by publication date (chronological) |

### SortDirection

| Value | Description |
|-------|-------------|
| Ascending | A-Z or oldest-first |
| Descending | Z-A or newest-first |

## Error Responses

All endpoints follow RFC 7807 Problem Details format:

| Status Code | Type | Title | Description |
|-------------|------|-------|-------------|
| 404 | `NOT_FOUND` | Book Not Found | The specified book ID does not exist or was already hard-deleted |
| 409 | `CONFLICT` | Already Active | Restore called on a book that is not soft-deleted (`DeletedAt == null`) |
| 500 | `SERVER_ERROR` | Internal Error | Database connection failure or unexpected state |
