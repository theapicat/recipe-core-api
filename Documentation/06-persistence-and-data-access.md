# Persistens og dataadgang i `recipe-core-api`

---

Per 2026-09-20. Sjekk mot faktisk kode ved tvil.

## 1. Prinsipp

Dapper mot Postgres via Npgsql — ingen EF Core. Alle spørringer/kommandoer er Postgres-funksjoner
(`CREATE FUNCTION`), aldri rå SQL bygget i C#. `Persistence`-prosjektet inneholder de generiske malklassene og de
konkrete klassene som kobler en modell til navnet på funksjonene den bruker; selve SQL-en ligger i migreringsskript
(§3).

Dapper-oppsett (satt i `PersistenceExtensions.AddPersistenceServices`):

- `DefaultTypeMap.MatchNamesWithUnderscores = true` — snake_case-kolonner matches mot PascalCase-egenskaper
  (`owner_user_id` → `OwnerUserId`) uten alias. Egenskapsnavnet må derfor tilsvare kolonnenavnet
  (`review_status` → `ReviewStatus`, ikke `Status`).
- `DateTimeOffsetTypeHandler` — Npgsql leverer `timestamptz` som `DateTime`, og Dapper konverterer ikke selv til
  `DateTimeOffset`. Skriving sender alltid UTC (Npgsql avviser andre offsets).
- **Enum sendes som tekst.** Dapper sender ellers enum som heltall, som feiler mot en `text`-kolonne med `CHECK`. Lesing
  fra tekst til enum fungerer direkte; ved skriving sendes `.ToString()` eksplisitt (se `UnconfirmedIngredientWriter`).
- `uuid[]` leveres som `Guid[]` — modeller som leses direkte fra en array-kolonne bruker `Guid[]`, ikke `List<Guid>`
  (`IngredientListItem`).

---

## 2. Generisk mal: `DbConnection` / `DbReader<T>` / `DbWriter<T>`

`Persistence/Services/`:

- `DbConnection(string connectionString)` — abstrakt basisklasse, åpner en `NpgsqlConnection`.
- `DbReader<T>` — abstrakt, virtuelle `GetAllQuery`/`GetByIdQuery`-strenger (null som standard), generiske
  `GetAllAsync()`/`GetByIdAsync<TId>(id)`. Kaster `DbQueryMissingException<T>` hvis en query ikke er overstyrt.
- `DbWriter<T>` — samme mønster for `InsertCommand`/`UpdateCommand`/`DeleteCommand`, kaster
  `DbCommandMissingException<T>`. `DeleteAsync` returnerer **antall slettede rader**: `delete_*`-funksjonene returnerer
  `integer`, og malen leser det som skalar (et `SELECT` gir ikke pålitelig «rader berørt» via `ExecuteAsync`).

Hver modell får sitt eget par i `Persistence/Implementation/` — for de enkle katalogene kun overstyring av
query/command-strenger, ingen annen logikk:

```csharp
public class AllergenReader(IConfiguration configuration)
    : DbReader<Allergen>(configuration.GetConnectionString("DefaultConnection")!)
{
    public override string? GetAllQuery { get; } = "SELECT * FROM get_all_allergen();";
    public override string? GetByIdQuery { get; } = "SELECT * FROM get_allergen_by_id(@Id);";
}
```

Registrert i DI mot malen (`DbReader<Allergen>` → `AllergenReader`), ikke et eget grensesnitt. Et grensesnitt per modell
finnes **bare** når modellen trenger metoder utover malen.

### Modeller som går utover malen

- **`Ingredient`** — `IngredientReader` overstyrer `GetByIdAsync` og henter raden og alle barn (allergener, nøkkelord,
  næringsverdier, porsjoner) i ett `QueryMultiple`-kall. `IngredientWriter` overstyrer `AddAsync`/`UpdateAsync` slik at
  ingrediensen og barna skrives i **én transaksjon** (malen åpner ellers ny forbindelse per kall). Oppdatering
  erstatter barna (slett + sett inn). Delt SQL-logikk ligger i `IngredientPersistence` (internal).
- **`IngredientListItem`** — `IngredientListItemReader`, kun `GetAll` (lettvekts-liste til cachen).
- **`UnconfirmedIngredient`** — egne grensesnitt i `Persistence/Interfaces/` (`IUnconfirmedIngredientReader`/
  `Writer`) siden den trenger spørringer per bruker/status, telling til grenser og statusendringer. Endrende metoder
  returnerer `bool` (`false` = ingen rad matchet vilkåret: finnes ikke, feil eier eller feil status). `ResolveAsync`
  oppretter evt. en ny ingrediens, avgjør forespørselen og flytter oppskriftslinjer i én transaksjon.
