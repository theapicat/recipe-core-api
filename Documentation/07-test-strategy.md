# Teststrategi i `recipe-core-api`

---

Per 2026-09-20. Sjekk mot faktisk kode ved tvil.

## 1. Oppsett

Ett testprosjekt, `Tests/Tests.csproj` (xUnit), referanse til `API`, `Application`, `Contracts`, `Domain`
og `Persistence`. Mappestrukturen speiler kildekoden 1:1 (samme regel som ellers i løsningen: namespace
skal alltid matche mappebanen).

```bash
dotnet test Tests/Tests.csproj
```

111 tester, alle grønne, ingen ekstern avhengighet (ingen Postgres/RabbitMQ trengs for å kjøre dem).

### Verktøyvalg

- **xUnit** — testrammeverk.
- **NSubstitute** — mocking. Valgt fordi det allerede lå i lokal NuGet-cache og har en moderne, enkel
  syntaks.
- **Innebygd `Assert`, ikke FluentAssertions** — FluentAssertions v8+ krever betalt lisens for kommersiell
  bruk, og dette er et kommersielt prosjekt. `Xunit.Assert` er tilstrekkelig og koster ingenting.

---

## 2. Hva som er dekket

| Område | Testfil(er) | Hva som verifiseres |
| --- | --- | --- |
| `Persistence/Services/DbReader<T>`/`DbWriter<T>` | `Tests/Persistence/Services/` | Kaster riktig exception-type når en query/command ikke er konfigurert — ingen databaseforbindelse trengs siden feilen kastes før tilkobling åpnes. |
| `Application/Caching/Services/MemoryCacheService` | `Tests/Application/Caching/` | Get/Set/Remove-primitivene med en ekte `IMemoryCache` (ingen mocking nødvendig). |
| Generiske katalog-handlers (`GetAll`/`GetById`/`Insert`/`Update`/`Delete`) | `Tests/Application/MediatR/Catalog/` | Cache-treff kaller aldri `DbReader<T>`; cache-miss henter fra reader og fyller cachen; skriveoperasjoner ugyldiggjør cachen; `Delete` returnerer `bool` basert på antall berørte rader. |
| `CatalogExtensions.AddCatalogHandlers()` | `Tests/Application/Extensions/` | Regresjonstest for et reelt problem oppdaget under utvikling — at alle fem handler-typer faktisk blir registrert for hver av de sju katalogtypene (Guid- og tekstnøkkel) (se [`03-cqrs-and-mediatr.md`](03-cqrs-and-mediatr.md) for hvorfor dette ikke skjer automatisk). |
| `MassTransitEventPublisher` | `Tests/Application/Messaging/` | Delegerer korrekt til `IPublishEndpoint.Publish`. |
| `SendContactFormCommandHandler` | `Tests/Application/MediatR/Public/ContactForm/` | Bygger og publiserer riktig hendelse, returnerer `true`. |
| `ReadCatalogController<T>`/`ReadWriteCatalogController<T>` | `Tests/API/Controllers/` | Riktig HTTP-resultat (`Ok`/`NotFound`/`Created` med server-tildelt id/`BadRequest` uten id på PUT/`NoContent`) basert på hva mediator returnerer, også for tekstnøkler. |
| `JwtAuthenticationExtensions` | `Tests/API/Extensions/` | Kaster `InvalidOperationException` når `Jwt:Key`/`Issuer`/`Audience` mangler; lykkes når alt er satt. |
| Server-tildelte id-er (`InsertCatalogCommandHandler`) | `Tests/Application/MediatR/Catalog/` | Guid-nøkler får en ny UUIDv7 (klientens id ignoreres); tekstnøkler (`NutrientDefinition`) beholdes. |
| Ingrediens: mapping og validering | `Tests/Application/MediatR/Admin/Ingredients/` | `IngredientMapper` tildeler id til ingrediensen og alle barn og dedupliserer koblinger; validering av tomt navn/negative verdier; create/update/delete-handlere (404, cache-invalidering). |
| Ingrediens: søk | `Tests/Application/MediatR/Ingredients/` | Filtrering i minnet: navn (også søkeord), kategori, allergen inkluder/ekskluder, søkeord, kombinasjon med OG. |
| Ubekreftede ingredienser | `Tests/Application/MediatR/User/…` og `…/Admin/UnconfirmedIngredients/` | Grenser (totalt/ventende), tilstandsmaskinen (kun `Pending` kan avgjøres, kun `NotRequested` kan endres), en annen brukers rad gir samme `NotFound` som en manglende, approve/merge/reject. |
| Eier fra token | `Tests/API/Controllers/UnconfirmedIngredientControllerTests`, `Tests/API/Extensions/ClaimsPrincipalExtensionsTests` | Bruker-id leses fra `NameIdentifier` og sendes inn i kommandoen; aldri fra body. |
| Feilmapping | `Tests/API/Extensions/ResultExtensionsTests`, `Tests/API/ExceptionHandlers/` | `Result` → 404/409/400; Postgres FK-/unikhetsbrudd → 409, andre feil ignoreres. |
| `DateTimeOffsetTypeHandler` | `Tests/Persistence/Implementation/` | `DateTime` fra Npgsql tolkes som UTC; skriving sender alltid UTC. |

---

## 3. Hva som ikke er dekket her

**Endepunkttester ligger utenfor dette repoet.** Brukeren har en uavhengig, omfattende endepunkttestpakke. Denne
pakken tester derfor kun klassenes funksjonalitet, og skal ikke utvides med endepunkt- eller kanttilfelletester.

**⚠️ Planlagt — ikke bygget:**

- **Integrasjonstester mot ekte Postgres.** De konkrete `Reader`/`Writer`-klassenes faktiske SQL (funksjonene i
  `Persistence/Scripts/`), transaksjonene (`IngredientWriter`, `ResolveAsync`) og Dapper-mappingen (enum, `DateTimeOffset`,
  `uuid[]`) er verifisert manuelt mot ekte Postgres (2026-09-20: en engangs Docker-container for skriptene og en
  engangs kjøring mot dev-databasen for hele flyten), ikke med en permanent testpakke. Naturlig neste steg:
  Testcontainers-basert prosjekt som kjører migreringsskriptene mot en ekte Postgres-instans i CI.
- **`Persistence.Extensions.MigrateDatabase`** — krever en ekte databaseforbindelse, hører hjemme i
  integrasjonstester, ikke enhetstester.

---

Sjekk mot faktisk kode ved tvil.
