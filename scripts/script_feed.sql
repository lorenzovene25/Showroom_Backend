-- ============================================================
-- ============================================================
--  SEED DATA
-- ============================================================
-- ============================================================


-- ============================================================
-- CARTS (uno per utente)
-- ============================================================
INSERT INTO carts
SELECT FROM generate_series(1, 5)
RETURNING id;

-- ============================================================
-- USERS con cart già collegato
-- ============================================================
INSERT INTO users (first_name, last_name, email, password_hash, is_admin, cart_id) VALUES
('Gabriele', 'Rossi',     'gabrielerossi@mail.it',    'password', FALSE, 1),
('Navpreet', 'Singh',     'navpreetsingh@mail.it',    'password', FALSE, 2),
('Lorenzo',  'Veneruzzo', 'lorenzoveneruzzo@mail.it', 'password', FALSE, 3),
('Ruben',    'Ranghiuc',  'rubenranghiuc@mail.it',    'password', FALSE, 4),
('Admin',    'Admin',     'admin@mail.it',             'admin', TRUE, 5);


-- ============================================================
-- EXHIBITIONS
-- ============================================================
INSERT INTO exhibitions (name, location, maps_url, status, start_date, end_date, image_url) VALUES
('Robert Doisneau',    'Galleria Harry Bertoia, Pordenone', 'https://maps.app.goo.gl/oJsGPYMUEjJfkkDj6', 'ongoing', '2025-11-22', '2026-04-06', '/media/Robert_Doisneau/preview.png'),
('Olivia Arthur',      'Galleria Harry Bertoia, Pordenone', 'https://maps.app.goo.gl/oJsGPYMUEjJfkkDj6', 'ongoing', '2025-11-22', '2026-04-26', '/media/Olivia_Arthur/preview.png'),
('Stefanie Moshammer', 'Galleria Harry Bertoia, Pordenone', 'https://maps.app.goo.gl/oJsGPYMUEjJfkkDj6', 'ongoing', '2026-02-14', '2026-04-26', '/media/Stefanie_Moshammer/preview.png'),
('Seiichi Furuya',     'Galleria Harry Bertoia, Pordenone', 'https://maps.app.goo.gl/oJsGPYMUEjJfkkDj6', 'past',    '2025-11-22', '2026-02-08', '/media/Seiichi_Furuya/preview.png');
-- id: 1=Doisneau  2=Arthur  3=Moshammer  4=Furuya


-- ============================================================
-- EXHIBITION_TRANSLATIONS  (EN + IT per ogni mostra)
-- ============================================================
INSERT INTO exhibition_translations (exhibition_id, language_code, title, description) VALUES

-- Robert Doisneau
(1, 'en', 'Robert Doisneau – The Poetry of the Street',
 'A retrospective dedicated to Robert Doisneau, one of the most celebrated French photographers of the 20th century. Spanning over five decades of work, the exhibition brings together iconic images of Parisian life, capturing everyday moments with warmth, irony and humanity. From the lovers frozen in a kiss to the children playing in the streets, Doisneau''s gaze transformed the ordinary into poetry.'),
(1, 'it', 'Robert Doisneau – La Poesia della Strada',
 'Una retrospettiva dedicata a Robert Doisneau, uno dei fotografi francesi più celebri del XX secolo. Attraverso oltre cinque decenni di lavoro, la mostra raccoglie immagini iconiche della vita parigina, catturando momenti quotidiani con calore, ironia e umanità. Dagli amanti immortalati in un bacio ai bambini che giocano per strada, lo sguardo di Doisneau ha trasformato l''ordinario in poesia.'),

-- Olivia Arthur
(2, 'en', 'Olivia Arthur – Between Worlds',
 'Olivia Arthur is a British photographer and Magnum member whose work explores the lives of women living between cultures. This exhibition brings together images from her projects in the Persian Gulf, India and Europe, revealing the tensions and freedoms found at the intersection of tradition and modernity.'),
(2, 'it', 'Olivia Arthur – Tra Due Mondi',
 'Olivia Arthur è una fotografa britannica e membro di Magnum Photos il cui lavoro esplora le vite delle donne che vivono tra culture diverse. Questa mostra raccoglie immagini dei suoi progetti nel Golfo Persico, in India e in Europa, rivelando le tensioni e le libertà che si incontrano al crocevia tra tradizione e modernità.'),

-- Stefanie Moshammer
(3, 'en', 'Stefanie Moshammer – Lucky You',
 'Austrian photographer Stefanie Moshammer turns her lens on outsiders, dreamers and those living on the margins of society. Her intimate and colourful images oscillate between documentary and fiction, building a universe of characters who drift through neon-lit nights and sun-bleached afternoons.'),
(3, 'it', 'Stefanie Moshammer – Lucky You',
 'La fotografa austriaca Stefanie Moshammer punta il suo obiettivo sugli emarginati, i sognatori e coloro che vivono ai margini della società. Le sue immagini intime e colorate oscillano tra documentario e finzione, costruendo un universo di personaggi che si muovono tra notti al neon e pomeriggi sbiaditi dal sole.'),

-- Seiichi Furuya
(4, 'en', 'Seiichi Furuya – Mémoires',
 'A deeply personal retrospective by Japanese photographer Seiichi Furuya, chronicling fifteen years of photographs of his wife Christine Gössler. Shot across divided Europe from 1978 to 1985, the series is an intimate portrait of a woman, a marriage, and a continent on the brink of transformation.'),
