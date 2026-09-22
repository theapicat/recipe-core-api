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
    is_official              boolean,
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
           i.is_official,
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

-- =========================================================================
-- recipe (brukereide - ikke cachet). ALLE funksjonene tar eier (p_owner_user_id) og filtrerer på den, så en annen brukers
-- oppskrift er umulig å lese, selv om id-en er kjent.
-- =========================================================================

-- Lettvekts-liste (én rad per oppskrift) til oppskriftsoversikten. Hele lista hentes og filtreres/søkes i klienten.
CREATE OR REPLACE FUNCTION get_all_recipe_list_item(p_owner_user_id uuid)
RETURNS TABLE (
    id uuid, title text, image_url text, category_id uuid, cook_time_minutes int, servings int, is_favorite boolean
)
LANGUAGE sql
AS $$
    SELECT r.id, r.title, r.image_url, r.category_id, r.cook_time_minutes, r.servings, r.is_favorite
    FROM recipe r
    WHERE r.owner_user_id = p_owner_user_id
    ORDER BY r.title;
$$;

CREATE OR REPLACE FUNCTION get_recipe_by_id(p_id uuid, p_owner_user_id uuid)
RETURNS SETOF recipe
LANGUAGE sql
AS $$
    SELECT * FROM recipe WHERE id = p_id AND owner_user_id = p_owner_user_id;
$$;

CREATE OR REPLACE FUNCTION get_recipe_steps(p_recipe_id uuid, p_owner_user_id uuid)
RETURNS SETOF recipe_step
LANGUAGE sql
AS $$
    SELECT s.*
    FROM recipe_step s
    JOIN recipe r ON r.id = s.recipe_id
    WHERE s.recipe_id = p_recipe_id AND r.owner_user_id = p_owner_user_id
    ORDER BY s.step_number;
$$;

-- Linjene får navnet på ingrediensen (offisiell eller brukerens egen) med, så klienten slipper egne oppslag for å vise dem.
CREATE OR REPLACE FUNCTION get_recipe_ingredients(p_recipe_id uuid, p_owner_user_id uuid)
RETURNS TABLE (
    id uuid, recipe_id uuid, ingredient_id uuid, unconfirmed_ingredient_id uuid, amount numeric, unit_id uuid,
    note text, sort_order int, name text
)
LANGUAGE sql
AS $$
    SELECT ri.id, ri.recipe_id, ri.ingredient_id, ri.unconfirmed_ingredient_id, ri.amount, ri.unit_id,
           ri.note, ri.sort_order, COALESCE(i.name, u.name)
    FROM recipe_ingredient ri
    JOIN recipe r ON r.id = ri.recipe_id
    LEFT JOIN ingredient i ON i.id = ri.ingredient_id
    LEFT JOIN unconfirmed_ingredient u ON u.id = ri.unconfirmed_ingredient_id
    WHERE ri.recipe_id = p_recipe_id AND r.owner_user_id = p_owner_user_id
    ORDER BY ri.sort_order;
$$;

-- Brukes til å håndheve grensen for antall oppskrifter per bruker.
CREATE OR REPLACE FUNCTION count_recipe_by_owner(p_owner_user_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    SELECT count(*)::integer FROM recipe WHERE owner_user_id = p_owner_user_id;
$$;

-- =========================================================================
-- Rådata til næringsberegningen for en oppskrift (RecipeNutritionCalculator regner ut i Application). Tre funksjoner, alle med eier-
-- filter: linjene med enhet og ingrediensens energi/spiselig del, ingrediensenes porsjoner (enhet -> gram), og næringsverdiene.
-- Ubekreftede ingredienser (ingredient_id null) har ingen næringsdata og returneres bare som linje.
-- =========================================================================

CREATE OR REPLACE FUNCTION get_recipe_nutrition_lines(p_recipe_id uuid, p_owner_user_id uuid)
RETURNS TABLE (
    line_id uuid, sort_order int, name text, ingredient_id uuid, amount numeric, unit_id uuid, unit_type_name text,
    unit_base_ratio numeric, energy_kcal numeric, energy_kj numeric, edible_part_percent numeric
)
LANGUAGE sql
AS $$
    SELECT ri.id, ri.sort_order, COALESCE(i.name, u.name), ri.ingredient_id, ri.amount, ri.unit_id, ut.name,
           un.base_unit_ratio, i.energy_kcal, i.energy_kj, i.edible_part_percent
    FROM recipe_ingredient ri
    JOIN recipe r ON r.id = ri.recipe_id
    JOIN unit un ON un.id = ri.unit_id
    JOIN unit_type ut ON ut.id = un.unit_type_id
    LEFT JOIN ingredient i ON i.id = ri.ingredient_id
    LEFT JOIN unconfirmed_ingredient u ON u.id = ri.unconfirmed_ingredient_id
    WHERE ri.recipe_id = p_recipe_id AND r.owner_user_id = p_owner_user_id
    ORDER BY ri.sort_order;
$$;

CREATE OR REPLACE FUNCTION get_recipe_nutrition_portions(p_recipe_id uuid, p_owner_user_id uuid)
RETURNS TABLE (ingredient_id uuid, unit_id uuid, unit_type_name text, unit_base_ratio numeric, grams_per_portion numeric)
LANGUAGE sql
AS $$
    SELECT p.ingredient_id, p.unit_id, ut.name, un.base_unit_ratio, p.grams_per_portion
    FROM ingredient_portion p
    JOIN unit un ON un.id = p.unit_id
    JOIN unit_type ut ON ut.id = un.unit_type_id
    WHERE p.ingredient_id IN (
        SELECT ri.ingredient_id
        FROM recipe_ingredient ri
        JOIN recipe r ON r.id = ri.recipe_id
        WHERE ri.recipe_id = p_recipe_id AND r.owner_user_id = p_owner_user_id AND ri.ingredient_id IS NOT NULL
    );
$$;

CREATE OR REPLACE FUNCTION get_recipe_nutrition_values(p_recipe_id uuid, p_owner_user_id uuid)
RETURNS TABLE (ingredient_id uuid, nutrient_definition_id text, quantity numeric, nutrient_sort_order int)
LANGUAGE sql
AS $$
    SELECT v.ingredient_id, v.nutrient_definition_id, v.quantity, n.sort_order
    FROM ingredient_nutrient_value v
    JOIN nutrient_definition n ON n.id = v.nutrient_definition_id
    WHERE v.ingredient_id IN (
        SELECT ri.ingredient_id
        FROM recipe_ingredient ri
        JOIN recipe r ON r.id = ri.recipe_id
        WHERE ri.recipe_id = p_recipe_id AND r.owner_user_id = p_owner_user_id AND ri.ingredient_id IS NOT NULL
    );
$$;
