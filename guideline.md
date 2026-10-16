# SDD Workflow Guide

This guide is for the people working with Claude Code on this project: how to get started, how to take a feature all the way through, how to handle changes and bugs, and how to read the system's block messages. The kit's technical structure is described in [README-SDD-KIT.md](README-SDD-KIT.md).

---

## 1. The core idea in 30 seconds

- **Documents before code.** Each feature has `spec.md` (what and why) → `plan.md` (how) → `tasks.md` (in what order). Claude writes code only once **you** have approved all three.
- **You are the only approver.** A document moves from `Draft` to `Approved` only when you type `/sdd-approve` yourself. Hooks block every other way.
- **Tests before code** (TDD). Every requirement traces to tasks, tests, and `file:line` evidence.
- **Changes leave a trail.** Changing an approved document goes through `/sdd-amend`, which bumps the Revision and requires re-approval.
- **One architecture, one dependency list.** `/sdd-scaffold` creates the skeleton (.NET Clean Architecture backend, Next.js or React frontend) and `docs/architecture.md`. Code goes where that document says, and only the packages in its "Allowed dependencies" table can be installed. Changing it goes through `/sdd-architecture`, typed by you.
- **Nothing leaves the project without you.** Push, publish, deploy and infrastructure changes are blocked. Other external actions (network calls, GitHub writes, MCP writes…) ask for your confirmation first.

```
/sdd-init → /sdd-constitution → /sdd-scaffold        (once per project)

                    ┌──────────── /sdd-amend (mid-stream change) ───────────────┐
                    ▼                                                            │
/sdd-spec → /sdd-clarify → ✔spec → /sdd-plan → ✔plan → /sdd-tasks → ✔tasks → /sdd-build → /sdd-verify → /sdd-pr
                                                                                  ▲
/sdd-fix ──────────────────────────────────────────────→ ✔fix ────────────────────┘     (small bug fix lane)

/sdd-ba → /sdd-clarify RNNN → ✔requirements → one Draft spec + branch per backlog item,
                                              each continuing from /sdd-clarify above   (large/raw requirements lane)

✔ = you type /sdd-approve      Any time: /sdd-status · /sdd-analyze · /sdd-architecture      Periodically: /sdd-retro
```

---

## 2. First-time setup (about 10 minutes)

**Requirements:** `git`, `node` (version 20+) and the .NET SDK (an LTS version, e.g. 10) on the PATH. Check with `git --version`, `node --version` and `dotnet --list-sdks`. Claude never installs them for you.

| Step | You type | Result |
|------|----------|--------|
| 1 | `/sdd-init Project name — one-sentence description` | Claude asks one round of questions (git init? test/lint commands?), then fills in `CLAUDE.md`, adds permissions to `settings.json`, and makes the first commit. |
| 2 | `/sdd-constitution <priorities>` | Claude interviews you with up to 5 questions, then writes `docs/constitution.md`: 5–10 MUST/MUST NOT principles. |
| 3 | Read the constitution carefully, edit it if needed, then commit it yourself | This is the project's highest law; every plan is checked against it. |
| 4 | `/sdd-scaffold [project name]` | Claude asks four questions in one round (frontend: Next.js or React + Vite; database; styling; e2e), writes `docs/architecture.md`, creates `src/backend` (.NET Clean Architecture: Domain, Application, Infrastructure, Api + 4 test projects) and `src/frontend`, runs build and tests, fills in "Project commands" and commits `chore: scaffold …`. |
| 5 | Read `docs/architecture.md` carefully | It binds every agent from now on: where each kind of code goes, and the only packages that may be installed. |

Example for step 2:

```
/sdd-constitution Internal web app for 200 employees. Maintainability and security over speed.
Mandatory TDD, coverage ≥ 80%. .NET 8 + PostgreSQL only. Never store plain-text passwords.
```

**New project with no code yet?** Leave the test/lint commands empty in step 1; `/sdd-scaffold` fills them in.

**What `/sdd-scaffold` creates:**

```
src/backend/   <Name>.sln, global.json, Directory.Build.props
  src/<Name>.Domain          entities, value objects, Result — references nothing
  src/<Name>.Application     use cases, interfaces — references Domain
  src/<Name>.Infrastructure  database, mail, external clients — references Application
  src/<Name>.Api             Program.cs, endpoints, /health — composition root
  tests/  Domain.Tests · Application.Tests · Api.IntegrationTests · ArchitectureTests
src/frontend/  Next.js (App Router) or React + Vite, TypeScript, Vitest + Testing Library
  src/features/<feature>/ · src/shared/{ui,lib/api-client.ts,config}
```

