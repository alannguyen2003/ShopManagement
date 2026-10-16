---
name: sdd-architecture
description: Change docs/architecture.md — add or remove an allowed dependency, change a layer rule, folder convention or project — with impact analysis, a version bump and a changelog row. The only way the architecture and its dependency allowlist change after /sdd-scaffold.
disable-model-invocation: true
argument-hint: "<change and reason, e.g. add FluentValidation to Application for command validation>"
---

# Change the architecture

Requested change: $ARGUMENTS

Typing this command is the user's permission to edit `docs/architecture.md` for this run only; the hooks block every other edit of that file. This skill changes the document only. Installing a newly allowed package happens later, in a task during `/sdd-build`.

## Steps

1. Read `docs/architecture.md`, `docs/constitution.md`, `CLAUDE.md`. If the change is empty: ask for it. If `docs/architecture.md` is missing:
   - No `src/backend` yet: stop and suggest `/sdd-scaffold`.
   - An existing codebase (the kit was installed into a project that already has code): offer to write `docs/architecture.md` from [the scaffold template](../sdd-scaffold/architecture-template.md) describing the structure that exists (layers, folders, every package currently referenced as an allowed row), `**Status:** Active`, Version 1.0.0. Ask the user before writing; then continue at step 6.

2. **Analyze** the change:
   - **New dependency**: name, ecosystem (`nuget`/`npm`), layer or `runtime`/`dev`, purpose. Check, with read-only commands only (`npm view <pkg> version license repository`, `dotnet package search <pkg> --exact-match`): it exists, its license (flag anything that is not permissive, or is commercial), its last release date, and whether the stack already has something that does the same job (the "Forbidden" section rules out a second way of doing the same thing). Recommend the narrowest scope (one layer, `dev` rather than `runtime`).
   - **Layer rule, project or convention change**: which existing code and which plans in `specs/` it affects.
   - Does it contradict the constitution? If so, it cannot be applied here: say which principle and suggest `/sdd-constitution`.

3. **Confirm**: show a short table (change · scope · license/risk · affected code or plans · version bump) and ask the user whether to apply it. Write nothing until they agree.

4. **Apply**:
   - Edit the section as if it had always been written that way; for a dependency, add the row in its ecosystem's block of the table.
   - Bump `**Version:**`: new dependency or convention = minor; removed dependency, changed layer rule or project = major; wording = patch. Update `**Last amended:**` (`date +%F`).
   - Add a Changelog row.

5. **Impact on features**: list the `plan.md` files in `specs/` (also on unmerged branches: `git branch --list "feat/*"` and `git show <branch>:specs/<NNN-slug>/plan.md`) whose `**Architecture:**` is older than the new version. For the feature on the current branch, if it now needs a task to use the change (install the package, move code): suggest `/sdd-amend plan` or `/sdd-amend tasks`. Suggest `/sdd-analyze` for each in-progress feature.

6. **Commit** on the current branch: `docs: architecture v<X.Y.Z> — <summary>`. If the current branch is a feature branch, remind the user that other branches see the change only after this one is merged (or after they merge it).

7. **Report**: the change, the new version, affected features and the next command.
