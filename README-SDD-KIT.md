# SDD Kit for Claude Code

A home-grown Spec-Driven Development workflow built only from native Claude Code features: skills, subagents, hooks, CLAUDE.md and settings. The hooks are written in Node; no other external tooling is required.

Step-by-step usage guide: **[guideline.md](guideline.md)**. This file describes the kit's structure and design rationale.

## What is in the kit

```
CLAUDE.md                         SDD rules + conventions + project commands (read by Claude every session)
guideline.md                      Human-facing usage guide
docs/constitution.md              Placeholder; /sdd-constitution writes the content
docs/architecture.md              Created by /sdd-scaffold: layers, folder conventions, allowed dependencies
docs/requirements/                Created by /sdd-ba: one folder per requirements analysis, RNNN-slug/
docs/sdd-workflow.html            Visual walkthrough from raw requirements to a merged product
specs/                            One folder per feature/fix: specs/NNN-slug/
src/                              Created by /sdd-scaffold: backend/ (.NET Clean Architecture), frontend/ (Next.js or React)
.gitignore                        Ignores .claude/state/ and settings.local.json
.claude/
├── settings.json                 Permissions + hook registration
├── hooks/
│   ├── lib/sdd.js                Reads document status, revision chain, command tokens
│   ├── lib/deps.js               Reads the dependency allowlist and layer rules from docs/architecture.md
│   ├── sdd-status.js             Status table (used by /sdd-status and SessionStart)
│   ├── sdd-guard-edit.js         PreToolUse Edit/Write: approval gate, architecture gate, dependency guard, code gate
│   ├── sdd-guard-bash.js         PreToolUse Bash: blocks shell detours
│   ├── sdd-guard-external.js     PreToolUse Bash/MCP/Artifact: dependency guard for installs, external-action guard
│   └── sdd-prompt.js             UserPromptSubmit: writes a token when the user types /sdd-approve, /sdd-scaffold, /sdd-architecture
├── skills/
│   ├── sdd-init/                 /sdd-init          bootstrap git, commands, permissions
│   ├── sdd-constitution/         /sdd-constitution  + template.md
│   ├── sdd-scaffold/             /sdd-scaffold      project skeleton in src/ + architecture-template.md
│   ├── sdd-architecture/         /sdd-architecture  change docs/architecture.md (dependencies, layers, conventions)
│   ├── sdd-ba/                   /sdd-ba            business analysis → requirements + backlog, + template.md
│   ├── sdd-ba-spec/              /sdd-ba-spec       one Draft spec + branch per approved backlog item
│   ├── sdd-spec/                 /sdd-spec          + template.md
│   ├── sdd-clarify/              /sdd-clarify
│   ├── sdd-approve/              /sdd-approve       (the human approval gate)
│   ├── sdd-plan/                 /sdd-plan          + template.md
│   ├── sdd-tasks/                /sdd-tasks         + template.md
│   ├── sdd-build/                /sdd-build         orchestrates software-engineer per task
│   ├── sdd-verify/               /sdd-verify
│   ├── sdd-pr/                   /sdd-pr            PR description
│   ├── sdd-fix/                  /sdd-fix           fast lane for bug fixes + template.md
│   ├── sdd-amend/                /sdd-amend         change approved documents
│   ├── sdd-analyze/              /sdd-analyze       read-only cross-check
│   ├── sdd-retro/                /sdd-retro         process improvement
│   └── sdd-status/               /sdd-status
└── agents/
    ├── software-engineer.md      Implements one task with TDD inside docs/architecture.md (Sonnet, writes code)
    ├── business-analyst.md       Analyzes raw requirements, drafts backlog and specs (Opus, read-only)
    ├── spec-reviewer.md          Reviews requirements/spec/plan/tasks/fix (Opus, read-only)
    ├── code-reviewer.md          Reviews diffs (Sonnet, read-only)
    ├── security-reviewer.md      Security review (Opus, read-only)
    ├── test-runner.md            Runs tests, reports concisely (Haiku)
    ├── e2e-tester.md             Checks e2e scenarios (Sonnet)
    └── acceptance-verifier.md    Checks spec against code + tests, demands evidence (Opus)
```

## Installation

1. Copy the kit into the project root. If the project already has `CLAUDE.md`, `.claude/settings.json` or `.gitignore`, merge the contents instead of overwriting.
2. `git`, `node` (≥ 20) and the .NET SDK (LTS) must be on the PATH.
3. Open Claude Code in the project folder, run `/sdd-init`, then `/sdd-constitution`, then `/sdd-scaffold`.