(4, 'it', 'Seiichi Furuya – Mémoires',
 'Una retrospettiva profondamente personale del fotografo giapponese Seiichi Furuya, che documenta quindici anni di fotografie della moglie Christine Gössler. Scattata attraverso un''Europa divisa dal 1978 al 1985, la serie è un ritratto intimo di una donna, di un matrimonio e di un continente sull''orlo della trasformazione.');


-- ============================================================
-- EXHIBITION_TIME_SLOTS ( quelli sotto sono gli orari della mostra di Doisneau, replicati per tutte le mostre)
--   Mer–Ven (3,4,5)  -> 15:00–19:00
--   Sab–Dom (6,7)    -> 10:00–13:00
--   Sab–Dom (6,7)    -> 15:00–19:00
-- slot.id per Doisneau: 1,2,3 | Arthur: 4,5,6 | Moshammer: 7,8,9 | Furuya: 10,11,12
-- ============================================================
INSERT INTO exhibition_time_slots (exhibition_id, days_of_week, start_time, end_time, max_capacity) VALUES
(1, ARRAY[3,4,5], '15:00', '19:00', 50),
(1, ARRAY[6,7],   '10:00', '13:00', 50),
(1, ARRAY[6,7],   '15:00', '19:00', 50),
(2, ARRAY[3,4,5], '15:00', '19:00', 50),
(2, ARRAY[6,7],   '10:00', '13:00', 50),
(2, ARRAY[6,7],   '15:00', '19:00', 50),
(3, ARRAY[3,4,5], '15:00', '19:00', 50),
(3, ARRAY[6,7],   '10:00', '13:00', 50),
(3, ARRAY[6,7],   '15:00', '19:00', 50),
(4, ARRAY[3,4,5], '15:00', '19:00', 50),
(4, ARRAY[6,7],   '10:00', '13:00', 50),
(4, ARRAY[6,7],   '15:00', '19:00', 50);


