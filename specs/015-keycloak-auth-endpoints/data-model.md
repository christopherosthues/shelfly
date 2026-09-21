# Data Model: Keycloak Authentication Endpoints

**Date**: 2026-09-20  
**Feature**: 015-keycloak-auth-endpoints

## Overview

The authentication feature delegates user account management to Keycloak. The API maintains minimal local state — primarily runtime configuration stored in MongoDB and rate limiting counters. No local user entity models are required since Keycloak is the source of truth for accounts.

## Entities

### KeycloakConfig (MongoDB)

**Purpose**: Stores connection details and authentication parameters for Keycloak Admin API communication.

**Fields**:
- `_id`: `"keycloak"` (fixed identifier)
- `IssuerUrl`: string — Base URL of Keycloak instance (e.g., `https://auth.example.com/realms/shelfly`)
- `AdminClientId`: string — Service account client ID for Admin API access
- `AdminClientSecret`: string — Client secret for service account authentication
- `RealmName`: string — Target realm name for user operations
- `JwksEndpoint`: string — URL for JWKS discovery (auto-derived from issuer)
- `Audience`: string — Expected audience claim for JWT validation
- `LastUpdated`: DateTime? — Timestamp of last configuration update

**Validation Rules**:
- IssuerUrl must be valid HTTPS URL
- RealmName must match Keycloak realm identifier format
- Audience must be non-empty string matching configured client ID

### RateLimitEntry (MongoDB)

**Purpose**: Tracks authentication attempt counts for rate limiting and lockout enforcement.

**Fields**:
- `_id`: Guid — Unique identifier per entry
- `UserId`: string — Keycloak user ID or email address
- `IpAddress`: string? — Client IP address (nullable for non-web clients)
- `AttemptCount`: int — Number of failed attempts in current window
- `WindowStart`: DateTime — Start of current rate limit window
- `LockedUntil`: DateTime? — Lockout expiration timestamp (null if not locked)

**Validation Rules**:
- AttemptCount must be ≥ 0
- WindowStart must be within last 15 minutes for active entries
- LockedUntil must be in future when set

### AuthRequestLog (MongoDB, optional)

**Purpose**: Records authentication events for audit and observability correlation.

**Fields**:
- `_id`: Guid — Unique identifier per log entry
- `EventType`: string — "Register", "Login", "Refresh", "ResetPassword"
- `UserId`: string? — Associated user ID (nullable for registration)
- `IpAddress`: string? — Client IP address
- `UserAgent`: string? — HTTP User-Agent header
- `Timestamp`: DateTime — Event occurrence time
- `Success`: bool — Whether the operation succeeded
- `FailureReason`: string? — Error code or message on failure
- `TraceId`: string? — OpenTelemetry trace ID for correlation

**Validation Rules**:
- EventType must be one of defined values
- Timestamp must not exceed current time by more than 5 minutes (clock skew tolerance)
- TraceId format matches W3C Trace Context specification

## Relationships

### KeycloakConfig → RateLimitEntry

**Type**: One-to-many  
**Description**: Each rate limit entry references the active Keycloak configuration implicitly through realm context. Configuration changes do not invalidate existing entries but affect future evaluations.

### AuthRequestLog → RateLimitEntry

**Type**: Correlated (not foreign key)  
**Description**: Log entries and rate limit entries share user identifiers for correlation. No direct database relationship — linked via query-time joins on UserId/Timestamp.

## State Transitions

### Rate Limiting States

```
[Active] --(failed attempt)--> [Counted]
  |                              |
  |--(window expires)           |--(count >= threshold)
  v                              v
[Expired]                  [Locked]
                               |
                               |--(lockout period passes)
                               v
                             [Active]
```

**States**:
- **Active**: User can attempt authentication; counter at zero or within limits
- **Counted**: Failed attempts recorded but threshold not yet reached
- **Expired**: Rate limit window passed without new attempts; resets to Active
- **Locked**: Threshold exceeded; user blocked until lockout period expires

### Authentication Flow States

```
[Unauthenticated] --(register)--> [Registered]
                                    |
                                    |--(login success)
                                    v
                              [Authenticated]
                                    |
                                    |--(token refresh)
                                    v
                              [Token Renewed]
                                    |
                                    |--(password reset)
                                    v
                              [Password Updated]
```

**States**:
- **Unauthenticated**: No active session; user must login or register
- **Registered**: Account created in Keycloak but not yet authenticated
- **Authenticated**: Valid JWT token issued; user has active session
- **Token Renewed**: Refresh token used to obtain new access token
- **Password Updated**: Password reset flow completed successfully

## Validation Rules Summary

| Entity | Field | Rule | Error Code |
|--------|-------|------|------------|
| KeycloakConfig | IssuerUrl | Must be valid HTTPS URL | `INVALID_ISSUER_URL` |
| KeycloakConfig | RealmName | Non-empty, alphanumeric with hyphens | `INVALID_REALM_NAME` |
| RateLimitEntry | AttemptCount | ≥ 0, ≤ max threshold (10) | `RATE_LIMIT_EXCEEDED` |
| AuthRequestLog | EventType | Enum: Register/Login/Refresh/ResetPassword | `UNKNOWN_EVENT_TYPE` |

## Indexes

### MongoDB Collections

**RateLimitEntry**:
- Compound index on `(UserId, WindowStart)` for efficient window queries
- TTL index on `WindowStart` (15 minutes) for automatic cleanup

**AuthRequestLog**:
- Index on `Timestamp` for time-range queries
- Compound index on `(EventType, UserId)` for event filtering
- TTL index on `Timestamp` (30 days) for log retention

## Notes

- All entity identifiers use UUID version 7 (`Guid.CreateVersion7()`) per constitution principle V
- MongoDB collections created lazily on first write; no migration required
- Rate limit entries pruned automatically via TTL indexes
- Auth request logs retained for 30 days unless configured otherwise
