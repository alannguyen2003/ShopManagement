# Constitution — Shop Management

**Version:** 1.0.0
**Status:** Active
**Ratified:** 2026-10-16
**Last amended:** 2026-10-16

## Scope

This document is the highest law of the project. Every spec, plan, task and line of code must follow it. When there is a conflict, the constitution wins.

Priorities, in order: **security**, then **maintainability**. When the two conflict, security wins.

## Principles

### P1. Single administrator
- **Rule:** The system MUST support exactly one administrator account. It MUST NOT offer self-registration, additional user accounts or role management.
- **Rationale:** The product serves a single admin; extra account features add attack surface and code to maintain with no user benefit.
- **How to verify:** Spec review rejects multi-user or sign-up requirements; code review checks that no registration or user-creation endpoint exists.

### P2. Test first (TDD)
- **Rule:** Every behavior change MUST start with a failing test that is committed before or together with the code that makes it pass. Production code MUST NOT be written without a test that covers it.
- **Rationale:** Tests written first keep the design simple and give a safety net for every future change.
- **How to verify:** `code-reviewer` checks each task's commit for a matching test; `test-runner` confirms the test suite passes.

### P3. Test coverage above 80%
- **Rule:** Line coverage of production code MUST be greater than 80% for the backend and for the frontend, measured separately. A change MUST NOT lower coverage below that threshold.
- **Rationale:** A high, enforced coverage floor keeps refactoring safe and catches regressions early.
- **How to verify:** The coverage report in the test command fails the run below the threshold; `/sdd-verify` records the coverage figures.

### P4. Password hashing
- **Rule:** Passwords MUST be stored only as a salted, slow hash: PBKDF2 with HMAC-SHA256, a unique random salt of at least 16 bytes per password, and at least 600,000 iterations. Passwords MUST NOT be stored or transmitted in plain text, with reversible encryption, or as an unsalted or single-round hash (including plain SHA-256). Hash comparison MUST be constant-time.
- **Rationale:** A leaked database must not reveal the admin password; a fast hash such as plain SHA-256 can be brute-forced offline.
- **How to verify:** Unit tests check the stored format (algorithm, salt, iterations) and that the same password produces different hashes; `security-reviewer` checks every place a password is handled.

### P5. Authentication by default
- **Rule:** Every API endpoint and every admin page MUST require an authenticated admin session, except the login endpoint and the login page. Login MUST be protected against brute force (attempt limiting or lockout).
- **Rationale:** All data in the system is admin-only; an endpoint that is open by mistake exposes all of it.
- **How to verify:** Integration tests call each endpoint without credentials and expect 401; `security-reviewer` checks new endpoints.

### P6. Input validation at the boundary
- **Rule:** All external input (HTTP requests, query strings, uploaded files, configuration) MUST be validated at the boundary before it reaches business logic. Database access MUST use parameterized queries or the ORM; SQL MUST NOT be built by concatenating input.
- **Rationale:** Validating once at the edge prevents injection and keeps invalid data out of the domain.
- **How to verify:** Tests cover rejected invalid input for each endpoint; `security-reviewer` and `code-reviewer` check for raw SQL and missing validation.

### P7. No secrets in code or logs
- **Rule:** Secrets (passwords, password hashes, keys, tokens, connection strings) MUST NOT appear in source code, committed configuration, logs or error responses. Error responses to the client MUST NOT expose stack traces or internal details.
- **Rationale:** Source and logs are widely shared; a single leaked secret compromises the system.
- **How to verify:** `security-reviewer` scans the diff for secrets; tests check that error responses contain no stack traces.

### P8. Simplicity first
- **Rule:** Code MUST NOT add a library, abstraction layer, project or service unless it serves a specific requirement in an approved spec. Each module MUST expose a clear public interface, and other modules MUST NOT reach into its internals.
- **Rationale:** Less code and fewer dependencies mean an app that one person can understand and maintain.
- **How to verify:** `code-reviewer` checks new abstractions against the spec; new dependencies must be on the `docs/architecture.md` allowlist.

### P9. Clean build and visible errors
- **Rule:** Code MUST pass the project's lint, format and typecheck with zero errors and zero warnings before merge. Exceptions MUST NOT be swallowed; unexpected errors MUST be logged with context (no secrets, per P7).
- **Rationale:** A clean build and visible errors keep the codebase readable and failures easy to diagnose.
- **How to verify:** The lint/typecheck command in `CLAUDE.md` fails on any warning; `code-reviewer` checks for empty catch blocks.

## Amendment process

- All amendments go through `/sdd-constitution` and are reviewed by the user.
- Semantic versioning: removing/redefining a principle = major, adding one = minor, wording fixes = patch.

## Changelog

- 1.0.0 (2026-10-16): Initial version.