-- ============================================================
-- ARTWORKS — 20 opere più celebri di Robert Doisneau
-- ============================================================
INSERT INTO artworks (archive_id, name, year, dimensions, image_url) VALUES
('RD001', 'Mademoiselle Anita, Cabaret La Boule Rouge',    1950, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD001.jpg'),
('RD002', 'Accordion Street Girl',                        1948, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD002.jpg'),
('RD003', 'Café Noir et Blanc',                           1949, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD003.jpg'),
('RD004', 'Hell (L''Enfer)',                              1952, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD004.jpg'),
('RD005', 'Children of Place Hébert',                     1945, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD005.jpg'),
('RD006', 'School Information',                           1953, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD006.jpg'),
('RD007', 'The Drunken Kiss',                             1953, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD007.jpg'),
('RD008', 'The Kiss by the Hôtel de Ville',               1955, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD008.jpg'),
('RD009', 'The Last Waltz of July 14',                    1956, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD009.jpg'),
('RD010', 'The Bicycle of Spring',                        1957, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD010.jpg'),
('RD011', 'Children of Place Hébert',                     1958, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD011.jpg'),
('RD012', 'Aprons of Rivoli',                             1945, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD012.jpg'),
('RD013', 'The Brothers, Rue du Docteur Lecène',          1950, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD013.jpg'),
('RD014', 'Musician Under the Rain',                      1953, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD014.jpg'),
('RD015', 'Pablo Picasso and the Little Breads',          1945, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD015.jpg'),
('RD016', 'Self-Portrait, 1949',                          1953, '40 × 50 cm', '/media/Robert_Doisneau/artworks/RD016.jpg'),
('RD017', 'The Little Balcony',                           1953, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD017.jpg'),
('RD018', 'Schoolchildren in Rue Damesme',                1952, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD018.jpg'),
('RD019', 'Tina Aumont',                                  1954, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD019.jpg'),
('RD020', 'An Oblique Look',                              1945, '30 × 40 cm', '/media/Robert_Doisneau/artworks/RD020.jpg');


-- ============================================================
-- ARTWORK_TRANSLATIONS  (EN + IT)
-- ============================================================
INSERT INTO artwork_translations (artwork_id, language_code, title, description, historical_period, support, camera) VALUES

-- RD001
(1,'en','Mademoiselle Anita, Cabaret La Boule Rouge',
 'A lively portrait of a cabaret performer captured in the vibrant nightlife of Paris.',
 'Post-war Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(1,'it','Mademoiselle Anita, Cabaret La Boule Rouge',
 'Un ritratto vivace di una performer di cabaret catturata nella vivace vita notturna di Parigi.',
 'Parigi del dopoguerra, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD002
(2,'en','Accordion Street Girl',
 'A young girl plays the accordion on a Paris street, embodying the musical spirit of the city.',
 'Post-war Paris, late 1940s','Gelatin silver print','Rolleiflex 2.8'),
(2,'it','La Ragazza con la Fisarmonica',
 'Una giovane ragazza suona la fisarmonica in una strada di Parigi, incarnando lo spirito musicale della città.',
 'Parigi del dopoguerra, fine anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD003
(3,'en','Café Noir et Blanc',
 'Customers sit quietly in a Parisian café, contrasting light and shadow in everyday urban life.',
 'Post-war Paris, 1940s','Gelatin silver print','Rolleiflex 2.8'),
(3,'it','Café Noir et Blanc',
 'I clienti siedono in silenzio in un caffè parigino, in un gioco di contrasti tra luce e ombra nella vita urbana quotidiana.',
 'Parigi del dopoguerra, anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD004
(4,'en','Hell (L''Enfer)',
 'A humorous and ironic street scene reflecting the chaos and character of city life.',
 'Post-war France, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(4,'it','L''Inferno (L''Enfer)',
 'Una scena di strada umoristica e ironica che riflette il caos e il carattere della vita urbana.',
 'Francia del dopoguerra, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD005
(5,'en','Children of Place Hébert',
 'A group of children gather in a working-class neighborhood square, full of energy and curiosity.',
 'Post-war Paris, 1940s','Gelatin silver print','Rolleiflex 2.8'),
(5,'it','I Bambini di Place Hébert',
 'Un gruppo di bambini si raduna in una piazza di quartiere popolare, piena di energia e curiosità.',
 'Parigi del dopoguerra, anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD006
(6,'en','School Information',
 'Students engage with school materials, highlighting the importance of education in postwar France.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(6,'it','Informazioni Scolastiche',
 'Gli studenti interagiscono con materiale scolastico, sottolineando l''importanza dell''istruzione nella Francia del dopoguerra.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD007
(7,'en','The Drunken Kiss',
 'A playful couple shares a spontaneous kiss, capturing humor and romance in public life.',
 'Suburban Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(7,'it','Il Bacio Ubriaco',
 'Una coppia giocosa condivide un bacio spontaneo, catturando umorismo e romanticismo nella vita pubblica.',
 'Periferia di Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD008
(8,'en','The Kiss by the Hôtel de Ville',
 'A young couple kisses on a busy Paris street, symbolizing love and romance in the city.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(8,'it','Il Bacio all''Hôtel de Ville',
 'Una giovane coppia si bacia in una trafficata strada di Parigi, simbolo dell''amore e del romanticismo in città.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD009
(9,'en','The Last Waltz of July 14',
 'Couples dance during Bastille Day celebrations, marking the end of a festive evening.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(9,'it','L''Ultimo Valzer del 14 Luglio',
 'Le coppie ballano durante i festeggiamenti del 14 luglio, segnando la fine di una serata di festa.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD010
(10,'en','The Bicycle of Spring',
 'A bicycle rests among blooming surroundings, celebrating the arrival of spring in Paris.',
 'Paris, Saint-Germain-des-Prés, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(10,'it','La Bicicletta della Primavera',
 'Una bicicletta riposa tra un ambiente in fiore, celebrando l''arrivo della primavera a Parigi.',
 'Parigi, Saint-Germain-des-Prés, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD011
(11,'en','Children of Place Hébert',
 'Children play freely in a neighborhood square, reflecting innocence and community life.',
 'Paris, late 1950s','Gelatin silver print','Rolleiflex 2.8'),
(11,'it','I Bambini di Place Hébert',
 'I bambini giocano liberamente in una piazza di quartiere, riflettendo l''innocenza e la vita comunitaria.',
 'Parigi, fine anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD012
(12,'en','Aprons of Rivoli',
 'Shop workers wearing aprons stand along Rue de Rivoli, representing everyday commerce.',
 'Suburban Paris, post-war 1940s','Gelatin silver print','Rolleiflex 2.8'),
(12,'it','I Grembiuli di Rivoli',
 'Lavoratori di negozi con i grembiuli si trovano lungo Rue de Rivoli, a rappresentare il commercio quotidiano.',
 'Periferia di Parigi, dopoguerra anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD013
(13,'en','The Brothers, Rue du Docteur Lecène',
 'Two young brothers pose proudly in a modest Parisian street.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(13,'it','I Fratelli, Rue du Docteur Lecène',
 'Due giovani fratelli posano con orgoglio in una modesta strada parigina.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD014
(14,'en','Musician Under the Rain',
 'A musician continues to perform despite the rain, showing dedication to his craft.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(14,'it','Il Musicista sotto la Pioggia',
 'Un musicista continua a suonare nonostante la pioggia, mostrando dedizione al suo mestiere.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD015
(15,'en','Pablo Picasso and the Little Breads',
 'Artist Pablo Picasso playfully poses with loaves of bread arranged like his hands.',
 'Suburban Paris, 1940s','Gelatin silver print','Rolleiflex 2.8'),
(15,'it','Pablo Picasso e i Panini',
 'L''artista Pablo Picasso posa in modo giocoso con dei panini disposti come le sue mani.',
 'Periferia di Parigi, anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD016
(16,'en','Self-Portrait, 1949',
 'The photographer turns the camera on himself in a thoughtful and introspective pose.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(16,'it','Autoritratto, 1949',
 'Il fotografo rivolge la macchina su se stesso in una posa riflessiva e introspettiva.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD017
(17,'en','The Little Balcony',
 'A quiet domestic scene observed from a balcony in a Paris apartment.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(17,'it','Il Piccolo Balcone',
 'Una tranquilla scena domestica osservata da un balcone di un appartamento parigino.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD018
(18,'en','Schoolchildren in Rue Damesme',
 'Children walk together through a Paris street on their way to school.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(18,'it','Scolari in Rue Damesme',
 'I bambini camminano insieme lungo una strada di Parigi diretti a scuola.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD019
(19,'en','Tina Aumont',
 'A portrait of actress Tina Aumont, capturing elegance and personality.',
 'Paris, 1950s','Gelatin silver print','Rolleiflex 2.8'),
(19,'it','Tina Aumont',
 'Un ritratto dell''attrice Tina Aumont, che cattura eleganza e personalità.',
 'Parigi, anni ''50','Stampa ai sali d''argento','Rolleiflex 2.8'),

-- RD020
(20,'en','An Oblique Look',
 'A humorous moment as a passerby glances sideways at a shop window display.',
 'Paris, 1940s','Gelatin silver print','Rolleiflex 2.8'),
(20,'it','Uno Sguardo Obliquo',
 'Un momento umoristico in cui un passante lancia uno sguardo di traverso a una vetrina di negozio.',
 'Parigi, anni ''40','Stampa ai sali d''argento','Rolleiflex 2.8');


-- ============================================================
-- ARTWORK_EXHIBITIONS — tutte le 20 opere → mostra Doisneau (id=1)
-- ============================================================
INSERT INTO artwork_exhibitions (artwork_id, exhibition_id) VALUES
(1,1),(2,1),(3,1),(4,1),(5,1),(6,1),(7,1),(8,1),(9,1),(10,1),
(11,1),(12,1),(13,1),(14,1),(15,1),(16,1),(17,1),(18,1),(19,1),(20,1);


-- ============================================================
-- TICKET_TIERS
-- ============================================================
INSERT INTO ticket_tiers (type, description, price) VALUES
('full',    'Full-price admission for all adults.',                                          12.00),
('reduced', 'Reduced admission for groups of 10+, disabled visitors and their companions.',   9.00),
('student', 'Student admission — valid student ID required.',                                 5.00),
('free',    'Free admission for children under 10 and seniors over 65.',                      0.00);
-- id: 1=full  2=reduced  3=student  4=free


-- ============================================================
-- TICKET_TIER_TRANSLATIONS  (EN + IT)
-- ============================================================
INSERT INTO ticket_tier_translations (ticket_tier_id, language_code, name, description) VALUES
(1,'en','Full Price',  'Full-price admission for all adults.'),
(1,'it','Intero',      'Ingresso a prezzo intero per tutti gli adulti.'),
(2,'en','Reduced',     'Reduced admission for groups of 10 or more, disabled visitors and their companions.'),
(2,'it','Ridotto',     'Ingresso ridotto per gruppi di 10 o più persone, visitatori con disabilità e accompagnatori.'),
(3,'en','Student',     'Student admission — valid student ID required at the entrance.'),
(3,'it','Studenti',    'Ingresso per studenti — tesserino universitario valido richiesto all''ingresso.'),
(4,'en','Free',        'Free admission for children under 10 and seniors over 65.'),
(4,'it','Gratuito',    'Ingresso gratuito per bambini sotto i 10 anni e anziani over 65.');


-- ============================================================
-- TICKETS
--   slot per Doisneau: 1=Mer-Ven 15-19 | 2=Sab-Dom 10-13 | 3=Sab-Dom 15-19
--   slot per Arthur:   4=Mer-Ven 15-19 | 5=Sab-Dom 10-13 | 6=Sab-Dom 15-19
--   slot per Moshammer:7=Mer-Ven 15-19 | 8=Sab-Dom 10-13 | 9=Sab-Dom 15-19
--   slot per Furuya:  10=Mer-Ven 15-19 |11=Sab-Dom 10-13 |12=Sab-Dom 15-19
-- ============================================================
INSERT INTO tickets (exhibition_id, tier_id, user_id, visit_date, time_slot_id, purchased_at) VALUES
-- Gabriele: Doisneau sabato mattina (slot 2)
(1, 1, 1, '2026-03-14', 2,  '2026-03-05 10:15:00'),
-- Navpreet: Doisneau venerdì pomeriggio (slot 1)
(1, 3, 2, '2026-03-20', 1,  '2026-03-10 14:22:00'),
-- Lorenzo: Arthur domenica mattina (slot 5)
(2, 2, 3, '2026-03-22', 5,  '2026-03-12 09:07:00'),
-- Ruben: Doisneau domenica pomeriggio (slot 3)
(1, 1, 4, '2026-03-15', 3,  '2026-03-08 18:33:00'),
-- Gabriele: anche Arthur mercoledì pomeriggio (slot 4)
(2, 1, 1, '2026-03-18', 4,  '2026-03-05 10:17:00'),
-- Navpreet: Moshammer sabato mattina (slot 8)
(3, 3, 2, '2026-03-21', 8,  '2026-03-11 20:55:00'),
-- Lorenzo: Furuya (passata) — dato storico (slot 10)
(4, 2, 3, '2026-01-15', 10, '2026-01-10 11:40:00'),
-- Ruben: Furuya (passata) — dato storico (slot 11)
(4, 1, 4, '2026-01-18', 11, '2026-01-12 16:20:00');


-- ============================================================
-- CATEGORIES
-- ============================================================
INSERT INTO categories (slug, name, description) VALUES
('prints',      'Fine Art Prints',    'High-quality photographic prints from the exhibition, available in various sizes.'),
('books',       'Books & Catalogues', 'Exhibition catalogues, monographs and photo books dedicated to the featured photographers.'),
('postcards',   'Postcards',          'A selection of postcards reproducing iconic images from the exhibition.'),
('apparel',     'Apparel',            'Clothing items featuring artwork and branding from the exhibition.'),
('accessories', 'Accessories',        'A range of accessories and everyday items featuring exhibition imagery.');
-- id: 1=prints  2=books  3=postcards  4=apparel  5=accessories


-- ============================================================
-- SOUVENIRS
-- ============================================================
INSERT INTO souvenirs (archive_id, category_id, price, in_stock, quantity_available, image_url, specifications) VALUES
-- PRINTS
('RD-P001', 1, 29.90, TRUE,  40, '/media/Robert_Doisneau/souvenirs/RD-P001.jpg',
 '{"size":"30x40 cm","paper":"Hahnemuhle Photo Rag 308g","finish":"matte","framed":false}'),
('RD-P002', 1, 29.90, TRUE,  35, '/media/Robert_Doisneau/souvenirs/RD-P002.jpg',
 '{"size":"30x40 cm","paper":"Hahnemuhle Photo Rag 308g","finish":"matte","framed":false}'),
('RD-P003', 1, 59.90, TRUE,  15, '/media/Robert_Doisneau/souvenirs/RD-P003.jpg',
 '{"size":"50x70 cm","paper":"Hahnemuhle Photo Rag 308g","finish":"matte","framed":false}'),
-- BOOKS
('RD-B001', 2, 35.00, TRUE,  25, '/media/Robert_Doisneau/souvenirs/RD-B001.jpg',
 '{"pages":224,"dimensions":"24x28 cm","languages":["it","en"],"publisher":"Silvana Editoriale","cover":"hardcover"}'),
('RD-B002', 2, 45.00, TRUE,  20, '/media/Robert_Doisneau/souvenirs/RD-B002.jpg',
 '{"pages":320,"dimensions":"28x32 cm","languages":["en","fr"],"publisher":"Thames & Hudson","cover":"hardcover"}'),
('RD-B003', 2, 55.00, FALSE,  0, '/media/Robert_Doisneau/souvenirs/RD-B003.jpg',
 '{"pages":400,"dimensions":"30x34 cm","languages":["it","en","fr"],"publisher":"Contrasto","cover":"hardcover"}'),
-- POSTCARDS
('RD-C001', 3,  9.90, TRUE, 100, '/media/Robert_Doisneau/souvenirs/RD-C001.jpg',
 '{"quantity":10,"size":"10.5x14.8 cm","finish":"glossy"}'),
('RD-C002', 3,  1.50, TRUE, 200, '/media/Robert_Doisneau/souvenirs/RD-C002.jpg',
 '{"quantity":1,"size":"10.5x14.8 cm","finish":"glossy","image":"The Kiss by the Hotel de Ville"}'),
('RD-C003', 3,  1.50, TRUE, 150, '/media/Robert_Doisneau/souvenirs/RD-C003.jpg',
 '{"quantity":1,"size":"10.5x14.8 cm","finish":"glossy","image":"The Sidelong Glance"}'),
-- APPAREL
('RD-A001', 4, 25.00, TRUE,  50, '/media/Robert_Doisneau/souvenirs/RD-A001.jpg',
 '{"type":"t-shirt","material":"100% organic cotton","sizes":["XS","S","M","L","XL","XXL"],"colour":"white"}'),
('RD-A002', 4, 15.00, TRUE,  60, '/media/Robert_Doisneau/souvenirs/RD-A002.jpg',
 '{"type":"tote bag","material":"100% cotton canvas","dimensions":"38x42 cm","colour":"natural"}'),
-- ACCESSORIES
('RD-X001', 5, 12.00, TRUE,  80, '/media/Robert_Doisneau/souvenirs/RD-X001.jpg',
 '{"type":"mug","material":"ceramic","volume_ml":330,"dishwasher_safe":true,"microwave_safe":true}'),
('RD-X002', 5,  8.00, TRUE, 120, '/media/Robert_Doisneau/souvenirs/RD-X002.jpg',
 '{"type":"magnet set","quantity":5,"size_each":"5x7 cm","material":"flexible magnet"}'),
('RD-X003', 5, 10.00, TRUE,  70, '/media/Robert_Doisneau/souvenirs/RD-X003.jpg',
 '{"type":"notebook","pages":192,"dimensions":"14x21 cm","cover":"softcover","ruling":"lined"}');
-- souvenir.id: 1=RD-P001  2=RD-P002  3=RD-P003  4=RD-B001  5=RD-B002  6=RD-B003
--              7=RD-C001  8=RD-C002  9=RD-C003  10=RD-A001 11=RD-A002
--             12=RD-X001 13=RD-X002 14=RD-X003


-- ============================================================
-- SOUVENIRS_TRANSLATIONS  (EN + IT)
-- ============================================================
INSERT INTO souvenirs_translations (souvenir_id, language_code, name, short_description, full_description, specifications) VALUES

-- RD-P001
(1,'en','Fine Art Print – "The Kiss by the Hotel de Ville" (30×40 cm)',
 'Museum-quality fine art print of Doisneau''s most iconic photograph.',
 'A high-quality fine art print of Robert Doisneau''s legendary photograph "The Kiss by the Hotel de Ville" (1950), printed on Hahnemühle Photo Rag 308g matte paper using archival pigment inks for colour stability exceeding 100 years. Supplied unframed.',
 '{"dimensione":"30x40 cm","carta":"Hahnemuhle Photo Rag 308g","finitura":"opaca","cornice":false}'),
(1,'it','Stampa Fine Art – "Il Bacio all''Hotel de Ville" (30×40 cm)',
 'Stampa fine art di qualità museale della fotografia più iconica di Doisneau.',
 'Una stampa fine art di alta qualità della leggendaria fotografia di Robert Doisneau "Il Bacio all''Hotel de Ville" (1950), stampata su carta opaca Hahnemühle Photo Rag 308g con inchiostri pigmentati di archivio per una stabilità del colore superiore ai 100 anni. Fornita senza cornice.',
 '{"dimensione":"30x40 cm","carta":"Hahnemuhle Photo Rag 308g","finitura":"opaca","cornice":false}'),

-- RD-P002
(2,'en','Fine Art Print – "The Sidelong Glance" (30×40 cm)',
 'Museum-quality fine art print of Doisneau''s most playful street photograph.',
 'A high-quality fine art print of Robert Doisneau''s celebrated photograph "The Sidelong Glance" (1948), printed on Hahnemühle Photo Rag 308g matte paper with archival pigment inks. Supplied unframed.',
 '{"size":"30x40 cm","paper":"Hahnemuhle Photo Rag 308g","finish":"matte","framed":false}'),
(2,'it','Stampa Fine Art – "Lo Sguardo Obliquo" (30×40 cm)',
 'Stampa fine art di qualità museale della fotografia di strada più celebre di Doisneau.',
 'Una stampa fine art di alta qualità della celebre fotografia di Robert Doisneau "Lo Sguardo Obliquo" (1948), stampata su carta opaca Hahnemühle Photo Rag 308g con inchiostri pigmentati di archivio. Fornita senza cornice.',
 '{"dimensione":"30x40 cm","carta":"Hahnemuhle Photo Rag 308g","finitura":"opaca","cornice":false}'),

-- RD-P003
(3,'en','Large Format Fine Art Print – "The Kiss by the Hotel de Ville" (50×70 cm)',
 'Large format museum-quality fine art print — ideal as a statement piece.',
 'A large format fine art print of Robert Doisneau''s legendary photograph "The Kiss by the Hotel de Ville" (1950), printed on Hahnemühle Photo Rag 308g matte paper with archival pigment inks. Supplied unframed.',
 '{"size":"50x70 cm","paper":"Hahnemuhle Photo Rag 308g","finish":"matte","framed":false}'),
(3,'it','Stampa Fine Art Grande Formato – "Il Bacio all''Hotel de Ville" (50×70 cm)',
 'Stampa fine art grande formato di qualità museale — ideale come pezzo d''impatto.',
 'Una stampa fine art grande formato della leggendaria fotografia di Robert Doisneau "Il Bacio all''Hotel de Ville" (1950), stampata su carta opaca Hahnemühle Photo Rag 308g con inchiostri pigmentati di archivio. Fornita senza cornice.',
 '{"dimensione":"50x70 cm","carta":"Hahnemuhle Photo Rag 308g","finitura":"opaca","cornice":false}'),

-- RD-B001
(4,'en','Robert Doisneau: A Life in Pictures – Exhibition Catalogue',
 'The official bilingual catalogue of the retrospective, with 150 full-colour works and critical essays.',
 'The official catalogue of the Robert Doisneau retrospective at the Galleria Harry Bertoia. Features 150 full-colour reproductions accompanied by critical essays by leading photography scholars. Bilingual edition (Italian/English). Published by Silvana Editoriale. 224 pages, 24×28 cm, hardcover.',
 '{"pagine":224,"dimensioni":"24x28 cm","lingue":["it","en"],"editore":"Silvana Editoriale","copertina":"rigida"}'),
(4,'it','Robert Doisneau: Una Vita in Fotografie – Catalogo della Mostra',
 'Il catalogo ufficiale bilingue della retrospettiva, con 150 opere a colori e saggi critici.',
 'Il catalogo ufficiale della retrospettiva di Robert Doisneau alla Galleria Harry Bertoia. Presenta 150 riproduzioni a colori accompagnate da saggi critici dei principali studiosi di fotografia. Edizione bilingue (italiano/inglese). Pubblicato da Silvana Editoriale. 224 pagine, 24×28 cm, copertina rigida.',
 '{"pagine":224,"dimensioni":"24x28 cm","lingue":["it","en"],"editore":"Silvana Editoriale","copertina":"rigida"}'),

-- RD-B002
(5,'en','Doisneau: Paris – Thames & Hudson Monograph',
 'The definitive monograph on Doisneau''s Paris photographs, published by Thames & Hudson.',
 'The essential monograph on Robert Doisneau''s work in Paris, featuring over 300 photographs spanning five decades with an introductory essay by Brigitte Ollier. English/French bilingual edition. Published by Thames & Hudson. 320 pages, 28×32 cm, hardcover.',
 '{"pages":320,"dimensions":"28x32 cm","languages":["en","fr"],"publisher":"Thames & Hudson","cover":"hardcover"}'),
(5,'it','Doisneau: Parigi – Monografia Thames & Hudson',
 'La monografia definitiva sulle fotografie parigine di Doisneau, pubblicata da Thames & Hudson.',
 'La monografia essenziale sul lavoro di Robert Doisneau a Parigi, con oltre 300 fotografie che abbracciano cinque decenni e un saggio introduttivo di Brigitte Ollier. Edizione bilingue inglese/francese. Pubblicata da Thames & Hudson. 320 pagine, 28×32 cm, copertina rigida.',
 '{"pagine":320,"dimensioni":"28x32 cm","lingue":["en","fr"],"editore":"Thames & Hudson","copertina":"rigida"}'),

-- RD-B003
(6,'en','Doisneau: The Retrospective – Contrasto',
 'The most comprehensive monograph ever published on Doisneau''s complete works. Currently out of stock.',
 'The most comprehensive retrospective monograph ever published on Robert Doisneau''s complete body of work. Over 400 photographs spanning five decades, from early industrial work for Renault to his celebrated street photography. Trilingual edition (Italian/English/French). Published by Contrasto. 400 pages, 30×34 cm, hardcover. Currently out of stock.',
 '{"pages":400,"dimensions":"30x34 cm","languages":["it","en","fr"],"publisher":"Contrasto","cover":"hardcover"}'),
(6,'it','Doisneau: La Retrospettiva – Contrasto',
 'La monografia retrospettiva più completa mai pubblicata sull''opera di Doisneau. Attualmente esaurita.',
 'La monografia retrospettiva più completa mai pubblicata sull''intera opera di Robert Doisneau. Oltre 400 fotografie che abbracciano cinque decenni, dal primo lavoro industriale per la Renault alla celebre fotografia di strada. Edizione trilingue (italiano/inglese/francese). Pubblicata da Contrasto. 400 pagine, 30×34 cm, copertina rigida. Attualmente esaurita.',
 '{"pagine":400,"dimensioni":"30x34 cm","lingue":["it","en","fr"],"editore":"Contrasto","copertina":"rigida"}'),

-- RD-C001
(7,'en','Postcard Set – Paris by Doisneau (10 cards)',
 'A set of 10 glossy postcards reproducing iconic Doisneau photographs.',
 'A set of 10 glossy postcards, each reproducing a different iconic photograph by Robert Doisneau. Each card measures 10.5×14.8 cm (A6). Packaged in a branded envelope. Ideal as a gift or keepsake.',
 '{"quantity":10,"size":"10.5x14.8 cm","finish":"glossy"}'),
(7,'it','Set di Cartoline – Parigi di Doisneau (10 cartoline)',
 'Un set di 10 cartoline patinate che riproducono iconiche fotografie di Doisneau.',
 'Un set di 10 cartoline patinate, ognuna con una diversa fotografia iconica di Robert Doisneau. Ogni cartolina misura 10,5×14,8 cm (A6). Confezionate in una busta brandizzata. Ideale come regalo o ricordo.',
 '{"quantita":10,"dimensione":"10.5x14.8 cm","finitura":"lucida"}'),

-- RD-C002
(8,'en','Postcard – "The Kiss by the Hotel de Ville"',
 'Single glossy postcard of Doisneau''s most celebrated photograph.',
 'A single glossy postcard reproducing Robert Doisneau''s most celebrated photograph, "The Kiss by the Hotel de Ville" (1950). Measures 10.5×14.8 cm (A6). Suitable for sending or display.',
 '{"quantity":1,"size":"10.5x14.8 cm","finish":"glossy"}'),
(8,'it','Cartolina – "Il Bacio all''Hotel de Ville"',
 'Cartolina patinata singola della fotografia più celebre di Doisneau.',
 'Una cartolina patinata singola che riproduce la fotografia più celebre di Robert Doisneau, "Il Bacio all''Hotel de Ville" (1950). Misura 10,5×14,8 cm (A6). Adatta per l''invio o per l''esposizione.',
 '{"quantita":1,"dimensione":"10.5x14.8 cm","finitura":"lucida"}'),

-- RD-C003
(9,'en','Postcard – "The Sidelong Glance"',
 'Single glossy postcard of one of Doisneau''s most beloved street photographs.',
 'A single glossy postcard reproducing Robert Doisneau''s much-loved street photograph "The Sidelong Glance" (1948). Measures 10.5×14.8 cm (A6). Suitable for sending or display.',
 '{"quantity":1,"size":"10.5x14.8 cm","finish":"glossy"}'),
(9,'it','Cartolina – "Lo Sguardo Obliquo"',
 'Cartolina patinata singola di una delle fotografie di strada più amate di Doisneau.',
 'Una cartolina patinata singola che riproduce la famosa fotografia di strada di Robert Doisneau "Lo Sguardo Obliquo" (1948). Misura 10,5×14,8 cm (A6). Adatta per l''invio o per l''esposizione.',
 '{"quantita":1,"dimensione":"10.5x14.8 cm","finitura":"lucida"}'),

-- RD-A001
(10,'en','T-Shirt – "The Kiss" (White)',
 'High-quality organic cotton t-shirt featuring "The Kiss by the Hotel de Ville".',
 'A high-quality t-shirt in 100% organic cotton featuring a print of Robert Doisneau''s iconic photograph "The Kiss by the Hotel de Ville". White. Available sizes: XS–XXL. Machine washable at 30°C.',
 '{"type":"t-shirt","material":"100% organic cotton","sizes":["XS","S","M","L","XL","XXL"],"colour":"white"}'),
(10,'it','T-Shirt – "Il Bacio" (Bianca)',
 'T-shirt in cotone biologico di alta qualità con "Il Bacio all''Hotel de Ville".',
 'Una t-shirt di alta qualità in 100% cotone biologico con stampa della fotografia iconica di Robert Doisneau "Il Bacio all''Hotel de Ville". Colore: bianco. Taglie disponibili: XS–XXL. Lavabile in lavatrice a 30°C.',
 '{"tipo":"t-shirt","materiale":"100% cotone biologico","taglie":["XS","S","M","L","XL","XXL"],"colore":"bianco"}'),

-- RD-A002
(11,'en','Tote Bag – "The Kiss"',
 'Sturdy cotton canvas tote bag featuring "The Kiss by the Hotel de Ville".',
 'A sturdy, spacious tote bag in 100% natural cotton canvas featuring a print of Robert Doisneau''s iconic photograph "The Kiss by the Hotel de Ville". Dimensions: 38×42 cm, handles 70 cm. Machine washable at 30°C.',
 '{"type":"tote bag","material":"100% cotton canvas","dimensions":"38x42 cm","handle_length":"70 cm"}'),
(11,'it','Borsa Tote – "Il Bacio"',
 'Robusta borsa tote in tela di cotone con "Il Bacio all''Hotel de Ville".',
 'Una borsa tote robusta e spaziosa in 100% tela di cotone naturale con stampa della fotografia iconica di Robert Doisneau "Il Bacio all''Hotel de Ville". Dimensioni: 38×42 cm, manici 70 cm. Lavabile in lavatrice a 30°C.',
 '{"tipo":"borsa tote","materiale":"100% tela di cotone","dimensioni":"38x42 cm","lunghezza_manici":"70 cm"}'),

-- RD-X001
(12,'en','Ceramic Mug – "The Kiss"',
 'A 330ml ceramic mug featuring Robert Doisneau''s most iconic photograph.',
 'A high-quality ceramic mug (330ml) featuring a reproduction of Robert Doisneau''s iconic photograph "The Kiss by the Hotel de Ville". Dishwasher safe and microwave safe.',
 '{"type":"mug","material":"ceramic","volume_ml":330,"dishwasher_safe":true,"microwave_safe":true}'),
(12,'it','Tazza in Ceramica – "Il Bacio"',
 'Una tazza in ceramica da 330ml con la fotografia più iconica di Robert Doisneau.',
 'Una tazza in ceramica di alta qualità (330ml) con una riproduzione della fotografia iconica di Robert Doisneau "Il Bacio all''Hotel de Ville". Adatta a lavastoviglie e microonde.',
 '{"tipo":"tazza","materiale":"ceramica","volume_ml":330,"lavastoviglie":true,"microonde":true}'),

-- RD-X002
(13,'en','Magnet Set – Paris by Doisneau (5 magnets)',
 'A set of 5 flexible magnets reproducing iconic Doisneau photographs.',
 'A set of 5 flexible magnets, each reproducing a different iconic photograph by Robert Doisneau. Each magnet measures 5×7 cm. Ideal for decorating refrigerators, whiteboards and other metal surfaces.',
 '{"type":"magnet set","quantity":5,"size_each":"5x7 cm","material":"flexible magnet"}'),
(13,'it','Set di Magneti – Parigi di Doisneau (5 magneti)',
 'Un set di 5 magneti flessibili che riproducono iconiche fotografie di Doisneau.',
 'Un set di 5 magneti flessibili, ognuno con una diversa fotografia iconica di Robert Doisneau. Ogni magnete misura 5×7 cm. Ideali per decorare frigoriferi, lavagne e altre superfici metalliche.',
 '{"tipo":"set magneti","quantita":5,"dimensione_singolo":"5x7 cm","materiale":"magnete flessibile"}'),

-- RD-X003
(14,'en','Softcover Notebook – "The Kiss"',
 'A 192-page lined softcover notebook featuring "The Kiss by the Hotel de Ville" on the cover.',
 'A softcover lined notebook (192 pages, A5 / 14×21 cm) with a reproduction of Robert Doisneau''s iconic photograph "The Kiss by the Hotel de Ville" on the cover. Ideal for notes, sketches and journalling.',
 '{"type":"notebook","pages":192,"dimensions":"14x21 cm","cover":"softcover","ruling":"lined"}'),
(14,'it','Taccuino con Copertina Morbida – "Il Bacio"',
 'Un taccuino a righe da 192 pagine con "Il Bacio all''Hotel de Ville" in copertina.',
 'Un taccuino a righe con copertina morbida (192 pagine, A5 / 14×21 cm) con una riproduzione della fotografia iconica di Robert Doisneau "Il Bacio all''Hotel de Ville" in copertina. Ideale per appunti, schizzi e diari personali.',
 '{"tipo":"taccuino","pagine":192,"dimensioni":"14x21 cm","copertina":"morbida","carta":"a righe"}');


-- ============================================================
-- CART_ITEMS  (riempito con i souvenir scelti per ogni ordine da utenti creati di prima)
-- ============================================================
INSERT INTO cart_items (cart_id, souvenir_id, quantity) VALUES
-- Gabriele: stampa P001 + set cartoline C001
(1,  1, 1),
(1,  7, 1),
-- Navpreet: tazza X001 + t-shirt A001
(2, 12, 1),
(2, 10, 1),
-- Lorenzo: libro B001 + cartolina C002 ×2
(3,  4, 1),
(3,  8, 2),
-- Ruben: stampa grande P003 + borsa A002
(4,  3, 1),
(4, 11, 1);


-- ============================================================
-- ORDERS
-- ============================================================
INSERT INTO orders (user_id, status, total_amount, created_at) VALUES
(1, 'delivered', 39.80, '2026-01-10 14:23:00'),  -- Gabriele: P001 + C001
(2, 'shipped',   37.00, '2026-02-05 10:11:00'),  -- Navpreet: X001 + A001
(3, 'paid',      44.90, '2026-03-01 16:45:00'),  -- Lorenzo:  B001 + C001
(4, 'pending',   74.90, '2026-03-20 09:30:00'),  -- Ruben:    P003 + A002
(1, 'cancelled', 29.90, '2026-02-14 11:00:00');  -- Gabriele: P002 (annullato)
-- order.id: 1=Gabriele delivered | 2=Navpreet shipped | 3=Lorenzo paid
--           4=Ruben pending      | 5=Gabriele cancelled


-- ============================================================
-- ORDER_ITEMS
-- ============================================================
INSERT INTO order_items (order_id, souvenir_id, quantity, unit_price) VALUES
-- Ordine 1 (Gabriele, delivered): P001 29.90 + C001 9.90 = 39.80 
(1,  1, 1, 29.90),
(1,  7, 1,  9.90),
-- Ordine 2 (Navpreet, shipped): X001 12.00 + A001 25.00 = 37.00 
(2, 12, 1, 12.00),
(2, 10, 1, 25.00),
-- Ordine 3 (Lorenzo, paid): B001 35.00 + C001 9.90 = 44.90 
(3,  4, 1, 35.00),
(3,  7, 1,  9.90),
-- Ordine 4 (Ruben, pending): P003 59.90 + A002 15.00 = 74.90 
(4,  3, 1, 59.90),
(4, 11, 1, 15.00),
-- Ordine 5 (Gabriele, cancelled): P002 29.90 
(5,  2, 1, 29.90);