# Миграция Crnc.Oms.Sales на .NET 10

## Контекст

Второй шаг постепенного перевода CRNC OMS на современный стек. Первый — [Security](security-net10-migration-plan.md) — выполнен и служит шаблоном: e2e-тесты как страховка → правки инфраструктуры сборки → смена TFM и пакетов → minimal hosting → System.Text.Json → проверка тем же набором тестов.

Sales отличается от Security по объёму принципиально: там был чистый HTTP-сервис с одним Mongo-драйвером, здесь — EF Core + PostgreSQL, MassTransit + RabbitMQ, MediatR, RestSharp и живая асинхронная интеграция с Production и Notification, которые **остаются на .NET Core 3.1 и MassTransit 6**. Поэтому в плане три темы, которых в миграции Security не было вовсе: маппинг дат в Npgsql, кросс-версионная совместимость шины и лицензии пакетов.

### Согласованные решения

1. **Целевая версия**: .NET 10 (LTS), как у Security.
2. **Hosting model**: слить `Startup.cs` в `Program.cs` (minimal hosting) — так же, как в Security.
3. **JSON**: полностью уйти с `Newtonsoft.Json` на `System.Text.Json`.
4. **MassTransit — остаёмся на ветке 8.x**, не 9.x. Проверено по nuspec на nuget.org: `MassTransit 8.5.10` — `Apache-2.0`, `MassTransit 9.2.0` — проприетарная лицензия (`licenseUrl = https://massient.com/license`, проект переехал на massient.com). 8.5.10 явно таргетит `net10.0`, так что ограничение лицензией ничего не стоит технически.
5. **MediatR — остаёмся на 12.5.0**, не 13/14. Аналогично: `MediatR 12.5.0` — `Apache-2.0`, начиная с `13.0.0` — `requireLicenseAcceptance=true` и лицензия-файл (mediatr.io, коммерческая). 12.5.0 таргетит `netstandard2.0`/`net6.0` и на net10.0 работает.
6. **RestSharp — заменить на `HttpClient` + `System.Net.Http.Json`**, а не обновлять со 106 до 114. Обоснование в §7: живое использование ровно одно, а API RestSharp с 106 по 112 сломан почти целиком, так что «обновление» по объёму правок не отличается от переписывания, но оставляет лишнюю зависимость.
7. **PostgreSQL в docker-compose поднять с 9.6.17 до 18.6.** Npgsql держит совместимость «5 лет назад» от актуальных версий PostgreSQL; 9.6 (EOL ноябрь 2021) вне этого окна — «может работать, но не тестируется». Миграции данных не нужно: `SalesDbInitializer` делает `EnsureDeleted()`/`EnsureCreated()` на каждом старте.
8. **`<Nullable>` не включаем** — как и в Security, отложено.
9. **Домен и контракты сообщений не трогаем.** `Crnc.Oms.Sales.Messaging.Contract` остаётся на `netstandard2.0` — он парная копия контракта Production, и смена TFM тут ничего не даёт.

Все факты ниже перепроверены чтением реальных файлов и запросами к nuget.org / документации Npgsql и MassTransit (даты проверки — 2026-08-19), а не перенесены по аналогии с планом Security.

---

## Блокер №0: рассинхрон JWT-ключа после миграции Security (уже в `master`)

**Это надо чинить до всего остального, включая e2e-тесты.**

Миграция Security сгенерировала новый 32-байтный ключ подписи JWT (риск 2 её плана). Ключ поменяли только в самом Security:

| Сервис | `Auth:JwtBase64SymmetricKey` |
|---|---|
| Security (выдаёт токены) | `ldtLvqRHfPc8UW0My3jOKr0imGUjZjsNGVYSWn4NdCY=` (32 байта) |
| Sales | `D66D0341FB220444284FC1A90700B38A` (24 байта, старый) |
| Production | тот же старый |
| Notification.Gateway / .Email / .Push | тот же старый |

`docker-compose.yml` секцию `Auth` не переопределяет вообще (`grep -n "Auth" docker-compose.yml` пуст), то есть каждый сервис валидирует подпись своим `appsettings.json`. Значит **сейчас в `master` токен, выданный Security, отвергается всеми остальными сервисами**: логин в SPA проходит, а любой запрос к Sales/Production/Notification возвращает 401. Система сломана end-to-end.

Фикс: проставить новый ключ Security в `appsettings.json` остальных пяти сервисов. Это баг в `master`, а не часть миграции, но чинится он внутри неё (фаза 0 в §10). E2E-тесты Sales от него **не зависят** — они не поднимают Security и подписывают токен сами, см. следующий раздел.

---

## Пререквизит: e2e-тесты Sales (следующий шаг, ветка `6-add-end-to-end-tests-for-salesproject`)

По схеме Security: сначала набор e2e-тестов, зелёный на текущем `netcoreapp3.1`-сервисе, потом миграция под его защитой. Конвенции — в AGENTS.md, раздел «Test conventions»; проект `Crnc.Oms.Sales.E2ETests` на `net10.0`, вне `Crnc.Oms.Sales.sln`, без `ProjectReference`, поверх Testcontainers.

### Периметр: настоящая только БД, всё внешнее — заглушки

Sales, в отличие от Security, не изолирован: он ходит в Security по HTTP и в RabbitMQ по AMQP. Тянуть в тесты соседние сервисы целиком — значит проверять чужой код и делать прогон хрупким. Поэтому периметр такой:

- **PostgreSQL — настоящий.** Это и есть то, что здесь надо проверять по-настоящему: EF-маппинг, owned types, типы колонок под даты. Ровно тот слой, куда бьёт риск 1.
- **Security — заглушка** (`wiremock/wiremock:3.13.2` в контейнере, стабы задаются из фикстуры через его admin API). Sales ждёт от него ровно один ответ — `GET /api/users?roles=Main manager` со списком `UserItemDto`. Настоящий Security для этого не нужен, а его отсутствие заодно снимает зависимость тестов от блокера №0.
- **RabbitMQ — настоящий брокер, но проверяем только факт отправки.** Шину подменить нечем (без брокера сервис не стартует), но система за ней в тесты не входит: убеждаемся, что сообщение легло в очередь, и на этом останавливаемся. Ни Production, ни Notification не поднимаются.

### Контейнеры фикстуры

| Контейнер | Алиас в сети | Зачем |
|---|---|---|
| `postgres:18.6` | `sales-db` | настоящая БД Sales |
| `rabbitmq:3-management` | `message-broker` | шина; management API (15672) — для проверки очередей из теста |
| `wiremock/wiremock:3.13.2` | `security-api` | заглушка Security для `EmployeeSecurityGateway` |
| образ из `Crnc.Oms.Sales/Dockerfile` | — | тестируемый сервис |

Тег RabbitMQ держим равным `docker-compose.yml` (`3-management`); поднимать его до 4.x в рамках этой работы не нужно — текущий сервис на MassTransit 6, и это отдельный вопрос.

### Аутентификация: тест подписывает токен сам

Sales только **валидирует** JWT — выдаёт их Security, которого в тестах нет. Поэтому фикстура генерирует токен сама: HS256 тем же ключом, с `iss`/`aud` из `Auth`, и клеймами, которые читает `CurrentUserContext` (`NameIdentifier`, `Name`, `GivenName`, `Surname`, `Email`).

Ключ фикстура **задаёт сама** через переменную окружения контейнера (`Auth:JwtBase64SymmetricKey=<тестовый ключ>`), а не берёт из `appsettings.json`. Тогда тесты не сломаются ни от выравнивания ключа в фазе 0, ни от любой будущей его ротации.

