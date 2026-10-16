---
name: sdd-build
description: Implement the tasks of an approved tasks.md (or fix.md) with TDD, one task at a time through the software-engineer subagent, checking the result, ticking the checkbox and committing after each task.
disable-model-invocation: true
argument-hint: "[T005 | T005-T010 | phase 3 | all]"
---

# Implement tasks

Requested scope: $ARGUMENTS
With no arguments, work on the first phase that still has unfinished tasks.

## Before starting

1. Determine the current feature per "SDD conventions" in `CLAUDE.md`. Run `node .claude/hooks/sdd-status.js <NNN>`:
   - Regular feature: `tasks.md` must be `Approved`, spec and plan must be `Approved`, no "⚠ stale" flag.
   - Fix: `fix.md` must be `Approved`; the task list is the "Tasks" section of `fix.md`.
   - Otherwise: stop and name the missing step. The code-gate hook would block every code edit in this state anyway.
2. Make sure you are on branch `feat/NNN-slug` (or `fix/NNN-slug`).
3. Read `docs/constitution.md`, `docs/architecture.md`, `spec.md`, `plan.md`, `tasks.md` (or `fix.md`), and if present: `data-model.md`, `contracts/`. If `docs/architecture.md` is missing or still `Scaffolding`: stop and suggest `/sdd-scaffold`.
4. Pick the tasks in scope, in file order, skipping those already `[x]`. If a task depends on an unfinished task outside the scope: tell the user before starting.
5. Take the test/lint commands from "Project commands" in `CLAUDE.md`. If still empty and no Setup task fills them in: ask the user once, then fill them into `CLAUDE.md` and add matching permissions to `.claude/settings.json`.

## For each task

The main session orchestrates; the `software-engineer` subagent writes the code. Tasks run one at a time, in file order, even when marked `[P]` (parallel agents in one working tree would mix their changes and commits).

a. **Delegate**: hand the task to the `software-engineer` subagent with the feature folder path, the task ID and its line from `tasks.md`, the related acceptance scenarios and FRs from the Traceability table, and for an implementation task the ID of its test task. It follows `docs/architecture.md` and TDD, and reports `DONE` or `BLOCKED`.

b. **BLOCKED**: do not retry with another approach and do not write the code yourself. Relay the blocker and the command it proposes (`/sdd-architecture …`, `/sdd-amend …`) to the user and stop.

c. **Check the report** (do not take it on trust):
   - Test task: run the new test via the `test-runner` subagent and confirm it **FAILS for the right reason** (behavior missing), not because of a syntax or import error.
   - Implementation task: run the related tests via `test-runner`; they must pass. If they fail: send the failure back to `software-engineer`, at most 3 rounds in total. Still failing: stop and report the suspected cause to the user.
   - `git status --porcelain`: every changed file belongs to the task or the plan's "File structure". Anything else: ask `software-engineer` to revert it, or stop and ask the user.

d. **Tick and commit**: change `- [ ]` to `- [x]` for the task in `tasks.md` (or `fix.md`), then commit code and document together with `feat(NNN): T00X <description>`, `test(NNN): T00X …` for test-only tasks, or `fix(NNN): T00X …` for tasks of a fix. Save the agent's "Memory suggestions" per the convention in `CLAUDE.md`.

## When a document is wrong or incomplete

If you (or `software-engineer`) discover that spec/plan/tasks are wrong, incomplete or contradict the real codebase: **STOP**. Describe the problem, propose a concrete `/sdd-amend <spec|plan|tasks> <change>` command, and wait for the user's decision. Never silently deviate from the documents, even when another way is "obviously better". Never edit the content of approved documents yourself, except for checkboxes.

A task needs a package that is not in the "Allowed dependencies" table, a new project, or code in a place `docs/architecture.md` does not provide: **STOP** and propose `/sdd-architecture "<change and reason>"`. The hooks block the install or the manifest edit anyway; never look for another way around them.

## At each Checkpoint (end of a phase)

1. Run the full test suite and lint via `test-runner`.
2. Perform the verification step written in the Checkpoint.
3. Hand the phase's diff (from the phase's first commit to HEAD) to the `code-reviewer` subagent. Have `software-engineer` fix every "Must fix" item, check with `test-runner`, and commit `fix(NNN): review phase N`. Save the "Memory suggestions" section per the convention in `CLAUDE.md`.
4. Stop and report to the user before moving to the next phase, unless the scope is `all`. Suggest `/clear` between phases if the conversation is long; progress lives in the checkboxes, so a new session can continue.

## Final report

- Tasks done (ID + one line), tasks remaining.
- Test results.
- Issues the user must decide on.
- Next step: `/sdd-build` for the next phase, or `/sdd-verify` if everything is done.
