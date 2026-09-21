# Research: Keycloak Authentication Endpoints

**Date**: 2026-09-20  
**Feature**: 015-keycloak-auth-endpoints

## Technical Decisions Resolved

### Decision 1: HTTP Client for Keycloak Admin API Communication

**Rationale**: The API needs to communicate with Keycloak's Admin REST API. Since the project already uses `Microsoft.Extensions.Http.Resilience` and `Polly`, we'll leverage these for resilient HTTP communication.

**Alternatives considered**:
- **Kiota-generated client**: Provides strongly-typed SDK but adds dependency overhead and may not cover all Keycloak endpoints needed
- **Raw HttpClient**: Flexible but requires manual serialization/deserialization
- **Refit**: Declarative REST client with clean syntax, but adds external dependency

**Selected approach**: Use `HttpClient` with resilience policies from existing packages. This avoids new dependencies while maintaining code clarity through well-structured service methods. The Keycloak Admin API uses standard JSON payloads that serialize cleanly via `System.Text.Json`.

### Decision 2: Token Format for Authentication Responses

**Rationale**: The API must return authentication tokens to clients after successful login and refresh operations.

**Alternatives considered**:
- **Raw JWT from Keycloak**: Pass through the access token directly
- **Custom wrapper format**: Wrap Keycloak tokens in application-specific response structure
- **Opaque reference tokens**: Store token metadata server-side, return opaque references

**Selected approach**: Return Keycloak's standard token response format (access_token, refresh_token, expires_in) wrapped in a consistent DTO. This preserves interoperability with existing Keycloak clients while providing predictable response structure. The API validates the JWT against configured issuer using JWKS discovery before returning it to ensure token integrity.

### Decision 3: Password Reset Flow Design

**Rationale**: Password reset requires coordination between the API and Keycloak's email-based flow.

**Alternatives considered**:
- **Direct password update**: API receives new password, updates Keycloak directly (requires admin privileges)
- **Email-based token flow**: Keycloak sends reset link to user email; user follows link to complete reset
- **API-mediated flow**: API generates reset code, sends via email service, validates code before updating Keycloak

**Selected approach**: Use Keycloak's built-in password reset flow (`POST /admin/realms/{realm}/users/{id}/execute-actions-email` with `ACTION: UPDATE_PASSWORD`). This leverages Keycloak's existing email infrastructure and security controls. The API delegates the action execution to Keycloak, which handles token generation, email delivery, and link validation.

### Decision 4: Rate Limiting Implementation

**Rationale**: FR-011 requires configurable rate limiting for login attempts with lockout thresholds.

**Alternatives considered**:
- **In-memory counter**: Simple but loses state on restart; single-instance only
- **Redis-backed limiter**: Distributed, persistent, but adds infrastructure dependency
- **MongoDB-backed limiter**: Uses existing infrastructure; survives restarts
- **Keycloak native limiting**: Keycloak has built-in brute force detection

**Selected approach**: Leverage Keycloak's native brute force detection (`/admin/realms/{realm}/attack-detection/brute-force/users/{userId}`). The API queries this endpoint to check user lockout status before forwarding login attempts. This avoids duplicating logic and ensures consistency with Keycloak's security policies. For additional application-level rate limiting, use MongoDB to track attempt counts per IP/user combination.

### Decision 5: OpenTelemetry Instrumentation Strategy

**Rationale**: FR-014 requires full distributed tracing for authentication events.

**Alternatives considered**:
- **Manual span creation**: Full control but verbose boilerplate
- **Auto-instrumentation only**: Minimal code but limited custom attributes
- **Hybrid approach**: Auto-instrumentation for HTTP pipeline + manual spans for Keycloak calls

**Selected approach**: Hybrid instrumentation using existing OpenTelemetry packages. The `OpenTelemetry.Instrumentation.AspNetCore` package automatically captures endpoint metrics and traces. Custom spans wrap each Keycloak Admin API call to capture delegation latency, operation type, and user context. This provides end-to-end visibility without excessive boilerplate.

