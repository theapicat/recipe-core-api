# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`recipe-core-api` is the core backend service of the Recipe platform: recipes, ingredients/nutrition,
meal planning, and user data. It sits behind a YARP gateway and talks to sibling microservices
(`recipe-scraper-service`, `recipe-notification-service`) over RabbitMQ, not direct HTTP calls. This repo
is early-stage: the domain model, all catalog models (catalogs, nutrient definitions, ingredients, and users'
unconfirmed-ingredient requests) and JWT auth are built end-to-end; recipes, meal planning, and shopping lists are not started (see
`RECIPE_BACKEND_NOTES.md` for the domain model's design rationale, and `Documentation/` for everything
else — start with `Documentation/01-architecture-and-setup.md`).

## Commands

```bash
dotnet build                          # build the whole solution
dotnet run --project API              # run the API (http://localhost:5002, see API/Properties/launchSettings.json)
dotnet watch --project API run        # run with hot reload
dotnet test Tests/Tests.csproj        # run unit tests (111 tests, no external dependencies needed)
```

Local dependencies (Postgres, RabbitMQ, Seq) are expected to run externally (e.g. via the platform's
docker-compose, in a sibling repo) — connection settings for them live in
`API/appsettings.Development.json` (localhost, port 5433 for Postgres) vs. `API/appsettings.json`
(Docker service names like `recipe-core-db`, used in the containerized/production setup).

## Architecture

Clean Architecture, one .NET project per layer, referencing inward only:

```
API  →  Application  →  Persistence  →  Domain
              ↓
          Contracts
```

- **Domain** — plain entity classes (no persistence or framework concerns). No repository interfaces or
  behavior live here yet.
- **Contracts** — cross-service message records (e.g. `Contracts/Event/ContactFormSubmittedEvent.cs`)
  published to RabbitMQ. This is the only project meant to be shared/compared against other
  microservices' contracts.
- **Persistence** — Dapper + Npgsql against Postgres, `dbup-postgresql` for migrations. Generic
  `DbReader<T>`/`DbWriter<T>` base classes in `Persistence/Services/`, one concrete Reader/Writer pair
  per model in `Persistence/Implementation/`. All queries/commands call Postgres functions, never raw SQL
  built in C#. Details: `Documentation/06-persistence-and-data-access.md`.
- **Application** — MediatR commands/queries and their handlers (CQRS), plus all cross-cutting technical
  services (caching, event publishing, SignalR) — there is no separate `Infrastructure` project (merged
  into `Application` 2026-09-19; default new cross-cutting concerns here too, only extract a project once
  something is genuinely large). `ApplicationMarker` is the assembly-scanning anchor for
  `services.AddMediatR(...)`. Details: `Documentation/03-cqrs-and-mediatr.md`,
  `Documentation/04-events-and-messaging.md`.
- **API** — ASP.NET Core controllers, DI/startup wiring (`API/Extensions/*.cs`), Serilog + JWT auth
  config. Controllers only translate HTTP ↔ MediatR; no business logic in controllers. Details:
  `Documentation/02-endpoints-and-controllers.md`.

### Request flow

Controller → `IMediator.Send(command/query)` → Application handler → Persistence Reader/Writer for
reads/writes, and/or `IEventPublisher.PublishAsync(...)` (wraps MassTransit) to emit an event onto
RabbitMQ for another service to consume. See `Application/MediatR/Public/ContactForm/` for the
hand-written reference pattern, and `Application/MediatR/Catalog/` for the generic pattern used by the
admin-managed catalog models.

### Controller convention

Three access-tier base classes carry route prefix + auth attribute: `PublicController` (`/api/public`,
`[AllowAnonymous]`, rare — most of the app requires a signed-in user), `UserController` (`/api/user`,
`[Authorize]`), `AdminController` (`/api/admin`, `[Authorize(Roles = "admin")]`). Concrete controllers for
the simple catalog models instead inherit from the generic `ReadCatalogController<T, TKey>`/
`ReadWriteCatalogController<T, TKey>` (C# only allows one base class, and the CQRS-shape axis and the
access-tier axis both want that slot) and apply `[Route]`/`[Authorize(...)]` directly. Full details incl.
the complete endpoint table: `Documentation/02-endpoints-and-controllers.md`.

**Before adding any `/api/user`, `/api/admin` or `/hubs` endpoint, read
`Documentation/05-authentication-and-authorization.md`** (implemented state) and the "Autentisering og
autorisering" section of `RECIPE_BACKEND_NOTES.md` (full rationale). Core API validates the JWT itself,
never trusts `X-User-Id`/`X-User-Roles`, reads the user id from `ClaimTypes.NameIdentifier` (not `"sub"`),
and uses lowercase roles (`admin`/`user`). `Jwt:Key`/`Issuer`/`Audience` must match the gateway and auth-api;
the app throws at startup if any is missing.

