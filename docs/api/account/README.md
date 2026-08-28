# API.Account — Overview

The **API.Account** microservice handles user authentication and account management for Trader Analyzer.

| Item | Value |
|------|-------|
| Project | `src/API/API.Account` |
| Base route | `/api` |
| Dev URL | `http://localhost:5065` (see `launchSettings.json`) |
| Docker port | `5003` (via docker-compose) |
| Auth | JWT Bearer tokens |

## Controllers

| Controller | Route prefix | Doc |
|------------|--------------|-----|
| [AuthController](./controllers/AuthController.md) | `/api/auth` | Login, logout, password reset, Google OAuth |
| [AccountController](./controllers/AccountController.md) | `/api/account` | Profile, change password, delete account |

## Features (User Stories)

| Story | Endpoint | Doc |
|-------|----------|-----|
| Login | `POST /api/auth/login` | [login.md](./features/login.md) |
| Logout | `POST /api/auth/logout` | [logout.md](./features/logout.md) |
| Change password | `PUT /api/account/change-password` | [change-password.md](./features/change-password.md) |
| Forgot password | `POST /api/auth/forgot-password` | [forgot-password.md](./features/forgot-password.md) |
| Edit profile | `PUT /api/account/profile` | [edit-profile.md](./features/edit-profile.md) |
| Delete account | `DELETE /api/account` | [delete-account.md](./features/delete-account.md) |
| Google login | `POST /api/auth/google` | [google-login.md](./features/google-login.md) |

## Architecture

```
HTTP Request
    → Controller (API.Account)
    → MediatR Command/Query (Core.Application)
    → Domain Entity rules (Core.Domain)
    → Repository / Token service (Infra.Persistence)
    → SQL Server
```

## Configuration (`appsettings.json`)

| Section | Purpose |
|---------|---------|
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `Jwt:SigningKey` | Symmetric key for access tokens (change in production) |
| `Jwt:Issuer` / `Jwt:Audience` | JWT validation |
| `Jwt:AccessTokenLifetimeMinutes` | Access token expiry (default 15) |
| `Google:ClientId` | Google OAuth client ID for ID token validation |
| `Email:PasswordResetUrlBase` | Base URL embedded in reset emails |

## Database Migration

After pulling these changes, create and apply a migration for new tables/columns:

```bash
dotnet ef migrations add AccountAuth --project src/Infra/Infra.Persistence --startup-project src/API/API.Account
dotnet ef database update --project src/Infra/Infra.Persistence --startup-project src/API/API.Account
```

New schema objects:
- `Users.GoogleSubjectId` column
- `RefreshTokens` table
- `PasswordResetTokens` table

## Related Documentation

- [PROJECT_OVERVIEW.md](../../PROJECT_OVERVIEW.md) — solution-wide architecture
