---
name: sdd-scaffold
description: Create the project skeleton in src/ — a .NET Clean Architecture backend (fixed) and a Next.js or React frontend (user's choice) — and write docs/architecture.md, the structure and dependency allowlist every later implementation must follow. Run once, after /sdd-constitution and before the first /sdd-plan.
disable-model-invocation: true
argument-hint: "[project name]"
---

# Scaffold the project

Arguments: $ARGUMENTS

Typing this command is the user's permission for this run only: while it runs, the hooks let Claude write under `src/` on `main`, run the official generators (`create-next-app`, `create-vite`) and write `docs/architecture.md`. The permission ends at the user's next prompt. If a hook blocks partway (the user typed something else in between): tell the user to type `/sdd-scaffold` again; this skill resumes where it stopped.

Fixed choices, not asked: backend = **.NET + Clean Architecture** (Domain / Application / Infrastructure / Api, xUnit); frontend = **TypeScript**; unit tests = **Vitest + Testing Library**.

## Steps

1. **Preconditions** (in parallel):
   - Git repo (otherwise `/sdd-init`); `git status --porcelain` empty (otherwise ask); on `main` (switch if needed).
   - `docs/constitution.md` is not a placeholder (otherwise suggest `/sdd-constitution` and ask whether to continue).
   - `dotnet --list-sdks`, `node --version`, `npm --version`. Missing .NET SDK or Node < 20: stop and tell the user what to install. Never install it yourself (the hooks block system installers).
   - **Resume or stop**: if `docs/architecture.md` has `**Status:** Active`, the project is already scaffolded: stop and suggest `/sdd-architecture <change>`. If it has `**Status:** Scaffolding`: read it for the earlier choices, skip the question round, and continue from the first step whose output is missing.

2. **Ask the user in ONE turn** (multiple-choice question tool, four questions; recommended option first):
   - **Frontend**: Next.js (App Router) *(recommended when pages need SEO or server rendering)* · React + Vite *(recommended for an internal SPA)*.
   - **Database**: PostgreSQL · SQL Server · SQLite · Decide later (no persistence packages yet).
   - **Styling**: Tailwind CSS · CSS Modules (no extra package).
   - **E2E tests**: Playwright · None for now.

   Also determine without asking: the project name from the arguments, else from the title of `CLAUDE.md`, as PascalCase without spaces (`<Name>`, e.g. `LeaveManager`); the .NET version = the highest installed SDK with an even major version (LTS), else the highest installed. Mention both in the report.

3. **Write `docs/architecture.md` first** from [architecture-template.md](architecture-template.md), `**Status:** Scaffolding`, today's date from `date +%F`. The dependency guard reads its "Allowed dependencies" table, so it must exist before anything is installed.
   - Keep only the chosen frontend variant; delete the rows marked for the other one.
   - Database rows (Allowed in → Infrastructure, unless noted): PostgreSQL → `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design` (Api), `Npgsql.EntityFrameworkCore.PostgreSQL`, `Testcontainers.PostgreSql` (Tests). SQL Server → the same with `Microsoft.EntityFrameworkCore.SqlServer` and `Testcontainers.MsSql`. SQLite → `Microsoft.EntityFrameworkCore`, `Microsoft.EntityFrameworkCore.Design` (Api), `Microsoft.EntityFrameworkCore.Sqlite` (Infrastructure, Tests). Decide later → no rows.
   - Styling: Tailwind → `tailwindcss`, `@tailwindcss/postcss` (dev). E2E: Playwright → `@playwright/test` (dev).

4. **Backend** under `src/backend/` (`<v>` = the chosen .NET major, e.g. `net10.0`):
   ```
   dotnet new sln -n <Name> -o src/backend
   dotnet new classlib -n <Name>.Domain         -o src/backend/src/<Name>.Domain         -f <v>
   dotnet new classlib -n <Name>.Application    -o src/backend/src/<Name>.Application    -f <v>
   dotnet new classlib -n <Name>.Infrastructure -o src/backend/src/<Name>.Infrastructure -f <v>
   dotnet new webapi   -n <Name>.Api            -o src/backend/src/<Name>.Api            -f <v>
   dotnet new xunit    -n <Name>.Domain.Tests          -o src/backend/tests/<Name>.Domain.Tests          -f <v>
   dotnet new xunit    -n <Name>.Application.Tests     -o src/backend/tests/<Name>.Application.Tests     -f <v>
   dotnet new xunit    -n <Name>.Api.IntegrationTests  -o src/backend/tests/<Name>.Api.IntegrationTests  -f <v>
   dotnet new xunit    -n <Name>.ArchitectureTests     -o src/backend/tests/<Name>.ArchitectureTests     -f <v>
   ```
   Then `dotnet sln src/backend add` every project, and the references exactly as the layer table allows: Application → Domain; Infrastructure → Application; Api → Application, Infrastructure; each test project → the project it tests (ArchitectureTests → all four). Then:
   - `src/backend/global.json` (pin the SDK, `rollForward: latestFeature`), `Directory.Build.props` (`Nullable` enable, `ImplicitUsings` enable, `TreatWarningsAsErrors` true), `.editorconfig`.
   - Remove the template samples (`Class1.cs`, `WeatherForecast`, `UnitTest1.cs`). Add the minimum that proves the wiring: `Domain/Common/Result.cs`; `Application/DependencyInjection.cs` (`AddApplication()`); `Infrastructure/DependencyInjection.cs` (`AddInfrastructure(IConfiguration)`); `Api/Program.cs` calling both, ProblemDetails, a `GET /health` endpoint in `Api/Endpoints/HealthEndpoints.cs`, and `public partial class Program;` for tests.
   - Packages with `dotnet add <project> package <name>`: NSubstitute (unit test projects), `Microsoft.AspNetCore.Mvc.Testing` (IntegrationTests), `NetArchTest.Rules` (ArchitectureTests), `Microsoft.Extensions.DependencyInjection.Abstractions` (Application, Infrastructure), and the database rows, if any.
   - Tests: an integration test that `GET /health` returns 200; architecture tests asserting the layer table (Domain depends on nothing, Application not on Infrastructure/Api, Infrastructure not on Api).

5. **Frontend** under `src/frontend/`:
   - Next.js: `npx create-next-app@latest src/frontend --ts --eslint --app --src-dir --import-alias "@/*" --use-npm --yes` plus `--tailwind` or `--no-tailwind` (`--yes` takes the defaults for anything not given, so the generator never waits for input).
   - React + Vite: `npm create vite@latest src/frontend -- --template react-ts`, then `npm --prefix src/frontend install`, then `npm --prefix src/frontend install react-router`.
   - Unit tests: `npm --prefix src/frontend install -D vitest @vitejs/plugin-react jsdom @testing-library/react @testing-library/dom @testing-library/jest-dom @testing-library/user-event vite-tsconfig-paths`; `vitest.config.ts` (jsdom, setup file importing `@testing-library/jest-dom/vitest`).
   - E2E (if chosen): `npm --prefix src/frontend install -D @playwright/test`, a `playwright.config.ts` and `tests/e2e/smoke.spec.ts`. Do not run `npx playwright install` (it downloads browsers): give the user the command instead.
   - Create the folders of the chosen variant in `architecture.md` (`features/`, `shared/ui`, `shared/lib/api-client.ts`, `shared/config`); replace the generator's home page with a minimal page; add one smoke test of it.
   - `package.json` scripts: `test` = `vitest run`, `typecheck` = `tsc --noEmit`, `lint` (keep the generator's), `test:e2e` = `playwright test` if chosen.
   - Dev wiring to the backend as the variant describes (Next.js rewrites or Vite `server.proxy` for `/api`).

6. **Reconcile the allowlist**: read `src/frontend/package.json` and every `.csproj`. Every package there must have a row in "Allowed dependencies". The generators chose some packages themselves (ESLint plugins, `@eslint/js`, `globals`, `typescript-eslint`…): add a row for each, Purpose "installed by <generator>". Never add a row for something you chose to install outside steps 4–5.

7. **Prove it works**: `dotnet build src/backend` and `dotnet test src/backend`, `npm --prefix src/frontend run lint`, `run typecheck`, `test`, via the `test-runner` subagent. Everything must be green; fix and rerun (at most 3 rounds) before going on.

8. **Wire the kit**:
   - `.gitignore`: add `bin/`, `obj/`, `node_modules/`, `.next/`, `dist/`, `coverage/`, `playwright-report/`, `test-results/`, `*.user` if missing; never remove lines.
   - `CLAUDE.md` "Project commands": Install `dotnet restore src/backend && npm --prefix src/frontend ci`; Run tests `dotnet test src/backend && npm --prefix src/frontend test`; Single test `dotnet test src/backend --filter "FullyQualifiedName~<TestName>"` / `npm --prefix src/frontend test -- <file>`; Lint / typecheck `dotnet build src/backend && npm --prefix src/frontend run lint && npm --prefix src/frontend run typecheck`; Run dev `dotnet run --project src/backend/src/<Name>.Api` and `npm --prefix src/frontend run dev`; Run e2e `npm --prefix src/frontend run test:e2e` (or empty).
   - `.claude/settings.json` `allow`: `Bash(dotnet build *)`, `Bash(dotnet test *)`, `Bash(dotnet restore *)`, `Bash(npm --prefix src/frontend run *)`, `Bash(npm --prefix src/frontend test *)`, `Bash(npm --prefix src/frontend ci)`. Keep `deny` and `hooks` intact.
   - Set `**Status:** Active` in `docs/architecture.md`.

9. **Commit** (ask first): `chore: scaffold .NET Clean Architecture backend and <Next.js|React> frontend`.

10. **Report**: the tree under `src/` (two levels), .NET version and project name, the choices, test results, the number of allowed dependencies, the manual follow-ups (e.g. `npx playwright install`, a local database). Next step: read `docs/architecture.md` carefully (it binds every agent from now on), then `/sdd-ba <requirements>` or `/sdd-spec <feature>`.

## Rules

- Never install anything that is not in step 3's rows or the generators' own output. If a generator fails or asks something interactive, stop and report; do not switch to another tool.
- Build no business features here. Only the wiring that the smoke tests prove.
