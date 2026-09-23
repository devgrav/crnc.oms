# AGENTS.md

This file provides guidance to AI coding agents (Claude Code, Codex, etc.) when working with code in this repository.

## Project overview

CRNC OMS is an order-management system for a fictional manufacturing company, built as a set of independent .NET microservices (one per bounded context) plus a React SPA. It tracks an order's lifecycle from creation through conversion into a production job.

Bounded contexts / services:
- **Security** (`src/Server/src/Crnc.Oms.Security`) — auth (JWT issuance), user & role CRUD. ASP.NET Core + MongoDB. Caches users in-memory via `IMemoryCache`.
- **Sales** (`src/Server/src/Crnc.Oms.Sales`) — order & customer management, order → job conversion. ASP.NET Core + PostgreSQL (EF Core), RabbitMQ/MassTransit.
- **Production** (`src/Server/src/Crnc.Oms.Production`) — production job management, created in response to order conversion. ASP.NET Core + PostgreSQL (EF Core), RabbitMQ/MassTransit.
- **Notification** (`src/Server/src/Crnc.Oms.Notification`) — **four deploy units**, not three: `Notification.Gateway` (resolves the delivery channel and fans out), `Notification.Email`, `Notification.Push` (SignalR), and `Notification.Push.Client`, a console app with its own Dockerfile and compose service, used to observe push messages in dev. ASP.NET Core + RabbitMQ/MassTransit; **no database and no domain layer** — the whole context is thin services over the bus and a SignalR hub. Its inbound contract `SendNotificationToUserCommand` deliberately carries only `UserId` and `Message`: the sender says "notify this user" and Notification alone decides *where* to deliver, resolving the address itself from Security. That is why `UserInfoGateway` calls `GET /api/users/{id}` with **no** bearer token — Security marks its user reads `[AllowAnonymous]` precisely to support this. Don't "fix" either side without reading the reasoning in `docs/migrations/notification-net10-migration-plan.md`. Known debt: `MonitoringRequestMiddleware` exists in all three WebApi projects and is wired up in none of them (Security, Sales and Production do call it) — left alone deliberately, since switching it on changes the metric set.
- **Client** (`src/Client`) — React + TypeScript + Mobx + Semantic UI SPA.

Each backend service is its own independently buildable/deployable solution and Docker image (own `.sln`, own Dockerfile) — there is no shared solution that builds everything at once.

## Repository layout

- `src/Server/` — all backend microservices (see contexts above), each under `src/Server/src/Crnc.Oms.<Context>/`.
- `src/Client/` — the React SPA (single frontend project, source under `src/Client/src/`).
- `prometheus/`, `grafana/` — configuration for the monitoring stack, mounted into pinned upstream images. Neither has a Dockerfile any more: edit a file and restart the container, no rebuild.
- `docker-compose.yml` (repo root) — wires every service, its DB, and the monitoring stack together for local runs.
- `docs/migrations/` — written-up plans for cross-cutting migrations (e.g. `security-net10-migration-plan.md`, `monitoring-stack-upgrade-plan.md`). Put a plan here before starting a multi-service migration, and update it as steps land.
- `docs/ci/` — how the pipelines are put together and why (`backend-ci.md`). Read it before changing `.github/workflows/`.
- `.github/workflows/` — CI. Today just `backend-ci.yml` (see "CI" under Commands).
- `README.md` (Russian) — the product-level spec: what each bounded context is supposed to do, the messaging flows in prose, and links to the architecture diagrams / Miro context map. Read it for intent; this file for mechanics.

## Architecture (per backend service)

Each backend context follows the same layered/DDD structure (project names prefixed `Crnc.Oms.<Context>.`):

