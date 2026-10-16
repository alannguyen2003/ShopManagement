# Tasks: <Feature name>

**Feature:** NNN-slug
**Plan:** [plan.md](plan.md)
**Status:** Draft
**Revision:** 1
**Plan revision:** <current Revision of plan.md>
**Created:** <YYYY-MM-DD>

Legend: `[P]` can run in parallel · `[USn]` belongs to user story n · tests always come before code.

## Phase 1 — Setup

- [ ] T001 <setup this feature needs on top of the scaffold: an allowed package, configuration, a migration> — <file>
- [ ] T002 Fill in the test/lint/dev commands under "Project commands" in CLAUDE.md and add matching permissions to .claude/settings.json — CLAUDE.md (only if that section is still empty)
- [ ] T003 [P] <task> — <file>

**Checkpoint:** <command/action proving the phase is done>

## Phase 2 — Foundation (blocks all later phases)

- [ ] T004 <task> — <file>

**Checkpoint:**

## Phase 3 — US1: <story name> (P1) 🎯 MVP

**Goal:** <what the story delivers>
**Independent test:** <how to prove it>

- [ ] T005 [P] [US1] Write test for <behavior> — <tests/...>
- [ ] T006 [US1] Implement <component> so T005 passes — <src/...>

**Checkpoint:** US1 works on its own; full test suite green.

## Phase 4 — US2: <story name> (P2)

…

## Phase N — Polish

- [ ] T0XX [P] Update documentation — <docs/...>
- [ ] T0XX Review logging, error messages, performance against the NFRs

**Checkpoint:** run `/sdd-verify`.

## Dependencies

- Phase 2 blocks every user story phase.
- <Special dependencies between tasks, if any.>

## Traceability

| Requirement / scenario | Tasks |
|------------------------|-------|
| FR-001 | T005, T006 |
| US1 #1 | T005 |
| NFR-001 | |

## Revision history

| Rev | Date | Plan rev | Change |
|-----|------|----------|--------|
| 1 | <YYYY-MM-DD> | 1 | Initial version |
