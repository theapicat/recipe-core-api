-- Seed: ingrediens-kategorier. Kun matvarer som brukes som ingredienser/i måltider (ikke spedbarnsmat, ferdigretter,
-- kosttilskudd, kaker/desserter ...). «Diverse matvarer» er fordelt på de andre kategoriene.

INSERT INTO ingredient_category (id, name, is_system) VALUES
    ('01a088b5-a200-7339-9d1d-cdf167f4fa16', 'meieriprodukter', true),
    ('01a088b5-a201-7d6d-bc03-348c6500982e', 'egg', true),
    ('01a088b5-a202-73cb-b81b-c6053c22a0e7', 'kjøtt og fjørfe', true),
    ('01a088b5-a203-762c-ab6e-7afb3d411966', 'fisk og skalldyr', true),
    ('01a088b5-a204-7a12-9b23-cb425e755738', 'grønnsaker og poteter', true),
    ('01a088b5-a205-78d2-b19e-bdec93733802', 'frukt og bær', true),
    ('01a088b5-a206-70d2-b894-7b3e8e3e9031', 'belgvekster', true),
    ('01a088b5-a207-72df-bacc-eed66d5b1e1b', 'nøtter og frø', true),
    ('01a088b5-a208-7fbd-aab4-bc057f8308dc', 'korn, mel og pasta', true),
    ('01a088b5-a209-7334-a2d2-55cd29be763e', 'brød', true),
    ('01a088b5-a20a-796f-ac5d-1a4d793f4984', 'frokostblandinger', true),
    ('01a088b5-a20b-72d5-af33-7b4d3b3d2cb7', 'sukker og søtning', true),
    ('01a088b5-a20c-7f50-a382-85d686bd2029', 'sjokolade og søtsaker', true),
    ('01a088b5-a20d-7cad-b41e-0593457c22d2', 'fett og oljer', true),
    ('01a088b5-a20e-755e-a20e-6396ff2c51d4', 'urter, krydder og sauser', true),
    ('01a088b5-a20f-7a75-b1aa-a0c4a73183c9', 'drikke', true)
ON CONFLICT DO NOTHING;
