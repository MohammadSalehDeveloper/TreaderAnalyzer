# SQL Server

Local and containerized **SQL Server 2022** is the first persistence database for Trader Analyzer. PostgreSQL is planned as a second provider; it is not wired yet.

## Where it lives

| Piece | Location |
|-------|----------|
| Portable SQL Server Compose | `deploy/docker/docker-compose.sqlserver.yml` |
| Full-stack Compose (includes SQL Server) | `deploy/docker/docker-compose.yml` |
| Local connection | `ConnectionStrings:DefaultConnection` in `src/API/API.Account` |
| Provider switch | `Database:Provider` (`SqlServer` today) |
| EF Core | `src/Infra/Infra.Persistence` (`UseSqlServer`) |

Contract: repositories and `IUnitOfWork` in `Core.Contracts`. Implementation: `Infra.Persistence`.

## Why Docker first

The Microsoft SQL Server Linux image (`mcr.microsoft.com/mssql/server:2022-latest`) is the portable option: same engine on any developer machine with Docker, without installing Windows SQL Server.

## Start the database

From the repo root:

```powershell
docker compose -f deploy/docker/docker-compose.sqlserver.yml up -d
```

Wait until the container is healthy, then run API.Account against `localhost,14333`. In Development, the account API applies EF migrations on startup.

```powershell
dotnet run --project src/API/API.Account
```

Override the SA password by copying `deploy/docker/.env.example` to `deploy/docker/.env` (`.env` is gitignored).

## Connection strings

| Caller | Host |
|--------|------|
| `dotnet run`, EF tools, host machine | `localhost,14333` |
| Other Compose services (e.g. `account`) | `sqlserver,1433` |

Host port **14333** avoids clashing with other local SQL Server instances that already bind `1433`. Override with `MSSQL_PORT` in `deploy/docker/.env` if needed.

Database name: `TraderAnalyzerDb`. User: `sa`. The Compose default password is for **local development only**.

`AppDbContextFactory` uses the same local Docker connection unless `ConnectionStrings__DefaultConnection` is set.

## Provider seam (Postgres later)

`SupportedDatabaseProviders` accepts omitted or `SqlServer` values. Anything else (including `PostgreSql`) throws `NotSupportedException`. Adding Postgres will need architecture-guard approval (Npgsql package, second migrations set, DI branch).

## What not to do

- Do not point production at this SA password or the published `1433` mapping.
- Do not add PostgreSQL packages or a second `DbContext` until that step is approved.
- Do not invert layers: APIs stay composition roots; handlers still talk to contracts, not EF.

## Related

- [Persistence](./persistence.md)
- [Docker](../tech/docker.md)
- [Infra index](./README.md)
