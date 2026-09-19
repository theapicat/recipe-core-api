# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`recipe-core-api` is the core backend service of the Recipe platform: recipes, ingredients/nutrition,
meal planning, and user data. It sits behind a YARP gateway and talks to sibling microservices
(`recipe-scraper-service`, `recipe-notification-service`) over RabbitMQ, not direct HTTP calls. This repo
is early-stage: the domain model exists, but persistence, most application handlers, and API endpoints
are still to be built (see `RECIPE_BACKEND_NOTES.md` for the design rationale behind the domain model).

## Commands

```bash
dotnet build                          # build the whole solution
dotnet run --project API              # run the API (http://localhost:5002, see API/Properties/launchSettings.json)
dotnet watch --project API run        # run with hot reload
```

There is currently no test project in the solution — do not assume `dotnet test` has anything to run
until one is added.

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
- **Persistence** — intended for Dapper/Dapper.Plus repositories and raw SQL against Postgres via
  Npgsql, with `dbup-postgresql` for migrations. Currently just the empty project shell — no repositories,
  connection factory, or migration scripts exist yet.
- **Application** — MediatR commands/queries and their handlers (CQRS). `ApplicationMarker` is the
  assembly-scanning anchor used by `services.AddMediatR(...)` in `API/Extensions/ApplicationExtensions.cs`.
  Handlers depend on `Persistence` (not yet implemented) and `Contracts` (for publishing events).
- **API** — ASP.NET Core controllers, DI/startup wiring (`API/Extensions/*.cs`), Serilog + MassTransit
  config. Controllers only translate HTTP ↔ MediatR; no business logic in controllers.

### Request flow

Controller → `IMediator.Send(command/query)` → Application handler → (future) Persistence repository
for reads/writes, and/or `IPublishEndpoint.Publish(...)` (MassTransit) to emit an event onto RabbitMQ for
another service to consume. See `Application/MediatR/Public/ContactForm/` for the reference
implementation of this pattern (command → handler → publish `ContactFormSubmittedEvent`).

### Controller convention

All public-facing controllers inherit `API.Controllers.PublicController` (route base `/api/public`),
live under `API/Controllers/PublicControllers/`, and are `[AllowAnonymous]`. There is no authenticated
controller base yet — when adding one, follow the same "abstract base class carries the route prefix"
pattern.

**Before adding any `/api/user`, `/api/admin` or `/hubs` endpoint, read the "Autentisering og autorisering"
section of `RECIPE_BACKEND_NOTES.md`.** It defines the auth procedure: Core API validates the JWT itself
(JwtBearer, same key/issuer/audience as the gateway and `recipe-auth-api`), never trusts `X-User-Id`/`X-User-Roles`,
reads the user id from `ClaimTypes.NameIdentifier` (not `"sub"`), and uses lowercase roles (`admin`/`user`).

### Domain namespace vs. folder layout

Domain classes physically live under `Domain/<Area>/` (matching the `Domain` project structure), but their
`namespace` is still the placeholder `RecipeCoreApi.Domain.Models.<Area>` inherited from the design draft
described in `RECIPE_BACKEND_NOTES.md`. This mismatch is known, pre-existing debt, not a mistake to "fix"
incidentally while touching unrelated code — rename deliberately if asked to.

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
- `Recipe.CookTimeMinutes` is a single total field — the sum of `RecipeStep.TimerMinutes` across steps
  that have a timer, not separate prep/cook fields.

## Tech stack

- .NET 10, ASP.NET Core Web API, C# nullable-enabled.
- MediatR for CQRS command/query dispatch.
- MassTransit + RabbitMQ for async cross-service messaging (connection config under `RabbitMQ:*` in
  appsettings, defaulted in `API/Extensions/MassTransitExtensions.cs`).
- Serilog (Console + Seq sinks) for structured logging, configured via `API/Extensions/SerilogsExtensions.cs`
  and the `Serilog` section in appsettings.
- Planned but not yet wired: Dapper/Dapper.Plus + Npgsql for data access, `dbup-postgresql` for
  migrations, SignalR for realtime push to the frontend.

## Documentation

When asked to write or update documentation, follow `DOCUMENTATION_GUIDE.md` (file layout, structure, tone,
and which sibling-repo docs to flag afterwards). Do not edit sibling repos without asking.

## Language note

Log messages, user-facing API responses, and code comments in this repo are written in Norwegian
(Bokmål) — follow that convention when adding to existing files. New identifiers (class/method/property
names) are English.
