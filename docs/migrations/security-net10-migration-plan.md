# Миграция Crnc.Oms.Security на .NET 10

## Контекст

CRNC OMS — pet-проект, набор независимых .NET-микросервисов (Security, Sales, Production, Notification.*) на .NET Core 3.1 + React SPA. Владелец — единственный разработчик и хочет постепенно перевести весь проект на современный стек. Это первый шаг: пилотная миграция одного сервиса на .NET 10 (актуальный LTS, вышел в ноябре 2025, поддержка до ~2028), результат станет шаблоном для миграции остальных сервисов позже (вне рамок этого документа).

**Security** выбран как пилот по результатам разведки (3 параллельных Explore-агента + фактчекинг):
- Единственный сервис без MassTransit — самая трудозатратная часть апгрейда (переход с MassTransit 6.x/`GreenPipes`/`IBusControl` на современный API) тут не нужна.
- Нет EF Core/Npgsql — только MongoDB.Driver.
- Архитектурно проще остальных: нет `.Application`-слоя и CQRS-диспетчера (`ICommandQueryDispatcher`), контроллеры дёргают `IUserRepository` напрямую.
- Наименьший и самый однородный набор пакетов.

Sales/Production (MassTransit + EF Core, единственный тестовый проект в репо) и Notification.* — следующие кандидаты, не сейчас.

### Согласованные решения
1. **Целевая версия**: .NET 10 (LTS).
2. **MongoDB.Driver**: обновить сразу до последней мажорной ветки 3.x (не оставаться на 2.x), несмотря на то что это убирает старый LINQ2-провайдер.
3. **Hosting model**: слить `Startup.cs` в `Program.cs` с top-level statements (minimal hosting) — идиоматичный современный подход, а не просто смена TFM.
4. **JSON**: полностью уйти с `Newtonsoft.Json` на `System.Text.Json`.
5. **Без repo-wide `global.json`/`Directory.Build.props`/`.editorconfig`** сейчас — это отложено до момента полной миграции репозитория и решения по структуре `.sln`. Опционально — one-line `global.json` только внутри папки Security, не обязателен.

Все факты в плане перепроверены чтением реальных файлов (не только результатами суб-агентов) — см. точные пути и номера строк ниже.

---

## Пререквизит: e2e-тесты

Перед тем как приступать к самой миграции, на ветке `features/2-add-end-to-end-tests-for-security-project` добавлен набор e2e-тестов: `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests`. Тесты HTTP-интеграционные, на Testcontainers — каждый прогон сам поднимает изолированное окружение (контейнер Mongo + образ security-api, собранный из реального `Dockerfile`), без ручного `docker-compose up`.

Набор покрывает основные сценарии API (аутентификация, CRUD пользователей, роли, авторизация по ролям) и, что важнее всего для этой миграции, включает два теста, которые прицельно проверяют риски, описанные ниже:
- `CreateUser_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys` — автоматическая версия проверки из §4 (`DictionaryKeyPolicy`/`System.Text.Json`).
- `CreateUser_DuplicateLoginDifferentCase_ReturnsBadRequest` — автоматическая версия проверки из §5 (`UserQueries.IsExisted`/LINQ3).

Порядок работы с миграцией: прогнать `dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj` на текущем (ещё netcoreapp3.1) сервисе — это baseline, все тесты должны быть зелёными. После каждого значимого шага миграции (особенно после §4 и §5) прогонять набор снова — в первую очередь два regression-теста выше — прежде чем переходить к ручной проверке через Swagger UI из §7/§8.

---

## Инвентаризация (подтверждено)

Все 4 проекта Security сейчас на `netcoreapp3.1`, без `LangVersion`/`Nullable`:

