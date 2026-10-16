# Architecture — Shop Management

**Version:** 1.0.0
**Status:** Active
**Created:** 2026-10-16
**Last amended:** 2026-10-16
**Backend:** .NET 10 · Clean Architecture
**Frontend:** React + Vite · TypeScript
**Database:** SQLite

This document tells every agent **where code goes and what it may depend on**. Implementation extends the structure below; it never invents a new one. It changes only through `/sdd-architecture <change>`, typed by the user. The hooks read the "Allowed dependencies" table: adding any package that is not in it is blocked.

Precedence: `docs/constitution.md` > this document > requirements > spec > plan > tasks > code.

## Overview

```
src/
├── backend/                       .NET solution (Clean Architecture)
│   ├── ShopManagement.sln         (or .slnx, whichever the SDK creates)
│   ├── global.json                pins the SDK
│   ├── Directory.Build.props      shared settings: Nullable, ImplicitUsings, warnings as errors
│   ├── src/
│   │   ├── ShopManagement.Domain/
│   │   ├── ShopManagement.Application/
│   │   ├── ShopManagement.Infrastructure/
│   │   └── ShopManagement.Api/
│   └── tests/
│       ├── ShopManagement.Domain.Tests/
│       ├── ShopManagement.Application.Tests/
│       ├── ShopManagement.Api.IntegrationTests/
│       └── ShopManagement.ArchitectureTests/
└── frontend/                      React + Vite app
```

Request flow: browser → frontend → HTTP JSON → `Api` endpoint → `Application` use case → `Domain` model; `Infrastructure` implements the interfaces `Application` declares (database, mail, clock, external APIs).

## Backend layers

| Layer | Contains | May reference | Must not |
|-------|----------|---------------|----------|
| `Domain` | Entities, value objects, domain events, domain errors, business invariants | nothing | reference any project or package; know about HTTP, databases or DI |
| `Application` | Use cases (one handler class per command/query), interfaces for what it needs (`IRepository`, `IClock`, `IPasswordHasher`…), DTOs, validation, `DependencyInjection.cs` | `Domain` | reference `Infrastructure` or `Api`; use EF Core, HttpContext or concrete services |
| `Infrastructure` | Implementations of Application interfaces: persistence (DbContext, configurations, migrations, repositories), password hashing, clock, external clients, `DependencyInjection.cs` | `Application`, `Domain` | contain business rules; be referenced by `Application` or `Domain` |
| `Api` | `Program.cs` (composition root), endpoints, request/response contracts, ProblemDetails error mapping, auth setup | `Application`, `Infrastructure` (registration only), `Domain` | contain business rules or data access; call `DbContext` directly |
| `Tests` | xUnit test projects | anything | ship in production code |

The hooks block a `ProjectReference` or `dotnet add reference` that breaks this table. `ShopManagement.ArchitectureTests` checks the same rules on the compiled code (namespaces and types), so a violation also fails the test suite.

### Backend conventions

- **Feature folders inside each layer**: `Application/<Feature>/` holds `<Action>Command.cs` / `<Action>Query.cs`, its handler and validator; `Api/Endpoints/<Feature>Endpoints.cs` maps the routes with `MapGroup`. Example: `Application/Products/CreateProduct/CreateProductHandler.cs`.
- **Use cases are plain classes** registered in DI by `Application/DependencyInjection.cs` (`AddApplication()`); no mediator library.
- **Endpoints**: Minimal APIs, thin: bind → call one handler → map the result to HTTP. Routes are plural nouns: `/api/products/{id}`.
- **Errors**: handlers return a `Result<T>` (in `Domain/Common/`) instead of throwing for expected failures; `Api` maps them to RFC 7807 ProblemDetails. Exceptions are for bugs only.
- **Configuration**: strongly typed options classes, bound in `Program.cs`; secrets come from user-secrets or environment variables, never from committed files.
- **Persistence**: EF Core `AppDbContext` in `Infrastructure/Persistence/`, one `IEntityTypeConfiguration<T>` per entity, migrations in `Infrastructure/Persistence/Migrations/`. The SQLite connection string comes from configuration (`ConnectionStrings:Default`); integration tests use an in-memory SQLite connection.
- **Naming**: namespaces follow folders; async methods end in `Async`; one public type per file.
- **Tests**: unit tests for Domain and Application (mock interfaces with NSubstitute); integration tests through `WebApplicationFactory<Program>` in `Api.IntegrationTests`; test names `Method_State_ExpectedResult`.

## Frontend

### Variant: React + Vite

