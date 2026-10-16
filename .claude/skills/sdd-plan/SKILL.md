---
name: sdd-plan
description: Create or update the technical plan (plan.md) for a feature whose spec is approved — architecture, technology, data model, interfaces and test strategy.
disable-model-invocation: true
argument-hint: "[technology direction, e.g. Next.js + PostgreSQL]"
---

# Technical planning

Technology direction from the user: $ARGUMENTS

## Steps

1. Determine the current feature per "SDD conventions" in `CLAUDE.md`. If `spec.md` is not `Approved`: stop and remind the user to run `/sdd-approve spec`. If `docs/architecture.md` is missing or still `Scaffolding`: stop and suggest `/sdd-scaffold` (the plan builds on the architecture).

2. **Update mode**: if `plan.md` already exists (typically after `/sdd-amend spec`, or after `/sdd-architecture` raised the architecture version; then update `Architecture` and the Architecture check too):
   - Read the spec's "Revision history" to learn what changed since the `Spec revision` the plan is based on.
   - Change only the affected parts; keep the rest as is.
   - If the plan is still `Approved`: that would be editing an approved document; stop and suggest `/sdd-amend plan`.
   - If the plan is `Draft`: if the file still has an `**Approved:**` line (the current version was approved before), bump `Revision` by 1 and delete that line; update `Spec revision`; add a row to "Revision history". Skip steps 3–4 for unchanged parts and continue from step 5.

3. **Gather context**
   - Read `docs/constitution.md`, `docs/architecture.md`, `spec.md`, and the plans of completed features in `specs/` for consistency.
   - The technology direction in the arguments cannot override the architecture: the backend is .NET Clean Architecture and the frontend is the one scaffolded. A direction that conflicts with it (another framework, another database) → stop and suggest `/sdd-architecture` first.
   - Explore the existing codebase: directory layout, libraries in use, naming conventions, how tests are written. For a large repo, delegate this to the `Explore` subagent and take back only a summary.
   - Prefer reusing what exists over adding new things.

4. **Research when needed**: for any major decision the user has not specified (library, storage, protocol, external service), write `research.md` in the same folder: for each decision, the options, pros/cons, the choice and why. Ask the user before writing the plan about decisions that are truly theirs to make.
   - Prefer what the "Allowed dependencies" table already has. If the feature truly needs a package that is not in it: list it under "Dependencies requiring /sdd-architecture" in the plan, and tell the user to run `/sdd-architecture "add <package> to <layer> — <reason>"` before approving the plan.

5. **Write `plan.md`** following [template.md](template.md). Required:
   - Header: `Revision`, `Spec revision` (= the spec's current `Revision`), `Constitution` (= the constitution's current `Version`), `Architecture` (= the current `Version` of `docs/architecture.md`).
   - **Constitution check**: a table of every principle → `Pass` or `Violation`, with a justification of why it cannot be avoided and why the simpler alternative was rejected.
   - **Architecture check**: which layers and feature folders the feature touches; every package it uses, each marked `allowed` (in the table, for that layer) or `requires /sdd-architecture`; confirmation that no layer rule is broken. The "File structure" uses only places the architecture defines.
   - **Requirement mapping**: every FR/NFR in the spec → which design component handles it.
   - **File structure**: the files/directories to create or modify, following the repo's existing conventions.
   - If the feature has data: also write `data-model.md` (entities, fields, constraints, relationships, state transitions).
   - If there are interfaces between parts (API, events, public functions): write `contracts/` (e.g. OpenAPI files, schemas, or function signatures with descriptions).
   - **Test strategy**: which tests (unit / integration / e2e) prove which acceptance scenario. Mark scenarios that must be checked through the UI as `e2e` so `/sdd-verify` calls `e2e-tester`.
   - **Keep it simple**: no layer, pattern or library that does not serve a specific requirement.
   - `**Status:** Draft`.

6. **Self-review**: hand the plan to the `spec-reviewer` subagent to check it against the spec and constitution. Fix "Critical" findings. Save the "Memory suggestions" section per the convention in `CLAUDE.md`.

7. Commit: `docs(NNN): draft plan` (or `docs(NNN): update plan rev N`).

8. **Report**: the key technical decisions (one line each), risks, what the user should read carefully. Next step: review, then `/sdd-approve plan`.
