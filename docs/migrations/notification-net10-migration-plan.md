# Миграция Crnc.Oms.Notification на .NET 10

## Контекст

Четвёртый и последний шаг перевода CRNC OMS на современный стек. [Security](security-net10-migration-plan.md), [Sales](sales-net10-migration-plan.md) и [Production](production-net10-migration-plan.md) выполнены и служат шаблоном: e2e-тесты как страховка → правки инфраструктуры сборки → смена TFM и пакетов → minimal hosting → System.Text.Json → проверка тем же набором тестов.

Notification отличается от всех трёх предшественников не технологиями, а формой:

- **Это не один сервис, а четыре деплой-юнита в одном ограниченном контексте**: `Notification.Gateway`, `Notification.Email`, `Notification.Push` (по 4 проекта каждый) и консольный `Notification.Push.Client`. Четыре Dockerfile, четыре образа в `docker-compose.yml`, пять `.sln` (по одному на юнит плюс зонтичный `Crnc.Oms.Notification.sln` со всеми 13 проектами). Всё, что в прошлых планах делалось один раз, здесь делается три-четыре раза.
- **Нет ни БД, ни домена, ни CQRS.** Нет EF Core, нет Npgsql, нет Mongo, нет MediatR. Целых три раздела прошлых планов (EF Core/даты/owned-типы, версия PostgreSQL, MediatR) здесь отсутствуют — вместе с самым дорогим риском всех предыдущих миграций (`Npgsql.EnableLegacyTimestampBehavior`).
- **Единственный контекст с SignalR** — и единственный, у которого есть два живых клиента: SPA (`src/Client`, `@microsoft/signalr`) и консольный `Push.Client` в этом же репозитории.
- **Единственный, кто остался на MassTransit 6.2.1.** После этой миграции 6.x в репозитории не останется совсем.
- **Единственный контекст, где HTTP-порт ещё 80.** Переезд на 8080 (навязанный образом `aspnet:10.0`) задевает соседей: `prometheus.yml`, переменные окружения `sales-api` и `notification-push-client` в compose.
- **Enum'ов в контрактах нет вообще** (проверено `grep -rn "enum "` по всему контексту — ноль совпадений), хотя `StringEnumConverter` зарегистрирован во всех трёх `Startup.cs`. Значит риск «случайный `JsonStringEnumConverter` сломает числовые enum'ы» из планов Security/Production здесь неприменим по построению. Зато **атрибуты валидации есть** (`SendEmailMessageInputDto`: `[Required]`, `[EmailAddress]`), поэтому `DictionaryKeyPolicy` — не «ради паритета», как в Production, а ради реального наблюдаемого поведения.

Отдельно: у контекста есть **предсуществующий дефект в доставке письма** (§9) — консьюмер Email выбрасывает адресатов из команды. Миграцию он не блокирует, но e2e-набор фиксирует фактическое поведение, а не желаемое, — ровно так, как в Production фиксировались 500 вместо 404.

### Согласованные решения

1. **Целевая версия**: .NET 10 (LTS), как у Security, Sales и Production.
2. **Hosting model**: слить `Startup.cs` в `Program.cs` (minimal hosting) во всех трёх WebApi. `Push.Client` уже на `Host.CreateDefaultBuilder` — там достаточно смены TFM и пакетов.
3. **JSON**: полностью уйти с `Newtonsoft.Json` на `System.Text.Json`.
4. **MassTransit — ветка 8.x (8.5.10), не 9.x.** Обоснование лицензией — в §1 плана Sales: `8.5.10` — `Apache-2.0`, `9.2.0` — проприетарная. После этой миграции все четыре контекста окажутся на одной версии.
5. **RestSharp 106.6.10 → типизированный `HttpClient`**, как в Sales (§7 её плана). Пакет с известной уязвимостью высокой степени (NU1903, GHSA-9pq7-rcxv-47vq), а его API 106 → 112 сломан целиком (`Method.POST`, `DataFormat.Json`, `ExecuteTaskAsync` — ничего из этого в новых версиях нет). Живых вызовов всего два (§6).
6. **`Microsoft.AspNetCore.SignalR` 1.1.0 из `Push.Integration` — удалить**, заменив на `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (§7). Пакет версии 1.1.0 в проекте на 3.1 — это обход того, что серверный SignalR с 3.0 живёт в shared framework, а не в NuGet; на .NET 10 обход перестаёт быть безобидным.
7. **`<Nullable>` не включаем** — как в трёх предыдущих миграциях, отложено.
8. **Контракты сообщений: TFM выравниваем, содержимое не трогаем.** `Gateway.Messaging.Contract` уже `netstandard2.0`; `Email.Messaging.Contract` и `Push.Messaging.Contract` сейчас `netcoreapp3.1` — приводим к `netstandard2.0`. Это не смена поведения, а устранение расхождения: три парные копии одних и тех же интерфейсов должны собираться одинаково.
9. **Порядок работ — по юнитам, а не «сразу всё»**: Email (самый простой, нет исходящих вызовов) → Push (SignalR) → Gateway (RestSharp + `EndpointConvention`) → Push.Client. Каждый юнит — свой коммит, включающий его собственные Dockerfile, порт в compose и таргет в `prometheus.yml`. Так поломка всегда атрибутируется одному юниту.
10. **E2E-набор — один проект и одна фикстура на весь контекст** (`Crnc.Oms.Notification.E2ETests`), поднимающая все три сервиса разом. Отступление от правила «настоящая только своя БД» здесь мнимое: у Notification нет БД, а Email и Push — не «соседние сервисы», а тот же ограниченный контекст. Обоснование и периметр — в разделе «Пререквизит».
11. **Security в тестах заменяется на WireMock** — по образцу набора Sales, включая тот же сетевой алиас и порт. Стаб **не требует** `Authorization`: настоящий `GET /api/users/{id}` анонимен (раздел «Разрешение канала доставки»), и заглушка обязана повторять реальный контракт, а не более строгий выдуманный.
12. **`Push.Client` в e2e не поднимаем.** Его роль (клиент SignalR-хаба) в тестах играет `Microsoft.AspNetCore.SignalR.Client` внутри тестового процесса — это и делает проверку доставки пуша наблюдаемой.
13. **`MapHealthChecks("/health")` добавляем** во всех трёх WebApi. Это сознательное отступление от «поведение 1:1»: `AddHealthChecks()` вызывается уже сейчас, но `/health` никогда не маппился, то есть эндпойнта нет, хотя AGENTS.md его обещает. Правка аддитивная и приводит контекст к виду остальных трёх.
14. **Предсуществующие баги (§9) чиним отдельными коммитами в фазе 5**, а не внутри коммита миграции. Поведение, которое e2e зафиксировали как текущее, меняем **после** миграции, чтобы baseline оставался baseline'ом.

---

## Разрешение канала доставки: почему Gateway ходит в Security и почему без токена

Контракт `SendNotificationToUserCommand` намеренно несёт только `UserId` и `Message` — ни email'а, ни телефона, ни какого-либо признака канала. Отправитель (Sales) говорит «уведомить пользователя X», а **куда** доставлять, решает сам Notification. Обращение Gateway к Security за карточкой пользователя — не костыль и не обход, а несущая часть этого разделения: параметры доставки принадлежат контексту Notification и добываются им же.

Security это поддерживает явно. В `Crnc.Oms.Security.WebApi/Controllers/UsersController` класс помечен `[Authorize]`, но **обе операции чтения перекрыты `[AllowAnonymous]`**:

| Эндпойнт | Строка | Авторизация |
|---|---|---|
| `GET /api/users` | 43–45 | `[AllowAnonymous]` |
| `GET /api/users/{id}` | 69–72 | `[AllowAnonymous]` |
| `POST /api/users` | 108–109 | `[Authorize(Roles = Admin)]` |
| `PUT /api/users/{id}` | 151–152 | `[Authorize(Roles = Admin)]` |
| `DELETE /api/users/{id}` | 194–195 | `[Authorize(Roles = Admin)]` |

Именно поэтому `UserInfoGateway`, создающий `RestClient` без аутентификатора, работает — и должен продолжать работать после миграции.

История это подтверждает, а не опровергает: `d655cac` (2020-02-19) снял авторизацию с Security «так как обмен сообщениями не предполагает авторизации», `4bf86e1` (2020-03-04) вернул `[Authorize]` на класс, оставив операции чтения анонимными. Схема с тех пор не менялась и работает как задумано.

**Следствия для миграции:**

- **Токен в `UserInfoGateway` добавлять не нужно** ни на каком шаге. Переписывание на `HttpClient` (§6) — эквивалентная замена транспорта, и только.
- **Заглушка Security в e2e не требует `Authorization`** (решение 11). Стаб обязан повторять реальный контракт; более строгая заглушка проверяла бы выдуманное поведение.
- **Класть email или другой адрес канала в контракт сообщения нельзя** — это ровно то разделение ответственности, ради которого контракт сделан узким. Если однажды понадобится хранить предпочтения доставки, их место в контексте Notification, а не в команде от Sales.

Отдельно, **вне периметра этой миграции**: `GET /api/users` анонимен и возвращает `UserItemDto` с полем `Password = user.PasswordHash`, а также email'ы и телефоны всех пользователей. Это вопрос к контексту Security и к тому, где проходит граница «внутренней» сети; здесь фиксируется только чтобы не потерялось.

---

## Фаза 0: зафиксировать реальный baseline

Прошлые три миграции начинались с инвентаризации по коду. Здесь этого мало: два места ведут себя не так, как написано, и оба надо увидеть глазами до того, как под них писать тесты.

1. **`docker-compose --profile notification up` + смена статуса заказа в Sales.** Прогнать цепочку целиком и записать, что реально происходит: дошла ли команда до Gateway, вернул ли Security карточку пользователя, ушли ли команды в `sendEmailNotificationToReceiver` и `sendPushNotificationToReceiver`, появился ли пуш в логах `notification-push-client`, не выросла ли очередь `sendNotificationToUser_error`. По коду цепочка должна работать; шаг существует, чтобы e2e-набор писался под измеренное поведение, а не под вычитанное.
2. **`POST http://localhost:8104/api/emailNotifications`** — Email вызывает `app.UseAuthentication()`, но `services.AddAuthentication(...)` в его `Startup.cs` **не вызывает вообще**. Ожидание: `AuthenticationMiddleware` не сможет получить `IAuthenticationSchemeProvider`. Нужно увидеть, падает ли это на старте контейнера, на первом запросе, или (вопреки ожиданию) не падает вовсе — от ответа зависит, что писать в e2e-тест на HTTP-вход Email.
3. **Проверить `/health` на всех трёх** — ожидание: 404, потому что `MapHealthChecks` нигде нет (решение 13).

