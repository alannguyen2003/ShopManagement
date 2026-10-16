# Constitution — <Project name>

**Version:** 1.0.0
**Status:** Active
**Ratified:** <YYYY-MM-DD>
**Last amended:** <YYYY-MM-DD>

## Scope

This document is the highest law of the project. Every spec, plan, task and line of code must follow it. When there is a conflict, the constitution wins.

## Principles

### P1. <Short name>
- **Rule:** <The system MUST / MUST NOT ...>
- **Rationale:** <one line>
- **How to verify:** <which test / lint / review / checklist detects a violation>

### P2. <Short name>
- **Rule:**
- **Rationale:**
- **How to verify:**

<!-- Common example principles; edit or delete per project:
- Test first: MUST write a failing test before writing the code that makes it pass.
- Simplicity first: MUST NOT add a library, abstraction layer or service unless it serves a specific requirement in the spec.
- Module boundaries: each module MUST have a clear public interface; other modules MUST NOT reach into its internals.
- Input validation: all external data (requests, files, env) MUST be validated at the boundary.
- Secrets: secrets MUST NOT appear in code or logs.
- Observability: errors MUST be logged with context; exceptions MUST NOT be swallowed.
-->

## Amendment process

- All amendments go through `/sdd-constitution` and are reviewed by the user.
- Semantic versioning: removing/redefining a principle = major, adding one = minor, wording fixes = patch.

## Changelog

- 1.0.0 (<YYYY-MM-DD>): Initial version.
