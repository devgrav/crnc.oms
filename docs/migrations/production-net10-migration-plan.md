# Миграция Crnc.Oms.Production на .NET 10

## Контекст

Третий шаг перевода CRNC OMS на современный стек. Первые два — [Security](security-net10-migration-plan.md) и [Sales](sales-net10-migration-plan.md) — выполнены и служат шаблоном: e2e-тесты как страховка → правки инфраструктуры сборки → смена TFM и пакетов → minimal hosting → System.Text.Json → проверка тем же набором тестов.

Production технологически почти совпадает с Sales (EF Core + PostgreSQL, MassTransit + RabbitMQ, NSwag, prometheus-net, JWT), но заметно проще:

- **Нет ни одного исходящего HTTP-вызова.** `RestSharp` в сервисе отсутствует, единственный внешний шлюз (`ISalesOrderGateway`) — это `IPublishEndpoint.Publish`. Целого раздела «RestSharp → HttpClient» здесь не будет, и заглушка Security в тестах не нужна.
- **Нет CQRS-слоя.** Вместо `ICommandQueryDispatcher`/MediatR — обычный `JobService` за интерфейсом. MediatR в Production присутствует, но полностью мёртв (см. решение 5).
- **Owned-типы плоские.** Один `OwnsOne(x => x.Manager)` с двумя строковыми свойствами вместо трёхуровневой вложенности `Order → Customer → Title → NameAbbreviation` из Sales. Каскад `Navigation(...).IsRequired()`, который всплыл при миграции Sales, здесь по построению неприменим.
- **Совместимость шины перестала быть риском.** На миграции Sales эмпирически проверено, что MassTransit 8 (System.Text.Json) и MassTransit 6 разговаривают друг с другом. Более того, Production обменивается сообщениями **только с Sales**, а Sales уже на MassTransit 8.5.10 — после этой миграции обе стороны окажутся на одной ветке. Notification.* остаются на MassTransit 6.2.1, но Production до них не доходит.

Главное же отличие — **в e2e-тестах, а не в миграции**. У Sales вход был HTTP, а шина только на выходе, поэтому набор ограничился «сообщение легло в очередь». У Production основной сценарий — *входящий*: работа создаётся исключительно консьюмером `OrderConvertedToJobEvent`, HTTP-эндпоинта создания нет вовсе. Значит тест обязан уметь публиковать сообщение в конверте MassTransit — это и есть главное проектное решение набора (см. решение 8 и раздел «Пререквизит»).

Все факты ниже перепроверены чтением реальных файлов; версии пакетов взяты из уже мигрированного Sales (проверены на nuget.org 2026-08-19).

### Согласованные решения

1. **Целевая версия**: .NET 10 (LTS), как у Security и Sales.
2. **Hosting model**: слить `Startup.cs` в `Program.cs` (minimal hosting).
3. **JSON**: полностью уйти с `Newtonsoft.Json` на `System.Text.Json`.
4. **MassTransit — ветка 8.x (8.5.10), не 9.x.** Обоснование лицензией — в §1 плана Sales: `MassTransit 8.5.10` — `Apache-2.0`, `9.2.0` — проприетарная. 8.5.10 таргетит `net10.0`.
5. **MediatR — не обновлять, а удалить целиком.** В Production он используется ровно в одном месте: `DomainEvent : INotification`. При этом `IDomainEventDispatcher` не имеет ни одной реализации, `AddMediatR` нигде не вызывается, ни один агрегат не вызывает `AddDomainEvent`, доменных событий в коде нет. Обновлять до 12.5.0 нечего — правильный ход убрать пакет и наследование. Это снимает лицензионный вопрос и делает `Crnc.Oms.Production.Domain` по-настоящему framework-free (в отличие от Sales, где MediatR несущий).
6. **`<Nullable>` не включаем** — как в Security и Sales, отложено.
7. **Контракты сообщений не трогаем.** `Crnc.Oms.Production.Messaging.Contract` остаётся на `netstandard2.0` — он парная копия контракта Sales, смена TFM тут ничего не даёт.
8. **E2E-набор использует MassTransit 8 как клиент шины, а контракт подключает через `ProjectReference` на `Crnc.Oms.Production.Messaging.Contract`** — без локальных переобъявлений интерфейсов. Обоснование, цена и альтернативы — в разделе «Пререквизит».
9. **PostgreSQL в docker-compose поднять с 9.6.17 до 18.6** — тем же решением и по тем же причинам, что для `sales-db` (§5.4 плана Sales): 9.6 EOL с ноября 2021 и вне окна поддержки Npgsql. Данные не мигрируем — `ProductionDbInitializer` делает `EnsureDeleted()`/`EnsureCreated()` на каждом старте.
10. **Предсуществующие баги (§7) чиним отдельными коммитами**, а не внутри коммита миграции, и только те, что перечислены явно. Поведение, которое e2e-тесты фиксируют как текущее (500 вместо 404), меняем **после** миграции, чтобы baseline оставался baseline'ом.

---

## Пререквизит: e2e-тесты Production

По схеме Security и Sales: сначала набор e2e-тестов, зелёный на текущем `netcoreapp3.1`-сервисе, потом миграция под его защитой. Конвенции — в AGENTS.md, раздел «Test conventions»; проект `Crnc.Oms.Production.E2ETests` на `net10.0`, поверх Testcontainers, без ссылок на код сервиса — с единственным исключением в виде `Messaging.Contract`, см. «Как тест публикует и ловит сообщения».

Блокера уровня «сначала почини `master`» здесь нет: `Auth:JwtBase64SymmetricKey` в `Crnc.Oms.Production.WebApi/appsettings.json` уже равен ключу Security (`ldtLvqRHfPc8UW0My3jOKr0imGUjZjsNGVYSWn4NdCY=`) — блокер №0 плана Sales закрыт его фазой 0.

### Периметр: настоящие только БД и брокер

- **PostgreSQL — настоящий.** Это то, что здесь надо проверять по-настоящему: EF-маппинг, owned-тип `Manager`, тип колонки под `DateCreated`. Ровно тот слой, куда бьёт риск 1.
- **RabbitMQ — настоящий**, и в отличие от Sales он не только приёмник, но и источник: без публикации `OrderConvertedToJobEvent` в Production нельзя создать работу вообще.
- **Sales не поднимается.** Его роль в обе стороны играет сам тест: он публикует `OrderConvertedToJobEvent` вместо Sales и потребляет `JobCreatedForOrderEvent` вместо Sales.
- **Security не поднимается и не нужен** — Production в него не ходит (проверено: ни `HttpClient`, ни `RestSharp` в сервисе нет). WireMock-контейнера, в отличие от набора Sales, здесь не будет.

### Контейнеры фикстуры

| Контейнер | Алиас в сети | Зачем |
|---|---|---|
| `postgres:18.6` | `production-db` | настоящая БД Production |
| `rabbitmq:3-management` | `message-broker` | шина; AMQP-порт 5672 и management 15672 пробрасываются на хост |
| образ из `Crnc.Oms.Production/Dockerfile` | — | тестируемый сервис |

Тег RabbitMQ держим равным `docker-compose.yml` (`3-management`).

Про `postgres:18.6` на фазе 1: compose на этот момент ещё держит `production-db` на `9.6.17`, то есть фикстура временно «впереди» compose. Так же было сделано в наборе Sales, и там EF Core 3.1 / Npgsql 4.1 против PostgreSQL 18 отработали без единой правки (19 зелёных тестов на `netcoreapp3.1`). Берём 18.6 сразу, чтобы после фазы 3 ничего не менять; если против ожидания всплывут ошибки аутентификации или SQL — временно откатить тег фикстуры на `9.6.17` и вернуть на шаге 12.

### Аутентификация: тест подписывает токен сам

Как в Sales: Production токены только **валидирует**, выдаёт их Security, которого в наборе нет. Фикстура генерирует HS256-токен сама, с `iss`/`aud` из `Auth` и короткими клеймами (`nameid`/`unique_name`/`given_name`/`family_name`/`email`/`role`) — ровно так их кладёт на провод Security. Ключ задаётся контейнеру через переменную окружения `Auth:JwtBase64SymmetricKey`, а не берётся из `appsettings.json`, чтобы набор пережил любую ротацию.

`TestJwt.cs` из `Crnc.Oms.Sales.E2ETests` переносится почти дословно (меняется неймспейс и, при желании, значение тестового ключа).

Отдельно: `JobsController` помечен просто `[Authorize]` — проверок ролей в Production нет ни одной. Значит тестов «wrong role → 403», которые есть в наборе Security, здесь не будет; остаются только «нет токена → 401».

