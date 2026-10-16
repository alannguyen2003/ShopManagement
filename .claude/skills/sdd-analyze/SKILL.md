---
name: sdd-analyze
description: Read-only cross-check of consistency between the constitution, spec, plan, tasks and code of a feature — missed requirements, stale documents, code drifting from the documents — or between a requirements document (RNNN) and the specs generated from it. Use when the user asks whether the documents agree, after an amend, or before approving tasks.
argument-hint: "[NNN | RNNN]"
---

# Cross-check documents and code

Arguments: $ARGUMENTS

Read-only. Modify no files, make no commits. Every suggested fix comes with the SDD command the user should run.

## Steps

1. Determine the feature per "SDD conventions" in `CLAUDE.md`. Run `node .claude/hooks/sdd-status.js <NNN>` to get the status and staleness flags.

2. Read `docs/constitution.md`, `docs/architecture.md` and every document in the feature folder: `spec.md`, `plan.md`, `research.md`, `data-model.md`, `contracts/`, `tasks.md` (or `fix.md`), `verification.md`.

3. Check by group (skip groups that do not apply because the documents do not exist yet):

   | Group | Checks |
   |-------|--------|
   | Revision chain | The plan's `Spec revision` equals the spec's `Revision`; the tasks' `Plan revision` equals the plan's `Revision`; the plan's `Constitution` equals the constitution's current `Version`. |
   | Coverage | Every FR/NFR/SC appears in the plan's mapping table; every FR/NFR and acceptance scenario has a task in the Traceability table; every scenario has a row in the Test strategy. |
   | Excess | Plan components or tasks that trace back to no requirement. |
   | Contradiction | Entity, field or endpoint names that differ between spec, data-model, contracts and tasks; NFR numbers that differ between documents. |
   | Ambiguity | Leftover `[NEEDS CLARIFICATION`, "TODO", unmeasurable adjectives in NFRs. |
   | Constitution | The plan says "Pass" but the design or tasks actually violate a principle. |
   | Architecture | The plan's `Architecture` is older than `docs/architecture.md`'s `Version`; a package in the plan, tasks or manifests (`*.csproj`, `src/frontend/package.json`) is missing from the "Allowed dependencies" table or used outside its layer; a `ProjectReference` breaks the layer table; files in tasks or in the diff sit outside the places the architecture defines. |
   | Code vs documents (once building) | `[x]` tasks whose file does not exist; no commit found by `git log --oneline --grep "T0XX"`; new files in `git diff --stat <main branch>...HEAD` missing from the plan's File structure. |
   | Source (spec has `**Source:** RNNN/BL-XX`) | Every BR/QE in that backlog item's `Covers` is reflected in an FR, scenario or NFR; glossary terms are used unchanged; nothing the requirements assign to another backlog item is specified here. |

4. Return the report in the conversation:

   ```
   ## Status
   <one line from sdd-status>

   ## Findings
   | Severity | Group | Location | Problem | Suggestion |
   |----------|-------|----------|---------|------------|
   | Critical | Coverage | spec FR-004 | No task covers it | /sdd-amend tasks "add a task for FR-004" |

   ## Conclusion
   <Consistent / Fix before approving / Fix before building further>
   ```

   `Critical` is for anything that would make the build miss a requirement; `Warning` for risks; `Suggestion` for further clarification. Write "None" for any empty severity.

## Requirements mode (argument `RNNN`)

Run `node .claude/hooks/sdd-status.js RNNN`. Read `requirements.md` and every spec named in its `Spec` column, from its branch with `git show feat/NNN-slug:specs/NNN-slug/spec.md` (or from `specs/` once merged). Check, and report in the same format:

| Group | Checks |
|-------|--------|
| Coverage | Every BG/BR is in the Traceability table with a backlog item, or under "Out of scope"; every backlog item of an Approved document has a spec. |
| Spec fidelity | Each spec's `**Source:**` matches its backlog row; each BR/QE in `Covers` appears in that spec; no two specs specify the same rule differently. |
| Drift | Specs whose content goes beyond their backlog item (new rules not in the requirements) → either `/sdd-amend requirements` or trim the spec. Requirements revised after a spec was drafted → list those specs for review. |
| Backlog health | Items of size `L`, dependency cycles, P1 items depending on P2/P3 items. |
