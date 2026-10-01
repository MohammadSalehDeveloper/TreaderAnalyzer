# Changelog

Agents **must** record every change here under **Unreleased** before proposing a commit. The human owner commits; agents never commit.

Format: newest first. After a real commit, move the matching Unreleased item into a dated/git-sha section if you maintain history that way.

## Unreleased

### Startup script top-level await
- **Why:** The Blazor startup script wrapped `await` in an async function instead of awaiting at the top level.
- **What:** Marked that script as a module and awaited `Blazor.start` directly. The loader script still runs first.
- **Layers:** Web (`Web.Client` markup only).
- **Tests:** None. This is static markup, and `Tests.Unit` does not reference `Web.Client`.
- **Proposed commit message:** Await Blazor startup at the top level of the loading script.

### Loading canvas accessibility
- **Why:** The startup chart was marked `aria-hidden` on a canvas, which assistive tools treat as a focusable element.
- **What:** Removed `aria-hidden` from `#forensics-canvas`. The loader status text remains the accessible description.
- **Layers:** Web (`Web.Client` markup only).
- **Tests:** None. This is static markup, and `Tests.Unit` does not reference `Web.Client`.
- **Proposed commit message:** Remove aria-hidden from the loading canvas so a focusable element is not hidden from assistive tech.

### SQL password out of appsettings
- **Why:** The SQL Server SA password was committed in `appsettings`, so anyone with the repo had the database credential.
- **What:** Removed the password from account `appsettings`. The API and EF factory add it from `Database__Password` or `MSSQL_SA_PASSWORD`. Compose requires that variable from `deploy/docker/.env` instead of a default in the file.
- **Layers:** API.Account / Infra.Persistence / deploy Docker. No domain or package changes.
- **Tests:** `tests/Tests.Unit/Persistence/SqlServerConnectionStringTests.cs`
- **Proposed commit message:** Stop committing the SQL Server password and read it from the environment instead.

### Create admin API
- **Why:** There was no way to create an admin account, so the admin panel could not be signed into.
- **What:** Added `POST /api/admin/users`, which dispatches the existing `CreateAdminCommand`. The first call is open while no admin exists; later calls require an admin token. The Blazor client does not call it.
- **Layers:** API (`API.Account`). No domain, persistence, or package changes.
- **Tests:** `tests/Tests.Unit/Application/Users/Commands/CreateAdmin/CreateAdminCommandHandlerTests.cs`
- **Proposed commit message:** Add a manual create-admin endpoint so the first admin can be registered without a client screen.

### Login request validation
- **Why:** Sign-in crashed before the handler ran, because ASP.NET Core ignores validation attributes placed on record properties.
- **What:** Moved `Required` and `EmailAddress` on the auth and account request records from the property to the constructor parameter.
- **Layers:** API (`API.Account` controllers only).
- **Tests:** None. The failure is inside MVC model binding, and `Tests.Unit` does not reference `API.Account`.
- **Proposed commit message:** Put validation on auth request constructor parameters so login no longer crashes before the handler runs.

### Trader and admin panels
- **Why:** Signed-in traders need a place to see their account, and admins need to activate or suspend users without using the database.
- **What:** Added a panel layout in `Web.Client`. Traders get `/app`, `/app/profile`, and `/app/password` on the existing account API. Admins get `/admin`, backed by new `AdminUsersController` actions that dispatch `GetUsers`, `ActivateUser`, and `SuspendUser`. Login and registration send each role to its panel. The public header links there when a session exists.
- **Layers:** Web (`Web.Client`) / API (`API.Account`). No domain, persistence, or package changes.
- **Tests:** `tests/Tests.Unit/Application/Users/Queries/GetUsers/GetUsersQueryHandlerTests.cs`, `tests/Tests.Unit/Application/Users/Commands/ActivateUser/ActivateUserCommandHandlerTests.cs`, `tests/Tests.Unit/Application/Users/Commands/SuspendUser/SuspendUserCommandHandlerTests.cs`
- **Proposed commit message:** Add trader and admin panels so each role has a workspace after sign-in, and admins can activate or suspend users from the existing account commands.

### Public home page
- **Why:** Give the public site a home with a shared header and footer, and introduce trading panels, the AI system, and the platforms before those workspaces exist.
- **What:** Replaced the Blazor sidebar shell in `Web.Client` with a sticky header (logo, Trading, Platforms, Hubs, About Us, search, download, Login) and a footer. The home page has a sample candlestick animation plus three sections. Login and register stay on the auth layout. Search jumps to those sections; the download icon opens the desktop platform note. There is no installer file yet.
- **Layers:** Web (`Web.Client` only).
- **Tests:** None. The page is presentational markup, and `Tests.Unit` does not reference `Web.Client`. Adding a browser test package would need approval.
- **Proposed commit message:** Add a public home page so visitors can see the product, platforms, and account entry before the trading workspace exists.

### Docker SQL Server as the local database
- **Why:** Give persistence a portable SQL Server so development does not depend on a Windows SQL Server install, while keeping PostgreSQL as a later second provider.
- **What:** Added a dedicated SQL Server 2022 Compose file (healthcheck, volume, env template), aligned Account connection strings and the design-time factory to that instance, applied migrations on Development startup, and guarded `AddPersistence` so only SQL Server is accepted today.
- **Layers:** Infra.Persistence / API.Account / deploy Docker (no domain or new NuGet packages).
- **Tests:** `tests/Tests.Unit/Persistence/*`, `tests/Tests.Integration/Persistence/SqlServerContainerTests.cs`
- **Proposed commit message:** Run SQL Server in Docker so persistence has a portable local database before PostgreSQL is added.

### Agent protocols and guard files
- **Why:** Constrain every agent in this repo: no direct commits, no silent architecture/package/domain/persistence changes, mandatory docs and unit tests, Clean Architecture / Clean Code / ASP.NET practices.
- **What:** Added `AGENTS.md`, `.cursor/rules/*`, `.cursor/skills/{propose-commit,write-project-docs,write-unit-tests}`, and this changelog.
- **Layers:** Docs / agent tooling only (no domain, persistence, or package changes).
- **Tests:** None required (protocol files only; no production behavior).
- **Proposed commit message:** Add agent guard rules so contributors and coding agents cannot commit blindly or change architecture without approval.

## Related

- [Agent protocols](./agent/PROTOCOLS.md)
- [Project overview](./PROJECT_OVERVIEW.md)