### Что покрываем

- **CRUD заказов через HTTP**: `GET /api/orders`, `GET /api/orders/{id}`, `GET /api/orders/new`, `POST /api/orders`, `PUT /api/orders`; 401 без токена; 404 на неизвестный id. Сид-заказ `5c5c6017-…` из `SalesDbInitializer` — известная точка опоры.
- **Даты** — прицельно под риск 1 (Npgsql/timestamptz): созданный заказ должен сохраняться и читаться обратно, а `dateCreated`/`dateSentToCustomer` приходить в формате `dd.MM.yyyy` (`DateTimeExtensions`). Именно этот тест первым покажет `InvalidCastException` после смены провайдера.
- **Enum'ы числами** — под §4: `status`, `jobType`, `materialSource` в JSON числа, не строки.
- **camelCase-ключи ошибок валидации** — под §4, аналог `CreateUser_MissingRequiredField_ReturnsBadRequestWithCamelCaseKeys`: `POST /api/orders` без `jobDescription` → 400 с ключом `jobDescription`, не `FirstName`-стиля.
- **Смена статуса → сообщение в очереди**: `PUT /api/orders` со статусом `NeedSignoff` → в очереди `sendNotificationToUser` появилось сообщение. Проверка через management API RabbitMQ (`GET /api/queues/%2f/sendNotificationToUser` → `messages`), без MassTransit в тестовом проекте.
- **Конвертация в работу → событие опубликовано**: `PUT /api/orders` со статусом `ConvertedToJob` (и заполненным `materialSource`) → событие ушло в exchange `Crnc.Oms.Messaging.Contract.Events:OrderConvertedToJobEvent`. Важная деталь: `Publish` идёт в fanout-exchange, и **без подписчика сообщение просто отбрасывается**, счётчик очереди не вырастет. Поэтому тест до действия сам объявляет временную очередь и биндит её к этому exchange через management API, а потом проверяет, что в ней что-то появилось.
- **Обращение к Security** — под §7 (замена RestSharp на `HttpClient`): смена статуса проходит успешно, когда заглушка отвечает списком менеджеров, и не валит запрос, когда заглушка отвечает 500 (`OrderStatusChangedHandler` глотает ошибку в лог). Заодно можно сверить через `GET /__admin/requests` WireMock, что Sales действительно сходил на `/api/users` с `Bearer`-заголовком — это и есть регресс-тест на склейку `BaseAddress` (риск 10).

### Что сознательно НЕ покрываем

- **Потребление `JobCreatedForOrderEvent`** (`JobCreatedForOrderConsumer`). Чтобы его дёрнуть, тест должен опубликовать сообщение в формате конверта MassTransit — то есть либо тащить MassTransit в тестовый проект, либо собирать конверт руками. Обратная связь «работа создана → `jobId` вернулся в заказ» проверяется вручную на `docker-compose up` (шаг 14 в §10).
- **Сквозной сценарий Sales → Production → Sales.** Вне периметра по определению; риск 2 (совместимость MassTransit 8 и 6) e2e-тестами не закрывается — только ручной проверкой.
- **Notification и Push.** Sales до них не доходит, его зона ответственности заканчивается очередью.

Baseline фиксируется на `netcoreapp3.1` **после** фикса блокера №0.

---

## Инвентаризация (подтверждено чтением файлов)

Шесть проектов в папке сервиса, все на `netcoreapp3.1`, кроме `Messaging.Contract` (`netstandard2.0`). `Nullable`/`LangVersion` нигде не заданы.

| Проект | TFM | Пакеты сейчас |
|---|---|---|
| Domain | `netcoreapp3.1` | `MediatR 8.0.0` |
| Application | `netcoreapp3.1` | `MediatR 8.0.0` |
| DataAccess | `netcoreapp3.1` | `EFCore.NamingConventions 1.0.0`, `Microsoft.EntityFrameworkCore 3.1.0`, `Microsoft.EntityFrameworkCore.Proxies 3.1.0`, `Npgsql.EntityFrameworkCore.PostgreSQL 3.1.0` |
| Integration | `netcoreapp3.1` | `MassTransit 6.1.0`, `MassTransit.RabbitMQ 6.1.0`, `Microsoft.Extensions.Logging.Abstractions 3.1.2`, `Microsoft.Extensions.Options 3.1.0`, `RestSharp 106.10.1` |
| Messaging.Contract | `netstandard2.0` | нет |
| WebApi | `netcoreapp3.1` | `MassTransit.AspNetCore 6.1.0`, `MassTransit.Extensions.DependencyInjection 6.1.0`, `MassTransit.Extensions.Logging 5.5.6`, `MassTransit.RabbitMQ 6.1.0`, `MediatR.Extensions.Microsoft.DependencyInjection 8.0.0`, `Microsoft.AspNetCore.Authentication.JwtBearer 3.1.1`, `Microsoft.AspNetCore.Diagnostics 2.2.0`, `Microsoft.AspNetCore.Diagnostics.HealthChecks 2.2.0`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson 3.1.1`, `Microsoft.EntityFrameworkCore 3.1.1`, `Microsoft.IdentityModel.Tokens 5.6.0`, `Npgsql.EntityFrameworkCore.PostgreSQL 3.1.1.2`, `NSwag.AspNetCore 13.2.3`, `prometheus-net.AspNetCore 3.5.0` |

`Crnc.Oms.Sales.Tests` (юниты по Domain) в `Crnc.Oms.Sales.sln` **не входит** — в solution только 5 проектов, тестового нет. То же самое было у Security с его e2e-проектом (§6.4 того плана).

**Важные проверенные факты:**

- **Ни одного Newtonsoft-специфичного атрибута в коде нет** (`grep` по `JsonProperty|JsonIgnore|JsonConverter|Newtonsoft` даёт только три строки в самом `Startup.cs`). Как и в Security — переход на STJ ограничен одним блоком конфигурации.
- **`StringEnumConverter` НЕ зарегистрирован** (в отличие от Security, где он был мёртвым кодом). `AddNewtonsoftJson` задаёт только `CamelCasePropertyNamesContractResolver`. При этом enum'ы в контрактах есть и живые: `JobType`, `OrderStatusEnum`, `MaterialSource?`, `SignoffType?` — значит **сейчас они ездят числами**, и SPA (`EditOrderModel`) ожидает именно числа. В новую конфигурацию `JsonStringEnumConverter` добавлять **нельзя** — сломает фронт. Это ровно обратный вывод по сравнению с планом Security, где конвертер добавили «на будущее».
- `CamelCasePropertyNamesContractResolver` включает `ProcessDictionaryKeys = true`, то есть ключи `ModelState` в автоматическом 400 от `[ApiController]` сейчас camelCase. SPA их не читает (`OrderService` не обрабатывает ошибки вовсе), так что риск ниже, чем в Security, но паритет API держим — см. §4.
- **Все даты в системе — `DateTime.Now`/`DateTime.Today`, то есть `Kind=Local`**: `CurrentDateTimeProvider`, `SalesDbInitializer`, `DomainEvent.CreatedDate`. Персистятся `Order.DateCreated`, `Order.StatusDate`, `Order.DateSentToCustomer`. Это прямой вход в риск 1.
- **`EFCore.NamingConventions` и `Microsoft.EntityFrameworkCore.Proxies` не используются**: единственное упоминание — закомментированный `//optionsBuilder.UseSnakeCaseNamingConvention();` в `SalesDataContext.cs:41`, `UseLazyLoadingProxies` не вызывается нигде, `public virtual` в Domain нет ни одного. Оба пакета удалить.
- **`HttpNotificationGateway` — мёртвый код**: в DI зарегистрирован `MessageBrokerNotificationGateway` (`Startup.cs`), а HTTP-вариант не упоминается больше нигде. Это второе из двух мест с RestSharp; удаление оставляет одно.
- `MessageBrokerNotificationGateway.cs:10` — `using MassTransit.Conductor.Server;` при том, что Conductor в коде не используется. В MassTransit 8 его нет — просто убрать using.
- В `Crnc.Oms.Sales.WebApi/Authorization/` нет policy handlers — только `AuthSettings` и `CurrentUserContext`. (AGENTS.md утверждает обратное про все сервисы; правится в §11.)
- **AGENTS.md неточен насчёт CQRS**: там сказано, что команды/запросы «do not use MediatR directly». На деле `CommandQueryDispatcher` — тонкая обёртка над `IMediator.Send`, `IUseCaseCommand<TOut> : IRequest<TOut>`, `IUseCaseCommandHandler : IRequestHandler`, а `DomainEvent : INotification` тянет MediatR даже в Domain. MediatR — несущая конструкция всего слоя Application, а не только диспетчер доменных событий. Правится в §11.
- Локально стоят SDK 8.0.404 / 9.0.101 / **10.0.100** — верификация возможна без доустановки.
- `Crnc.Oms.Sales/Dockerfile` — точная копия Security'шного до миграции, включая `dotnet restore` и `dotnet publish -c Release -o out` без явного таргета (см. §9.2). `.dockerignore` отсутствует.
- `docker-compose.yml`: `sales-api` — `ports: "8091:80"`, `sales-db` — `postgres:9.6.17`. `prometheus/prometheus.yml` — таргет `sales-api` без порта.

