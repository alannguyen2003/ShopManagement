---
name: sdd-spec
description: Create a specification (spec.md) for a new feature from the user's description. The spec describes only WHAT and WHY, never technology.
disable-model-invocation: true
argument-hint: "<feature description>"
---

# Create a spec for a new feature

Feature description from the user: $ARGUMENTS

If the description is empty: ask the user to describe the feature before continuing.

## Steps

1. **Prepare**
   - If the project is not a git repo: stop and suggest `/sdd-init` (branches and commits are the backbone of the workflow).
   - Read `docs/constitution.md`. If it is still a placeholder: suggest running `/sdd-constitution` first and ask whether to continue anyway.
   - List `specs/` to get the next `NNN` (highest existing + 1; `001` if none).
   - Choose a `slug` of 2–4 English words, kebab-case, capturing the essence of the feature.
   - If the feature looks like a duplicate of, or overlaps with, an existing spec: stop, point to that spec and ask whether to extend it (`/sdd-amend spec`) or create a new one. If it is really a bug fix: suggest `/sdd-fix`. If it is too big for one spec (more than 5 user stories, several actors with separate goals, or a whole module): suggest `/sdd-ba` to analyze it into a backlog first.

2. **Branch**
   - Check `git status`. If there are uncommitted changes: ask the user before switching branches.
   - Create and switch to `feat/NNN-slug` from the main branch.

3. **Write `specs/NNN-slug/spec.md`** following [template.md](template.md):
   - Write from the user's and the business's point of view. Do NOT mention frameworks, languages, databases, libraries or code structure.
   - User stories carry a priority P1/P2/P3. P1 alone must be enough for a usable MVP.
   - Each user story has acceptance scenarios in **Given / When / Then** form that a test can verify.
   - Functional requirements `FR-001…`; non-functional requirements `NFR-001…` must have measurable numbers (time, count, rate).
   - Include Edge cases: empty data, very large data, duplicates, lost connectivity, insufficient permissions…
   - **Do not guess** anything that affects scope, security or user experience: write `[NEEDS CLARIFICATION: <question>]` in place and list it again under "Open questions". Keep to roughly the 5 most important.
   - Minor details with a sensible default: choose it and record it under "Assumptions".
   - State "Out of scope" explicitly.
   - `**Source:** —` (only `/sdd-ba-spec` fills it), `**Status:** Draft`, `**Revision:** 1`.

4. **Self-review**: hand the file to the `spec-reviewer` subagent. Fix "Critical" findings that need no decision from the user; put those that do under "Open questions". Save the "Memory suggestions" section per the convention in `CLAUDE.md`.

5. **Commit** the spec: `docs(NNN): draft spec`.

6. **Report** to the user, briefly:
   - File path and branch.
   - The feature in 3–5 lines, user stories by priority.
   - Number of open questions.
   - Next step: `/sdd-clarify` if there are open questions; otherwise read the spec and run `/sdd-approve spec`.
