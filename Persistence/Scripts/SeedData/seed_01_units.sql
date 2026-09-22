-- Seed: enhetstyper og enheter. Kjerneenhetene under, pluss antall-enheter som er hentet fra Matvaretabellens
-- porsjonstyper (glass, boks, pose ...) og brukes av de importerte ingrediensene. Enhetene mg, µg, µg RAE, µg RE og
-- mg-ATE brukes av næringsstoffene (nutrient_definition.unit_id) - derfor kjøres dette skriptet før dem.
-- Vekt: basisenhet gram. Volum: basisenhet milliliter. Antall: ingen omregning (forholdstall 1) - gram per enhet
-- kommer fra ingrediensens porsjoner. Navn er små bokstaver og unike; forkortelsen er et symbol (µg, mg-ATE) og er unik
-- uavhengig av store/små bokstaver.

INSERT INTO unit_type (id, name, is_system) VALUES
    ('01a088b5-a200-713f-8326-46fedd9fe90d', 'vekt', true),
    ('01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 'volum', true),
    ('01a088b5-a202-7018-a543-7e2f99128ead', 'antall', true)
ON CONFLICT DO NOTHING;

INSERT INTO unit (id, name, abbreviation, unit_type_id, base_unit_ratio, is_system) VALUES
    ('01a088b5-a200-7d34-9e44-3e470ec50ebf', 'gram', 'g', '01a088b5-a200-713f-8326-46fedd9fe90d', 1, true),
    ('01a088b5-a201-7015-8120-b1090d86178c', 'hektogram', 'hg', '01a088b5-a200-713f-8326-46fedd9fe90d', 100, true),
    ('01a088b5-a202-72b3-af4f-e9789f4ddf02', 'kilogram', 'kg', '01a088b5-a200-713f-8326-46fedd9fe90d', 1000, true),
    ('01a088b5-a211-71f1-b489-b996350034d2', 'milligram', 'mg', '01a088b5-a200-713f-8326-46fedd9fe90d', 0.001, true),
    ('01a088b5-a212-77d7-ae44-e70cf422b75a', 'mikrogram', 'µg', '01a088b5-a200-713f-8326-46fedd9fe90d', 0.000001, true),
    ('01a088b5-a213-7baf-9667-5c4ed2d9dcfa', 'mikrogram retinolaktivitetsekvivalent', 'µg RAE', '01a088b5-a200-713f-8326-46fedd9fe90d', 0.000001, true),
    ('01a088b5-a214-73e1-94c4-425acf404592', 'mikrogram retinolekvivalent', 'µg RE', '01a088b5-a200-713f-8326-46fedd9fe90d', 0.000001, true),
    ('01a088b5-a215-7181-beb4-2f912c5e3320', 'milligram alfa-tokoferolekvivalent', 'mg-ATE', '01a088b5-a200-713f-8326-46fedd9fe90d', 0.001, true),
    ('01a088b5-a203-7b7f-aa54-a60fd90f3f38', 'milliliter', 'ml', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 1, true),
    ('01a088b5-a204-7758-a04d-67941fcf3681', 'centiliter', 'cl', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 10, true),
    ('01a088b5-a205-7f7c-9319-cf1910cf8f70', 'desiliter', 'dl', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 100, true),
    ('01a088b5-a206-7e72-9a9d-a24817b57f9f', 'liter', 'l', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 1000, true),
    ('01a088b5-a207-798e-b5ab-9561ba557116', 'teskje', 'ts', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 5, true),
    ('01a088b5-a208-7674-8a0f-f952da652b5e', 'spiseskje', 'ss', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 15, true),
    ('01a088b5-a209-7257-adec-6902e47d787f', 'kryddermål', 'krm', '01a088b5-a201-7d1c-86ed-0faaf8ff9a8d', 1, true),
    ('01a088b5-a20a-7d3d-aa68-334ab1955a48', 'stykk', 'stk', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a20b-7d6a-872c-97e2ca0acb2d', 'klype', 'klype', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a20c-77c4-830e-66ed67711355', 'bunt', 'bunt', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a20d-794d-8af8-7210bf653256', 'skive', 'skive', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a20e-7d12-a411-4259672c334b', 'fedd', 'fedd', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a20f-7538-ac1d-45c3043338ba', 'neve', 'neve', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a210-769f-bcaf-1bb67c426b32', 'pakke', 'pk', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5e8-7e4f-b77f-b44d028e5b4f', 'porsjon', 'porsjon', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5e9-7b86-b755-7573778dbe6c', 'kopp', 'kopp', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ea-792f-a0f5-e36ba0e65ad5', 'ring', 'ring', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5eb-71c7-9349-68e3c3196f0d', 'glass (stort)', 'glass (stort)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ec-7ea5-a778-9abeb96221e1', 'glass', 'glass', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ed-773d-b080-9f369f1f1031', 'glass (lite)', 'glass (lite)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ee-7aad-86bc-eddb31fdc0cd', 'båt', 'båt', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ef-7ecf-88b6-c8929f30ef21', 'beger', 'beger', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f0-75aa-8ef5-bb9d9f126a35', 'pr brødskive', 'pr brødskive', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f1-7cf6-ba97-328331a55542', 'stk (stor)', 'stk (stor)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f2-71d9-8c5c-c00ffeb4bf41', 'stk (liten)', 'stk (liten)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f3-7a14-9bbc-da7ab20fc887', 'bukett', 'bukett', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f4-72d6-8ab8-e3afa17aa2e1', 'boks', 'boks', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f5-71bf-92fd-a4df954e86cf', 'pose', 'pose', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f6-780d-92df-c6f8e99e4f94', 'ferdig skivet', 'ferdig skivet', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f7-72a5-9af6-94c6f6e6020b', 'boks (liten)', 'boks (liten)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f8-7de7-b25a-e4bfbd417450', 'stk (middels)', 'stk (middels)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5f9-72f7-9dd4-9acd5c1679e3', 'kartong', 'kartong', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5fa-7f07-86c3-c0d767553599', 'cm rot', 'cm rot', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5fb-7f21-a3f1-baf08eabda28', 'blad', 'blad', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5fc-7c07-a47f-42a74927b865', 'filet', 'filet', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5fd-7258-adb2-e2a1f550354b', 'stang', 'stang', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5fe-7893-a054-081c34ecd198', 'plate', 'plate', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a5ff-7f3b-9925-de4519ed9509', 'plate (liten)', 'plate (liten)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a600-75dd-83ac-28d06ef4bdcb', 'plate (stor)', 'plate (stor)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a601-7cca-add7-a08dfd901c68', 'plate (middels)', 'plate (middels)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a602-7c79-8c99-92931fddd9f5', 'terning', 'terning', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a603-7482-aa69-b77d662cee44', 'pose (stor)', 'pose (stor)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a604-76b3-87cd-a594be016e5f', 'pose (liten)', 'pose (liten)', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true),
    ('01a088b5-a605-7fac-a478-c2d4434d3981', 'stilk', 'stilk', '01a088b5-a202-7018-a543-7e2f99128ead', 1, true)
ON CONFLICT DO NOTHING;
