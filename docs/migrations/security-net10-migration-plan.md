# Миграция Crnc.Oms.Security на .NET 10

## Контекст

CRNC OMS — pet-проект, набор независимых .NET-микросервисов (Security, Sales, Production, Notification.*) на .NET Core 3.1 + React SPA. Владелец — единственный разработчик и хочет постепенно перевести весь проект на современный стек. Это первый шаг: пилотная миграция одного сервиса на .NET 10 (актуальный LTS, вышел в ноябре 2025, поддержка до ~2028), результат станет шаблоном для миграции остальных сервисов позже (вне рамок этого документа).

**Security** выбран как пилот по результатам разведки (3 параллельных Explore-агента + фактчекинг):
- Единственный сервис без MassTransit — самая трудозатратная часть апгрейда (переход с MassTransit 6.x/`GreenPipes`/`IBusControl` на современный API) тут не нужна.
- Нет EF Core/Npgsql — только MongoDB.Driver.
- Архитектурно проще остальных: нет `.Application`-слоя и CQRS-диспетчера (`ICommandQueryDispatcher`), контроллеры дёргают `IUserRepository` напрямую.
- Наименьший и самый однородный набор пакетов.

Sales/Production (MassTransit + EF Core; у Sales единственный юнит-тестовый проект в репо) и Notification.* — следующие кандидаты, не сейчас.

### Согласованные решения
1. **Целевая версия**: .NET 10 (LTS).
2. **MongoDB.Driver**: обновить сразу до последней мажорной ветки 3.x (не оставаться на 2.x), несмотря на то что это убирает старый LINQ2-провайдер.
3. **Hosting model**: слить `Startup.cs` в `Program.cs` с top-level statements (minimal hosting) — идиоматичный современный подход, а не просто смена TFM.
4. **JSON**: полностью уйти с `Newtonsoft.Json` на `System.Text.Json`.
5. **Без repo-wide `global.json`/`Directory.Build.props`/`.editorconfig`** сейчас — это отложено до момента полной миграции репозитория и решения по структуре `.sln`. Опционально — one-line `global.json` только внутри папки Security, не обязателен.

Все факты в плане перепроверены чтением реальных файлов (не только результатами суб-агентов) — см. точные пути и номера строк ниже.

**Ревизия 2026-08-15** (после добавления e2e-тестов): версии пакетов §1 пересверены с nuget.org, breaking changes MongoDB 3.x — с официальным upgrade-гайдом, анализ LINQ-рисков §5 — повторным чтением `MongoDbUserRepository.cs`. Найдены и добавлены два блокера, которых не было в первой редакции: `NETSDK1194` в Dockerfile (§6.2) и короткий JWT-ключ (риск 2). Также добавлены §6.3/§6.4 (`.dockerignore`, членство e2e-проекта в `.sln`) и §8 (правки AGENTS.md).

---

## Пререквизит: e2e-тесты (выполнено, в `master`)

Перед тем как приступать к самой миграции, добавлен набор e2e-тестов: `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests` (18 тестов, все зелёные на текущем netcoreapp3.1-сервисе). Тесты HTTP-интеграционные, на Testcontainers — каждый прогон сам поднимает изолированное окружение (контейнер Mongo + образ security-api, собранный из реального `Dockerfile`), без ручного `docker-compose up`.

Набор покрывает основные сценарии API (аутентификация, CRUD пользователей, роли, авторизация по ролям) и, что важнее всего для этой миграции, включает два теста, которые прицельно проверяют риски, описанные ниже:
- `CreateUser_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys` — автоматическая версия проверки из §4 (`DictionaryKeyPolicy`/`System.Text.Json`).
- `CreateUser_DuplicateLoginDifferentCase_ReturnsBadRequest` — автоматическая версия проверки из §5 (`UserQueries.IsExisted`/LINQ3).

