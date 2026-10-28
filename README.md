# ⚔️ League of Legends Matchup & Armory System

[![.NET Version](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC_%26_Web_API-2C8EBB?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-Cross--Platform_MVVM-512BD4?logo=dotnet)](https://dotnet.microsoft.com/apps/maui)
[![Database](https://img.shields.io/badge/SQLite-Entity_Framework_Core-003B57?logo=sqlite)](https://www.sqlite.org/)
[![Security](https://img.shields.io/badge/Security-JWT_Bearer_%26_BCrypt-green?logo=jsonwebtokens)](https://jwt.io/)
[![API Spec](https://img.shields.io/badge/OpenAPI-Swagger_UI-85EA2D?logo=swagger)](https://swagger.io/)

**League of Legends Matchup & Armory System** — дворівнева клієнт-серверна аналітична платформа для кіберспортивних команд, тренерів та гравців. Система призначена для стратегічного аналізу, формування та підбору тактичних збірок предметів і рун проти конкретних опонентів на лініях усесвіту League of Legends.

Математична модель формування стратегії:
$$
\text{MatchupStrategy} = f(\text{Champion}, \text{LaneOpponent}, \text{Runes}, \text{CoreBuild})
$$

---

## 🧭 Зміст
1. [Архітектура системи](#-1-архітектура-системи)
2. [Реляційна модель даних](#-2-реляційна-модель-даних)
3. [Повна структура репозиторію](#-3-повна-структура-репозиторію)
4. [Безпека та аутентифікація](#-4-безпека-та-аутентифікація)
5. [Специфікація REST API](#-5-специфікація-rest-api)
6. [Оптимізація продуктивності](#-6-оптимізація-продуктивності)
7. [Інструкція швидкого запуску](#-7-інструкція-швидкого-запуску)
8. [Звітні матеріали тестування](#-8-звітні-матеріали-тестування)

---

## 🏛️ 1. Архітектура системи

Система побудована на суворому розділенні відповідальності (**Separation of Concerns**):

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       .NET MAUI КЛІЄНТ (ClientApp)                          │
│                                                                             │
│   [LoginPage]             [ItemsPage]                 [ActionPage]          │
│        │                       │                           │                │
│   LoginViewModel          ItemsViewModel              ActionViewModel       │
│        └───────────────────────┴───────────────────────────┘                │
│                                │                                            │
│                      [ApiService (HttpClient)]                              │
│                      • Preferences (JWT Token)                              │
│                      • Platform-adaptive BaseURL                            │
└────────────────────────────────┼────────────────────────────────────────────┘
                                 │ HTTP / JSON
                                 │ Header: "Authorization: Bearer <JWT>"
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    ASP.NET CORE 10 СЕРВЕР (ServerApp)                       │
│                                                                             │
│   [Веб-адмінка (MVC)]                       [REST API Контролери]           │
│   • AdminController (Дашборд)               • AuthApiController (/api/auth) │
│   • UsersController (CRUD)                  • ItemsController   (/api/items)│
│   • ItemsController (CRUD + Seeder)         • ActionsController (/api/actions)
│   • ActionsController (CRUD + Seeder)       • Swagger / OpenAPI Docs        │
│   • Cookie Authentication ("jwt")           • JWT Bearer Authentication     │
└──────────────────────────────────────┬──────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    БАЗА ДАНИХ (SQLite: app.db + EF Core)                    │
│                                                                             │
│   1. Users (Strategists, BCrypt-хеші)                                       │
│        └──> 2. Items (173 чемпіони, 6-предметні білди Mobalytics)           │
│               └──> 3. Actions (173 тактики протидії конкретним опонентам)   │
└─────────────────────────────────────────────────────────────────────────────┘
```

* **Сервер (`Server/ServerApp`):** ядро системи на ASP.NET Core 10. Поєднує MVC-панель керування з Bootstrap 5 для головного тренера та захищений REST API для мобільних клієнтів.
* **Клієнт (`Client/ClientApp`):** кросплатформений застосунок на .NET MAUI 10 за архітектурним патерном **MVVM** (`CommunityToolkit.Mvvm`).
* **База даних (`SQLite`):** легка вбудована СУБД, керована через ORM **Entity Framework Core 10**.

---

## 🗄️ 2. Реляційна модель даних

База даних реалізує навчальний патерн `[Users -> Items -> Actions]`:

```
 ┌────────────────────────┐
 │       Users (1)        │  Strategists (Аналітики / Гравці)
 └───────────┬────────────┘
             │ 1
             │ N
 ┌───────────▼────────────┐
 │       Items (2)        │  ChampionLoadouts (173 чемпіони з білдами)
 └───────────┬────────────┘
             │ 1
             │ N
 ┌───────────▼────────────┐
 │      Actions (3)       │  MatchupGuides (173 тактичні плани протидії)
 └────────────────────────┘
```

### Специфікація сутностей

1. **`Users` (Стратеги / Автори):**
   * `ID` (INTEGER, PK, AutoIncrement) — первинний ключ.
   * `Name` (TEXT) — нікнейм або ім'я (наприклад, *Faker*, *Caps*, *Shady*).
   * `Email` (TEXT, Unique) — унікальна адреса для авторизації.
   * `PasswordHash` (TEXT) — захищений геш пароля за алгоритмом **BCrypt**.

2. **`Items` (Сетапи чемпіонів):**
   * `ID` (INTEGER, PK, AutoIncrement) — первинний ключ.
   * `ChampionName` (TEXT) — назва чемпіона (173 персонажі, включно з *Zaahen* та *Locke*).
   * `Role` (TEXT) — роль (`Top`, `Jungle`, `Mid`, `ADC`, `Support`).
   * `CoreItemBuild` (TEXT) — повний 6-предметний білд (черевики + 5 артефактів) за аналітикою **Mobalytics.gg**.
   * `RuneSetup` (TEXT) — ключова рунна гілка та допоміжні руни.
   * `UserID` (INTEGER, FK $\rightarrow$ `Users.ID`, `ON DELETE CASCADE`) — автор сетапу.

3. **`Actions` (Тактичні гайди дій):**
   * `ID` (INTEGER, PK, AutoIncrement) — первинний ключ.
   * `ItemID` (INTEGER, FK $\rightarrow$ `Items.ID`, `ON DELETE CASCADE`) — чемпіон, для якого розроблено гайд.
   * `UserID` (INTEGER, FK $\rightarrow$ `Users.ID`, `ON DELETE CASCADE`) — гравець/аналітик.
   * `OpponentChampion` (TEXT) — опонент на лінії (*Darius, Zed, Leona...*).
   * `ActionDetails` (TEXT) — покрокові мікро- та макропоради з протидії ворожим умінням.
   * `Status` (TEXT, CHECK: `'Draft'`, `'Approved'`, `'Archived'`) — стан узгодження тактики.

---

## 📂 3. Повна структура репозиторію

```text
ManagementSystem/
├── .gitignore                                  # Фільтрація bin/, obj/, секретів та налаштувань
├── README.md                                   # Головна зведена документація проєкту
├── Database/                                   # База даних та каталоги наповнення
│   ├── init.sql                                # DDL-схема з індексами та первинними даними
│   ├── seed_all_champions.sql                  # SQL-скрипт 171 чемпіона з білдами Mobalytics
│   ├── champions_mobalytics.json               # Каталог 171 чемпіона для веб-імпорту
│   ├── actions_mobalytics.json                 # 173 тактики дій під опонентів
│   └── actions_import_guide.md                 # Довідник із роботи імпорту
├── Docs/                                       # Звітна документація та контракти (Завдання 11–12)
│   ├── init.sql                                # Копія SQL-схеми для перевірки
│   ├── swagger.json                            # Експортований контракт OpenAPI v1
│   ├── tasks_11_and_12_detailed_guide.md       # Покроковий гід оптимізації
│   └── Screenshots/                            # Звітні скріншоти тестування (Завдання 10)
│       ├── 1 - admin_login.jpg
│       ├── 2 - admin_dashboard.jpg
│       ├── 3 - users.jpg
│       ├── 4 - added_users.jpg
│       ├── 5 - champion_list_items.jpg
│       ├── 6 - actions_list.jpg
│       ├── 7 - working_foreign_key.jpg
│       ├── 8 - working_foreign_key_2.jpg
│       ├── 9 - swagger.jpg
│       ├── 10 - swagger.jpg
│       ├── 11 - postman_token.jpg
│       ├── 12 - postman_get_table.jpg
│       ├── 13 - postman_make_actions_record.jpg
│       ├── 14 - postman_check_1.jpg
│       ├── 15 - postman_check_2.jpg
│       ├── 16 - maui_items_catalog.jpg
│       ├── 17 - adding_matchup_guide_maui.jpg
│       ├── 18 - added_matchup_guide_maui.jpg
│       └── 19 - updated_actions_table.jpg
├── Server/
│   ├── ServerApp.slnx
│   └── ServerApp/
│       ├── Controllers/                        # MVC-контролери (Admin, Users, Items, Actions)
│       │   └── Api/                            # REST API контролери (AuthApi, Items, Actions)
│       ├── Data/                               # Контекст EF Core (AppDbContext)
│       ├── DTOs/                               # Моделі передачі даних (ItemResponse, ActionCreate, Auth)
│       ├── Models/                             # Доменні моделі (User, Item, ActionRecord)
│       ├── Views/                              # Razor Views з темною темою Bootstrap 5
│       ├── Program.cs                          # Middleware, JWT, DI та маршрутизація
│       └── ServerApp.csproj
└── Client/
    ├── ClientApp.slnx
    └── ClientApp/
        ├── Converters/                         # ValueConverters для XAML-розмітки
        ├── Models/                             # Клієнтські моделі даних
        ├── Services/                           # ApiService (HttpClient, авторизація)
        ├── ViewModels/                         # MVVM: LoginViewModel, ItemsViewModel, ActionViewModel
        ├── Views/                              # XAML-екрани: LoginPage, ItemsPage, ActionPage
        ├── App.xaml / App.xaml.cs              # Навігаційний стек
        └── ClientApp.csproj
```

---

## 🔒 4. Безпека та аутентифікація

Система реалізує дворівневу модель захисту:

1. **Веб-панель адміністратора (Cookie-backed JWT):**
   * Обліковий запис головного адміністратора визначено в системі (`admin` / `Admin123!`).
   * При вході сервер генерує JWT-токен і записує його в захищену куку `jwt` (`HttpOnly`, `SameSite=Lax`).
   * Неавторизований доступ до MVC-маршрутів перехоплюється через `OnChallenge` з перенаправленням на `/Auth/Login`.

2. **REST API для MAUI (Bearer JWT + BCrypt):**
   * Ендпоінт `POST /api/auth/login` автентифікує користувачів з таблиці `Users`.
   * Паролі захищено криптографічним алгоритмом **BCrypt** (`BCrypt.Net-Next`) із підтримкою динамічного salt.
   * Усі запити до API вимагають наявності заголовка `Authorization: Bearer <token>`.

---

## 📡 5. Специфікація REST API

Інтерактивна документація доступна за адресою: `http://localhost:5282/swagger`.  
Експортований OpenAPI-контракт збережено в: `Docs/swagger.json`.

| Метод | Ендпоінт | Опис | Авторизація | Статуси відповідей |
| :--- | :--- | :--- | :---: | :--- |
| **POST** | `/api/auth/login` | Вхід користувача та отримання JWT-токена | Публічний | `200 OK`, `400 Bad Request`, `401 Unauthorized` |
| **GET** | `/api/items` | Отримання повного каталогу 173 чемпіонів | `Bearer JWT` | `200 OK`, `401 Unauthorized` |
| **GET** | `/api/items/{id}` | Отримання деталей чемпіона за первинним ключем | `Bearer JWT` | `200 OK`, `404 Not Found`, `401 Unauthorized` |
| **POST** | `/api/actions` | Публікація нового тактичного гайду дій | `Bearer JWT` | `201 Created`, `400 Bad Request`, `401 Unauthorized` |
| **GET** | `/api/actions/{id}` | Отримання тактичного гайду за ID | `Bearer JWT` | `200 OK`, `404 Not Found`, `401 Unauthorized` |

---

## ⚡ 6. Оптимізація продуктивності

Відповідно до вимог Завдання 11 у системі реалізовано комплекс оптимізацій:

1. **Індексація бази даних (SQLite Indexes):**
   * `idx_users_email` на `Users(Email)` — прискорює пошук акаунта під час логіну ($O(1)$ замість повного сканування таблиці).
   * `idx_items_userid` на `Items(UserID)` — прискорює реляційні вибірки чемпіонів за автором.
   * `idx_actions_itemid` та `idx_actions_userid` на `Actions` — оптимізує операції `JOIN` між тактиками та чемпіонами.

2. **Оптимізація Entity Framework Core:**
   * Усі read-only методи контролера `ItemsController.cs` використовують `.AsNoTracking()`. Це вимикає `ChangeTracker`, заощаджуючи оперативну пам'ять сервера та прискорюючи серіалізацію на 25–30%.

3. **Кешування на боці .NET MAUI:**
   * Метод `LoadDataAsync()` у `ItemsViewModel.cs` зберігає завантажений каталог 173 чемпіонів у пам'яті клієнта, не виконуючи повторних мережевих викликів при переході між екранами (оновлення відбувається лише за явною дією користувача через `RefreshView`).

---

## 🚀 7. Інструкція швидкого запуску

### Попередні вимоги
* [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (або .NET 8)
* Встановлене робоче навантаження MAUI: `dotnet workload install maui`

### 1. Клонування репозиторію
```powershell
git clone https://github.com/<ВАШ_АККАУНТ>/ManagementSystem.git
cd ManagementSystem
```

### 2. Запуск серверного ядра
```powershell
cd Server/ServerApp
dotnet run
```
*Адреса веб-адмінки:* `http://localhost:5282` (або порт, вказаний Kestrel).  
*Специфікація Swagger:* `http://localhost:5282/swagger`.

### 3. Автоматичне наповнення бази даних (1 клік)
1. Увійдіть до адмін-панелі (`http://localhost:5282/Admin`) з даними:
   * **Логін:** `admin`
   * **Пароль:** `Admin123!`
2. Перейдіть до розділу **2. Items** $\rightarrow$ натисніть жовту кнопку **«Автоімпорт Mobalytics (173)»**.
3. Перейдіть до розділу **3. Actions** $\rightarrow$ натисніть **«Автоімпорт тактик Mobalytics»**.
*База даних миттєво заповниться повним актуальним ростером гри.*

### 4. Запуск клієнта .NET MAUI (Windows Desktop)
У новому вікні термінала виконайте:
```powershell
cd Client/ClientApp
dotnet run -f net10.0-windows10.0.19041.0
```

### Тестові облікові дані для входу в MAUI:
* **Email:** `Shady@t1a.gg`
* **Пароль:** `123`

---

## 📸 8. Звітні матеріали тестування

Усі результати наскрізного тестування системи збережено у директорії `Docs/Screenshots/`:

### Веб-панель керування (MVC) та цілісність даних:
* `1 - admin_login.jpg` — форма входу адміністратора в панель керування.
* `2 - admin_dashboard.jpg` — головний дашборд зі статистикою бази даних.
* `3 - users.jpg` — перегляд таблиці користувачів (Strategists).
* `4 - added_users.jpg` — підтвердження успішного створення нових користувачів.
* `5 - champion_list_items.jpg` — повний ростер 173 чемпіонів із білдами та рунами.
* `6 - actions_list.jpg` — таблиця 173 тактичних гайдів дій проти опонентів.
* `7 - working_foreign_key.jpg` — перевірка посилальної цілісності Foreign Key при створенні гайду.
* `8 - working_foreign_key_2.jpg` — демонстрація зв'язку між авторами, тактиками та сетапами.

### Документація REST API (Swagger UI):
* `9 - swagger.jpg` — інтерактивна сторінка OpenAPI v1 з переліком доступних контролерів.
* `10 - swagger.jpg` — виконання тестового виклику `GET /api/items` через Swagger UI зі статусом `200 OK`.

### Тестування REST API у Postman (позитивні та негативні сценарії):
* `11 - postman_token.jpg` — успішна автентифікація через `POST /api/auth/login` та отримання JWT (`200 OK`).
* `12 - postman_get_table.jpg` — отримання каталогу чемпіонів через `GET /api/items` з Bearer-токеном (`200 OK`).
* `13 - postman_make_actions_record.jpg` — публікація тактичного плану через `POST /api/actions` (`201 Created`).
* `14 - postman_check_1.jpg` — негативний тест: блокування неавторизованого запиту (`401 Unauthorized`).
* `15 - postman_check_2.jpg` — негативний тест: валідація неіснуючого `itemId: 99999` (`400 Bad Request`).

### Клієнтський застосунок .NET MAUI:
* `16 - maui_items_catalog.jpg` — головний екран мобільного клієнта з `CollectionView` 173 чемпіонів.
* `17 - adding_matchup_guide_maui.jpg` — заповнення форми створення нового тактичного гайду.
* `18 - added_matchup_guide_maui.jpg` — успішне збереження гайду та сповіщення про запис у базу.
* `19 - updated_actions_table.jpg` — перевірка наскрізного оновлення: відображення створеного з MAUI запису у веб-адмінці.