### Как тест публикует и ловит сообщения — главное решение набора

Работу в Production нельзя создать по HTTP. Единственный вход — `OrderConvertedToJobConsumer`, то есть тест обязан положить в брокер валидный конверт MassTransit. Два способа:

**Выбранный: MassTransit 8 как клиент + `ProjectReference` на контракт.** В `Crnc.Oms.Production.E2ETests` добавляются `PackageReference` на `MassTransit` и `MassTransit.RabbitMQ` (8.5.10) — они нужны, чтобы поднять бус, — и `ProjectReference` на `Crnc.Oms.Production.Messaging.Contract`, откуда берутся `OrderConvertedToJobEvent` и `JobCreatedForOrderEvent`.

Ссылка на контракт лучше локальных переобъявлений сразу по трём причинам:

- **Ничего не расходится молча.** Локальная копия интерфейса живёт своей жизнью: поменяли поле в контракте — тест продолжает компилироваться и публиковать сообщение старой формы, которое сервис просто не разберёт. С `ProjectReference` это ошибка компиляции.
- **Не нужно объявлять типы в чужом неймспейсе.** `urn:message:` строится из полного имени типа, поэтому локальная копия обязана лежать в `Crnc.Oms.Messaging.Contract.Events` — неймспейсе, который тестовому проекту не принадлежит. Со ссылкой вопрос снимается: `RootNamespace` контракта и так `Crnc.Oms.Messaging.Contract`.
- **Ничего не стоит по совместимости.** `Crnc.Oms.Production.Messaging.Contract` — `netstandard2.0`, ноль `PackageReference`, два файла с интерфейсами. Из `net10.0` потребляется без оговорок, и решение 7 держит его на `netstandard2.0` навсегда — значит ссылка переживает миграцию нетронутой.

**Цену называем честно:** AGENTS.md обещает, что e2e-набор «carries no `ProjectReference` to the service» и «drives the running API purely over HTTP». Формально ссылка на контракт этого не нарушает — контракт не сервис, а общий артефакт по определению, и TFM тестов он не ограничивает. Фактически — набор перестаёт быть «чисто HTTP» и получает первую ссылку внутрь дерева сервиса. Это осознанный размен, а не техническая формальность; в §10 он документируется, а не заметается.

Что эта ссылка **не** проверяет: Sales и Production держат парные копии контракта (`Crnc.Oms.Sales.Messaging.Contract` и `Crnc.Oms.Production.Messaging.Contract`, у обоих `RootNamespace = Crnc.Oms.Messaging.Contract`). Тест ссылается на копию Production, то есть сверяет сервис с его собственным представлением о контракте и расхождение между копиями поймать не может. На момент написания плана копии сверены и семантически идентичны (различие — один лишний пробел и перевод строки в `JobCreatedForOrderEvent.cs`). Ссылаться на копию Sales было бы строже, но это ссылка из дерева одного сервиса в дерево другого — цена выше выигрыша; расхождение копий, если оно кого-то беспокоит, ловится отдельной проверкой, а не e2e-набором.

Почему кросс-версионность безопасна на фазе 1, когда сервис ещё на MassTransit 6.2.4: она уже проверена на живом round-trip миграции Sales — MT8 публикует → MT6 Production потребляет, и обратно (Sales MT8 потреблял `JobCreatedForOrderEvent` от Production MT6). Тестовый клиент повторяет ровно тот путь, который уже работает в `master`. Побочный плюс: на baseline тест и сервис действительно на разных версиях библиотеки, то есть конверт проверяется по-настоящему. После миграции обе стороны окажутся на 8.5.10, и это свойство пропадёт — тест и предмет проверки поедут на одной библиотеке. Смириться с этим приходится: альтернатива — собирать конверт руками (см. ниже).

Ответное событие ловится тем же бусом: в конфигурации поднимается временный (безымянный, auto-delete) receive endpoint с консьюмером `JobCreatedForOrderEvent`, который складывает сообщения в потокобезопасную коллекцию фикстуры. Тесты ждут появления сообщения **с нужным `OrderId`**, а не «последнего» — фикстура одна на всю коллекцию, порядок тестов не гарантирован.

**Запасной вариант, если бус в тестах окажется неудобен:** публиковать конверт руками через management API RabbitMQ (`POST /api/exchanges/%2f/{exchange}/publish` с `content_type: application/vnd.masstransit+json`, полями `messageId`, `messageType: ["urn:message:…"]`, `message: {…}`), а входящее событие ловить «шпионской» очередью — `RabbitMqAdmin.EnsureSpyQueueAsync` из `Crnc.Oms.Sales.E2ETests` переносится без изменений. Способ не требует ни одного пакета, но конверт придётся собирать и поддерживать вручную.

Бус фикстуры стартует **после** контейнера RabbitMQ и **до** старта сервиса, чтобы не потерять первое же событие.

### Что покрываем

Чтение и запись по HTTP:

- `GET /api/jobs` — сид-работа `f425e777-1d53-40d3-99dd-d51e1a72fafa` из `ProductionDbInitializer` как известная точка опоры; `manager` склеен как `"Shon Bean (shon_bean)"`, `priority`/`jobType`/`materialSource` — строки-`Description` (`"Low"`, `"New"`, `"Included by customer"`).
- `GET /api/jobs/{id}` — включая список `priorities` из `TextValueDto<int,string>`: дженерик-DTO, тот же кандидат на расхождение схемы NSwag 14, что `TextValueOutputDto` в Sales.
- `GET /api/jobs/{unknownId}` → 404.
- `PUT /api/jobs/{id}/finished` → 200, и `isJobCompeted: true` при повторном чтении.
- `PUT /api/jobs/{id}/priority` с телом `{"priority": 1}` → 200, и `priorityEnum: 1`, `priority: "High"` при повторном чтении. Это же единственная точка **входа** enum'а числом.
- 401 без токена на каждом из четырёх маршрутов.

Прицельно под риски миграции:

- **Даты** — под риск 1 (Npgsql/`timestamptz`). Работа, созданная консьюмером через `DateTime.Now`, должна записаться и прочитаться обратно. **Внимание при написании теста:** `DateTimeExtensions` в Production форматирует как `"dd.MM.yyy hh:mm:ss"` — с тремя `y` и 12-часовым `hh` без AM/PM (то есть 13:05 выводится как `01:05`). Это предсуществующее поведение, менять его нельзя; assert'ы пишутся по факту, а не по «ожидаемому» `dd.MM.yyyy HH:mm:ss`.
- **Enum'ы числами** — под §4: `priorityEnum` в JSON число, не строка. Тест-страховка от случайно скопированного из плана Security `JsonStringEnumConverter`.
- **Owned-тип `Manager`** — под §5.2: `manager` не должен приехать пустым или `null` после смены версии EF Core.

Через шину:

- `OrderConvertedToJobEvent` с новым `OrderId` → работа появилась и читается по HTTP; `jobType`/`materialSource` разобраны консьюмером через `Enum.Parse` корректно.
- Та же публикация → в ответ пришёл `JobCreatedForOrderEvent` с этим же `OrderId` и с `JobId`, равным `OrderId` (`JobService.CreateJob` использует `dto.OrderId` в качестве id работы — квирк, но фиксируем как есть).
- Повторная публикация с тем же `OrderId` → вторая работа не создаётся и **второго `JobCreatedForOrderEvent` не приходит** (ранний `return` в `CreateJob`). Реальная идемпотентность, которую стоит закрепить.

Фиксация предсуществующего поведения (не «правильного», а текущего):

- `PUT /api/jobs/{unknownId}/finished` и `.../priority` → **500, а не 404**: `JobService.FinishJob`/`ChangePriority` не проверяют результат `FindByIdAsync` и падают с `NullReferenceException`, хотя контроллер декларирует `[ProducesResponseType(404)]` и ловит `MissingEntityException`, которую никто не бросает. Тест пишется на 500 со ссылкой на §7 этого плана — иначе baseline перестанет быть baseline'ом. Починка — отдельным шагом **после** миграции.

### Что сознательно НЕ покрываем