Typing `/sdd-scaffold` is your permission for that run only: while it runs, Claude may write under `src/` on `main` and run the official generators. The permission ends at your next prompt. If something interrupts it, type `/sdd-scaffold` again; it resumes.

From the next time you open Claude Code, each session starts with a one-line summary like `[SDD] Branch feat/002-… → Next step: /sdd-build`.

---

## 3. Taking a feature all the way through

Running example: *"Users can reset their password by email."*

### Step 1. Write the spec

```
/sdd-spec Users who forget their password can request a reset link by email; the link expires after a while
```

Claude creates the branch `feat/001-password-reset` and the file `specs/001-password-reset/spec.md`, has `spec-reviewer` review it, then commits. The spec does **not** mention technology. It contains only user stories, Given/When/Then acceptance scenarios, FRs/NFRs with numbers, edge cases, and out-of-scope items. Anything Claude does not dare to guess is marked `[NEEDS CLARIFICATION: …]`.

### Step 2. Clarify

```
/sdd-clarify
```

Claude asks **one question at a time** (at most 5 per round), each with a recommended option. Answer "go with the recommendation" if you agree. The spec is updated after every answer. Run it again if open points remain.

### Step 3. Approve the spec

Read the spec with these questions in mind:

- [ ] Does the P1 story alone deliver value (an MVP)?
- [ ] Can each acceptance scenario be turned into a test? If you were the tester, would you know what to check?
- [ ] Do the NFRs have numbers ("< 2 seconds", "at most 5 per hour")?
- [ ] Is "Out of scope" what you meant? This is what prevents scope creep.

Then type:

```
/sdd-approve spec
```

### Step 4. Plan

```
/sdd-plan ASP.NET Core minimal API, send mail through the company's existing SMTP server
```

The text after the command is an optional technology direction within the scaffolded stack (it cannot switch the backend away from .NET or the frontend framework). Claude surveys the codebase and writes `plan.md` (plus `research.md`, `data-model.md`, `contracts/` when needed). Whenever a major choice comes up that you have not specified, Claude asks.

When reviewing the plan, look at:

- [ ] **Constitution check**: is the justification for each "Violation" row convincing?
- [ ] **Architecture check**: are the layers and feature folders right? Every package marked `allowed`? A package marked "requires /sdd-architecture" must be allowed first (section 7), otherwise `/sdd-approve plan` refuses.
- [ ] **New libraries**: are they really needed?
- [ ] **File structure**: does it follow the repo's conventions?
- [ ] **Test strategy**: does every scenario have a test? Are scenarios that need to be checked through the UI marked `e2e`?

```
/sdd-approve plan
```

### Step 5. Tasks

```
/sdd-tasks
```

Claude splits the work into phases: Setup → Foundation → one phase per user story → Polish. In each phase the test-writing task always comes before the implementation task. A **Traceability** table at the end maps requirements to tasks.

Before approving, it is worth running:

```
/sdd-analyze
```

This command is read-only. It cross-checks spec, plan and tasks for missed requirements, inconsistent names and excess tasks. If all is well:

```
/sdd-approve tasks
```

### Step 6. Build

```
/sdd-build              # the first unfinished phase
/sdd-build phase 3      # a specific phase
/sdd-build T007         # a single task
/sdd-build all          # everything, without stopping between phases
```

For each task, Claude hands the work to the `software-engineer` agent, which reads the architecture, spec, plan and contracts, writes the test (or the code) in the place the architecture defines, and reports back. Claude then checks the result with `test-runner` itself: a new test must **fail for the right reason**, an implementation must make its tests pass, and no file outside the task may change. Only then does it tick the checkbox and commit. At the end of each phase, Claude runs the full test suite, has `code-reviewer` review the phase (including architecture violations), has the "Must fix" items fixed, then **stops to report**.

If `software-engineer` reports `BLOCKED` (a package that is not allowed, a document that is wrong), Claude stops and shows you the command to run; it never writes around the blocker.

Tips:

