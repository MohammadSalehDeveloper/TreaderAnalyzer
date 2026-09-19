# Architecture docs

Write one page per layer when that layer’s role, dependencies, or public types change.

| Layer | Project | Rule |
|-------|---------|------|
| Domain | `src/Core/Core.Domain` | `.cursor/rules/domain-layer.mdc` |
| Contracts | `src/Core/Core.Contracts` | `.cursor/rules/contracts-layer.mdc` |
| Application | `src/Core/Core.Application` | `.cursor/rules/application-layer.mdc` |
| Infrastructure | `src/Infra/*` | `.cursor/rules/infra-persistence.mdc` |
| API | `src/API/*` | `.cursor/rules/api-layer.mdc` |
| Web | `src/Web/*` | `.cursor/rules/web-layer.mdc` |

See [PROJECT_OVERVIEW.md](../PROJECT_OVERVIEW.md) for the current map. Persistence currently targets Docker SQL Server ([infra/sql-server.md](../infra/sql-server.md)). Agents must not change these boundaries without approval ([PROTOCOLS.md](../agent/PROTOCOLS.md)).
