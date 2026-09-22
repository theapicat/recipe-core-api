-- Feature-gruppe: oppskrifter, ingredienser og næringsinnhold.
-- Rekkefølge: enheter -> ingredienskataloger -> næringsstoffer -> ingrediens -> ubekreftet ingrediens -> oppskrifter.
-- Alle setninger er idempotente (IF NOT EXISTS). Det hindrer feil ved gjenkjøring, men endrer ikke en tabell som
-- allerede finnes - endringer i eksisterende tabeller etter første utrulling går i 11000-serien.

-- =========================================================================
-- Enheter
-- =========================================================================

CREATE TABLE IF NOT EXISTS unit_type (
    id        uuid PRIMARY KEY,
    name      text NOT NULL CHECK (name = lower(name)),
    -- true for seed-rader (Matvaretabellens enhetstyper) - admin kan ikke slette dem. Aldri satt via API-et,
    -- kun av seed-data (se Dapper-triksen i insert/update_unit_type - @IsSystem finnes ikke som SQL-parameter).
    is_system boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_unit_type_name ON unit_type (name);

CREATE TABLE IF NOT EXISTS unit (
    id              uuid PRIMARY KEY,
    name            text NOT NULL CHECK (name = lower(name)),
    -- Symbol (g, ml, µg, mg-ATE): beholder store/små bokstaver, unntatt fra små-bokstav-regelen for navn.
    abbreviation    text NOT NULL,
    unit_type_id    uuid NOT NULL REFERENCES unit_type (id) ON DELETE RESTRICT,
    -- Forholdstall til enhetstypens basisenhet (f.eks. gram for Vekt) - universelt, ikke ingrediensavhengig.
    -- Mange desimaler fordi mikrogram er 0,000001 g.
    base_unit_ratio numeric(20,10) NOT NULL CHECK (base_unit_ratio > 0),
    -- true for seed-rader - se unit_type.is_system.
    is_system       boolean NOT NULL DEFAULT false,
    -- Sikkerhetsnett i tillegg til applikasjonsvalideringen (CatalogValidation) - fanger opp enhver skrivevei.
    CHECK (abbreviation <> '')
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_unit_name ON unit (name);
-- Forkortelsen er unik uavhengig av store/små bokstaver (l og L er samme symbol).
CREATE UNIQUE INDEX IF NOT EXISTS ux_unit_abbreviation ON unit (lower(abbreviation));

CREATE INDEX IF NOT EXISTS ix_unit_unit_type_id ON unit (unit_type_id);

-- =========================================================================
-- Ingredienskataloger (adminstyrte oppslagstabeller)
-- =========================================================================

CREATE TABLE IF NOT EXISTS ingredient_category (
    id        uuid PRIMARY KEY,
    name      text NOT NULL CHECK (name = lower(name)),
    -- true for seed-rader - se unit_type.is_system.
    is_system boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ingredient_category_name ON ingredient_category (name);

CREATE TABLE IF NOT EXISTS allergen (
    id        uuid PRIMARY KEY,
    name      text NOT NULL CHECK (name = lower(name)),
    is_system boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_allergen_name ON allergen (name);

CREATE TABLE IF NOT EXISTS search_keyword (
    id        uuid PRIMARY KEY,
    name      text NOT NULL CHECK (name = lower(name)),
    is_system boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_search_keyword_name ON search_keyword (name);

-- =========================================================================
-- Næringsstoffer (speiler Matvaretabellens katalog - id er kildens egen kode). Statisk og skrivebeskyttet: fylles kun av
-- seed-data. Navnene beholder kildens store/små bokstaver (NaCl, EPA ...) - unntatt fra små-bokstav-regelen.
-- Hierarkiet ligger i nutrient_group (fett -> mettede/enumettede/flerumettede fettsyrer, vitaminer -> vitamin a-e ...), ikke
-- i en parent_id på stoffene. Enheten er en fremmednøkkel (uuid) til unit. Gruppen leses bare nøstet inni stoffet.
-- =========================================================================

CREATE TABLE IF NOT EXISTS nutrient_group (
    id              uuid PRIMARY KEY,
    name            text NOT NULL CHECK (name = lower(name)),
    -- Undergruppe: f.eks. "mettede fettsyrer" -> parent_group_id "fett". Null = hovedgruppe.
    parent_group_id uuid REFERENCES nutrient_group (id) ON DELETE RESTRICT,
    -- Visningsrekkefølge for gruppene (dybde-først), satt av seed-dataene.
    sort_order      int NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_nutrient_group_name ON nutrient_group (name);
CREATE INDEX IF NOT EXISTS ix_nutrient_group_parent_group_id ON nutrient_group (parent_group_id);

CREATE TABLE IF NOT EXISTS nutrient_definition (
    id                text PRIMARY KEY,
    name              text NOT NULL,
    -- Måleenhet (mg, µg ...) - vekten/typen og omregningsforholdet ligger i unit/unit_type.
    unit_id           uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    decimal_precision int  NOT NULL,
    -- Gruppen (den innerste, f.eks. "vitamin c" eller "mettede fettsyrer") stoffet hører til. Første stoff i en
    -- undergruppe er summen for gruppen, resten er delverdiene.
    group_id          uuid NOT NULL REFERENCES nutrient_group (id) ON DELETE RESTRICT,
    source_url        text,
    -- Visningsrekkefølge i hele lista (gruppe for gruppe, dybde-først), satt av seed-dataene.
    sort_order        int NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_nutrient_definition_unit_id ON nutrient_definition (unit_id);
CREATE INDEX IF NOT EXISTS ix_nutrient_definition_group_id ON nutrient_definition (group_id);

-- =========================================================================
-- Ingrediens
-- =========================================================================

CREATE TABLE IF NOT EXISTS ingredient (
    id                      uuid PRIMARY KEY,
    name                    text NOT NULL CHECK (name = lower(name)),
    category_id             uuid NOT NULL REFERENCES ingredient_category (id) ON DELETE RESTRICT,
    primary_unit_type_id    uuid NOT NULL REFERENCES unit_type (id) ON DELETE RESTRICT,
    default_unit_id         uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    energy_kcal             numeric(10,2) NOT NULL,
    energy_kj               numeric(10,2),
    -- Andel av matvaren som er spiselig (f.eks. 97 for agurk).
    edible_part_percent     numeric(5,2),
    -- Kun satt for offisielt importerte ingredienser - lar brukeren slå opp kilden.
    source_id               text,
    source_url              text,
    -- Satt når ingrediensen er en variant av en annen. Næringsdata kopieres ved opprettelse og følger ikke basen videre.
    variant_of_ingredient_id uuid REFERENCES ingredient (id) ON DELETE RESTRICT,
    -- false for adminlagte innslag som venter på fullstendige nærings-/allergendata.
    is_verified             boolean NOT NULL,
    -- true kun for rader fra den offisielle kilden (Matvaretabellen-seeden). Tildeles av serveren, kan aldri endres via PUT -
    -- en admin-opprettet eller brukergodkjent ingrediens er alltid false. Låser kildedata (navn, energi, spiselig del,
    -- kilde-id/-url, næringsverdi-settet) mot endring - se IngredientMapper.ValidateOfficialLock.
    is_official             boolean NOT NULL DEFAULT false,
    -- Brukes til optimistisk samtidighetskontroll på PUT (klienten sender tilbake verdien fra sin siste GET).
    updated_at              timestamptz NOT NULL DEFAULT now(),
    created_at              timestamptz NOT NULL DEFAULT now(),
    -- Satt av admin når allergen-tilordningen (ingredient_allergen) er verifisert å være komplett og korrekt for
    -- ingrediensen - uavhengig av is_verified/is_official, og fritt redigerbar selv om ingrediensen er offisiell.
    allergens_reviewed      boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_ingredient_name ON ingredient (name);

CREATE INDEX IF NOT EXISTS ix_ingredient_category_id ON ingredient (category_id);
CREATE INDEX IF NOT EXISTS ix_ingredient_variant_of_ingredient_id ON ingredient (variant_of_ingredient_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_ingredient_source_id ON ingredient (source_id) WHERE source_id IS NOT NULL;

CREATE TABLE IF NOT EXISTS ingredient_allergen (
    ingredient_id uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    allergen_id   uuid NOT NULL REFERENCES allergen (id) ON DELETE RESTRICT,
    PRIMARY KEY (ingredient_id, allergen_id)
);

CREATE TABLE IF NOT EXISTS ingredient_search_keyword (
    ingredient_id     uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    search_keyword_id uuid NOT NULL REFERENCES search_keyword (id) ON DELETE RESTRICT,
    PRIMARY KEY (ingredient_id, search_keyword_id)
);

-- Kun én rad per (ingredient_id, nutrient_definition_id) som faktisk er målt -
-- ikke alle ingredienser har verdi for alle næringsstoffer.
CREATE TABLE IF NOT EXISTS ingredient_nutrient_value (
    id                     uuid PRIMARY KEY,
    ingredient_id          uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    nutrient_definition_id text NOT NULL REFERENCES nutrient_definition (id) ON DELETE RESTRICT,
    quantity               numeric(12,4) NOT NULL,
    -- Kildens egen referansekode for denne spesifikke verdien (sporbarhet utover ingredient.source_id).
    source_id              text,
    UNIQUE (ingredient_id, nutrient_definition_id)
);

CREATE INDEX IF NOT EXISTS ix_ingredient_nutrient_value_nutrient_definition_id ON ingredient_nutrient_value (nutrient_definition_id);

-- Enhet -> gram-konvertering per ingrediens (speiler Matvaretabellens "portions"-data).
CREATE TABLE IF NOT EXISTS ingredient_portion (
    id                uuid PRIMARY KEY,
    ingredient_id     uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    unit_id           uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    grams_per_portion numeric(10,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ingredient_portion_ingredient_id ON ingredient_portion (ingredient_id);
-- Én porsjonsdefinisjon per enhet per ingrediens - to rader for samme enhet ville gitt en tvetydig omregning.
CREATE UNIQUE INDEX IF NOT EXISTS ux_ingredient_portion_ingredient_unit ON ingredient_portion (ingredient_id, unit_id);

-- Brukeroppgitt ingrediens som ikke finnes i den offisielle katalogen ennå.
-- created_by_user_id har ingen FK - brukeridentitet eies av en annen tjeneste/database.
-- Raden beholdes etter avgjørelse (Approved/Merged/Rejected) som historikk brukeren kan se.
CREATE TABLE IF NOT EXISTS unconfirmed_ingredient (
    id                     uuid PRIMARY KEY,
    name                   text NOT NULL CHECK (name = lower(name)),
    created_by_user_id     uuid NOT NULL,
    review_status          text NOT NULL DEFAULT 'NotRequested'
                           CHECK (review_status IN ('NotRequested', 'Pending', 'Approved', 'Merged', 'Rejected')),
    rejection_reason       text,
    reviewed_at            timestamptz,
    -- Den offisielle ingrediensen dette endte som - satt hvis og bare hvis status er Approved eller Merged.
    resolved_ingredient_id uuid REFERENCES ingredient (id) ON DELETE RESTRICT,
    created_at             timestamptz NOT NULL,
    CHECK ((review_status IN ('Approved', 'Merged')) = (resolved_ingredient_id IS NOT NULL))
);

-- En bruker kan ikke ha samme navn to ganger (løste forespørsler - Approved/Merged - regnes ikke med, de er historikk).
CREATE UNIQUE INDEX IF NOT EXISTS ux_unconfirmed_ingredient_user_name ON unconfirmed_ingredient (created_by_user_id, name)
    WHERE review_status NOT IN ('Approved', 'Merged');

CREATE INDEX IF NOT EXISTS ix_unconfirmed_ingredient_created_by_user_id ON unconfirmed_ingredient (created_by_user_id);
CREATE INDEX IF NOT EXISTS ix_unconfirmed_ingredient_pending ON unconfirmed_ingredient (created_at) WHERE review_status = 'Pending';

-- =========================================================================
-- Oppskrifter
-- =========================================================================

CREATE TABLE IF NOT EXISTS recipe_category (
    id        uuid PRIMARY KEY,
    name      text NOT NULL CHECK (name = lower(name)),
    is_system boolean NOT NULL DEFAULT false
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_recipe_category_name ON recipe_category (name);

-- owner_user_id har ingen FK - brukeridentitet eies av en annen tjeneste/database.
-- RecipeSource er flatet ut som kolonner - alltid tilstede, ikke egen tabell.
CREATE TABLE IF NOT EXISTS recipe (
    id                            uuid PRIMARY KEY,
    owner_user_id                 uuid NOT NULL,
    title                         text NOT NULL CHECK (title = lower(title)),
    description                   text NOT NULL,
    category_id                   uuid NOT NULL REFERENCES recipe_category (id) ON DELETE RESTRICT,
    -- Summen av recipe_step.timer_minutes for stegene som har en timer - ikke separat inntastet.
    cook_time_minutes             int NOT NULL CHECK (cook_time_minutes >= 0),
    servings                      int NOT NULL CHECK (servings > 0),
    image_url                     text,
    image_attribution             text,
    is_favorite                   boolean NOT NULL,
    source_type                   text NOT NULL CHECK (source_type IN ('Manual', 'Scraped')),
    source_reference              text,
    -- Kun satt for source_type = 'Scraped' - låst, kan ikke fjernes av brukeren.
    source_url                    text,
    source_is_edited_from_source  boolean,
    created_at                    timestamptz NOT NULL,
    updated_at                    timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_recipe_owner_user_id ON recipe (owner_user_id);
CREATE INDEX IF NOT EXISTS ix_recipe_category_id ON recipe (category_id);

CREATE TABLE IF NOT EXISTS recipe_step (
    id            uuid PRIMARY KEY,
    recipe_id     uuid NOT NULL REFERENCES recipe (id) ON DELETE CASCADE,
    step_number   int NOT NULL CHECK (step_number > 0),
    description   text NOT NULL,
    timer_minutes int CHECK (timer_minutes IS NULL OR timer_minutes >= 0),
    UNIQUE (recipe_id, step_number)
);

-- Peker på nøyaktig én av ingredient_id eller unconfirmed_ingredient_id, aldri begge/ingen.
CREATE TABLE IF NOT EXISTS recipe_ingredient (
    id                        uuid PRIMARY KEY,
    recipe_id                 uuid NOT NULL REFERENCES recipe (id) ON DELETE CASCADE,
    ingredient_id             uuid REFERENCES ingredient (id) ON DELETE RESTRICT,
    unconfirmed_ingredient_id uuid REFERENCES unconfirmed_ingredient (id) ON DELETE RESTRICT,
    -- 0 = ikke oppgitt / «etter smak» (f.eks. salt). Bidrar ikke til næringsberegningen - næringsverdier er veiledende.
    amount                    numeric(10,3) NOT NULL DEFAULT 0 CHECK (amount >= 0),
    unit_id                   uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    note                      text,
    -- Rekkefølgen brukeren skrev ingrediensene i (1..n), satt av serveren.
    sort_order                int NOT NULL,
    CHECK (
        (ingredient_id IS NOT NULL AND unconfirmed_ingredient_id IS NULL) OR
        (ingredient_id IS NULL AND unconfirmed_ingredient_id IS NOT NULL)
    )
);

CREATE INDEX IF NOT EXISTS ix_recipe_ingredient_recipe_id ON recipe_ingredient (recipe_id);
CREATE INDEX IF NOT EXISTS ix_recipe_ingredient_ingredient_id ON recipe_ingredient (ingredient_id);
CREATE INDEX IF NOT EXISTS ix_recipe_ingredient_unconfirmed_ingredient_id ON recipe_ingredient (unconfirmed_ingredient_id);