Что уже проверено и переспрашивать не нужно:

- **`Crnc.Oms.Notification.sln` собирается на SDK 10.0.100 без ошибок**: 0 errors, 28 warnings (`NETSDK1138` ×5, `NU1902` ×6 — JwtBearer 3.1.0 и `System.IdentityModel.Tokens.Jwt` 5.6.0, `NU1903` ×4 — RestSharp 106.6.10). Локальный baseline воспроизводим.
- **Адресация команд в шине работает, несмотря на подозрительный вид.** `EndpointConvention.Map<SendNotificationToUserCommand>(new Uri($"{endpoint}/commands/sendNotificationToUser"))` при хосте `rabbitmq://message-broker` выглядит как отправка в vhost `commands`. Это не так: e2e-набор Sales (`RabbitMqAdmin.EnsureSpyQueueAsync`) вешает шпионскую очередь на exchange `sendNotificationToUser` **в vhost по умолчанию `/`** и он зелёный. Значит сегмент `commands` съедается адресацией MassTransit, а команда попадает ровно на тот exchange, к которому привязан `ReceiveEndpoint("sendNotificationToUser")` Gateway. Ту же конструкцию Gateway использует для Email и Push — она сохраняется при миграции как есть.

---

## Пререквизит: e2e-тесты Notification

По схеме Security, Sales и Production: сначала набор, зелёный на текущем `netcoreapp3.1`, потом миграция под его защитой. Конвенции — в AGENTS.md, раздел «Test conventions»; проект `Crnc.Oms.Notification.E2ETests` на `net10.0`, поверх Testcontainers, без `ProjectReference` на код сервисов.

Расположение: `src/Server/src/Crnc.Oms.Notification/Crnc.Oms.Notification.E2ETests/`, членом зонтичного `Crnc.Oms.Notification.sln`. Каталог лежит **вне** всех четырёх docker-контекстов (контекст каждого юнита — его собственная папка), поэтому в `.dockerignore` его исключать не нужно — в отличие от Sales и Production.

### Периметр: настоящие все три сервиса и брокер, Security — заглушка

- **Настоящие**: `notification-gateway-api`, `notification-email-api`, `notification-push-api` (образы собираются из их настоящих Dockerfile), RabbitMQ.
- **Заглушка**: Security → `wiremock/wiremock:3.13.2` под тем же сетевым алиасом и портом 8080, что и настоящий сервис. Стабы ставятся через `/__admin`, `GET /__admin/requests` служит доказательством, что исходящий вызов состоялся.
- **Не поднимаем**: Sales, Production, SPA, `notification-push-client`.

Почему все три сервиса разом, вопреки правилу «настоящая только своя БД» из конвенции Sales: у Notification нет БД вообще, а Email и Push — не соседние контексты, а части одного. Разрезать набор на три фикстуры значило бы поднимать RabbitMQ и собирать образы трижды, а самую ценную проверку — сквозную цепочку `команда → Gateway → шина → Push → SignalR` — не покрыть вовсе.

### Контейнеры фикстуры

| Контейнер | Образ | Готовность |
|---|---|---|
| RabbitMQ | `rabbitmq:3-management` (как в compose и в наборе Sales) | TCP 5672 + `GET /api/overview` |
| Security-заглушка | `wiremock/wiremock:3.13.2` | `GET /__admin/mappings` |
| Gateway / Email / Push | `ImageFromDockerfileBuilder` из настоящих Dockerfile | `GET /swagger/index.html` |

Порт API в фикстуре — 80 на фазе 1 и 8080 после фазы 3 (§10.4). Это единственное место в наборе, которое миграция меняет.

### Аутентификация: тест подписывает токен сам

`TestJwt.cs` переносится из набора Sales без изменений, ключ навязывается контейнерам через `Auth:JwtBase64SymmetricKey`. Набор не зависит ни от Security, ни от ротации ключей.

Важная деталь: **стаб Security не требует `Authorization`** и отвечает на `GET /api/users/{id}` без него. Настоящий эндпойнт анонимен (раздел «Разрешение канала доставки»), и заглушка обязана повторять именно это. Более строгий стаб выглядел бы «безопаснее», но проверял бы контракт, которого нет.

### Что покрываем

**Gateway** (`GatewayNotificationsTests`)
- `POST /api/notifications/user` без токена → 401.
- С валидным токеном и известным пользователем → 200, а в шпионских очередях `sendEmailNotificationToReceiver` и `sendPushNotificationToReceiver` прибавилось по сообщению. Это и есть проверка разрешения канала доставки: Gateway сходил в Security за email'ом и развёл уведомление по двум каналам.
- Невалидное тело (пустой `message`) → 400, и **ключи `ModelState` в ответе в camelCase** — это тест на §4.
- Неизвестный пользователь: стаб отвечает 404 → Gateway возвращает 400 (`MissingUserException` → `BadNotificationDataException`), в шпионских очередях ничего не прибавилось.
- Пользователь без email в карточке → 400, и снова ни одной команды в шине. Это прямая проверка того, что разрешение канала доставки живёт в Gateway.
- `GET /__admin/requests` подтверждает, что вызов в Security состоялся и что ушёл он на `GET /api/users/{id}` с ожидаемым id.

**Email** (`EmailNotificationsTests`)
- Консьюмер: публикуем `SendEmailNotificationToReceiverCommand` в очередь `sendEmailNotificationToReceiver` → сервис его съел (счётчик очереди вернулся к нулю), в логах контейнера строка об отправке.
- `POST /api/emailNotifications` — фиксируем фактическое поведение по итогам шага 2 фазы 0.
- Валидация: тело без `senderEmail`/`receiverEmail` → 400 с camelCase-ключами.