- **`NutrientDefinition`** — statisk og skrivebeskyttet: bare `NutrientDefinitionReader`, ingen Writer og ingen
  `insert_/update_/delete_nutrient_definition` i SQL. Radene (inkl. `is_group` og `sort_order`) kommer kun fra seed-data.

---

## 3. Migreringsskript (`Persistence/Scripts/`)

Kjøres av DbUp (`Persistence/Extensions/DatabaseMigrationExtensions.cs`, `MigrateDatabase`), kalt fra `Program.cs`
før noe annet. Filene er embedded resources, sortert alfabetisk på fullt ressursnavn (tall før bokstaver).
DbUp kjører hvert skript **én gang** og journalfører navnet i `schemaversions`.

### Nummereringskonvensjon

| Område | Nummerserie | Innhold |
| --- | --- | --- |
| Databasekonfigurasjon | `00000`–`09999` | Extensions, roller, skjema. Sjeldent — ingen skript ennå (id-er genereres i applikasjonen). |
| Opprette tabeller | `10000`–`10999` | Én fil per feature-gruppe (ikke per tabell). `10000` = oppskrifter/ingredienser/næring. |
| Endre tabeller | `11000`–`19999` | Én fil per endring, økes fortløpende. |
| Spørringer (lesing) | `20000`–`20999` | `SELECT`-funksjoner. Suffiks-parer med tilhørende `10000`-fil. |
| Endre spørringer | `21000`–`29999` | |
| Kommandoer (skriving) | `30000`–`30999` | `INSERT`/`UPDATE`/`DELETE`-funksjoner. |
| Endre kommandoer | `31000`–`39999` | |
| Seed-data | `Persistence/Scripts/SeedData/seed_<nn>_<beskrivelse>.sql` | Undermappen sorterer etter alle numeriske skript (siffer før bokstaver). Innad i mappen styrer tosifret `<nn>` rekkefølgen (avhengigheter først) — se «Seed-data» under. |

Hver kategori har 1000 plasser til «opprett» og 9000 til «endre» — mer enn nok, siden endringer er langt hyppigere enn
nye feature-grupper.

Bygget så langt (alle for feature-gruppen oppskrifter/ingredienser/næring):

- `10000_recipes_ingredients_nutrition_tables.sql` — alle tabeller (enheter, ingredienskataloger,
  næringsstoffer, `ingredient` med barn, `unconfirmed_ingredient`, oppskrifter).
- `20000_recipes_ingredients_nutrition_queries.sql` — `get_all_*`/`get_*_by_id` for alle katalogtyper og
  ingrediens (inkl. `get_all_ingredient_list_item` med `array_agg`-kolonner og barnefunksjonene), samt per bruker/
  status/telling for ubekreftede ingredienser. Read-funksjonene returnerer `SETOF <tabell>` (Postgres lager en
  rad-type per tabell).
- `30000_recipes_ingredients_nutrition_commands.sql` — `insert_*`/`update_*`/`delete_*` for alle katalogtyper og
  ingrediens (raden + barnefunksjoner), og statusfunksjonene for ubekreftede ingredienser
  (`request_…_review`, `reject_…`, `resolve_…`). `delete_*` og statusfunksjonene returnerer antall berørte rader.

### Seed-data (`Persistence/Scripts/SeedData/`)

Seed-data er vanlige SQL-skript som DbUp kjører (embedded resources, ingen egen kode). De kjører etter alle nummererte skript,
hvert **én gang** (journalført i `schemaversions`), og er skrevet idempotent (`ON CONFLICT DO NOTHING`) i tillegg.

| Skript | Innhold |
| --- | --- |
| `seed_01_nutrient_definitions` | 57 næringsstoffer fra Matvaretabellen + 4 grupperader (`is_group`), med `sort_order`. Skrivebeskyttet katalog. |
| `seed_02_units` | 3 enhetstyper (vekt, volum, antall), 17 kjerneenheter og 30 antall-enheter avledet av Matvaretabellens porsjonstyper. |
| `seed_03_ingredient_categories` | 16 ingrediens-kategorier. |
| `seed_04_allergens` | De 14 EU-allergenene + laktose. |
| `seed_05_recipe_categories` | 13 oppskriftskategorier. |
| `seed_10`–`seed_25_ingredients_<kategori>` | 1577 ingredienser (én fil per kategori) med næringsverdier (89 438), porsjoner (2 481) og søkeord (328 unike, 1 312 koblinger). |

