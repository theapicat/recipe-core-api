-- Feature-gruppe: oppskrifter, ingredienser og næringsinnhold - SPØRRINGER (lesing). Parer 10000 (tabeller) og
-- 30000 (kommandoer). Alle funksjoner er CREATE OR REPLACE (idempotente). Endres en funksjons returtype må den
-- droppes først - etter første utrulling gjøres slike endringer i 21000-serien.
--
-- Read-funksjonene returnerer SETOF <tabell> - Postgres oppretter automatisk en rad-type per tabell, så
-- get_all_... (mange rader) og get_..._by_id (0 eller 1 rad) deler samme returform.

-- =========================================================================
-- ingredient_category
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_ingredient_category()
RETURNS SETOF ingredient_category
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_category ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_ingredient_category_by_id(p_id uuid)
RETURNS SETOF ingredient_category
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_category WHERE id = p_id;
$$;

-- =========================================================================
-- allergen
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_allergen()
RETURNS SETOF allergen
LANGUAGE sql
AS $$
    SELECT * FROM allergen ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_allergen_by_id(p_id uuid)
RETURNS SETOF allergen
LANGUAGE sql
AS $$
    SELECT * FROM allergen WHERE id = p_id;
$$;

-- =========================================================================
-- search_keyword
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_search_keyword()
RETURNS SETOF search_keyword
LANGUAGE sql
AS $$
    SELECT * FROM search_keyword ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_search_keyword_by_id(p_id uuid)
RETURNS SETOF search_keyword
LANGUAGE sql
AS $$
    SELECT * FROM search_keyword WHERE id = p_id;
$$;

-- =========================================================================
-- unit_type
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_unit_type()
RETURNS SETOF unit_type
LANGUAGE sql
AS $$
    SELECT * FROM unit_type ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_unit_type_by_id(p_id uuid)
RETURNS SETOF unit_type
LANGUAGE sql
AS $$
    SELECT * FROM unit_type WHERE id = p_id;
$$;

-- =========================================================================
-- unit
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_unit()
RETURNS SETOF unit
LANGUAGE sql
AS $$
    SELECT * FROM unit ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_unit_by_id(p_id uuid)
RETURNS SETOF unit
LANGUAGE sql
AS $$
    SELECT * FROM unit WHERE id = p_id;
$$;

-- =========================================================================
-- recipe_category
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_recipe_category()
RETURNS SETOF recipe_category
LANGUAGE sql
AS $$
    SELECT * FROM recipe_category ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_recipe_category_by_id(p_id uuid)
RETURNS SETOF recipe_category
LANGUAGE sql
AS $$
    SELECT * FROM recipe_category WHERE id = p_id;
$$;

-- =========================================================================
-- nutrient_definition (id er tekst - Matvaretabellens kode). nutrient_group har ingen egne lesefunksjoner: gruppen leses bare
-- nøstet i stoffet.
-- =========================================================================

-- Ett flat rad per stoff med enheten (unit = forkortelsen, unit_type_id) og gruppen (og overordnet gruppe) utfoldet, så
-- klienten kan vise, omregne/summere og bygge gruppevisningen uten flere oppslag. Persistence bygger raden om til et objekt
-- med gruppen nøstet inni.
CREATE OR REPLACE FUNCTION get_all_nutrient_definition()
RETURNS TABLE (
    id text, name text, unit_id uuid, unit text, unit_type_id uuid, decimal_precision int, source_url text, sort_order int,
    group_id uuid, group_name text, group_sort_order int,
    parent_group_id uuid, parent_group_name text, parent_group_sort_order int
)
LANGUAGE sql
AS $$
    SELECT n.id, n.name, n.unit_id, u.abbreviation, u.unit_type_id, n.decimal_precision, n.source_url, n.sort_order,
           g.id, g.name, g.sort_order,
           p.id, p.name, p.sort_order
    FROM nutrient_definition n
    JOIN unit u ON u.id = n.unit_id
    JOIN nutrient_group g ON g.id = n.group_id
    LEFT JOIN nutrient_group p ON p.id = g.parent_group_id
    ORDER BY n.sort_order;
$$;

