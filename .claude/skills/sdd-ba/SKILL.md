---
name: sdd-ba
description: Business analysis of raw requirements (free text or files) into docs/requirements/RNNN-slug/requirements.md — actors, goals, business rules, glossary, processes and a backlog of spec-sized features. Once the user approves it, specs are generated for every backlog item.
disable-model-invocation: true
argument-hint: "<raw requirements text and/or file paths>"
---

# Analyze business requirements

Raw requirements from the user: $ARGUMENTS

If the arguments are empty: ask the user to paste the requirements or give the path of a file before continuing.

## Steps

1. **Prepare**
   - If the project is not a git repo: stop and suggest `/sdd-init`.
   - Read `docs/constitution.md`. If it is still a placeholder: suggest running `/sdd-constitution` first and ask whether to continue anyway.
   - Requirements live on the main branch. If the current branch is not `main`: check `git status`; if there are uncommitted changes, ask the user before switching; then switch to `main`.
   - List `docs/requirements/` to get the next `RNNN` (highest existing + 1; `R001` if none). Choose a `slug` of 2–4 English words, kebab-case.
   - If the request is really one small feature: suggest `/sdd-spec` instead. If it is a bug: suggest `/sdd-fix`. If it extends an existing requirements document: suggest `/sdd-amend requirements RNNN <change>`. Ask before continuing.

2. **Capture the input** in `docs/requirements/RNNN-slug/input.md`, verbatim, never paraphrased:
   - Header: `# Input: <initiative name>`, `**Received:** <date from date +%F>`, `**From:** <requester if known>`.
   - Pasted text goes in as is. For each file given: copy its text content under a `## <file name>` heading (read PDFs page by page; for formats you cannot read, ask the user to paste the text) and keep the original path.

3. **Analyze**: hand the analysis to the `business-analyst` subagent in mode `analyze`, giving it the path of `input.md` and of [template.md](template.md).

4. **Write `docs/requirements/RNNN-slug/requirements.md`** from the agent's draft. Check before writing:
   - Header: `**Requirements:** RNNN-slug`, `**Status:** Draft`, `**Revision:** 1`, created date from `date +%F`.
   - Every backlog row has a priority, a size `S` or `M`, and `—` in the `Spec` column.
   - Each question the agent ranked is either a `[NEEDS CLARIFICATION: …]` marker in place or under "Open questions".
   - Report the agent's "Overlaps with existing work" to the user in step 7.

5. **Self-review**: hand the file to the `spec-reviewer` subagent. Fix "Critical" findings that need no decision from the user; put those that do under "Open questions". Save the "Memory suggestions" of both agents per the convention in `CLAUDE.md` (`.claude/agent-memory/business-analyst/MEMORY.md` and `.claude/agent-memory/spec-reviewer/MEMORY.md`).

6. **Commit** `input.md`, `requirements.md` and any memory files: `docs(RNNN): draft requirements`.

7. **Report** to the user, briefly:
   - File path.
   - The initiative in 3–5 lines; actors; number of business rules.
   - The backlog summary table (ID, feature, priority, size, depends on).
   - Overlaps with existing work, if any.
   - Number of open questions.
   - Next step: `/sdd-clarify RNNN` if there are open questions; otherwise read the document and run `/sdd-approve requirements RNNN`. Remind the user that approving generates one spec (and one branch) per backlog item, so the backlog should be what they want built.
