# Docker

Docker is the portable host for local infrastructure. The first database target is the official **SQL Server 2022** Linux image so developers do not need a Windows SQL Server install.

## Where it lives

| Asset | Path |
|-------|------|
| SQL Server only | `deploy/docker/docker-compose.sqlserver.yml` |
| Full stack (APIs + SQL Server + Redis + Kafka) | `deploy/docker/docker-compose.yml` |
| API images | `src/API/*/Dockerfile` |
| Env template | `deploy/docker/.env.example` |

## Why this image

Compose publishes **14333** on the host (container port stays `1433`) so a machine that already has SQL Server on `1433` can still run this stack. From `dotnet run` use `localhost,14333`. Other Compose services still use host name `sqlserver` on port `1433`.

## How to use

```powershell
docker compose -f deploy/docker/docker-compose.sqlserver.yml up -d
docker compose -f deploy/docker/docker-compose.sqlserver.yml ps
```

Stop:

```powershell
docker compose -f deploy/docker/docker-compose.sqlserver.yml down
```

Data is stored in the `sqlserver-data` volume. `down -v` deletes it.

If you need a different host port, set `MSSQL_PORT` in `deploy/docker/.env` and update `ConnectionStrings:DefaultConnection` to match.

## What not to do

- Do not commit `.env` files or production SA passwords.
- Do not treat Compose as Kubernetes or production topology; it is local/dev portability.
- PostgreSQL Compose is not added yet.

## Related

- [SQL Server](../infra/sql-server.md)
- [Persistence](../infra/persistence.md)