- Build one phase at a time and read the report at each checkpoint.
- If the conversation gets long, type `/clear` between phases. Progress lives in the checkboxes, so `/sdd-build` picks up straight away in a new session.
- If Claude stops saying "the document is wrong/incomplete", that is the intended behavior. See section 6 (`/sdd-amend`); a missing package or a place for code the architecture does not define goes through section 7 (`/sdd-architecture`).

### Step 7. Verify

```
/sdd-verify
```

Claude runs these agents in parallel:

| Agent | Checks |
|-------|--------|
| `acceptance-verifier` | Each FR/NFR/SC/scenario: code exists, a meaningful test exists, with `file:line` evidence |
| `test-runner` | Full test suite and lint |
| `code-reviewer` | The whole diff against `main` |
| `security-reviewer` | When the feature touches login, permissions, personal data, external input… |
| `e2e-tester` | When the plan has e2e scenarios |

The result goes into `verification.md` with a verdict of **READY TO MERGE** or **NOT READY**. If not ready, Claude names the command for each remaining item.

### Step 8. Pull Request

```
/sdd-pr
```

Claude drafts `pr.md` (summary, requirements delivered, how to test, risks). Claude is **not allowed to push**, so you run:

```bash
git push -u origin feat/001-password-reset
gh pr create --base main --title "feat(001): …" --body-file specs/001-password-reset/pr.md   # with GitHub CLI
```

After merging: type `/clear` before starting the next feature.

---

## 4. Large or raw requirements: `/sdd-ba`

Use it when what you have is bigger or messier than one feature: a whole module, meeting notes, a customer email, a requirements document. The `business-analyst` agent turns it into a requirements analysis and a backlog of spec-sized features; once you approve the backlog, Claude generates one spec per item. For a single, clear feature, `/sdd-spec` directly is faster.

```
/sdd-ba Employees request leave online; the manager approves within 2 working days; HR sees the balance of everyone.
/sdd-ba docs/inbox/leave-management-brief.md notes from the kickoff meeting: ...
```

| Step | You type | Claude does |
|------|----------|-------------|
| 1 | `/sdd-ba <text and/or file paths>` | On `main`: saves the input verbatim in `docs/requirements/R001-slug/input.md`; the `business-analyst` agent writes `requirements.md`: actors, business goals `BG-`, processes `PR-`, business rules `BR-`, quality expectations `QE-`, glossary, and a backlog `BL-01…` (priority, size, dependencies, which rules each item covers, a spec brief). `spec-reviewer` reviews it, then Claude commits. |
| 2 | `/sdd-clarify R001` | One question at a time, the document is updated after each answer. |
| 3 | Read `requirements.md`, then `/sdd-approve requirements R001` | Approves the backlog, then **immediately** generates the specs: reserves numbers `NNN` in the backlog's `Spec` column, and for each item (dependencies first, then P1 → P3) creates branch `feat/NNN-slug` from `main`, has the agent draft `spec.md` with `**Source:** R001/BL-XX`, has `spec-reviewer` review it, and commits. It ends on the first feature's branch with a summary table. |
| 4 | Per feature, in the table's order | The normal feature flow from section 3, step 2: `/sdd-clarify` → `/sdd-approve spec` → `/sdd-plan` → … Move to the next one with `git switch feat/NNN-slug`. |

When reviewing the requirements, look at:

- [ ] Is every business rule really what you said (or marked as an assumption)? The `Source` of each rule points to the input.
- [ ] Do the P1 items alone give a usable first release?
- [ ] Is each backlog item small (`S` ≤ 1 day, `M` 1–3 days) and does it deliver value on its own?
- [ ] Is "Out of scope" what you meant? Approving generates a spec and a branch **for every item** in the backlog.

Later changes:

- Requirements change after approval → `/sdd-amend requirements <change> R001`. Claude lists the specs affected (by the rules they cover) and tells you which ones to update with `/sdd-clarify` or `/sdd-amend spec`; it never edits them automatically.
- New backlog items after an amend → `/sdd-approve requirements R001` again generates specs only for items that do not have one yet. `/sdd-ba-spec R001 BL-07` generates one by hand.
- `/sdd-analyze R001` checks that every rule has a backlog item and that every spec still matches its item.

---

## 5. Small bug fixes: `/sdd-fix`

Use it when the system **misbehaves** relative to expected behavior, the fix is small (about ≤ 3 files), and needs no new library, data change or interface change.

