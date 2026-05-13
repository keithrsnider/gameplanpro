# GamePlanPro — Stack Decisions

## Context

- **Developer:** Solo dev with strong Angular and .NET experience
- **App type:** SaaS product — primarily user/membership level data scoping, no RBAC or org/team features planned
- **Priorities:** Ship fast without making scalability or performance an afterthought
- **CSS experience:** Self-described weakest area — stack choices reflect this

---

## Full Stack

| Concern | Choice |
|---|---|
| **Frontend** | Angular + Spartan UI (Tailwind) |
| **Backend** | ASP.NET Core |
| **Database** | PostgreSQL |
| **ORM** | Entity Framework Core + Npgsql |
| **Auth** | ASP.NET Core Identity + Google OAuth middleware |
| **API client generation** | Kiota (consuming Swashbuckle/OpenAPI spec) |
| **Transactional email** | Resend |
| **Frontend hosting** | Cloudflare Pages |
| **Backend + DB hosting** | Railway |

---

## Key Decisions & Reasoning

### Angular (not Next.js or other full-stack JS)
Stayed with Angular due to existing expertise. No compelling reason to switch — the only meaningful gap vs. full-stack JS (end-to-end type safety) is bridged by Kiota generating a typed TypeScript client from the OpenAPI spec.

### ASP.NET Core Identity (not Auth0, Firebase, or Clerk)
- User-scoped SaaS with no RBAC/org requirements — managed auth services earn their cost at higher complexity
- ASP.NET Core Identity handles all cryptographic concerns (PBKDF2 hashing, token generation, lockout)
- Google OAuth handled by `Microsoft.AspNetCore.Authentication.Google` middleware — not hand-rolled
- User data lives in own Postgres database — no vendor lock-in, fully portable
- Security responsibilities reduce to: keep dependencies updated, use EF Core (not raw SQL), manage secrets via environment variables, configure CORS explicitly, add rate limiting to auth endpoints

### Google OAuth
- Supported natively via ASP.NET Core OAuth middleware
- When a user signs in with Google, Identity creates/links a record in `AspNetUserLogins` — same user table, auth method is an implementation detail
- User never rolls their own OAuth protocol

### ASP.NET Core Identity table structure
Identity creates its own tables via EF Core migrations:
- `AspNetUsers` — core user records
- `AspNetUserLogins` — linked OAuth providers (Google sign-in stored here)
- `AspNetUserTokens`, `AspNetUserClaims` — supporting auth data

Extend with app-specific fields by subclassing `IdentityUser`:
```csharp
public class AppUser : IdentityUser
{
    public string DisplayName { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### PostgreSQL (not SQL Server)
- EF Core + Npgsql is mature and well-supported
- JSONB columns available if flexible schema is needed later
- Hosted on Railway as a managed service — no direct public exposure

### Kiota (not NSwag or openapi-generator)
- Swashbuckle generates the OpenAPI spec on the .NET side (unchanged from prior experience)
- Kiota reads that spec and generates a typed TypeScript client for Angular
- Flow: `ASP.NET Core → Swashbuckle → OpenAPI spec → Kiota → TypeScript client → Angular`
- Kiota is Microsoft-backed, v1.0+, actively developed (preferred over NSwag which is less actively maintained)
- Slightly more Angular wiring upfront vs NSwag but worth it for the modern toolchain
- Regenerate client after backend changes — TypeScript surfaces breaking changes immediately

### Spartan UI (not Angular Material or PrimeNG)
- Angular Material: rejected — limited component count, Material Design aesthetic not wanted
- PrimeNG: considered but community complaints, heavier than needed, import/versioning friction
- Spartan UI: shadcn/ui-inspired, Tailwind-based, components are copied into the project (not imported from a package)
- "Own your components" model — no fighting library APIs, modify directly when needed
- Tailwind is actually well-suited for CSS-weak developers — constraint system makes decisions, no blank-canvas CSS
- No heavy data grids or charts needed — Spartan UI's component coverage is sufficient

### Resend (not SendGrid, Postmark, or Mailgun)
- Needed day one for password reset and email confirmation flows
- SendGrid is familiar but platform has degraded since Twilio acquisition
- Resend: best DX for solo/indie developers, 3,000 emails/month free tier, clean .NET HTTP API

### Cloudflare Pages (Angular frontend)
- Angular builds to static files — no reason to pay for compute to serve them
- Free tier, global CDN, auto-deploys on git push
- Preview URLs generated automatically per branch — useful for stakeholder reviews without screensharing

### Railway (backend + Postgres)
- Managed platform — no server administration, no SSH, no OS patching
- Postgres provisioned as a managed service (one click), not publicly exposed
- Docker-based deployment: write one Dockerfile for the .NET API, Railway builds and runs it
- SSL/TLS automatic
- First-class environments (dev/staging/prod) within a single project
- Custom domains supported — map `api.yourdomain.com` to Railway, `yourdomain.com` to Cloudflare Pages

---

## Deployment Architecture

```
yourdomain.com        → Cloudflare Pages (main branch)   + Railway prod environment
dev.yourdomain.com    → Cloudflare Pages (dev branch)     + Railway dev environment
```

Git push → Railway builds Dockerfile → deploys container (API)
Git push → Cloudflare runs `ng build` → deploys static files (Angular)

---

## Cloudflare Workers Assets Notes

- Use `wrangler deploy` (not `wrangler pages deploy`) for this setup.
- In `ClientApp/wrangler.jsonc`, keep `assets.directory` (not `pages_build_output_dir`).
- Keep `not_found_handling: "single-page-application"` for Angular routing.
- Do not add a `_redirects` file with this setup (can trigger Cloudflare error 10021 loop).
- Build output is `dist/ClientApp/browser`.
- Cloudflare Pages root directory should be `ClientApp` (not `/`).

---

## Docker Notes

- Developer has Docker basics (running images locally) but hasn't written Dockerfiles yet
- Standard .NET multi-stage Dockerfile is the starting point:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "YourApp.dll"]
```

- `docker-compose.yml` for local development (Postgres only — Railway handles orchestration in production):

```yaml
services:
  db:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: localpassword
      POSTGRES_DB: gameplanpro
    ports:
      - "5432:5432"
```

---

## Security Checklist (One-Time Setup)

- [ ] HTTPS enforced via HSTS middleware
- [ ] Cookie flags: HttpOnly, Secure, SameSite (ASP.NET Core defaults are correct)
- [ ] CORS configured explicitly to own domain — no wildcard in production
- [ ] All secrets in environment variables / Railway environment config — never in source code
- [ ] Rate limiting on login and password reset endpoints (ASP.NET Core 7+ built-in middleware)

## Security Checklist (Ongoing)

- [ ] Run `dotnet outdated` periodically and update packages
- [ ] Enable Dependabot on GitHub repository for automated CVE alerts
- [ ] Set Railway billing alert to avoid surprise costs

---

## Next Steps (Setup Session)

1. Scaffold Angular project with Spartan UI and Tailwind
2. Scaffold ASP.NET Core Web API project with Swashbuckle
3. Configure EF Core + Npgsql connection to local Postgres (via Docker Compose)
4. Set up ASP.NET Core Identity with custom `AppUser` class and run initial migrations
5. Configure Google OAuth middleware
6. Configure Resend for transactional email (password reset, email confirmation)
7. Set up Kiota client generation workflow
8. Write Dockerfile for ASP.NET Core API
9. Set up Railway project (API service + Postgres service + dev/prod environments)
10. Set up Cloudflare Pages connected to GitHub repo
11. Configure custom domains
