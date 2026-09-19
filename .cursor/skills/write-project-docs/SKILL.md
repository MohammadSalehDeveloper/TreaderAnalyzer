---
name: write-project-docs
description: Create and update Trader Analyzer docs for features, layers, models, infrastructure, technologies, and changelog commit explanations. Use when adding features, stories, endpoints, domain types, infra, packages, or finishing a change that needs documentation.
---

# Write project docs

Docs are mandatory for new features, layers, models, infra, and technologies, and for **every** change that would be a commit.

## Changelog (every change)

Prepend under `## Unreleased` in `docs/CHANGELOG.md`:

```markdown
### <short title>
- **Why:** …
- **What:** …
- **Layers:** Application / API / Web (list only what changed)
- **Tests:** `tests/Tests.Unit/...`
- **Proposed commit message:** …
```

## Feature / story

Path: `docs/api/{service}/features/{kebab-name}.md`  
Mirror: `docs/api/account/features/login.md`

Include: story, endpoint, command/query, auth, request example, responses, errors, application flow, config, security notes. Link from `docs/api/{service}/README.md` and the controller doc.

## Layer / model / infra / tech

| Kind | Path |
|------|------|
| Layer | `docs/architecture/{layer}.md` |
| Model | `docs/domain/{name}.md` |
| Infra | `docs/infra/{name}.md` |
| Technology | `docs/tech/{name}.md` |

Each page: purpose, where it lives in the solution, dependencies, how to use, what not to do. Link from `docs/PROJECT_OVERVIEW.md` if it changes the public architecture picture.

Do not copy secrets from `appsettings.json`.
