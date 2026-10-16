---
name: sdd-clarify
description: Resolve ambiguities in the current feature's Draft spec.md (or fix.md), or in a Draft requirements document (RNNN), by asking the user one question at a time and updating the document right after each answer.
disable-model-invocation: true
argument-hint: "[NNN | specs/NNN-slug | RNNN]"
---

# Clarify the spec

Arguments: $ARGUMENTS

## Steps

1. Determine the document:
   - Arguments contain `RNNN`: the requirements document `docs/requirements/RNNN-slug/requirements.md` (also read its `input.md`).
   - Otherwise: the current feature per "SDD conventions" in `CLAUDE.md`, with `spec.md` (or `fix.md` for a fix).
   Read it and `docs/constitution.md`.
   - If the document is already `Approved`: do not edit it. Explain that changes after approval must go through `/sdd-amend <spec|requirements> <change>` so the Revision is bumped and the status cascades to the documents below it.

2. Scan the document and list what needs clarifying:
   - Every `[NEEDS CLARIFICATION: …]` marker and the "Open questions" section.
   - Unmeasurable adjectives: "fast", "easy", "secure", "many"…
   - Error flows, edge cases, permissions and limits that are not addressed.
   - Acceptance scenarios that cannot be verified or contradict each other.
   - Anything that might violate the constitution.
   - For requirements also: business rules that are not atomic or testable, terms used but missing from the glossary, backlog items of size `L` or with a fuzzy boundary between two items, P1 items that do not form a usable MVP, goals or rules covered by no backlog item.

3. Rank by impact on the design (scope > security/data > user experience > minor detail). Pick **at most 5** of the most important questions for this round.

4. Ask **one question at a time**, never batching several in one turn:
   - Offer 2–4 short options per question, mark the recommended one with a one-line reason.
   - Use the multiple-choice question tool if available (the user can always answer freely).
   - If the user says "your call" or "go with the recommendation", use the recommended option.

5. Right after each answer:
   - Update the document in the right section (FR, NFR, scenario, edge case, out of scope…; for requirements: BR, QE, glossary, process, backlog item, traceability). Write it as if it had always been there, not as a pasted answer. In requirements, set the BR's source to `clarification <date>`.
   - Remove the corresponding marker and tick the open question.
   - Add a row to the "Clarification log" table (fix.md has no such table; skip it there).

6. Finish:
   - Keep `**Status:** Draft` and do not change `Revision` (Revision only increases when an approved document is amended).
   - Commit: `docs(NNN): clarify spec` (for requirements: `docs(RNNN): clarify requirements`, on `main`).
   - Report how many points were resolved and how many remain. If important ones remain, suggest running `/sdd-clarify` again; otherwise suggest reading the document and running `/sdd-approve spec` (or `/sdd-approve fix`, or `/sdd-approve requirements RNNN`, which also generates the specs).
