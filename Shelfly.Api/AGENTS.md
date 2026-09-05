# Shelfly.Api — Agent Notes

## Quirks

- **Request validation**: FluentValidation in the API.

## Architecture

- **Data**: PostgreSQL (EF Core + Npgsql) for primary data; MongoDB for configuration.
- **Auth**: Keycloak handles authn/authz; API delegates to it.
- **Minimal hosting**: single `Program.cs`, endpoints via `app.MapGet()` etc., no Controllers.
- **API surface**: both REST and GraphQL.
- **Entity models** in `Shelfly.Api/Data/Entities/`, separate from Common domain classes.

### Keycloak Authentication Flow

1. **Startup**: load config (issuer URL, audience, JWKS endpoint) from MongoDB.
2. **Caching**: in-memory, 5-minute TTL.
3. **JWT validation**: against Keycloak issuer via JWKS discovery.
4. **Audience match**: custom `JwtAudienceValidator` checks `aud`; mismatch → 401.
5. **Role-based access**: MongoDB rules map roles to endpoints; enforced at runtime.
6. **Runtime refresh**: admins can update config/rules without restarting.

### Configuration Storage (MongoDB)

- `KeycloakConfig` (`_id: "keycloak"`): issuer URL, audience, JWKS endpoint.
- `PostgreSqlConfig` (`_id: "postgresql"`): PostgreSQL config.
- `AuthorizationRule` (`_id: "auth-rules"`): array of endpoint→role mappings.
- **Seeding**: defaults seeded on first startup if the collection is empty.

### Resilience

- MongoDB wrapped with Polly retry (exponential backoff, max 5 attempts); graceful failure with a clear error after retries.
- `IMemoryCache` cuts config read latency.
