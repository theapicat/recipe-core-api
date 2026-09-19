-- Feature-gruppe: oppskrifter, ingredienser og næringsinnhold (parer 10000_recipes_ingredients_nutrition_tables.sql).
-- Kun de enkle adminstyrte katalogene denne runden: ingredient_category, allergen, search_keyword,
-- unit_type, unit, recipe_category. nutrient_definition/ingredient/unconfirmed_ingredient kommer senere
-- siden de trenger mer enn den enkle get_all/get_by_id/insert/update/delete-formen.
--
-- Alle read-funksjonene returnerer SETOF <tabell> - Postgres oppretter automatisk en rad-type per tabell,
-- så get_..._by_id (0 eller 1 rad) og get_all_... (mange rader) kan dele samme returform.

-- =========================================================================
-- ingredient_category
-- =========================================================================

CREATE FUNCTION get_all_ingredient_category()
RETURNS SETOF ingredient_category
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_category ORDER BY name;
$$;

CREATE FUNCTION get_ingredient_category_by_id(p_id uuid)
RETURNS SETOF ingredient_category
LANGUAGE sql
AS $$
    SELECT * FROM ingredient_category WHERE id = p_id;
$$;

CREATE FUNCTION insert_ingredient_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_category (id, name) VALUES (p_id, p_name);
$$;

CREATE FUNCTION update_ingredient_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE ingredient_category SET name = p_name WHERE id = p_id;
$$;

CREATE FUNCTION delete_ingredient_category(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM ingredient_category WHERE id = p_id;
$$;

-- =========================================================================
-- allergen
-- =========================================================================

CREATE FUNCTION get_all_allergen()
RETURNS SETOF allergen
LANGUAGE sql
AS $$
    SELECT * FROM allergen ORDER BY name;
$$;

CREATE FUNCTION get_allergen_by_id(p_id uuid)
RETURNS SETOF allergen
LANGUAGE sql
AS $$
    SELECT * FROM allergen WHERE id = p_id;
$$;

CREATE FUNCTION insert_allergen(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO allergen (id, name) VALUES (p_id, p_name);
$$;

CREATE FUNCTION update_allergen(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE allergen SET name = p_name WHERE id = p_id;
$$;

CREATE FUNCTION delete_allergen(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM allergen WHERE id = p_id;
$$;

-- =========================================================================
-- search_keyword
-- =========================================================================

CREATE FUNCTION get_all_search_keyword()
RETURNS SETOF search_keyword
LANGUAGE sql
AS $$
    SELECT * FROM search_keyword ORDER BY name;
$$;

CREATE FUNCTION get_search_keyword_by_id(p_id uuid)
RETURNS SETOF search_keyword
LANGUAGE sql
AS $$
    SELECT * FROM search_keyword WHERE id = p_id;
$$;

CREATE FUNCTION insert_search_keyword(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO search_keyword (id, name) VALUES (p_id, p_name);
$$;

CREATE FUNCTION update_search_keyword(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE search_keyword SET name = p_name WHERE id = p_id;
$$;

CREATE FUNCTION delete_search_keyword(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM search_keyword WHERE id = p_id;
$$;

-- =========================================================================
-- unit_type
-- =========================================================================

CREATE FUNCTION get_all_unit_type()
RETURNS SETOF unit_type
LANGUAGE sql
AS $$
    SELECT * FROM unit_type ORDER BY name;
$$;

CREATE FUNCTION get_unit_type_by_id(p_id uuid)
RETURNS SETOF unit_type
LANGUAGE sql
AS $$
    SELECT * FROM unit_type WHERE id = p_id;
$$;

CREATE FUNCTION insert_unit_type(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO unit_type (id, name) VALUES (p_id, p_name);
$$;

CREATE FUNCTION update_unit_type(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE unit_type SET name = p_name WHERE id = p_id;
$$;

CREATE FUNCTION delete_unit_type(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM unit_type WHERE id = p_id;
$$;

-- =========================================================================
-- unit
-- =========================================================================

CREATE FUNCTION get_all_unit()
RETURNS SETOF unit
LANGUAGE sql
AS $$
    SELECT * FROM unit ORDER BY name;
$$;

CREATE FUNCTION get_unit_by_id(p_id uuid)
RETURNS SETOF unit
LANGUAGE sql
AS $$
    SELECT * FROM unit WHERE id = p_id;
$$;

CREATE FUNCTION insert_unit(
    p_id uuid,
    p_name text,
    p_abbreviation text,
    p_unit_type_id uuid,
    p_base_unit_ratio numeric
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO unit (id, name, abbreviation, unit_type_id, base_unit_ratio)
    VALUES (p_id, p_name, p_abbreviation, p_unit_type_id, p_base_unit_ratio);
$$;

CREATE FUNCTION update_unit(
    p_id uuid,
    p_name text,
    p_abbreviation text,
    p_unit_type_id uuid,
    p_base_unit_ratio numeric
)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE unit
    SET name = p_name,
        abbreviation = p_abbreviation,
        unit_type_id = p_unit_type_id,
        base_unit_ratio = p_base_unit_ratio
    WHERE id = p_id;
$$;

CREATE FUNCTION delete_unit(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM unit WHERE id = p_id;
$$;

-- =========================================================================
-- recipe_category
-- =========================================================================

CREATE FUNCTION get_all_recipe_category()
RETURNS SETOF recipe_category
LANGUAGE sql
AS $$
    SELECT * FROM recipe_category ORDER BY name;
$$;

CREATE FUNCTION get_recipe_category_by_id(p_id uuid)
RETURNS SETOF recipe_category
LANGUAGE sql
AS $$
    SELECT * FROM recipe_category WHERE id = p_id;
$$;

CREATE FUNCTION insert_recipe_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO recipe_category (id, name) VALUES (p_id, p_name);
$$;

CREATE FUNCTION update_recipe_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE recipe_category SET name = p_name WHERE id = p_id;
$$;

CREATE FUNCTION delete_recipe_category(p_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM recipe_category WHERE id = p_id;
$$;