```
/sdd-fix The order list page crashes when the customer has no orders. Log: NullReferenceException at OrderList.cs:42
```

| Step | You type | Claude does |
|------|----------|-------------|
| 1 | `/sdd-fix …` | Creates branch `fix/NNN-slug`, investigates the root cause (read-only, no edits), writes `fix.md`: symptom, reproduction, expected behavior, root cause `file:line`, fix scope, 3 tasks. |
| 2 | Read `fix.md`, then `/sdd-approve fix` | Unlocks code edits. |
| 3 | `/sdd-build` | Writes the regression test (must fail) → fixes → runs the full suite. |
| 4 | `/sdd-verify` → `/sdd-pr` | Same as for a feature. |

If Claude finds the "bug" is really a behavior change, it stops and suggests `/sdd-spec` or `/sdd-amend`.

---

## 6. Mid-stream changes: `/sdd-amend`

When requirements change after documents were approved, or the build reveals the plan is wrong, do **not** edit approved documents by hand. Use:

```
/sdd-amend spec The reset link expires after 30 minutes instead of 24 hours
/sdd-amend plan Send mail through a queue instead of synchronously
/sdd-amend tasks Add a task enforcing the 5-requests-per-hour limit for FR-006
```

Claude analyzes the impact (which FRs change, which finished tasks must be redone) and **asks you to confirm** before writing anything. Then:

1. The amended document's `Revision` goes up by 1, it returns to `Draft`, and a row is added to its "Revision history".
2. The lower-level documents also return to `Draft`.
3. The code gate **locks** until the whole chain is approved again.

Re-approval path, by the level that was amended:

```
spec  → /sdd-approve spec → /sdd-plan → /sdd-approve plan → /sdd-tasks → /sdd-approve tasks → /sdd-build
plan  →                     /sdd-approve plan → /sdd-tasks → /sdd-approve tasks → /sdd-build
tasks →                                                      /sdd-approve tasks → /sdd-build
requirements → /sdd-approve requirements (new items get specs) + update each affected spec on its branch
```

`/sdd-plan` and `/sdd-tasks` run in **update mode**: they change only the affected parts and keep unrelated finished tasks.

> If the spec is still `Draft` (never approved), no amend is needed: use `/sdd-clarify` or ask Claude to edit it directly.

---

## 7. Architecture and allowed dependencies: `/sdd-architecture`

`docs/architecture.md` is written by `/sdd-scaffold` and loaded into every session. It holds:

- **The layer table** of the backend: Domain references nothing; Application references Domain; Infrastructure references Application and Domain; Api references the rest and is the composition root. Hooks block a project reference that breaks it, and `ArchitectureTests` fails if code breaks it.
- **Folder conventions**: feature folders in each layer (`Application/<Feature>/…`, `Api/Endpoints/<Feature>Endpoints.cs`), and in the frontend `features/<feature>/` and `shared/`, with all HTTP going through `shared/lib/api-client.ts`.
- **The "Allowed dependencies" table**: package, ecosystem (`nuget`/`npm`), where it is allowed (a layer, `runtime`/`dev`, `*`), purpose.
- **Forbidden**: anything not in the table, global installs, system installers, other ecosystems, a second way of doing what the stack already does, business logic in Api or the frontend.

What the hooks check against the table:

| Claude tries | Result |
|--------------|--------|
| `npm install <pkg>`, `pnpm add`, `yarn add`, `npx <pkg>`, `npm create …` | Allowed only if the package is in the table for that scope (`dev` for `-D`) or already installed in the project |
| `dotnet add <project> package <pkg>`, `dotnet package add` | Allowed only if the package is in the table for that project's layer |
| `dotnet add <project> reference <other>` | Allowed only if the layer table allows it |
| Editing `*.csproj`, `Directory.*.props`, `package.json` | Every added package and project reference is checked the same way |
| `npm -g`, `dotnet tool install -g`, `winget`, `choco`, `pip`, `cargo`… | Always blocked |
| Editing `docs/architecture.md` | Blocked unless you typed `/sdd-scaffold` or `/sdd-architecture` |

When a feature needs something new:

```
/sdd-architecture add FluentValidation to Application for command validation
/sdd-architecture allow @tanstack/react-query (runtime) for server state in the frontend
/sdd-architecture add a Notifications feature folder convention for background jobs
```