Details: see [guideline.md](guideline.md).

## Design and rationale

| Decision | Reason |
|----------|--------|
| Every phase skill sets `disable-model-invocation: true` | You decide when to move between phases; Claude does not jump from spec to code on its own. |
| Critical rules are enforced by hooks, not only written in CLAUDE.md | Instructions can be lost in long conversations; hooks run every time, deterministically. |
| Approval gate: token from `UserPromptSubmit`, transcript as fallback | Only a prompt typed by a human creates a token; Claude cannot approve documents it wrote itself. |
| The code gate checks the whole spec → plan → tasks chain and revisions | Amending the spec after approval automatically locks code edits until plan/tasks are updated and re-approved. |
| `Revision` + `Spec revision` / `Plan revision` | Stale documents are detected by a machine, not by memory. |
| A dedicated `/sdd-amend` | Post-approval changes get history and impact analysis, and always return to the approval gate. |
| A dedicated `/sdd-fix` | Small bugs still get a regression test and approval, without a full spec/plan/tasks. |
| An optional BA lane (`/sdd-ba` → `requirements.md` → specs) | Raw or large requirements are decomposed once, with shared actors, rules and glossary, before any spec exists. Every generated spec carries `**Source:** RNNN/BL-XX`, so coverage of each business rule can be checked. |
| Specs are generated only after the user approves the backlog | Generating creates one branch per item; the approval is the human decision about *what* gets specified. |
| `business-analyst` is read-only and returns drafts | Same rule as the reviewers; also, a subagent cannot interview the user, so questions and file writes stay in the main session. It still pays off by reading long input documents in its own context. |
| Requirements live on `main`; spec numbers are reserved in the backlog before branching | Each spec lives on its own unmerged branch, so `specs/` on `main` alone cannot tell which numbers are taken. |
| Input is kept verbatim in `input.md` | Every business rule cites its source, so you can tell what was said from what was interpreted. |
| Status computed by a script (`sdd-status.js`) | One logic shared by `/sdd-status`, the SessionStart hook and the other skills. |
| Reviewers have no write tools; the main session saves "Memory suggestions" | A subagent's `memory` field silently grants Write/Edit, which would let a reviewer modify the file under review. Saving through the main session keeps reviewers read-only while still accumulating experience in `.claude/agent-memory/`. |
| Templates live next to SKILL.md | Loaded into context only when the skill runs; no standing token cost. |
| Reviewers run in subagents | Long output (diffs, test logs) stays in the subagent's context; the main session only gets the conclusion. |
| `acceptance-verifier` is separate from the code author | The verifier does not grade its own work and must give file:line evidence. |
| Backend fixed to .NET Clean Architecture; frontend is a choice between Next.js and React + Vite | One known stack means one scaffold, one architecture document and checkable layer rules, instead of every plan re-deciding the structure. |
| `docs/architecture.md` is written before anything is installed, and loaded into every session | The dependency guard reads its table, and agents extend the structure it describes instead of inventing one, so code does not need refactoring later. |
| The dependency allowlist is a markdown table read by a hook | Humans review it like any document; the hook enforces it on `npm`/`npx`/`dotnet add` and on every manifest edit. Generators run only inside `/sdd-scaffold`. |
| Layer rules are checked twice: by the hook (project references) and by `ArchitectureTests` (compiled code) | The hook stops the wrong reference before it exists; the test catches what a reference check cannot see, such as a forbidden namespace. |
| `/sdd-scaffold` and `/sdd-architecture` tokens last only until the next prompt | Typing the command is a permission for that one run, not a 15-minute window in which anything goes. |
| External actions are split into deny (push, publish, deploy, infrastructure) and ask (everything else that leaves the machine) | Irreversible or shared-environment actions stay with the human; everyday ones such as a GitHub comment or a `curl` stay possible, but only with an explicit confirmation. In a compound command the strictest part wins. |
| `software-engineer` writes the code; the main session checks it with `test-runner` and commits | The implementer works in its own context with one task at a time; the orchestrator does not grade the work on trust, and commits stay one per task. |
| Models by role: Opus for review/verification/security, Sonnet for implementation, code review and e2e, Haiku for test runs | Balances quality and cost. Change the `model` field if you like (Opus for `software-engineer` on complex domains). |
| `/sdd-status` and `/sdd-analyze` are the only skills Claude may invoke on its own | Both are read-only; asking naturally "what's left?" or "do the documents agree?" just works. |

