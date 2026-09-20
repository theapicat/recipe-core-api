-- Seed: næringsstoff-grupper. Hovedgrupper (fett, karbohydrat, protein, vitaminer, mineraler, sporstoffer, annet) og
-- undergrupper (mettede/enumettede/flerumettede fettsyrer under fett, vitamin a-e under vitaminer). Statisk og
-- skrivebeskyttet, og leses bare nøstet i næringsstoffet (ingen egne endepunkter). Id er Guid (fast), navn er små bokstaver.
-- sort_order er visningsrekkefølgen (dybde-først), foreldre står før barn. parent_group_id peker på hovedgruppen.

INSERT INTO nutrient_group (id, name, parent_group_id, sort_order) VALUES
    ('01a088b5-a200-78da-aeaa-c9b5d342efad', 'fett', NULL, 1),  -- hovedgruppe
    ('01a088b5-a201-7820-ad67-26fbe59771ba', 'mettede fettsyrer', '01a088b5-a200-78da-aeaa-c9b5d342efad', 2),  -- undergruppe av fett
    ('01a088b5-a202-774f-a7a4-cb018294d1d1', 'enumettede fettsyrer', '01a088b5-a200-78da-aeaa-c9b5d342efad', 3),  -- undergruppe av fett
    ('01a088b5-a203-7239-9775-753ca133086d', 'flerumettede fettsyrer', '01a088b5-a200-78da-aeaa-c9b5d342efad', 4),  -- undergruppe av fett
    ('01a088b5-a204-781b-b3b9-abfa7e93d347', 'karbohydrat', NULL, 5),  -- hovedgruppe
    ('01a088b5-a205-7a17-9745-8c2aad613708', 'protein', NULL, 6),  -- hovedgruppe
    ('01a088b5-a206-7c5d-bf95-99a9a60f413a', 'vitaminer', NULL, 7),  -- hovedgruppe
    ('01a088b5-a207-754e-90c7-a2852ce8c3af', 'vitamin a', '01a088b5-a206-7c5d-bf95-99a9a60f413a', 8),  -- undergruppe av vitaminer
    ('01a088b5-a208-7788-ac9b-6f0b36afb96c', 'vitamin b', '01a088b5-a206-7c5d-bf95-99a9a60f413a', 9),  -- undergruppe av vitaminer
    ('01a088b5-a209-78cb-b528-5bd58d53a38e', 'vitamin c', '01a088b5-a206-7c5d-bf95-99a9a60f413a', 10),  -- undergruppe av vitaminer
    ('01a088b5-a20a-75a6-8d05-aaedb7742f38', 'vitamin d', '01a088b5-a206-7c5d-bf95-99a9a60f413a', 11),  -- undergruppe av vitaminer
    ('01a088b5-a20b-7fbd-bcda-c9921812e3e6', 'vitamin e', '01a088b5-a206-7c5d-bf95-99a9a60f413a', 12),  -- undergruppe av vitaminer
    ('01a088b5-a20c-7ee4-86f7-59b7a9ff617d', 'mineraler', NULL, 13),  -- hovedgruppe
    ('01a088b5-a20d-732f-b656-0584d2507ed5', 'sporstoffer', NULL, 14),  -- hovedgruppe
    ('01a088b5-a20e-7f2a-adf0-794d91b23e47', 'annet', NULL, 15)  -- hovedgruppe
ON CONFLICT DO NOTHING;
