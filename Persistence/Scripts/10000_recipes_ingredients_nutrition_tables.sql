-- Feature-gruppe: oppskrifter, ingredienser og næringsinnhold.
-- Rekkefølge: enheter -> ingredienskataloger -> næringsstoffer -> ingrediens -> oppskrifter.

-- =========================================================================
-- Enheter
-- =========================================================================

CREATE TABLE unit_type (
    id   uuid PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE unit (
    id              uuid PRIMARY KEY,
    name            text NOT NULL,
    abbreviation    text NOT NULL,
    unit_type_id    uuid NOT NULL REFERENCES unit_type (id) ON DELETE RESTRICT,
    -- Forholdstall til enhetstypens basisenhet (f.eks. gram for Vekt) - universelt, ikke ingrediensavhengig.
    base_unit_ratio numeric(12,4) NOT NULL
);

CREATE INDEX ix_unit_unit_type_id ON unit (unit_type_id);

-- =========================================================================
-- Ingredienskataloger (adminstyrte oppslagstabeller)
-- =========================================================================

CREATE TABLE ingredient_category (
    id   uuid PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE allergen (
    id   uuid PRIMARY KEY,
    name text NOT NULL
);

CREATE TABLE search_keyword (
    id   uuid PRIMARY KEY,
    name text NOT NULL
);

-- =========================================================================
-- Næringsstoffer (speiler Matvaretabellens katalog - id er kildens egen kode)
-- =========================================================================

CREATE TABLE nutrient_definition (
    id                text PRIMARY KEY,
    name              text NOT NULL,
    unit              text NOT NULL,
    decimal_precision int  NOT NULL,
    -- Selvreferanse for hierarki (f.eks. "Mettet" -> parent_id "Fett"). Null = toppnivå.
    parent_id         text REFERENCES nutrient_definition (id) ON DELETE RESTRICT,
    source_url        text
);

CREATE INDEX ix_nutrient_definition_parent_id ON nutrient_definition (parent_id);

-- =========================================================================
-- Ingrediens
-- =========================================================================

CREATE TABLE ingredient (
    id                   uuid PRIMARY KEY,
    name                 text NOT NULL,
    category_id          uuid NOT NULL REFERENCES ingredient_category (id) ON DELETE RESTRICT,
    primary_unit_type_id uuid NOT NULL REFERENCES unit_type (id) ON DELETE RESTRICT,
    default_unit_id      uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    energy_kcal          numeric(10,2) NOT NULL,
    energy_kj            numeric(10,2),
    -- Andel av matvaren som er spiselig (f.eks. 97 for agurk).
    edible_part_percent  numeric(5,2),
    -- Kun satt for offisielt importerte ingredienser - lar brukeren slå opp kilden.
    source_id            text,
    source_url           text,
    -- false for adminlagte innslag som venter på fullstendige nærings-/allergendata.
    is_verified          boolean NOT NULL
);

CREATE INDEX ix_ingredient_category_id ON ingredient (category_id);
CREATE UNIQUE INDEX ux_ingredient_source_id ON ingredient (source_id) WHERE source_id IS NOT NULL;

CREATE TABLE ingredient_allergen (
    ingredient_id uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    allergen_id   uuid NOT NULL REFERENCES allergen (id) ON DELETE RESTRICT,
    PRIMARY KEY (ingredient_id, allergen_id)
);

CREATE TABLE ingredient_search_keyword (
    ingredient_id     uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    search_keyword_id uuid NOT NULL REFERENCES search_keyword (id) ON DELETE RESTRICT,
    PRIMARY KEY (ingredient_id, search_keyword_id)
);

-- Kun én rad per (ingredient_id, nutrient_definition_id) som faktisk er målt -
-- ikke alle ingredienser har verdi for alle næringsstoffer.
CREATE TABLE ingredient_nutrient_value (
    id                     uuid PRIMARY KEY,
    ingredient_id          uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    nutrient_definition_id text NOT NULL REFERENCES nutrient_definition (id) ON DELETE RESTRICT,
    quantity               numeric(12,4) NOT NULL,
    -- Kildens egen referansekode for denne spesifikke verdien (sporbarhet utover ingredient.source_id).
    source_id              text,
    UNIQUE (ingredient_id, nutrient_definition_id)
);

CREATE INDEX ix_ingredient_nutrient_value_nutrient_definition_id ON ingredient_nutrient_value (nutrient_definition_id);

-- Enhet -> gram-konvertering per ingrediens (speiler Matvaretabellens "portions"-data).
CREATE TABLE ingredient_portion (
    id                uuid PRIMARY KEY,
    ingredient_id     uuid NOT NULL REFERENCES ingredient (id) ON DELETE CASCADE,
    unit_id           uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    grams_per_portion numeric(10,2) NOT NULL
);

CREATE INDEX ix_ingredient_portion_ingredient_id ON ingredient_portion (ingredient_id);

-- Brukeroppgitt ingrediens som ikke finnes i den offisielle katalogen ennå.
-- created_by_user_id har ingen FK - brukeridentitet eies av en annen tjeneste/database.
CREATE TABLE unconfirmed_ingredient (
    id                    uuid PRIMARY KEY,
    name                  text NOT NULL,
    created_by_user_id    uuid NOT NULL,
    request_official_data boolean NOT NULL,
    created_at            timestamptz NOT NULL
);

-- =========================================================================
-- Oppskrifter
-- =========================================================================

CREATE TABLE recipe_category (
    id   uuid PRIMARY KEY,
    name text NOT NULL
);

-- owner_user_id har ingen FK - brukeridentitet eies av en annen tjeneste/database.
-- RecipeSource er flatet ut som kolonner - alltid tilstede, ikke egen tabell.
CREATE TABLE recipe (
    id                            uuid PRIMARY KEY,
    owner_user_id                 uuid NOT NULL,
    title                         text NOT NULL,
    description                   text NOT NULL,
    category_id                   uuid NOT NULL REFERENCES recipe_category (id) ON DELETE RESTRICT,
    -- Summen av recipe_step.timer_minutes for stegene som har en timer - ikke separat inntastet.
    cook_time_minutes             int NOT NULL,
    servings                      int NOT NULL,
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

CREATE INDEX ix_recipe_owner_user_id ON recipe (owner_user_id);
CREATE INDEX ix_recipe_category_id ON recipe (category_id);

CREATE TABLE recipe_step (
    id            uuid PRIMARY KEY,
    recipe_id     uuid NOT NULL REFERENCES recipe (id) ON DELETE CASCADE,
    step_number   int NOT NULL,
    description   text NOT NULL,
    timer_minutes int,
    UNIQUE (recipe_id, step_number)
);

-- Peker på nøyaktig én av ingredient_id eller unconfirmed_ingredient_id, aldri begge/ingen.
CREATE TABLE recipe_ingredient (
    id                        uuid PRIMARY KEY,
    recipe_id                 uuid NOT NULL REFERENCES recipe (id) ON DELETE CASCADE,
    ingredient_id             uuid REFERENCES ingredient (id) ON DELETE RESTRICT,
    unconfirmed_ingredient_id uuid REFERENCES unconfirmed_ingredient (id) ON DELETE RESTRICT,
    amount                    numeric(10,3) NOT NULL,
    unit_id                   uuid NOT NULL REFERENCES unit (id) ON DELETE RESTRICT,
    note                      text,
    CHECK (
        (ingredient_id IS NOT NULL AND unconfirmed_ingredient_id IS NULL) OR
        (ingredient_id IS NULL AND unconfirmed_ingredient_id IS NOT NULL)
    )
);

CREATE INDEX ix_recipe_ingredient_recipe_id ON recipe_ingredient (recipe_id);
CREATE INDEX ix_recipe_ingredient_ingredient_id ON recipe_ingredient (ingredient_id);