- **`.Domain`** — aggregates, domain events, repository interfaces, `SeedWork` (base types: `DomainEntity`, `IAggregateRoot`, `DomainEvent`, `DomainException`, `Enumeration`, `ICurrentUserContext`, `ICurrentDateTimeProvider`, `IDomainEventDispatcher`). Framework-free for Security/Production/Notification; Sales's `Domain` is the exception — it carries a direct `PackageReference` on MediatR because `DomainEvent : INotification`. Production's `Domain` used to carry the same `MediatR` reference for the same reason, but no domain event was ever actually raised or dispatched anywhere in the codebase (`IDomainEventDispatcher` had no implementation, `AddMediatR` was never called) — removed as dead weight during its .NET 10 migration rather than upgraded.
- **`.Application`** — use cases, organized under `Features/<Aggregate>/{CommandHandlers,QueryHandlers,EventHandlers,Dto}`. Commands/queries go through a custom `ICommandQueryDispatcher` / `IUseCaseCommandHandler<TIn,TOut>` / `IUseCaseQueryHandler<TIn,TOut>` pattern (see `Crnc.Oms.Sales.Application/CommandQueryDispatcher.cs`) — but this is a thin wrapper over MediatR, not an alternative to it: `IUseCaseCommand<TOut> : IRequest<TOut>`, `IUseCaseCommandHandler : IRequestHandler`, and `CommandQueryDispatcher` itself just calls `IMediator.Send`. MediatR is the load-bearing dispatch mechanism for the whole Application layer, not only for domain events (`IDomainEventNotificationHandler`, wired via `IDomainEventDispatcher`).
- **`.DataAccess`** (Sales/Production, EF Core+Postgres) or **`.Infrastructure.DataAccess`** (Security, Mongo) — repository implementations, EF mappings / Mongo mappings, DB initializer/seeder.
- **`.Integration`** — outbound gateways to other services (typed `HttpClient` calls to other APIs, MassTransit-based gateways), and typed settings bound from `IntegrationEndpoints` config section.
- **`.Messaging.Contract`** — shared MassTransit command/event contracts published/consumed across services (e.g. `SendNotificationToUserCommand`, order/job conversion events).
- **`.WebApi`** — ASP.NET Core host wiring DI, EF `DbContext`, MassTransit bus + consumers, JWT auth, NSwag/Swagger, Prometheus metrics, health checks (`/health`). `Controllers/` call the `ICommandQueryDispatcher`; `Consumers/` handle inbound MassTransit messages; `Authorization/` holds role constants, `AuthSettings` and `CurrentUserContext`. **Every context now uses minimal hosting in `Program.cs` — there is no `Startup.cs` left in the repository**, and no role-based policy handlers either (Production's `JobsController` is a bare `[Authorize]` with no role checks at all). Security has no MassTransit (pure inbound HTTP); Sales, Production and all three Notification services keep it (RabbitMQ, MassTransit 8.5.10) — Sales because it both publishes and consumes workflow events, Production because it has no HTTP endpoint to create a job at all (the only way in is consuming `OrderConvertedToJobEvent`), Notification because commands are its primary entry point. MassTransit 8 also registers a bus health check, so `/health` genuinely reports 503 while the bus is still connecting.

Cross-service integration is two-pronged:
1. **Sync HTTP** for reads (e.g. Sales calling Security to resolve an employee/manager).
2. **Async messaging via RabbitMQ/MassTransit** for workflow events — e.g. Sales publishes an order-converted event, Production's consumer creates a job and replies with a job-created event, which Sales's consumer uses to store the job id back on the order. Commands (imperative, e.g. `SendNotificationToUserCommand`) and events (past-tense, e.g. job/order state changes) are modeled as distinct MassTransit contract types.

Test coverage today: `Crnc.Oms.Sales.Tests` (Sales `Domain` unit tests), plus `Crnc.Oms.Security.E2ETests`, `Crnc.Oms.Sales.E2ETests`, `Crnc.Oms.Production.E2ETests` and `Crnc.Oms.Notification.E2ETests` (those contexts over HTTP/messaging, via Testcontainers). Production has no `Domain` unit test project yet (the convention below expects one eventually). Notification has e2e but **cannot** have domain unit tests — it has no domain layer; that is a property of the context, not a debt. See "Test conventions" below.

Monitoring: Prometheus scrapes each service's `/metrics` endpoint every 5s (via `prometheus-net`); Grafana ships with a default dashboard. Not collected for infra containers (Mongo/Postgres/RabbitMQ). Both run as pinned upstream images with their config mounted from `prometheus/` and `grafana/` — no Dockerfiles, so a config edit needs a container restart, not a rebuild. Three things to know before touching Grafana:

- **The provisioning directory is mounted whole**, the way Grafana's own examples do it — mounting subdirectories one by one is how people end up adding, say, `alerting/` on the host and silently never provisioning it. The cost is that `alerting/` and `plugins/` must exist even though this repo provisions neither: replacing `/etc/grafana/provisioning` hides the empty directories the image ships, and Grafana logs a `level=error` per missing one. They are kept with an `empty.yaml` holding just `apiVersion: 1` — a `.gitkeep` would be flagged as a file with an unknown suffix.
- **The dashboard JSON on disk is what Grafana serves — verbatim.** Schema migration is a frontend concern, so the API returns whatever `schemaVersion` and panel types the file declares. A panel type the running version dropped (Angular `graph`, removed in 12) renders as a blank panel rather than being migrated for you: fix the file.
- **The datasource's `uid` is pinned to `prometheus` in provisioning**, and every panel, target and template variable references it. The dashboard's own `uid` is `zyAf4i4Zz` and is linked from README.md and from the table below — keep both stable.

## Architecture (frontend, `src/Client`)

React 19 + TypeScript + Mantine + TanStack Query, bundled by Vite. Entry point `src/main.tsx` → `src/App.tsx`. Files group by feature (`src/pages/<feature>/`), not by file type; one component per file, PascalCase, `export default`, no barrel files. The migration that produced this shape is written up in `docs/migrations/client-modern-stack-migration-plan.md` — read it before re-litigating any of the choices below.

- **No global store.** MobX is gone and nothing replaced it: server data lives in TanStack Query (`useServiceQuery` in `src/hooks/`, invalidated by key after mutations), form state in local `useState`, and the only application-wide state is the current user. Redux/Zustand were considered and declined — after those two moves there is nothing global left to keep. Don't reintroduce a store "for later".
- **Routing & auth guard** (`src/routes.tsx`) — `react-router` v8, the whole map in one place, a layout route with `<Outlet/>` instead of per-page wrappers, `index` redirect and a catch-all `*` → `NotFound`. `ProtectedRoute` redirects to `/login` when unauthenticated and renders `Forbidden` on a role mismatch. It nests twice on purpose: an outer `<ProtectedRoute />` with **no** `roles` guards authentication only, so `Layout` never flashes before the redirect, and the per-section guards (`managerRoles`, `[Roles.Admin]`) sit inside it around one shared `Layout` — a role mismatch therefore renders `Forbidden` inside the chrome. Declare the roles a route needs; never express "admin only" as an empty `roles` array leaning on the bypass below. **`Admin` passes every route regardless of the declared `roles`** — deliberate, carried over from the old guard, and covered by a test. The order card is the one `React.lazy` route.
- **Current user** (`src/auth/`) — a real React context: `AuthContext` + `AuthProvider` + `useAuth()`. `tokenStorage.ts` owns `sessionStorage` under the key `crnc.oms.currentUser` and validates the shape on read. The API client reads the token from there on every request rather than from the context, because interceptors live outside the component tree. Its response interceptor treats a 401 *while a token is stored* as an expired session: it clears the token and sends the browser to `/login` — the token check is what stops an unauthenticated 401 from looping.
- **Services** (`src/services/`) — one configured `apiClient` (relative `baseURL`, request interceptor stamping `Authorization`) plus one module of operations per aggregate. **Services never throw**: every operation returns `{ success, data?, fieldErrors?, generalError? }`, which is why no component has a `try/catch`. That rule lives in exactly one file — `request.ts` (`request` / `requestItems` / `requestVoid`) is the only place with a `try/catch`, and every operation is a one-line call through it; `requestItems` also unwraps the `{ items: [...] }` envelope Sales and Production use, which is why `ItemsResponse.items` is optional (an empty grid arrives without it). `errorHandler.ts` is the single place that normalizes failures, and it must keep handling all three 400 shapes the backends produce — `ValidationProblemDetails` with an `errors` wrapper (Sales), a flat `SerializableError` dictionary (Security's `UsersController`, which has no `[ApiController]`), and a bare string (`AccountsController`) — plus network errors, 5xx, and JSON hidden inside a binary response.
- **Forms** (`src/hooks/useFormValidation.ts`) — `errors`, `generalError`, `getErrorMessage(field)`, `clearFieldError`, `clearAllErrors`; a field's error clears on the user's first keystroke. `fieldErrors` keys are the form field names — both backends emit camelCase dictionary keys, and that is a contract, not a coincidence.
- **Screens** (`src/pages/`) — `orders` (grid + one card component serving both create and edit, rendered over the grid through an `Outlet`), `jobs` (read-only grid), `users` (cards, client-side filter in `filterUsers.ts`, paged at 8), `login`. Cards are routes, not local state: `/orders/new`, `/orders/:id`, `/users/new`, `/users/:id`. Note that `/…/new` has no `:id` param at all, so `undefined` there means "create" — the static segment outranks `:id`, so the param is never the literal string `"new"` and nothing should compare against it. Each card fetches **its own** record (`GET /sales/orders/{id}`, `GET /security/users/{id}`) rather than picking it out of the grid's list, and does so with `gcTime: 0` — **don't drop that**. A card seeds its form state once at mount, so a cached response served instantly on reopen would leave stale values in the form and save them back; the Playwright conversion test catches exactly this (status chain `Signed` → rejected by Sales). Both grids and both cards report a failed load the same way: a red `Alert` above the content, inside a `<Box pos="relative">` that the `LoadingOverlay` anchors to.
- **Push notifications** (`src/notifications/`) — one SignalR connection per session, opened by `NotificationsProvider` at app level (not by the bell component, which used to rebuild it on every remount). The hub URL is relative (`/hubs/push`); `accessTokenFactory` supplies the JWT, and the server addresses clients by the `nameid` claim. `NotificationsBell` in the layout reads the collected messages through `useNotifications()`. Both contexts follow the same shape — `createContext<T | null>(null)` plus a hook that throws when the provider is missing; no default-value stubs, which would let a component silently render empty outside its provider.
- **Layout** (`src/components/`) — `Layout` is a route element wrapping every authenticated page; it holds the nav, the bell and sign-out. `ErrorBoundary` at the root is the only class component in the codebase — React 19 still has no hook equivalent of `componentDidCatch`.
- **React Compiler lint rules are on.** `react-hooks` v7 rejects `setState` inside an effect. Forms do not need a workaround for it: a card renders a loader until its query resolves and only then mounts the form component, which seeds `useState` from the loaded record once — prefer that split over syncing state to props at all, together with the `gcTime: 0` that makes seeding-once sound. The one place that genuinely adjusts state during render is `NotificationsProvider` (clearing messages when the user signs out), against a stored source reference; copy that only when there is no data-loading boundary to split on, and never reach for `useEffect` instead.

## Test conventions

There are two established shapes. Every service is expected to eventually have both — add them opportunistically as services get touched, not only during a dedicated migration. Both use xUnit + FluentAssertions, and both name tests `Method_Condition_ExpectedResult` with `//Arrange` / `//Act` / `//Assert` comment blocks in the body.

**Unit tests, Sales-style (`Crnc.Oms.Sales.Tests`)** — a `Crnc.Oms.<Context>.Tests` project sitting next to the other projects in the context's `.sln`, `ProjectReference`-ing only `.Domain`, and mirroring the domain's namespace layout under a `Domain/Aggregates/<X>Aggregate/` folder. Targets the same TFM as the service (`net10.0` everywhere). Aggregates are constructed through their real constructors with real value objects — no mocking framework is in use anywhere in the repo; if a test needs a collaborator, prefer a hand-written fake over adding one.

**E2E tests, Security-style (`Crnc.Oms.Security.E2ETests`)** — a `net10.0` project that *is* a member of the service's `.sln` (added once the Dockerfile restores/publishes an explicit `.csproj` rather than the whole solution, so an extra project in the `.sln` can't affect the image build) but carries **no `ProjectReference`** to the service: it drives the running API purely over HTTP, so it stays on modern .NET regardless of the service's own TFM and exercises the same artifact that ships. Key pieces:

- `SecurityApiFixture` (`IAsyncLifetime` + `ICollectionFixture` via `SecurityApiCollection`) builds a Testcontainers network, starts the infra container, then builds the service image **from its real `Dockerfile`** (`ImageFromDockerfileBuilder`, context located by walking up from `AppContext.BaseDirectory` to the `.sln`) and wires it to the infra by network alias. Container config is overridden with `WithEnvironment` using the same keys as `docker-compose.yml`; readiness waits on `/swagger/index.html`. One fixture is shared by the whole collection — tests must not depend on each other's writes, so generate unique logins/emails per test (`Guid.NewGuid():N`) rather than reusing fixed ones.
- Infra container image tags must stay in sync with `docker-compose.yml` (see the comments in the fixture about Mongo's wire version and why the plain `ContainerBuilder` is used instead of the `Testcontainers.MongoDb` module).
- Request/response DTOs are re-declared locally as `record`s in `TestModels.cs` — never referenced from the service — alongside `SeedData` (the seeded role/user ids and the `admin` / `shon_bean` logins) and `JsonDefaults.Options` (`JsonSerializerDefaults.Web`), which every `PostAsJsonAsync` / `ReadFromJsonAsync` call passes.
- The fixture logs in once per role at startup and exposes `AdminJwt` / `MainManagerJwt` plus `CreateAuthorizedClient(jwt)`; the unauthenticated `Client` is used directly for 401 checks. Each endpoint group gets its own `[Collection(SecurityApiCollection.Name)]` class (`RolesTests`, `UsersReadTests`, `UsersWriteTests`, `AuthenticateTests`) covering the happy path plus the no-auth (401) and wrong-role (403) cases.
- Where a test guards a specific migration hazard (JSON casing of ModelState keys, Mongo LINQ3 translation), the `//Arrange` comment says so and cites the plan under `docs/migrations/` — keep that habit so the regression's reason survives.

**E2E tests for a service with outbound dependencies (`Crnc.Oms.Sales.E2ETests`)** — same shape as above, plus the rule that **only the database is real**. Security is replaced by a `wiremock/wiremock` container running under the *same network alias and port* as the real service, stubbed from `WireMockAdmin` via its `/__admin` API; `GET /__admin/requests` doubles as the assertion that the service actually made the outbound call. RabbitMQ is real (the bus must connect for the service to start) but the assertion stops at "the message reached the queue" — no consumer services are started. Two consequences worth knowing before writing more of these:

- The fixture **mints its own JWT** (`TestJwt`, short claim names as the real issuer emits them) and forces the signing key onto the container via `Auth:JwtBase64SymmetricKey`, so the suite neither needs Security nor breaks when keys rotate.
- MassTransit `Publish` goes to a fanout exchange, and **an event with no subscriber is silently dropped** — the queue counter never moves. `RabbitMqAdmin.EnsureSpyQueueAsync` therefore declares a spy queue and binds it to the message's exchange *before* the acting request, and assertions are deltas (`before`/`after`) because the fixture is shared.

**E2E tests for a message-driven service (`Crnc.Oms.Production.E2ETests`)** — same fixture shape again (network, real DB, real RabbitMQ, image built from the real Dockerfile), but for a service whose *entry point* is a message rather than HTTP: Production has no endpoint to create a job, the only way in is consuming `OrderConvertedToJobEvent`. Rather than poke the broker by hand (the `RabbitMqAdmin`-plus-manual-envelope approach the other two suites don't need), the test project runs its own MassTransit 8 bus and plays the missing neighbor service — here Sales — on both ends: it publishes the conversion event and listens on a temporary receive endpoint for the reply `JobCreatedForOrderEvent`, collecting messages by `OrderId` for the test to poll. This is the **one deliberate exception** to "no `ProjectReference` to the service": the test project references `Crnc.Oms.Production.Messaging.Contract` (`netstandard2.0`, zero packages, just the message interfaces) instead of redeclaring the interfaces locally, so a contract change breaks the build instead of silently drifting. Do this only when the convention above genuinely can't apply — the service's own contract is the one exception worth making; nothing else in the service's tree.

**E2E tests for a multi-unit bounded context (`Crnc.Oms.Notification.E2ETests`)** — one test project and one fixture for a context that ships as several deploy units, starting *all* of them for real. This looks like a break from "only the database is real", and isn't: Notification has no database at all, and Email and Push are not neighbouring services but parts of the same context. Splitting it per unit would start RabbitMQ three times and still leave the only chain worth testing — `command → Gateway → bus → Push → SignalR` — uncovered. Specifics worth copying:

- **A SignalR client inside the test process** plays `notification-push-client`. `Microsoft.AspNetCore.SignalR.Client` connects to the hub with a test-minted JWT and collects `ReceivePushMessageAsync` callbacks; without it, push delivery has no observable outcome at all. One test connects two clients and asserts a message addressed to one user never reaches the other's connection — that check exercises `Clients.User(...)` addressing, which rides on the `nameid` claim.
- **The bus entry is driven by hand-built MassTransit envelopes** posted through the RabbitMQ management API (`RabbitMqAdmin.PublishCommandAsync`): a JSON envelope with `messageType` URNs and content type `application/vnd.masstransit+json`. This is the cheaper alternative to Production's "run your own bus" approach and keeps the project free of any `ProjectReference`. Verify the envelope format against a live stand before relying on it.
- **Assertions match each test's own message by a unique marker**, never queue-count deltas. The fixture is shared and sends are asynchronous to the HTTP response, so a late message from a neighbouring test moves any counter — the first run of this suite failed exactly that way. `RabbitMqAdmin.DrainAsync` consumes what it reads, so spy queues self-clean.
- **The WireMock stub of Security deliberately does not require `Authorization`**, and a test asserts the Gateway sends none. The real `GET /api/users/{id}` is `[AllowAnonymous]` because the notification contract carries no delivery-channel parameters (see the Notification bullet under "Bounded contexts"). A stricter stub would look safer and would assert a contract that does not exist.
- Container ports are **per unit**, not one constant: each unit's port comes from its own base image, and during a staged migration the values legitimately differ.


## Commands

### Run the whole system, or one context at a time (Docker)

From the repo root:
```
docker-compose build
docker-compose up
docker-compose down
```
If something is broken after a change, a clean rebuild is often needed:
```
docker-compose down
docker system prune
docker-compose build
docker-compose up
```

Every service in `docker-compose.yml` carries `profiles:`. The root `.env` sets `COMPOSE_PROFILES=full`, so a bare `docker-compose up` (no flags) still starts everything, same as before. Passing `--profile <name>` on the CLI **overrides** that default (it does not add to it), so it starts only that context plus whatever it actually depends on:
```
docker-compose --profile security up      # security-db + security-api only
docker-compose --profile sales up         # sales + its real deps: security, notification, message-broker
docker-compose --profile production up
docker-compose --profile notification up  # all 3 notification sub-services + push-client + security
docker-compose --profile client up        # the SPA + the whole backend it talks to
docker-compose --profile server up        # everything except the SPA - see below
docker-compose --profile monitoring up    # prometheus + grafana only
```
**`server` is `full` minus `crnc-oms-ui`**: every backend service, both databases, the broker, the push console client and the monitoring stack, with no SPA image built or started. That is what you want while working on the frontend with `npm run dev` — Vite serves the UI on the same port 8092 and proxies to the backends, so leaving the containerised SPA out avoids two builds of the same thing and a port clash.

Available profiles: `security`, `sales`, `production`, `notification`, `client`, `server`, `monitoring`, `full`. Docker Compose does **not** auto-activate a dependency's own profile via `depends_on` — every service lists every context profile that can reach it transitively, so e.g. `security-api` carries `security`, `sales`, `production`, `notification`, and `client` (every context that ends up depending on it), not just `security`. Keep this in sync when changing `depends_on` edges or adding services.

Service endpoints once running:
| Service | URL |
|---|---|
| Security API (Swagger) | http://localhost:8090/swagger |
| Sales API (Swagger) | http://localhost:8091/swagger |
| Production API (Swagger) | http://localhost:8098/swagger |
| Notification Gateway API (Swagger) | http://localhost:8100/swagger |
| Email Notification API (Swagger) | http://localhost:8104/swagger |
| Push Notification API (Swagger) | http://localhost:8107/swagger |
| SPA UI | http://localhost:8092 |
| RabbitMQ UI | http://localhost:15673 |
| Prometheus UI | http://localhost:9090 |
| Grafana UI | http://localhost:3000/d/zyAf4i4Zz/prometheus-net (admin/p@ssw0rd) |

Seeded logins in the UI: `admin/111111` (administrator), `shon_bean/111111` (manager — receives order-status push notifications).

Databases, reachable from the host once `docker-compose up` is running (e.g. via MongoDB Compass / pgAdmin):
| DB | Engine | Host:port | Database | Auth |
|---|---|---|---|---|
| security-db | MongoDB 8.3.8 | `localhost:27021` | `crnc_oms_security_db` | none |
| sales-db | PostgreSQL 18.6 | `localhost:5433` | `crnc_oms_sales_db` | `postgres` / `docker` |
| production-db | PostgreSQL 18.6 | `localhost:5434` | `crnc_oms_production_db` | `postgres` / `docker` |

These are the ports mapped in `docker-compose.yml`; inside the Docker network services reach each other by container name (`security-db`, `sales-db`, `production-db`) on the default port.

**Inside the Docker network every API now listens on 8080**, not 80 — that is the default baked into `mcr.microsoft.com/dotnet/aspnet:10.0`. Host-side ports in the table above are unchanged, so the SPA and README need nothing, but any container-to-container URL must carry `:8080` explicitly, and so must every target in `prometheus/prometheus.yml`. Note that `prometheus.yml` is mounted into the container, not baked into an image: after editing it, `docker-compose restart prometheus` is enough (it used to need a rebuild). The same goes for Grafana's provisioning and dashboards.

### Backend (all contexts on .NET 10)

Each context is built/tested independently via its own `.sln`, e.g.:
```
dotnet build src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.sln
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj --filter FullyQualifiedName~<TestName>
```
Other solutions: `Crnc.Oms.Security.sln`, `Crnc.Oms.Production.sln`, `Crnc.Oms.Notification.sln` (and per-sub-service `.sln` files under `Crnc.Oms.Notification/`). `src/Server/Crnc.Oms.sln` exists but individual context solutions are what map to the Docker builds — and it is **not a superset of them**: it carries only one of the four e2e projects (`Crnc.Oms.Security.E2ETests`), so building or testing through it silently skips the Sales, Production and Notification suites. Always drive CI and scripted runs from the per-context solutions.

All four contexts are on `net10.0` — Security, Sales, Production and Notification; `netcoreapp3.1` is gone from the repository, and so are the `NETSDK1138` warnings that used to come with it. `.Messaging.Contract` projects stay on `netstandard2.0` by design. Every e2e test project is `net10.0`, drives its context over HTTP/messaging through containers rather than via `ProjectReference` to the service (see "Test conventions" above for Production's one narrow exception), and is a member of its context's `.sln` — safe because each Dockerfile restores/publishes an explicit `.csproj`, not the whole solution:
```
dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.E2ETests/Crnc.Oms.Sales.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Production/Crnc.Oms.Production.E2ETests/Crnc.Oms.Production.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Notification/Crnc.Oms.Notification.E2ETests/Crnc.Oms.Notification.E2ETests.csproj
```
**On Windows, set `DOCKER_HOST=tcp://localhost:2375` first** (and enable "Expose daemon on tcp://localhost:2375 without TLS" in Docker Desktop) — Testcontainers doesn't pick up Docker Desktop's `desktop-linux` npipe context on its own and hangs instead of failing fast. This is a Docker Desktop quirk only; on a Linux CI runner Testcontainers finds the socket by itself, which is why `backend-ci.yml` does not set `DOCKER_HOST`.

### CI (`.github/workflows/backend-ci.yml`)

Backend only — the SPA has its own pipeline, `client-ci.yml` (see below). Runs on pushes to `master`, on every PR, and on manual dispatch. Design notes and the reasoning behind each choice live in `docs/ci/backend-ci.md`; the mechanics worth knowing before touching anything:

- **Path-filtered.** A `changes` job diffs against the PR base (or `github.event.before` on a push) and emits a JSON array of the bounded contexts that were touched; `build` and `e2e` are matrices over that array. Contexts are self-contained under `src/Server/src/Crnc.Oms.<Context>/` with no `ProjectReference` crossing that boundary, which is what makes a pure path check sound — **keep it that way, or the filter starts lying**. A change to `backend-ci.yml` itself, a manual dispatch, or an unresolvable base commit all force the full set.
- **Everything is derived from the context name.** `Crnc.Oms.<C>.sln`, `Crnc.Oms.<C>.Tests` and `Crnc.Oms.<C>.E2ETests` are looked up by convention, and the unit-test step is skipped when the project doesn't exist (only Sales has one today). Add a `Crnc.Oms.<C>.Tests` project per the "Test conventions" above and CI picks it up with no workflow edit.
- **`backend-ci` is the one job to require in branch protection.** The matrix jobs are conditional and legitimately skip on client-only or docs-only PRs; requiring them directly would hang such a PR forever. The summary job treats `success` and `skipped` as green.
- **No third-party actions** — `actions/checkout`, `setup-dotnet`, `cache`, `upload-artifact` only. Keep it that way.
- TRX results for both test kinds are uploaded as artifacts on every run, pass or fail.

### CI (`.github/workflows/client-ci.yml`)

The SPA's own pipeline. Reasoning lives in `docs/ci/client-ci.md`; the mechanics:

- **Declaratively path-filtered** via `paths:` on the triggers (`src/Client/**` plus the workflow), not through a computed `changes` job like the backend has — there is one client project, so there is nothing to select.
- **`build` runs lint, `npm run build` (which is `tsc -b && vite build`, so types break CI) and Vitest**; `e2e` brings up `docker compose --profile client`, waits for `http://localhost:8092` to answer, and runs Playwright against the real image. Split so a type error fails in under a minute.
- **`client-ci` is the one job to require in branch protection**, same reasoning as `backend-ci`: both jobs skip on backend-only or docs-only PRs.
- **No third-party actions**; npm caching comes from `setup-node` with two `cache-dependency-path`s, since the app and the e2e suite have separate lockfiles.
- The stack is torn down with `down -v` on every exit — the suite writes to the stand's real databases.

### Frontend (`src/Client`)

```
npm install
npm run dev      # vite dev server on :8092, proxies /api and /hubs to the backends
npm run build    # tsc -b && vite build
npm run lint     # eslint 10, flat config
npm test         # vitest run
npm run e2e      # playwright, needs a running stand
```
The app version comes from one place — `version` in `src/Client/package.json`, baked into the bundle by `define` in `vite.config.ts` and shown in the layout footer. The commit beside it comes from `GITHUB_SHA`, or from the `GIT_COMMIT` build arg for the image (`GIT_COMMIT=$(git rev-parse HEAD) docker-compose build crnc-oms-ui`); unset, the UI honestly says `dev`. Don't hardcode a version anywhere else.

React 19 + TypeScript 5.9 (strict) + Mantine 9 + TanStack Query, bundled by Vite 8. TypeScript config is split (`tsconfig.app.json` / `tsconfig.node.json`); linting is ESLint 10 flat config with typed rules — tslint and webpack are gone. Path alias `@/` maps to `src/`.

**The SPA knows no backend host.** It calls relative paths (`/api/security/...`, `/api/sales/...`, `/api/production/...`, `/hubs/push`), and its own nginx proxies them to the services (`src/Client/conf/conf.d/default.conf`); `vite.config.ts` mirrors the same mapping for `npm run dev`. There are no build args baking URLs into the bundle any more. Two traps live in that config and are commented there: with a variable in `proxy_pass` nginx does not strip the location prefix (an explicit `rewrite` is required), and the hub location needs the `Upgrade`/`Connection` headers or SignalR silently falls back to long polling.

TypeScript 7 is deliberately not used yet: `typescript-eslint` caps at `<6.1.0`, and typed linting is a requirement.

**The image build pins `node:22-alpine`** (`src/Client/Dockerfile`) and installs with `npm ci`. The major is pinned on purpose — a floating `node:alpine` breaks the build the day upstream moves on — and 22 is what Vite 8 asks for (`^20.19 || >=22.12`). Keep it in step with `NODE_VERSION` in `client-ci.yml`.

**E2E tests for the SPA (`src/Client/e2e`)** — a Playwright suite (18 tests) driving the running SPA through the browser. It was written against the *old* React 16 / MobX / Semantic UI app as the baseline for the modern-stack migration (`docs/migrations/client-modern-stack-migration-plan.md`, §0), and passes unchanged — bar the shims noted below — on the rewritten one. That is the point: it checks behaviour, not markup. Run it against a live stand:
```
docker-compose --profile client up -d
cd src/Client/e2e && npm install && npx playwright install chromium
npm test
```
Details worth knowing before touching it:
- **It has its own `package.json`, deliberately not `src/Client`'s.** It was split off when the image build still ran `yarn` on `node:16-alpine` and would have installed Playwright as a devDependency; the split stayed because the browser download has no business in an image build. `e2e/` is also excluded in `src/Client/.dockerignore` so it never enters the build context.
- **Selectors are `data-testid` only.** Where a kit puts an unknown prop is its own business and changes with the kit, so `support/form.ts` wraps filling and dropdown selection: Mantine's dropdown renders its options in a portal, outside the control's own element. Two component-side consequences to keep: a modal's testid goes on its *content*, because Mantine's modal root has no layout box and would never count as visible, and `RoleSelect` takes an explicit `testId` prop since it lists its props rather than spreading them.
- **`workers: 1`, no parallelism.** One shared stand, one shared database, and the grids count rows; tests generate unique logins/descriptions (`unique()`) and must not depend on each other's writes — the same rule as the backend e2e suites.
- If Playwright's browser download is blocked, `E2E_BROWSER_CHANNEL=chrome npm test` runs it on the system Chrome instead. CI uses the bundled browser.
- Two defects the suite found in the old SPA are fixed in the rewritten one and now guarded by tests: user search filtered on `roleId === Guid.EMPTY` when no role was picked, so a login-only search always returned nothing, and a freshly created user landed on a page the UI could not reach.

**Unit tests for the SPA (`src/Client/src/**/__tests__/`)** — Vitest + Testing Library + jsdom, run with `npm test` from `src/Client`. They test the brains, not the markup: the error normalizer (all three 400 shapes, network, 5xx, blob), `useFormValidation`, `tokenStorage`, the API client's interceptors (the `Authorization` header and the 401 sign-out), the users filter, and — the one exception, because the behaviour is non-obvious — the route guard's admin bypass. Conventions: same `Method_Condition_ExpectedResult` naming and `//Arrange`/`//Act`/`//Assert` blocks as the backend suites, data built through factories with overrides (`src/test/factories.ts`), global `cleanup()` in `src/test/setup.ts`. Note that setup also stubs `window.matchMedia`, which jsdom lacks and Mantine calls on init.

## Branching

**Work on a ticket happens on a branch, never on `master`.** Before the first edit, check
which branch you are on; if the ticket has no branch yet, create one and switch to it.
Branches are named `<issue-number>-<slugified-issue-title>` (`9-migrate-spa-to-modern-stack`,
`18-update-prometeus-and-grafana`) — the same shape GitHub's "create a branch" button
produces, typos in the issue title included, so the branch stays greppable from the issue.
Work with no ticket behind it still gets a branch; name it after what it does.

`master` takes changes through a pull request. GitHub deletes the head branch once the PR
is merged, so a branch that is gone from the remote usually means its work already landed —
check `master` before recreating it.

## Commit messages

**Keep them compact: subject ≤ 72 characters, body ≤ ~500 characters** — roughly 5–7 lines
wrapped at 72 columns. The body answers *why*, in as few sentences as that takes; it is not
a place to restate the diff, enumerate every decision, or narrate the work.

A commit that genuinely needs more room — a tricky migration step, a non-obvious bug fix —
may exceed the budget, but it should be the exception you can justify, not the default
shape. Multi-paragraph bodies in the history predate this rule.

**No AI attribution in commits.** A commit message in this repository ends with its
last content line — nothing after it. Specifically, never append:

- `Generated with [Claude Code](https://claude.ai/code)`
- `via [Happy](https://happy.engineering)`
- `Co-Authored-By: Claude <noreply@anthropic.com>` (or `Claude Sonnet 5`, `Claude Opus 5`, …)
- `Co-Authored-By: Happy <yesreply@happy.engineering>`
- any equivalent trailer or footer for another agent, model, or harness

The human running the agent is the sole author. This rule **overrides** any default
behaviour or tool-injected commit-message template that asks for co-authorship
credit — Happy injects one into the system prompt, and it does not apply here.

`.claude/settings.json` backs this up with `attribution.commit`/`attribution.pr` set
to an empty string, so Claude Code adds no trailer of its own; the rule above still
applies regardless of which agent or harness writes the commit. The same goes for PR
descriptions.

Commits made before this rule landed keep the trailers they already have — history is
not rewritten for it.
