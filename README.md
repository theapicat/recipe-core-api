# 🍳 Recipe Core API

Selve motoren og hjertet i Recipe-plattformen. Dette API-et håndterer all kjernelogikk for oppskrifter, ingredienser, måltidsplanlegging og brukerdata, og fungerer som bindeleddet mellom brukergrensesnittet og de asynkrone bakgrunnstjenestene.

---

## 🎯 Hovedansvar & Features

* **📖 Oppskrifter & Næringsinnhold:** Lagring, redigering, sletting og henting av oppskrifter, trinnvise instruksjoner, ingredienser og beregnet næringsinnhold.
* **📅 Ukesplaner & Handlelister:** Generering og styring av dynamiske ukesplaner og interaktive handlelister som brukeren kan tilpasse i sanntid.
* **⚙️ Brukerinnstillinger:** Håndtering av personlige preferanser, allergier og visningsvalg.
* **📡 Sanntidsoppdateringer (SignalR):** Umiddelbare push-varsler til frontend når bakgrunnsjobber fullføres eller data endres.
* **🔄 Meldinger & Orkestrering (RabbitMQ):**
* Sender skrapeforespørsler videre til `recipe-scraper-service`.
* Utløser e-postutsendelser (f.eks. kontaktskjema) via `recipe-notification-service`.



---

## 🛠️ Teknologistakk & Designmønstre

* **.NET (Web API)** – Høyytelses web-API eksponert skjermet bak YARP Gateway.
* **PostgreSQL (`recipe-core-db`)** – Relasjonsdatabase for strukturert domenelagring.
* **Dapper & Dapper.Plus** – Lynrask dataadgang med skreddersydde SQL-spørringer og effektiv bulk-håndtering uten EF Core-overhead.
* **MediatR (CQRS)** – Skiller lesing (*Queries*) og skriving (*Commands*) i helt isolerte handlers for ryddig og skalerbar kildekode.
* **MassTransit & RabbitMQ** – Asynkron hendelsesstyrt kommunikasjon mot andre mikrotjenester.
* **Serilog & Seq** – Strukturt logging mot sentralisert dashboards på port `5341`.

---

## 🔄 Typiske Arbeidsflyter (Workflows)

```text
[ Web App / YARP Gateway ]
            │ (HTTP REST / WebSocket)
            ▼
   [ Recipe.Core.API ]
            │
            ▼ (MediatR CQRS)
 ┌──────────┴──────────┐
 │                     │
 ▼ (Queries / Read)    ▼ (Commands / Write)
[ Dapper / SQL ]     [ Dapper / SQL ] ──► [ PostgreSQL (recipe-core-db) ]
                       │
                       ├─► [ SignalR Hub ] ─────────► (Sanntids push til frontend)
                       │
                       └─► [ RabbitMQ Bus ] ───────┬─► (ScrapeRecipeCommand -> Scraper Service)
                                                   └─► (ContactFormSubmittedEvent -> Notification Service)

```

---

## 🏗️ Prosjektstruktur (Clean Architecture)

```text
recipe-core-api/
├── Contracts/      # Rene record-events for RabbitMQ (deles med andre mikrotjenester)
├── Domain/         # Kjerne-entiteter (Recipe, Ingredient, ...), Enums og Value Objects
├── Application/    # MediatR Commands/Queries, Handlers, caching, meldingspublisering, SignalR
├── Persistence/    # Dapper-repositorier, Npgsql-kobling, DbUp-migreringsskript
├── API/            # Controllers, DI/oppstartskobling, JWT-autentisering
└── Tests/          # xUnit-enhetstester

```

---

## 📚 Dokumentasjonsoversikt (`Documentation/`)

| Dokument | Beskrivelse |
| --- | --- |
| **[01-architecture-and-setup.md](Documentation/01-architecture-and-setup.md)** | Prosjektgraf, mappekonvensjoner, hvordan kjøre lokalt. |
| **[02-endpoints-and-controllers.md](Documentation/02-endpoints-and-controllers.md)** | Full endepunktstabell, tilgangsnivåer, kontrollermønster. |
| **[03-cqrs-and-mediatr.md](Documentation/03-cqrs-and-mediatr.md)** | MediatR-bruk, det generiske katalogmønsteret, DI-registreringsfallgruve. |
| **[04-events-and-messaging.md](Documentation/04-events-and-messaging.md)** | MassTransit/RabbitMQ-oppsett, `IEventPublisher`, meldingskontrakter, SignalR-status. |
| **[05-authentication-and-authorization.md](Documentation/05-authentication-and-authorization.md)** | JWT-validering, tilgangsnivåer, kjente hull og åpne spørsmål. |
| **[06-persistence-and-data-access.md](Documentation/06-persistence-and-data-access.md)** | Dapper-mønster, SQL-skript-nummerering, tilkoblingsoppsett. |
| **[07-test-strategy.md](Documentation/07-test-strategy.md)** | Testverktøy, hva som er dekket, hva som mangler. |

Domenemodellens designbegrunnelse («hvorfor») ligger i `RECIPE_BACKEND_NOTES.md`, ikke i `Documentation/`.

---

*Husk: Når vi begynner å bygge ut de enkelte endepunktene, tabellstrukturene og SignalR-hubsene, oppdaterer vi denne README-en fortløpende så den alltid gjenspeiler koden!*