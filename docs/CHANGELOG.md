# Changelog

Agents **must** record every change here under **Unreleased** before proposing a commit. The human owner commits; agents never commit.

Format: newest first. After a real commit, move the matching Unreleased item into a dated/git-sha section if you maintain history that way.

## Unreleased

### Agent protocols and guard files
- **Why:** Constrain every agent in this repo: no direct commits, no silent architecture/package/domain/persistence changes, mandatory docs and unit tests, Clean Architecture / Clean Code / ASP.NET practices.
- **What:** Added `AGENTS.md`, `.cursor/rules/*`, `.cursor/skills/{propose-commit,write-project-docs,write-unit-tests}`, and this changelog.
- **Layers:** Docs / agent tooling only (no domain, persistence, or package changes).
- **Tests:** None required (protocol files only; no production behavior).
- **Proposed commit message:** Add agent guard rules so contributors and coding agents cannot commit blindly or change architecture without approval.

## Related

- [Agent protocols](./agent/PROTOCOLS.md)
- [Project overview](./PROJECT_OVERVIEW.md)