| Проект | Путь | Пакеты сейчас |
|---|---|---|
| Domain | `Crnc.Oms.Security.Domain/Crnc.Oms.Security.Domain.csproj` | `Microsoft.Extensions.Caching.Abstractions 1.1.0` (не используется нигде — grep по `Caching\|IMemoryCache\|IDistributedCache\|ICacheEntry` внутри Domain пуст) |
| Infrastructure.CrossCutting | `.../Crnc.Oms.Security.Infrastructure.CrossCutting.csproj` | нет пакетов (хелпер хеширования пароля) |
| Infrastructure.DataAccess | `.../Crnc.Oms.Security.Infrastructure.DataAccess.csproj` | `Bogus 28.4.4`, `Microsoft.Extensions.Caching.Memory 3.1.1`, `Microsoft.Extensions.Options 3.1.1`, `MongoDB.Driver 2.10.1` |
| WebApi | `.../Crnc.Oms.Security.WebApi.csproj` | `Microsoft.AspNetCore.Authentication.JwtBearer 3.1.0`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson 3.1.0`, `Microsoft.Extensions.Caching.Memory 3.1.1`, `NSwag.AspNetCore 13.2.0`, `prometheus-net.AspNetCore 3.4.0` |

Нет `.Application`-проекта — `UsersController`/`RolesController`/`AccountsController` работают с `IUserRepository` напрямую. Нет MassTransit — сервис чисто HTTP inbound (выдача JWT + CRUD пользователей/ролей).

**Важные проверенные факты**:
- В коде Security **нет ни одного Newtonsoft-специфичного атрибута** (`[JsonProperty]`, `[JsonIgnore]`, `[JsonConstructor]`, кастомных `JsonConverter`) — единственное место с Newtonsoft это сам `Startup.cs` и `.csproj`.
- **Нет enum-типов** нигде в Domain/Dto Security — регистрация `StringEnumConverter` сейчас мёртвый код без наблюдаемого эффекта.
- `CamelCasePropertyNamesContractResolver` включает `ProcessDictionaryKeys = true` — значит `BadRequest(ModelState)` в `UsersController.cs:141,185` сейчас отдаёт **camelCase-ключи** словаря ошибок валидации (`"firstName": [...]`). `System.Text.Json`'s `PropertyNamingPolicy` НЕ применяется к ключам словаря — нужен отдельный `DictionaryKeyPolicy`. Без этого валидационные ошибки на фронте молча сломаются.
- В `MongoDataContext.cs:25` — `BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;` — это API убрано в MongoDB.Driver 3.x, вызовет **ошибку компиляции**, не рантайм-баг.
- Локально уже установлен .NET 10 SDK (`dotnet --list-sdks` → `10.0.100`) наравне с 8.0.404 и 9.0.101 — можно верифицировать через `dotnet build`/`dotnet run` без доп. установок.

---

## 1. Версии пакетов (проверено на nuget.org)

| Пакет | Проект(ы) | Было | Станет |
|---|---|---|---|
| `MongoDB.Driver` | Infrastructure.DataAccess | 2.10.1 | **3.11.0** |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | WebApi | 3.1.0 | **10.0.11** |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | WebApi | 3.1.0 | **удалить** (переход на STJ) |
| `NSwag.AspNetCore` | WebApi | 13.2.0 | **14.7.1** — breaking rename `UseSwaggerUi3()` → `UseSwaggerUi()` |
| `prometheus-net.AspNetCore` | WebApi | 3.4.0 | **8.2.1** |
| `Bogus` | Infrastructure.DataAccess | 28.4.4 | **35.6.5** (используемое API: `CustomInstantiator`, `RuleFor`, `PickRandom`, `GenerateForever` — давно стабильно) |
| `Microsoft.Extensions.Caching.Memory` | Infrastructure.DataAccess | 3.1.1 | **10.0.11** (остаётся явной зависимостью — это class-library без `FrameworkReference`) |
| `Microsoft.Extensions.Options` | Infrastructure.DataAccess | 3.1.1 | **10.0.11** |
| `Microsoft.Extensions.Caching.Memory` | WebApi | 3.1.1 | **удалить** — уже даёт `Microsoft.AspNetCore.App` shared framework через `Microsoft.NET.Sdk.Web` |
| `Microsoft.Extensions.Caching.Abstractions` | Domain | 1.1.0 | **удалить** — не используется |

---

## 2. Изменения в csproj

`<TargetFramework>` → `net10.0` во всех 4 файлах. Добавить `<ImplicitUsings>enable</ImplicitUsings>` (низкий риск, аддитивно). **`<Nullable>` оставить выключенным** для этого пилота — в коде много намеренно nullable-полей (`User.Photo`, DTO без NRT, ручные null-checks), включение дало бы десятки предупреждений без немедленной пользы; сделать это позже осознанно.

Итоговые файлы (полностью, с учётом удалений неиспользуемых пакетов):

**Domain.csproj** — TFM → `net10.0`, `<ItemGroup>` с `Microsoft.Extensions.Caching.Abstractions` удалить целиком.

**Infrastructure.CrossCutting.csproj** — только TFM → `net10.0` + `ImplicitUsings`.

**Infrastructure.DataAccess.csproj** — TFM → `net10.0`; версии: `Bogus 35.6.5`, `Microsoft.Extensions.Caching.Memory 10.0.11`, `Microsoft.Extensions.Options 10.0.11`, `MongoDB.Driver 3.11.0`.

**WebApi.csproj** — TFM → `net10.0`; версии: `Microsoft.AspNetCore.Authentication.JwtBearer 10.0.11`, `NSwag.AspNetCore 14.7.1`, `prometheus-net.AspNetCore 8.2.1`; удалить строки `Microsoft.AspNetCore.Mvc.NewtonsoftJson` и `Microsoft.Extensions.Caching.Memory`.

---

## 3. Program.cs (слияние Startup.cs → minimal hosting)

Удалить `Crnc.Oms.Security.WebApi/Startup.cs` целиком. Переписать `Crnc.Oms.Security.WebApi/Program.cs`, сохранив 1:1 всё текущее поведение (CORS-политика "AllOrigins", биндинг опций, JWT validation parameters, NSwag doc + security scheme, cache-conditional DI, Prometheus middleware, культура en-US, синхронный вызов `MongoDbInitializer.Initialize()` на каждом старте). Убрать избыточный `ConfigureAppConfiguration` (то же самое уже делает `WebApplication.CreateBuilder` по умолчанию, включая `appsettings.{Environment}.json`) и `ConfigureKestrel(options => {})` (был no-op).

```csharp
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Repositories;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Infrastructure.DataAccess;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.Infrastructure.DataAccess.Repositories;
using Crnc.Oms.Security.WebApi.Authorization;
using Crnc.Oms.Security.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllOrigins", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddOptions();
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("ConnectionStrings:OmsSecurityDb"));
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("Cache"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authSettings = new AuthSettings();
        builder.Configuration.GetSection("Auth").Bind(authSettings);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = authSettings.JwtIssuer,
            ValidAudience = authSettings.JwtAudience,
            IssuerSigningKey = authSettings.SymmetricSecurityKey
        };
    });

