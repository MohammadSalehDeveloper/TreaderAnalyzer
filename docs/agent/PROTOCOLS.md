# Agent protocols

Human-readable copy of the rules in `AGENTS.md` and `.cursor/rules/`. Coding agents must follow these on every task.

## 1. Do not commit — propose a message

Agents must not run `git commit`, stage files for a commit, push, amend, or skip hooks. When work is ready, they output a commit message only.

Skill: `.cursor/skills/propose-commit`

## 2. Do not change architecture without asking

Stop and explain before changing:

- Layers, solution structure, project references
- Infrastructure (cache, messaging, feeds, email, tokens)
- Domain model and invariants
- Persistence (EF, schema, repositories)
- NuGet or project packages

Wait for an explicit yes.

## 3. Engineering bar

Clean Architecture (dependency rule as in `docs/PROJECT_OVERVIEW.md`), Clean Code, existing design patterns (CQRS/MediatR, repository/UoW, factory methods), ASP.NET Core / .NET 9.

## 4. Explain each commit in docs

Every change gets an **Unreleased** entry in [CHANGELOG.md](../CHANGELOG.md) (why, what, layers, tests, proposed message).

## 5. Document new work

| Kind | Where |
|------|--------|
| Feature / story | `docs/api/{service}/features/` |
| Controller | `docs/api/{service}/controllers/` |
| Layer | `docs/architecture/` |
| Model | `docs/domain/` |
| Infra | `docs/infra/` |
| Technology | `docs/tech/` |

Skill: `.cursor/skills/write-project-docs`

## 6. Unit tests for new functionality

New features, stories, and infra require tests in `tests/Tests.Unit` (xUnit, FluentAssertions, Moq). Integration tests in `tests/Tests.Integration` when the database or full host is involved.

Skill: `.cursor/skills/write-unit-tests`

## Definition of done

Implementation in approved layers → changelog + docs → unit tests passing → **commit message proposed, not committed**.