---

## 1. Версии пакетов (проверено на nuget.org 2026-08-19)

| Пакет | Проект(ы) | Было | Станет |
|---|---|---|---|
| `MediatR` | Domain, Application | 8.0.0 | **12.5.0** (последняя Apache-2.0) |
| `MediatR.Extensions.Microsoft.DependencyInjection` | WebApi | 8.0.0 | **удалить** — в MediatR 12 DI-расширения встроены в основной пакет |
| `Microsoft.EntityFrameworkCore` | DataAccess, WebApi | 3.1.0 / 3.1.1 | **10.0.11** |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | DataAccess, WebApi | 3.1.0 / 3.1.1.2 | **10.0.3** |
| `EFCore.NamingConventions` | DataAccess | 1.0.0 | **удалить** — не используется |
| `Microsoft.EntityFrameworkCore.Proxies` | DataAccess | 3.1.0 | **удалить** — не используется |
| `MassTransit` | Integration | 6.1.0 | **8.5.10** |
| `MassTransit.RabbitMQ` | Integration, WebApi | 6.1.0 | **8.5.10** |
| `MassTransit.AspNetCore` | WebApi | 6.1.0 | **удалить** — в 8.x хостинг встроен в `AddMassTransit` |
| `MassTransit.Extensions.DependencyInjection` | WebApi | 6.1.0 | **удалить** — влит в основной пакет |
| `MassTransit.Extensions.Logging` | WebApi | 5.5.6 | **удалить** — логирование через `Microsoft.Extensions.Logging` из коробки |
| `RestSharp` | Integration | 106.10.1 | **удалить** — замена на `HttpClient` (§7) |
| `Microsoft.Extensions.Logging.Abstractions` | Integration | 3.1.2 | **10.0.11** |
| `Microsoft.Extensions.Options` | Integration | 3.1.0 | **10.0.11** |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | WebApi | 3.1.1 | **10.0.11** |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | WebApi | 3.1.1 | **удалить** (переход на STJ) |
| `Microsoft.AspNetCore.Diagnostics` | WebApi | 2.2.0 | **удалить** — часть shared framework `Microsoft.AspNetCore.App` |
| `Microsoft.AspNetCore.Diagnostics.HealthChecks` | WebApi | 2.2.0 | **удалить** — то же |
| `Microsoft.IdentityModel.Tokens` | WebApi | 5.6.0 | **удалить** — придёт транзитивно с `JwtBearer 10.0.11` (8.22.0) |
| `NSwag.AspNetCore` | WebApi | 13.2.3 | **14.7.1** — breaking rename `UseSwaggerUi3()` → `UseSwaggerUi()` |
| `prometheus-net.AspNetCore` | WebApi | 3.5.0 | **8.2.1** |

Итого в WebApi остаётся 6 `PackageReference` вместо 14.

---

## 2. Изменения в csproj

`<TargetFramework>` → `net10.0` в пяти проектах (Domain, Application, DataAccess, Integration, WebApi). `Messaging.Contract` остаётся `netstandard2.0` (решение 9). Добавить `<ImplicitUsings>enable</ImplicitUsings>` — аддитивно, риск низкий. `<Nullable>` не включаем.

`<GenerateDocumentationFile>true</GenerateDocumentationFile>` + `<NoWarn>$(NoWarn);1591</NoWarn>` в Application и WebApi оставить как есть — на них держится наполнение Swagger из XML-комментариев контроллера.

`Crnc.Oms.Sales.Tests.csproj` в этой миграции **тоже надо перевести** на `net10.0` (он ссылается на `Domain` через `ProjectReference`, поэтому при смене TFM домена перестанет собираться) и обновить в нём `xunit 2.4.0 → 2.9.3`, `xunit.runner.visualstudio 2.4.0 → 3.1.4`, `Microsoft.NET.Test.Sdk 16.2.0 → 17.14.1`, `FluentAssertions 5.10.2 → 7.2.2`, `coverlet.collector 1.0.1 → 6.0.4` — то есть выровнять с `Crnc.Oms.Security.E2ETests`. Внимание: в `FluentAssertions` 6 изменилось поведение `Should().Be()` для некоторых типов и переехали неймспейсы; единственный существующий тест (`OrderTests.ComposeOrderNumber_WithGuidId_ExpectedResult`, сравнение строк) это не задевает. Будущий `Crnc.Oms.Sales.E2ETests` уже родится на `net10.0` и правок не потребует.

---

## 3. Program.cs (слияние Startup.cs → minimal hosting)

Удалить `Crnc.Oms.Sales.WebApi/Startup.cs`, переписать `Program.cs`. Поведение сохраняется 1:1, кроме явно оговорённых пунктов. Убрать, как и в Security, избыточный `ConfigureAppConfiguration` (дублирует дефолт `WebApplication.CreateBuilder`) и no-op `ConfigureKestrel`.