```
src/frontend/
├── src/
│   ├── main.tsx
│   ├── app/                       router.tsx, providers.tsx, layout
│   ├── features/<feature>/        components/, api.ts, hooks.ts, types.ts, __tests__/
│   └── shared/
│       ├── ui/
│       ├── lib/api-client.ts      the only place that calls the backend
│       └── config/                env access (import.meta.env.VITE_* read here only)
├── tests/e2e/                     Playwright specs
└── vite.config.ts                 dev proxy /api → backend
```

- Styling: Tailwind CSS through PostCSS (`postcss.config.js` + `@import "tailwindcss"` in `src/index.css`); no other CSS framework.

### Frontend conventions

- A feature never imports from another feature's folder; shared code moves to `shared/`.
- Components are function components in `PascalCase.tsx`; hooks start with `use`; one component per file.
- All HTTP goes through `shared/lib/api-client.ts` (fetch based); errors are ProblemDetails from the backend.
- Tests: Vitest + Testing Library next to the feature (`__tests__/`); query by role and label, not by class name.
- E2E: Playwright specs in `tests/e2e/`, run with `npm run test:e2e`.

## Allowed dependencies

Only these packages may be added. "Allowed in": .NET layer(s) for `nuget` (`Domain`, `Application`, `Infrastructure`, `Api`, `Tests`, `tool`), `runtime` / `dev` for `npm`, or `*` for anywhere. A trailing `*` matches a prefix. Versions: latest stable at the time they are added; upgrades of a major version go through `/sdd-architecture`.

| Package | Ecosystem | Allowed in | Purpose |
|---------|-----------|------------|---------|
| `Microsoft.AspNetCore.OpenApi` | nuget | Api | OpenAPI document for the endpoints |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | nuget | Application, Infrastructure | `AddApplication()` / `AddInfrastructure()` extension methods |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | nuget | Infrastructure | Strongly typed options |
| `Microsoft.NET.Test.Sdk` | nuget | Tests | Test host |
| `xunit` | nuget | Tests | Test framework |
| `xunit.v3` | nuget | Tests | Test framework (if the SDK template uses v3) |
| `xunit.runner.visualstudio` | nuget | Tests | Test runner |
| `coverlet.collector` | nuget | Tests | Coverage |
| `NSubstitute` | nuget | Tests | Mocks for Application interfaces |
| `Microsoft.AspNetCore.Mvc.Testing` | nuget | Tests | `WebApplicationFactory` integration tests |
| `NetArchTest.Rules` | nuget | Tests | Architecture tests for the layer rules |
| `Microsoft.EntityFrameworkCore` | nuget | Infrastructure | ORM |
| `Microsoft.EntityFrameworkCore.Design` | nuget | Api | EF Core migrations tooling |
| `Microsoft.EntityFrameworkCore.Sqlite` | nuget | Infrastructure, Tests | SQLite provider |
| `react` | npm | runtime | UI library |
| `react-dom` | npm | runtime | UI library |
| `react-router` | npm | runtime | Routing |
| `typescript` | npm | dev | Type checking |
| `@types/*` | npm | dev | Type definitions |
| `oxlint` | npm | dev | Lint (installed by create-vite) |
| `vite` | npm | dev | Build tool, also used by Vitest |
| `@vitejs/plugin-react` | npm | dev | React support for Vite / Vitest |
| `vitest` | npm | dev | Unit test runner |
| `jsdom` | npm | dev | DOM for unit tests |
| `@testing-library/react` | npm | dev | Component tests |
| `@testing-library/dom` | npm | dev | Component tests |
| `@testing-library/jest-dom` | npm | dev | DOM matchers |
| `@testing-library/user-event` | npm | dev | User interaction in tests |
| `tailwindcss` | npm | dev | Styling |
| `@tailwindcss/postcss` | npm | dev | Tailwind PostCSS plugin |
| `@playwright/test` | npm | dev | E2E tests |

## Forbidden

- Any package not in the table above, including "small" helpers; copy-pasted library code; downloading binaries or scripts.
- Global installs (`npm -g`, `dotnet tool install -g`), system installers (`winget`, `choco`…), other ecosystems (`pip`, `cargo`…).
- A second way of doing something the stack already does (another HTTP client, state library, test framework, ORM, mapper).
- Business logic in `Api` or in the frontend.
- New top-level folders under `src/` or new projects in the solution without an architecture change.

## Change process

1. Need something outside this document (a package, a layer rule, a new project, a convention)? Stop and propose `/sdd-architecture "<change and reason>"`.
2. The user runs it. Claude shows the impact, the user confirms, the Version is bumped (new dependency or convention = minor, removed dependency or changed layer rule = major, wording = patch) and a Changelog row is added.
3. Plans recording an older `**Architecture:**` version are listed for `/sdd-analyze`.

## Changelog

| Version | Date | Change |
|---------|------|--------|
| 1.0.0 | 2026-10-16 | Scaffold: .NET Clean Architecture backend, React + Vite frontend, SQLite |
