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

From the repo root, copy the env template and set a local SA password. `.env` is gitignored.

```powershell
copy deploy\docker\.env.example deploy\docker\.env
docker compose -f deploy/docker/docker-compose.sqlserver.yml up -d
```

Wait until the container is healthy. `appsettings` has the server, database, and user, and no password. Pass the same SA password into the account API, then run it. In Development it applies EF migrations on startup.

```powershell
$env:Database__Password = "<the MSSQL_SA_PASSWORD from deploy/docker/.env>"
dotnet run --project src/API/API.Account
```

`MSSQL_SA_PASSWORD` is accepted as well. EF tools use `ConnectionStrings__DefaultConnection` or those same password variables.

## Connection strings

| Caller | Host |
|--------|------|
| `dotnet run`, EF tools, host machine | `localhost,14333` |
| Other Compose services (e.g. `account`) | `sqlserver,1433` |

Host port **14333** avoids clashing with other local SQL Server instances that already bind `1433`. Override with `MSSQL_PORT` in `deploy/docker/.env` if needed.

Database name: `TraderAnalyzerDb`. User: `sa`. The password is only in `deploy/docker/.env` or the process environment, never in `appsettings`.

`AppDbContextFactory` uses the password-less local Docker host unless `ConnectionStrings__DefaultConnection` is set, and still requires `Database__Password` or `MSSQL_SA_PASSWORD` when that string has no password.

## Provider seam (Postgres later)

`SupportedDatabaseProviders` accepts omitted or `SqlServer` values. Anything else (including `PostgreSql`) throws `NotSupportedException`. Adding Postgres will need architecture-guard approval (Npgsql package, second migrations set, DI branch).

## What not to do

- Do not commit the SA password in `appsettings`, Compose files, or source.
- Do not add PostgreSQL packages or a second `DbContext` until that step is approved.
- Do not invert layers: APIs stay composition roots; handlers still talk to contracts, not EF.

## Related

- [Persistence](./persistence.md)
- [Docker](../tech/docker.md)
- [Infra index](./README.md)