```csharp
using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Messaging.Contract.Commands;
using Crnc.Oms.Sales.Application;
using Crnc.Oms.Sales.DataAccess;
using Crnc.Oms.Sales.DataAccess.Repositories;
using Crnc.Oms.Sales.Domain.Gateways;
using Crnc.Oms.Sales.Domain.Repositories;
using Crnc.Oms.Sales.Domain.SeedWork;
using Crnc.Oms.Sales.Integration.Gateways;
using Crnc.Oms.Sales.Integration.Settings;
using Crnc.Oms.Sales.WebApi.Authorization;
using Crnc.Oms.Sales.WebApi.Consumers;
using Crnc.Oms.Sales.WebApi.Middlewares;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using Prometheus;

// Риск 1: сохраняем поведение Npgsql до 6.0 - DateTime с Kind=Local/Unspecified
// пишутся в timestamp without time zone. Весь домен оперирует DateTime.Now.
// Ставить строго до создания любого NpgsqlDataSource/DbContext.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllOrigins", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    // JsonStringEnumConverter НЕ добавляем - см. §4: enum'ы в контрактах Sales
    // сейчас числовые, SPA ожидает числа.
});

builder.Services.AddDbContext<SalesDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OmsSalesDb")));

builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<JobCreatedForOrderConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        cfg.ReceiveEndpoint("jobCreatedForOrder", e =>
        {
            e.ConfigureConsumer<JobCreatedForOrderConsumer>(context);
        });

        EndpointConvention.Map<SendNotificationToUserCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendNotificationToUser"));
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(IDomainEventNotificationHandler).Assembly));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// §7: EmployeeSecurityGateway переехал с RestSharp на типизированный HttpClient.
builder.Services.AddHttpClient<IEmployeeGateway, EmployeeSecurityGateway>(client =>
    client.BaseAddress = new Uri(integrationSettings.SecurityServiceEndpoint));

builder.Services.AddScoped<INotificationGateway, MessageBrokerNotificationGateway>();
builder.Services.AddScoped<IProductionJobGateway, ProductionJobGateway>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();
builder.Services.AddScoped<ICommandQueryDispatcher, CommandQueryDispatcher>();

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));

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
    options.Title = "Crnc Oms Sales API Doc";
    options.Version = "1.0";
    options.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "Please insert JWT with Bearer into field"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseMonitoringRequestMiddleware();

app.UseRouting();
app.UseHttpMetrics();
app.UseCors("AllOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.UseOpenApi();
app.UseSwaggerUi(); // переименован из UseSwaggerUi3() в NSwag v14

// Существующее поведение вне скоупа миграции: полное пересоздание схемы и сида
// при каждом старте. Раньше SalesDataContext инжектился параметром Configure(),
// теперь достаём из scope явно.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SalesDataContext>();
    SalesDbInitializer.Initialize(dbContext);
}

app.Run();
```

Два содержательных отличия от старого кода, оба намеренные:

1. `app.UseHealthChecks("/health", …)` заменён на `app.MapHealthChecks("/health", …)` с тем же `Predicate`. Поведение то же (эндпоинт и сейчас отвечает — в отличие от Security, где `MapHealthChecks` вообще забыли). Важно: `Predicate = check => check.Tags.Contains("ready")`, а зарегистрированных проверок с тегом `ready` нет ни одной, поэтому `/health` возвращает `Healthy` всегда — это ровно текущее поведение, менять его здесь не надо.
2. `SalesDataContext` больше не приходит параметром метода — в minimal hosting его берём из явного scope. `AddDbContext` регистрирует scoped-сервис, поэтому `app.Services.GetRequiredService` напрямую бросил бы исключение.

---

## 4. Переход на System.Text.Json

Как и в Security, единственная точка изменений — блок сериализации в `Program.cs`; файлов с Newtonsoft-атрибутами нет.

Причины настроек:

- `PropertyNamingPolicy = CamelCase` — эквивалент `CamelCasePropertyNamesContractResolver`. Обязателен: SPA читает `response.data.items`, `dateCreated`, `jobType` и т.д.
- `DictionaryKeyPolicy = CamelCase` — `PropertyNamingPolicy` в STJ не распространяется на ключи словаря, а `CamelCasePropertyNamesContractResolver` их переименовывал (`ProcessDictionaryKeys = true`). Без этого ключи `ModelState` в автоматическом 400 регрессируют в PascalCase. SPA их сейчас не читает, так что это вопрос паритета API, а не поломки фронта — но регрессия молчаливая, поэтому её ловит отдельный e2e-тест.
- `PropertyNameCaseInsensitive = true` — Newtonsoft десериализует регистронезависимо, STJ по умолчанию нет; сохраняет поведение биндинга `[FromBody]`.
- **`JsonStringEnumConverter` не добавляем** — в отличие от Security. В Sales enum'ы в контрактах живые (`JobType`, `OrderStatusEnum`, `MaterialSource?`, `SignoffType?`), сейчас сериализуются числами, и SPA присылает/ожидает числа. Добавление конвертера сломает и создание, и редактирование заказа.

Проверено, что внимания не требует: `DateTime` в выходных DTO нет вовсе — даты форматируются вручную в `string` через `DateTimeExtensions.ToStandartFormat*`, так что различие форматов дат Newtonsoft/STJ на API не влияет (на БД — влияет, см. §5). `decimal` в контрактах нет. `Guid` и `Guid?` форматируются одинаково. `null` по умолчанию сериализуют оба.

Отдельно: `TextValueOutputDto<int,string>` — дженерик-DTO, для STJ проблемы не представляет; проверить только, что NSwag 14 генерирует для него ту же схему (шаг ручной проверки).

---

## 5. EF Core 3.1 → 10 и Npgsql

Главный раздел этой миграции. Два независимых источника проблем: провайдер Npgsql и сам EF Core.

### 5.1. Риск №1 — маппинг `DateTime` (блокирующий, рантайм)

Начиная с **Npgsql 6.0** `DateTime`-свойства маппятся на `timestamp with time zone` (`timestamptz`) вместо `timestamp`, и запись `DateTime` с `Kind=Local` или `Unspecified` в такое поле **бросает исключение**. Весь Sales оперирует `DateTime.Now`/`DateTime.Today` (`Kind=Local`) — см. инвентаризацию.

Падать будет не на каком-то редком сценарии, а на старте: `SalesDbInitializer.Initialize` вызывает `SaveChanges()` с `DateTime.Now` в `Order.DateCreated`/`StatusDate` до приёма трафика. То есть сервис просто не поднимется.

Три варианта, выбран первый:

1. **`AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`** в самом начале `Program.cs` (уже в черновике §3). Одна строка, полный паритет поведения: колонки остаются `timestamp without time zone`, значения — локальное время, формат вывода в SPA не меняется. Для миграции, цель которой — не менять наблюдаемое поведение, это правильный размен.
2. Явно проставить `HasColumnType("timestamp without time zone")` трём свойствам `Order` в `OrderMappingConfiguration`. Честнее и локальнее, но забудется при добавлении четвёртого поля.
3. Перевести домен на UTC (`DateTime.UtcNow`) и `timestamptz`. Правильная цель, но это смена поведения (отображаемые даты уедут на часовой пояс), домена и `DateTimeExtensions` — отдельная задача **после** миграции, не внутри неё.

### 5.2. Owned types и валидация модели

Маппинг `Order` — три уровня вложенных `OwnsOne`: `Customer` → `Title` → `NameAbbreviation`, `Customer` → `ContactPerson` → `Email`/`Phone`/`FullName`, плюс `OwnsOne(x => x.Status)` для `OrderStatus : Enumeration`. Начиная с EF Core 6 такая форма (optional dependent при table-sharing без обязательного «своего» свойства) даёт предупреждение `OptionalDependentWithoutIdentifyingPropertyWarning`, а поведение «все свойства null → навигация читается как null» стало строже.

Практических следствий два, оба проверяются первым же запуском:

- при построении модели могут посыпаться предупреждения (не ошибки) — их надо прочитать, а не заглушить;
- если у существующего заказа, скажем, `Customer.ContactPerson.Phone` не заполнен, объект `Phone` может прийти как `null` там, где раньше приходил экземпляр с `Value = null`. В сиде и в `OrderMapper` все поля заполняются, так что практически это не должно проявиться — но e2e-тест на `GET /api/orders/{id}` для созданного через API заказа это закрывает.

**Смягчающее обстоятельство, важное для всей секции**: `SalesDbInitializer` делает `EnsureDeleted()` + `EnsureCreated()` на каждом старте. Значит любые изменения в дефолтных именах колонок/таблиц owned-типов между EF Core 3.1 и 10 нам безразличны — схема каждый раз создаётся заново из текущей модели. Совместимость со старой БД проверять не нужно; ни `Migrations`, ни `__EFMigrationsHistory` в проекте нет.

### 5.3. Прочее по EF Core

- `SalesDataContext.OnConfiguring` содержит только `base.OnConfiguring` и закомментированную строку — можно удалить метод целиком вместе с пакетом `EFCore.NamingConventions`.
- `Repository<TEntity>` использует `Set<TEntity>()`, `ToListAsync`, `SingleOrDefaultAsync`, `ChangeTracker.Entries<T>()` — всё без изменений в EF Core 10.
- `OrderRepository.FindByIdAsync` использует `.Include(x => x.Manager)` — без изменений.
- Запросов с клиентской оценкой (`AsEnumerable` в середине LINQ, вызовы .NET-методов внутри `Where`) в DataAccess нет; в EF Core 3.0 клиентская оценка уже стала ошибкой, так что этот класс проблем был бы виден и сейчас.
- `Microsoft.EntityFrameworkCore.Proxies` удаляется (см. §1) — `UseLazyLoadingProxies` не вызывается, `virtual`-навигаций нет.

### 5.4. Версия PostgreSQL

`postgres:9.6.17` → `postgres:18.6` в `docker-compose.yml` (и тот же тег в фикстуре e2e-тестов — держать синхронно, как в Security с Mongo). Npgsql тестируется на PostgreSQL «5 лет назад» от актуальных; 9.6 EOL с ноября 2021. Данные не мигрируем — БД пересоздаётся на каждом старте.

Заодно: `Crnc.Oms.Production.WebApi/appsettings.json` содержит `Port=5433` вместо `5434` (известная опечатка, зафиксирована в AGENTS.md). Не трогаем — это про Production.

---

## 6. MassTransit 6.1 → 8.5

### 6.1. Конфигурация

MassTransit 8 полностью переехал на единый `AddMassTransit(x => …)`; старая связка «фабрика `IBusControl` + `IServiceCollectionConfigurator`» из 6.x исчезла. Что удаляется из кода:

- `using GreenPipes;` — пакет влит в MassTransit;
- `using MassTransit.AspNetCoreIntegration;`, `using MassTransit.ExtensionsDependencyInjectionIntegration;` — этих неймспейсов больше нет;
- `using MassTransit.Conductor.Server;` в `MessageBrokerNotificationGateway.cs` (и так не использовался);
- локальные `CreateBus`/`ConfigureMassTransit` — заменяются на форму из §3.

Что остаётся без изменений: `IConsumer<T>`/`ConsumeContext<T>` (`JobCreatedForOrderConsumer`), `ISendEndpointProvider.Send<T>`, `IPublishEndpoint.Publish<T>`, `EndpointConvention.Map<T>(uri)`, интерфейсные контракты сообщений (MT 8 по-прежнему генерирует прокси для интерфейсов), имя очереди `jobCreatedForOrder`. Хостинг шины теперь поднимается самим `AddMassTransit` — отдельный `MassTransit.AspNetCore` не нужен.

### 6.2. Риск №2 — сериализатор и совместимость с Production/Notification

**MassTransit 8 сменил сериализатор по умолчанию с Newtonsoft.Json на System.Text.Json**; Newtonsoft вынесен в отдельный пакет `MassTransit.Newtonsoft` и включается вызовом `UseNewtonsoftJsonSerializer()`.

Sales после миграции разговаривает с сервисами, которые остаются на **MassTransit 6.2.4 с Newtonsoft**:

| Направление | Контракт | Кто на другом конце |
|---|---|---|
| Sales публикует | `OrderConvertedToJobEvent` | Production (`OrderConvertedToJobConsumer`, MT 6.2.4) |
| Sales потребляет | `JobCreatedForOrderEvent` | Production публикует |
| Sales отправляет | `SendNotificationToUserCommand` | Notification.Gateway (MT 6.x) |

Формат конверта MassTransit (`messageType` в виде `urn:message:…`, `message` с телом) между 6 и 8 не менялся, и контракты здесь простые — `Guid` и `string`, без дат, коллекций и полиморфизма. То есть по всем признакам совместимость есть. Но это **единственный узел, который нельзя проверить сборкой или юнит-тестом**, поэтому:

- обязательный шаг проверки — полный round-trip на живом `docker-compose up`: перевести заказ в `ConvertedToJob` через SPA/Swagger, увидеть созданную работу в Production API и проставленный `jobId` обратно в заказе;
- если round-trip не проходит — escape hatch: добавить `MassTransit.Newtonsoft` и `cfg.UseNewtonsoftJsonSerializer()` в конфигурацию Sales, что возвращает ровно прежний формат. Это временная мера до миграции Production.

E2E-набор эту проблему **не поймает по построению**: его периметр заканчивается на «сообщение легло в очередь», а несовместимость проявляется на стороне читателя. Ловится только ручным round-trip с реальным Production (шаг 14).

### 6.3. RabbitMQ

`MassTransit.RabbitMQ 8.5.10` тянет `RabbitMQ.Client 7.2.1`. Сервер в compose — `rabbitmq:3-management`, протокол AMQP 0-9-1, клиент 7.x с ним работает. Менять образ в рамках этой миграции не нужно (в отличие от Mongo в Security, где драйвер жёстко требовал wire version). Если при первом запуске появятся ошибки подключения — это первое, на что смотреть.

---

## 7. RestSharp → HttpClient

Два использования, из них одно мёртвое:

- `HttpNotificationGateway` — **удалить файл**. В DI зарегистрирован `MessageBrokerNotificationGateway`, HTTP-вариант не используется нигде.
- `EmployeeSecurityGateway.GetUsersByRolesAsync` — единственный живой вызов: `GET {Security}/api/users?roles=Main manager` с `Bearer`-токеном текущего пользователя.

Переписывать всё равно придётся: между RestSharp 106 и 112+ сломано практически всё используемое API — `new RestClient(url) { Authenticator = … }` → `RestClientOptions`, `new RestRequest(resource, DataFormat.Json)` → конструктор без `DataFormat`, `Method.GET` → `Method.Get`, `ParameterType.GetOrPost` → `AddQueryParameter`, `IRestResponse` → `RestResponse`, плюс сменился сериализатор по умолчанию. Раз объём правок тот же, лучше убрать зависимость.

Замена — типизированный `HttpClient` (регистрация в §3, `AddHttpClient<IEmployeeGateway, EmployeeSecurityGateway>`), токен ставится per-request из `ICurrentUserContext`:

```csharp
public EmployeeSecurityGateway(HttpClient client, ILogger<EmployeeSecurityGateway> logger,
    ICurrentUserContext currentUserContext)
{
    _client = client;
    _logger = logger;
    _currentUserContext = currentUserContext;
}

private async Task<List<UserItemDto>> GetUsersByRolesAsync(List<string> roles,
    CancellationToken cancellationToken = default)
{
    var query = string.Join("&", roles.Select(r => $"roles={Uri.EscapeDataString(r)}"));
    using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users?{query}");
    request.Headers.Authorization =
        new AuthenticationHeaderValue("Bearer", _currentUserContext.AuthToken);

    var response = await _client.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
        throw new Exception(
            $"Error retrieving response from Security Api. Status code is {response.StatusCode}");

    return await response.Content.ReadFromJsonAsync<List<UserItemDto>>(
        JsonDefaults.Options, cancellationToken);
}
```

Существенные детали:

- **`BaseAddress` требует завершающего слэша, а относительный путь — его отсутствия.** `SecurityServiceEndpoint` в конфиге — `http://security-api:8080` без слэша; `new Uri(base, "/api/users")` при абсолютном пути от корня отработает верно, но при склейке через `HttpClient.BaseAddress` правило другое. Безопаснее задавать `BaseAddress` с завершающим `/` и путь без ведущего `/`. Проверяется e2e-тестом на смену статуса заказа.
- Десериализация — `JsonSerializerOptions(JsonSerializerDefaults.Web)` (регистронезависимо), Security отдаёт camelCase, `UserItemDto` — PascalCase-свойства.
- В сообщении об ошибке в текущем коде опечатка — `"Error retrieving response from Sales Api"` в гейтвее, который ходит в Security. Заодно поправить.
- `EmployeeSecurityGateway` регистрируется через `AddHttpClient`, то есть перестаёт быть просто `AddScoped` — это учтено в §3.

---

## 8. MediatR 8 → 12.5

Малый по объёму, но затрагивает три проекта.

Что меняется:

- `services.AddMediatR(typeof(IDomainEventNotificationHandler).Assembly)` → `services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(…))` (единственная правка в коде);
- пакет `MediatR.Extensions.Microsoft.DependencyInjection` удаляется — в 12.x DI-расширения внутри `MediatR`.

Что не меняется: `IRequest<T>`, `IRequestHandler<TIn,TOut>`, `INotification`, `INotificationHandler<T>`, `IMediator.Send/Publish`. То есть `IUseCaseCommand`/`IUseCaseQuery`/`IUseCaseCommandHandler`/`IUseCaseQueryHandler`, `DomainEvent : INotification`, `CommandQueryDispatcher`, `DomainEventDispatcher` и все хендлеры остаются как есть. Удалённый в 12.x `ServiceFactory` здесь не использовался.

Заметка на будущее (не в этой миграции): MediatR используется ради `Send`/`Publish` по типу, и при желании его можно вычистить полностью — тогда `Crnc.Oms.Sales.Domain` перестанет зависеть от внешнего пакета (сейчас `DomainEvent : INotification` тянет MediatR прямо в домен, вопреки заявленному в AGENTS.md «No framework dependencies»). Это снимет и лицензионный вопрос навсегда. Отдельная задача.

---

## 9. Dockerfile, сборка и compose

### 9.1. Базовые образы

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build       # было mcr.microsoft.com/dotnet/core/sdk:3.1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime  # было mcr.microsoft.com/dotnet/core/aspnet:3.1
```

(путь без сегмента `/core/` — старый ретайрен).

### 9.2. `dotnet publish -o` по solution — блокер `NETSDK1194`

Dockerfile Sales — копия дореформенного Security'шного: `RUN dotnet restore` и `RUN dotnet publish -c Release -o out` без явного таргета, то есть по `Crnc.Oms.Sales.sln`. С SDK 7.0.200 `--output` для solution — ошибка `NETSDK1194`, сборка образа на `sdk:10.0` упадёт. Заменить:

```dockerfile
RUN dotnet restore Crnc.Oms.Sales.WebApi/Crnc.Oms.Sales.WebApi.csproj
RUN dotnet publish Crnc.Oms.Sales.WebApi/Crnc.Oms.Sales.WebApi.csproj -c Release -o out
```

Побочный плюс тот же, что в Security: состав `.sln` перестаёт влиять на прод-образ, что делает безопасным §9.4.

### 9.3. `.dockerignore` (новый файл)

Нужен `src/Server/src/Crnc.Oms.Sales/.dockerignore` — сейчас его нет, а `COPY . ./aspnetapp` тащит `bin/`/`obj/` всех проектов. С появлением `Crnc.Oms.Sales.E2ETests` внутри контекста это станет дороже: фикстура пересобирает образ из этого же каталога, её собственный `bin/` меняется на каждом прогоне и инвалидирует слой `COPY` каждый раз.

```
Crnc.Oms.Sales.E2ETests/
**/bin/
**/obj/
```

### 9.4. `.sln`

`Crnc.Oms.Sales.sln` сейчас содержит только 5 проектов — ни `Crnc.Oms.Sales.Tests`, ни будущего `Crnc.Oms.Sales.E2ETests` в нём нет. Добавить оба, **строго после 9.2 и 9.3** — пока Dockerfile делает restore по solution на `sdk:3.1`, появление в ней `net10.0`-проекта немедленно ломает сборку образа (`NETSDK1045`), а с ней и сами e2e-тесты.

### 9.5. Риск №3 — порт 8080 в `aspnet:10.0`

Образ `mcr.microsoft.com/dotnet/aspnet:10.0` слушает 8080 (`ASPNETCORE_HTTP_PORTS=8080` зашит в образ), а не 80, как `dotnet/core/aspnet:3.1`. Проявляется не ошибкой, а «зависшей» проверкой готовности контейнера. Решение то же, что приняли для Security, — принимаем дефолт образа и правим тех, кто ходит на Sales:

- `docker-compose.yml`: `sales-api.ports` → `"8091:8080"` (внешний порт 8091 не меняется, значит README/AGENTS.md/SPA править не надо — `SALES_API_URL` указывает на `http://localhost:8091/api`);
- `prometheus/prometheus.yml`: таргет `sales-api` → `sales-api:8080`;
- фикстура e2e-тестов: `ApiContainerPort = 8080`.

Внутри Docker-сети по имени `sales-api` никто не ходит — проверено по `docker-compose.yml` (Production и Notification общаются с Sales только через RabbitMQ), так что больше править нечего.

### 9.6. compose — прочее

- `sales-db`: `postgres:9.6.17` → `postgres:18.6` (§5.4).
- Профили (`profiles:`) и `depends_on` не меняются.
- Переменные `ConnectionStrings:OmsSalesDb` и `IntegrationEndpoints:*` синтаксически от TFM не зависят.
- `Auth:JwtBase64SymmetricKey` в compose не задан — ключ берётся из `appsettings.json`, см. блокер №0.

---

## 10. Порядок выполнения

**Фаза 1 — e2e-тесты на текущем 3.1 (текущая ветка):**

1. Создать `Crnc.Oms.Sales.E2ETests` по конвенциям Security и периметру из раздела «Пререквизит»: настоящий PostgreSQL, WireMock вместо Security, RabbitMQ с проверкой только факта отправки. Тесты пишутся и доводятся до зелёного **на текущем `netcoreapp3.1`-сервисе** — это baseline миграции. От блокера №0 не зависят: токен фикстура подписывает сама, ключ задаёт переменной окружения.

**Фаза 0 — починить `master` (внутри миграции, до смены TFM):**

2. Выровнять `Auth:JwtBase64SymmetricKey` в Sales, Production и трёх Notification-сервисах по значению из Security (блокер №0). Отдельный коммит. Проверка ручная: `docker-compose up`, логин в SPA, открыть список заказов — не должно быть 401. E2E-набор на это не отреагирует (и не должен).