**Push** (`PushNotificationsTests`)
- **Главный тест набора**: тестовый SignalR-клиент подключается к `/hubs/push` с токеном пользователя `X`, в очередь `sendPushNotificationToReceiver` кладётся команда с `ReceiverUserId = X` → клиент получает `ReceivePushMessageAsync`. Это покрывает разом MassTransit, DI, `SignalRPushGateway`, `IHubContext<PushHub, IPushNotificationClient>`, маппинг хаба и JWT-через-query-string (§7).
- Подключение к хабу без токена → отказ.
- `POST /api/pushNotifications` (`[AllowAnonymous]`) → 200 и та же доставка в хаб.

**Шина как таковая** — по образцу Sales: шпионские очереди на exchange'ах `sendEmailNotificationToReceiver` и `sendPushNotificationToReceiver` объявляются **до** действия, проверки — дельтами.

### Что сознательно НЕ покрываем

- Реальную отправку письма — её нет: `Email.Integration.EmailGateway` только пишет в лог.
- `Push.Client` — консольная утилита для наблюдения в dev; её роль в наборе играет тестовый SignalR-клиент.
- Сквозной сценарий с настоящим Sales — вне периметра; он проверяется вручную на шаге фазы 4.
- Grafana/Prometheus — только факт `UP` таргета, вручную.

---

## Инвентаризация (подтверждено чтением файлов и сборкой)

**13 проектов, 4 деплой-юнита, 5 решений.**

| Юнит | Проекты | Вход | Исходящие вызовы |
|---|---|---|---|
| Gateway | Application, Integration, Messaging.Contract (`netstandard2.0`), WebApi | HTTP `POST /api/notifications/user` (`[Authorize]`) + консьюмер `SendNotificationToUserCommand` | Security по HTTP (RestSharp, **живой**), Email и Push через шину |
| Email | Application, Integration, Messaging.Contract (`netcoreapp3.1`), WebApi | HTTP `POST /api/emailNotifications` (без атрибута = аноним) + консьюмер `SendEmailNotificationToReceiverCommand` | нет (лог) |
| Push | Application, Integration, Messaging.Contract (`netcoreapp3.1`), WebApi | HTTP `POST /api/pushNotifications` (`[AllowAnonymous]`) + консьюмер `SendPushNotificationToReceiverCommand` | SignalR-хаб `/hubs/push` |
| Push.Client | 1 проект, `Exe` | — | Security по HTTP (RestSharp, **живой**), SignalR-хаб Push |

Общее для трёх WebApi: `AddControllers().AddNewtonsoftJson(CamelCase + StringEnumConverter)`, JWT-bearer (кроме Email — см. ниже), NSwag 13.2.0, prometheus-net 3.5.0, `AddMassTransit(CreateBus, ConfigureMassTransit)` в стиле 6.x, `UseSwaggerUi3()`, `Startup.cs` + `WebHost.CreateDefaultBuilder` с избыточными `ConfigureAppConfiguration` и пустым `ConfigureKestrel`.

Что отличается между тремя и требует внимания:

- **Email не вызывает `AddAuthentication`**, хотя вызывает `app.UseAuthentication()` (§9, проверка — шаг 2 фазы 0).
- **Email ставит `UseHttpMetrics()` до `UseRouting()`**, Gateway и Push — после. Приводим к порядку Gateway/Push (в prometheus-net 8 метрика с маршрутом требует, чтобы роутинг уже отработал).
- **Push** дополнительно имеет `AddSignalR()`, CORS с `AllowCredentials()` и конкретным origin из `IntegrationEndpoints:UiEndpoint`, а также `JwtBearerEvents.OnMessageReceived`, читающий `access_token` из query string для пути `/hubs/push`.
- **Gateway** — единственный с `EndpointConvention.Map` (три штуки) и с `IHttpContextAccessor` (зарегистрирован, но не используется).

`MonitoringRequestMiddleware` физически есть во всех трёх WebApi и **не подключён ни в одном** (в Sales, Security и Production он подключён — `app.UseMonitoringRequestMiddleware()`).

`MapHealthChecks` не вызывается ни в одном из трёх, при том что `AddHealthChecks()` вызывается везде.

Namespace'ы разъехались копипастой (§9) — на сборку это не влияет, но читается как ошибка.

---

## 1. Версии пакетов

Проверено на nuget.org (`dotnet package search`, 2026-08-20). Версии совпадают с уже мигрированными Security/Sales/Production везде, где пакет общий.

| Пакет | Где | Было | Станет |
|---|---|---|---|
| `MassTransit` | Gateway.Integration, Gateway.WebApi | 6.2.1 | **8.5.10** |
| `MassTransit.RabbitMQ` | все 3 WebApi | 6.2.1 | **8.5.10** |
| `MassTransit.AspNetCore` | все 3 WebApi | 6.2.1 | **удалить** — в 8.x хостинг встроен в `AddMassTransit` |
| `MassTransit.Extensions.DependencyInjection` | все 3 WebApi | 6.2.1 | **удалить** — влит в основной пакет |
| `MassTransit.Extensions.Logging` | все 3 WebApi | 5.5.6 | **удалить** — логирование через `Microsoft.Extensions.Logging` из коробки |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | все 3 WebApi | 3.1.0 | **10.0.11** |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | все 3 WebApi | 3.1.0 | **удалить** (переход на STJ) |
| `System.IdentityModel.Tokens.Jwt` | все 3 WebApi | 5.6.0 | **удалить** — ни один тип из него не используется, остался только `using` в `Startup.cs` Gateway. `SymmetricSecurityKey` для `AuthSettings` берётся из `Microsoft.IdentityModel.Tokens`, который приходит транзитивно с `JwtBearer 10.0.11` (8.22.0) — как это подтвердилось в Sales и Production |
| `NSwag.AspNetCore` | все 3 WebApi | 13.2.0 | **14.7.1** — breaking rename `UseSwaggerUi3()` → `UseSwaggerUi()` |
| `prometheus-net.AspNetCore` | все 3 WebApi | 3.5.0 | **8.2.1** |
| `prometheus-net` | Email.Application, Push.Application | 3.5.0 | **8.2.1** — используется по делу (`Metrics.CreateCounter` в `EmailNotificationService`/`PushNotificationService`), не удалять |
| `Microsoft.Extensions.Logging.Abstractions` | 6 проектов | 3.1.0 | **10.0.11** |
| `Microsoft.Extensions.Options` | Gateway.Integration | 3.1.0 | **10.0.11** |
| `RestSharp` | Gateway.Integration, Push.Client | 106.6.10 | **удалить** → `HttpClient` (§6) |
| `RestSharp` | Email.Integration, Push.Integration | 106.6.10 | **удалить** — в этих двух проектах нет ни одного `using RestSharp` |
| `Microsoft.AspNetCore.SignalR` | Push.Integration | 1.1.0 | **удалить** → `FrameworkReference` (§7) |
| `Microsoft.AspNetCore.SignalR.Client` | Push.Client | 3.1.0 | **10.0.11** |
| `Microsoft.Extensions.{Configuration,Configuration.Binder,Configuration.FileExtensions,Configuration.Json,Hosting,Logging,Logging.Console,Options}` | Push.Client | 3.1.0 | **10.0.11** |
| `Polly` | Push.Client | 7.2.0 | **удалить** — только два `using Polly;`, ни одной политики в коде |

Итого в каждом WebApi остаётся 4 `PackageReference` вместо 9; в `Push.Client` — 9 вместо 12; в `Push.Integration` — 1 вместо 3.

Три `NU1903`/`NU1902` из baseline (RestSharp 106.6.10, JwtBearer 3.1.0, `System.IdentityModel.Tokens.Jwt` 5.6.0) закрываются этой таблицей полностью — после миграции сборка должна быть без предупреждений безопасности.

---

## 2. Изменения в csproj

`<TargetFramework>` → `net10.0` в 10 проектах: по три «прикладных» на юнит (Application, Integration, WebApi) плюс `Push.Client`. Ещё два — `Email.Messaging.Contract` и `Push.Messaging.Contract` — переводятся с `netcoreapp3.1` на **`netstandard2.0`** (решение 8), чтобы совпасть с `Gateway.Messaging.Contract`.

Добавить `<ImplicitUsings>enable</ImplicitUsings>` в проекты на `net10.0` — аддитивно, риск низкий. `<Nullable>` не включаем.

