---
name: sdd-amend
description: Change an approved spec, plan or tasks document of a feature, or an approved requirements document, bumping its Revision and sending affected documents back to Draft for re-approval. Use for mid-stream change requests, when /sdd-build finds a document is wrong, or when /sdd-verify finds missing tasks.
disable-model-invocation: true
argument-hint: "<spec|plan|tasks> <description of the change> [NNN] | requirements <change> [RNNN]"
---

# Amend an approved document

Arguments: $ARGUMENTS

Principle: an `Approved` document is never edited silently. Every change goes through this skill, gets a Revision and a history entry, and is approved again by the user with `/sdd-approve`.

## Steps

1. **Identify** the document level (`spec`, `plan`, `tasks`) and the change from the arguments; ask if missing. Determine the feature per "SDD conventions" in `CLAUDE.md`. For a fix (`fix.md`), only `fix.md` changes: edit it, set it back to `Draft`, and follow steps 4–6.
   - If the document at that level is not `Approved`: this skill is not needed; edit it directly (spec Draft → `/sdd-clarify`; plan/tasks Draft → `/sdd-plan` or `/sdd-tasks`; requirements Draft → `/sdd-clarify RNNN`).
   - For `requirements`, see "Amending requirements" below instead of steps 2–7.

2. **Impact analysis** — read spec, plan, tasks, data-model, contracts:
   - Which items change (FR/NFR/SC/US/scenario; components, files, contracts in the plan; task IDs).
   - Which `[x]` tasks have code that must be redone.
   - Whether the change still belongs to this feature. If it is really a new feature: suggest `/sdd-spec` instead of amending.

3. **Confirm**: present a short impact table and ask the user whether to apply it. Write nothing until they agree.

4. **Apply at the chosen level**:
   - Edit the content as if it had always been written that way (no struck-through passages).
   - Bump `**Revision:**` by 1. Set `**Status:**` to `Draft` and delete the `**Approved:**` line.
   - Add a row to the "Revision history" table (rev, date from `date +%F`, change, affected items).
   - At `tasks` level: reset affected `[x]` tasks to `[ ]` with the note `(redo — amend rev N)`; number new tasks after the highest existing number, never renumber; update the Traceability table.

5. **Cascade to lower-level documents**: change only their `**Status:**` line to `Draft`, keeping the `**Approved:**` line and the content. `/sdd-plan` and `/sdd-tasks` use that leftover `**Approved:**` line to know they must bump the Revision when they update:
   - Amending `spec` → `plan.md` and `tasks.md` (if present).
   - Amending `plan` → `tasks.md` (if present).
   The code-gate hook blocks code edits until the whole chain is approved again.

6. **Commit**: `docs(NNN): amend <level> rev <N> — <summary>`.

7. **Report** and show the re-approval path:
   - spec → review, then `/sdd-approve spec` → `/sdd-plan` (update) → `/sdd-approve plan` → `/sdd-tasks` (update) → `/sdd-approve tasks` → `/sdd-build`
   - plan → `/sdd-approve plan` → `/sdd-tasks` (update) → `/sdd-approve tasks` → `/sdd-build`
   - tasks → `/sdd-approve tasks` → `/sdd-build`

## Amending requirements

Work on `main` (switch to it; ask first if there are uncommitted changes).

1. **Impact analysis**: which BG/BR/QE/BL change, are added or removed; and which existing specs are affected. A spec is affected when its backlog item (the `Spec` column, and the `**Source:**` line in that spec) covers a changed ID. Read those specs from their branches with `git show feat/NNN-slug:specs/NNN-slug/spec.md` without switching.
2. **Confirm**: show a table `ID | change | affected specs (NNN, status)` and ask the user. Write nothing until they agree.
3. **Apply**: edit `requirements.md` as if it had always been written that way; bump `**Revision:**`; set `**Status:**` to `Draft` and delete the `**Approved:**` line; add a "Revision history" row. New backlog items get `—` in `Spec`; never renumber existing IDs. A removed item that already has a spec stays in the table with ` (withdrawn)` after its name, and the report tells the user to decide what to do with that spec's branch.
4. **Commit**: `docs(RNNN): amend requirements rev <N> — <summary>`.
5. **Report**: the re-approval path `/sdd-approve requirements RNNN` (which generates specs only for items with `—`), and for each affected existing spec: `git switch feat/NNN-slug`, then `/sdd-clarify` if the spec is still Draft, or `/sdd-amend spec <change>` if it is Approved. Specs are never changed automatically from here.
