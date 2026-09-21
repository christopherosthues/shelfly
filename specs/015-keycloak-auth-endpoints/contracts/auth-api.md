# Authentication Endpoints Contract

**Date**: 2026-09-20  
**Feature**: 015-keycloak-auth-endpoints

## Overview

This document defines the REST API contract for authentication endpoints. All endpoints follow URL versioning (`/v1/auth/*`) and use RFC 7807 Problem Details format for error responses.

## Endpoints

### POST /v1/auth/register

**Summary**: Register a new user account in Keycloak

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (201 Created)**:
```json
{
  "userId": "uuid-v7-here",
  "email": "user@example.com",
  "status": "active"
}
```

**Error Responses**:
- `400 Bad Request`: Validation failure (RFC 7807 Problem Details)
- `409 Conflict`: Email already registered
- `502 Bad Gateway`: Keycloak service unavailable

### POST /v1/auth/login

**Summary**: Authenticate user and obtain JWT tokens

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK)**:
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "dGhpcyBpcy...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses**:
- `400 Bad Request`: Validation failure (RFC 7807 Problem Details)
- `401 Unauthorized`: Invalid credentials or user locked out
- `429 Too Many Requests`: Rate limit exceeded
- `502 Bad Gateway`: Keycloak service unavailable

### POST /v1/auth/refresh

**Summary**: Refresh access token using refresh token

**Request Body**:
```json
{
  "refreshToken": "dGhpcyBpcy..."
}
```

**Response (200 OK)**:
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "bmV3IHJlZn...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

**Error Responses**:
- `400 Bad Request`: Validation failure (RFC 7807 Problem Details)
- `401 Unauthorized`: Refresh token expired or revoked
- `502 Bad Gateway`: Keycloak service unavailable

### POST /v1/auth/reset-password

**Summary**: Initiate password reset flow via email

**Request Body**:
```json
{
  "email": "user@example.com"
}
```

**Response (200 OK)**:
```json
{
  "message": "Password reset link sent to user@example.com",
  "expiresIn": 900
}
```

**Error Responses**:
- `400 Bad Request`: Validation failure (RFC 7807 Problem Details)
- `404 Not Found`: Email not found in realm
- `502 Bad Gateway`: Keycloak service unavailable

## Error Response Format (RFC 7807)

All error responses follow RFC 7807 Problem Details:

```json
{
  "type": "https://shelfly.example.com/errors/validation",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more request fields are invalid",
  "instance": "/v1/auth/register",
  "extensions": {
    "errors": [
      {
        "field": "email",
        "message": "Email format is invalid"
      },
      {
        "field": "password",
        "message": "Password must be at least 8 characters"
      }
    ]
  }
}
```

## Rate Limiting Headers

Login endpoints include rate limiting headers on success:

```
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 9
X-RateLimit-Reset: 1632038400
```

On rate limit exhaustion (429):

```json
{
  "type": "https://shelfly.example.com/errors/rate-limit",
  "title": "Rate Limit Exceeded",
  "status": 429,
  "detail": "Too many login attempts. Try again in 15 minutes.",
  "instance": "/v1/auth/login",
  "retryAfter": 900
}
```

## OpenTelemetry Context

All endpoints accept and propagate W3C Trace Context headers:

```
traceparent: 00-0af7651916cd73dd34ae85ce8d2c7b6a-0000000000000001-01
tracestate: vendor1=congo,tango,echo,vendor2=009
```

This enables distributed tracing across the API and Keycloak boundary.

## Content Negotiation

**Request**: `Content-Type: application/json` (required)  
**Response**: `Content-Type: application/problem+json` (errors), `application/json` (success)