`<GenerateDocumentationFile>true</GenerateDocumentationFile>` + `<NoWarn>$(NoWarn);1591</NoWarn>` в Application и WebApi оставить: на них держится наполнение Swagger из XML-комментариев контроллеров и `<example>`-блоков DTO.

`Push.Client.csproj`: блок `<Content Include="appsettings.json">` с `CopyToPublishDirectory` оставить как есть — он не про TFM.

---

## 3. Program.cs × 3 (слияние Startup.cs → minimal hosting)

Удалить `Startup.cs` во всех трёх WebApi, переписать `Program.cs`. Поведение сохраняется 1:1, кроме явно оговорённого. Убрать, как в трёх предыдущих миграциях, избыточный `ConfigureAppConfiguration` (дублирует дефолт `WebApplication.CreateBuilder`) и no-op `ConfigureKestrel`.

Каркас, общий для трёх (за образец берётся `Crnc.Oms.Sales.WebApi/Program.cs`):

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddCors(/* политика юнита — без изменений */);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    // JsonStringEnumConverter НЕ добавляем: enum'ов в контрактах Notification нет вообще (§4).
});

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);
builder.Services.Configure<IntegrationEndpointSettings>(builder.Configuration.GetSection("IntegrationEndpoints"));

builder.Services.AddMassTransit(x => { /* §5 */ });

// ... регистрации сервисов юнита ...

builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection("Auth"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(/* без изменений */);
builder.Services.AddOpenApiDocument(/* без изменений */);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

app.UseRouting();
app.UseHttpMetrics();
app.UseCors("...");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();
app.MapHealthChecks("/health");   // решение 13 — новое

app.UseOpenApi();
app.UseSwaggerUi();               // было UseSwaggerUi3()

app.Run();
```

Отличия по юнитам:

- **Email**: `AddAuthentication`/`AddJwtBearer` сейчас отсутствуют (§9). Добавляем — иначе `UseAuthentication()` на .NET 10 гарантированно не соберёт пайплайн. Конфигурация `Auth` в его `appsettings.json` уже есть и совпадает с остальными, `AuthSettings` в проекте лежит. Порядок `UseHttpMetrics`/`UseRouting` исправляется здесь же.
- **Push**: добавляется `builder.Services.AddSignalR()` и `app.MapHub<PushHub>("/hubs/push")`. `JwtBearerEvents.OnMessageReceived` переносится дословно. CORS-политика `CorsPolicy` с `WithOrigins(...).AllowCredentials()` — дословно; **`UseCors` обязан стоять до `MapHub`**, иначе SPA перестанет подключаться к хабу.
- **Gateway**: `AddHttpContextAccessor()` вместо `TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>()`; три `EndpointConvention.Map` переезжают внутрь `UsingRabbitMq` (§5); `UserInfoGateway` регистрируется через `AddHttpClient` (§6).

`MonitoringRequestMiddleware` оставляем неподключённым — то есть 1:1 с текущим поведением. Подключать его «заодно, как в Sales» не надо: это изменило бы набор метрик, а причин для такого изменения в рамках миграции нет (§9 фиксирует это как долг).

---

## 4. Переход на System.Text.Json

`AddNewtonsoftJson(CamelCasePropertyNamesContractResolver + StringEnumConverter)` → `AddJsonOptions` с `PropertyNamingPolicy`, `DictionaryKeyPolicy`, `PropertyNameCaseInsensitive`.

- **`StringEnumConverter` отбрасывается без последствий**: `grep -rn "enum "` по всему контексту не находит ни одного объявления. Ни один DTO, ни один контракт сообщения enum'ов не содержит. Это единственный из четырёх контекстов, где переход на STJ не может задеть сериализацию enum'ов в принципе.
- **`DictionaryKeyPolicy` здесь обязателен, а не «для паритета»**: `SendEmailMessageInputDto` несёт `[Required]` и `[EmailAddress]`, `SendToNotificationUserInputDto` и `SendPushMessageInputDto` — `[Required]`. Значит `BadRequest(ModelState)` реально срабатывает и ключи словаря видны снаружи. Без `DictionaryKeyPolicy` они молча станут PascalCase — та самая регрессия, которую план Security ловил тестом на JSON-регистр ключей `ModelState`. Тесты валидации из раздела «Пререквизит» существуют именно для этого.
- **Сужение толерантности входа**: STJ строже Newtonsoft (не примет числа в кавычках и т.п.). Внешних потребителей у этих трёх API нет — SPA ходит только в хаб Push, остальное идёт через шину. Изменение осознанное.
- `Newtonsoft.Json` остаётся транзитивно только там, где его тянет что-то ещё; явных `PackageReference` после §1 не остаётся.

---

## 5. MassTransit 6.2.1 → 8.5.10

### 5.1. Конфигурация

Уходит вся 6.x-обвязка: `MassTransit.AspNetCoreIntegration`, `MassTransit.ExtensionsDependencyInjectionIntegration`, `IServiceCollectionConfigurator`, `IBusControl CreateBus(IServiceProvider)`, `Bus.Factory.CreateUsingRabbitMq`, перегрузка `services.AddMassTransit(CreateBus, ConfigureMassTransit)`. Новая форма — как в Sales и Production:

```csharp
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SendNotificationToUserConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        cfg.ReceiveEndpoint("sendNotificationToUser", e =>
        {
            e.ConfigureConsumer<SendNotificationToUserConsumer>(context);
        });

        // Только в Gateway:
        EndpointConvention.Map<SendNotificationToUserCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendNotificationToUser"));
        EndpointConvention.Map<SendPushNotificationToReceiverCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendPushNotificationToReceiver"));
        EndpointConvention.Map<SendEmailNotificationToReceiverCommand>(
            new Uri($"{integrationSettings.MessageBrokerEndpoint}/commands/sendEmailNotificationToReceiver"));
    });
});
```

Имена очередей (`sendNotificationToUser`, `sendEmailNotificationToReceiver`, `sendPushNotificationToReceiver`) и адреса в `EndpointConvention` **не меняются** — иначе Sales, уже отправляющий команды по этим адресам, потеряет получателя.

`IConsumer<T>`, `ConsumeContext<T>`, `ISendEndpointProvider.Send<T>(object)` в 8.x не изменились — три консьюмера и два `MessageBroker*Gateway` правок не требуют, кроме удаления `using MassTransit.Conductor.Server;` из `MessageBrokerEmailGateway` (Conductor из 8.x убран; здесь этот `using` и так ничего не даёт).

`SendEmailInputDto` не реализует контрактный интерфейс — `Send<SendEmailNotificationToReceiverCommand>(dto)` полагается на message initializer по именам свойств. В 8.x механизм сохранён; `SendPushInputDto` интерфейс реализует и идёт обычным путём.

### 5.2. Совместимость шины — здесь это уже не риск

На миграции Sales эмпирически проверено, что MassTransit 8 (System.Text.Json) и MassTransit 6 разговаривают друг с другом; Production подтвердил это живым round-trip'ом. Notification обменивается сообщениями только с Sales (входящий `SendNotificationToUserCommand`) и сам с собой (Gateway → Email/Push). Sales уже на 8.5.10, значит после этой миграции разноверсионных пар в системе не останется вовсе, а во время миграции пара «Sales 8.5.10 → Notification 6.2.1» — та самая, что уже работает сегодня.

### 5.3. RabbitMQ

Образ `rabbitmq:3-management` не меняется. `notification-email-api` и `notification-push-api` не имеют `depends_on: message-broker` — при старте раньше брокера шина в 8.x уходит в retry, не роняя хост. Порядок в compose не трогаем.

---

## 6. RestSharp → HttpClient

Живых вызовов через RestSharp ровно два:

1. **`Gateway.Integration/Gateways/UserInfoGateway.cs`** → Security `GET /api/users/{id}`.
2. **`Push.Client/Auth/AuthClient.cs`** → Security `POST /api/accounts/auth`.

Оба переписываются по образцу `Crnc.Oms.Sales.Integration/Gateways/EmployeeSecurityGateway.cs`: конструктор принимает `HttpClient`, регистрация через `AddHttpClient<TInterface, TImpl>(client => client.BaseAddress = ...)`, статический `JsonSerializerOptions(JsonSerializerDefaults.Web)`, `ReadFromJsonAsync`. Обработка ошибок сохраняет текущие ветки: 404 → `MissingUserException` в `UserInfoGateway`, всё остальное → `Exception` с текстом про статус-код (`NotificationService` ловит только `MissingUserException`, поэтому менять типы исключений нельзя).

`BaseAddress` требует завершающего слэша, а относительный путь — его отсутствия: `client.BaseAddress = new Uri(endpoint.TrimEnd('/') + "/")` и `"api/users/{id}"` без ведущего слэша. В Sales это уже так; повторить дословно.

Ещё три файла в `Gateway.Integration` содержат `using RestSharp` — `EmailGateway.cs`, `PushGateway.cs` (HTTP-реализации шлюзов, **не зарегистрированные в DI**) и `MessageBrokerEmailGateway`/`MessageBrokerPushGateway` (лишние `using`). Судьба первых двух — §9.

**Токен в `UserInfoGateway` не появляется — ни на этом шаге, ни позже.** `GET /api/users/{id}` у Security анонимен намеренно (раздел «Разрешение канала доставки»), и переписывание на `HttpClient` обязано остаться эквивалентной заменой транспорта.

---

## 7. SignalR: пакет 1.1.0 → FrameworkReference

`Crnc.Oms.Notification.Push.Integration` — обычный `Microsoft.NET.Sdk`-проект, которому нужны серверные типы SignalR (`Hub<T>`, `IHubContext<THub, TClient>`) для `PushHub` и `SignalRPushGateway`. Сейчас они берутся из `PackageReference Microsoft.AspNetCore.SignalR 1.1.0` — пакета эпохи ASP.NET Core 2.1. С 3.0 серверный SignalR входит в shared framework `Microsoft.AspNetCore.App`, и одноимённые типы из старого пакета в лучшем случае дублируются, в худшем — расходятся с теми, что реально грузятся в рантайме хоста.

Правильная форма:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

Пакет `Microsoft.AspNetCore.SignalR` удаляется. Ловится это мгновенно компилятором (`Hub<>`/`IHubContext<,>` не найдутся, если забыть `FrameworkReference`), а проверяется — главным push-тестом из раздела «Пререквизит».

`Push.WebApi` тип `PushHub` уже видит через `ProjectReference` на `Push.Integration`; `MapHub<PushHub>` в minimal hosting работает так же, как `e.MapHub<PushHub>` в `UseEndpoints`.

---

## 8. Push.Client (консольное приложение)

Отдельный деплой-юнит, свой Dockerfile, свой `.sln`, TFM `netcoreapp3.1`, `OutputType=Exe`. Работы:

- TFM → `net10.0`, версии `Microsoft.Extensions.*` и `Microsoft.AspNetCore.SignalR.Client` → 10.0.11 (§1).
- `Polly` и `RestSharp` — удалить (§1), `AuthClient` переписать на `HttpClient` (§6).
- `Program.cs` уже на `Host.CreateDefaultBuilder(...).RunConsoleAsync()` — переписывать на `WebApplication` нечего и незачем; `ConfigureAppConfiguration`/`ConfigureLogging` оставить как есть.
- `HubConnectionBuilder`, `AccessTokenProvider`, `connection.On<string,string>("ReceivePushMessageAsync", ...)` — API SignalR-клиента между 3.1 и 10 совместим, правок не ожидается.
- Базовый образ рантайма: `mcr.microsoft.com/dotnet/core/runtime:3.1` → **`mcr.microsoft.com/dotnet/runtime:10.0`** (не `aspnet`, это консоль).
- В compose: `IntegrationEndpoints:PushNotificationServiceEndpoint=http://notification-push-api` → **`http://notification-push-api:8080`**. Это единственный по-настоящему живой межконтейнерный HTTP-вызов внутри контекста, и без правки Push.Client молча перестанет подключаться к хабу (§10.4).