**Фаза 2 — инфраструктура сборки (до смены TFM, работает на 3.1):**

3. Dockerfile: restore/publish по явному csproj (§9.2) + `.dockerignore` (§9.3). Прогнать e2e — образ должен собираться как раньше.
4. Добавить `Crnc.Oms.Sales.Tests` и `Crnc.Oms.Sales.E2ETests` в `Crnc.Oms.Sales.sln` (§9.4). Снова прогнать e2e.

**Фаза 3 — миграция:**

5. TFM → `net10.0` в пяти проектах + `Crnc.Oms.Sales.Tests`, версии пакетов и удаления по §1–§2. `Messaging.Contract` не трогать.
6. `Startup.cs` → `Program.cs` (§3), удалить `Startup.cs`. Сюда же входят: STJ вместо `AddNewtonsoftJson` (§4, отдельно сверить `DictionaryKeyPolicy` и отсутствие `JsonStringEnumConverter`), новая конфигурация MassTransit (§6.1), `AddMediatR` с `RegisterServicesFromAssembly` (§8), `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` первой строкой (§5.1), scope для `SalesDbInitializer`.
7. Удалить `HttpNotificationGateway.cs`, переписать `EmployeeSecurityGateway` на `HttpClient` (§7).
8. Убрать `OnConfiguring` из `SalesDataContext` (§5.3), лишние `using` (`GreenPipes`, `MassTransit.Conductor.Server`, `Newtonsoft.*`).
9. `dotnet build Crnc.Oms.Sales.sln` — до чистой сборки. Ожидаемые точки поломки: неймспейсы MassTransit, `UseSwaggerUi3()`, `AddMediatR`, RestSharp-типы, `Configure(…, SalesDataContext)`.
10. `dotnet test Crnc.Oms.Sales.Tests` — юниты Domain должны остаться зелёными (проверка, что FluentAssertions 7 ничего не сломал).
11. Базовые образы в Dockerfile → `sdk:10.0`/`aspnet:10.0` (§9.1).
12. `docker-compose.yml`: `sales-api.ports` → `"8091:8080"`, `sales-db` → `postgres:18.6`; `prometheus.yml` → `sales-api:8080`; тот же тег Postgres и порт 8080 в фикстуре e2e (§9.5, §9.6).

**Фаза 4 — проверка:**

