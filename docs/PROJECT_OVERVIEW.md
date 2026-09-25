# Trader Analyzer — Project Overview

This document describes what has been built so far in the **Trader Analyzer** solution: the overall architecture, implemented features, and what remains to be done next.

---

## What This Project Is

Trader Analyzer is a **.NET 9 microservices platform** for trading analysis and account management. The solution is organized as a **Clean Architecture** monorepo with separate API services, a shared core layer, infrastructure projects, a web frontend (scaffolded), and test projects.

Work completed to date establishes the **foundation** and implements the **user & account domain** end to end through the application and persistence layers, with **API.Account** wired to use them.

---

## Commit History (High Level)

| Commit | Summary |
|--------|---------|
| `3e486e0` | Initial solution structure — projects, folders, scaffolding |
| `01268d6` | Dockerfiles for all API microservices |
| `228bacb` | User management — domain, CQRS, EF Core persistence, API.Account wiring |

---

## Solution Structure

```
TraderAnalyzer/
├── src/
│   ├── API/                    # Microservice APIs
│   │   ├── API.Gateway/        # API gateway (scaffold)
│   │   ├── API.Account/        # User & account service (partially wired)
│   │   ├── API.Trading/        # Trading service (scaffold)
│   │   ├── API.MarketData/     # Market data service (scaffold)
│   │   └── API.AI/             # AI service (scaffold)
│   ├── Core/                   # Business logic (framework-agnostic)
│   │   ├── Core.Domain/        # Entities, enums, value objects, domain rules
│   │   ├── Core.Application/   # CQRS handlers, validation, mapping
│   │   └── Core.Contracts/     # DTOs and repository/security interfaces
│   ├── Infra/                  # Infrastructure implementations
│   │   ├── Infra.Persistence/  # EF Core, repositories, password hashing
│   │   ├── Infra.Cache/        # Caching (scaffold)
│   │   ├── Infra.Messaging/    # Messaging (scaffold)
│   │   └── Infra.ExternalFeeds/# External data feeds (scaffold)
│   └── Web/                    # Frontend (scaffold)
│       ├── Web.App/
│       ├── Web.Client/
│       └── Web.Components/
├── tests/
│   ├── Tests.Unit/
│   └── Tests.Integration/
├── deploy/                     # Deployment assets (placeholder)
├── NuGet.config                # NuGet source configuration
└── TraderAnalyzer.slnx         # Solution file
```

---

## Architecture

The project follows **Clean Architecture** with **CQRS** (Command Query Responsibility Segregation) via MediatR.

```mermaid
flowchart TB
    subgraph API["API Layer"]
        Account["API.Account"]
        Gateway["API.Gateway"]
        Others["API.Trading / MarketData / AI"]
    end

    subgraph Application["Core.Application"]
        Commands["Commands"]
        Queries["Queries"]
        Validation["FluentValidation"]
        MediatR["MediatR Pipeline"]
    end

    subgraph Domain["Core.Domain"]
        User["User Entity"]
        Balance["Balance Entity"]
        Rules["Domain Rules & Value Objects"]
    end

    subgraph Contracts["Core.Contracts"]
        DTOs["DTOs"]
        Interfaces["Repository Interfaces"]
    end

    subgraph Infra["Infra.Persistence"]
        EF["EF Core / SQL Server"]
        Repos["Repositories"]
        Hasher["PasswordHasher"]
    end

    Account --> Application
    Application --> Domain
    Application --> Contracts
    Infra --> Contracts
    Infra --> Domain
    EF --> Repos
```

### Dependency direction

- **Core.Domain** — no dependencies on other project layers
- **Core.Contracts** — DTOs and interfaces only
- **Core.Application** — depends on Domain + Contracts; orchestrates use cases
- **Infra.Persistence** — implements Contracts; depends on Domain
- **API.Account** — composition root for the account service; registers Application + Persistence

---

## Technology Stack

| Area | Choice |
|------|--------|
| Runtime | .NET 9 |
| API | ASP.NET Core Web API |
| CQRS / Mediator | MediatR 12 |
| Validation | FluentValidation 11 |
| Mapping | AutoMapper 14 |
| ORM | Entity Framework Core 9 |
| Database | SQL Server 2022 (Docker Linux image; PostgreSQL planned) |
| Password hashing | ASP.NET Core Identity `PasswordHasher` |
| Containerization | Docker (API Dockerfiles + Compose SQL Server) |

---

## Domain Model

### User

Central aggregate for both **Admin** and **Trader** roles.

| Field | Description |
|-------|-------------|
| `Email`, `UserName` | Unique identifiers (enforced in DB and application layer) |
| `PasswordHash` | Hashed password (never stored in plain text) |
| `Role` | `Admin` or `Trader` |
| `Status` | `Pending`, `Active`, `Suspended`, or `Closed` |
| `FirstName`, `LastName` | Profile fields |
| `PhoneNumber`, `TimeZoneId` | Optional profile fields |
| `DisplayName` | Trader-only display name |
| `PreferredCurrency` | Trader-only ISO 4217 currency code (3 letters) |
| `LastLoginAtUtc` | Updated on successful login |
| `Balance` | One-to-one relationship for traders |

