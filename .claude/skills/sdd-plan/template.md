# Plan: <Feature name>

**Feature:** NNN-slug
**Spec:** [spec.md](spec.md)
**Status:** Draft
**Revision:** 1
**Spec revision:** <current Revision of spec.md>
**Constitution:** <current Version of docs/constitution.md>
**Architecture:** <current Version of docs/architecture.md>
**Created:** <YYYY-MM-DD>

## Summary

<Main requirement and technical approach in 3–5 lines.>

## Technical context

| Item | Choice |
|------|--------|
| Language / version | |
| Main framework | |
| Storage | |
| New libraries | (each with a reason) |
| Test tooling | |
| Target platform | |
| Performance / scale constraints | (from NFRs) |

## Constitution check

| Principle | Result | Notes / justification |
|-----------|--------|-----------------------|
| P1. … | Pass | |
| P2. … | Violation | <why it is needed, why the simpler alternative was rejected> |

## Architecture check

Against [docs/architecture.md](../../docs/architecture.md) v<version>.

| Item | Value |
|------|-------|
| Backend layers touched | <Domain / Application / Infrastructure / Api> |
| Feature folders | <e.g. Application/LeaveRequests/, Api/Endpoints/LeaveRequestEndpoints.cs, frontend features/leave-requests/> |
| Layer rules | Respected / <the exception and why it is not one> |

| Package | Layer / scope | Status |
|---------|---------------|--------|
| <name> | <Infrastructure> | allowed |
| <name> | <Application> | requires /sdd-architecture |

Dependencies requiring /sdd-architecture: <none, or the exact command for each; the plan is not approved until they are allowed>

## Design

<Components, responsibility of each, main data flow. Mermaid diagrams welcome.>

## Requirement → design mapping

| Requirement | Responsible component | Notes |
|-------------|-----------------------|-------|
| FR-001 | | |
| NFR-001 | | |

## File structure

```
<paths to create / modify, following the repo's existing conventions>
```

## Supporting documents

- [research.md](research.md) — if any
- [data-model.md](data-model.md) — if the feature has data
- [contracts/](contracts/) — if there are interfaces

## Test strategy

| Acceptance scenario | Test type | Test description |
|---------------------|-----------|------------------|
| US1 #1 | integration | |

## Risks and mitigations

- <Risk> → <mitigation>

## Implementation notes

<Sensible order, dependencies between parts, migrations, feature flags if needed.>

## Revision history

| Rev | Date | Spec rev | Change |
|-----|------|----------|--------|
| 1 | <YYYY-MM-DD> | 1 | Initial version |
