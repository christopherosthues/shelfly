# Contract: Sync Server API Interface

**Date**: 2026-09-16 | **Branch**: `014-server-sync`

## Overview

This contract documents what the MAUI client expects from the sync server (Shelfly API). The server-side implementation is out-of-scope for this feature, but these contracts define the interface assumptions that guide client-side development.

## Connection Test

### GET /health

**Purpose**: Verify server reachability and protocol compatibility.

**Response**:
- `200 OK` with JSON body containing version information
- Used by FR-002 (connection test) to validate URL before saving

**Client expectation**: A successful HTTP 200 response indicates the server is reachable and speaks the expected protocol. Any other status or exception means connection failure.

## Authentication

### POST /v1/auth/register

**Purpose**: Register a new account on the server.

**Request Body**:
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**Response**:
- `200 OK` with JWT token and profile information on success
- `409 Conflict` if username already exists (per spec edge case)
- `400 Bad Request` for validation errors

### POST /v1/auth/login

**Purpose**: Sign in with existing credentials.

**Request Body**:
```json
{
  "username": "string",
  "password": "string"
}
```

**Response**:
- `200 OK` with JWT token and profile information on success
- `401 Unauthorized` with generic error message (per FR-013)

### POST /v1/auth/logout

**Purpose**: Sign out (invalidate session).

**Headers**: `Authorization: Bearer <JWT>`

**Response**:
- `200 OK` on success

## Synchronization

### GET /v1/sync/books

**Purpose**: Download all books for the authenticated profile.

**Headers**: `Authorization: Bearer <JWT>`

**Response**:
- `200 OK` with array of book objects including server-assigned IDs and timestamps
- Each book includes: Id, Title, Author, ISBN, Publisher, PublishDate, CreatedAt, LastModifiedAt, DeletedAt

### POST /v1/sync/books/upload

**Purpose**: Upload local books to the server.

**Headers**: `Authorization: Bearer <JWT>`

**Request Body**: Array of book objects with changes since last sync

**Response**:
- `200 OK` with confirmation and any server-assigned IDs for new books
- Conflict resolution applied (most recent change wins per LastModifiedAt)

### GET /v1/sync/bookmarks/{bookServerId}

**Purpose**: Download bookmarks for a specific book.

**Headers**: `Authorization: Bearer <JWT>`

**Response**:
- `200 OK` with array of bookmark objects including timestamps

### POST /v1/sync/bookmarks/upload

**Purpose**: Upload local bookmarks to the server.

**Headers**: `Authorization: Bearer <JWT>`

**Request Body**: Array of bookmark objects with changes since last sync

**Response**:
- `200 OK` with confirmation and any server-assigned IDs for new bookmarks

## Security Requirements (from Spec)

| Requirement | Contract Implication |
|-------------|---------------------|
| FR-011: Encrypted at rest | Client stores JWT tokens in encrypted form on device |
| FR-012: Encrypted in transit | All endpoints use HTTPS; client validates TLS certificates |
| FR-013: Generic error for invalid credentials | Login endpoint returns single generic message for 401 |

## Error Response Format

All error responses follow **RFC 7807 Problem Details** format (per constitution Principle VI):

```json
{
  "type": "https://httpstatuses.io/401",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Generic authentication error"
}
```

## Assumptions

- The server supports JWT-based authentication with tokens valid for the session duration
- The server maintains data per profile; no cross-profile data leakage occurs at the API level
- Timestamps are in UTC and use ISO 8601 format
- Server-assigned IDs are stable strings (UUID v7 or similar) that uniquely identify books/bookmarks on the server
- The API supports pagination for large datasets (books, bookmarks) — client will handle page iteration
