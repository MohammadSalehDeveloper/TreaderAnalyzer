# Infrastructure docs

Add `docs/infra/{name}.md` when adding or changing persistence, cache, messaging, feeds, email, or token/hash adapters. Document the contract in `Core.Contracts` and the implementation project.

Infra and package changes require architecture-guard approval.

| Page | Topic |
|------|--------|
| [SQL Server](./sql-server.md) | Docker SQL Server 2022 for local persistence |
| [Persistence](./persistence.md) | EF Core, `AddPersistence`, current SQL Server provider |
