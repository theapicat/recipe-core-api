-- Feature-gruppe: oppskrifter, ingredienser og næringsinnhold - KOMMANDOER (skriving). Parer 10000 (tabeller) og
-- 20000 (spørringer). Alle funksjoner er CREATE OR REPLACE (idempotente).
--
-- delete_...-funksjonene returnerer antall slettede rader (integer), slik at kalleren vet om noe faktisk ble
-- slettet. Funksjoner som endrer status/eierskap (unconfirmed_ingredient) returnerer likeledes antall berørte
-- rader - 0 betyr "ingen rad matchet vilkåret" (finnes ikke, feil eier eller feil status).

-- =========================================================================
-- ingredient_category
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_ingredient_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_category (id, name) VALUES (p_id, p_name);
$$;

CREATE OR REPLACE FUNCTION update_ingredient_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE ingredient_category SET name = p_name WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION delete_ingredient_category(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM ingredient_category WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- allergen
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_allergen(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO allergen (id, name) VALUES (p_id, p_name);
$$;

CREATE OR REPLACE FUNCTION update_allergen(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE allergen SET name = p_name WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION delete_allergen(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM allergen WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- search_keyword
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_search_keyword(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO search_keyword (id, name) VALUES (p_id, p_name);
$$;

CREATE OR REPLACE FUNCTION update_search_keyword(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE search_keyword SET name = p_name WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION delete_search_keyword(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM search_keyword WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- unit_type
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_unit_type(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO unit_type (id, name) VALUES (p_id, p_name);
$$;

CREATE OR REPLACE FUNCTION update_unit_type(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE unit_type SET name = p_name WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION delete_unit_type(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM unit_type WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- recipe_category
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_recipe_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO recipe_category (id, name) VALUES (p_id, p_name);
$$;

CREATE OR REPLACE FUNCTION update_recipe_category(p_id uuid, p_name text)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE recipe_category SET name = p_name WHERE id = p_id;
$$;

CREATE OR REPLACE FUNCTION delete_recipe_category(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM recipe_category WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- unit
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_unit(
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

CREATE OR REPLACE FUNCTION update_unit(
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

CREATE OR REPLACE FUNCTION delete_unit(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM unit WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

-- =========================================================================
-- ingredient (selve raden - barna har egne funksjoner under. Persistence kaller dem i én transaksjon.)
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_ingredient(
    p_id uuid,
    p_name text,
    p_category_id uuid,
    p_primary_unit_type_id uuid,
    p_default_unit_id uuid,
    p_energy_kcal numeric,
    p_energy_kj numeric,
    p_edible_part_percent numeric,
    p_source_id text,
    p_source_url text,
    p_variant_of_ingredient_id uuid,
    p_is_verified boolean,
    p_is_official boolean,
    p_updated_at timestamptz
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient (id, name, category_id, primary_unit_type_id, default_unit_id, energy_kcal, energy_kj,
                            edible_part_percent, source_id, source_url, variant_of_ingredient_id, is_verified,
                            is_official, updated_at)
    VALUES (p_id, p_name, p_category_id, p_primary_unit_type_id, p_default_unit_id, p_energy_kcal, p_energy_kj,
            p_edible_part_percent, p_source_id, p_source_url, p_variant_of_ingredient_id, p_is_verified,
            p_is_official, p_updated_at);
$$;

-- is_official er bevisst ikke en parameter her - den kan aldri endres via oppdatering, kun ved opprettelse.
CREATE OR REPLACE FUNCTION update_ingredient(
    p_id uuid,
    p_name text,
    p_category_id uuid,
    p_primary_unit_type_id uuid,
    p_default_unit_id uuid,
    p_energy_kcal numeric,
    p_energy_kj numeric,
    p_edible_part_percent numeric,
    p_source_id text,
    p_source_url text,
    p_variant_of_ingredient_id uuid,
    p_is_verified boolean,
    p_updated_at timestamptz
)
RETURNS void
LANGUAGE sql
AS $$
    UPDATE ingredient
    SET name = p_name,
        category_id = p_category_id,
        primary_unit_type_id = p_primary_unit_type_id,
        default_unit_id = p_default_unit_id,
        energy_kcal = p_energy_kcal,
        energy_kj = p_energy_kj,
        edible_part_percent = p_edible_part_percent,
        source_id = p_source_id,
        source_url = p_source_url,
        variant_of_ingredient_id = p_variant_of_ingredient_id,
        is_verified = p_is_verified,
        updated_at = p_updated_at
    WHERE id = p_id;
$$;

-- Barna (næringsverdier, porsjoner, allergen-/nøkkelord-koblinger) slettes med ON DELETE CASCADE.
CREATE OR REPLACE FUNCTION delete_ingredient(p_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM ingredient WHERE id = p_id RETURNING 1) SELECT count(*)::integer FROM d;
$$;

CREATE OR REPLACE FUNCTION insert_ingredient_allergen(p_ingredient_id uuid, p_allergen_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_allergen (ingredient_id, allergen_id) VALUES (p_ingredient_id, p_allergen_id);
$$;

CREATE OR REPLACE FUNCTION insert_ingredient_search_keyword(p_ingredient_id uuid, p_search_keyword_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_search_keyword (ingredient_id, search_keyword_id) VALUES (p_ingredient_id, p_search_keyword_id);
$$;

CREATE OR REPLACE FUNCTION insert_ingredient_nutrient_value(
    p_id uuid,
    p_ingredient_id uuid,
    p_nutrient_definition_id text,
    p_quantity numeric,
    p_source_id text
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_nutrient_value (id, ingredient_id, nutrient_definition_id, quantity, source_id)
    VALUES (p_id, p_ingredient_id, p_nutrient_definition_id, p_quantity, p_source_id);
$$;

CREATE OR REPLACE FUNCTION insert_ingredient_portion(
    p_id uuid,
    p_ingredient_id uuid,
    p_unit_id uuid,
    p_grams_per_portion numeric
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO ingredient_portion (id, ingredient_id, unit_id, grams_per_portion)
    VALUES (p_id, p_ingredient_id, p_unit_id, p_grams_per_portion);
$$;

-- Brukes ved oppdatering av en ingrediens: barna erstattes (slett + sett inn på nytt) i samme transaksjon.
CREATE OR REPLACE FUNCTION delete_ingredient_allergens(p_ingredient_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM ingredient_allergen WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION delete_ingredient_search_keywords(p_ingredient_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM ingredient_search_keyword WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION delete_ingredient_nutrient_values(p_ingredient_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM ingredient_nutrient_value WHERE ingredient_id = p_ingredient_id;
$$;

CREATE OR REPLACE FUNCTION delete_ingredient_portions(p_ingredient_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM ingredient_portion WHERE ingredient_id = p_ingredient_id;
$$;

-- =========================================================================
-- unconfirmed_ingredient
-- Eierskap håndheves også her (created_by_user_id i WHERE), ikke bare i applikasjonslaget.
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_unconfirmed_ingredient(
    p_id uuid,
    p_name text,
    p_created_by_user_id uuid,
    p_review_status text,
    p_created_at timestamptz
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO unconfirmed_ingredient (id, name, created_by_user_id, review_status, created_at)
    VALUES (p_id, p_name, p_created_by_user_id, p_review_status, p_created_at);
$$;

-- Kun mens ingen forespørsel er sendt (NotRequested).
CREATE OR REPLACE FUNCTION update_unconfirmed_ingredient_name(p_id uuid, p_created_by_user_id uuid, p_name text)
RETURNS integer
LANGUAGE sql
AS $$
    WITH u AS (
        UPDATE unconfirmed_ingredient SET name = p_name
        WHERE id = p_id AND created_by_user_id = p_created_by_user_id AND review_status = 'NotRequested'
        RETURNING 1
    ) SELECT count(*)::integer FROM u;
$$;

-- NotRequested -> Pending.
CREATE OR REPLACE FUNCTION request_unconfirmed_ingredient_review(p_id uuid, p_created_by_user_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH u AS (
        UPDATE unconfirmed_ingredient SET review_status = 'Pending'
        WHERE id = p_id AND created_by_user_id = p_created_by_user_id AND review_status = 'NotRequested'
        RETURNING 1
    ) SELECT count(*)::integer FROM u;
$$;

CREATE OR REPLACE FUNCTION delete_unconfirmed_ingredient(p_id uuid, p_created_by_user_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (
        DELETE FROM unconfirmed_ingredient WHERE id = p_id AND created_by_user_id = p_created_by_user_id RETURNING 1
    ) SELECT count(*)::integer FROM d;
$$;

-- Admin avslår en ventende forespørsel. Pending -> Rejected.
CREATE OR REPLACE FUNCTION reject_unconfirmed_ingredient(p_id uuid, p_reason text, p_reviewed_at timestamptz)
RETURNS integer
LANGUAGE sql
AS $$
    WITH u AS (
        UPDATE unconfirmed_ingredient
        SET review_status = 'Rejected', rejection_reason = p_reason, reviewed_at = p_reviewed_at
        WHERE id = p_id AND review_status = 'Pending'
        RETURNING 1
    ) SELECT count(*)::integer FROM u;
$$;

-- Admin godkjenner (Approved) eller kobler til en eksisterende ingrediens (Merged). Pending -> p_status.
-- Alle oppskriftslinjer som pekte på den ubekreftede ingrediensen flyttes over til den offisielle i samme
-- funksjonskall (atomisk), slik at check-kravet "nøyaktig én av ingredient_id/unconfirmed_ingredient_id" holder.
-- Ved Approved har Persistence allerede opprettet ingrediensen i samme transaksjon.
CREATE OR REPLACE FUNCTION resolve_unconfirmed_ingredient(
    p_id uuid,
    p_status text,
    p_resolved_ingredient_id uuid,
    p_reviewed_at timestamptz
)
RETURNS integer
LANGUAGE plpgsql
AS $$
DECLARE
    v_updated integer;
BEGIN
    UPDATE unconfirmed_ingredient
    SET review_status = p_status, resolved_ingredient_id = p_resolved_ingredient_id, reviewed_at = p_reviewed_at
    WHERE id = p_id AND review_status = 'Pending';

    GET DIAGNOSTICS v_updated = ROW_COUNT;

    IF v_updated > 0 THEN
        UPDATE recipe_ingredient
        SET ingredient_id = p_resolved_ingredient_id, unconfirmed_ingredient_id = NULL
        WHERE unconfirmed_ingredient_id = p_id;
    END IF;

    RETURN v_updated;
END;
$$;

-- =========================================================================
-- recipe (brukereide). Skriving av selve raden og barna (steg, ingredienslinjer) har hver sin funksjon - Persistence kaller dem i
-- én transaksjon. Endre/slette/favoritt filtrerer på eier og returnerer antall berørte rader (0 = finnes ikke eller ikke din).
-- =========================================================================

CREATE OR REPLACE FUNCTION insert_recipe(
    p_id uuid,
    p_owner_user_id uuid,
    p_title text,
    p_description text,
    p_category_id uuid,
    p_cook_time_minutes int,
    p_servings int,
    p_image_url text,
    p_image_attribution text,
    p_is_favorite boolean,
    p_source_type text,
    p_source_reference text,
    p_source_url text,
    p_source_is_edited_from_source boolean,
    p_created_at timestamptz,
    p_updated_at timestamptz
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO recipe (id, owner_user_id, title, description, category_id, cook_time_minutes, servings, image_url,
                        image_attribution, is_favorite, source_type, source_reference, source_url,
                        source_is_edited_from_source, created_at, updated_at)
    VALUES (p_id, p_owner_user_id, p_title, p_description, p_category_id, p_cook_time_minutes, p_servings, p_image_url,
            p_image_attribution, p_is_favorite, p_source_type, p_source_reference, p_source_url,
            p_source_is_edited_from_source, p_created_at, p_updated_at);
$$;

-- Endrer ikke eier, kilde-type/-url (låst), favoritt eller opprettelsestidspunkt.
CREATE OR REPLACE FUNCTION update_recipe(
    p_id uuid,
    p_owner_user_id uuid,
    p_title text,
    p_description text,
    p_category_id uuid,
    p_cook_time_minutes int,
    p_servings int,
    p_image_url text,
    p_image_attribution text,
    p_source_reference text,
    p_source_is_edited_from_source boolean,
    p_updated_at timestamptz
)
RETURNS integer
LANGUAGE sql
AS $$
    WITH u AS (
        UPDATE recipe
        SET title = p_title,
            description = p_description,
            category_id = p_category_id,
            cook_time_minutes = p_cook_time_minutes,
            servings = p_servings,
            image_url = p_image_url,
            image_attribution = p_image_attribution,
            source_reference = p_source_reference,
            source_is_edited_from_source = p_source_is_edited_from_source,
            updated_at = p_updated_at
        WHERE id = p_id AND owner_user_id = p_owner_user_id
        RETURNING 1
    )
    SELECT count(*)::integer FROM u;
$$;

CREATE OR REPLACE FUNCTION set_recipe_favorite(p_id uuid, p_owner_user_id uuid, p_is_favorite boolean)
RETURNS integer
LANGUAGE sql
AS $$
    WITH u AS (
        UPDATE recipe SET is_favorite = p_is_favorite WHERE id = p_id AND owner_user_id = p_owner_user_id RETURNING 1
    )
    SELECT count(*)::integer FROM u;
$$;

-- Steg og ingredienslinjer følger med (ON DELETE CASCADE).
CREATE OR REPLACE FUNCTION delete_recipe(p_id uuid, p_owner_user_id uuid)
RETURNS integer
LANGUAGE sql
AS $$
    WITH d AS (DELETE FROM recipe WHERE id = p_id AND owner_user_id = p_owner_user_id RETURNING 1)
    SELECT count(*)::integer FROM d;
$$;

CREATE OR REPLACE FUNCTION insert_recipe_step(
    p_id uuid, p_recipe_id uuid, p_step_number int, p_description text, p_timer_minutes int
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO recipe_step (id, recipe_id, step_number, description, timer_minutes)
    VALUES (p_id, p_recipe_id, p_step_number, p_description, p_timer_minutes);
$$;

CREATE OR REPLACE FUNCTION delete_recipe_steps(p_recipe_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM recipe_step WHERE recipe_id = p_recipe_id;
$$;

CREATE OR REPLACE FUNCTION insert_recipe_ingredient(
    p_id uuid, p_recipe_id uuid, p_ingredient_id uuid, p_unconfirmed_ingredient_id uuid,
    p_amount numeric, p_unit_id uuid, p_note text, p_sort_order int
)
RETURNS void
LANGUAGE sql
AS $$
    INSERT INTO recipe_ingredient (id, recipe_id, ingredient_id, unconfirmed_ingredient_id, amount, unit_id, note, sort_order)
    VALUES (p_id, p_recipe_id, p_ingredient_id, p_unconfirmed_ingredient_id, p_amount, p_unit_id, p_note, p_sort_order);
$$;

CREATE OR REPLACE FUNCTION delete_recipe_ingredients(p_recipe_id uuid)
RETURNS void
LANGUAGE sql
AS $$
    DELETE FROM recipe_ingredient WHERE recipe_id = p_recipe_id;
$$;
