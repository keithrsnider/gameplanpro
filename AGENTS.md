# AGENTS.md

## Purpose
- This guide is optimized for feature implementation agents shipping end-to-end changes in GamePlanPro.
- Stack is split: `Api/` (ASP.NET Core API) + `ClientApp/` (Angular/Nx SPA) connected by generated Kiota client code.

## Non-Negotiable Guardrails
- Preserve backend layering: Controller -> Service -> Repository (`Api/Controllers`, `Api/Services`, `Api/Repositories`).
- Keep controllers thin (service calls only), as in `Api/Controllers/PracticePlansController.cs`.
- Put business rules + auth/ownership checks in services using `IUserContext`, as in `Api/Services/PracticePlanService.cs`.
- Keep repositories EF-only data access with `AppDbContext`, as in `Api/Repositories/PracticePlanRepository.cs`.
- Register new repos/services in `Api/Extensions/ApplicationServiceExtensions.cs`; do not wire app services in `Api/Program.cs`.
- Keep local secrets in user-secrets (`dotnet user-secrets`), not `appsettings*.json`.
- Never hand-edit generated client files in `ClientApp/src/app/core/api/`.
- API URLs and payloads use UUID `Key`; relational joins/FKs use int `Id` (`Api/Models/BaseEntity.cs`).

## Feature Implementation Loop
- 1) Trace the vertical slice first (controller -> service -> repository -> DTO/mapper -> frontend call site).
- 2) Implement backend contract and behavior, then regenerate Kiota client with `./generate-client.sh`.
- 3) Update frontend to use generated API surface via `ApiClientService` (`ClientApp/src/app/core/api-client.service.ts`).
- 4) Verify auth and ownership behavior for user-scoped data before finishing.
- 5) Run relevant lint/tests before handoff.

## Backend Patterns To Follow
- Routes use `{key:guid}` for entity lookups (example: `Api/Controllers/PracticePlansController.cs`).
- Map entities/DTOs with extension mappers in `Api/Models/Mappers/` (example: `Api/Models/Mappers/PracticePlanMapper.cs`).
- Keep interfaces with implementations unless multiple implementations are needed.
- Keep `Program.cs` wiring-only; current pipeline uses `ExceptionHandlingMiddleware`, CORS, rate limiting, auth, controllers, and `/health` (`Api/Program.cs`).
- Startup applies pending EF migrations via `MigrateAsync()` in `Program.cs`; account for schema changes before client regeneration.
- Respect infra setup: Identity + Postgres config in `Api/Extensions/IdentityServiceExtensions.cs`, CORS allowlist in `Api/appsettings.Development.json`.

## Frontend Integration Patterns
- App is standalone and route-lazy-loaded (`ClientApp/src/app/app.routes.ts`).
- Auth bootstrap depends on `provideAppInitializer(() => inject(AuthService).checkAuth())` in `ClientApp/src/app/app.config.ts`.
- API requests must include cookies (`credentials: 'include'`) through the configured Kiota adapter.
- Local dev frontend expects `/api` proxy to `http://localhost:5115` (`ClientApp/proxy.conf.json`).

## Developer Workflow Commands
- DB (repo root): `docker-compose up -d`
- API (`Api/`): `dotnet run`
- API migrations (`Api/`): `dotnet ef migrations add <Name>` and `dotnet ef database update`
- API local secrets (`Api/`): `dotnet user-secrets set "<Key>" "<Value>"`
- Frontend (`ClientApp/`): `npm start`
- Client regen (repo root, API running on 5115): `./generate-client.sh`
- Frontend checks (`ClientApp/`): `npx nx lint` and `npx nx test`

## Code Style Rules That Matter
- C#: tabs, file-scoped namespaces, `var` preference (`.editorconfig`).
- TS/Angular: tabs, single quotes, semicolons, print width 100 (`ClientApp/.prettierrc`).
- Angular selectors: component `gpp-*`, directive `gpp*` (`ClientApp/eslint.config.mjs`).
- Use `import type` for type-only imports (`consistent-type-imports`).

## Source Docs
- Conventions and architecture: `CLAUDE.md`
- Domain/behavior constraints: `docs/DOMAIN.md`, `docs/BUSINESS-RULES.md`, `docs/USER-FLOWS.md`, `docs/UX-SPECS.md`
- Stack/deployment rationale: `STACK.md`

