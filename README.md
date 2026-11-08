# Практичне заняття 13: Командний інтерфейс додатку

**Дисципліна:** Об'єктно-орієнтоване програмування (3 курс)  
**Тема:** Архітектура та розробка командного інтерфейсу (CLI Facade, Strategy pattern, ArgsParser, IRenderer, IRenderable, CLIGraphic)  
**Стек / Платформа:** .NET 8 (C# 12), xUnit 2.5  

---

## 🎯 Мета роботи
- Доробити й фіналізувати консольну частину додатку (рівень Користувацького Інтерфейсу / Presentation Layer).
- Впровадити об'єктно-орієнтовані патерни проєктування (**Facade**, **Strategy**, **Dependency Inversion / Inverted Rendering**) для забезпечення гнучкості, розширюваності та чистоти архітектури.
- Створити типізовані локальні моделі командного інтерфейсу:
  - `CliFacade`: центральний фасад взаємодії із зовнішнім кодом (`useStrategy`, `execCommand`, `display`).
  - `Args` та `ArgsParser`: об'єктне представлення термінальних аргументів, опцій та прапорців із підтримкою лапок.
  - `Command`: об'єктно-орієнтована інкапсуляція конкретної команди замість сирих лямбд.
  - `CLIGraphic`: графічні елементи консолі (динамічні таблиці з рамками, смужки очікування / progress bars, діалогові бокси).
  - `IRenderer` та `IRenderable`: інвертована стратегія відображення сутностей у консолі.
- Реалізувати три предметні стратегії команд:
  1. **Information Strategy (Вивід інформації)**: таблиці персонажів із посиланнями на графічні аватари, детальні профілі, списки спорядження та вмінь.
  2. **Transformative Strategy (Трансформативні дії)**: створення сутностей, екіпірування, бойовий цикл (атака, лікування, навички), дуельна арена.
  3. **Infrastructure Strategy (Інфраструктурні дії)**: збереження/відновлення стану в JSON-репозиторії, імпорт із віддаленого REST API з анімованим прогрес-баром, діагностика системи.
- Скласти детальний дизайн-документ ([DESIGN.md](file:///d:/Project/lerning/pk13cli/DESIGN.md)) та покрити рішення автоматизованими модульними тестами xUnit.

---

## 🏗️ Структура модуля CLI

```
pk13cli/
├── CLI/
│   ├── Core/                           # Локальні моделі командного інтерфейсу
│   │   ├── Args.cs                     # Args & ArgsParser (парсинг аргументів, ключів і прапорців)
│   │   ├── Command.cs                  # Абстрактний базовий клас команди та DelegateCommand
│   │   ├── CommandResult.cs            # Результат виконання команди (Success, Message, Data)
│   │   └── CommandExecutionContext.cs  # Контекст виконання (доступ до Game, Repo, Api, Renderer)
│   │
│   ├── Rendering/                      # Рівень візуалізації та консольної графіки
│   │   ├── IRenderer.cs                # Контракт рендерера та інтерфейс IRenderable
│   │   ├── ConsoleRenderer.cs          # Продакшн-рендерер виводу в термінал
│   │   ├── BufferedRenderer.cs         # Тестовий рендерер із накопиченням у буфер
│   │   └── CLIGraphic.cs               # Графічні віджети: таблиці (Table), прогрес-бар, бокси
│   │
│   ├── Strategies/                     # [Strategy Pattern] Групи команд
│   │   ├── ICommandStrategy.cs         # Контракт стратегії команд
│   │   ├── InformationCommandStrategy.cs  # Запити: chars (з аватарками), char, items, abilities
│   │   ├── TransformativeCommandStrategy.cs # Дії: create-char, equip, learn, attack, heal, start-game
│   │   └── InfrastructureCommandStrategy.cs# Інфраструктура: save, load, api-import, api-query, status
│   │
│   ├── Facade/                         # [Facade Pattern] Головний фасад CLI
│   │   └── CliFacade.cs                # Фасад із методами UseStrategy, ExecCommand, Display
│   │
│   └── Presenter/
│       └── ConsolePresenter.cs         # Інтеграція CliFacade в інтерактивний цикл додатку
│
├── DESIGN.md                           # Офіційний архітектурний дизайн-документ проєкту
├── Tests/
│   ├── CliFacadeTests.cs               # [xUnit] 12 комплексних тестів для нового модуля CLI
│   └── Tests.csproj
└── pk13cli.zip                         # Фінальний чистий архів для здачі
```

---

## 📐 Звіт про реалізацію ключових компонентів

### 1. Фасад командного інтерфейсу (`CliFacade`)
Надає чистий зовнішній API:
- `UseStrategy(ICommandStrategy strategy)`: динамічно реєструє нову групу команд.
- `ExecCommand(string rawInput)`: токенізує рядок через `ArgsParser`, знаходить відповідну команду за назвою чи аліасом та виконує її.
- `Display(IRenderable renderable)`: передає активний `IRenderer` у доменну сутність або DTO для форматованого виводу.
- `Display(string message)`: прямий вивід повідомлення.

### 2. Стратегії команд (Command Strategies)
- **Information Strategy**:
  - `chars`: виводить динамічну таблицю персонажів з ID, здоров'ям, силою, бронею та прямим посиланням на аватар (`https://genshin.jmp.blue/characters/{name}/icon`).
  - `char <name>`: відображає оформлену рамкою картку персонажа з повним переліком предметів і вмінь.
  - `items`, `abilities`: структуровані каталоги інвентарю та навичок.
- **Transformative Strategy**:
  - `create-char <name> [hp] [pwr] [res] [--avatar <url>]`: створення персонажа через аргументи або діалог.
  - `create-item <name> <hp> <pwr>`: реєстрація спорядження.
  - `equip <char> <item>`, `learn <char> <ability>`: зміна стану персонажа.
  - `attack <actor> <target> [--critical|--pierce]`: проведення бойового раунду з підтримкою тактичних модифікаторів.
  - `start-game <p1> <p2>`: ініціалізація поєдинку на дуельній арені.
- **Infrastructure Strategy**:
  - `save [name]`, `load [name]`: збереження та завантаження знімків гри через `JsonRepositoryService`.
  - `api-import <charId> [weaponId]`: імпорт даних персонажа та зброї з віддаленого REST API з покроковим відображенням прогрес-бару `[████████░░░░] 50%`.
  - `api-query <charId>`: швидкий попередній перегляд віддалених даних.
  - `status`: вивід поточної системної діагностики та кількості сутностей.
  - `help`: автоматична генерація документації по всіх зареєстрованих стратегіях і командах.

### 3. Консольна графіка (`CLIGraphic`)
- **Таблиці (`FormatTable`)**: автоматичний розрахунок ширини колонок, вирівнювання тексту та формування рамок Unicode (`┌─┬┐`, `├─┼┤`, `└─┴┘`).
- **Смужка прогресу (`FormatProgressBar`)**: візуалізація виконання фонових/довготривалих задач з індикатором відсотка та лічильником кроків `[██████████░░░░░░░░░░] 50% (5/10) Connecting...`.
- **Діалогові бокси (`FormatBox`)**: естетичне обрамлення важливих повідомлень і профілів.

---

## 🧪 Модульне тестування (Unit Testing)

У файлі `Tests/CliFacadeTests.cs` реалізовано 12 модульних тестів (AAA pattern):
- `ArgsParser_ParsesQuotesOptionsAndFlagsAccurately`: токенізація аргументів у лапках, опцій та прапорців.
- `ArgsParser_GenericOptionParser_ReturnsParsedOrFallback`: приведення типів аргументів (`GetOption<int>`).
- `CLIGraphic_FormatTable_ProducesBoxedAsciiTable`: побудова таблиці з рамками.
- `CLIGraphic_FormatProgressBar_ComputesPercentAndFills`: розрахунок заповнення прогрес-бару.
- `CLIGraphic_FormatBox_WrapsLinesWithTitle`: генерація оформлених боксів.
- `InformationStrategy_CharsCommand_RendersTableOfCharacters`: вивід каталогу персонажів.
- `InformationStrategy_CharCommand_RendersDetailedCard`: вивід детальної картки персонажа.
- `TransformativeStrategy_CreateChar_AddsCharacterToGame`: створення персонажа через CLI.
- `TransformativeStrategy_AttackCommand_ExecutesCombatCycle`: виконання атаки з модифікатором.
- `TransformativeStrategy_EquipCommand_AssignsItemToCharacter`: екіпірування зброї.
- `InfrastructureStrategy_SaveAndLoad_PersistsAndRestoresState`: збереження та відновлення стану.
- `CliFacade_DisplayRenderable_InvokesRendererStrategy`: перевірка інвертованого рендерингу `IRenderable`.
- `CliFacade_HelpCommand_ListsAllStrategiesAndCommands`: генерація довідки по всіх стратегіях.

---

## 🚀 Інструкція зі збірки та запуску

### 1. Збірка проєкту
```bash
dotnet build Project.sln
```

### 2. Запуск інтерактивного CLI
```bash
dotnet run --project Project.csproj
```

### 3. Запуск модульних тестів
```bash
dotnet test Tests/Tests.csproj
```