builder.Services.AddOpenApiDocument(options =>
{
    options.Title = "Crnc Oms Security API Doc";
    options.Version = "1.0";
    options.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Please insert JWT with Bearer into field. Example: Bearer {your token}"
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var cacheSettings = new CacheSettings();
builder.Configuration.GetSection("Cache").Bind(cacheSettings);

if (cacheSettings.IsUse)
{
    builder.Services.AddScoped<IEntityCollectionCacheProvider<User>, MongoInMemoryEntityCollectionCacheProvider<User>>();
    builder.Services.AddScoped<IEntityCollectionCacheProvider<Role>, MongoInMemoryEntityCollectionCacheProvider<Role>>();
    builder.Services.AddScoped<IUserRepository, CachedMongoDbUserRepository>();
    builder.Services.AddMemoryCache();
}
else
{
    builder.Services.AddScoped<IUserRepository, MongoDbUserRepository>();
}

builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
builder.Services.AddSingleton<MongoDataContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseMonitoringRequestMiddleware();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRouting();
app.UseHttpMetrics();
app.UseCors("AllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();

// Опционально: AddHealthChecks() был зарегистрирован, но никогда не маплен в старом
// Startup.cs — /health фактически никогда не работал, хотя AGENTS.md его упоминает.
// Одна строка, чтобы починить. Можно пропустить, если не хочется трогать поведение сейчас.
app.MapHealthChecks("/health");

app.UseOpenApi();
app.UseSwaggerUi(); // переименован из UseSwaggerUi3() в NSwag v14

// Существующее поведение, вне скоупа этой миграции: полностью пересоздаёт Mongo-базу
// фейковыми Bogus-данными при каждом старте. Работает синхронно перед стартом приёма
// трафика, как и раньше.
var mongoDataContext = app.Services.GetRequiredService<MongoDataContext>();
new MongoDbInitializer(mongoDataContext).Initialize();

app.Run();
```

---

## 4. Переход на System.Text.Json

Единственное место, требующее изменений — блок сериализации выше в `Program.cs` (см. §3). Файлов с Newtonsoft-атрибутами для правки нет (подтверждено grep).

Причины каждой настройки:
- `PropertyNamingPolicy = CamelCase` — прямой эквивалент `CamelCasePropertyNamesContractResolver` для имён свойств.
- `DictionaryKeyPolicy = CamelCase` — **неочевидный нюанс**: без этого ключи словаря в `BadRequest(ModelState)` (`UsersController.cs:141,185`) молча регрессируют с camelCase на PascalCase, ломая рендер ошибок валидации на SPA.
- `PropertyNameCaseInsensitive = true` — Newtonsoft по умолчанию десериализует регистронезависимо, STJ — нет; сохраняет прежнее поведение биндинга `[FromBody]`.
- `JsonStringEnumConverter()` — эквивалент `StringEnumConverter()`; на практике enum'ов в контрактах Security сейчас нет, это задел на будущее.

Проверено, что не требует внимания: null-handling (оба сериализатора по умолчанию включают null), DateTime/decimal (в DTO Security таких полей нет вовсе), Guid-форматирование (одинаковое по умолчанию), байтовые массивы (фото передаётся как `string` через ручной `ContentBase64`, не напрямую).

---

## 5. MongoDB.Driver 3.x

**Обязательное для компиляции изменение** в `Crnc.Oms.Security.Infrastructure.DataAccess/MongoDataContext.cs` — `BsonDefaults.GuidRepresentation` убран в 3.x. Заменить (порядок важен: до `MongoDbMapping.RegisterAllMappings()`, т.к. `AutoMap()` резолвит сериализатор `Guid Id` в момент регистрации маппинга):

```csharp
// было (строка 25)
BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

// станет — до вызова MongoDbMapping.RegisterAllMappings()
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
```

`MongoDataContext` зарегистрирован как `AddSingleton`, конструктор выполняется один раз за жизнь процесса — повторного вызова `RegisterSerializer` (который бросает исключение при повторной регистрации того же типа) не будет.

**LINQ2 → LINQ3**: в 3.x LINQ2-провайдер убран полностью, и клиентские проекции по умолчанию бросают `ExpressionNotSupportedException`, если явно не включить `TranslationOptions.EnableClientSideProjections`. При построчной проверке `MongoDbUserRepository.cs`/`UserQueries.cs` риск оказался уже, чем казалось на первом проходе разведки:

| Запрос | Где выполняется | Риск |
|---|---|---|
| `UserQueries.ById`/`ByLogin`, `RoleQueries.ById` | server-side, простое равенство полей | нет |
| `UserQueries.IsExisted(entity)` — `x.Id == entity.Id \|\| x.Login.ToLower() == entity.Login.ToLower()`, используется в `MongoDbUserRepository.AddAsync` (строка 82) | **server-side**, `.ToLower()` переводится в Mongo aggregation expression | **единственный реальный риск трансляции LINQ3 в этом сервисе** |
| `UserQueries.ByFilter`, `AsUserItemDtoProjection`/`AsUserShortInfoItemDtoProjection` (с `.ToLower()`, `u.Photo?.ContentBase64`, `u.Role.Title`) | применяются через `.Where()`/`.Select()` на уже материализованном `List<User>.AsQueryable()` (после `.ToListAsync()` без предиката) — это обычный LINQ-to-Objects, **не** идёт через Mongo LINQ-провайдер | нет |
| `MongoInMemoryEntityCollectionCacheProvider<T>` | server-side, но без предиката/проекции — полное сканирование коллекции | нет |

Регресс-проверка для `IsExisted` (единственное, что нужно реально протестировать вручную): `POST /api/users` с логином, отличающимся от существующего только регистром → ожидается `400` с `EntityAlreadyExistedException`; затем с валидным новым логином → `200 OK`. Это подтверждает, что и `.ToLower()`, и `||`-композиция всё ещё транслируются корректно.

`Mappings/MongoDbMapping.cs` (сам паттерн `BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap())`) правок не требует. `using MongoDB.Driver.Linq;` в `MongoDbUserRepository.cs:13` — проверить на этапе сборки, не используется ли явно ни один тип оттуда (похоже, что нет — можно будет убрать при появлении warning/ошибки, поведение не изменится).

---

## 6. Dockerfile

`src/Server/src/Crnc.Oms.Security/Dockerfile` — меняются только две строки `FROM`:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build      # было: mcr.microsoft.com/dotnet/core/sdk:3.1
...
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime  # было: mcr.microsoft.com/dotnet/core/aspnet:3.1
```

Обратите внимание — у новых образов путь без сегмента `/core/` (старый путь ретайрен). Триггер кеширования restore-слоя (`COPY *.sln .` + `COPY */*.csproj ./` + `for file in *.csproj; do mkdir -p ...; mv ...; done`) от версии фреймворка не зависит, править не нужно.

`docker-compose.yml` в корне репо изменений не требует — `security-api` продолжает билдиться из того же Dockerfile, переменные окружения (`ConnectionStrings:OmsSecurityDb:Server=mongodb://security-db`, `Cache:IsUse=true`) синтаксически не зависят от TFM.

---

## 7. Порядок выполнения

1. Обновить TFM + версии пакетов во всех 4 csproj (§2).
2. Слить `Startup.cs` в `Program.cs` (§3), удалить `Startup.cs`.
3. Заменить `AddNewtonsoftJson` на `AddJsonOptions` (уже в шаге 2, но отдельно сверить, что `DictionaryKeyPolicy` на месте).
4. Поправить `MongoDataContext.cs` под MongoDB.Driver 3.x (§5) — `GuidSerializer` до `RegisterAllMappings()`.
5. `dotnet build` по всем 4 проектам — добиться чистой сборки; ожидаемые точки поломки: `BsonDefaults.GuidRepresentation` (пока не поправлено), `UseSwaggerUi3()` (переименован).
6. Регресс-проверка LINQ3 для `IsExisted` (§5) — через локальный запуск (`docker-compose up -d security-db` + `dotnet run --project .../Crnc.Oms.Security.WebApi.csproj`).
7. Обновить Dockerfile (§6).
8. End-to-end: `docker-compose build security-api && docker-compose up security-db security-api`, полный ручной прогон через Swagger UI (`http://localhost:8090/swagger`): `POST /api/accounts/auth` (`admin`/`111111`) → JWT → `GET /api/users`, `GET /api/users/{id}`, `GET /api/roles`, `POST/PUT/DELETE /api/users/{id}` (полный CRUD), и отдельно — `POST /api/users` с невалидным телом → проверить, что ключи в теле 400-ответа camelCase (`firstName`, не `FirstName`) — это прямая проверка фикса из §4. По возможности — то же самое через реальный SPA-логин.
9. Опционально, по желанию: `app.MapHealthChecks("/health")` (уже в черновике Program.cs), удалить неиспользуемый `Microsoft.Extensions.Caching.Abstractions` из Domain.csproj, заменить `RNGCryptoServiceProvider` в `PasswordHelper.cs:37` (обозначен `[Obsolete("SYSLIB0023")]` начиная с .NET 6 — новый build warning, не ошибка) на `RandomNumberGenerator.Fill/GetBytes`.

**Изменений в SPA не требуется** — маршруты/контракты не меняются, форма ответов при выбранной JSON-конфигурации побайтово совпадает с текущей (см. §4), кроме сознательно зафиксированного `DictionaryKeyPolicy`-фикса. Подтверждается шагом 8 выше, а не просто предполагается.

---

## Риски и как их ловить

| # | Риск | Как ловить |
|---|---|---|
| 1 | `BsonDefaults.GuidRepresentation` — breaking-компиляция | `dotnet build` сразу падает на `MongoDataContext.cs` — фикс в §5 |
| 2 | Забытый `DictionaryKeyPolicy` → молчаливая регрессия camelCase→PascalCase в ошибках валидации | Явно протестировать `POST /api/users` с невалидным телом (шаг 8) — обычный smoke-test это не поймает |
| 3 | Неверная трансляция `.ToLower()` в `IsExisted` через LINQ3 | Регресс-тест кейс-инсенситивного дубля логина (§5) |
| 4 | `UseSwaggerUi3()` переименован в NSwag 14 | Ловится компилятором мгновенно |
| 5 | Bogus 28→35 (мажорный скачок) | Использованное API (`CustomInstantiator`, `RuleFor`, `PickRandom`, `GenerateForever`) давно стабильно; проверить на `GET /api/users`, что seed-пользователи (включая `admin`/`111111`) на месте |
| 6 | NSwag может не подхватить camelCase-схему из STJ-конфига | Визуально сверить схему DTO в Swagger UI после миграции |

## Критичные файлы

- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.WebApi/Startup.cs` (удалить) и `Program.cs` (переписать, §3)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.Infrastructure.DataAccess/MongoDataContext.cs` (GuidSerializer, §5)
- Все 4 `.csproj` под `Crnc.Oms.Security/` (§2)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.Infrastructure.DataAccess/Repositories/MongoDbUserRepository.cs` (цель регресс-теста `IsExisted`)
- `src/Server/src/Crnc.Oms.Security/Dockerfile` (§6)
