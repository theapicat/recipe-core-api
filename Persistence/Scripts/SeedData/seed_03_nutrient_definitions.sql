-- Seed: næringsstoffer (Matvaretabellens katalog, 57 stoffer). Kjøres etter enhetene (seed_01) og næringsgruppene (seed_02).
-- Statisk og skrivebeskyttet: fylles kun her, ingen skrive-endepunkter. Navn beholder kildens store/små bokstaver.
-- unit_id og group_id er Guid-er (fremmednøkler til unit og nutrient_group) - kommentaren på slutten av hver rad viser
-- enhet og gruppe i klartekst. Hierarkiet ligger i gruppene, ikke i en parent_id på stoffene.
-- Første stoff i en undergruppe (Mettet, Enumet, Flerum, Vit A ...) er summen, resten er delverdiene.
-- sort_order = visningsrekkefølge i hele lista (gruppe for gruppe, dybde-først), 1-57.

INSERT INTO nutrient_definition (id, name, unit_id, decimal_precision, group_id, source_url, sort_order) VALUES
    ('Fett', 'Fett', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a200-78da-aeaa-c9b5d342efad', 'https://www.matvaretabellen.no/fett/', 1),  -- g | fett
    ('Trans', 'Transfettsyrer', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a200-78da-aeaa-c9b5d342efad', 'https://www.matvaretabellen.no/transfettsyrer/', 2),  -- g | fett
    ('Omega-3', 'Omega-3', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a200-78da-aeaa-c9b5d342efad', 'https://www.matvaretabellen.no/omega-3/', 3),  -- g | fett
    ('Omega-6', 'Omega-6', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a200-78da-aeaa-c9b5d342efad', 'https://www.matvaretabellen.no/omega-6/', 4),  -- g | fett
    ('Kolest', 'Kolesterol', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a200-78da-aeaa-c9b5d342efad', 'https://www.matvaretabellen.no/kolesterol/', 5),  -- mg | fett
    ('Mettet', 'Mettede fettsyrer', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a201-7820-ad67-26fbe59771ba', 'https://www.matvaretabellen.no/mettede-fettsyrer/', 6),  -- g | mettede fettsyrer
    ('C12:0Laurinsyre', 'C12:0 (laurinsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a201-7820-ad67-26fbe59771ba', 'https://www.matvaretabellen.no/c12-0-laurinsyre/', 7),  -- g | mettede fettsyrer
    ('C14:0Myristinsyre', 'C14:0 (myristinsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a201-7820-ad67-26fbe59771ba', 'https://www.matvaretabellen.no/c14-0-myristinsyre/', 8),  -- g | mettede fettsyrer
    ('C16:0Palmitinsyre', 'C16:0 (palmitinsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a201-7820-ad67-26fbe59771ba', 'https://www.matvaretabellen.no/c16-0-palmitinsyre/', 9),  -- g | mettede fettsyrer
    ('C18:0Stearinsyre', 'C18:0 (stearinsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a201-7820-ad67-26fbe59771ba', 'https://www.matvaretabellen.no/c18-0-stearinsyre/', 10),  -- g | mettede fettsyrer
    ('Enumet', 'Enumettede fettsyrer', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a202-774f-a7a4-cb018294d1d1', 'https://www.matvaretabellen.no/enumettede-fettsyrer/', 11),  -- g | enumettede fettsyrer
    ('C16:1', 'C16:1 sum (palmitoleinsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a202-774f-a7a4-cb018294d1d1', 'https://www.matvaretabellen.no/c16-1-sum-palmitoleinsyre/', 12),  -- g | enumettede fettsyrer
    ('C18:1', 'C18:1 sum (oljesyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a202-774f-a7a4-cb018294d1d1', 'https://www.matvaretabellen.no/c18-1-sum-oljesyre/', 13),  -- g | enumettede fettsyrer
    ('Flerum', 'Flerumettede fettsyrer', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/flerumettede-fettsyrer/', 14),  -- g | flerumettede fettsyrer
    ('C18:2n-6Linolsyre', 'C18:2n-6 (linolsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c18-2n-6-linolsyre/', 15),  -- g | flerumettede fettsyrer
    ('C18:3n-3AlfaLinolensyre', 'C18:3n-3 (alfalinolensyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c18-3n-3-alfalinolensyre/', 16),  -- g | flerumettede fettsyrer
    ('C20:3n-3Eikosatriensyre', 'C20:3n-3 (eikosatriensyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c20-3n-3-eikosatriensyre/', 17),  -- g | flerumettede fettsyrer
    ('C20:3n-6DihomoGammaLinolensyre', 'C20:3n-6 (dihomo-gamma-linolensyre, DGLA)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c20-3n-6-dihomo-gamma-linolensyre-dgla/', 18),  -- g | flerumettede fettsyrer
    ('C20:4n-3Eikosatetraensyre', 'C20:4n-3 (eikosatetraensyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c20-4n-3-eikosatetraensyre/', 19),  -- g | flerumettede fettsyrer
    ('C20:4n-6Arakidonsyre', 'C20:4n-6 (arakidonsyre)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c20-4n-6-arakidonsyre/', 20),  -- g | flerumettede fettsyrer
    ('C20:5n-3Eikosapentaensyre', 'C20:5n-3 (eikosapentaensyre, EPA)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c20-5n-3-eikosapentaensyre-epa/', 21),  -- g | flerumettede fettsyrer
    ('C22:5n-3Dokosapentaensyre', 'C22:5n-3 (dokosapentaensyre, DPA)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c22-5n-3-dokosapentaensyre-dpa/', 22),  -- g | flerumettede fettsyrer
    ('C22:6n-3Dokosaheksaensyre', 'C22:6n-3 (dokosaheksaensyre, DHA)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a203-7239-9775-753ca133086d', 'https://www.matvaretabellen.no/c22-6n-3-dokosaheksaensyre-dha/', 23),  -- g | flerumettede fettsyrer
    ('Karbo', 'Karbohydrat', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/karbohydrat/', 24),  -- g | karbohydrat
    ('Stivel', 'Stivelse', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/stivelse/', 25),  -- g | karbohydrat
    ('Mono+Di', 'Sukkerarter', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/sukkerarter/', 26),  -- g | karbohydrat
    ('Sukker', 'Sukker, tilsatt', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/sukker-tilsatt/', 27),  -- g | karbohydrat
    ('SUGAN', 'Sukker, fritt', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/sukker-fritt/', 28),  -- g | karbohydrat
    ('Fiber', 'Kostfiber', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a204-781b-b3b9-abfa7e93d347', 'https://www.matvaretabellen.no/kostfiber/', 29),  -- g | karbohydrat
    ('Protein', 'Protein', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a205-7a17-9745-8c2aad613708', 'https://www.matvaretabellen.no/protein/', 30),  -- g | protein
    ('Vit A', 'Vitamin A (RAE)', '01a088b5-a213-7baf-9667-5c4ed2d9dcfa', 0, '01a088b5-a207-754e-90c7-a2852ce8c3af', 'https://www.matvaretabellen.no/vitamin-a-rae/', 31),  -- µg RAE | vitamin a
    ('Vit A RE', 'Vitamin A (RE)', '01a088b5-a214-73e1-94c4-425acf404592', 0, '01a088b5-a207-754e-90c7-a2852ce8c3af', 'https://www.matvaretabellen.no/vitamin-a-re/', 32),  -- µg RE | vitamin a
    ('Retinol', 'Retinol', '01a088b5-a212-77d7-ae44-e70cf422b75a', 0, '01a088b5-a207-754e-90c7-a2852ce8c3af', 'https://www.matvaretabellen.no/retinol/', 33),  -- µg | vitamin a
    ('B-karo', 'Betakaroten', '01a088b5-a212-77d7-ae44-e70cf422b75a', 0, '01a088b5-a207-754e-90c7-a2852ce8c3af', 'https://www.matvaretabellen.no/betakaroten/', 34),  -- µg | vitamin a
    ('Vit B1', 'Vitamin B1 (tiamin)', '01a088b5-a211-71f1-b489-b996350034d2', 2, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b1-tiamin/', 35),  -- mg | vitamin b
    ('Vit B2', 'Vitamin B2 (riboflavin)', '01a088b5-a211-71f1-b489-b996350034d2', 2, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b2-riboflavin/', 36),  -- mg | vitamin b
    ('Niacin', 'Vitamin B3 (niacin)', '01a088b5-a211-71f1-b489-b996350034d2', 1, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b3-niacin/', 37),  -- mg | vitamin b
    ('NIAEQ', 'Niacinekvivalenter', '01a088b5-a211-71f1-b489-b996350034d2', 1, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/niacinekvivalenter/', 38),  -- mg | vitamin b
    ('Vit B6', 'Vitamin B6 (pyridoksin)', '01a088b5-a211-71f1-b489-b996350034d2', 2, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b6-pyridoksin/', 39),  -- mg | vitamin b
    ('Folat', 'Vitamin B9 (folat)', '01a088b5-a212-77d7-ae44-e70cf422b75a', 0, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b9-folat/', 40),  -- µg | vitamin b
    ('Vit B12', 'Vitamin B12 (kobalamin)', '01a088b5-a212-77d7-ae44-e70cf422b75a', 1, '01a088b5-a208-7788-ac9b-6f0b36afb96c', 'https://www.matvaretabellen.no/vitamin-b12-kobalamin/', 41),  -- µg | vitamin b
    ('Vit C', 'Vitamin C (askorbinsyre)', '01a088b5-a211-71f1-b489-b996350034d2', 1, '01a088b5-a209-78cb-b528-5bd58d53a38e', 'https://www.matvaretabellen.no/vitamin-c-askorbinsyre/', 42),  -- mg | vitamin c
    ('Vit D', 'Vitamin D', '01a088b5-a212-77d7-ae44-e70cf422b75a', 1, '01a088b5-a20a-75a6-8d05-aaedb7742f38', 'https://www.matvaretabellen.no/vitamin-d/', 43),  -- µg | vitamin d
    ('Vit E', 'Vitamin E', '01a088b5-a215-7181-beb4-2f912c5e3320', 1, '01a088b5-a20b-7fbd-bcda-c9921812e3e6', 'https://www.matvaretabellen.no/vitamin-e/', 44),  -- mg-ATE | vitamin e
    ('NaCl', 'Salt (NaCl)', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/salt-nacl/', 45),  -- g | mineraler
    ('Ca', 'Kalsium (Ca)', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/kalsium-ca/', 46),  -- mg | mineraler
    ('Na', 'Natrium (Na)', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/natrium-na/', 47),  -- mg | mineraler
    ('K', 'Kalium (K)', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/kalium-k/', 48),  -- mg | mineraler
    ('Mg', 'Magnesium (Mg)', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/magnesium-mg/', 49),  -- mg | mineraler
    ('P', 'Fosfor (P)', '01a088b5-a211-71f1-b489-b996350034d2', 0, '01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'https://www.matvaretabellen.no/fosfor-p/', 50),  -- mg | mineraler
    ('Fe', 'Jern (Fe)', '01a088b5-a211-71f1-b489-b996350034d2', 1, '01a088b5-a20d-732f-b656-0584d2507ed5', 'https://www.matvaretabellen.no/jern-fe/', 51),  -- mg | sporstoffer
    ('Zn', 'Sink (Zn)', '01a088b5-a211-71f1-b489-b996350034d2', 1, '01a088b5-a20d-732f-b656-0584d2507ed5', 'https://www.matvaretabellen.no/sink-zn/', 52),  -- mg | sporstoffer
    ('Se', 'Selen (Se)', '01a088b5-a212-77d7-ae44-e70cf422b75a', 0, '01a088b5-a20d-732f-b656-0584d2507ed5', 'https://www.matvaretabellen.no/selen-se/', 53),  -- µg | sporstoffer
    ('Cu', 'Kobber (Cu)', '01a088b5-a211-71f1-b489-b996350034d2', 2, '01a088b5-a20d-732f-b656-0584d2507ed5', 'https://www.matvaretabellen.no/kobber-cu/', 54),  -- mg | sporstoffer
    ('I', 'Jod (I)', '01a088b5-a212-77d7-ae44-e70cf422b75a', 0, '01a088b5-a20d-732f-b656-0584d2507ed5', 'https://www.matvaretabellen.no/jod-i/', 55),  -- µg | sporstoffer
    ('Vann', 'Vann', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 0, '01a088b5-a20e-7f2a-adf0-794d91b23e47', 'https://www.matvaretabellen.no/vann/', 56),  -- g | annet
    ('Alko', 'Alkohol', '01a088b5-a200-7d34-9e44-3e470ec50ebf', 1, '01a088b5-a20e-7f2a-adf0-794d91b23e47', 'https://www.matvaretabellen.no/alkohol/', 57)  -- g | annet
ON CONFLICT DO NOTHING;
