---
name: sdd-approve
description: The user approves a document (spec, plan, tasks or fix of a feature, or a requirements document) to unlock the next step of the SDD workflow.
disable-model-invocation: true
argument-hint: "<spec|plan|tasks|fix> [NNN] | requirements [RNNN]"
---

# Approve a document

Arguments: $ARGUMENTS

This skill runs only when the user types the command. Typing it IS the human approval. Claude never invokes this skill itself and never sets Status to `Approved` any other way. The `sdd-guard-edit` hook blocks every change to `Approved` that does not originate from this command.

## Steps

1. Determine the document type (`spec`, `plan`, `tasks`, `fix` or `requirements`) from the arguments. Ask if missing. Determine the feature, or for `requirements` the requirements document `docs/requirements/RNNN-slug/requirements.md`, per "SDD conventions" in `CLAUDE.md`.

2. Check the preconditions. If they are not met: **do not approve**; list the reasons and how to fix them.

   | Type | Preconditions |
   |------|---------------|
   | spec | No `[NEEDS CLARIFICATION` markers left; every user story has acceptance scenarios; every NFR has a measurable number. |
   | plan | `spec.md` is `Approved`; the plan's `Spec revision` equals the spec's `Revision`; the plan's `Architecture` equals the current `Version` of `docs/architecture.md`; no "Violation" row in the Constitution check lacks a justification; every package in the Architecture check is in the "Allowed dependencies" table for that layer (none left as "requires /sdd-architecture"); every FR/NFR appears in the mapping table. |
   | tasks | `spec.md` and `plan.md` are `Approved`; the tasks' `Plan revision` equals the plan's `Revision`; every FR and every acceptance scenario is referenced by at least one task; no task says "TODO" or is vague. |
   | fix | No `[NEEDS CLARIFICATION` markers left; the root cause has a `file:line`; the expected behavior is in Given/When/Then form; the first task is a regression test; the fix scope lists concrete files. |
   | requirements | No `[NEEDS CLARIFICATION` markers and no unticked open question left; every BG and BR appears in the Traceability table with at least one backlog item (or is under "Out of scope"); every backlog item has a priority P1–P3, a size `S` or `M` (no `L`), a spec brief and a `Covers` list; `Depends on` refers only to existing BL IDs, with no cycle; at least one item is P1; every QE has a number. |

   On a revision mismatch (including an `Architecture` version older than `docs/architecture.md`): name the command that updates the document (`/sdd-plan` or `/sdd-tasks`). On a package that still requires approval: name the `/sdd-architecture` command.

3. If the preconditions are met:
   - Get today's date with `date +%F`.
   - Change the `**Status:**` line to `Approved` and add (or update) an `**Approved:** <date>` line right below it.
   - If the hook blocks this edit (the token expires after 15 minutes): tell the user to type `/sdd-approve …` again; do not look for another way.
   - When approving `spec` while a `plan.md` with an older `Spec revision` exists: remind the user that the plan must be updated with `/sdd-plan`.
   - Commit that file alone: `docs(NNN): approve <type>` (for requirements: `docs(RNNN): approve requirements`).

4. Report the next step:
   - spec → `/sdd-plan [technology direction]`
   - plan → `/sdd-tasks`
   - tasks → `/sdd-build` (suggest starting with the first phase)
   - fix → `/sdd-build`
   - requirements → do not stop: read `.claude/skills/sdd-ba-spec/SKILL.md` and follow it right away for this `RNNN`, generating a Draft spec for every backlog item whose `Spec` column is `—`. The user's approval of the backlog is the go-ahead for this step.
