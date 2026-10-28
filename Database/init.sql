-- Увімкнення підтримки зовнішніх ключів у SQLite
PRAGMA foreign_keys = ON;

-- 1. Таблиця Users (Користувачі / Аналітики)
CREATE TABLE IF NOT EXISTS Users (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);

-- 2. Таблиця Items (Тактичні конфігурації чемпіонів)
CREATE TABLE IF NOT EXISTS Items (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    ChampionName TEXT NOT NULL,
    Role TEXT NOT NULL,
    CoreItemBuild TEXT NOT NULL,
    RuneSetup TEXT NOT NULL,
    UserID INTEGER NOT NULL,
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE
);

-- 3. Таблиця Actions (Гайди дій під конкретний матчап)
CREATE TABLE IF NOT EXISTS Actions (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    UserID INTEGER NOT NULL,
    ItemID INTEGER NOT NULL,
    OpponentChampion TEXT NOT NULL,
    ActionDetails TEXT NOT NULL,
    Status TEXT NOT NULL CHECK(Status IN ('Draft', 'Approved', 'Archived')),
    FOREIGN KEY (UserID) REFERENCES Users(ID) ON DELETE CASCADE,
    FOREIGN KEY (ItemID) REFERENCES Items(ID) ON DELETE CASCADE
);

-- =======================================================
-- Тестові дані (Seed Data)
-- =======================================================

INSERT INTO Users (Name, Email, PasswordHash) VALUES
('Faker', 'faker@t1.gg', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy'),
('Caps', 'caps@g2.es', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy'),
('Shady', 'shady@t1a.gg', '$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy');

INSERT INTO Items (ChampionName, Role, CoreItemBuild, RuneSetup, UserID) VALUES
('Yasuo', 'Mid', 'Storm Razor, Infinity Edge, Death"'"s Dance', 'Lethal Tempo, Triumph, Legend: Alacrity, Last Stand', 1),
('Ahri', 'Mid', 'Malignance, Shadowflame, Rabadon Deathcap', 'Electrocute, Taste of Blood, Eyeball Collection, Ultimate Hunter', 2),
('Yone', 'Top', 'Immortal Shieldbow, Infinity Edge, Death"'"s Dance', 'Lethal Tempo, Triumph, Legend: Alacrity, Cut Down', 3);

INSERT INTO Actions (UserID, ItemID, OpponentChampion, ActionDetails, Status) VALUES
(1, 1, 'Zed', 'Rush Plated Steelcaps early. Save Windwall exclusively for Zed triple Q combo after Death Mark.', 'Approved'),
(2, 2, 'Syndra', 'Bait Scatter the Weak before Charm engage. Roam to side lanes with Spirit Rush.', 'Approved'),
(3, 3, 'Renekton', 'Concede early prio. Freeze near tower and trade only when Fury bar is empty.', 'Draft');

-- ==============================================================================
-- Оптимізація: Індекси для прискорення вибірок та JOIN операцій (Завдання 11)
-- ==============================================================================
CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_items_userid ON Items(UserID);
CREATE INDEX IF NOT EXISTS idx_actions_itemid ON Actions(ItemID);
CREATE INDEX IF NOT EXISTS idx_actions_userid ON Actions(UserID);