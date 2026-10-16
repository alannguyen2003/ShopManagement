---
name: sdd-verify
description: Verify that a feature or fix fully meets its documents before merging — check every acceptance criterion against code and tests, run the tests, review the whole diff and, when needed, run a security review and e2e checks.
disable-model-invocation: true
argument-hint: "[NNN]"
---

# Verify a feature

Arguments: $ARGUMENTS

## Steps

1. Determine the current feature per "SDD conventions" in `CLAUDE.md`. Run `node .claude/hooks/sdd-status.js <NNN>`.
   - Source documents: a regular feature uses `spec.md` + `tasks.md`; a fix uses `fix.md` ("Expected behavior" holds the acceptance scenarios).
   - If tasks remain unticked: list them and ask the user whether to verify what is done so far.
   - If any document is "⚠ stale": stop, because the code would be checked against outdated documents.

2. Determine the main branch (`main` or `master`) to diff against.

3. Choose the agents to run:
   - Always: `acceptance-verifier`, `test-runner` (full test suite and lint), `code-reviewer` (whole diff against the main branch).
   - `security-reviewer`: when the spec, plan or diff touches authentication, authorization, personal data, user or external input, file upload, external service calls, or secrets/configuration. When unsure, run it.
   - `e2e-tester`: when the plan's Test strategy has scenarios of type `e2e`, and "Project commands" has an e2e command or the agent has browser tools.

4. Run the chosen agents **in parallel**, each receiving the feature folder path and the main branch name.

5. Save the reviewers' "Memory suggestions" sections per the convention in `CLAUDE.md`.

6. Consolidate into `specs/NNN-slug/verification.md`:

   ```markdown
   # Verification: <Feature name>
   **Date:** <date +%F>
   **Verdict:** READY TO MERGE | NOT READY
   **Documents:** spec rev N · plan rev N · tasks rev N   (fix: fix.md)

   ## Requirement check
   | ID | Status | Evidence (file:line or test name) |

   ## Test and lint results

   ## E2E              (omit if not run)

   ## Code review
   ### Must fix
   ### Should fix

   ## Security review  (omit if not run)
   ### Must fix
   ### Should fix

   ## Remaining work
   ```

7. Conclude **READY TO MERGE** only when all of these hold: every requirement is `Pass` with evidence (or `Not automatable` with manual check instructions), tests and lint are green, e2e (if run) is green, and there are no "Must fix" items in either the code review or the security review.
   - Ready: change `**Status:**` in `tasks.md` (or `fix.md`) to `Done`, commit `docs(NNN): verification passed`. Next step: `/sdd-pr`.
   - Not ready: commit `docs(NNN): verification report`, list the remaining work with the matching command:
     - Problems within existing tasks (review "Must fix", failing tests): ask the user whether to fix them now. If they agree, fix them (tasks are still `Approved`, so the code gate allows it), commit `fix(NNN): verify — <description>`, then suggest rerunning `/sdd-verify` to re-verify from scratch.
     - A requirement without a task → `/sdd-amend tasks "<task to add>"`.
     - The spec is wrong or incomplete → `/sdd-amend spec "<change>"`.