- **Сквозной сценарий Sales → Production → Sales.** Вне периметра: Sales не поднимается, его роль играет тест. Реальный round-trip проверяется вручную на `docker-compose up` (шаг 15 в §9).
- **Notification и Push.** Production до них не доходит вовсе.
- **camelCase-ключи ошибок валидации.** В наборах Security и Sales такой тест был. Здесь он **неприменим**: в контрактах Production нет ни одного атрибута `System.ComponentModel.DataAnnotations` (проверено grep'ом), а значит нет и автоматического 400 с ключами-именами свойств. Единственные ключи `ModelState`, до которых можно дотянуться, — это имена параметров маршрута и JSON-пути, и они лежат в нижнем регистре независимо от `DictionaryKeyPolicy`. Настройку в §4 всё равно ставим — ради паритета API с двумя другими сервисами, — но честно фиксируем: тестом она здесь не закрывается.
- **`/health` и `/metrics`.** Можно добавить дешёвый smoke-тест, но ни один из них не менялся при миграциях Security и Sales; необязательно.

Baseline фиксируется на `netcoreapp3.1` до любых правок сервиса.

---

## Инвентаризация (подтверждено чтением файлов)

Шесть проектов, все на `netcoreapp3.1`, кроме `Messaging.Contract` (`netstandard2.0`). `Nullable`/`LangVersion` нигде не заданы. Тестовых проектов нет ни одного — ни юнитов, ни e2e.

| Проект | TFM | Пакеты сейчас |
|---|---|---|
| Domain | `netcoreapp3.1` | `MediatR 8.0.0` |
| Application | `netcoreapp3.1` | нет |
| DataAccess | `netcoreapp3.1` | `EFCore.NamingConventions 1.0.0`, `Microsoft.EntityFrameworkCore 3.1.0`, `Microsoft.EntityFrameworkCore.Proxies 3.1.0`, `Npgsql.EntityFrameworkCore.PostgreSQL 3.1.0` |
| Integration | `netcoreapp3.1` | `MassTransit 6.2.1` |
| Messaging.Contract | `netstandard2.0` | нет |
| WebApi | `netcoreapp3.1` | `MassTransit.AspNetCore 6.2.4`, `MassTransit.Extensions.DependencyInjection 6.2.4`, `MassTransit.Extensions.Logging 5.5.6`, `MassTransit.RabbitMQ 6.2.4`, `Microsoft.AspNetCore.Authentication.JwtBearer 3.1.1`, `Microsoft.AspNetCore.Diagnostics 2.2.0`, `Microsoft.AspNetCore.Diagnostics.HealthChecks 2.2.0`, `Microsoft.AspNetCore.Mvc.NewtonsoftJson 3.1.1`, `Microsoft.EntityFrameworkCore 3.1.1`, `Microsoft.IdentityModel.Tokens 5.6.0`, `Npgsql.EntityFrameworkCore.PostgreSQL 3.1.1.2`, `NSwag.AspNetCore 13.2.3`, `prometheus-net.AspNetCore 3.5.0` |

**Важные проверенные факты:**

- **Исходящих HTTP-вызовов нет вообще.** `grep` по `RestSharp|HttpClient|IRestClient` в сервисе пуст. Единственный внешний шлюз — `SalesOrderGateway` через `IPublishEndpoint`. Раздела «RestSharp → HttpClient» в этой миграции нет.
- **Ни одного Newtonsoft-специфичного атрибута.** Все упоминания `Newtonsoft` — три строки в `Startup.cs` (`using Newtonsoft.Json.Serialization`, `using Newtonsoft.Json.Converters`, `CamelCasePropertyNamesContractResolver`). Причём `Newtonsoft.Json.Converters` не используется — `StringEnumConverter` не зарегистрирован, как и в Sales.
- **Enum'ы в контрактах живые и числовые**: `Priority PriorityEnum` в `JobListItemDto` и `GetJobDto`, `Priority Priority` в `ChangePriorityDto`. SPA (`components/jobs/priority.ts` — `enum Priority { High = 1, Middle = 2, Low = 3 }`, `JobsGridRowModel.priorityEnum`) ожидает именно числа. `JsonStringEnumConverter` добавлять **нельзя**.
- **Атрибутов валидации нет ни одного** (`DataAnnotations` не подключён нигде). Следствие для §4 и для набора тестов описано выше.
- **MediatR полностью мёртв**: единственное использование — `DomainEvent : INotification`; `IDomainEventDispatcher` без реализаций, `AddMediatR` не вызывается, `AddDomainEvent` не вызывается ни разу, доменных событий нет. `using MediatR;` в `Startup.cs` не используется.
- **Все даты — `DateTime.Now`/`DateTime.Today`, то есть `Kind=Local`**: `CurrentDateTimeProvider`, `DomainEvent.CreatedDate`, `ProductionDbInitializer`. Персистится одно свойство — `Job.DateCreated`. Прямой вход в риск 1.
- **`EFCore.NamingConventions` и `Microsoft.EntityFrameworkCore.Proxies` не используются**: единственное упоминание — закомментированный `//optionsBuilder.UseSnakeCaseNamingConvention();` в `ProductionDataContext.cs`; `UseLazyLoadingProxies` не вызывается, `public virtual` в Domain нет. Оба пакета удалить, `OnConfiguring` удалить целиком.
- **Owned-тип ровно один и плоский**: `builder.OwnsOne(x => x.Manager, …)` с двумя `string`. Вложенных владений нет, поэтому каскад `Navigation(...).IsRequired()`, который потребовался в Sales, здесь неприменим — ожидается только безвредный `OptionalDependentWithoutIdentifyingPropertyWarning` на листовом dependent.
- **`ICurrentUserContext` зарегистрирован, но никуда не инжектится** (`JobService` его не принимает). Реализация в конструкторе разыменовывает `httpContextAccessor.HttpContext.Request` — то есть при резолве вне HTTP-запроса (например, в scope консьюмера) упала бы с `NullReferenceException`. Сейчас это не проявляется; при переносе регистрации в `Program.cs` ничего не меняем, но факт держим в голове.
- **`Phone.cs` — мёртвый код с чужим неймспейсом.** Лежит в `Aggregates/JobAggregate/`, но объявляет `namespace Crnc.Oms.Production.Domain.Aggregates.Order`, и это **единственный** тип в том неймспейсе. Сам `Phone` не используется нигде. При этом шесть файлов тащат `using Crnc.Oms.Production.Domain.Aggregates.Order;` — удаление `Phone.cs` без вычистки этих `using` даст шесть `CS0246`.
- **`Crnc.Oms.Production.Application.csproj` дважды ссылается на `Domain`** — дублирующийся `ProjectReference`.
- **`EnumHelper.ToDictionaryWithKeysAndDescriptions(List<object> e)`** — мёртвая перегрузка, вызывающая `Enum.GetValues(e.GetType())` для `List<object>`; при вызове упала бы в рантайме. Не вызывается.
- **`DateTimeExtensions` форматирует как `"dd.MM.yyy"` / `"dd.MM.yyy hh:mm:ss"`** — три `y` и 12-часовой `hh`. Предсуществующее поведение, миграция его не меняет; тесты пишутся по факту.
- `Crnc.Oms.Production/Dockerfile` — точная копия дореформенного Security'шного и Sales'ового, включая `dotnet restore` и `dotnet publish -c Release -o out` без явного таргета (см. §8.2). `.dockerignore` отсутствует.
- `Crnc.Oms.Production.sln` содержит все 6 проектов сервиса (в отличие от Sales, где тестовых не было — здесь их просто не существует).
- `docker-compose.yml`: `production-api` — `ports: "8098:80"`, `production-db` — `postgres:9.6.17` (порт хоста 5434). `prometheus/prometheus.yml` — таргет `production-api` без порта.
- **`appsettings.json` содержит `Port=5433` вместо `5434`** в локальной (не-Docker) строке подключения — известная опечатка, скопированная из Sales, зафиксирована в AGENTS.md. Теперь она в скоупе — чиним (§7).
- `Auth:JwtBase64SymmetricKey` уже выровнен с Security. `docker-compose.yml` секцию `Auth` не переопределяет.
- Локально стоят SDK 8.0.404 / 9.0.101 / **10.0.100**.

---

## 1. Версии пакетов

| Пакет | Проект(ы) | Было | Станет |
|---|---|---|---|
| `MediatR` | Domain | 8.0.0 | **удалить** — решение 5, вместе с `DomainEvent : INotification` |
| `Microsoft.EntityFrameworkCore` | DataAccess, WebApi | 3.1.0 / 3.1.1 | **10.0.11** |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | DataAccess, WebApi | 3.1.0 / 3.1.1.2 | **10.0.3** |
| `EFCore.NamingConventions` | DataAccess | 1.0.0 | **удалить** — не используется |
| `Microsoft.EntityFrameworkCore.Proxies` | DataAccess | 3.1.0 | **удалить** — не используется |
| `MassTransit` | Integration | 6.2.1 | **8.5.10** |
| `MassTransit.RabbitMQ` | WebApi | 6.2.4 | **8.5.10** |
| `MassTransit.AspNetCore` | WebApi | 6.2.4 | **удалить** — в 8.x хостинг встроен в `AddMassTransit` |
| `MassTransit.Extensions.DependencyInjection` | WebApi | 6.2.4 | **удалить** — влит в основной пакет |
| `MassTransit.Extensions.Logging` | WebApi | 5.5.6 | **удалить** — логирование через `Microsoft.Extensions.Logging` из коробки |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | WebApi | 3.1.1 | **10.0.11** |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | WebApi | 3.1.1 | **удалить** (переход на STJ) |
| `Microsoft.AspNetCore.Diagnostics` | WebApi | 2.2.0 | **удалить** — часть shared framework `Microsoft.AspNetCore.App` |
| `Microsoft.AspNetCore.Diagnostics.HealthChecks` | WebApi | 2.2.0 | **удалить** — то же |
| `Microsoft.IdentityModel.Tokens` | WebApi | 5.6.0 | **удалить** — придёт транзитивно с `JwtBearer 10.0.11` (8.22.0). Нужен для `AuthSettings.SymmetricSecurityKey` — проверить, что транзитивной ссылки достаточно, как это оказалось в Sales |
| `NSwag.AspNetCore` | WebApi | 13.2.3 | **14.7.1** — breaking rename `UseSwaggerUi3()` → `UseSwaggerUi()` |
| `prometheus-net.AspNetCore` | WebApi | 3.5.0 | **8.2.1** |

Итого в WebApi остаётся 6 `PackageReference` вместо 13, в Domain — ноль.

`Microsoft.Extensions.Logging.Abstractions` в `Integration` явно не прописан и не нужен: `SalesOrderGateway` берёт `ILogger<>` из зависимости, которую тянет MassTransit (в 6.x и в 8.x одинаково). В Sales явная ссылка была, потому что там пакет уже стоял в csproj; здесь добавлять её не надо.

Версии — те же, что в уже мигрированном `Crnc.Oms.Sales` (проверены на nuget.org 2026-08-19); отдельной перепроверки не требуют, но перед стартом стоит убедиться, что `dotnet restore` их находит.

---

## 2. Изменения в csproj

`<TargetFramework>` → `net10.0` в пяти проектах (Domain, Application, DataAccess, Integration, WebApi). `Messaging.Contract` остаётся `netstandard2.0` (решение 7). Добавить `<ImplicitUsings>enable</ImplicitUsings>` — аддитивно, риск низкий. `<Nullable>` не включаем.

`<GenerateDocumentationFile>true</GenerateDocumentationFile>` + `<NoWarn>$(NoWarn);1591</NoWarn>` в Application и WebApi оставить как есть — на них держится наполнение Swagger из XML-комментариев `JobsController`.

Заодно (§7): убрать дублирующийся `ProjectReference` на Domain в `Crnc.Oms.Production.Application.csproj` и вестигиальный `<Folder Include="Aggregates" />` в `Crnc.Oms.Production.Domain.csproj`.

Тестовых проектов, требующих смены TFM, нет: `Crnc.Oms.Production.E2ETests` рождается на `net10.0` в фазе 1 и правок не потребует.

---

## 3. Program.cs (слияние Startup.cs → minimal hosting)

Удалить `Crnc.Oms.Production.WebApi/Startup.cs`, переписать `Program.cs`. Поведение сохраняется 1:1, кроме явно оговорённых пунктов. Убрать, как в Security и Sales, избыточный `ConfigureAppConfiguration` (дублирует дефолт `WebApplication.CreateBuilder`) и no-op `ConfigureKestrel`.

```csharp
using System.Globalization;
using System.Text.Json;
using Crnc.Oms.Production.Application.Services;
using Crnc.Oms.Production.Application.Services.Abstractions;
using Crnc.Oms.Production.DataAccess;
using Crnc.Oms.Production.DataAccess.Repositories;
using Crnc.Oms.Production.Domain.Gateways;
using Crnc.Oms.Production.Domain.Repositories;
using Crnc.Oms.Production.Domain.SeedWork;
using Crnc.Oms.Production.Integration.Gateways;
using Crnc.Oms.Production.Integration.Settings;
using Crnc.Oms.Production.WebApi.Authorization;
using Crnc.Oms.Production.WebApi.Consumers;
using Crnc.Oms.Production.WebApi.Middlewares;
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
    // JsonStringEnumConverter НЕ добавляем - см. §4: PriorityEnum сейчас числовой,
    // SPA (components/jobs/priority.ts) ожидает числа.
});

builder.Services.AddDbContext<ProductionDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OmsProductionDb")));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddSingleton<ICurrentDateTimeProvider, CurrentDateTimeProvider>();

builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<ISalesOrderGateway, SalesOrderGateway>();

builder.Services.Configure<IntegrationEndpointSettings>(
    builder.Configuration.GetSection("IntegrationEndpoints"));

var integrationSettings = new IntegrationEndpointSettings();
builder.Configuration.GetSection("IntegrationEndpoints").Bind(integrationSettings);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConvertedToJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(integrationSettings.MessageBrokerEndpoint);

        cfg.ReceiveEndpoint("orderConvertedToJob", e =>
        {
            e.ConfigureConsumer<OrderConvertedToJobConsumer>(context);
        });
    });
});

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
    options.Title = "Crnc Oms Production API Doc";
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
// при каждом старте. Раньше ProductionDataContext инжектился параметром Configure(),
// теперь достаём из scope явно.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductionDataContext>();
    ProductionDbInitializer.Initialize(dbContext);
}

app.Run();
```

Содержательные отличия от старого кода, все намеренные:

1. `services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>()` → `AddHttpContextAccessor()` — то же самое, идиоматичнее, снимает `using Microsoft.Extensions.DependencyInjection.Extensions`.
2. `app.UseHealthChecks("/health", …)` → `app.MapHealthChecks("/health", …)` с тем же `Predicate`. Проверок с тегом `ready` не зарегистрировано ни одной, поэтому `/health` возвращает `Healthy` всегда — это ровно текущее поведение, менять его здесь не надо.
3. `ProductionDataContext` больше не приходит параметром метода — берём из явного scope: `AddDbContext` регистрирует scoped-сервис, и `app.Services.GetRequiredService` напрямую бросил бы исключение.
4. Ушли `using MediatR`, `using GreenPipes`, `using MassTransit.AspNetCoreIntegration`, `using MassTransit.ExtensionsDependencyInjectionIntegration`, `using Newtonsoft.Json.*`, `using Microsoft.AspNetCore.HttpsPolicy` — часть неиспользуемых, часть исчезнувших в MassTransit 8.

---

## 4. Переход на System.Text.Json

Как в Security и Sales, единственная точка изменений — блок сериализации; файлов с Newtonsoft-атрибутами нет.

Причины настроек:

- `PropertyNamingPolicy = CamelCase` — эквивалент `CamelCasePropertyNamesContractResolver`. Обязателен: SPA читает `response.data.items`, `priorityEnum`, `isJobCompeted`, `dateCreated`.
- `DictionaryKeyPolicy = CamelCase` — ставим ради паритета с Security и Sales. Как отмечено выше, в Production он ничего наблюдаемого не меняет (атрибутов валидации нет), поэтому и тестом не покрывается; это осознанное «на будущее», а не пропущенная проверка.
- `PropertyNameCaseInsensitive = true` — Newtonsoft десериализует регистронезависимо, STJ по умолчанию нет; сохраняет поведение биндинга `[FromBody]` для `ChangePriorityDto`.
- **`JsonStringEnumConverter` не добавляем.** `PriorityEnum` сейчас едет числом, SPA (`priority.ts`) сравнивает с числовым enum'ом, а `ChangePriorityDto.Priority` принимается числом. Конвертер сломает и вывод, и вход.

**Единственное сознательно принимаемое изменение поведения на входе:** Newtonsoft принимал для enum-свойства и число, и строку-имя (`{"priority": "High"}` работало), STJ без конвертера принимает только число. SPA этот эндпоинт не вызывает вовсе (`JobService.ts` умеет только `getJobs`), внешних клиентов у него нет, так что практических последствий нет — но зафиксировать стоит, чтобы не было сюрприза при ручной проверке через Swagger. По этой же причине e2e-тест на «строковый enum принимается» писать **нельзя**: он был бы зелёным на baseline и красным после миграции по проектному решению.

Проверено, что внимания не требует: `DateTime` в выходных DTO нет вовсе — даты форматируются вручную в `string` через `DateTimeExtensions` (на БД это влияет, см. §5). `decimal` в контрактах нет. `Guid` и `Guid?` форматируются одинаково.

Отдельно: `TextValueDto<int,string>` — дженерик-DTO, для STJ проблемы не представляет; проверить только, что NSwag 14 генерирует для него ту же схему (шаг ручной проверки).

---

## 5. EF Core 3.1 → 10 и Npgsql

### 5.1. Риск №1 — маппинг `DateTime` (блокирующий, рантайм)

Начиная с **Npgsql 6.0** `DateTime`-свойства маппятся на `timestamp with time zone` (`timestamptz`) вместо `timestamp`, и запись `DateTime` с `Kind=Local` или `Unspecified` в такое поле **бросает исключение**. Production оперирует `DateTime.Now` (`CurrentDateTimeProvider`, `ProductionDbInitializer`).

Падать будет на старте: `ProductionDbInitializer.Initialize` вызывает `SaveChanges()` с `DateTime.Now` в `Job.DateCreated` до приёма трафика. Сервис просто не поднимется.

Решение то же, что принято и подтверждено в Sales: **`AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`** первой строкой `Program.cs` (уже в черновике §3). Одна строка, полный паритет: колонка остаётся `timestamp without time zone`, значение — локальное время, формат вывода в SPA не меняется.

Перевод домена на UTC — правильная цель, но это смена наблюдаемого поведения (даты уедут на часовой пояс) и трогает `DateTimeExtensions`; отдельная задача **после** миграции, для всех трёх контекстов сразу.

### 5.2. Owned types и валидация модели

Здесь всё сильно проще, чем в Sales. Owned-тип ровно один — `Job.Manager` с двумя строковыми свойствами и без вложенных владений. Каскад `InvalidOperationException` на `OptionalDependentWithoutIdentifyingPropertyWarning`, который в Sales потребовал трёх `Navigation(...).IsRequired()`, воспроизводится только для dependent'ов, которые сами владеют другими dependent'ами. `Manager` — листовой, значит ожидается **warning, а не падение** (в Sales на листовых `Email`/`Phone`/`FullName`/`NameAbbreviation` warning так и остался, и это оказалось безвредно).

Тем не менее проверять надо первым же стартом, и если `Job.Manager` всё-таки потребует явного признака обязательности — фикс тот же однострочный `builder.Navigation(x => x.Manager).IsRequired()` в `JobMappingConfiguration` (доменно это корректно: конструктор `Job` бросает `ArgumentNullException` на `manager == null`).

Наблюдаемое следствие, которое ловит e2e: если бы `Manager` начал читаться как `null`, `JobService` упал бы на `x.Manager.FullName` в `GetJobsForList` — то есть `GET /api/jobs` вернул бы 500. Тест на сид-работу это закрывает.

**Смягчающее обстоятельство, важное для всей секции**: `ProductionDbInitializer` делает `EnsureDeleted()` + `EnsureCreated()` на каждом старте. Любые изменения в дефолтных именах колонок owned-типа между EF Core 3.1 и 10 нам безразличны — схема каждый раз создаётся заново из текущей модели. Совместимость со старой БД проверять не нужно; ни `Migrations`, ни `__EFMigrationsHistory` в проекте нет.

### 5.3. Прочее по EF Core

- `ProductionDataContext.OnConfiguring` содержит только `base.OnConfiguring` и закомментированную строку — удалить метод целиком вместе с пакетом `EFCore.NamingConventions` (и `using Microsoft.EntityFrameworkCore.Infrastructure`, который станет лишним).
- `Repository<TEntity>` использует `Set<TEntity>()`, `ToListAsync`, `SingleOrDefaultAsync` — всё без изменений в EF Core 10.
- `JobRepository.GetJobByOrderIdAsync` — `SingleOrDefaultAsync(x => x.OrderId == orderId)`, транслируется без изменений.
- Запросов с клиентской оценкой в DataAccess нет; в EF Core 3.0 она уже была ошибкой, так что этот класс проблем был бы виден и сейчас.
- `Microsoft.EntityFrameworkCore.Proxies` удаляется — `UseLazyLoadingProxies` не вызывается, `virtual`-навигаций нет.

### 5.4. Версия PostgreSQL

`postgres:9.6.17` → `postgres:18.6` в `docker-compose.yml` (и тот же тег в фикстуре e2e — держать синхронно). Порт хоста `5434:5432` не меняется. Данные не мигрируем — БД пересоздаётся на каждом старте.

---

## 6. MassTransit 6.2 → 8.5

### 6.1. Конфигурация

MassTransit 8 переехал на единый `AddMassTransit(x => …)`; связка «фабрика `IBusControl` + `IServiceCollectionConfigurator`» из 6.x исчезла. Что удаляется из кода:

- `using GreenPipes;` — пакет влит в MassTransit;
- `using MassTransit.AspNetCoreIntegration;`, `using MassTransit.ExtensionsDependencyInjectionIntegration;` — этих неймспейсов больше нет;
- локальные `CreateBus`/`ConfigureMassTransit` — заменяются на форму из §3.

Что остаётся без изменений: `IConsumer<T>`/`ConsumeContext<T>` (`OrderConvertedToJobConsumer`), `IPublishEndpoint.Publish<T>` (`SalesOrderGateway`), интерфейсные контракты сообщений (MT 8 по-прежнему генерирует прокси для интерфейсов, и `JobCreatedForOrderDto : JobCreatedForOrderEvent` работает как раньше), имя очереди `orderConvertedToJob`. `EndpointConvention.Map` в Production не используется — Production только публикует и потребляет, `Send` не делает.

### 6.2. Совместимость шины — здесь это уже не риск

MassTransit 8 сменил сериализатор по умолчанию с Newtonsoft.Json на System.Text.Json. В плане Sales это был риск №2 с готовым escape hatch. Здесь он снят по двум причинам:

1. **Проверено на живом round-trip миграции Sales**: `OrderConvertedToJobEvent` дошёл от Sales (MT 8.5.10, STJ) до Production (MT 6.2.4, Newtonsoft), работа создалась, `JobCreatedForOrderEvent` вернулся обратно и проставил `jobId`/`jobNumber` на заказе. Оба контракта этой миграции — ровно те же два.
2. **После миграции обе стороны на одной ветке.** Production обменивается сообщениями исключительно с Sales (проверено по `docker-compose.yml` и по коду: Notification.* Production не касается), а Sales уже на 8.5.10.

Escape hatch на всякий случай остаётся тем же: `MassTransit.Newtonsoft` + `cfg.UseNewtonsoftJsonSerializer()`.

### 6.3. RabbitMQ

`MassTransit.RabbitMQ 8.5.10` тянет `RabbitMQ.Client 7.2.1`. Сервер в compose — `rabbitmq:3-management`, протокол AMQP 0-9-1; та же связка уже работает в мигрированном Sales. Менять образ не нужно.

---

## 7. Мёртвый код и предсуществующие баги

Разделено на три группы по степени связи с миграцией.

**Убирается в рамках миграции (иначе не соберётся или останется висеть):**

- `Crnc.Oms.Production.Domain`: убрать `: INotification` и `using MediatR` из `DomainEvent.cs`, удалить `PackageReference` на MediatR (решение 5). `DomainEvent`, `IDomainEventDispatcher`, `DomainEntity.AddDomainEvent/RemoveDomainEvent` и `builder.Ignore(x => x.DomainEvents)` **оставляем** — это каркас, симметричный трём другим контекстам; удаляется только зависимость от фреймворка.
- `Startup.cs` целиком, вместе со всеми мёртвыми `using` (§3).
- `ProductionDataContext.OnConfiguring` (§5.3).
- Дублирующийся `ProjectReference` на Domain в `Crnc.Oms.Production.Application.csproj`.

**Чистка, тесно связанная со сменой TFM (отдельный коммит, до или сразу после):**

- **`Phone.cs` — удалить.** Тип не используется нигде, и он единственный житель неймспейса `Crnc.Oms.Production.Domain.Aggregates.Order`, который сегодня заставляет шесть файлов тащить `using Crnc.Oms.Production.Domain.Aggregates.Order;`. После удаления убрать этот `using` из `Job.cs`, `ICurrentUserContext.cs`, `CurrentUserContext.cs`, `JobRepository.cs`, `JobMappingConfiguration.cs`, `ProductionDbInitializer.cs` — иначе шесть `CS0246`. Прямой аналог удаления `HttpNotificationGateway` в Sales.
- `<Folder Include="Aggregates" />` в `Crnc.Oms.Production.Domain.csproj` — вестигиальный, убрать.
- `EnumHelper.ToDictionaryWithKeysAndDescriptions(List<object> e)` — мёртвая и заведомо падающая перегрузка, убрать.

**Баги, которые чиним отдельно и НЕ внутри миграции:**

- **`appsettings.json`: `Port=5433` → `5434`.** Локальная строка подключения указывает на порт Sales'овой БД. В Docker не проявляется (compose переопределяет `ConnectionStrings:OmsProductionDb`), ломает только `dotnet run` против докеризованной `production-db`. Известная опечатка, зафиксирована в AGENTS.md; после фикса убрать оговорку оттуда (§10). Можно чинить в любой момент, e2e на это не реагируют.
- **`JobService.FinishJob`/`ChangePriority` падают с `NullReferenceException` на неизвестном id** → 500, хотя `JobsController` декларирует 404 и ловит `MissingEntityException`, которую никто не бросает. Правильный фикс — бросать `MissingEntityException`, как это делает `GetJob`. **Делать это надо после миграции**, отдельным коммитом, вместе с правкой соответствующих e2e-тестов (которые на фазе 1 фиксируют текущее 500). Причина строгого порядка ровно та же, что была у Security с его `51ed364 Fix 500 instead of 404 for unknown user id`: baseline должен описывать сервис *до* изменений, иначе он не страховка.
- `[ProducesResponseType(StatusCodes.Status400BadRequest)]` на `GET /api/jobs/{id}`, который на деле возвращает 404 — косметика в Swagger, можно поправить тем же коммитом.
- `ICurrentUserContext` зарегистрирован, но не инжектится никуда, а его реализация упала бы вне HTTP-контекста. Не трогаем: удаление — это уже проектное решение о том, нужен ли Production доступ к текущему пользователю, а не миграция.

---

## 8. Dockerfile, сборка и compose

### 8.1. Базовые образы

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build       # было mcr.microsoft.com/dotnet/core/sdk:3.1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime  # было mcr.microsoft.com/dotnet/core/aspnet:3.1
```

(путь без сегмента `/core/` — старый ретайрен).

### 8.2. `dotnet publish -o` по solution — блокер `NETSDK1194`

Dockerfile Production — копия дореформенных Security/Sales: `RUN dotnet restore` и `RUN dotnet publish -c Release -o out` без явного таргета, то есть по `Crnc.Oms.Production.sln`. С SDK 7.0.200 `--output` для solution — ошибка `NETSDK1194`, сборка образа на `sdk:10.0` упадёт. Заменить:

```dockerfile
RUN dotnet restore Crnc.Oms.Production.WebApi/Crnc.Oms.Production.WebApi.csproj
RUN dotnet publish Crnc.Oms.Production.WebApi/Crnc.Oms.Production.WebApi.csproj -c Release -o out
```

Побочный плюс тот же: состав `.sln` перестаёт влиять на прод-образ, что делает безопасным §8.4.

### 8.3. `.dockerignore` (новый файл)

Нужен `src/Server/src/Crnc.Oms.Production/.dockerignore` — сейчас его нет, а `COPY . ./aspnetapp` тащит `bin/`/`obj/` всех проектов. С появлением `Crnc.Oms.Production.E2ETests` внутри контекста это станет дороже: фикстура пересобирает образ из этого же каталога, её собственный `bin/` меняется на каждом прогоне и инвалидирует слой `COPY` каждый раз. В миграции Sales это пришлось делать вне очереди — здесь делаем сразу, в фазе 1.

```
Crnc.Oms.Production.E2ETests/
**/bin/
**/obj/
```

### 8.4. `.sln`

`Crnc.Oms.Production.sln` содержит все 6 проектов сервиса. Добавить в него `Crnc.Oms.Production.E2ETests`, **строго после 8.2 и 8.3** — пока Dockerfile делает restore по solution на `sdk:3.1`, появление в ней `net10.0`-проекта немедленно ломает сборку образа (`NETSDK1045`), а с ней и сами e2e-тесты.

`ProjectReference` из E2ETests на `Messaging.Contract` (решение 8) на сборку образа не влияет: ссылка направлена только наружу (E2ETests → Contract, никогда обратно), в граф `Crnc.Oms.Production.WebApi.csproj` тестовый проект не входит, а `.dockerignore` (§8.3) вырезает его каталог из контекста ещё до `COPY */*.csproj ./`.

### 8.5. Риск №2 — порт 8080 в `aspnet:10.0`

Образ `mcr.microsoft.com/dotnet/aspnet:10.0` слушает 8080 (`ASPNETCORE_HTTP_PORTS=8080` зашит в образ), а не 80, как `dotnet/core/aspnet:3.1`. Проявляется не ошибкой, а «зависшей» проверкой готовности контейнера. Решение то же, что для Security и Sales, — принимаем дефолт образа и правим тех, кто ходит на Production:

- `docker-compose.yml`: `production-api.ports` → `"8098:8080"` (внешний порт 8098 не меняется, значит README/AGENTS.md/SPA править не надо — `PRODUCTION_API_URL` указывает на `http://localhost:8098/api`);
- `prometheus/prometheus.yml`: таргет `production-api` → `production-api:8080`;
- фикстура e2e-тестов: `ApiContainerPort` с 80 на 8080.

Внутри Docker-сети по имени `production-api` никто не ходит — проверено по `docker-compose.yml` (Sales общается с Production только через RabbitMQ), так что больше править нечего.

### 8.6. compose — прочее

- `production-db`: `postgres:9.6.17` → `postgres:18.6` (§5.4). Порт хоста `5434:5432` не меняется.
- Профили (`profiles:`) и `depends_on` не меняются. В `depends_on` у `production-api` остаются `sales-db`, `security-api` и `notification-gateway-api` — это предсуществующие copy-paste-хвосты (Production не ходит ни в один из них), про них уже есть комментарий в `docker-compose.yml`; чистка `depends_on` меняет набор поднимаемых профилями сервисов и в скоуп этой миграции не входит.
- Переменные `ConnectionStrings:OmsProductionDb` и `IntegrationEndpoints:MessageBrokerEndpoint` синтаксически от TFM не зависят.
- `Auth:JwtBase64SymmetricKey` в compose не задан — ключ берётся из `appsettings.json` и уже выровнен.

---

## 9. Порядок выполнения

**Фаза 1 — e2e-тесты на текущем 3.1 (отдельная ветка, по образцу `6-add-end-to-end-tests-for-salesproject`):**

1. Создать `src/Server/src/Crnc.Oms.Production/.dockerignore` (§8.3) — до тестов, иначе каждый прогон фикстуры будет пересобирать образ с нуля.
2. Создать `Crnc.Oms.Production.E2ETests` по конвенциям Security/Sales и периметру из раздела «Пререквизит»: настоящие PostgreSQL и RabbitMQ, MassTransit 8 как клиент шины, Sales и Security не поднимаются. `TestJwt.cs` переносится из набора Sales. Тесты доводятся до зелёного **на текущем `netcoreapp3.1`-сервисе** — это baseline миграции.
3. Зафиксировать в тестах текущее (неправильное) поведение `PUT /{id}/finished` и `PUT /{id}/priority` на неизвестном id: 500, со ссылкой на §7.

**Фаза 2 — инфраструктура сборки (до смены TFM, работает на 3.1):**

4. Dockerfile: restore/publish по явному csproj (§8.2). Прогнать e2e — образ должен собираться как раньше.
5. Добавить `Crnc.Oms.Production.E2ETests` в `Crnc.Oms.Production.sln` (§8.4). Снова прогнать e2e.

**Фаза 3 — миграция (ветка `5-migrate-production-service-to-net-10`):**

6. Чистка мёртвого кода из второй группы §7 (`Phone.cs` + шесть `using`, `<Folder Include="Aggregates" />`, лишняя перегрузка `EnumHelper`, дубль `ProjectReference`) — отдельным коммитом на 3.1, с прогоном e2e. Так поломки от чистки не смешаются с поломками от смены TFM.
7. TFM → `net10.0` в пяти проектах, версии пакетов и удаления по §1–§2. `Messaging.Contract` не трогать. Сюда же — удаление MediatR и `: INotification` (§7, первая группа).
8. `Startup.cs` → `Program.cs` (§3), удалить `Startup.cs`. Сюда же входят: STJ вместо `AddNewtonsoftJson` (§4, отдельно сверить отсутствие `JsonStringEnumConverter`), новая конфигурация MassTransit (§6.1), `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` первой строкой (§5.1), scope для `ProductionDbInitializer`.
9. Убрать `OnConfiguring` из `ProductionDataContext` (§5.3).
10. `dotnet build Crnc.Oms.Production.sln` — до чистой сборки. Ожидаемые точки поломки: неймспейсы MassTransit, `UseSwaggerUi3()`, `Configure(…, ProductionDataContext)`, `IServiceCollectionConfigurator`, транзитивный `Microsoft.IdentityModel.Tokens` в `AuthSettings`.
11. Первый запуск: проверить логи модели EF Core на предмет `OptionalDependentWithoutIdentifyingPropertyWarning` по `Manager` (§5.2); если это не warning, а `InvalidOperationException` — добавить `builder.Navigation(x => x.Manager).IsRequired()`.
12. Базовые образы в Dockerfile → `sdk:10.0`/`aspnet:10.0` (§8.1); `docker-compose.yml`: `production-api.ports` → `"8098:8080"`, `production-db` → `postgres:18.6`; `prometheus.yml` → `production-api:8080`; порт 8080 в фикстуре e2e (§8.5, §8.6).

**Фаза 4 — проверка:**

13. Прогнать e2e-набор — основная автоматическая проверка. Покрывает §4 (числовые enum'ы), §5.1 (даты), §5.2 (owned-тип `Manager`), §6.1 (потребление и публикация в шину, включая идемпотентность) и сам факт сборки образа на новом SDK.
14. Сверить, что `PriorityEnum` по-прежнему число, а `dateCreated` — строка в прежнем формате (тесты фазы 1 это и делают; отдельного шага не требуется, пункт для протокола).
15. **Ручной round-trip через `docker-compose up`**: логин в SPA, создание заказа в Sales, конвертация в работу → работа появилась на `/jobs` в SPA и в Production API (`http://localhost:8098/swagger`) → `jobId` вернулся в заказ. Это проверка связки Sales(MT8) ↔ Production(MT8) в реальном окружении, которую периметр e2e по построению не покрывает.
16. Swagger UI (`http://localhost:8098/swagger`) — визуально сверить схемы DTO после NSwag 14, особенно `TextValueDto<int,string>` в `GetJobDto.Priorities`.
17. Grafana/Prometheus — убедиться, что таргет `production_api_monitoring` снова `UP` после смены порта.
18. Обновить AGENTS.md (§10).

**Фаза 5 — после миграции, отдельными коммитами:**

19. Фикс `Port=5433` → `5434` в `appsettings.json` (§7) и снятие оговорки про него из AGENTS.md.
20. Фикс 500 → 404 в `JobService.FinishJob`/`ChangePriority` (§7) вместе с правкой e2e-тестов шага 3 и `[ProducesResponseType]` на `GET /api/jobs/{id}`.

**Изменений в SPA не требуется**: маршруты и контракты не меняются, `priorityEnum` остаётся числовым, даты по-прежнему приходят строками, внешний порт 8098 сохраняется. Подтверждается шагами 13–16, а не предполагается.

---

## 10. Обновление AGENTS.md

Станет неверным после миграции:

- Заголовок «### Backend (mixed: Security and Sales on .NET 10, Production and Notification.* on .NET Core 3.1)» — перенести Production к мигрированным; в тексте про `NETSDK1138` остаётся только Notification.*.
- «`.WebApi` — ASP.NET Core host: `Startup.cs` wires DI…» и оговорка «**Security and Sales are the exception**» — добавить Production к сервисам на minimal hosting. Уточнить, что в Production, как и в Sales, `Authorization/` не содержит policy handlers (только `AuthSettings` и `CurrentUserContext`) — это, кстати, верно уже сейчас.
- «`.Domain` — … Framework-free for Security/Production/Notification; Sales's `Domain` is the exception» — после удаления MediatR (решение 5) для Production это станет правдой не только на словах: сейчас там висит `PackageReference` на `MediatR 8.0.0`. Формулировку про «framework-free» стоит поправить сразу, отметив, что до этой миграции Production формально тоже зависел от MediatR.
- Таблица БД: `production-db` — PostgreSQL 9.6.17 → **18.6**.
- Оговорка «Note: `Crnc.Oms.Production.WebApi/appsettings.json`'s local connection string has `Port=5433` … instead of `5434`» — снять после шага 19.
- Раздел Commands: добавить запуск e2e-тестов Production и ту же Windows-готчу с `DOCKER_HOST=tcp://localhost:2375`.
- «Test coverage today» и «Test conventions»: Production больше не «has none» — у него появляются e2e. Заодно описать третью разновидность конвенции — **e2e для сервиса, у которого вход это сообщение**: тестовый проект держит собственный MassTransit-клиент и играет роль отсутствующего соседнего сервиса на обоих концах.
- Там же — **явно оговорить отступление от конвенции**: формулировку «carries no `ProjectReference` to the service … drives the running API purely over HTTP» нельзя оставлять как есть, `Crnc.Oms.Production.E2ETests` ссылается на `Crnc.Oms.Production.Messaging.Contract`. Не расширять правило задним числом, а записать границу: ссылаться можно **только** на `Messaging.Contract` (`netstandard2.0`, ноль пакетов, одни интерфейсы — TFM тестов он не ограничивает и кода сервиса не тянет), и делается это лишь там, где входной контракт сервиса — сообщение, а не HTTP. Заодно отметить, что тест сверяет сервис с *его собственной* копией контракта и расхождение парных копий Sales/Production поймать не может.
- Отдельно упомянуть, что у Production по-прежнему нет юнит-тестов домена (`Crnc.Oms.Production.Tests`) — конвенция ожидает оба вида, этот остаётся долгом.
- Раздел про MassTransit-версии: после этой миграции на 6.x остаются только Notification.*.

---

## Риски и как их ловить

| # | Риск | Как ловить |
|---|---|---|
| 1 | **Npgsql 6+ маппит `DateTime` на `timestamptz` и запрещает запись `Kind=Local`** — сервис падает на старте в `ProductionDbInitializer.Initialize` | Любой запуск. Фикс — `EnableLegacyTimestampBehavior` первой строкой `Program.cs` (§5.1); e2e-тесты на даты подтверждают формат вывода |
| 2 | **`aspnet:10.0` слушает 8080, а не 80** — выглядит как «зависший» контейнер, а не ошибка | Проверенный по опыту Security и Sales сценарий: `docker exec <c> env \| grep ASPNETCORE_HTTP_PORTS`. Фикс — §8.5 |
| 3 | `dotnet publish -o` по solution → `NETSDK1194` | Гарантированно ловится любой сборкой образа. Фикс — §8.2, до смены базовых образов |
| 4 | **Удаление `Phone.cs` роняет сборку шестью `CS0246`** — неймспейс `Aggregates.Order` держится на единственном мёртвом типе | Ловится компилятором мгновенно. Именно поэтому чистка вынесена в отдельный шаг 6 на 3.1, а не смешана со сменой TFM |
| 5 | **Случайно добавленный `JsonStringEnumConverter`** (по аналогии с планом Security) ломает вывод `priorityEnum` и вход `ChangePriorityDto` | E2E-тест «enum'ы приходят числами» + тест на `PUT /{id}/priority`. Отдельно отмечено в §4 |
| 6 | Валидация модели EF Core 10 на owned-типе `Manager` | Первый же старт сервиса + `GET /api/jobs` в e2e (при `Manager == null` сервис вернул бы 500 на `x.Manager.FullName`). Ожидается warning, не падение (§5.2); фикс — одна строка `Navigation(...).IsRequired()` |
| 7 | **Тестовый бус на MassTransit 8 не договорится с сервисом на MassTransit 6** на фазе 1 | Проявится сразу, на первом же messaging-тесте. Вероятность низкая — та же пара версий и те же два контракта уже проверены round-trip'ом миграции Sales. Запасной вариант — ручной конверт через management API (раздел «Пререквизит») |
| 8 | Npgsql 4.1 (провайдер 3.1) против `postgres:18.6` в фикстуре на фазе 1 | Проявится как ошибка аутентификации или странный SQL на первом же прогоне. В наборе Sales эта же связка отработала без правок; запасной вариант — временно `postgres:9.6.17` в фикстуре, вернуть на шаге 12 |
| 9 | `UseSwaggerUi3()` переименован в NSwag 14; `Configure(…, ProductionDataContext)` в minimal hosting так не работает | Ловится компилятором мгновенно |
| 10 | NSwag 14 может иначе сгенерировать схему для `TextValueDto<int,string>` | Тестами не покрыто — визуальная сверка Swagger UI, шаг 16 |
| 11 | **Сужение толерантности входа: STJ не принимает строковые имена enum'ов**, а Newtonsoft принимал | Осознанное изменение поведения (§4). Внешних потребителей у `PUT /{id}/priority` нет; e2e-теста на строковый вход намеренно не пишем |
| 12 | `DictionaryKeyPolicy` забыт → молчаливая регрессия camelCase→PascalCase в ключах ошибок | **Тестом не ловится и ничего не ломает** — в Production нет атрибутов валидации, ключи `ModelState` берутся из имён параметров и JSON-путей (§4). Ставим ради паритета, не ради поведения |
| 13 | E2E-тесты фиксируют 500 на неизвестном id, а фаза 5 меняет его на 404 | Не риск, а порядок: тесты правятся тем же коммитом, что и `JobService` (шаг 20). Обратный порядок сделал бы baseline недостоверным |
| 14 | **Тест ссылается на копию контракта самого Production** — расхождение с копией Sales по построению не ловится | Не ловится и ловиться не должно: это отдельная проверка, не e2e. На момент составления плана копии сверены и идентичны. Если понадобится — дешёвый способ это `diff` двух `Messaging.Contract/Events/` в CI, а не усложнение набора |

---

## Критичные файлы

- `Crnc.Oms.Production.WebApi/Startup.cs` (удалить) и `Program.cs` (переписать, §3)
- 5 мигрируемых `.csproj` (§1, §2); `Crnc.Oms.Production.Messaging.Contract.csproj` не трогаем
- `Crnc.Oms.Production.Domain/SeedWork/DomainEvent.cs` — убрать `: INotification` и `using MediatR` (решение 5)
- `Crnc.Oms.Production.Domain/Aggregates/JobAggregate/Phone.cs` — **удалить** (мёртвый код), плюс шесть `using Crnc.Oms.Production.Domain.Aggregates.Order;` (§7)
- `Crnc.Oms.Production.DataAccess/ProductionDataContext.cs` — удалить `OnConfiguring` (§5.3)
- `Crnc.Oms.Production.DataAccess/Mappings/JobMappingConfiguration.cs` — цель проверки owned-типа (§5.2)
- `Crnc.Oms.Production.Application/Services/JobService.cs` — фикс 500 → 404, **фаза 5** (§7)
- `Crnc.Oms.Production/Dockerfile` (§8.1, §8.2) и новый `Crnc.Oms.Production/.dockerignore` (§8.3)
- `Crnc.Oms.Production.sln` (§8.4)
- `Crnc.Oms.Production.WebApi/appsettings.json` — `Port=5433` → `5434`, **фаза 5** (§7)
- `docker-compose.yml` — `production-api.ports` → `"8098:8080"`, `production-db` → `postgres:18.6`
- `prometheus/prometheus.yml` — таргет `production-api` → `production-api:8080`
- `AGENTS.md` (§10)

Новые файлы набора e2e (фаза 1), по образцу `Crnc.Oms.Sales.E2ETests`:

- `Crnc.Oms.Production.E2ETests.csproj` — `net10.0`, xunit 2.9.3 / runner 3.1.4 / Test.Sdk 17.14.1 / FluentAssertions 7.2.2 / Testcontainers 4.14.0 / Microsoft.IdentityModel.JsonWebTokens 8.22.0 + `MassTransit` и `MassTransit.RabbitMQ` 8.5.10 + **`ProjectReference` на `Crnc.Oms.Production.Messaging.Contract`** (решение 8)
- `ProductionApiFixture.cs` — сеть, три контейнера, тестовый бус, сборка образа из настоящего `Dockerfile`
- `TestJwt.cs` — перенос из набора Sales
- `TestModels.cs` — DTO ответов, `SeedData` (сид-работа `f425e777-…`, сид-заказ `5c5c6017-…`, ключ/issuer/audience), `JsonDefaults.Options`. Контрактных интерфейсов здесь **нет** — они приходят по `ProjectReference`; локально объявляются только классы-реализации для `Publish<T>` (по образцу `JobCreatedForOrderDto` в `Integration`)
- `JobsReadTests.cs`, `JobsWriteTests.cs`, `JobMessagingTests.cs`

## Статус

**Фазы 1–4 выполнены.** `Crnc.Oms.Production.E2ETests` — 14 тестов, зелёные и на baseline (`netcoreapp3.1`), и на мигрированном сервисе, стабильно на нескольких прогонах подряд. Сервис переведён на `net10.0`, `dotnet build Crnc.Oms.Production.sln` чистый (ноль предупреждений), образ собирается на `sdk:10.0`/`aspnet:10.0`.

Что подтвердилось на живом прогоне и было в плане только предположением:

- **§5.2 подтвердился без осечек** — в отличие от Sales, где трёхуровневая вложенность `Customer → Title/ContactPerson` потребовала трёх `Navigation(...).IsRequired()`. `Job.Manager` — лист без вложенных владений, и на реальном старте (проверено не только e2e-фикстурой, но и отдельным ручным прогоном образа против настоящего `postgres:18.6` вне Testcontainers, с чтением сырых логов) видно ровно предсказанное: безвредный `warn: Microsoft.EntityFrameworkCore.Model.Validation[20606] OptionalDependentWithoutIdentifyingPropertyWarning`, не `InvalidOperationException`. Фикс не понадобился.
- **Риск совместимости шины (снятый в §6.2) подтверждён живым round-trip'ом**, а не только рассуждением по аналогии с Sales: полный цикл через реальный `docker-compose` (профили `sales` + `production`, без SPA — её сборка падает на предсуществующей проблеме `yarn: not found` в `crnc-oms-ui`, как и было зафиксировано в статусе плана Sales) — логин через Security, создание заказа в Sales, переходы `NotSent → NeedSignoff → Signed → ConvertedToJob` через HTTP, `OrderConvertedToJobEvent` дошёл до Production (оба уже на MassTransit 8.5.10), работа создалась, `JobCreatedForOrderEvent` вернулся, `jobId`/`jobNumber` проставились на заказе. Проверено и предсуществующее поведение из инвентаризации: `[EnumRequired]` отвергает `null` в `materialSource` на любом `PUT`, не только при конвертации — как и в Sales.
- **Шаг 15 (ручной round-trip) сделан через прямые HTTP-вызовы API, а не через клики в SPA** — SPA недоступна по той же причине, что и в проверке Sales. Это не ослабляет проверку: цель шага 15 — подтвердить совместимость Sales(MT8) и Production(MT8) в реальном окружении, а не UI, и это проверено полностью.
- **Шаг 16 (визуальная сверка Swagger) вскрыл предсуществующий, не связанный с миграцией факт**: `GetJobDto` (а с ним и `TextValueDto<int,string>` внутри `Priorities`) в схеме OpenAPI не появляется вовсе — ни на NSwag 13, ни на NSwag 14. Причина: `JobsController.Get(Guid id)` возвращает голый `IActionResult` без `[ProducesResponseType(typeof(GetJobDto), 200)]`, и NSwag не может вывести тип ответа. Тот же паттерн подтверждён и в `Crnc.Oms.Sales.OrdersController.Get(Guid id)` — систематическая особенность кодовой базы, не регрессия этой миграции. Что было в схеме, сравнено чисто: `Priority` — `type: integer` с `enum: [1,2,3]`, свойства camelCase (`priorityEnum`, `isJobCompeted`, `dateCreated`) — без сюрпризов от STJ.
- **Шаг 17 (Prometheus) прошёл не с первой попытки**: после смены порта в `prometheus/prometheus.yml` таргет `production_api_monitoring` остался `down` до тех пор, пока не пересобран сам образ `prometheus` (`docker-compose build prometheus`) — Dockerfile копирует `prometheus.yml` на этапе сборки (`ADD prometheus.yml /etc/prometheus/`), обновления файла в репозитории недостаточно, нужен `--build`. После пересборки таргет `up`. Это не баг миграции, а особенность локальной проверки — на реальном CI/CD образ и так пересобирается всегда.

Не проверялось и не должно: сквозной сценарий с реальным Notification (вне периметра, Production до него не доходит), Grafana-дашборд визуально (Prometheus-таргета `up` достаточно для цели шага 17).

**Фаза 5 (баг 500→404 и опечатка порта в `appsettings.json`) не начата** — по плану идёт отдельными коммитами после фазы 4.
