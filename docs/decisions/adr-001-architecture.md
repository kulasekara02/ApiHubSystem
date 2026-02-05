# ADR-001: Clean Architecture with CQRS

## Status
Accepted

## Context
We need to build a scalable, maintainable web application for API management. The system needs to handle multiple external API integrations, complex business logic, and various user roles.

## Decision
We will use Clean Architecture with CQRS (Command Query Responsibility Segregation) pattern.

### Architecture Layers

1. **Domain Layer** - Pure business entities and rules
2. **Application Layer** - Use cases with MediatR commands/queries
3. **Infrastructure Layer** - External concerns (DB, APIs, services)
4. **Presentation Layer** - API and Web UI

### Key Libraries

- **MediatR** - For CQRS pattern
- **FluentValidation** - For input validation
- **AutoMapper** - For object mapping
- **Entity Framework Core** - For data access

## Consequences

### Positive
- Clear separation of concerns
- Testable business logic
- Easy to add new features
- Framework-agnostic domain
- Supports multiple presentation layers

### Negative
- More boilerplate code
- Learning curve for developers
- Slight performance overhead from MediatR pipeline

## Alternatives Considered

1. **Traditional N-Tier** - Rejected due to tight coupling
2. **Vertical Slice Architecture** - Considered but team more familiar with Clean Architecture
3. **Microservices** - Overkill for initial scope

---

# ADR-002: PostgreSQL as Primary Database

## Status
Accepted

## Context
Need a reliable, scalable database for storing API records, user data, and audit logs.

## Decision
Use PostgreSQL as the primary database with Entity Framework Core.

## Rationale
- Open source with strong community
- JSON support for flexible data storage
- Good performance with proper indexing
- Easy to run in Docker
- EF Core has excellent PostgreSQL support

## Consequences
- Need PostgreSQL expertise
- Slightly more complex setup than SQLite

---

# ADR-003: JWT Authentication

## Status
Accepted

## Context
Need stateless authentication for both web UI and API clients.

## Decision
Use JWT tokens for authentication with ASP.NET Core Identity for user management.

## Token Structure
- Access token: Short-lived (60 minutes)
- Refresh token: Longer-lived
- Claims: UserId, Email, Roles

## Security Measures
- HTTPS enforced
- Token stored in HttpOnly cookies for web
- Rate limiting on auth endpoints

---

# ADR-004: Polly for Resilience

## Status
Accepted

## Context
External API calls can fail due to network issues, rate limiting, or service outages.

## Decision
Use Polly for implementing resilience patterns.

## Policies
1. **Retry**: 3 attempts with exponential backoff
2. **Circuit Breaker**: Opens after 50% failure rate
3. **Timeout**: 30 seconds per request
4. **Rate Limiter**: 100 concurrent requests

## Consequences
- More reliable external API interactions
- Graceful degradation under failure
- Additional complexity in HTTP client configuration
