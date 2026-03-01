# GamePlanPro

SaaS application. Solo dev. See `STACK.md` for full stack decisions and rationale.

## Project Structure

- `Api/` — ASP.NET Core 9 Web API
- `ClientApp/` — Angular 21 + Nx 22
- `docker-compose.yml` — local Postgres 16
- `GamePlanPro.sln` — links `Api/Api.csproj`
- `generate-client.sh` — regenerates Kiota TypeScript client from OpenAPI spec

## Code Patterns

### C# / ASP.NET Core

- **Service registration:** Always use service extension classes in `Api/Extensions/` — never register services inline in `Program.cs`
- **Entity configuration:** Use DataAnnotation attributes on models (e.g. `[Required]`, `[MaxLength]`, `[Table]`). Only use `IEntityTypeConfiguration<T>` nested `Configuration` class for complex config (relationships, seed data, enum conversions, etc.)
- **Secrets:** Connection strings and API keys go in `dotnet user-secrets` for local dev — never in `appsettings*.json`
- **Namespaces:** File-scoped namespaces only
- **API style:** Controllers-based (not minimal API)
- **Architecture:** Controller → Service → Repository pattern
  - Controllers: thin, inject services only, no direct DbContext/repository access
  - Services: contain all business logic, inject repositories, handle validation and orchestration
  - Repositories: entity-specific, pure CRUD, inject `AppDbContext` directly
  - DI: register all repositories and services in `ApplicationServiceExtensions`
- **Base entity:** All domain entities inherit `BaseEntity` (int `Id` PK + Guid `Key` for external use)
- **Interfaces:** Define in the same file as the implementation. Only break out to a separate file if multiple implementations exist.
- **Mappers:** `{ModelName}Mapper.cs` files in `Api/Models/Mappers/` with static extension methods off the model/DTO they map from (e.g. `drill.ToResponse()`, `request.ToEntity(...)`)
- **var:** Use `var` everywhere

### Angular / TypeScript

- **Components:** Standalone, prefix `gpp`, kebab-case selector (e.g. `gpp-login`)
- **Directives:** Prefix `gpp`, camelCase selector (e.g. `gppTooltip`)
- **Imports:** Use `import type` for type-only imports (`consistent-type-imports` enforced by ESLint)

## Commands

### API — run from `Api/`

```bash
dotnet run                                 # start dev server (http: 5115, https: 7236)
dotnet ef migrations add <Name>            # add EF migration
dotnet ef database update                  # apply migrations locally
dotnet user-secrets set "<Key>" "<Value>"  # set local secret
```

### Frontend — run from `ClientApp/`

```bash
npm start                                  # serve (localhost:4200)
npx nx build                               # production build
npx nx lint                                # lint
npx nx test                                # unit tests
npx nx g @spartan-ng/cli:ui --name=<name>  # add Spartan UI component
```

### Full stack — run from repo root

```bash
docker-compose up -d       # start local Postgres
./generate-client.sh       # regenerate API client (API must be running on http://localhost:5115)
```

## Coding Standards

- **Indentation:** Tabs (all files)
- **Line length:** 100
- **Trailing commas:** ES5 (arrays/objects yes, function params no)
- **Quotes:** Single
- **Semicolons:** Required
- **TypeScript:** `strict: true`

Config files: `.editorconfig` at root, `ClientApp/.prettierrc`, `ClientApp/eslint.config.mjs`

## Product Brief Reference Docs

Detailed specs from PM's architecture & requirements brief, broken into focused files:

- `docs/DOMAIN.md` — entities, relationships, naming conventions, glossary
- `docs/USER-FLOWS.md` — 5 core flows in priority order (acceptance criteria)
- `docs/BUSINESS-RULES.md` — rules grouped by entity (plan, section, station, drill, library)
- `docs/UX-SPECS.md` — interaction behaviors, user mental model
- `docs/FUTURE-STATE.md` — out-of-scope features + architecture notes for future compatibility

## Domain Model (cheat sheet — see `docs/DOMAIN.md` for full details)

### Entities & Naming

| Entity | DB table | C# class | TS name | Notes |
|---|---|---|---|---|
| Practice Plan | `practice_plan` | `PracticePlan` | `practicePlan` | Reusable template, NOT a scheduled event |
| Section | `section` | `Section` | `section` | Ordered block within a plan |
| Plan Drill | `plan_drill` | `PlanDrill` | `planDrill` | Drill instance in a section (independent copy) |
| Drill (Library) | `drill` | `Drill` | `drill` | `source` field: `system` or `user` |
| Drill Type | `drill_type` | `DrillType` | `drillType` | Classification tag (Hitting, Pitching, etc.) |

### Key Fields

- `station_group` (UUID, nullable) on `plan_drill` — drills sharing the same value form a Station (parallel). Null = sequential.
- `coach_assignment` (string) — free text, NOT a FK in MVP
- `player_count` (int, optional) — plain integer placeholder for future player assignment
- `demo_link` (string) — YouTube URL, basic URL format check only
- `team_name`, `age_group` — on User profile, no separate Team entity in MVP

### Duration Tracking Formula

`total = sum(sequential drill durations) + sum(max duration per station_group)`

Non-blocking warning if total exceeds intended duration. Coach is never prevented from saving.

### Architecture Constraints

- **Int PKs + UUID column** on all entities — `Id` (int, auto-increment) is the PK used for joins/FKs; `Key` (UUID, unique, indexed, auto-generated) is the external identifier exposed in APIs and URLs. Future analytics/sharing reference the UUID, never the int PK.
- **No hardcoded single-user scoping** — structure plan queries so a permissions layer can be added later
- **Auto-save** — no manual save button; debounce + PATCH pattern
- **Station = not a DB entity** — just a shared `station_group` UUID on `plan_drill` rows
- **`source` discriminator on drills** — `system` (read-only, shared) vs `user` (personal, editable)
- **Save as Template = independent copy** — editing Plan Drill does NOT update My Drill, and vice versa

### MVP Scope Boundaries

**In scope:** Auth, user profile, practice plan builder (CRUD + sections + drills + stations), drill library (system + personal), duration tracking, plan export (PDF), auto-save.

**NOT in scope:** AI plan generation, calendar/scheduling, player/coach rosters, team management UI, plan sharing, game film, communications, tryout management, multi-sport, mobile app, payments.

## Deployment

- **Frontend:** Cloudflare Pages — auto-deploys on push to `main`
- **Backend:** Railway — auto-deploys on push to `main`, runs `MigrateAsync()` on startup
- **Database:** Railway managed Postgres

### Railway — Npgsql connection string format

Npgsql requires key=value format. Railway's `DATABASE_URL` is a URI and **does not work**:

```
# Correct — use reference variables from the Postgres plugin
Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}

# Wrong — Npgsql cannot parse URI format
postgresql://user:pass@host:port/db
```
