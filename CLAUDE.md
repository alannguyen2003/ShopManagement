# Shop Management

An application for a shop Admin to manage product information and stock.

This project follows a custom **Spec-Driven Development (SDD)** workflow, using the `/sdd-*` skills in `.claude/skills/`. Human-facing usage guide: `guideline.md`.

## SDD rules (mandatory)

- Do not write or modify product code until the current feature's documents are approved: a regular feature needs `spec.md`, `plan.md` and `tasks.md` all `Approved` and not stale; a fix needs `fix.md` `Approved`. Small out-of-process fixes only when the user explicitly asks, and preferably through `/sdd-fix`.
- Precedence when documents conflict: `docs/constitution.md` > `docs/architecture.md` > `requirements.md` > `spec.md` > `plan.md` > `tasks.md` > code. If code deviates from the documents, fix the code, or stop and propose `/sdd-amend`. Never silently deviate from the documents.
- Code goes where `docs/architecture.md` says (layers, feature folders, naming) and uses only the packages in its "Allowed dependencies" table, in the layer listed there. Need anything else (a package, a project, a new place for code): stop and propose `/sdd-architecture "<change and reason>"`. Only the user runs it; Claude never edits `docs/architecture.md` outside `/sdd-scaffold` and `/sdd-architecture`.
- No action with effects outside this project without the user's explicit permission: no push, publish, deploy or infrastructure change (the user runs those), and no network call to a non-local host, remote shell, mail, GitHub write, cloud CLI, or MCP/connector write unless the user asked for it. The `sdd-guard-external` hook denies the first group and asks the user to confirm the second.
- Only a human approves, through the `/sdd-approve` command. Claude never changes a `**Status:**` line to `Approved` itself.
- An `Approved` document may only change through `/sdd-amend` (except task checkboxes, `/sdd-verify` moving `Approved` → `Done`, and `/sdd-ba-spec` filling the `Spec` column of a backlog).
- New requirements or ambiguities discovered mid-way: record them under "Open questions" in the spec and ask the user. Never expand scope on your own.
- The hooks in `.claude/hooks/` enforce these rules. When a hook blocks you, tell the user why; never look for a workaround (Bash, renaming files, editing hooks or settings).

## Workflow

```
/sdd-init                    (once: git, project commands, permissions)
/sdd-constitution            (once per project, updated when needed)
/sdd-scaffold                (once: src/backend .NET Clean Architecture + src/frontend Next.js or React,
                              writes docs/architecture.md with the dependency allowlist)

Business analysis (large or raw requirements, optional):
/sdd-ba <requirements text | file paths> → docs/requirements/RNNN-slug/requirements.md (Draft), on main
/sdd-clarify RNNN            → Q&A, requirements updated
/sdd-approve requirements RNNN → then generates one Draft spec per backlog item (/sdd-ba-spec),
                               each on its own branch feat/NNN-slug; continue each feature from /sdd-clarify

Feature:
/sdd-spec <feature description>  → spec.md (Draft), branch feat/NNN-slug
/sdd-clarify                 → Q&A, spec updated
/sdd-approve spec
/sdd-plan [technology direction] → plan.md (+ research.md, data-model.md, contracts/)
/sdd-approve plan
/sdd-tasks                   → tasks.md
/sdd-approve tasks
/sdd-build [T005 | phase 2 | all]  → software-engineer agent writes TDD code, one commit per task
/sdd-verify                  → verification.md, ready to merge or not
/sdd-pr                      → pr.md, push and PR instructions

Small bug fix:  /sdd-fix <bug> → /sdd-approve fix → /sdd-build → /sdd-verify → /sdd-pr
Change an approved document: /sdd-amend <spec|plan|tasks|requirements> <change> → re-approve the chain
Change the architecture or allowed dependencies: /sdd-architecture <change and reason>
Backlog items added later: /sdd-ba-spec RNNN [BL-XX …]
Any time: /sdd-status, /sdd-analyze [NNN | RNNN] (read-only cross-check)
Periodically: /sdd-retro (every 2–3 features)
```

