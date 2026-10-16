# Requirements: <Initiative name>

**Requirements:** RNNN-slug
**Status:** Draft
**Revision:** 1
**Created:** <YYYY-MM-DD>
**Requested by:** <person or group>
**Input:** [input.md](input.md)

## Context

<The business problem, who has it, why now, what success looks like. 3–8 lines.>

## Stakeholders and actors

| Actor / stakeholder | Role | Goals | Interaction with the system |
|---------------------|------|-------|-----------------------------|
| <Actor> | <role> | <what they want> | <uses / approves / receives / none> |

## Business goals

- **BG-01:** <Measurable outcome, e.g. "reduce leave-request handling time from 3 days to 1 day">

## Scope

### In scope

- <Capability area>

### Out of scope

- <What this initiative deliberately does NOT do, and why>

## Glossary

| Term | Definition | Do not call it |
|------|------------|----------------|
| <Term> | <meaning in this business> | <synonyms to avoid> |

## Business processes

### PR-01 — <Process name>

1. <Actor> <does something>.
2. The system <responds>.

**Exceptions:**
- <Condition> → <what happens>

## Business rules

- **BR-001:** <Atomic, testable rule>. *(Source: input §… / assumption / clarification YYYY-MM-DD)*

## Quality expectations

- **QE-01:** <Performance / availability / security / accessibility / volume… with a number>

## Constraints

- <Imposed by the requester or the environment: deadline, regulation, existing system, technology mandate. Source.>

## Assumptions

- <Default chosen and why.>

## Risks and dependencies

- <Risk or external dependency> → <mitigation or owner>

## Backlog

### Summary

| ID | Feature | Priority | Size | Depends on | Covers | Spec |
|----|---------|----------|------|------------|--------|------|
| BL-01 | <short name> | P1 | M | — | BG-01, BR-001, BR-002 | — |

Size: `S` ≤ 1 day, `M` 1–3 days. The `Spec` column holds the reserved `NNN-slug` once `/sdd-ba-spec` creates the spec; `—` until then.

### BL-01 — <Short name>

**Value:** <who benefits and how>

**Actors:** <actors>

**Spec brief:** <3–6 lines a spec can start from alone: who, what, why, main flow, key exceptions.>

**Key scenarios:**
- <Main happy path>
- <Important exception>

**Not included:** <what is left to other items (BL-…) or out of scope>

## Traceability

| Goal / rule / quality | Backlog items |
|-----------------------|---------------|
| BG-01 | BL-01 |
| BR-001 | BL-01 |

## Open questions

- [ ] <Question> (affects: BR-003, BL-02)

## Clarification log

| Date | Question | Decision |
|------|----------|----------|

## Revision history

| Rev | Date | Change | Affected items |
|-----|------|--------|----------------|
| 1 | <YYYY-MM-DD> | Initial version | — |
