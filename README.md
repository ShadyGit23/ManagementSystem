# ⚔️ League of Legends Matchup & Armory System

[![.NET Version](https://img.shields.io/badge/.NET-10.0.401-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC_%26_Web_API-2C8EBB?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-Cross--Platform-512BD4?logo=dotnet)](https://dotnet.microsoft.com/apps/maui)
[![Database](https://img.shields.io/badge/SQLite-Entity_Framework_Core-003B57?logo=sqlite)](https://www.sqlite.org/)

**League of Legends Matchup & Armory System** — це дворівнева аналітична та конфігураційна платформа, призначена для підготовки кіберспортивних команд і гравців до матчів. 

Система реалізує концепцію динамічної адаптації під ворожого опонента:

$$
\text{Strategy} = f(\text{Champion}, \text{Opponent}, \text{Runes}, \text{Items})
$$

Комплекс складається з веб-панелі керування для головного тренера та мобільного кросплатформеного застосунку для гравців/аналітиків.

---

## 🏛️ 1. Архітектурне проєктування (Клієнт–Сервер)

Проєкт розділено на два автономні компоненти відповідно до вимог курсу:

* **`Server/ServerApp` (ASP.NET Core 10):**
  * Централізоване ядро системи.
  * Реалізує бізнес-правила, міграції бази даних через ORM Entity Framework Core, MVC-панель адміністратора та REST API з JWT-аутентифікацією.
* **`Client/ClientApp` (.NET MAUI 10):**
  * Кросплатформений клієнтський додаток (Windows Desktop / Android).
  * Побудований за шаблоном проєктування **MVVM** (Model-View-ViewModel). Забезпечує перегляд каталогу доступних чемпіонів та надсилання адаптивних тактичних гайдів на сервер через REST API.

---

## 🗄️ 2. Реляційна модель бази даних (Архітектура 3 таблиць)

Схема бази даних суворо відповідає навчальному патерну `[Users, Items, Actions]` і реалізована за допомогою **SQLite**:

```
 ┌────────────────────────┐
 │      Users (1)         │  Strategists (Автори / Гравці)
 └───────────┬────────────┘
             │ 1
             │
             │ N
 ┌───────────▼────────────┐
 │      Items (2)         │  ChampionLoadouts (Тактичні збірки чемпіонів)
 └───────────┬────────────┘
             │ 1
             │
             │ N
 ┌───────────▼────────────┐
 │     Actions (3)        │  MatchupGuides (Гайди під конкретних опонентів)
 └────────────────────────┘
```

### Специфікація таблиць

1. **`Users` (Strategists):**
   * `ID` (INTEGER, Primary Key, AutoIncrement) — унікальний ідентифікатор користувача.
   * `Name` (TEXT) — нікнейм або ім'я аналітика/гравця (наприклад, *Faker*, *Caps*).
   * `Email` (TEXT, Unique) — обліковий запис для авторизації.
   * `PasswordHash` (TEXT) — безпечний хеш пароля для доступу до системи.

2. **`Items` (ChampionLoadouts):**
   * `ID` (INTEGER, Primary Key, AutoIncrement) — унікальний ідентифікатор збірки.
   * `ChampionName` (TEXT) — ім'я чемпіона (наприклад, *Yasuo*, *Ahri*).
   * `Role` (TEXT) — ігрова позиція (*Top, Jungle, Mid, ADC, Support*).
   * `CoreItemBuild` (TEXT) — ключова лінійка артефактів.
   * `RuneSetup` (TEXT) — дерево рун та ключовий камінь (Keystone).
   * `UserID` (INTEGER, Foreign Key $\rightarrow$ `Users.ID`, `ON DELETE CASCADE`) — закріплений автор.

3. **`Actions` (MatchupGuides):**
   * `ID` (INTEGER, Primary Key, AutoIncrement) — унікальний ідентифікатор гайду.
   * `UserID` (INTEGER, Foreign Key $\rightarrow$ `Users.ID`, `ON DELETE CASCADE`) — гравець, що сформував гайд.
   * `ItemID` (INTEGER, Foreign Key $\rightarrow$ `Items.ID`, `ON DELETE CASCADE`) — зв'язок із конкретним сетапом чемпіона.
   * `OpponentChampion` (TEXT) — ворожий чемпіон на лінії (наприклад, *Zed*, *Syndra*).
   * `ActionDetails` (TEXT) — покрокові тактичні настанови щодо контролю лінії та розіграшу ультимейту.
   * `Status` (TEXT, CHECK: `'Draft'`, `'Approved'`, `'Archived'`) — стан валідації гайду тренером.

---

## 📂 3. Структура репозиторію

```text
ManagementSystem/
├── .gitignore                      # Виключення збірок bin/obj, *.user, appsettings.json
├── README.md                       # Документація проєкту
├── Database/
│   ├── init.sql                    # Первинний DDL-скрипт зі схемою та тестовими даними
│   └── app.db                      # Фізична локальна база даних SQLite (генерується EF Core)
├── Server/
│   ├── ServerApp.slnx              # Файл рішення сервера
│   └── ServerApp/
│       ├── Controllers/            # Контролери MVC та REST API
│       ├── Data/                   # Контекст бази даних (AppDbContext)
│       ├── Migrations/             # Міграції Entity Framework Core
│       ├── Models/                 # C#-моделі: User, Item, ActionRecord
│       ├── Program.cs              # Точка входу веб-сервера та реєстрація сервісів
│       └── ServerApp.csproj
└── Client/
    ├── ClientApp.slnx              # Файл рішення клієнта
    └── ClientApp/
        ├── ViewModels/             # Компоненти патерну MVVM
        ├── App.xaml
        ├── MauiProgram.cs          # Точка входу мобільного додатка
        └── ClientApp.csproj
```

---

## 🛠️ 4. Технологічний стек

* **Базова платформа:** .NET 10 (SDK 10.0.401)
* **Серверна частина:** ASP.NET Core 10 (MVC + Web API)
* **ORM та збереження даних:** Entity Framework Core 10, SQLite
* **Клієнтська частина:** .NET MAUI 10 (кросплатформений інтерфейс XAML/C#)
* **Архітектурні патерни:** MVC (сервер), MVVM (клієнт), Repository / Unit of Work

---

## 🚀 5. Інструкція для локального розгортання та запуску

### Попередні вимоги
* Встановлений [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* Встановлене робоче навантаження MAUI (`dotnet workload install maui`)

### Кроки запуску

1. **Клонувати репозиторій:**
   ```bash
   git clone https://github.com/<ВАШ_ЮЗЕРНЕЙМ>/ManagementSystem.git
   cd ManagementSystem
   ```

2. **Застосування міграцій до бази даних:**
   ```bash
   cd Server/ServerApp
   dotnet ef database update
   cd ../..
   ```

3. **Запуск веб-сервера (ASP.NET Core):**
   ```bash
   dotnet run --project Server/ServerApp/ServerApp.csproj
   ```
   *Веб-сервер буде доступний за адресою: `http://localhost:5xxx`.*

4. **Компіляція та запуск клієнта (.NET MAUI під Windows):**
   ```bash
   dotnet run --project Client/ClientApp/ClientApp.csproj -f net10.0-windows10.0.19041.0
   ```