13. Прогнать e2e-набор — основная автоматическая проверка. Покрывает §4 (camelCase, числовые enum'ы), §5.1 (даты), §5.2 (owned types), §6.1 (публикация в шину), §7 (поход в Security и склейка URL) и сам факт сборки образа на новом SDK. Не покрывает потребление сообщений и совместимость шины с Production — это шаг 14.
14. **Ручной round-trip через `docker-compose up` — обязателен**, e2e его не заменяет (§6.2): логин в SPA, создание заказа, смена статуса → push-уведомление в `notification-push-client`, конвертация в работу → работа появилась в Production API (`http://localhost:8098/swagger`) → `jobId` вернулся в заказ. Это единственная проверка совместимости MassTransit 8 ↔ MassTransit 6.
15. Swagger UI (`http://localhost:8091/swagger`) — визуально сверить схемы DTO после NSwag 14, особенно `TextValueOutputDto<int,string>` и nullable-enum'ы.
16. Grafana/Prometheus — убедиться, что таргет `sales_api_monitoring` снова `UP` после смены порта.
17. Обновить AGENTS.md (§11).

**Изменений в SPA не требуется**: маршруты и контракты не меняются, enum'ы остаются числовыми, даты по-прежнему приходят строками в `dd.MM.yyyy`, внешний порт 8091 сохраняется. Подтверждается шагами 13–15, а не предполагается.

---

## 11. Обновление AGENTS.md

Уже неверно сейчас (можно править в любой момент):

- «Commands/queries do **not** use MediatR directly — they go through a custom `ICommandQueryDispatcher`…» — вводит в заблуждение: `CommandQueryDispatcher` это обёртка над `IMediator.Send`, а `IUseCaseCommand`/`IUseCaseCommandHandler` наследуют `IRequest`/`IRequestHandler`. Переформулировать: собственные интерфейсы задают форму use-case'ов, а исполняются они MediatR'ом; MediatR при этом присутствует и в Domain через `DomainEvent : INotification`.
- «`.Domain` — … No framework dependencies» — для Sales неверно, там `PackageReference` на MediatR.
- «`Authorization/` has role-based policy handlers» — в Sales их нет, только `AuthSettings` и `CurrentUserContext`.
- Таблица БД: `sales-db` — PostgreSQL 9.6.17 (после миграции → 18.6).

Станет неверным после миграции:

- Заголовок «### Backend (mixed: Security on .NET 10, others on .NET Core 3.1)» — добавить Sales к мигрированным.
- «`.WebApi` — ASP.NET Core host: `Startup.cs` wires DI…» — оговорить, что Sales, как и Security, перешёл на minimal hosting.
- В Commands добавить запуск e2e-тестов Sales и ту же Windows-готчу с `DOCKER_HOST=tcp://localhost:2375`.
- Обновить состояние тестов: Sales — юниты Domain + e2e; Security — e2e; Production и Notification.* — пока без тестов.

---

## Риски и как их ловить

| # | Риск | Как ловить |
|---|---|---|
| 0 | **Рассинхрон JWT-ключа между Security и остальными сервисами (уже в `master`)** — Sales отвергает все токены, 401 на любой запрос | **E2E-тестами не ловится намеренно** — фикстура подписывает токен сама и задаёт ключ через переменную окружения, чтобы тесты не зависели от ротации ключа. Виден только при логине в SPA. Фикс — фаза 0 |
| 1 | **Npgsql 6+ маппит `DateTime` на `timestamptz` и запрещает запись `Kind=Local`** — сервис падает на старте в `SalesDbInitializer.Initialize` | Любой запуск. Фикс — `EnableLegacyTimestampBehavior` первой строкой `Program.cs` (§5.1); e2e-тесты на даты подтверждают формат вывода |
| 2 | **MassTransit 8 (STJ) ↔ MassTransit 6 (Newtonsoft) в Production/Notification** — конвертация заказа в работу молча перестаёт доходить | **E2E-тестами не ловится.** Только ручной round-trip шага 14. Escape hatch — `MassTransit.Newtonsoft` + `UseNewtonsoftJsonSerializer()` |
| 3 | **`aspnet:10.0` слушает 8080, а не 80** — выглядит как «зависший» контейнер, а не ошибка | Проверенный по опыту Security сценарий: `docker exec <c> env \| grep ASPNETCORE_HTTP_PORTS`. Фикс — §9.5 |
| 4 | `dotnet publish -o` по solution → `NETSDK1194` | Гарантированно ловится любой сборкой образа. Фикс — §9.2, до смены базовых образов |
| 5 | Забытый `DictionaryKeyPolicy` → молчаливая регрессия camelCase→PascalCase в ключах ошибок валидации | Отдельный e2e-тест; smoke-тестом не ловится. SPA их не читает, поэтому вручную это не всплывёт вообще |
| 6 | **Случайно добавленный `JsonStringEnumConverter`** (по аналогии с планом Security) ломает создание/редактирование заказа | E2E-тест «enum'ы приходят числами». Отдельно отмечено в §4, чтобы не скопировали из плана Security |
| 7 | Валидация модели EF Core 10 на трёхуровневых `OwnsOne` (optional dependents) | Первый же старт сервиса + `GET /api/orders/{id}` в e2e. Смягчено тем, что схема пересоздаётся при каждом старте (§5.2) |
| 8 | PostgreSQL 9.6 вне окна поддержки Npgsql | Проявляется как странные SQL-ошибки на ровном месте. Профилактика — `postgres:18.6` (§5.4); данные не мигрируем |
| 9 | `UseSwaggerUi3()` переименован в NSwag 14; `Configure(…, SalesDataContext)` в minimal hosting так не работает | Ловится компилятором мгновенно |
| 10 | `HttpClient.BaseAddress` + относительный путь склеиваются не так, как у RestSharp | E2E-тест на смену статуса заказа + сверка `GET /__admin/requests` у WireMock: Sales должен реально сходить на `/api/users`. Детали — §7 |
| 11 | FluentAssertions 5 → 7 в `Crnc.Oms.Sales.Tests` | `dotnet test` шага 10; существующий тест сравнивает строки, задеть не должно |
| 12 | NSwag 14 может иначе сгенерировать схему для дженерик-DTO и nullable-enum'ов | Тестами не покрыто — визуальная сверка Swagger UI, шаг 15 |

---

## Критичные файлы

- `Crnc.Oms.Sales.WebApi/Startup.cs` (удалить) и `Program.cs` (переписать, §3)
- 5 мигрируемых `.csproj` + `Crnc.Oms.Sales.Tests.csproj` (§2); `Crnc.Oms.Sales.Messaging.Contract.csproj` не трогаем
- `Crnc.Oms.Sales.DataAccess/SalesDataContext.cs` — удалить `OnConfiguring` (§5.3)
- `Crnc.Oms.Sales.DataAccess/Mappings/OrderMappingConfiguration.cs` — цель проверки owned types (§5.2)
- `Crnc.Oms.Sales.Integration/Gateways/EmployeeSecurityGateway.cs` — переписать на `HttpClient` (§7)
- `Crnc.Oms.Sales.Integration/Gateways/HttpNotificationGateway.cs` — **удалить** (мёртвый код)
- `Crnc.Oms.Sales.Integration/Gateways/MessageBrokerNotificationGateway.cs` — убрать `using MassTransit.Conductor.Server`
- `Crnc.Oms.Sales/Dockerfile` (§9.1, §9.2) и новый `Crnc.Oms.Sales/.dockerignore` (§9.3)
- `Crnc.Oms.Sales.sln` (§9.4)
- `Crnc.Oms.Sales.WebApi/appsettings.json` — JWT-ключ (блокер №0)
- `Crnc.Oms.Production.WebApi/appsettings.json`, `Crnc.Oms.Notification.{Gateway,Email,Push}.WebApi/appsettings.json` — тот же ключ (блокер №0)
- `docker-compose.yml` — `sales-api.ports` → `"8091:8080"`, `sales-db` → `postgres:18.6`
- `prometheus/prometheus.yml` — таргет `sales-api` → `sales-api:8080`
- `AGENTS.md` (§11)

## Статус

**Фаза 1 выполнена.** `Crnc.Oms.Sales.E2ETests` — 19 тестов, все зелёные на текущем `netcoreapp3.1`-сервисе; это baseline миграции. Заодно, вне очереди, добавлен `.dockerignore` (§9.3) — без него фикстура тянет в контекст сборки `bin/` тестового проекта и инвалидирует слой `COPY` на каждом прогоне.

Что подтвердилось на живом прогоне и было в плане только предположением:

- Имена точек в брокере угаданы верно: очередь `sendNotificationToUser` и fanout-exchange `Crnc.Oms.Messaging.Contract.Events:OrderConvertedToJobEvent`. Оба messaging-теста ждут появления сообщения и получают его за 5–6 секунд.
- `[EnumRequired]` отвергает `null`, поэтому `materialSource` обязателен на любом PUT. Следствие: доменная проверка в `Order.ConvertToJob()` через API недостижима — запрос отсекается валидацией с 400. Зафиксировано тестом `EditOrder_WithoutMaterialSource_ReturnsBadRequest`.
- `EditOrderHandler` вызывает `ChangeStatus` безусловно, даже когда статус не менялся, поэтому каждый PUT ходит в Security и кладёт сообщение в очередь. Счётчики в тестах меряются дельтой.

**Фазы 0, 2, 3, 4 выполнены.** Сервис переведён на `net10.0`, `dotnet build`/`dotnet test Crnc.Oms.Sales.Tests` зелёные, образ собирается на `sdk:10.0`/`aspnet:10.0`, все 19 e2e-тестов зелёные на мигрированном сервисе, ручной round-trip через `docker-compose` (реальный Security/Production/Notification, без WireMock) успешно проверен по API.

Что было в плане неточным и потребовало правки по ходу:

- **§5.2 недооценивал последствия.** На практике EF Core 10 не просто предупреждает про owned-типы с несколькими вложенными dependents без обязательного свойства — она **падает с фатальным `InvalidOperationException` на старте** (`EnsureDeleted`/`Initialize`), а не просто пишет warning. Проблема каскадная: `ContactPerson` (3 вложенных dependents: Email/Phone/FullName) → после фикса всплывает `Customer` (владеет `Title` и `ContactPerson`) → после фикса всплывает `Title` (владеет `NameAbbreviation`). Все три навигации помечены обязательными доменом (конструкторы бросают на `null`), так что фикс безопасен и минимален — три строки `Navigation(...).IsRequired()` в `OrderMappingConfiguration` (для `Customer.Title`, `Customer.ContactPerson`, `Order.Customer`). Warning на листовых dependents без вложенных владений (`Email`, `Phone`, `FullName`, `NameAbbreviation`) остался — это уже безвредно, как и предполагал план.
- Отдельно нашёлся и починен несвязанный со сборкой Sales баг: `PhoneValueObjectAttribute.cs` содержал `using System.Runtime.InteropServices.WindowsRuntime;` — неиспользуемый, компилировался на `netcoreapp3.1`, но не существует на `net10.0`. Не было в инвентаризации плана, всплыло только на первой попытке `dotnet build`.
- **Риск №2 (MassTransit 8 ↔ 6) не подтвердился** — round-trip прошёл с первой попытки на дефолтном `System.Text.Json`-сериализаторе MassTransit 8, без escape hatch (`MassTransit.Newtonsoft`). Проверено по API: заказ переведён `NotSent → NeedSignoff → Signed → ConvertedToJob`, `OrderConvertedToJobEvent` дошёл до Production (MassTransit 6.2.4/Newtonsoft), джоба создана, `JobCreatedForOrderEvent` вернулся, `jobId`/`jobNumber` проставились на заказе.
- Блокер №0 (JWT-ключ) подтверждён устранённым тем же прогоном: логин через Security и последующие вызовы Sales/Production прошли без 401.
- Даты (риск №1, `EnableLegacyTimestampBehavior`) подтверждены на реальном `postgres:18.6` в compose, не только в e2e: `dateCreated`/`dateSentToCustomer` пришли в `dd.MM.yyyy HH:mm`, без исключений при записи.

Не проверено (осталось ручным по плану): визуальная сверка Swagger UI (шаг 15) и таргет `sales_api_monitoring` в Grafana/Prometheus (шаг 16) — оба требуют собранного полного стека с UI-образом, а его сборка сейчас падает на предсуществующей проблеме (`yarn: not found` в `crnc-oms-ui`), не связанной с этой миграцией.