- **Små kataloger har faste id-er** (deterministiske UUIDv7-lignende, generert én gang), slik at senere skript kan referere
  til dem uten å slå opp på navn (navn kan endres av admin). Ingredienser, verdier og søkeord får id fra en midlertidig
  `pg_temp.seed_uuid_v7()` (Postgres 16 har ikke UUIDv7 innebygd) og kobles på Matvaretabellens matvare-id (`source_id`).
- **Utvalg:** kun matvarer som brukes som ingredienser eller i måltider — ikke spedbarnsmat, ferdigretter, kosttilskudd,
  kaker/desserter, snacks. «Diverse matvarer» er fordelt på de andre kategoriene. Admin kan rydde bort det som ikke
  trengs; manglende ingredienser legges til senere (via admin eller nye seed-skript).
- **Alle importerte ingredienser har `is_verified = false`:** kilden har ingen allergendata, så `ingredient_allergen` er
  tom. Allergenfilteret kan derfor ikke stoles på før allergener er tilordnet (planlagt, se `todo.md`).
- **Næringsverdier** er *per 100 g spiselig del* (Matvaretabellens konvensjon; `edible_part_percent` sier hvor stor del av
  matvaren som er spiselig), og sparsomme: kun målte verdier lagres (`0` = målt som null, manglende rad = ukjent).
- Skriptene er generert av et lokalt hjelpescript fra `source-data/*.json` (ikke i repoet); resultatet er deterministisk.
  Filene er ca. 4 MB til sammen og kjøres på ca. 12 sekunder mot en tom database.
- **Etter frysepunktet** (første utrulling) endres seed-skript aldri på stedet — nye/endrede data går i et nytt
  seed-skript med høyere `<nn>`.

### Idempotente skript og frysepunkt

Alle setninger er idempotente: `CREATE TABLE/INDEX IF NOT EXISTS`, `CREATE OR REPLACE FUNCTION`. Det hindrer feil ved
gjenkjøring, men **endrer ikke et objekt som allerede finnes** (en eksisterende tabell får ikke nye kolonner av
`IF NOT EXISTS`, og `CREATE OR REPLACE` kan ikke endre returtype).

**Frysepunkt:** så lenge ingen database utenfor en utvikler sin maskin finnes, redigeres `10000`/`20000`/`30000` på
stedet og dev-databasen tilbakestilles (`DROP SCHEMA public CASCADE; CREATE SCHEMA public;` — husk at det også fjerner
`uuid-ossp`-utvidelsen infrastruktur-oppsettet legger inn, som må opprettes på nytt). Fra den første utrullingen
(staging/produksjon) er de tre filene **skrivebeskyttet**, og alle endringer går i `11000`/`21000`/`31000`-seriene.

### Navn: små bokstaver og unike

Alle navnekolonner (`name` i kataloger, ingrediens, ubekreftet ingrediens; `unit.abbreviation`; `recipe.title`) har
`CHECK (col = lower(col))`, og katalogtabellene har unik indeks på navnet (`ux_<tabell>_name`; enhet også på
forkortelsen). Ubekreftede ingredienser har unik `(created_by_user_id, name)` bortsett fra løste (`Approved`/`Merged`,
som er historikk). Brudd gir `409` via `PostgresExceptionHandler`. `nutrient_definition.name` er unntatt (navn som NaCl
og EPA beholder store bokstaver). Seed-data må derfor være skrevet med små bokstaver.

### Primærnøkler

Alle genererte ID-er er `uuid`-kolonner, generert i applikasjonslaget med `Guid.CreateVersion7()` (ikke `Guid.NewGuid()`)
for bedre indekslokalitet — Postgres er heap-organisert, så tilfeldig UUID-rekkefølge er langt mindre kostbart her enn i
et tidligere MySQL-prosjekt. `NutrientDefinition.Id` er unntaket: en `text`-kolonne som speiler Matvaretabellens
stabile kode.

---

## 4. Tilkoblingsstreng

`ConnectionStrings:DefaultConnection` i appsettings — `localhost:5433` i dev (`appsettings.Development.json`),
Docker-tjenestenavn `recipe-core-db:5432` i container-/produksjonsoppsett (`appsettings.json`).

---

Sjekk mot faktisk kode ved tvil.