**Factory methods**

- `User.CreateAdmin(...)` — creates an admin in `Active` status
- `User.CreateTrader(...)` — creates a trader in `Pending` status and auto-creates a zero balance

**Lifecycle methods**

- `Activate()`, `Suspend()`, `Close()`, `SoftDelete()`
- `UpdateProfile(...)`, `ChangePassword(...)`
- `RecordLogin()` — only allowed when user is active

All mutating operations enforce domain invariants and throw `DomainException` on invalid state transitions.

### Balance

One balance per trader, tied to their preferred currency.

| Field | Description |
|-------|-------------|
| `Available` | Funds available for use |
| `Reserved` | Funds held (e.g. for open orders) |
| `Total` | Computed: `Available + Reserved` |

**Operations**

- `Credit(amount)` — add to available
- `Debit(amount)` — remove from available (fails if insufficient)
- `Reserve(amount)` — move from available to reserved
- `Release(amount)` — move from reserved back to available
- `CaptureReserved(amount)` — consume reserved funds

### Enums

```text
UserRole:   Admin = 1, Trader = 2
UserStatus: Pending = 0, Active = 1, Suspended = 2, Closed = 3
```

### Value objects & base types

- **`Money`** — amount + 3-letter currency with same-currency arithmetic
- **`Entity`** — base type with `Guid Id`
- **`AuditableEntity`** — adds `CreatedAtUtc`, `UpdatedAtUtc`, soft-delete fields (`IsDeleted`, `DeletedAtUtc`)

---

## Application Layer (CQRS)

All use cases live under `Core.Application/Users/` and are dispatched through MediatR.

### Commands

| Command | Purpose |
|---------|---------|
| `CreateAdminCommand` | Register a new admin user |
| `CreateTraderCommand` | Register a new trader (with balance) |
| `UpdateUserProfileCommand` | Update name, phone, display name, timezone |
| `ChangeUserPasswordCommand` | Change password (hashed before storage) |
| `ActivateUserCommand` | Move user from Pending → Active |
| `SuspendUserCommand` | Suspend an active user |
| `SoftDeleteUserCommand` | Soft-delete and close the user |

Each command has a **handler** and a **FluentValidation validator**.

### Queries

| Query | Returns |
|-------|---------|
| `GetUserByIdQuery` | Single `UserDto` (optionally with balance) |
| `GetUsersQuery` | Filtered list of users by role/status |
| `GetBalanceByUserIdQuery` | `BalanceDto` for a user |

### Cross-cutting concerns

- **`ValidationBehavior`** — runs all FluentValidation validators before handlers execute
- **`ConflictException`** — thrown when email/username already exists
- **`NotFoundException`** — thrown when entity is not found
- **`MappingProfile`** — AutoMapper maps `User` → `UserDto`, `Balance` → `BalanceDto`

### Dependency injection

`Core.Application.DependencyInjection.AddApplication()` registers:

- MediatR (with validation pipeline behavior)
- FluentValidation validators
- AutoMapper

---

## Infrastructure Layer

### Persistence (`Infra.Persistence`)

**DbContext** — `AppDbContext` with:

- `DbSet<User>` and `DbSet<Balance>`
- Fluent API configurations via `IEntityTypeConfiguration`
- Global soft-delete query filter on all `AuditableEntity` types
- Automatic `UpdatedAtUtc` on save

**Repositories**

| Interface | Implementation |
|-----------|----------------|
| `IRepository<T>` | Generic `Repository<T>` |
| `IUserRepository` | `UserRepository` — lookup by email/username, list with filters |
| `IBalanceRepository` | `BalanceRepository` |
| `IUnitOfWork` | `UnitOfWork` — coordinates saves across repositories |

**Security**

- `PasswordHasher` implements `IPasswordHasher` using ASP.NET Core Identity hashing

**Design-time factory**

- `AppDbContextFactory` enables EF Core CLI migrations (`dotnet ef migrations add`) against local Docker SQL Server unless `ConnectionStrings__DefaultConnection` is set

### Database schema (configured, migrations not yet added)

**Users table**

- Unique indexes on `Email` and `UserName`
- Indexes on `Role` and `Status`
- One-to-one cascade delete to `Balances`

**Balances table**

- Linked to `Users` via `UserId` foreign key

---

## API.Account Service

`API.Account` is the first service wired to the new core layers.

**Registered in `Program.cs`:**

```csharp
builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
```

**Configuration** (`appsettings.json` / `appsettings.Development.json`):

- SQL Server connection string pointing to Docker `TraderAnalyzerDb` on `localhost:14333`
- `Database:Provider` = `SqlServer` (PostgreSQL is not implemented yet)

**Current state:**

- Application and persistence layers are registered
- OpenAPI is enabled in Development
- Development startup applies EF migrations to the configured SQL Server

**Docker:**

- Multi-stage Dockerfile builds and publishes `API.Account.dll` on .NET 9, exposing port 8080
- Compose SQL Server: `deploy/docker/docker-compose.sqlserver.yml`

