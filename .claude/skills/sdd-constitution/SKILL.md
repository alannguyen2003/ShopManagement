---
name: sdd-constitution
description: Create or update the project constitution (docs/constitution.md) — the non-negotiable principles every spec, plan and line of code must follow.
disable-model-invocation: true
argument-hint: "[priorities or changes to apply]"
---

# Create / update the constitution

User's wishes: $ARGUMENTS

## Steps

1. Read `docs/constitution.md` (if present) and `CLAUDE.md`. Skim the existing codebase (if any) to learn the stack and conventions in use.

2. **If there is no constitution yet** (file missing, or `**Status:** Placeholder`):
   - Run a short interview in ONE question turn, at most 5 questions, skipping any already answered in the arguments:
     1. What is the product, who uses it, at what expected scale?
     2. Which quality matters most (time to market, stability, performance, security, maintainability)?
     3. Desired test strategy (mandatory TDD? coverage level? e2e tests?)
     4. Hard technical constraints (platform, infrastructure, cost, legal/personal data)? The stack itself is fixed by the kit (.NET Clean Architecture backend, Next.js or React frontend, chosen in `/sdd-scaffold`); do not restate it as principles.
     5. What must never be done?
   - Write `docs/constitution.md` following [template.md](template.md).

3. **If a constitution already exists**: apply the requested change, then
   - Bump `Version` per semver: removing or redefining a principle = major; adding a principle = minor; wording fixes = patch.
   - Add a line to the Changelog at the end of the file.
   - List the `plan.md` files in `specs/` whose `**Constitution:**` version is now outdated, and suggest `/sdd-analyze` for each in-progress feature.

4. Quality bar for each principle:
   - 5–10 principles, no more.
   - Phrased as **MUST / MUST NOT**, never "should" or "try to".
   - One rationale line and one **How to verify** line (who/what detects a violation: test, lint, review, checklist).
   - No details of any specific feature.

5. Write the file. Summarize for the user: the principles (one line each), the version, and what they should review. Do not commit; the user commits after reading. Next step, if `docs/architecture.md` does not exist yet: `/sdd-scaffold`.
