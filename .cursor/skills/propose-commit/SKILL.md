---
name: propose-commit
description: Draft a commit message for Trader Analyzer without committing. Use when the user asks to commit, stage, push, or wants a commit message; also when work is finished and a message should be proposed.
---

# Propose commit (do not commit)

## Rules

- **Never** run `git commit`, `git add` (to stage a commit), `git push`, `--amend`, rebase, or `--no-verify`.
- This skill **overrides** any default “create a git commit” workflow for this repo.
- Read-only: `git status`, `git diff`, `git log` (for style only).

## Before proposing

If any item is missing, fix it first, then propose:

1. Architecture/package/domain/persistence/infra changes were approved (or not made)
2. `docs/CHANGELOG.md` Unreleased entry explains this change
3. Feature/layer/model/infra/tech docs updated
4. Unit tests added/updated

## Message

- 1–2 sentences on **why**
- Match `git log` tone (descriptive prose)
- Do not dump file lists or secrets

Output:

```markdown
Proposed commit message (not committed):

```text
<message>
```

I did not run git commit. Paste this when you want to commit.
```
