---
name: sdd-retro
description: Look back at recent features to find recurring problems and propose process improvements — reviewer checklists, constitution, conventions in CLAUDE.md, new hooks.
disable-model-invocation: true
argument-hint: "[number of recent features to review, default 3]"
---

# Process retrospective

Arguments: $ARGUMENTS

## Steps

1. **Collect** from the N most recent features/fixes (by NNN, default 3):
   - `.claude/agent-memory/*/MEMORY.md`: notes the reviewers have accumulated.
   - `specs/*/verification.md`: "Must fix" items, "Remaining work", requirements that were not `Pass` on the first verification.
   - The "Revision history" tables of spec/plan/tasks: why amendments happened.
   - `git log --oneline` for `fix(NNN): review …` commits and `fix/` branches (bugs that slipped through the process).
   - Previous retros in `docs/retros/` (if any), to know which proposals were already applied.

2. **Group** problems that occurred at least twice. For each group, determine at which step it should have been caught (spec, clarify, plan, tasks, build, review).

3. **Propose** improvements, each tied to a problem group, choosing the strongest suitable tool:
   - A 100%-mandatory rule a machine can check → hook or lint rule.
   - An architectural/quality principle → constitution change (via `/sdd-constitution`).
   - Document review misses → add an item to the `spec-reviewer` checklist.
   - Code issues → add an item to the `code-reviewer` or `security-reviewer` checklist.
   - Naming or structure conventions → `CLAUDE.md`, or a knowledge skill with `user-invocable: false`.
   Mark agent-memory notes that have been turned into checklist items for removal from memory.

4. **Write `docs/retros/<date +%F>.md`**: data reviewed, problem groups (with concrete examples: feature, file), proposals, status of each proposal.

5. **Ask the user** which proposals to apply (multi-select). Apply the chosen changes to agent checklists, `CLAUDE.md` and agent-memory right away. Do not edit the constitution yourself: print the ready-made `/sdd-constitution …` command for the user to run.

6. **Commit**: `docs: retro <date>` (the retro file plus the applied changes).
