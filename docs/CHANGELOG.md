# Changelog

Agents **must** record every change here under **Unreleased** before proposing a commit. The human owner commits; agents never commit.

Format: newest first. After a real commit, move the matching Unreleased item into a dated/git-sha section if you maintain history that way.

## Unreleased

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
