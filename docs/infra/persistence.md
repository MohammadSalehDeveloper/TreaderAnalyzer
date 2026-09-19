# Persistence

`Infra.Persistence` implements `Core.Contracts` with EF Core 9. The current database provider is **SQL Server**. PostgreSQL is planned and is not implemented.

## Where it lives

- Project: `src/Infra/Infra.Persistence`
- Registration: `AddPersistence(connectionString, provider)` from `API.Account`
- Schema: Fluent API configurations + EF migrations under `Migrations/`

## How to use

API composition roots call:

```csharp
builder.Services.AddPersistence(
    configuration.GetConnectionString("DefaultConnection")!,
    configuration["Database:Provider"]);
```

- Empty connection strings throw `ArgumentException`.
- `Database:Provider` must be omitted or `SqlServer`. Other values throw `NotSupportedException`.
- In Development, `API.Account` runs `Database.MigrateAsync()` so a fresh Docker SQL Server gets the account schema.

Handlers continue to use `IUserRepository`, `IUnitOfWork`, and other contracts — never `AppDbContext`.

## What not to do

- Do not expose EF types beyond the composition root.
- Do not add Npgsql, a second context, or split migrations until dual-provider work is approved.
- Do not put connection secrets in source for production; override with environment variables.

## Related

- [SQL Server](./sql-server.md)
- [Docker](../tech/docker.md)
