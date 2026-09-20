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
-- nutrient_definition (id er tekst - Matvaretabellens kode)
-- =========================================================================

CREATE OR REPLACE FUNCTION get_all_nutrient_definition()
RETURNS SETOF nutrient_definition
LANGUAGE sql
AS $$
    SELECT * FROM nutrient_definition ORDER BY name;
$$;

CREATE OR REPLACE FUNCTION get_nutrient_definition_by_id(p_id text)
RETURNS SETOF nutrient_definition
LANGUAGE sql
AS $$
    SELECT * FROM nutrient_definition WHERE id = p_id;
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