Claude checks the package (exists, license, last release, duplicates something already allowed), shows the impact and **asks you to confirm**, then edits the document, bumps its `Version` (new dependency = minor, removed dependency or changed layer rule = major) and commits `docs: architecture vX.Y.Z — …`. It never installs the package here: installing is a task in the feature that needs it. Plans that record an older `**Architecture:**` version are listed; update them with `/sdd-plan` (update mode) or check them with `/sdd-analyze`.

---

## 8. Actions outside the project

The `sdd-guard-external` hook watches every shell command and every MCP, Artifact and remote-trigger tool call.

| Kind | Examples | Result |
|------|----------|--------|
| Push, publish, deploy, change infrastructure | `git push`, `npm publish`, `dotnet nuget push`, `docker push`, `gh release create`, `vercel`, `az … deploy`, `kubectl apply`, `terraform apply` | **Blocked.** Claude gives you the command to run yourself. |
| Other effects outside this machine | `curl`/`Invoke-WebRequest` to a non-local host, `ssh`/`scp`, mail, `gh pr create` and other GitHub writes, `git remote add`, `git clone`, cloud CLIs, `node -e` code that contacts a URL, MCP tools that are not read-only, publishing an Artifact | **You confirm** each call in a permission prompt. Allow it only when you asked for it. |
| Local and read-only | `curl http://localhost:5000/health`, `gh pr view`, `gh api` GET, `git fetch`, `dotnet test`, read-only MCP tools (`get…`, `list…`, `search…`), `WebFetch` | Allowed |

If a compound command mixes kinds (`curl … && git push`), the strictest one wins.

---

## 9. Status and revisions

| Document | Status | Revision fields |
|----------|--------|-----------------|
| `requirements.md` | Draft → Approved | `Revision` |
| `spec.md` | Draft → Approved | `Revision`, `Source` (backlog item, if generated by `/sdd-ba-spec`) |
| `plan.md` | Draft → Approved | `Revision`, `Spec revision` (the spec rev the plan is based on), `Constitution` (version), `Architecture` (version) |
| `tasks.md` | Draft → Approved → Done | `Revision`, `Plan revision` |
| `fix.md` | Draft → Approved → Done | — |
| `docs/architecture.md` | Scaffolding → Active | `Version` (semver, bumped by `/sdd-architecture`) |

**Stale** means the plan's `Spec revision` is lower than the spec's `Revision` (likewise for tasks vs plan). `/sdd-status` shows `⚠ stale`, and the code gate blocks until the document is updated and re-approved.

`Done` is set by `/sdd-verify` when the verdict is READY TO MERGE. It is not an approval but the result of verification.

---

## 10. Automatic guard rails (hooks)

| Hook | When | What it does |
|------|------|--------------|
| Session-start status | A session starts or resumes | Injects the branch, feature, next step and configuration warnings into context |
| Approval gate | Claude edits `specs/*/{spec,plan,tasks,fix}.md` or `docs/requirements/*/requirements.md` | Blocks any change of `Status` to `Approved` that does not come from you typing `/sdd-approve` (token valid for 15 minutes) |
| Code gate | Claude edits a file outside `specs/`, `docs/`, `.claude/`, root-level `*.md`, `.gitignore` | Allowed only when: it is a git repo, the branch is `feat/NNN-…` or `fix/NNN-…`, and that feature's documents are approved and not stale. Exception: files under `src/` while your `/sdd-scaffold` runs |
| Architecture gate | Claude edits `docs/architecture.md` | Allowed only while your `/sdd-scaffold` or `/sdd-architecture` runs (until your next prompt) |
| Dependency guard | Claude edits a manifest or runs a package manager | Blocks packages missing from the "Allowed dependencies" table or outside their layer, project references that break the layers, global and system installs (section 7) |
| External guard | Claude runs Bash or an MCP / Artifact / remote-trigger tool | Blocks push, publish, deploy and infrastructure changes; asks you to confirm other external effects (section 8) |
| Shell detours | Claude runs Bash | Blocks writing `Status: Approved` from the shell and touching `.claude/state/` |

### Reading block messages

