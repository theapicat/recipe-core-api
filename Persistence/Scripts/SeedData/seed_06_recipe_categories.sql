-- Seed: oppskriftskategorier (kun noen få til å begynne med - admin legger til flere).

INSERT INTO recipe_category (id, name, is_system) VALUES
    ('01a088b5-a200-776e-bf33-bfc81870e1d8', 'frokost', true),
    ('01a088b5-a201-70e7-b6ce-ba8acbc1bb4e', 'lunsj', true),
    ('01a088b5-a202-776c-8a5a-3262bce36fd7', 'middag', true),
    ('01a088b5-a203-7f31-8c66-aca826201cef', 'forrett', true),
    ('01a088b5-a204-7392-a6e0-aca43bc91ff3', 'tilbehør', true),
    ('01a088b5-a205-7fa5-86c5-d2c521e9b0ef', 'supper', true),
    ('01a088b5-a206-75bd-ac16-b9f4acaa5f55', 'salater', true),
    ('01a088b5-a207-7965-91f1-cd554d1f0830', 'bakst', true),
    ('01a088b5-a208-75b9-843b-934c04df3475', 'dessert', true),
    ('01a088b5-a209-7d12-85ba-a5c7e964c3ed', 'snacks og småretter', true),
    ('01a088b5-a20a-7c12-9181-2cc1ac96f4be', 'drikke', true),
    ('01a088b5-a20b-72af-8334-b3c285c1cd93', 'sauser og dressinger', true),
    ('01a088b5-a20c-7342-b28e-f956bb4f0514', 'annet', true)
ON CONFLICT DO NOTHING;
