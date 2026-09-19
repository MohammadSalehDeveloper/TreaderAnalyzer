# Agent protocols — Trader Analyzer

These rules apply to **every** agent working in this repository. They override default “commit on request” behavior.

Read `.cursor/rules/` and the matching `.cursor/skills/` before implementing, documenting, testing, or proposing a commit.

## Hard rules

1. **Do not commit.** Never run `git commit`, `git add` (for a commit), `git push`, amend, rebase, or skip hooks. When work is ready, give the user a **commit message only**.
2. **Do not change architecture without approval.** Do not alter layers, solution structure, infrastructure, domain model, persistence, or NuGet/project packages unless you first **explain why** and **ask**. Wait for an explicit yes.
3. **Follow the engineering stack.** Clean Architecture, Clean Code, established design patterns, and ASP.NET Core / .NET 9 practices already used in this repo.
4. **Document every change.** Each unit of work that would become a commit must be explained in `docs/CHANGELOG.md` (and feature/layer docs as required).
5. **Document new things.** New features, layers, models, infrastructure, and technologies get docs under `docs/` before the work is treated as done.
6. **Test new things.** New functionality, features, stories, and infrastructure get unit tests in `tests/Tests.Unit` (integration tests in `tests/Tests.Integration` when the change crosses persistence or HTTP).

## Layers (do not invert)

```
API / Web  →  Core.Application  →  Core.Domain
                              ↘  Core.Contracts  ←  Infra.*
```

- **Core.Domain** — entities, value objects, enums, domain exceptions. No framework, EF, or ASP.NET.
- **Core.Contracts** — DTOs and interfaces only.
- **Core.Application** — CQRS (MediatR), FluentValidation, AutoMapper. Orchestrates; does not implement persistence.
- **Infra.*** — implements contracts (EF Core, hashing, tokens, email, cache, messaging).
- **API.*** — composition root: controllers, middleware, DI, auth. Thin HTTP adapters.
- **Web.*** — Blazor UI. Calls APIs; does not contain business rules.

## Definition of done (every task)

- [ ] Implementation stays inside approved layers and existing patterns
- [ ] Architecture / package / domain / persistence / infra change was either unused or **explicitly approved**
- [ ] Docs updated (`docs/CHANGELOG.md` + feature/layer/model/infra/tech docs)
- [ ] Unit tests added or updated and passing
- [ ] Commit message **proposed** to the user — **not** committed