| Message starts with | Meaning | What you do |
|---------------------|---------|-------------|
| `[SDD] Blocked editing …: the project is not a git repository yet` | Not initialized | `/sdd-init` |
| `… current branch "main" is not feat/…` | Editing code outside a feature | Start with `/sdd-spec` or `/sdd-fix` |
| `… tasks.md is "Draft"` | Not approved yet | Review, then `/sdd-approve tasks` |
| `… plan.md is based on an outdated spec revision` | The spec was just amended | `/sdd-plan` → `/sdd-approve plan` → … |
| `[SDD] Blocked: only the user can approve documents…` | Claude tried to approve, or the token expired | Type `/sdd-approve <type>` again |
| `… is not in the "Allowed dependencies" table…` / `… is allowed only in: …` | A package outside the allowlist or its layer | Decide whether it is needed; if so `/sdd-architecture add <package> to <layer> — <reason>` |
| `… Clean Architecture: X must not reference Y` | A reference that breaks the layers | Usually the code belongs in another layer; otherwise `/sdd-architecture` |
| `… the architecture and its allowed dependencies change only when the user types…` | Claude tried to edit `docs/architecture.md` | Run `/sdd-architecture <change>` yourself if you agree |
| `[SDD] Blocked: git push…` (or publish, deploy…) | An action only you may take | Run the command Claude gives you |
| Permission prompt `[SDD] Action outside the project: …` | An external effect | Allow only if you asked for it |

### When you really need to work outside the process

For example a spike or an urgent fix. Create `.claude/settings.local.json` (not committed):

```json
{ "env": { "SDD_GATE": "off" } }
```

Restart Claude Code. The code gate is off; the approval gate, the architecture gate, the dependency guard and the external guard stay on. Remember to delete the file when done. The other two switches work the same way, for exceptional cases only: `"SDD_DEPS": "off"` (dependency guard) and `"SDD_EXTERNAL": "off"` (external guard).

> Hooks are a guard rail against mistakes, not a security boundary. For example, code generated by a command such as `npm init` does not pass through the hooks. The rules in `CLAUDE.md` remain the second line of defense.

---

## 11. Command quick reference

| Command | When to use | Output |
|---------|-------------|--------|
| `/sdd-init [name — description]` | Once at the start; safe to rerun | git, `CLAUDE.md`, `settings.json`, `.gitignore` |
| `/sdd-constitution <priorities>` | Once; update when principles change | `docs/constitution.md` |
| `/sdd-scaffold [name]` | Once, after the constitution, before the first plan | `src/backend`, `src/frontend`, `docs/architecture.md`, project commands |
| `/sdd-architecture <change>` | A feature needs a package, project or convention the architecture lacks | `docs/architecture.md` Version +1 |
| `/sdd-ba <text \| files>` | Large or raw requirements | `docs/requirements/RNNN-slug/` (`input.md`, `requirements.md`) |
| `/sdd-approve requirements RNNN` | You agree with the backlog | Status → Approved, then one Draft spec + branch per backlog item |
| `/sdd-ba-spec RNNN [BL-XX …]` | Backlog items that have no spec yet | branches `feat/NNN-slug`, `spec.md` |
| `/sdd-spec <description>` | New feature | branch `feat/NNN-slug`, `spec.md` |
| `/sdd-clarify [RNNN]` | Spec or requirements have `[NEEDS CLARIFICATION]` | Updated `spec.md` / `requirements.md` |
| `/sdd-approve <spec\|plan\|tasks\|fix>` | You have read it and agree | Status → Approved, commit |
| `/sdd-plan [technology]` | After approving the spec, or after the spec was amended | `plan.md` (+ research, data-model, contracts) |
| `/sdd-tasks` | After approving the plan, or after the plan was amended | `tasks.md` |
| `/sdd-build [T005\|phase 3\|all]` | After approving tasks/fix | Code + tests by `software-engineer`, one commit per task |
| `/sdd-verify` | All tasks done | `verification.md` |
| `/sdd-pr` | Verification passed | `pr.md` + push instructions |
| `/sdd-fix <bug>` | Small bug | branch `fix/NNN-slug`, `fix.md` |
| `/sdd-amend <level> <change>` | Change an approved document | Revision +1, back to the approval gate |
| `/sdd-analyze [NNN\|RNNN]` | Before approving tasks, after an amend, when you suspect drift | Report in chat (read-only) |
| `/sdd-status [NNN\|RNNN]` | "Where am I? What next?" | Status table |
| `/sdd-retro [N]` | Every 2–3 features | `docs/retros/<date>.md`, improved checklists |

