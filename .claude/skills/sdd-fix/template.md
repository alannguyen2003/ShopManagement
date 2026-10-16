# Fix: <Short bug name>

**Feature:** NNN-slug
**Branch:** fix/NNN-slug
**Status:** Draft
**Created:** <YYYY-MM-DD>
**Related:** <specs/MMM-slug of the original feature, if any>

## Symptom

<What the user sees going wrong. Error message, logs, screenshots if available.>

## Steps to reproduce

1. <step>
2. <step>

**Actual result:** <…>

## Expected behavior

1. **Given** <state>, **When** <action>, **Then** <correct outcome>

<If the original feature has a spec: cite the FR/scenario being violated, e.g. "violates FR-003 of specs/002-…".>

## Root cause

<Short explanation with `file:line`. If not certain: write `[NEEDS CLARIFICATION: …]` and how to confirm it.>

## Fix scope

- `<path/to/file>` — <what changes>

No new libraries, no data model changes, no public interface changes. If any of those are needed this is not a fix: use `/sdd-spec` or `/sdd-amend`.

## Risks and impact

- <Other places that use the same code; how to check for regressions.>

## Tasks

- [ ] T001 Write a regression test reproducing the bug, confirm it FAILS for the right reason — <tests/...>
- [ ] T002 Fix <component> so T001 passes — <src/...>
- [ ] T003 Run the full test suite and lint, confirm no regressions

**Checkpoint:** T001 passes; full test suite and lint green.
