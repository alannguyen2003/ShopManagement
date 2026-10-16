---
name: sdd-ba-spec
description: Generate Draft spec.md files, one per feature branch, from the backlog of an Approved requirements document. Runs automatically right after /sdd-approve requirements; run it by hand for items added later.
disable-model-invocation: true
argument-hint: "[RNNN] [BL-03 BL-05 … | all]"
---

# Generate specs from an approved backlog

Arguments: $ARGUMENTS

Default: every backlog item of the requirements document whose `Spec` column is still `—`. If backlog IDs are given, only those.

## Steps

1. **Determine the requirements document**: an `RNNN` in the arguments; otherwise the only one in `docs/requirements/` with backlog items whose `Spec` is `—`; otherwise list them and ask.

2. **Preconditions** — if any fails, stop and say how to fix it:
   - `requirements.md` is `**Status:** Approved` (otherwise: `/sdd-approve requirements RNNN`).
   - The project is a git repo and `git status --porcelain` is empty.
   - The current branch is `main`; if not, switch to it.
   - The selected items exist and have `—` in the `Spec` column (an item that already has a spec is skipped and reported).

3. **Order** the selected items: dependencies first (`Depends on`), then priority (P1 → P3), then backlog order.

4. **Reserve numbers** on `main`, all in one go:
   - Next `NNN` = 1 + the highest of: the `specs/NNN-*` folders, the `NNN` already in any `Spec` column under `docs/requirements/`, and the `feat/NNN-*` / `fix/NNN-*` branches in `git branch --list`. This keeps numbers unique even while specs live on unmerged branches.
   - Give each item, in order, the next `NNN` and a slug of 2–4 English words, kebab-case.
   - Write `NNN-slug` into each item's `Spec` cell in the summary table. This is the only edit allowed on an Approved requirements document outside `/sdd-amend`, like ticking a task checkbox.
   - Commit: `docs(RNNN): reserve specs NNN–MMM for BL-…`.

5. **For each item, in order**:
   1. `git switch -c feat/NNN-slug main`.
   2. Hand the drafting to the `business-analyst` subagent in mode `draft-spec`, giving it the path of `requirements.md`, the backlog ID, the reserved `NNN-slug` and today's date.
   3. Write `specs/NNN-slug/spec.md` from its draft. Check the header (`Feature`, `Branch`, `Source: RNNN/BL-XX`, `Status: Draft`, `Revision: 1`, `Created`) and that its "Coverage" table lists every ID in the item's `Covers`; a missing one is a Critical finding.
   4. Hand the file to the `spec-reviewer` subagent. Fix "Critical" findings that need no decision from the user; put those that do under "Open questions" of the spec. Never change the requirements document to fit the spec; if the requirements themselves are wrong, note it for the final report (`/sdd-amend requirements`).
   5. Save "Memory suggestions" per the convention in `CLAUDE.md`, commit on the feature branch: `docs(NNN): draft spec from RNNN/BL-XX`.
   6. `git switch main` before the next item.

   If one item fails (agent error, review needs a decision that blocks the whole spec), keep its reservation, finish the others, and report it; it can be retried with `/sdd-ba-spec RNNN BL-XX` after deleting the branch with the user's agreement.

6. **Finish**: switch to the branch of the first spec in the order (the one with no unmet dependency and the highest priority), so the user can continue there right away.

7. **Report** to the user:

   ```
   | Backlog | Spec | Branch | Priority | Open questions |
   |---------|------|--------|----------|----------------|
   ```

   Then: requirements problems found while drafting (if any) → `/sdd-amend requirements RNNN`; and the next step: on the current branch, `/sdd-clarify` (if it has open questions) or read the spec and `/sdd-approve spec`. Recommend taking the features one at a time, in the order of the table: `git switch feat/NNN-slug` when moving to the next.
