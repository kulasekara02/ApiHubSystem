# Architecture Overview

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                        │
│  ┌─────────────────┐                    ┌─────────────────────┐ │
│  │   Blazor Web    │                    │      REST API       │ │
│  │   (ApiHub.Web)  │                    │    (ApiHub.Api)     │ │
│  └────────┬────────┘                    └──────────┬──────────┘ │
└───────────┼─────────────────────────────────────────┼───────────┘
            │                                         │
            ▼                                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Application Layer                          │
│                    (ApiHub.Application)                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────┐ │
│  │ Commands │  │  Queries │  │Validators│  │    Behaviors     │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────────────┘ │
│                          MediatR                                 │
└───────────────────────────────┬─────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Domain Layer                              │
│                      (ApiHub.Domain)                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────┐ │
│  │ Entities │  │  Value   │  │  Domain  │  │   Exceptions     │ │
│  │          │  │ Objects  │  │  Events  │  │                  │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
                                ▲
                                │
┌───────────────────────────────┴─────────────────────────────────┐
│                     Infrastructure Layer                         │
│                   (ApiHub.Infrastructure)                        │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────────┐ │
│  │ EF Core  │  │ External │  │ Security │  │    Services      │ │
│  │  DbCtx   │  │   APIs   │  │ Services │  │ (Cache, Files)   │ │
│  └──────────┘  └──────────┘  └──────────┘  └──────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
            │                      │
            ▼                      ▼
     ┌──────────┐           ┌──────────────┐
     │PostgreSQL│           │ External APIs│
     │  Redis   │           │  (5 connectors)
     └──────────┘           └──────────────┘
```

## Layer Responsibilities

### Domain Layer (ApiHub.Domain)
- **Entities**: ApplicationUser, Connector, ApiRecord, Dataset, Report, UploadedFile, AuditLog
- **Value Objects**: DateTimeRange, ApiKeySettings
- **Enums**: UserRole, HttpMethodType, AuthenticationType, ConnectorStatus
- **Domain Events**: ApiRequestCompletedEvent, UserRegisteredEvent
- **Exceptions**: DomainException, NotFoundException, ValidationException

### Application Layer (ApiHub.Application)
- **Commands**: RegisterCommand, LoginCommand, CreateConnectorCommand, SendApiRequestCommand
- **Queries**: GetConnectorsQuery, GetApiRecordsQuery, GetDashboardDataQuery
- **Interfaces**: IApplicationDbContext, ITokenService, IApiRunnerService, etc.
- **Behaviors**: ValidationBehavior, LoggingBehavior, PerformanceBehavior
- **Mapping**: AutoMapper profiles

### Infrastructure Layer (ApiHub.Infrastructure)
- **Persistence**: EF Core DbContext, configurations, migrations
- **External APIs**: JSONPlaceholder, Petstore, ReqRes, GitHub, OpenWeather clients
- **Security**: TokenService, EncryptionService, AuditService
- **Services**: FileStorageService, FileParser, ReportGenerator, CacheService
- **Resilience**: Polly policies (retry, circuit breaker, timeout)

### Presentation Layer

#### ApiHub.Api
- REST API controllers with versioning (v1, v2)
- Swagger/OpenAPI documentation
- JWT authentication
- Rate limiting

#### ApiHub.Web
- Blazor Server-side components
- Dashboard, Connectors, API Runner, History, Analytics, Reports, Uploads pages
- Bootstrap 5 styling

## Data Flow

### API Request Flow

```
1. User sends request via Web UI or API
          │
          ▼
2. AuthController validates JWT token
          │
          ▼
3. SendApiRequestCommand created
          │
          ▼
4. MediatR pipeline:
   - LoggingBehavior (logs request)
   - ValidationBehavior (validates input)
   - Handler processes request
          │
          ▼
5. ApiRunnerService:
   - Gets connector config from DB
   - Retrieves user's API credentials (decrypted)
   - Applies Polly resilience policies
   - Sends HTTP request
          │
          ▼
6. ApiRecord created and saved
          │
          ▼
7. AuditService logs the action
          │
          ▼
8. Response returned to user
```

## Security

- **Authentication**: ASP.NET Core Identity + JWT tokens
- **Authorization**: Role-based (Admin, Analyst, Viewer)
- **API Keys**: Encrypted at rest using AES-256
- **Audit Logging**: All significant actions logged
- **Input Validation**: FluentValidation on all inputs
- **HTTPS**: Enforced in production

## Resilience

- **Retry Policy**: 3 retries with exponential backoff
- **Circuit Breaker**: Opens after 50% failure rate
- **Timeout**: 30 seconds per request
- **Rate Limiting**: 100 concurrent requests

## Observability

- **Structured Logging**: Serilog with correlation IDs
- **Metrics**: OpenTelemetry instrumentation
- **Health Checks**: Database, Redis connectivity
- **Distributed Tracing**: Ready for Jaeger/Zipkin