CREATE OR REPLACE FUNCTION get_nutrient_definition_by_id(p_id text)
RETURNS TABLE (
    id text, name text, unit_id uuid, unit text, unit_type_id uuid, decimal_precision int, source_url text, sort_order int,
    group_id uuid, group_name text, group_sort_order int,
    parent_group_id uuid, parent_group_name text, parent_group_sort_order int
)
LANGUAGE sql
AS $$
    SELECT n.id, n.name, n.unit_id, u.abbreviation, u.unit_type_id, n.decimal_precision, n.source_url, n.sort_order,
           g.id, g.name, g.sort_order,
           p.id, p.name, p.sort_order
    FROM nutrient_definition n
    JOIN unit u ON u.id = n.unit_id
    JOIN nutrient_group g ON g.id = n.group_id
    LEFT JOIN nutrient_group p ON p.id = g.parent_group_id
    WHERE n.id = p_id;
$$;

-- =========================================================================
-- ingredient
-- =========================================================================

-- Lettvekts-liste for søk/valg. Allergen- og nøkkelord-id-ene følger med som arrays slik at filtrering kan
-- gjøres i minnet uten å hente den tunge modellen.
CREATE OR REPLACE FUNCTION get_all_ingredient_list_item()
RETURNS TABLE (
    id                       uuid,
    name                     text,
    category_id              uuid,
    primary_unit_type_id     uuid,
    default_unit_id          uuid,
    energy_kcal              numeric,
    is_verified              boolean,
    variant_of_ingredient_id uuid,
    allergen_ids             uuid[],
    search_keyword_ids       uuid[]
)
LANGUAGE sql
AS $$
    SELECT i.id,
           i.name,
           i.category_id,
           i.primary_unit_type_id,
           i.default_unit_id,
           i.energy_kcal,
           i.is_verified,
           i.variant_of_ingredient_id,
           COALESCE((SELECT array_agg(ia.allergen_id) FROM ingredient_allergen ia WHERE ia.ingredient_id = i.id), '{}'::uuid[]),
           COALESCE((SELECT array_agg(isk.search_keyword_id) FROM ingredient_search_keyword isk WHERE isk.ingredient_id = i.id), '{}'::uuid[])
    FROM ingredient i
    ORDER BY i.name;
$$;

-- Selve raden. Barna hentes med funksjonene under, i ett kall fra Persistence.
CREATE OR REPLACE FUNCTION get_ingredient_by_id(p_id uuid)
RETURNS SETOF ingredient
LANGUAGE sql
AS $$
    SELECT * FROM ingredient WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION get_ingredient_allergen_ids(p_ingredient_id uuid)
RETURNS SETOF uuid
LANGUAGE sql
AS $$
    SELECT allergen_id FROM ingredient_allergen WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION get_ingredient_search_keyword_ids(p_ingredient_id uuid)
RETURNS SETOF uuid
LANGUAGE sql
AS $$
    SELECT search_keyword_id FROM ingredient_search_keyword WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION get_ingredient_nutrient_values(p_ingredient_id uuid)
RETURNS SETOF ingredient_nutrient_value
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_nutrient_value WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION get_ingredient_portions(p_ingredient_id uuid)
RETURNS SETOF ingredient_portion
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_portion WHERE ingredient_id = p_ingredient_id;
$$;

-- =========================================================================
-- unconfirmed_ingredient (brukerstyrt - ikke cachet)
-- =========================================================================

CREATE OR REPLACE FUNCTION get_unconfirmed_ingredient_by_id(p_id uuid)
RETURNS SETOF unconfirmed_ingredient
LANGUAGE sql
AS $$
    SELECT * FROM unconfirmed_ingredient WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION get_unconfirmed_ingredient_by_user(p_user_id uuid)
RETURNS SETOF unconfirmed_ingredient
LANGUAGE sql
AS $$
    SELECT * FROM unconfirmed_ingredient WHERE created_by_user_id = p_user_id ORDER BY created_at DESC;
$$;

-- Admin-køen. p_status = NULL gir alle, ellers filtrert på status (eldste først, slik at køen tas i rekkefølge).
CREATE OR REPLACE FUNCTION get_all_unconfirmed_ingredient(p_status text DEFAULT NULL)
RETURNS SETOF unconfirmed_ingredient
LANGUAGE sql
AS $$
    SELECT * FROM unconfirmed_ingredient
    WHERE p_status IS NULL OR review_status = p_status
    ORDER BY created_at;
$$;

-- Brukes til å håndheve grenser per bruker (totalt og antall ventende forespørsler).
CREATE OR REPLACE FUNCTION count_unconfirmed_ingredient_by_user(p_user_id uuid, p_status text DEFAULT NULL)
RETURNS integer
LANGUAGE sql
AS $$
    SELECT count(*)::integer FROM unconfirmed_ingredient
    WHERE created_by_user_id = p_user_id AND (p_status IS NULL OR review_status = p_status);
$$;
