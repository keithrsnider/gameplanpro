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
- **Entity configuration:** `IEntityTypeConfiguration<T>` as a nested `Configuration` class inside the entity file (e.g. `AppUser.Configuration` inside `AppUser.cs`)
- **Secrets:** Connection strings and API keys go in `dotnet user-secrets` for local dev — never in `appsettings*.json`
- **Namespaces:** File-scoped namespaces only
- **API style:** Controllers-based (not minimal API)
- **var:** Use when type is apparent (`var x = new Foo()`), explicit otherwise

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
