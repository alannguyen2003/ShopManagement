---
name: sdd-status
description: Show SDD progress of the features and fixes in specs/ — spec, plan and tasks status, tasks done, stale documents and the next step. Use when the user asks where they are, what is left, or what to do next.
argument-hint: "[NNN]"
---

# SDD status

Arguments: $ARGUMENTS (empty = all features)

Read-only; modify no files.

1. Run `node .claude/hooks/sdd-status.js $ARGUMENTS`. The script computes status from the `**Status:**` and `**Revision:**` lines of every document, counts tasks, flags stale documents ("⚠ stale") and derives the next step. Show the table to the user as is.

2. If the script cannot run (Node missing): read the `**Status:**` line of `spec.md`, `plan.md`, `tasks.md`, `fix.md` in each `specs/NNN-*` folder yourself, count `- [x]` against the total number of tasks, and state clearly that the SDD hooks are not working either.

3. Add at most 3 lines of commentary when relevant:
   - A "⚠ stale" document → explain the chain that must be re-run (see `/sdd-amend`).
   - The next step is `/sdd-build` and there has been no recent cross-check, or an amend just happened → suggest `/sdd-analyze`.
   - Several unfinished features at once → suggest finishing one first.