### Known limitations

- Hooks are a guard rail against mistakes, not a security boundary: a shell can write files in many ways the hooks cannot anticipate (e.g. code generated by `npm init` or a custom script).
- Files outside the project folder are not covered by the code gate.
- The external guard reads command lines: a script file, a test, or a program the build runs can still reach the network without the hook seeing it. Shell commands are parsed with simple rules (quotes, `&&`, `;`, `|`, nested `powershell -Command` / `bash -c`), not a full shell parser.
- `WebFetch` and `WebSearch` are allowed (read-only); a URL can still carry data in its query string.
- The dependency guard checks package names, not versions; version policy lives in `docs/architecture.md` and code review.
- The approval gate relies on the `UserPromptSubmit` hook (with the transcript as fallback). The token is valid for 15 minutes; if it expires, type `/sdd-approve` again.
- Not implemented: running `[P]` tasks in parallel in worktrees (a worktree's branch does not match `feat/NNN-slug`, so it hits the code gate, and merging results easily breaks TDD order), and plugin packaging (worth doing once the kit has been stable for a few features).

## Common customizations

**Rename commands**: rename the skill folder (or its `name` field). Avoid clashing with built-ins such as `/plan`, `/tasks`, `/verify`, `/init`; that is why the kit uses the `sdd-` prefix.

**Add folders that can be edited freely** (outside the code gate): edit the `ALWAYS_ALLOWED` array in `.claude/hooks/sdd-guard-edit.js`.

**Temporarily disable the code gate** (e.g. a spike or experiment): create `.claude/settings.local.json`:

```json
{ "env": { "SDD_GATE": "off" } }
```

The approval gate stays active. Remove the line when done. `"SDD_DEPS": "off"` and `"SDD_EXTERNAL": "off"` switch off the dependency guard and the external guard the same way; use them only for a deliberate exception.

**Tune the external guard**: the command lists (`HTTP_CLIENTS`, `CLOUD`, `PLATFORMS`, `READ_VERBS`…) and the local-host pattern are at the top of `.claude/hooks/sdd-guard-external.js`. Move a command from deny to ask by changing `D(` to `A(` on its line. MCP tools whose name starts with a read verb (`get`, `list`, `search`…) pass; every other MCP tool asks.

**Add a stack element** (another layer rule, a new project kind): the layer table is `LAYER_REFS` in `.claude/hooks/lib/deps.js`; keep it in sync with the "Backend layers" table in `docs/architecture.md` and with `ArchitectureTests`.

**Add an auto-format hook** (in a `PostToolUse` array in `.claude/settings.json`; this example uses Prettier, adapt it to your stack):

```json
"PostToolUse": [
  {
    "matcher": "Edit|Write",
    "hooks": [
      { "type": "command",
        "command": "node -e \"const i=JSON.parse(require('fs').readFileSync(0,'utf8'));require('child_process').spawnSync('npx',['prettier','--write','--ignore-unknown',i.tool_input.file_path],{stdio:'ignore',shell:true})\"" }
    ]
  }
]
```

**Attach a browser to `e2e-tester`**: `claude mcp add playwright -- npx @playwright/mcp@latest`, then add the names of the `mcp__playwright__…` tools you need to the `tools:` line of `.claude/agents/e2e-tester.md`.

**Add a knowledge skill** for your own conventions (Claude uses it when relevant; you never invoke it):

```markdown
---
name: api-conventions
description: The project's API design conventions. Use when writing or modifying endpoints.
user-invocable: false
---
- REST, plural nouns: /users/{id}
- Errors: { "error": { "code": "...", "message": "..." } }
```

**Use it in every project**: copy `.claude/skills/sdd-*`, `.claude/agents/*` and `.claude/hooks/` into the new project (hooks are registered at project level, so `settings.json` is needed too). Each project still needs its own `CLAUDE.md`.

## Improving the kit over time

- Run `/sdd-retro` every 2–3 features: turn recurring problems from `.claude/agent-memory/` and `verification.md` into checklists, constitution principles or hooks.
- When you find yourself typing the same instruction repeatedly, turn it into a skill or add it to the relevant SKILL.md.
- Any rule that must hold 100% of the time belongs in a hook, not just an instruction.
- Run `/doctor` periodically to check that CLAUDE.md is not growing too large.
