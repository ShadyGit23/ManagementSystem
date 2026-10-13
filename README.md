<div align="center">

# ⚔️ League of Legends Matchup & Armory System

**Система для управління [Strategists, Champions, MatchupLoadouts] з MVC-адмін-панеллю та MAUI-застосунком**

[![Platform](https://img.shields.io/badge/.NET-10.0.401-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC_%26_WebAPI-2E7D32?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-Cross--Platform-107C41?logo=xamarin)](https://dotnet.microsoft.com/apps/maui)
[![Architecture](https://img.shields.io/badge/Architecture-MVVM_%2B_Repository-0078D4)]()
[![Status](https://img.shields.io/badge/Status-In_Development-F57C00)]()

</div>

---

## 📖 1. Опис предметної області

**League of Legends Matchup & Armory System** — це спеціалізована аналітична платформа для кіберспортивних команд і гравців. 

У сучасній League of Legends успіх визначається не ізольованим вибором предметів, а комплексною адаптацією стратегії:

$$\text{Strategy} = f(\text{Champion}, \text{Opponent}, \text{Runes}, \text{Items})$$

Система дозволяє аналітикам вести облік пулу чемпіонів команди, а гравцям — швидко конфігурувати й узгоджувати тактичні білди (руни, ключові артефакти, рівень складності) під конкретного ворожого суперника на лінії.

---

## 🏛️ 2. Реляційна структура БД (Архітектура 3 таблиць)

Проєкт суворо дотримується вимог курсу щодо розподілу на 3 реляційні сутності зі зв'язками `Foreign Key`:

```text
[Strategists] (Users)
     │
     ├── 1:N ──> [Champions] (Items)
     │                │
     └── 1:N ─────────┴── 1:N ──> [MatchupLoadouts] (Actions)
```

| Таблиця (Курсова роль) | Опис та призначення | Ключові атрибути |
| :--- | :--- | :--- |
| **`Strategists`** <br>*(Users)* | Облікові записи аналітиків, тренерів та гравців. | `ID`, `SummonerName`, `Email`, `PasswordHash`, `RankTier` |
| **`Champions`** <br>*(Items)* | Каталог персонажів у тренувальному пулі команди, закріплених за аналітиком. | `ID`, `Name`, `Role` *(Top, Mid, etc.)*, `DamageType`, `StrategistID` *(FK)* |
| **`MatchupLoadouts`** <br>*(Actions)* | Готові тактичні сетапи (руни + предмети + оцінка складності) проти конкретного ворога. | `ID`, `StrategistID` *(FK)*, `ChampionID` *(FK)*, `OpponentChampion`, `RuneSetup`, `CoreItemBuild`, `MatchupDifficulty`, `Status` |

---

## 📁 3. Структура проєкту

Репозиторій організований за модульною схемою відповідно до вимог методичних вказівок:

```text
ManagementSystem/
├── .gitignore                     # Правила ігнорування бінарних файлів та конфігів
├── README.md                      # Документація проєкту
├── Server/                        # Серверна частина (.NET 10)
│   ├── ServerApp.slnx             # Файл рішення бекенду
│   └── ServerApp/
│       ├── Controllers/           # MVC та REST API контролери
│       ├── Models/                # Entity Framework Core сутності
│       ├── Views/                 # Razor Views адмін-панелі (Bootstrap)
│       ├── Program.cs             # DI-контейнер, конфігурація JWT та пайплайн
│       └── ServerApp.csproj
├── Client/                        # Клієнтська мобільна частина (.NET MAUI 10)
│   ├── ClientApp.slnx             # Файл рішення клієнта
│   └── ClientApp/
│       ├── ViewModels/            # Архітектура MVVM (CommunityToolkit.Mvvm)
│       ├── Views/                 # Екрани XAML (LoginPage, ItemsPage, ActionPage)
│       ├── Services/              # Клієнт для взаємодії з REST API (HttpClient)
│       └── ClientApp.csproj
├── Database/                      # Скрипти БД (Лабораторна №2)
│   └── init.sql                   # DDL схеми та початкові дані (Seed Data)
└── Docs/                          # Специфікації та документація (OpenAPI/Swagger)
```

---

## 🛠️ 4. Технологічний стек

* **Платформа:** .NET 10.0 SDK (`10.0.401`)
* **Серверний бекенд:** ASP.NET Core (MVC Admin Panel + REST API)
* **Доступ до даних:** Entity Framework Core (Code-First / DDL SQL)
* **Мобільний клієнт:** .NET Multi-platform App UI (.NET MAUI)
* **Патерн клієнта:** MVVM (Model-View-ViewModel) за допомогою `CommunityToolkit.Mvvm`
* **Безпека:** JWT (JSON Web Tokens) для авторизації в REST API та окремий вбудований адмін для MVC

---

## 🚀 5. Інструкція для локального запуску

### Передумови
1. Встановлений [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.401` або новіший).
2. Встановлене мобільне робоче навантаження (Workload):
   ```powershell
   dotnet workload install maui
   ```
3. Встановлений [Git](https://git-scm.com/).

### Клонування репозиторію
```powershell
git clone https://github.com/<ВАШ_НІКНЕЙМ>/ManagementSystem.git
cd ManagementSystem
```

### Запуск серверної частини (ASP.NET Core)
```powershell
# Запуск веб-сервера
dotnet run --project Server/ServerApp/ServerApp.csproj
```
*Адреса за замовчуванням:* `http://localhost:5000` (або порт, вказаний консоллю).

### Збірка та запуск мобільного застосунку (MAUI)
```powershell
# Збірка клієнта
dotnet build Client/ClientApp/ClientApp.csproj

# Запуск на Windows Machine (для швидкої перевірки):
dotnet run --project Client/ClientApp/ClientApp.csproj -f net10.0-windows10.0.19041.0
```

---

## 📡 6. Основні ендпоінти REST API

| Метод | URL | Опис | Доступ |
| :---: | :--- | :--- | :---: |
| `POST` | `/api/auth/login` | Отримання JWT-токена за логіном аналітика | Public |
| `GET` | `/api/champions` | Список доступних чемпіонів для `ItemsPage` | Bearer Token |
| `POST` | `/api/matchuploadouts` | Відправка нового тактичного сетапу проти суперника | Bearer Token |

---

## 📋 7. Дорожня карта виконання (Roadmap)

- [x] **Лаб 1:** Ініціалізація структури, середовища .NET 10 та репозиторію Git.
- [ ] **Лаб 2:** Проєктування БД (`init.sql`, зв'язки Foreign Key, індекси).
- [ ] **Лаб 3:** Підключення Entity Framework Core та моделі сутностей.
- [ ] **Лаб 4:** Реалізація аутентифікації адміністратора (JWT).
- [ ] **Лаб 5:** Створення повноцінної MVC адмін-панелі з CRUD-операціями.
- [ ] **Лаб 6:** Розробка REST API та підключення Swagger UI.
- [ ] **Лаб 7:** JWT-аутентифікація користувачів мобільного додатку через API.
- [ ] **Лаб 8:** Розробка .NET MAUI інтерфейсу на патерні MVVM.
- [ ] **Лаб 9:** Інтеграція MAUI з сервером через `HttpClient`.
- [ ] **Лаб 10:** Комплексне тестування (Postman, емулятор, браузер).
- [ ] **Лаб 11:** Оптимізація запитів та фіналізація індексів БД.
- [ ] **Лаб 12:** Фінальне розгортання та здача проєкту на GitHub.