Запуск:
```
dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj
```
**На Windows обязательно задать `DOCKER_HOST=tcp://localhost:2375`** (и включить в Docker Desktop «Expose daemon on tcp://localhost:2375 without TLS»): Testcontainers не подхватывает npipe-контекст `desktop-linux` и вместо ошибки просто виснет.

Порядок работы с миграцией: baseline уже зафиксирован (18/18 зелёных до миграции). После каждого значимого шага миграции — особенно после §4 и §5 — прогонять набор снова, в первую очередь два regression-теста выше, прежде чем переходить к ручной проверке через Swagger UI.

Побочный эффект, полезный для миграции: фикстура собирает образ из реального `Dockerfile`, поэтому прогон тестов заодно проверяет, что сборка образа не сломалась (см. §6 — там как раз есть блокирующая правка).

Попутно этими тестами уже найден и исправлен (в `master`) баг, не связанный с миграцией: `CachedMongoDbUserRepository.FindByIdAsync` возвращал `null` вместо `MissingEntityException`, из-за чего `GET /api/users/{неизвестный id}` отдавал 500 вместо 404.

---

## Инвентаризация (подтверждено)

В папке сервиса 5 проектов, но мигрируются только 4 — `Crnc.Oms.Security.E2ETests` уже написан на `net10.0` и **из миграции исключён** (он не ссылается на проекты сервиса, а работает с ним по HTTP через контейнер, поэтому переживает смену TFM без правок).

Все 4 мигрируемых проекта сейчас на `netcoreapp3.1`, без `LangVersion`/`Nullable`:

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
- Локально уже установлен .NET 10 SDK (`dotnet --list-sdks` → `10.0.100`) наравне с 8.0.404 и 9.0.101 — можно верифицировать через `dotnet build`/`dotnet run` без доп. установок. Сборка netcoreapp3.1-проектов новым SDK работает, но выдаёт warning `NETSDK1138` («target framework is out of support») — это ожидаемо до миграции и не является ошибкой.
- **JWT-ключ короче, чем требует RFC**: `appsettings.json:14` — `"JwtBase64SymmetricKey": "D66D0341FB220444284FC1A90700B38A"` = 32 base64-символа = **24 байта = 192 бита**. RFC 7518 требует для HS256 ключ ≥256 бит. Сейчас работает, потому что исторический минимум `Microsoft.IdentityModel` — 128 бит, но 192 бита попадают ровно между старым минимумом и требованием RFC. Подробности и план действий — в §«Риски», пункт 2.
- В `Crnc.Oms.Security.WebApi/Authorization/` нет никаких policy handlers — только константы ролей (`Roles.cs`) и `AuthSettings`. (AGENTS.md сейчас утверждает обратное — правится в §8.)

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
| `UserQueries.ByFilter`, `AsUserItemDtoProjection`/`AsUserShortInfoItemDtoProjection` (с `.ToLower()`, `u.Photo?.ContentBase64`, `u.Role.Title`) | применяются через `.Where()`/`.Select()` на уже материализованном `List<User>.AsQueryable()` (после `.ToListAsync()` без предиката — `MongoDbUserRepository.cs:43,55`) — это обычный LINQ-to-Objects, **не** идёт через Mongo LINQ-провайдер | нет |
| Все методы `CachedMongoDbUserRepository` (включая `FindByIdAsync`) | работают поверх коллекции из `IMemoryCache` через `AsQueryable()` — тоже LINQ-to-Objects | нет |
| `MongoInMemoryEntityCollectionCacheProvider<T>` | server-side, но без предиката/проекции — полное сканирование коллекции | нет |

Регресс-проверка для `IsExisted` (единственное, что нужно реально протестировать вручную): `POST /api/users` с логином, отличающимся от существующего только регистром → ожидается `400` с `EntityAlreadyExistedException`; затем с валидным новым логином → `200 OK`. Это подтверждает, что и `.ToLower()`, и `||`-композиция всё ещё транслируются корректно.

`Mappings/MongoDbMapping.cs` (сам паттерн `BsonClassMap.RegisterClassMap<T>(cm => cm.AutoMap())`) правок не требует. `using MongoDB.Driver.Linq;` в `MongoDbUserRepository.cs:13` — проверить на этапе сборки, не используется ли явно ни один тип оттуда (похоже, что нет — можно будет убрать при появлении warning/ошибки, поведение не изменится).

**Запечатанные классы**: в 3.x `MongoClient`, `MongoDatabase` и `MongoCollection` стали `sealed`. На нас не влияет — `MongoDataContext` их только хранит (`public MongoClient Client { get; private set; }`), не наследуется от них. Опционально, по рекомендации MongoDB, можно сменить тип свойства на интерфейс `IMongoClient`.

---

## 6. Dockerfile и Docker build-контекст

Правок здесь больше, чем «две строки `FROM`» — есть блокирующая.

### 6.1. Базовые образы

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build      # было: mcr.microsoft.com/dotnet/core/sdk:3.1
...
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime  # было: mcr.microsoft.com/dotnet/core/aspnet:3.1
```

Обратите внимание — у новых образов путь без сегмента `/core/` (старый путь ретайрен).

### 6.2. `dotnet publish -o` по solution — блокер (NETSDK1194)

Текущие строки `RUN dotnet restore` и `RUN dotnet publish -c Release -o out` выполняются **без явного таргета**, то есть против solution (`Crnc.Oms.Security.sln` лежит рядом). Начиная с SDK 7.0.200 опция `--output` для solution — **ошибка `NETSDK1194`** («The '--output' option isn't supported when building a solution»). На `sdk:3.1` это работало, на `sdk:10.0` сборка образа упадёт. Заменить обе строки на явный таргет:

```dockerfile
RUN dotnet restore Crnc.Oms.Security.WebApi/Crnc.Oms.Security.WebApi.csproj
...
RUN dotnet publish Crnc.Oms.Security.WebApi/Crnc.Oms.Security.WebApi.csproj -c Release -o out
```

Побочные плюсы: restore тянет только WebApi и его три `ProjectReference` вместо всей solution, и состав solution перестаёт влиять на прод-образ (это то, что делает возможным §6.4).

Сам трюк с кешированием restore-слоя (`COPY *.sln .` + `COPY */*.csproj ./` + `for file in *.csproj; do mkdir -p ...; mv ...; done`) от версии фреймворка не зависит и остаётся как есть.

### 6.3. Нужен `.dockerignore` (новый файл)

`Crnc.Oms.Security.E2ETests/` физически лежит **внутри Docker build-контекста** сервиса. Последствия сегодня: `COPY . ./aspnetapp` затягивает `bin/` тестового проекта (~18 МБ), контекст вырос с ~1.7 МБ до ~21 МБ. Хуже того, фикстура тестов пересобирает образ из этого же каталога, а её собственный `bin/` меняется на каждом прогоне — то есть слой `COPY` инвалидируется **при каждом запуске тестов**. `.gitignore` для Docker не действует, нужен отдельный `src/Server/src/Crnc.Oms.Security/.dockerignore`:

```
Crnc.Oms.Security.E2ETests/
**/bin/
**/obj/
```

### 6.4. Добавить E2ETests в `Crnc.Oms.Security.sln` — только после 6.2 и 6.3

По конвенции из `AGENTS.md` каждый контекст собирается и тестируется своим `.sln`, но `Crnc.Oms.Security.E2ETests` сейчас зарегистрирован только в корневом `src/Server/Crnc.Oms.sln`. Его нужно добавить и в `Crnc.Oms.Security.sln`, **строго после** правок 6.2 и 6.3.

Причина жёсткого порядка: пока Dockerfile делает `dotnet restore` по solution на `sdk:3.1`, появление в ней `net10.0`-проекта немедленно ломает сборку образа (`NETSDK1045` — SDK 3.1 не знает про net10.0), а вместе с образом ломаются и сами e2e-тесты, которые его собирают. После 6.2 restore идёт по конкретному csproj, и состав solution на образ не влияет.

### 6.5. docker-compose

`docker-compose.yml` в корне репо изменений не требует — `security-api` продолжает билдиться из того же Dockerfile, переменные окружения (`ConnectionStrings:OmsSecurityDb:Server=mongodb://security-db`, `Cache:IsUse=true`) синтаксически не зависят от TFM.

---

## 7. Порядок выполнения

**Подготовка инфраструктуры сборки (до смены TFM, работает на текущем 3.1):**

1. Перевести restore/publish в Dockerfile на явный csproj (§6.2) и добавить `.dockerignore` (§6.3). Прогнать e2e-набор — образ должен собираться как раньше, тесты остаться зелёными.
2. Добавить `Crnc.Oms.Security.E2ETests` в `Crnc.Oms.Security.sln` (§6.4). Снова прогнать e2e-набор.

**Сама миграция:**

3. Обновить TFM + версии пакетов в 4 мигрируемых csproj (§2) — `Crnc.Oms.Security.E2ETests` не трогать, он уже на net10.0.
4. Слить `Startup.cs` в `Program.cs` (§3), удалить `Startup.cs`.
5. Заменить `AddNewtonsoftJson` на `AddJsonOptions` (уже в шаге 4, но отдельно сверить, что `DictionaryKeyPolicy` на месте).
6. Поправить `MongoDataContext.cs` под MongoDB.Driver 3.x (§5) — `GuidSerializer` до `RegisterAllMappings()`.
7. `dotnet build` по всем 4 проектам — добиться чистой сборки; ожидаемые точки поломки: `BsonDefaults.GuidRepresentation` (пока не поправлено), `UseSwaggerUi3()` (переименован).
8. Поменять базовые образы в Dockerfile на `sdk:10.0`/`aspnet:10.0` (§6.1).

**Проверка:**

9. Прогнать e2e-набор — это основная проверка миграции, она же покрывает регрессы из §4 (camelCase-ключи валидации), §5 (LINQ3 в `IsExisted`), выдачу JWT (см. риск 2) и сам факт того, что образ на новом SDK собирается:
   ```
   dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj
   ```
   (на Windows — с `DOCKER_HOST=tcp://localhost:2375`, см. секцию «Пререквизит»).
10. Дополнительно, вручную: `docker-compose build security-api && docker-compose up security-db security-api`, прогон через Swagger UI (`http://localhost:8090/swagger`) — логин `admin`/`111111`, `GET /api/users`, `GET /api/roles`, CRUD пользователя; и по возможности то же через реальный SPA-логин. Это ловит то, что тесты не проверяют: работу самого Swagger UI, генерацию OpenAPI-схемы новым NSwag (риск 6) и внешний вид ответов.
11. Обновить `AGENTS.md` (§8).
12. Опционально, по желанию: `app.MapHealthChecks("/health")` (уже в черновике Program.cs), удалить неиспользуемый `Microsoft.Extensions.Caching.Abstractions` из Domain.csproj, заменить `RNGCryptoServiceProvider` в `PasswordHelper.cs:37` (обозначен `[Obsolete("SYSLIB0023")]` начиная с .NET 6 — новый build warning, не ошибка) на `RandomNumberGenerator.Fill/GetBytes`, сменить тип `MongoDataContext.Client` на `IMongoClient` (§5).

**Изменений в SPA не требуется** — маршруты/контракты не меняются, форма ответов при выбранной JSON-конфигурации побайтово совпадает с текущей (см. §4), кроме сознательно зафиксированного `DictionaryKeyPolicy`-фикса. Подтверждается шагами 9-10, а не просто предполагается.

---

## 8. Обновление AGENTS.md

После миграции `AGENTS.md` станет местами неверным — часть утверждений неверна уже сейчас. Правки:

Уже неверно (можно править в любой момент):
- «Only `Crnc.Oms.Sales` currently has an automated test project» — добавить `Crnc.Oms.Security.E2ETests` (e2e поверх HTTP на Testcontainers).
- «`Authorization/` has role-based policy handlers» — для Security неверно: там только константы ролей и `AuthSettings`, никаких policy handlers.
- Health checks `/health` — уточнить, что в Security `AddHealthChecks()` зарегистрирован, но никогда не был замаплен, то есть эндпоинт не отвечает (чинится опционально, шаг 12).
- В секцию Commands добавить команду запуска e2e-тестов Security и Windows-готчу с `DOCKER_HOST=tcp://localhost:2375`.
- Зафиксировать как ориентир: **все сервисы потенциально должны иметь юнит- и интеграционные тесты**. Текущее состояние: Security — e2e-набор, Sales — юниты по Domain, остальные пока без тестов.

Станет неверным после миграции:
- Заголовок «### Backend (.NET Core 3.1)» — отразить смешанное состояние: Security на .NET 10, остальные сервисы ещё на .NET Core 3.1.
- «`.WebApi` — ASP.NET Core host: `Startup.cs` wires DI…» — оговорить, что Security перешёл на minimal hosting и `Startup.cs` у него больше нет.

---

## Риски и как их ловить

Риски 1-8 — из первой редакции плана, все подтвердились именно так, как описано. Риски 9-11 обнаружены только при реальном прогоне мигрированного сервиса (первая редакция плана их не предвидела — все три были ложно приняты за «зависание» e2e-тестов, пока не разобрались по логам контейнеров).

| # | Риск | Как ловить |
|---|---|---|
| 1 | **`dotnet publish -o` по solution — `NETSDK1194`, сборка образа падает на `sdk:10.0`** | Гарантированно ловится любой сборкой образа (в т.ч. прогоном e2e-тестов). Фикс — §6.2, сделать до смены базовых образов |
| 2 | **JWT-ключ 192 бита при требовании RFC 7518 ≥256 бит для HS256** — `Microsoft.IdentityModel` 7/8 ужесточал валидацию размера (есть escape-hatch `UnsafeRelaxHmacKeySizeValidation`). Падение **в рантайме при выдаче JWT**: ломается логин, а с ним всё приложение | Тест `Authenticate_ValidAdminCredentials_ReturnsJwtAndUserInfo` падает сразу. Фикс — сгенерировать новый 32-байтный ключ и положить в `appsettings.json` (в docker-compose он не переопределяется) |
| 3 | `BsonDefaults.GuidRepresentation` — breaking-компиляция | `dotnet build` сразу падает на `MongoDataContext.cs` — фикс в §5 |
| 4 | Забытый `DictionaryKeyPolicy` → молчаливая регрессия camelCase→PascalCase в ошибках валидации | Тест `CreateUser_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys`; обычный smoke-test это не поймает |
| 5 | Неверная трансляция `.ToLower()` в `IsExisted` через LINQ3 | Тест `CreateUser_DuplicateLoginDifferentCase_ReturnsBadRequest` |
| 6 | `UseSwaggerUi3()` переименован в NSwag 14 | Ловится компилятором мгновенно |
| 7 | Bogus 28→35 (мажорный скачок) | Использованное API (`CustomInstantiator`, `RuleFor`, `PickRandom`, `GenerateForever`) давно стабильно; тесты `GetUsers_*`/`GetUserById_KnownSeededAdminId_*` проверяют, что seed-пользователи (включая `admin`) на месте |
| 8 | NSwag может не подхватить camelCase-схему из STJ-конфига | Тестами не покрыто — визуально сверить схему DTO в Swagger UI после миграции (шаг 10) |
| 9 | **MongoDB.Driver 3.x требует сервер с wire version ≥9 (MongoDB ≥4.4.0)** — `mongo:4.2.3` (и в `docker-compose.yml`, и в тестовой фикстуре) даёт только wire version 8. Приложение падает сразу при первом обращении к БД с `MongoIncompatibleDriverException: Server ... reports wire version 8, but this version of the driver requires at least 9` | Любой e2e-тест/ручной запуск — сразу видно в логах контейнера. Фикс: обновить образ Mongo в обоих местах — здесь выбрана актуальная стабильная `mongo:8.3.8` (проверено на nuget/Docker Hub 2026-08-19); т.к. `MongoDbInitializer` дропает и пересевает базу на каждом старте, миграции данных не требуется |
| 10 | **Образ `mcr.microsoft.com/dotnet/aspnet:10.0` по умолчанию слушает порт 8080** (`ASPNETCORE_HTTP_PORTS=8080` зашит в образ), а не 80, как старый `dotnet/core/aspnet:3.1`. Приложение стартует и работает полностью нормально, просто на другом порту — выглядит это как «зависшая» проверка готовности контейнера (и в Testcontainers, и при `docker run -p host:80`), а не как явная ошибка | Обнаруживается только руками — сравнением `docker exec <container> env \| grep ASPNETCORE_HTTP_PORTS` с ожидаемым портом, или через `/proc/net/tcp` внутри контейнера. **Решение: принять дефолт образа (8080), а не переопределять его в Dockerfile** — обновить все места, которые ходят на Security изнутри Docker-сети без явного порта (там неявно резолвится 80): `docker-compose.yml` — `security-api.ports` → `"8090:8080"` (внешний порт 8090 не меняется) и три `IntegrationEndpoints:SecurityServiceEndpoint=http://security-api` (Sales, Notification.Gateway, Notification.Push.Client) → `http://security-api:8080`; `prometheus/prometheus.yml` — таргет `security-api` → `security-api:8080`; `SecurityApiFixture.cs` — `ApiContainerPort` → `8080`. AGENTS.md/README не трогать — внешний URL `http://localhost:8090` не изменился |
| 11 | `MongoDbInitializer.Initialize()` использовал `.GetAwaiter().GetResult()` поверх async Mongo-вызовов — не является причиной риска 10 (перепроверено: с этим кодом как есть и с переводом на честный `async`/`await` результат одинаков), но переведён на `InitializeAsync()`/`await` как более безопасная практика уже в рамках этой миграции, раз всё равно разбирались с этим кодом | Не тестозависимый риск сам по себе — просто заодно устранённый code smell |

## Критичные файлы

- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.WebApi/Startup.cs` (удалить) и `Program.cs` (переписать, §3)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.Infrastructure.DataAccess/MongoDataContext.cs` (GuidSerializer, §5)
- 4 мигрируемых `.csproj` под `Crnc.Oms.Security/` (§2) — `Crnc.Oms.Security.E2ETests.csproj` не трогаем
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.Infrastructure.DataAccess/Repositories/MongoDbUserRepository.cs` (цель регресс-теста `IsExisted`)
- `src/Server/src/Crnc.Oms.Security/Dockerfile` (§6.1, §6.2) и новый `src/Server/src/Crnc.Oms.Security/.dockerignore` (§6.3)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.sln` (§6.4)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.WebApi/appsettings.json` — риск 2, новый 32-байтный JWT-ключ (сработал)
- `src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.Infrastructure.DataAccess/MongoDbInitializer.cs` — переведён на `InitializeAsync`/`await` (риск 11)
- `docker-compose.yml` — `security-db` → `mongo:8.3.8` (риск 9); `security-api.ports` → `"8090:8080"` и три `IntegrationEndpoints:SecurityServiceEndpoint` → `http://security-api:8080` (риск 10)
- `prometheus/prometheus.yml` — таргет `security-api` → `security-api:8080` (риск 10)
- `Crnc.Oms.Security.E2ETests/SecurityApiFixture.cs` — `mongo:8.3.8` (риск 9) и `ApiContainerPort = 8080` (риск 10)
- `AGENTS.md` (§8 + версия MongoDB в таблице БД)

## Статус

Миграция выполнена и подтверждена: `dotnet build Crnc.Oms.Security.sln` — чисто, `dotnet test Crnc.Oms.Security.E2ETests` — 18/18 зелёных на полностью мигрированном сервисе (net10.0 + MongoDB.Driver 3.11.0 + MongoDB 8.3.8 + System.Text.Json + minimal hosting), плюс ручная проверка через `docker run`/`curl`: логин, роли, camelCase-ключи валидации, детект дубликата логина.