## SDD conventions

- Folder: `specs/NNN-slug/`. `NNN` is a 3-digit increasing number shared by features and fixes (001, 002…); `slug` is 2–4 English words, kebab-case.
- Requirements folder: `docs/requirements/RNNN-slug/` with `input.md` (raw input, verbatim) and `requirements.md` (analysis + backlog). `RNNN` is its own 3-digit sequence (R001, R002…). Requirements are committed on `main`. Spec numbers reserved in a backlog's `Spec` column count as taken.
- Branch: `feat/NNN-slug` for a feature, `fix/NNN-slug` for a fix. Main branch: `main`.
- Document headers:
  - `**Status:**` — `spec.md`, `plan.md`, `requirements.md`: `Draft` → `Approved`. `tasks.md`, `fix.md`: `Draft` → `Approved` → `Done`.
  - `**Revision:**` — `spec.md`, `plan.md`, `tasks.md`, `requirements.md`; starts at 1 and increases only when the content of an approved version changes.
  - `plan.md` records `**Spec revision:**`, `**Constitution:**` and `**Architecture:**`; `tasks.md` records `**Plan revision:**`. A mismatch with the current version means the document is stale and must be updated and re-approved.
  - `docs/architecture.md`: `**Status:**` `Scaffolding` → `Active`; `**Version:**` semver, bumped by `/sdd-architecture`.
- Determining the "current feature" a skill works on:
  1. If the arguments contain a number `NNN` or a path `specs/...`, use that.
  2. Otherwise, use the current git branch name if it matches `feat/NNN-slug` or `fix/NNN-slug`.
  3. Otherwise: list the unfinished features in `specs/` and ask the user.
- Determining the requirements document: an `RNNN` in the arguments; otherwise the only unfinished one in `docs/requirements/`; otherwise list them and ask.
- `spec.md` records `**Source:** RNNN/BL-XX` when generated from a backlog (`—` otherwise).
- Machine-computed status: `node .claude/hooks/sdd-status.js [NNN | RNNN]`.
- IDs: `FR-001` (functional), `NFR-001` (non-functional), `SC-001` (success criterion). Tasks: `T001`. User stories: `US1`. In requirements: `BG-01` (business goal), `BR-001` (business rule), `QE-01` (quality expectation), `PR-01` (business process), `BL-01` (backlog item).
- Commit messages:
  - `docs(NNN): ...` for a feature's documents; `docs(RNNN): ...` for a requirements document; `docs: ...` for shared documents (constitution, architecture, retro).
  - `chore: scaffold ...` for `/sdd-scaffold`.
  - `feat(NNN): T00X ...`, `test(NNN): T00X ...` for feature tasks; `fix(NNN): T00X ...` for fix tasks.
  - `fix(NNN): review phase N` / `fix(NNN): verify — ...` for review-driven fixes.
  - `chore: ...` for kit and tooling configuration.
- Agent memory: when `business-analyst`, `spec-reviewer`, `code-reviewer`, `security-reviewer` or `software-engineer` returns a "Memory suggestions" section other than "None", the main session appends the new lines (deduplicated) to `.claude/agent-memory/<agent-name>/MEMORY.md` and includes it in that step's commit.
- Implementation: during `/sdd-build`, code is written by the `software-engineer` subagent, one task at a time; the main session checks its result with `test-runner`, ticks the checkbox and commits.

@docs/constitution.md
@docs/architecture.md

## Project commands

<!-- Filled in by /sdd-scaffold (or /sdd-init for an existing codebase). Claude uses them to verify its own work. -->
- Install: `<not yet — filled in by /sdd-scaffold>`
- Run tests: `<not yet — filled in by /sdd-scaffold>`
- Run a single test: `<not yet — filled in by /sdd-scaffold>`
- Lint / typecheck: `<not yet — filled in by /sdd-scaffold>`
- Run dev: `<not yet — filled in by /sdd-scaffold>`
- Run e2e: `<not yet — filled in by /sdd-scaffold>` (leave empty if none)