`Push.Client` мигрируется **последним**: он единственный, кто ходит в уже мигрированный Push, и его прогон — самая дешёвая ручная проверка того, что хаб на .NET 10 работает.

---

## 9. Мёртвый код и предсуществующие баги

**Группа A — чистится отдельным коммитом на 3.1, до смены TFM** (по образцу шага 6 плана Production: так поломки от чистки не смешаются с поломками от миграции):

- `Gateway.Integration/Gateways/EmailGateway.cs` и `PushGateway.cs` — HTTP-реализации `IEmailGateway`/`IPushGateway` на RestSharp, **нигде не зарегистрированные** (в DI стоят `MessageBrokerEmailGateway`/`MessageBrokerPushGateway`). Удалить. Вместе с ними теряют смысл настройки `EmailNotificationServiceEndpoint`/`PushNotificationServiceEndpoint` в `IntegrationEndpointSettings` Gateway и одноимённые переменные в compose — но их **оставляем** до отдельного решения: удаление настроек из compose трогает файл, общий с другими контекстами.
- Лишние `using RestSharp` / `RestSharp.Authenticators` в `MessageBrokerEmailGateway`, `MessageBrokerPushGateway`; `using MassTransit.Conductor.Server` там же.
- `using Polly` в `Push.Client/Program.cs` и `Push/PushConnector.cs`.
- **Namespace-мусор от копипасты** (на сборку не влияет, читается как ошибка):
  - `Gateway.Integration/Dto/GetUserInfo{Input,Output}Dto.cs` объявлены в `Crnc.Oms.Notification.Email.Integration.Dto`;
  - `Email.Integration/Dto/EmailMessage{Input,Output}Dto.cs` — наоборот, в `Crnc.Oms.Notification.Gateway.Integration.Dto`;
  - `Email.WebApi/Authorization/AuthSettings.cs` и `Push.WebApi/Authorization/AuthSettings.cs` — оба в `Crnc.Oms.Notification.Gateway.WebApi.Authorization`;
  - `Push.Client/Settings/AuthSettings.cs` — в `Crnc.Oms.Notification.Gateway.Integration.Settings`;
  - `Push.WebApi/Middlewares/MonitoringRequestMiddleware.cs` — в `Crnc.Oms.Sales.Not.Middlewares`.

  Правка механическая, но затрагивает `using`-и в шести файлах; делать её строго отдельным коммитом и с прогоном e2e.

**Группа B — исправления поведения, фаза 5, каждое своим коммитом:**

- **`SendEmailNotificationToReceiverConsumer` теряет адресатов**: из команды берётся только `Message`, а `SenderEmail`/`ReceiverEmail` не переносятся в `SendEmailMessageInputDto`. То есть даже при живой цепочке письмо «отправлялось» бы никому. Поля в контракте есть, Gateway их заполняет — теряются они ровно в консьюмере.
- **Email без `AddAuthentication`** — если шаг 2 фазы 0 покажет, что HTTP-вход Email не работает вовсе, это отдельный факт для записи в тест; сам фикс входит в §3 (миграционный коммит Email), потому что на .NET 10 оставить как есть нельзя.
- **`MonitoringRequestMiddleware` не подключён** ни в одном из трёх (в остальных контекстах подключён). Оставляем как есть, фиксируем как долг: подключение меняет набор метрик и должно быть отдельным осознанным решением.
- **`/health` отсутствует** — закрывается решением 13 в самом миграционном коммите (аддитивно).
- **`.sln` подюнитов не содержат `Messaging.Contract`** (Email.sln, Gateway.sln, Push.sln — по 3 проекта из 4). Сборка не страдает: проект приезжает транзитивно через `ProjectReference` из WebApi. Добавить для порядка — можно в том же коммите, что и §10.5.

---

## 10. Dockerfile, сборка и compose