## Best Practices Identified

### Minimal API Endpoint Registration (C# 14+)

**Source**: Microsoft Learn documentation on ASP.NET Core minimal APIs

- Use extension methods for endpoint registration to enable clean syntax: `app.MapAuthEndpoints()`
- Leverage C# 12+ features like primary constructors and collection expressions
- Apply Result pattern consistently — no exceptions thrown from endpoints
- Validate requests using FluentValidation before business logic executes

### Problem Details Format (RFC 7807)

**Source**: RFC 7807 specification + ASP.NET Core implementation

- Use `Microsoft.AspNetCore.Http.Results.Problem()` for error responses
- Include extension fields for validation errors: `"extensions": { "errors": [...] }`
- Map HTTP status codes appropriately: 400 (validation), 401 (auth failure), 409 (conflict), 429 (rate limit)

### Keycloak Admin API Security

**Source**: Keycloak documentation on admin REST API access

- Admin API requires service account with appropriate roles
- Use client credentials grant for server-to-server communication
- Store service account token in MongoDB configuration with refresh logic
- Apply audience validation to ensure tokens match expected realm

## Integration Patterns Identified

### Service Account Authentication Pattern

**Pattern**: The API authenticates to Keycloak's Admin API using a dedicated service account (client credentials flow). This provides programmatic access without requiring user context.

**Implementation notes**:
1. Create a confidential client in Keycloak with `service-account` enabled
2. Map appropriate roles (`realm-admin`, `view-users`, `manage-users`)
3. Store client ID and secret in MongoDB configuration
4. Use token endpoint to obtain admin access token before API calls

### JWT Validation Pattern

**Pattern**: Validate incoming JWT tokens against Keycloak issuer using JWKS discovery.

**Implementation notes**:
1. Load JWKS from `{issuer}/protocol/openid-connect/certs`
2. Cache JWKS in memory with 5-minute TTL (per constitution)
3. Validate token signature, expiration, audience claims
4. Use `JwtAudienceValidator` custom handler for audience matching

### Configuration Caching Pattern

**Pattern**: MongoDB stores runtime configuration; API caches in memory to reduce read frequency.

**Implementation notes**:
1. On startup, load `KeycloakConfig` from MongoDB
2. Store in `IMemoryCache` with 5-minute absolute expiration
3. Expose admin endpoint to refresh cache without restart
4. Apply Polly retry policy for MongoDB connection resilience

## OpenAPI Specification Analysis

The Keycloak Admin REST API provides comprehensive user management endpoints:

### User Registration
- **Endpoint**: `POST /admin/realms/{realm}/users`
- **Payload**: `UserRepresentation` with username, email, enabled status
- **Response**: 201 Created on success; 409 Conflict if duplicate

### Login Flow (Token Endpoint)
- **Standard OAuth2/OIDC flow** via Keycloak's token endpoint:
  - `POST {issuer}/protocol/openid-connect/token` with grant type
- **Admin API alternative**: Query user status before forwarding login attempt

### Token Refresh
- **Endpoint**: Standard OIDC refresh grant
- **Payload**: `grant_type=refresh_token`, `refresh_token={token}`
- **Response**: New access/refresh token pair

### Password Reset
- **Endpoint**: `POST /admin/realms/{realm}/users/{id}/execute-actions-email`
- **Payload**: Array containing `"UPDATE_PASSWORD"`
- **Behavior**: Keycloak sends email with reset link; user completes flow externally

## Summary

All technical unknowns resolved. The implementation will:
1. Use HttpClient with resilience policies for Keycloak communication
2. Follow standard OIDC token flows wrapped in consistent DTOs
3. Leverage Keycloak's native password reset and brute force detection
4. Apply MongoDB-backed rate limiting with OpenTelemetry instrumentation
5. Register endpoints via extension methods following Minimal API best practices
