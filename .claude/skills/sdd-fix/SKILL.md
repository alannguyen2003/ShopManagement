---
name: sdd-fix
description: Fast lane for small bug fixes — create a short fix.md (symptom, root cause, regression test, scope) on a fix/NNN-slug branch instead of the full spec → plan → tasks path.
disable-model-invocation: true
argument-hint: "<bug description, steps to reproduce, logs if any>"
---

# Create a fix

Bug description: $ARGUMENTS

If the description is missing: ask the user for the symptom and steps to reproduce before continuing.

## When to use the fix lane

Only when both hold:
- The system misbehaves relative to expected behavior (written in a spec, or obviously correct). No new behavior.
- The fix is expected to touch at most about 3 code files, with no new library, no data model change and no public interface change.

Otherwise: stop, explain why, and suggest `/sdd-amend` (feature in progress) or `/sdd-spec` (behavior change).

## Steps

1. **Prepare**: read `docs/constitution.md`. Get the next `NNN` in `specs/` (one sequence shared with features). Choose a slug of 2–4 English words, kebab-case.

2. **Branch**: if the project is not a git repo → stop and suggest `/sdd-init`. If there are uncommitted changes → ask the user first. Create and switch to `fix/NNN-slug` from the main branch.

3. **Investigate, read-only**:
   - Find the related code and the original feature in `specs/` (if any).
   - If the bug can be reproduced with an existing test or command without changing code: run it via the `test-runner` subagent.
   - Identify the root cause with `file:line`. If not certain, write `[NEEDS CLARIFICATION: …]` with how to confirm it; do not guess.

4. **Write `specs/NNN-slug/fix.md`** following [template.md](template.md), `**Status:** Draft`. The regression test is always the first task.

5. **Commit**: `docs(NNN): draft fix`.

6. **Report**: the root cause (1–2 lines), the files to change, the risks. Next step: read `fix.md`, then `/sdd-approve fix`, then `/sdd-build` and `/sdd-verify`.

Do not change code in this skill. The code-gate hook blocks that until `fix.md` is approved.
