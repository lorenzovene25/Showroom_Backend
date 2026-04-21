-- ============================================================
--  SCHEMA + SEED PostgreSQL — Sito Robert Doisneau
--  Generato automaticamente — adattare gli hash delle password
--  con quelli prodotti dall'applicazione prima del deploy.
-- ============================================================

-- ============================================================
-- 1. CARTS
-- ============================================================
CREATE TABLE carts (
    id         SERIAL PRIMARY KEY,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);


-- ============================================================
-- 2. USERS  																			
-- ============================================================
CREATE TABLE users (
    id            SERIAL PRIMARY KEY,
    first_name    VARCHAR(100) NOT NULL,
    last_name     VARCHAR(100) NOT NULL,
    email         VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    is_admin      BOOLEAN NOT NULL DEFAULT FALSE,
    cart_id       INTEGER UNIQUE REFERENCES carts(id) ON DELETE SET NULL,
    created_at    TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 3. EXHIBITIONS
-- ============================================================
CREATE TABLE exhibitions (
    id         SERIAL        PRIMARY KEY,
    name       VARCHAR(100)  NOT NULL,
    location   VARCHAR(255)  NOT NULL,
    maps_url   VARCHAR(500),
    status     VARCHAR(20)   NOT NULL CHECK (status IN ('ongoing','past','upcoming')),
    start_date DATE          NOT NULL,
    end_date   DATE          NOT NULL,
    image_url  VARCHAR(255)
);

-- ============================================================
-- 4. EXHIBITION_TRANSLATIONS
-- ============================================================
CREATE TABLE exhibition_translations (
    id             SERIAL        PRIMARY KEY,
    exhibition_id  INTEGER       NOT NULL REFERENCES exhibitions(id) ON DELETE CASCADE,
    language_code  VARCHAR(2)    NOT NULL,
    title          VARCHAR(255)  NOT NULL,
    description    TEXT,
    UNIQUE (exhibition_id, language_code)
);

-- ============================================================
-- 5. EXHIBITION_TIME_SLOTS
--    days_of_week usa: 1=Lun 2=Mar 3=Mer 4=Gio 5=Ven 6=Sab 7=Dom
--    Orari galleria:
--      Mer–Ven  -> 15:00–19:00
--      Sab–Dom  -> 10:00–13:00 e 15:00–19:00
-- ============================================================
CREATE TABLE exhibition_time_slots (
    id             SERIAL    PRIMARY KEY,
    exhibition_id  INTEGER   NOT NULL REFERENCES exhibitions(id) ON DELETE CASCADE,
    days_of_week   INTEGER[] NOT NULL,                                                  -- es. ARRAY[3,4,5] = Mer,Gio,Ven
    start_time     TIME      NOT NULL,
    end_time       TIME      NOT NULL,
    max_capacity   INTEGER   NOT NULL DEFAULT 50
);

-- ============================================================
-- 6. ARTWORKS
-- ============================================================
CREATE TABLE artworks (
    id          SERIAL        PRIMARY KEY,
    archive_id  VARCHAR(50)   UNIQUE,
    name        VARCHAR(255)  NOT NULL,
    year        INTEGER,
    dimensions  VARCHAR(100),
    image_url   VARCHAR(255)
);

-- ============================================================
-- 7. ARTWORK_TRANSLATIONS
-- ============================================================
CREATE TABLE artwork_translations (
    id                SERIAL        PRIMARY KEY,
    artwork_id        INTEGER       NOT NULL REFERENCES artworks(id) ON DELETE CASCADE,
    language_code     VARCHAR(2)    NOT NULL,
    title             VARCHAR(255)  NOT NULL,
    description       TEXT,
    historical_period VARCHAR(100),
    support           VARCHAR(100),
    camera            VARCHAR(100),
    UNIQUE (artwork_id, language_code)
);

-- ============================================================
-- 8. ARTWORK_EXHIBITIONS  (tabella di giunzione)
-- ============================================================
CREATE TABLE artwork_exhibitions (
    artwork_id     INTEGER NOT NULL REFERENCES artworks(id)    ON DELETE CASCADE,
    exhibition_id  INTEGER NOT NULL REFERENCES exhibitions(id) ON DELETE CASCADE,
    PRIMARY KEY (artwork_id, exhibition_id)
);

-- ============================================================
-- 9. TICKET_TIERS
-- ============================================================
CREATE TABLE ticket_tiers (
    id          SERIAL          PRIMARY KEY,
    type        VARCHAR(50)     NOT NULL,
    description TEXT,
    price       DECIMAL(10,2)   NOT NULL
);

-- ============================================================
-- 10. TICKET_TIER_TRANSLATIONS
-- ============================================================
CREATE TABLE ticket_tier_translations (
    id              SERIAL        PRIMARY KEY,
    ticket_tier_id  INTEGER       NOT NULL REFERENCES ticket_tiers(id) ON DELETE CASCADE,
    language_code   VARCHAR(2)    NOT NULL,
    name            VARCHAR(100)  NOT NULL,
    description     TEXT,
    UNIQUE (ticket_tier_id, language_code)
);

-- ============================================================
-- 11. TICKETS
-- ============================================================
CREATE TABLE tickets (
    id             SERIAL    PRIMARY KEY,
    exhibition_id  INTEGER   NOT NULL REFERENCES exhibitions(id)            ON DELETE RESTRICT,
    tier_id        INTEGER   NOT NULL REFERENCES ticket_tiers(id)           ON DELETE RESTRICT,
    user_id        INTEGER   NOT NULL REFERENCES users(id)                  ON DELETE RESTRICT,
    visit_date     DATE      NOT NULL,
    time_slot_id   INTEGER   NOT NULL REFERENCES exhibition_time_slots(id)  ON DELETE RESTRICT,
    purchased_at   TIMESTAMP NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 12. CATEGORIES  (souvenir)
-- ============================================================
CREATE TABLE categories (
    id          SERIAL        PRIMARY KEY,
    slug        VARCHAR(100)  NOT NULL UNIQUE,
    name        VARCHAR(255)  NOT NULL,
    description TEXT
);

-- ============================================================
-- 13. SOUVENIRS
-- ============================================================
CREATE TABLE souvenirs (
    id                 SERIAL          PRIMARY KEY,
    archive_id         VARCHAR(50)     UNIQUE,
    category_id        INTEGER         NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    price              DECIMAL(10,2)   NOT NULL,
    in_stock           BOOLEAN         NOT NULL DEFAULT TRUE,
    quantity_available INTEGER         NOT NULL DEFAULT 0,
    image_url          VARCHAR(500),
    specifications     JSONB
);

-- ============================================================
-- 14. SOUVENIRS_TRANSLATIONS
-- ============================================================
CREATE TABLE souvenirs_translations (
    id                SERIAL        PRIMARY KEY,
    souvenir_id       INTEGER       NOT NULL REFERENCES souvenirs(id) ON DELETE CASCADE,
    language_code     VARCHAR(2)    NOT NULL,
    name              VARCHAR(255)  NOT NULL,
    short_description VARCHAR(500),
    full_description  TEXT,
    specifications    JSONB,
    UNIQUE (souvenir_id, language_code)
);

-- ============================================================
-- 15. CART_ITEMS
-- ============================================================
CREATE TABLE cart_items (
    id          SERIAL  PRIMARY KEY,
    cart_id     INTEGER NOT NULL REFERENCES carts(id)     ON DELETE CASCADE,
    souvenir_id INTEGER NOT NULL REFERENCES souvenirs(id) ON DELETE CASCADE,
    quantity    INTEGER NOT NULL DEFAULT 1 CHECK (quantity > 0),
    UNIQUE (cart_id, souvenir_id)
);

-- ============================================================
-- 16. ORDERS
-- ============================================================
CREATE TABLE orders (
    id            SERIAL          PRIMARY KEY,
    user_id       INTEGER         NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    status        VARCHAR(30)     NOT NULL DEFAULT 'pending'
                  CHECK (status IN ('pending','paid','shipped','delivered','cancelled')),
    total_amount  DECIMAL(10,2)   NOT NULL,
    created_at    TIMESTAMP       NOT NULL DEFAULT NOW()
);

-- ============================================================
-- 17. ORDER_ITEMS
-- ============================================================
CREATE TABLE order_items (
    id          SERIAL          PRIMARY KEY,
    order_id    INTEGER         NOT NULL REFERENCES orders(id)    ON DELETE CASCADE,
    souvenir_id INTEGER         NOT NULL REFERENCES souvenirs(id) ON DELETE RESTRICT,
    quantity    INTEGER         NOT NULL,
    unit_price  DECIMAL(10,2)   NOT NULL
);