---

## Other API Services (Scaffold Only)

These services exist with default ASP.NET Core templates and Dockerfiles but do **not** yet integrate with the core user domain:

| Service | Intended responsibility |
|---------|-------------------------|
| `API.Gateway` | Route and aggregate calls to backend services |
| `API.Trading` | Order execution, positions, trading logic |
| `API.MarketData` | Price feeds, candles, market snapshots |
| `API.AI` | Analysis, signals, ML-driven insights |

---

## Web Frontend

`Web.Client` serves the public home page: a header (logo, Trading, Platforms, Hubs, About Us, search, download, Login), a footer, and three sections (hero animation, trading panels and the AI system, platforms). Auth screens stay on their own layout. See [Web layer](./architecture/web.md).

`Web.App` and `Web.Components` are still scaffolds.

---

## Testing (Scaffold Only)

- `Tests.Unit` — unit test project (no tests written yet)
- `Tests.Integration` — integration test project (no tests written yet)

Both projects reference updated package versions aligned with .NET 9.

---

## Configuration & Tooling

| File | Purpose |
|------|---------|
| `NuGet.config` | Pins NuGet source to nuget.org |
| `.gitignore` | Standard .NET gitignore (includes `.env`) |
| `AppDbContextFactory` | Design-time EF Core connection for migrations CLI |

---

## Local Development Setup

### Prerequisites

- .NET 9 SDK
- Docker (SQL Server 2022 Linux image published on host port 14333)

### Database

1. Start SQL Server:

```powershell
docker compose -f deploy/docker/docker-compose.sqlserver.yml up -d
```

2. Run `API.Account` in Development — it applies existing EF migrations on startup. To apply them without starting the API:

```bash
dotnet ef database update --project src/Infra/Infra.Persistence --startup-project src/API/API.Account
```

Connection details must match Compose: `TraderAnalyzerDb` on `localhost,14333`, user `sa`. See [SQL Server](./infra/sql-server.md).

### Run API.Account

```bash
dotnet run --project src/API/API.Account
```

OpenAPI spec is available at `/openapi/v1.json` in Development.

### Run via Docker

```bash
docker build -f src/API/API.Account/Dockerfile -t trader-analyzer-account .
docker run -p 8080:8080 trader-analyzer-account
```

---

## What Is Done vs. What Is Next

### Done

- [x] Solution structure with microservice layout
- [x] Dockerfiles for all five API services
- [x] Domain model: `User`, `Balance`, enums, `Money` value object
- [x] Domain invariants and lifecycle rules
- [x] CQRS commands and queries for user management
- [x] FluentValidation on all commands/queries
- [x] MediatR pipeline with validation behavior
- [x] AutoMapper DTO mapping
- [x] EF Core persistence with repositories and unit of work
- [x] Soft-delete support with global query filters
- [x] Password hashing abstraction
- [x] API.Account DI wiring (Application + Persistence)
- [x] Docker SQL Server 2022 for local persistence
- [x] Web.Client public home page (header, footer, three sections)

### Not yet done (recommended next steps)

- [ ] PostgreSQL as a second persistence provider
- [ ] Database seeding
- [ ] Integration with `API.Gateway`
- [ ] Unit and integration tests for domain and handlers
- [ ] Trading, market data, and AI service implementations
- [ ] Trading workspace, hubs content, and the rest of the web product UI
- [ ] CI/CD pipeline and deployment manifests in `deploy/`

---

## Design Decisions

1. **Rich domain model** — business rules live on entities (`User`, `Balance`), not in handlers. Handlers orchestrate; entities enforce invariants.

2. **Trader vs. Admin separation** — same `User` entity with role-based behavior. Traders get a balance and extra profile fields; admins start active, traders start pending.

3. **Soft delete** — users are never hard-deleted. `IsDeleted` + global EF query filter keeps deleted records out of normal queries.

4. **Contracts layer** — repository interfaces and DTOs are separated from Domain so Application does not depend on EF Core.

5. **Password security** — plain passwords never touch the domain or database. Hashing happens in the application handler via `IPasswordHasher`.

6. **SQL Server in Docker first** — persistence uses EF Core SQL Server against a portable Linux image. PostgreSQL is a later second provider, not a replacement in this step.

---

## Security Note

`appsettings.json`, `AppDbContextFactory`, and Compose defaults contain a **local development** SQL Server password. Do not use these credentials in production. Prefer environment variables or a secrets manager for deployed environments.

---

## Related documentation

- [Agent protocols](./agent/PROTOCOLS.md) — no direct commits; architecture changes require approval; docs and tests are mandatory
- [Changelog](./CHANGELOG.md) — explanation of each change / proposed commit
- [Architecture docs](./architecture/README.md) · [Web](./architecture/web.md) · [Domain](./domain/README.md) · [Infra](./infra/README.md) · [Tech](./tech/README.md)
- [SQL Server](./infra/sql-server.md) · [Persistence](./infra/persistence.md) · [Docker](./tech/docker.md)

---

*Last updated: September 2026 — public home page on Web.Client; feature status in this overview may lag the code.*