### 10.1. Базовые образы

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build          # было .../core/sdk:3.1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime     # было .../core/aspnet:3.1   (3 WebApi)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime    # было .../core/runtime:3.1  (Push.Client)
```

(путь без сегмента `/core/` — старый ретайрен).

### 10.2. `dotnet publish -o` по solution — блокер `NETSDK1194`

Все четыре Dockerfile — копии дореформенных Security/Sales/Production: `RUN dotnet restore` и `RUN dotnet publish -c Release -o out` без явного таргета, то есть по `.sln`. С SDK 7.0.200 `--output` для solution — ошибка `NETSDK1194`, сборка образа на `sdk:10.0` упадёт. В каждом заменить на явный csproj, например для Gateway:

```dockerfile
RUN dotnet restore Crnc.Oms.Notification.Gateway.WebApi/Crnc.Oms.Notification.Gateway.WebApi.csproj
RUN dotnet publish Crnc.Oms.Notification.Gateway.WebApi/Crnc.Oms.Notification.Gateway.WebApi.csproj -c Release -o out
```

Для `Push.Client` таргет — `Crnc.Oms.Notification.Push.Client/Crnc.Oms.Notification.Push.Client.csproj`.

Побочный плюс тот же, что в трёх прошлых миграциях: состав `.sln` перестаёт влиять на прод-образ.

### 10.3. `.dockerignore` (4 новых файла)

Сейчас их нет ни в одном из четырёх контекстов, а `COPY . ./aspnetapp` тащит `bin/`/`obj/` всех проектов. Создать по одному в каждом:

```
**/bin/
**/obj/
```

Строки `Crnc.Oms.Notification.E2ETests/` здесь не нужно: тестовый проект лежит уровнем выше, вне всех четырёх docker-контекстов (в отличие от Sales и Production, где он внутри).

### 10.4. Риск: `aspnet:10.0` слушает 8080, а не 80

Образ `mcr.microsoft.com/dotnet/aspnet:10.0` слушает 8080 (`ASPNETCORE_HTTP_PORTS=8080` зашит в образ). Проявляется не ошибкой, а «зависшей» проверкой готовности контейнера. Решение то же, что для трёх предыдущих сервисов, — принимаем дефолт образа и правим тех, кто ходит на Notification. Здесь этот список длиннее всего:

| Файл | Было | Станет | Живой вызов? |
|---|---|---|---|
| `docker-compose.yml`, `notification-gateway-api.ports` | `"8100:80"` | `"8100:8080"` | — |
| `docker-compose.yml`, `notification-email-api.ports` | `"8104:80"` | `"8104:8080"` | — |
| `docker-compose.yml`, `notification-push-api.ports` | `"8107:80"` | `"8107:8080"` | — |
| `docker-compose.yml`, `notification-push-client` env `IntegrationEndpoints:PushNotificationServiceEndpoint` | `http://notification-push-api` | `http://notification-push-api:8080` | **да** — SignalR |
| `docker-compose.yml`, `sales-api` env `IntegrationEndpoints:NotificationServiceEndpoint` | `http://notification-gateway-api` | `http://notification-gateway-api:8080` | нет — Sales ходит в Gateway только через шину (`MessageBrokerNotificationGateway`), настройка мёртвая; правим ради согласованности |
| `docker-compose.yml`, `notification-gateway-api` env `Email/PushNotificationServiceEndpoint` | `http://notification-{email,push}-api` | `…:8080` | нет — HTTP-шлюзы не зарегистрированы (§9), но после чистки строки лучше оставить корректными |
| `prometheus/prometheus.yml`, 3 таргета | `notification-{gateway,email,push}-api` | `…:8080` | **да** — иначе таргеты `down` |
| фикстура e2e | `ApiContainerPort = 80` | `8080` | **да** |

Внешние порты хоста (8100/8104/8107) **не меняются**, поэтому README, AGENTS.md и SPA править не надо: `PUSH_HUBS_URL` в `docker-compose.yml` остаётся `http://localhost:8107/hubs`.

**Отдельно про Prometheus**: после правки `prometheus.yml` нужен `docker-compose build prometheus` — Dockerfile копирует конфиг на этапе сборки (`ADD prometheus.yml /etc/prometheus/`), обновления файла в репозитории недостаточно. Этот факт стоил времени на проверке Production (см. статус её плана).

### 10.5. `.sln`

`Crnc.Oms.Notification.E2ETests` добавляется в зонтичный `Crnc.Oms.Notification.sln`, **строго после §10.2 и §10.3**. Само по себе это безопаснее, чем в прошлых миграциях: зонтичный `.sln` не попадает ни в один docker-контекст (каждый Dockerfile копирует `*.sln` из папки своего юнита, где лежит только его собственное решение). Но правило «сначала явный csproj в Dockerfile» соблюдаем — оно снимает зависимость от состава решений вообще.

---

## 11. Порядок выполнения

**Фаза 0 — зафиксировать реальность (до всего остального):**

1. Три проверки на живом стенде из раздела «Фаза 0»: сквозная цепочка уведомления, поведение HTTP-входа Email, отсутствие `/health`. Результаты дописать в этот план.

**Фаза 1 — e2e-тесты на текущем 3.1 (отдельная ветка):**

2. Создать четыре `.dockerignore` (§10.3) — до тестов, иначе каждый прогон фикстуры будет пересобирать образы с нуля.
3. Создать `Crnc.Oms.Notification.E2ETests` по конвенциям Security/Sales/Production и периметру из раздела «Пререквизит». `TestJwt.cs` и `RabbitMqAdmin.cs`/`WireMockAdmin.cs` переносятся из набора Sales. Довести до зелёного **на текущем `netcoreapp3.1`** — это baseline.
4. Зафиксировать в тестах фактическое поведение цепочки, измеренное на шаге 1, — включая дефект консьюмера Email (§9), если он подтвердится.

**Фаза 2 — инфраструктура сборки (до смены TFM, работает на 3.1):**

5. Четыре Dockerfile: restore/publish по явному csproj (§10.2). Прогнать e2e — образы должны собираться как раньше.
6. Добавить `Crnc.Oms.Notification.E2ETests` в `Crnc.Oms.Notification.sln` (§10.5). Снова прогнать e2e.

**Фаза 3 — миграция (ветка `8-migrate-notification-service-to-net-10`), по коммиту на юнит:**

7. Чистка группы A из §9 — отдельным коммитом на 3.1, с прогоном e2e.
8. **Email**: TFM + пакеты (§1–2), `Startup.cs` → `Program.cs` (§3, включая появление `AddAuthentication` и порядок `UseRouting`/`UseHttpMetrics`), STJ (§4), MassTransit 8 (§5), базовые образы (§10.1), `ports: "8104:8080"`, таргет `notification-email-api:8080` в `prometheus.yml`, порт в фикстуре.
9. **Push**: то же + `AddSignalR`/`MapHub` (§3, §7), `FrameworkReference` вместо пакета SignalR, `ports: "8107:8080"`, таргет в `prometheus.yml`.
10. **Gateway**: то же + `EndpointConvention` (§5.1), `UserInfoGateway` на `HttpClient` (§6), `ports: "8100:8080"`, таргет в `prometheus.yml`, правка `sales-api` env (§10.4).
11. **Push.Client**: §8 целиком, включая `runtime:10.0` и `…-push-api:8080` в compose.
12. `dotnet build Crnc.Oms.Notification.sln` — до чистой сборки, ноль предупреждений. Ожидаемые точки поломки: неймспейсы MassTransit (`MassTransit.AspNetCoreIntegration`, `MassTransit.ExtensionsDependencyInjectionIntegration`, `IServiceCollectionConfigurator`), `UseSwaggerUi3()`, `Hub<>`/`IHubContext<,>` без `FrameworkReference`, `Microsoft.IdentityModel.Tokens` в трёх `AuthSettings`, API RestSharp.

**Фаза 4 — проверка:**

13. Прогнать e2e-набор — основная автоматическая проверка. Покрывает §4 (camelCase-ключи `ModelState`), §5 (потребление и отправка команд), §7 (доставка пуша через хаб) и сам факт сборки трёх образов на новом SDK.
14. **Ручной сквозной сценарий через `docker-compose up`** (профиль `client`): логин в SPA, смена статуса заказа в Sales → команда доходит до Gateway → Gateway резолвит email в Security → пуш доезжает до колокольчика в SPA и до логов `notification-push-client`. Сверяется с результатом шага 1 фазы 0: поведение должно совпасть один в один.
15. `notification-push-client` в логах: подключился к хабу на .NET 10 (`docker logs crnc-oms-notification-push-client`).
16. Swagger UI всех трёх (`8100`, `8104`, `8107`) — визуально сверить схемы DTO после NSwag 14, особенно `<example>`-блоки из XML-комментариев.
17. Prometheus: три таргета `notification_*_monitoring` снова `UP` (не забыть `docker-compose build prometheus`, §10.4).
18. Обновить AGENTS.md (§12).

**Фаза 5 — после миграции, отдельными коммитами:**

