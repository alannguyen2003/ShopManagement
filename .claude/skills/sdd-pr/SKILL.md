---
name: sdd-pr
description: Draft the Pull Request description (pr.md) for a feature or fix that passed /sdd-verify, from the spec, verification and commit history, then explain how to push and open the PR.
disable-model-invocation: true
argument-hint: "[NNN]"
---

# Prepare the Pull Request

Arguments: $ARGUMENTS

## Steps

1. Determine the feature per "SDD conventions" in `CLAUDE.md`. Preconditions: `tasks.md` (or `fix.md`) is `Done`, and `verification.md` says `**Verdict:** READY TO MERGE`. If not: stop and name the missing step (usually `/sdd-verify`).

2. Gather: `spec.md` (or `fix.md`), `plan.md`, `verification.md`, `git log --oneline <main branch>..HEAD`, `git diff --stat <main branch>...HEAD`.

3. Write `specs/NNN-slug/pr.md`:

   ```markdown
   # <type>(NNN): <short title, under 70 characters>

   ## Summary
   <2–4 lines: what problem, how it is solved. For a fix, state the root cause.>

   ## Documents
   - Spec: specs/NNN-slug/spec.md (rev N) · Plan: plan.md (rev N) · Verification: verification.md

   ## Requirements delivered
   | ID | Short description | Evidence |
   <taken from the Requirement check table in verification.md>

   ## Main changes
   - <grouped by component, not one line per commit>

   ## Testing
   - Automated: <test/lint results from verification.md>
   - Manual: <steps for reviewers to try it, including every "Not automatable" item>

   ## Risks and rollback
   - <migrations, new configuration, how to roll back>

   ## Out of scope
   - <from the spec, so reviewers do not ask for more>
   ```

   `<type>` is `feat` for a feature, `fix` for a fix.

4. Commit: `docs(NNN): pr description`.

5. Instructions for the user (Claude does not push, because `git push` is denied in settings):
   ```
   git push -u origin <branch>
   ```
   With the GitHub CLI: `gh pr create --base <main branch> --title "<title line>" --body-file specs/NNN-slug/pr.md`. Claude runs this `gh` command only after the user has pushed and explicitly agreed.

6. Remind: after merging, run `/clear` before the next feature; run `/sdd-retro` every 2–3 features.