You can also ask naturally, e.g. "what's left?" or "do the documents agree?". Claude invokes `/sdd-status` or `/sdd-analyze` on its own.

---

## 12. Conventions

- **Folders:** `specs/NNN-slug/`. Features and fixes share one number sequence. Requirements: `docs/requirements/RNNN-slug/`, with their own sequence, kept on `main`.
- **Branches:** `feat/NNN-slug`, `fix/NNN-slug`; the main branch is `main`.
- **IDs:** `FR-001`, `NFR-001`, `SC-001`, `US1`, `T001`.
- **Commits:**

  | Kind | Pattern |
  |------|---------|
  | Feature documents | `docs(001): draft spec`, `docs(001): approve plan`, `docs(001): amend spec rev 2 — …` |
  | Code | `test(001): T004 …`, `feat(001): T005 …`, `fix(002): T002 …` |
  | Review-driven fixes | `fix(001): review phase 3`, `fix(001): verify — …` |
  | Shared | `docs: retro 2026-11-01`, `docs: architecture v1.1.0 — add FluentValidation`, `chore: init SDD kit`, `chore: scaffold …` |

- **Agent memory:** `.claude/agent-memory/<agent>/MEMORY.md` (reviewers, `business-analyst`, `software-engineer`). Commit it so the whole team benefits.

---

## 13. Tips for a smooth workflow

1. **Keep specs small.** A feature should take 1–3 days. A spec with more than 5 user stories should be split.
2. **Invest in acceptance scenarios.** They are the only thing checked all the way through, from tests to `acceptance-verifier`.
3. **Actually read before `/sdd-approve`.** The approval gate is only worth as much as the reading behind it. Plan and tasks are the cheapest place to catch mistakes.
4. **Run `/sdd-analyze` before `/sdd-approve tasks`.** One minute that can save a whole phase of wrong building.
5. **One unfinished feature at a time.** Switching branches between several half-done features easily mixes up context.
6. **`/clear` between phases and between features.** All state lives in files, not in the conversation.
7. **Never hand-edit approved documents.** Use `/sdd-amend` to get a Revision and history.
8. **Keep the dependency list short.** Every `/sdd-architecture add …` is a long-term cost; prefer what the stack already has.
9. **Retro regularly.** A mistake seen twice becomes a checklist item; seen three times, it becomes a hook or lint rule.

---

## 14. Troubleshooting

| Situation | What to do |
|-----------|------------|
| No `[SDD] …` line at session start | Check `node --version`; try `node .claude/hooks/sdd-status.js`; type `/hooks` in Claude Code to see whether the hooks are loaded. |
| `/sdd-approve` is blocked even though you typed it | The token expired (15 minutes) or the prompt hook did not run. Type the command again; if still blocked, check `/hooks`. |
| Claude says "tests still fail after 3 attempts" | Claude stops on purpose. Read the suspected cause; often the plan/contract is wrong and needs `/sdd-amend plan`. |
| Claude says a document contradicts existing code | Decide which side is right. Document wrong → `/sdd-amend`; old code wrong → add a task via `/sdd-amend tasks`. |
| `/sdd-status` shows `⚠ stale` | A higher-level document just changed. Follow the "Next step" column. |
| Need to edit a config file outside the allowlist (`package.json`, `.editorconfig`…) before any feature exists | Put it in the Setup phase of the first feature, or temporarily set `SDD_GATE=off`. |
| `/sdd-scaffold` stopped halfway (a hook blocked after you typed something else) | Type `/sdd-scaffold` again: `docs/architecture.md` is still `Scaffolding`, so it resumes from the first missing step. |
| `/sdd-plan` or `/sdd-build` says there is no architecture | New project: run `/sdd-scaffold`. Kit installed into an existing codebase: type `/sdd-architecture describe the existing structure`; Claude writes `docs/architecture.md` from what is already there. |
| A permission prompt appears for something you did not ask for | Deny it, and ask Claude why it tried; it should have proposed the action instead. |
| The constitution got a major version bump | `/sdd-constitution` lists the affected plans. Run `/sdd-analyze` for each in-progress feature. |
| You want to drop the hooks entirely | Delete the `"hooks"` block in `.claude/settings.json`. The rules then live only in `CLAUDE.md`. |