19. Фикс `SendEmailNotificationToReceiverConsumer` (теряются `SenderEmail`/`ReceiverEmail`), с тестом.
20. По желанию — подключение `MonitoringRequestMiddleware` во всех трёх и/или чистка мёртвых `IntegrationEndpoints` из compose. Оба — самостоятельные решения, не хвост миграции.

**Изменений в SPA не требуется**: `PUSH_HUBS_URL` указывает на `localhost:8107`, внешний порт сохраняется, протокол SignalR и имя метода `ReceivePushMessageAsync` не меняются. Подтверждается шагами 13–15, а не предполагается.

---

## 12. Обновление AGENTS.md

Станет неверным после миграции:

- Заголовок «### Backend (mixed: Security, Sales and Production on .NET 10, Notification.* on .NET Core 3.1)» — переписать: весь бэкенд на `net10.0`, упоминание `NETSDK1138` убрать целиком (не останется ни одного 3.1-проекта).
- «`.WebApi` — ASP.NET Core host: `Startup.cs` wires DI…» и оговорка «**Security, Sales and Production are the exception**» — исключение становится правилом: `Startup.cs` в репозитории не остаётся.
- Абзац про MassTransit-версии: 6.x больше нет нигде; все четыре контекста на 8.5.10.
- Описание Notification («three sub-services…») дополнить: у контекста нет БД и домена; `Notification.Push.Client` — четвёртый деплой-юнит с собственным Dockerfile.
- Таблица эндпойнтов: внешние порты не меняются, но стоит отметить, что внутри сети все API теперь слушают 8080.
- «Test coverage today» и «Test conventions»: Notification больше не «has neither kind» — появляются e2e. Описать **четвёртую разновидность конвенции**: один тестовый проект на ограниченный контекст из нескольких деплой-юнитов, поднимающий их все, — и почему это не нарушает правило «настоящая только своя БД» (БД нет, а юниты — один контекст).
- Там же — про **SignalR-клиент внутри тестового процесса** как способ наблюдать доставку пуша, и про то, что **стаб Security намеренно анонимен**, потому что таков настоящий эндпойнт.
- Зафиксировать долги, которые миграция сознательно не закрывает: юнит-тестов домена у Notification нет и быть не может (домена нет); `MonitoringRequestMiddleware` не подключён; §9 группа B.
- Дописать в описание контекста то, что сейчас нигде не зафиксировано: **контракт `SendNotificationToUserCommand` намеренно не несёт параметров канала доставки**, Notification резолвит их сам через анонимные операции чтения Security. Без этой строки любая следующая правка рискует «починить» анонимность или расширить контракт.

---

## Риски и как их ловить

| # | Риск | Как ловить |
|---|---|---|
| 1 | **`aspnet:10.0` слушает 8080, а не 80** — выглядит как «зависший» контейнер. Здесь бьёт по трём сервисам сразу и по четырём внешним потребителям (§10.4) | `docker exec <c> env \| grep ASPNETCORE_HTTP_PORTS`. Живой вызов, который сломается первым, — `notification-push-client` → хаб (шаг 15) |
| 2 | `dotnet publish -o` по solution → `NETSDK1194` в четырёх Dockerfile | Гарантированно ловится любой сборкой образа. Фикс — §10.2, до смены базовых образов |
| 3 | **Удаление `Microsoft.AspNetCore.SignalR` 1.1.0 без `FrameworkReference`** роняет сборку `Push.Integration` | Компилятором мгновенно (`Hub<>`, `IHubContext<,>` не найдутся). Фикс — §7 |
| 4 | **JWT через query string для хаба перестанет работать** (`OnMessageReceived`) — SPA и `Push.Client` не подключатся | Главный push-тест набора + шаг 15. Код переносится дословно, вероятность низкая |
| 5 | **`UseCors` после `MapHub`** в minimal hosting — SPA получает CORS-ошибку при подключении к хабу, а `Push.Client` (без CORS) при этом работает | Только браузером: ручная проверка SPA на шаге 14. Тестами не покрывается — тестовый SignalR-клиент CORS не проверяет |
| 6 | `UseSwaggerUi3()` переименован в NSwag 14; `IServiceCollectionConfigurator` и `MassTransit.AspNetCoreIntegration` в 8.x отсутствуют | Ловится компилятором мгновенно |
| 7 | **`DictionaryKeyPolicy` забыт** → ключи `ModelState` молча становятся PascalCase | Здесь **ловится тестом** (в отличие от Production): в Email и Gateway есть `[Required]`/`[EmailAddress]`, ответы 400 наблюдаемы (§4) |
| 8 | **Поведение Email после добавления `AddAuthentication`.** На 3.1 измерено (фаза 0): `UseAuthentication()` без `AddAuthentication` безвреден, `POST /api/emailNotifications` отдаёт 200. На .NET 10 оставлять так нельзя, но добавление схемы — изменение, а не восстановление статус-кво | E2E-тест на HTTP-вход Email, зафиксированный на baseline как 200 без токена. Если после §3 он станет 401 — значит схема добавлена неверно (эндпойнт анонимен и должен таким остаться) |
| 9 | **Смена TFM `Messaging.Contract` с `netcoreapp3.1` на `netstandard2.0`** ломает что-то неожиданное | Компилятором. Содержимое — три интерфейса без зависимостей, риск близок к нулю; парная копия в Gateway уже `netstandard2.0` и собирается |
| 10 | **Порядок `UseHttpMetrics`/`UseRouting` в Email** — метрики без маршрута или пустая метка `endpoint` в prometheus-net 8 | Шаг 17 (таргет `UP`) + сравнение вывода `/metrics` с Gateway. Фикс — §3 |
| 11 | Сужение толерантности входа: STJ строже Newtonsoft | Осознанное изменение (§4). Внешних потребителей у трёх HTTP-API нет |
| 12 | **Кто-нибудь «починит» анонимность** `GET /api/users/{id}` в Security, не зная, что на ней держится разрешение канала доставки | Тестами Notification не ловится — стаб отвечает за Security. Защита организационная: §12 требует записать причину в AGENTS.md, а комментарий в `UserInfoGateway` — рядом с вызовом |
| 13 | Один тестовый набор поднимает три образа + RabbitMQ + WireMock — прогон долгий и потенциально флакающий | Фикстура одна на всю коллекцию, образы собираются один раз; ожидания по шине — дельтами с таймаутом (как в Sales). Если станет больно, резать по коллекциям, а не по проектам |
| 14 | Чистка namespace-мусора (§9, группа A) задевает `using`-и в шести файлах и смешивается с миграцией | Отдельный коммит на 3.1 до смены TFM + прогон e2e. Тот же приём, что со `Phone.cs` в Production |

---

## Критичные файлы

- `Crnc.Oms.Notification.{Gateway,Email,Push}.WebApi/Startup.cs` (удалить) и `Program.cs` (переписать, §3)
- 10 мигрируемых `.csproj` + 2 контрактных (`Email`/`Push.Messaging.Contract`, `netcoreapp3.1` → `netstandard2.0`)
- `Crnc.Oms.Notification.Push.Integration.csproj` — пакет SignalR → `FrameworkReference` (§7)
- `Crnc.Oms.Notification.Gateway.Integration/Gateways/UserInfoGateway.cs` — RestSharp → `HttpClient` (§6), **без добавления токена** (раздел «Разрешение канала доставки»)
- `Crnc.Oms.Notification.Gateway.Integration/Gateways/{EmailGateway,PushGateway}.cs` — **удалить** (мёртвый код, §9)
- `Crnc.Oms.Notification.Push.Client/Auth/AuthClient.cs` — RestSharp → `HttpClient` (§6, §8)
- `Crnc.Oms.Notification.Email.WebApi/Consumers/SendEmailNotificationToReceiverConsumer.cs` — теряет адресатов, **фаза 5** (§9)
- 4 × `Dockerfile` (§10.1, §10.2) и 4 новых `.dockerignore` (§10.3)
- `Crnc.Oms.Notification.sln` (§10.5)
- `docker-compose.yml` — три `ports` на 8080 и три env-строки с адресами (§10.4)
- `prometheus/prometheus.yml` — три таргета на `:8080` (§10.4)
- `AGENTS.md` (§12) — зафиксировать замысел узкого контракта уведомления

Новые файлы набора e2e (фаза 1), по образцу `Crnc.Oms.Sales.E2ETests`:

- `Crnc.Oms.Notification.E2ETests.csproj` — `net10.0`, xunit 2.9.3 / runner 3.1.4 / Test.Sdk 17.14.1 / FluentAssertions 7.2.2 / Testcontainers 4.14.0 / Microsoft.IdentityModel.JsonWebTokens 8.22.0 / **Microsoft.AspNetCore.SignalR.Client 10.0.11**. `ProjectReference` — ни одного (контракты сообщений в тестах не нужны: команды кладутся в очереди через management API, как в наборе Sales)
- `NotificationApiFixture.cs` — сеть, RabbitMQ, WireMock, три образа из настоящих Dockerfile, шпионские очереди
- `TestJwt.cs`, `RabbitMqAdmin.cs`, `WireMockAdmin.cs` — перенос из набора Sales
- `TestModels.cs` — DTO запросов/ответов как `record`, `SeedData`, `JsonDefaults.Options`
- `GatewayNotificationsTests.cs`, `EmailNotificationsTests.cs`, `PushNotificationsTests.cs`

## Статус

**Фаза 0 выполнена 2026-08-20.** План составлен по итогам инвентаризации кода и сборки baseline (`dotnet build Crnc.Oms.Notification.sln` на SDK 10.0.100 — 0 ошибок, 28 предупреждений), затем все три предположения проверены на живом стенде: `docker-compose --profile sales --profile notification up`, девять контейнеров, RabbitMQ и обе БД настоящие.

### Что измерено

**1. Цепочка уведомлений работает целиком — и по HTTP-входу, и по входу из шины.**

Прямой вызов `POST :8100/api/notifications/user` с токеном `admin` вернул 200. По логам Gateway: сходил в Security за карточкой `shon_bean`, получил её, отправил обе команды. Полный round-trip через Sales — создание заказа и переход `Not sent → Need signoff` — дал то же самое: `MessageBrokerNotificationGateway` в Sales отправил две команды (обоим Main manager'ам), Gateway их съел, резолвил обоих пользователей и развёл по двум каналам. `notification-push-client`, залогиненный как `shon_bean`, получил ровно своё сообщение и не получил чужое — адресация SignalR по пользователю работает.

Итоговое состояние очередей: четыре штуки, все пустые, **ни одной `_error`/`_skipped`**.

| Очередь | messages | ack |
|---|---|---|
| `sendNotificationToUser` | 0 | 2 |
| `sendEmailNotificationToReceiver` | 0 | 3 |
| `sendPushNotificationToReceiver` | 0 | 3 |
| `jobCreatedForOrder` | 0 | — |

Заодно окончательно снят вопрос §«Фаза 0» про адресацию: Sales логирует `SEND rabbitmq://message-broker/commands/sendNotificationToUser`, а `ReceiveEndpoint("sendNotificationToUser")` Gateway это принимает. Сегмент `commands` действительно поглощается адресацией, и пара MassTransit 8 (Sales) → MassTransit 6 (Notification) работает вживую.

**2. HTTP-вход Email работает — `AddAuthentication` не нужен на 3.1.**

`POST :8104/api/emailNotifications` с валидным телом → 200 и `{"messageId":"…"}`. Контейнер поднимается штатно, `UseAuthentication()` без `AddAuthentication` пайплайн не роняет. Ожидание из плана было пессимистичнее реальности; на .NET 10 проверять всё равно придётся, но это уже не «сервис не поднимется», а «поведение может измениться» — риск 8 переформулирован.

**Побочно измерен baseline §4**, и он именно тот, ради которого нужен `DictionaryKeyPolicy`. Тело без адресатов даёт 400 с ключами в camelCase:

```json
{"senderEmail":["The SenderEmail field is required."],
 "receiverEmail":["The ReceiverEmail field is required."]}
```

**3. `/health` — 404 на всех трёх** (`:8100`, `:8104`, `:8107`), как и предполагалось: `AddHealthChecks()` есть, `MapHealthChecks` нет. Решение 13 в силе.

### Дефект консьюмера Email подтверждён прямым наблюдением

§9, группа B — теперь это не подозрение, а измеренный факт. Две строки из лога `notification-email-api`, одно и то же поле:

```
Email sent in EmailService with id 1111…, sender : notifications@crnc.ru to receiver shon_bean@crnc.com, message: phase0 direct http
Email sent in EmailService with id d4a4…, sender :  to receiver , message: Status of order 0e223e3a changed from Not sent to Need signoff…
```

Первая строка — прямой HTTP, адресаты на месте. Вторая — тот же сервис через шину, адресаты пустые. Gateway их заполняет (в его логе `receiverEmail: shon_bean@crnc.com`), контракт их несёт, теряет их `SendEmailNotificationToReceiverConsumer`, который копирует в `SendEmailMessageInputDto` только `Message`.

---

## Фазы 1–4 выполнены 2026-08-21

**Набор e2e** — `Crnc.Oms.Notification.E2ETests`, 15 тестов, зелёные и на baseline `netcoreapp3.1`, и после каждого из четырёх миграционных коммитов. Три сервиса поднимаются разом, Security заменён WireMock'ом, роль `push-client` играет SignalR-клиент внутри тестового процесса.

**Весь контекст на `net10.0`.** `dotnet build Crnc.Oms.Notification.sln` — 0 ошибок, **0 предупреждений** (было 28). `netcoreapp3.1` в репозитории не осталось.

### Что подтвердилось на живом прогоне и было в плане только предположением

- **Конверт MassTransit можно собрать руками.** Перед тем как писать набор, формат проверен на живом стенде: JSON-конверт с `messageType` и content-type `application/vnd.masstransit+json`, отправленный через management API, MassTransit 6 принимает и обрабатывает. Это сняло необходимость и в собственной шине в тестах (путь Production), и в `ProjectReference` на контракты — набор Notification обошёлся без обоих.
- **Считать сообщения дельтами нельзя.** Первый прогон упал именно на этом: `Expected pushAfter to be 1, but found 2` — опоздавшее сообщение соседнего теста сдвинуло счётчик. Переписано на поиск своего сообщения по уникальному маркеру с вычиткой очереди; после этого набор стабилен. Это уточняет конвенцию Sales, где дельты пока работают только потому, что тестов меньше.
- **`UseAuthentication()` без `AddAuthentication` на 3.1 безвреден** (§ фазы 0), а добавление схемы в §3 не сделало анонимный эндпойнт Email закрытым — тест на 200 без токена зелёный до и после.
- **`FrameworkReference` вместо пакета SignalR 1.1.0 сработал с первого раза**, но потянул за собой `NU1510`: `Microsoft.Extensions.Logging.Abstractions` в `Push.Integration` стал лишним, потому что приходит из того же shared framework. Убран — иначе ноля предупреждений не добиться.
- **`/health` теперь не формальность.** MassTransit 8 регистрирует health-check шины, и в первые секунды после старта Email честно отдавал 503 `Unhealthy`, пока шина подключалась, а затем 200 `Healthy`. На 3.1 эндпойнта не было вовсе.
- **Ручной сквозной сценарий пройден полностью** (шаг 14): заказ создан в Sales, статус `Not sent → Need signoff`, две команды ушли в шину, Gateway обе съел, резолвил обоих Main manager'ов в Security **без токена** через новый `HttpClient`, развёл по двум каналам; `notification-push-client` получил свой пуш через SignalR на .NET 10. Ни одной `_error`-очереди.
- **Prometheus** (шаг 17): три таргета `notification_*_monitoring` — `up`. Пересборка образа `prometheus` понадобилась, как и предупреждал §10.4.
- **Swagger** (шаг 16): NSwag 14 отдаёт `openapi: 3.0.0`, схемы всех трёх DTO на месте, свойства camelCase. Регрессий от смены генератора нет.

### Что не проверялось

SPA в браузере не открывалась — её сборка падает на предсуществующей проблеме `yarn: not found` в `crnc-oms-ui`, как и при проверке миграций Sales и Production. Порядок `UseCors` до `MapHub` (риск 5) тем самым остался непроверенным вживую: тестовый SignalR-клиент CORS не проверяет. Внешний порт 8107 и `PUSH_HUBS_URL` не менялись, так что риск ограничен именно порядком middleware.

### Следующий шаг

Фаза 5 — фикс `SendEmailNotificationToReceiverConsumer`, теряющего адресатов.
