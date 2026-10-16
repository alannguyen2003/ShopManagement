---
name: sdd-tasks
description: Break an approved plan into small, ordered, test-first tasks (tasks.md), or update tasks.md after the plan changed.
disable-model-invocation: true
argument-hint: "[NNN]"
---

# Create the task list

Arguments: $ARGUMENTS

## Steps

1. Determine the current feature per "SDD conventions" in `CLAUDE.md`. If `plan.md` is not `Approved`: stop and remind the user to run `/sdd-approve plan`.

2. Read `spec.md`, `plan.md`, and if present: `data-model.md`, `contracts/`, `research.md`.

3. **Update mode**: if `tasks.md` already exists (typically after `/sdd-amend`):
   - `tasks.md` is `Approved`: that would be editing an approved document; stop and suggest `/sdd-amend tasks`.
   - It is `Draft`: read the plan's "Revision history" to learn what changed since the old `Plan revision`. Keep unaffected `[x]` tasks as they are; reset affected ones to `[ ]` with the note `(redo — plan rev N)`; number new tasks after the highest existing number, never renumber. If the file still has an `**Approved:**` line (the current version was approved before), bump `Revision` by 1 and delete that line; update `Plan revision`; add a row to "Revision history". Then continue from step 5.

4. **Write `tasks.md`** following [template.md](template.md):
   - Header: `Revision: 1`, `Plan revision` (= the plan's current `Revision`).
   - Group into phases: **Setup** → **Foundation** (what every story needs; blocks later phases) → **one phase per user story**, in priority order P1, P2, P3 → **Polish**.
   - The project skeleton already exists from `/sdd-scaffold`: never add a task that creates a new project, solution or app. Setup holds only what this feature needs on top of it: installing a package the plan's Architecture check marks `allowed` (`dotnet add <project> package …`, `npm --prefix src/frontend install …`), configuration, a migration.
   - Every file path in a task follows `docs/architecture.md` (layer, feature folder, naming).
   - If "Project commands" in `CLAUDE.md` is still empty: the Setup phase must include a task that fills those commands in and adds the permissions to `.claude/settings.json`.
   - Each user story phase is a vertical slice: once done, it runs and can be tested on its own.
   - Task format: `- [ ] T001 [P] [US1] <verb + concrete work> — <path/to/file>`
     - `[P]` only when the task can run in parallel: different files and no dependency on unfinished tasks.
     - `[US1]` only for tasks inside a user story phase.
   - **TDD**: in each phase, the test-writing task comes BEFORE the matching implementation task.
   - Each task is small enough to finish in one go and touches few files. Split anything bigger.
   - Each phase ends with a **Checkpoint**: a command or action that proves the phase is done.
   - Number tasks continuously across the file: T001, T002…
   - `**Status:** Draft`.

5. **Check coverage**: fill in the "Traceability" table at the end: every FR, NFR and acceptance scenario → task IDs. Add tasks for any gaps. Remove, or justify, any task that traces back to no requirement.

6. Hand the tasks to the `spec-reviewer` subagent to check them against the spec and plan. Fix "Critical" findings. Save the "Memory suggestions" section per the convention in `CLAUDE.md`.

7. Commit: `docs(NNN): draft tasks` (or `docs(NNN): update tasks rev N`).

8. **Report**: total tasks, number of phases, which phase completes the MVP, high-risk tasks. Next step: review (optionally run `/sdd-analyze` for a cross-check), then `/sdd-approve tasks`.
