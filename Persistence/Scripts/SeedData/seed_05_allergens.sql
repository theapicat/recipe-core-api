-- Seed: allergener - de 14 EU-allergenene (matinformasjonsforskriften) pluss laktose, som er et kosthold-/
-- intoleransefilter og ikke en lovpålagt allergen. Tilordning til ingredienser skjer ikke her (kilden har ingen
-- allergendata) - se is_verified på ingrediensene.

INSERT INTO allergen (id, name) VALUES
    ('01a088b5-a200-7258-b0b7-65e19c0d2a15', 'glutenholdig korn'),
    ('01a088b5-a201-7237-a915-493a57bd0fe9', 'skalldyr'),
    ('01a088b5-a202-73e1-98cd-73c0a3e2ab13', 'egg'),
    ('01a088b5-a203-7989-8c5f-350fde968207', 'fisk'),
    ('01a088b5-a204-79e5-a7d5-87f56997f62b', 'peanøtter'),
    ('01a088b5-a205-74ee-bd47-d1a76c314f75', 'soya'),
    ('01a088b5-a206-7be7-ad20-d6c030f674e2', 'melk'),
    ('01a088b5-a207-7ed0-a415-c76e91d69b15', 'laktose'),
    ('01a088b5-a208-799f-85a3-2448ce1f2cec', 'nøtter'),
    ('01a088b5-a209-7020-bf40-ae101e1237b6', 'selleri'),
    ('01a088b5-a20a-794f-bb8b-b945b54b0e6e', 'sennep'),
    ('01a088b5-a20b-787b-a0bb-f403f7c5b381', 'sesamfrø'),
    ('01a088b5-a20c-7cc2-8d0b-61c5430eece6', 'svoveldioksid og sulfitt'),
    ('01a088b5-a20d-7c2f-9352-53994077f81b', 'lupin'),
    ('01a088b5-a20e-7b4e-86d5-024595f72253', 'bløtdyr')
ON CONFLICT DO NOTHING;