### Domain model notes (see `RECIPE_BACKEND_NOTES.md` for full rationale)

- Recipes are strictly user-owned (`Recipe.OwnerUserId`); there is no sharing/lineage model yet.
- `RecipeSource` distinguishes manually-created vs. scraped recipes (scraped ones carry a locked `Url`
  and an `IsEditedFromSource` flag).
- Nutrition data is modeled to mirror the external Matvaretabellen (Norwegian food composition table)
  source almost 1:1: `NutrientDefinition` uses the source's own string codes as IDs (not `Guid`), and
  `IngredientNutrientValue` is sparse (most ingredients only have values for a subset of nutrients).
  `IngredientPortion` reuses the source's own unit→gram conversions rather than a density model.
  `Ingredient.SourceId`/`SourceUrl` and friends exist purely for traceability back to that source.
- `UnconfirmedIngredient` is a user-scoped stub (not part of the shared `Ingredient` catalog) created when
  a user can't find an ingredient in search.
- Admin-managed lookup catalogs (`RecipeCategory`, `IngredientCategory`, `Allergen`, `UnitType`,
  `SearchKeyword`) all share a simple `{Id, Name}` shape.
- Ids are always assigned by the server (`Guid.CreateVersion7()`, via `IHasId<TKey>`) and returned by every create
  (`201` + `Location`); `NutrientDefinition` is the exception (Matvaretabellen's text code, supplied by admin).
- `UnconfirmedIngredient` has a review lifecycle (`NotRequested → Pending → Approved | Merged | Rejected`); approve
  can create the ingredient as a variant (`Ingredient.VariantOfIngredientId`) of another. See
  `Documentation/02-endpoints-and-controllers.md` §5.
- `Recipe.CookTimeMinutes` is a single total field — the sum of `RecipeStep.TimerMinutes` across steps
  that have a timer, not separate prep/cook fields.

## Tech stack

- .NET 10, ASP.NET Core Web API, C# nullable-enabled.
- MediatR for CQRS command/query dispatch.
- MassTransit + RabbitMQ for async cross-service messaging (connection config under `RabbitMQ:*` in
  appsettings, defaulted in `Application/Extensions/MassTransitExtensions.cs`).
- Microsoft.AspNetCore.Authentication.JwtBearer for auth (`API/Extensions/JwtAuthenticationExtensions.cs`) —
  Core API validates the JWT itself, doesn't just trust the gateway.
- Dapper + Npgsql for data access, `dbup-postgresql` for migrations (numbered scripts in
  `Persistence/Scripts/`: `10000` tables, `20000` queries, `30000` commands, `11000/21000/31000` for changes —
  convention, idempotency rules and the freeze point are in `Documentation/06-persistence-and-data-access.md`).
- SignalR for realtime push — wired (`Application/Realtime/RecipeHub.cs`, `/hubs/recipe`) but the hub has
  no methods yet, built ahead of need as a placeholder.
- Serilog (Console + Seq sinks) for structured logging, configured via `API/Extensions/SerilogsExtensions.cs`
  and the `Serilog` section in appsettings.
- xUnit + NSubstitute for testing (`Tests/`) — not FluentAssertions, its v8+ license requires payment for
  commercial use. Unit tests cover class functionality only; endpoint/edge-case tests live in the user's separate suite.

## Documentation

`Documentation/` holds the technical deep-dives (numbered, one topic per file; match the
format and tone of the existing files when writing or updating one):

- `01-architecture-and-setup.md` — project graph, folder conventions, running locally.
- `02-endpoints-and-controllers.md` — full endpoint table, access tiers, controller pattern.
- `03-cqrs-and-mediatr.md` — MediatR usage, the generic catalog pattern, DI registration gotcha.
- `04-events-and-messaging.md` — MassTransit/RabbitMQ, `IEventPublisher`, message contracts, SignalR status.
- `05-authentication-and-authorization.md` — JWT validation, access tiers, known gaps and open questions.
- `06-persistence-and-data-access.md` — Dapper pattern, SQL script numbering, connection setup.
- `07-test-strategy.md` — test tooling, what's covered, what's not.

`RECIPE_BACKEND_NOTES.md` (repo root) holds the domain model's design rationale — the "why", not the "how
it's wired". Do not edit sibling-repo docs without asking the user first.

## Language note

Log messages, user-facing API responses, and code comments in this repo are written in Norwegian
(Bokmål) — follow that convention when adding to existing files. New identifiers (class/method/property
names) are English.
