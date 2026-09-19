# Autentisering og autorisering i `recipe-core-api`

---

Per 2026-09-19. Full designbegrunnelse og verifikasjonshistorikk (hvorfor JWT valideres lokalt, hvorfor
`ClaimTypes.NameIdentifier` og ikke `"sub"`, testresultater fra en engangstest mot en ekte token) ligger i
seksjonen «Autentisering og autorisering» i `RECIPE_BACKEND_NOTES.md`. Dette dokumentet beskriver kun
gjeldende kodetilstand. Sjekk mot faktisk kode ved tvil.

## 1. Prinsipp

Core API validerer JWT-en selv (`Microsoft.AspNetCore.Authentication.JwtBearer`), på samme måte som
gatewayen og `recipe-auth-api` gjør hver for seg. Gatewayen videresender `Authorization`-headeren uendret.

**Core API stoler aldri på `X-User-Id`/`X-User-Roles`-headere** — tjenesten er direkte nåbar på
`localhost:5002` utenom gatewayen i dev, så slike headere kan forfalskes. Identitet og roller leses kun
fra validerte JWT-claims.

Implementasjon: `API/Extensions/JwtAuthenticationExtensions.cs` (`AddJwtAuthentication`), kalt fra
`Program.cs` sammen med `AddAuthorization()`. Middleware-rekkefølge:

```
UseRouting() → UseAuthentication() → UseAuthorization() → MapControllers() / MapRealtimeHubs()
```

---

## 2. Konfigurasjon som må holdes i sync

| Core API (`appsettings`) | Gateway | Auth API | Dev-verdi |
| --- | --- | --- | --- |
| `Jwt:Key` | `Jwt:Key` | `JWT:SecretKey` | Samme som i gatewayens og Auth API sin dev-config — **ikke skrevet her**. |
| `Jwt:Issuer` | `Jwt:Issuer` | `JWT:Issuer` | `recipe-auth-app` |
| `Jwt:Audience` | `Jwt:Audience` | `JWT:Audience` | `recipe-frontend` |

`AddJwtAuthentication` kaster `InvalidOperationException` ved oppstart hvis noen av de tre mangler.

**⚠️ Nåværende tilstand:** `Jwt:Key` i `API/appsettings.Development.json` er tom per denne datoen —
API-et starter ikke før den fylles inn manuelt med samme verdi som gateway/Auth API bruker i dev. Ingen
ekte nøkkel skal noensinne stå i `appsettings.json`; i Docker/produksjon settes den via miljøvariabel
(`Jwt__Key`).

---

## 3. Tilgangsnivåer og ruter

Se [`02-endpoints-and-controllers.md`](02-endpoints-and-controllers.md) for full endepunktstabell. Kort
oppsummert:

| Basiskontroller | Rute | Attributt | Gateway-policy foran |
| --- | --- | --- | --- |
| `PublicController` | `/api/public` | `[AllowAnonymous]` | ingen |
| `UserController` | `/api/user` | `[Authorize]` | `AuthenticatedUser` |
| `AdminController` | `/api/admin` | `[Authorize(Roles = "admin")]` | `AdminUser` |
| `RecipeHub` (SignalR) | `/hubs/...` | `[Authorize]` | `AuthenticatedUser` |

Rollenavn er **alltid små bokstaver** (`admin`, `user`) — case-sensitivt. Gatewayen er første
forsvarslinje, ikke den eneste; Core API håndhever `[Authorize]` selv uansett.

**⚠️ Kjent hull i gateway (ikke rettet her):** gatewayens `AdminUser`-policy sjekker i dag
`RequireRole("Admin")` (stor bokstav) i `GatewayPolicyExtensions.cs`, mens Auth API utsteder rollen som
`admin`. Dette gir `403` for et ellers gyldig admin-token *når kallet går gjennom gatewayen* (ikke ved
direkte kall mot `:5002`). Ikke rediger gateway-repoet selv — si fra til brukeren hvis dette observeres.

---

## 4. Å hente innlogget bruker

- **Bruker-ID:** `User.FindFirstValue(ClaimTypes.NameIdentifier)` → `Guid.Parse`. **Ikke** `FindFirst("sub")`
  — standard `JwtBearer` omdøper `sub` til `ClaimTypes.NameIdentifier`.
- **Roller:** `[Authorize(Roles = "admin")]` eller `User.IsInRole("admin")`.
- Uthentingen skal skje ett sted i API-laget (en `ClaimsPrincipal`-extension), og bruker-ID sendes inn i
  MediatR-commands/-queries som eksplisitt parameter. **`Application` skal aldri kjenne til `HttpContext`
  eller `ClaimsPrincipal`** — dette er en hard grense, ikke en anbefaling.
- **Eierskap:** bruker-ID kommer alltid fra tokenet, aldri fra request-body/query/rute. Ingen
  `/api/user`-endepunkt skal ta imot en `userId`-parameter som påstår hvem eieren er.

**⚠️ Planlagt — ikke bygget:** ingen `/api/user`- eller `/api/admin`-endepunkter som faktisk bruker
brukerens ID finnes ennå (katalogendepunktene er upersonlige/delte). Dette blir relevant første gang
oppskrift-endepunkter bygges, siden `Recipe.OwnerUserId` skal settes derfra.

---

## 5. Åpne spørsmål

- Skal tilgang til en annen brukers ressurs gi `404` (ikke avslør at den finnes) eller `403`? Ikke
  avgjort — relevant først når eide ressurser (oppskrifter) får endepunkter.

---

## 6. Kjente begrensninger (bevisste, ikke glemt)

- Access-token er en frittstående JWT — Core API slår ikke opp brukeren i noen database per kall. En
  sperret eller slettet bruker kan bruke et allerede utstedt access-token til det utløper (opptil 60
  minutter). Akseptert eksponeringsvindu, samme som i Auth API sin dokumentasjon.
- Kontosletting i Auth API publiserer en hendelse Core API må konsumere for å kaskadeslette brukerens
  data. Ikke en del av autentiseringen i seg selv, men relevant den dagen `OwnerUserId`-data finnes.

---

Sjekk mot faktisk kode ved tvil.
