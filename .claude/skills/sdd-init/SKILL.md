---
name: sdd-init
description: Bootstrap the project for the SDD workflow — git, CLAUDE.md, test/lint commands, permissions in settings.json, hook check. Run once when starting a project or when installing the kit into an existing repo.
disable-model-invocation: true
argument-hint: "[project name — one-sentence description]"
---

# Initialize the SDD project

User input: $ARGUMENTS

Safe to run repeatedly: steps that are already done are only checked, then skipped.

## Steps

1. **Check the environment** (in parallel):
   - `git --version`, `node --version`. If Node is missing: warn that the SDD hooks (approval gate, code gate, session-start status) will not run, and ask the user whether to continue.
   - `git rev-parse --is-inside-work-tree` to see whether this is already a git repo.
   - Scan the root for stack markers: `package.json`, `pnpm-lock.yaml`, `yarn.lock`, `pyproject.toml`, `requirements.txt`, `*.csproj`, `*.sln`, `pom.xml`, `build.gradle*`, `go.mod`, `Cargo.toml`, `composer.json`, `pubspec.yaml`, `Makefile`. Read their scripts/targets to infer the install, test, single-test, lint/typecheck, dev and e2e commands.

2. **Ask the user in ONE turn** (use the multiple-choice question tool if available), only about what is still unknown:
   - Project name and one-sentence description (skip if given in the arguments).
   - If not a git repo yet: run `git init -b main`?
   - Whether the commands inferred in step 1 are correct. For a brand-new project without code, write `<not yet — filled in by /sdd-scaffold>`; do not choose a stack here (that is `/sdd-scaffold`'s job).

3. **Git** (if the user agreed): `git init -b main`. Make sure `.gitignore` contains these lines, adding any that are missing and never removing existing ones:
   ```
   .claude/state/
   .claude/settings.local.json
   ```

4. **CLAUDE.md**: replace `<Project name>`, the description line, and the lines under "Project commands". Do not touch the SDD rules and conventions.

5. **`.claude/settings.json`**: for each known test/lint/typecheck/e2e command, add a matching `allow` rule, e.g. `"Bash(npm test *)"`, `"Bash(npm run lint *)"`, `"Bash(dotnet test *)"`, `"Bash(pytest *)"`. Keep the `deny` and `hooks` sections intact. Show the user the diff.

6. **Check the hooks**: run `node .claude/hooks/sdd-status.js`. If it fails: report the error and how to fix it (usually Node missing from PATH).

7. **First commit** (for a new repo, or if steps 3–5 changed anything): ask the user first, then `git add -A` and commit `chore: init SDD kit`.

8. **Report**: what was configured, what is still empty. Next step:
   - Constitution is still a placeholder → `/sdd-constitution <your priorities>`.
   - No `docs/architecture.md` yet → `/sdd-scaffold` (after the constitution).
   - Both present → `/sdd-ba <requirements>`, `/sdd-spec <feature description>` or `/sdd-fix <bug description>`.
