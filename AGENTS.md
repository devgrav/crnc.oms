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
- `prometheus/`, `grafana/` — Docker build contexts for the monitoring stack.
- `docker-compose.yml` (repo root) — wires every service, its DB, and the monitoring stack together for local runs.
- `docs/migrations/` — written-up plans for cross-cutting migrations (e.g. `security-net10-migration-plan.md`). Put a plan here before starting a multi-service migration, and update it as steps land.
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

Monitoring: Prometheus scrapes each service's `/metrics` endpoint every 5s (via `prometheus-net`); Grafana ships with a default dashboard. Not collected for infra containers (Mongo/Postgres/RabbitMQ).

## Architecture (frontend, `src/Client`)

React + TypeScript + Mobx + Semantic UI, bundled with Webpack (no CRA). Entry point `src/index.tsx` → `src/app.tsx`.

- **Routing & auth guard** (`src/routes.tsx`) — `react-router` v5 `Switch` of routes, each wrapped in a `PrivateRoute` that checks `CurrentUserContext.isAuthentificated` (redirects to `/login`) and then checks the route's declared `roles` against `CurrentUserContext.user.role` (renders `Forbidden` otherwise). Roles: `Admin`, `MainManager`, `Manager` (`src/auth/CurrentUserRole.ts`).
- **Current user** (`src/auth/`) — `CurrentUserContext` is a static singleton holding the logged-in `CurrentUser` (id, jwt, role); persisted to/restored from `sessionStorage` under key `crnc.oms.currentUser` (restored in `App`'s constructor in `app.tsx`).
- **Services** (`src/services/`) — one static class per backend aggregate (`OrderService`, `JobService`, `UserService`, `RoleService`, `AuthService`), each a thin wrapper of `axios` calls to a base URL from `src/config.ts`. `AxiosProxy` is a lazily-created singleton `axios` instance that stamps the `Authorization: Bearer <jwt>` header from `CurrentUserContext`; call `AxiosProxy.clear()` on logout/user-switch to force a fresh instance with the new token.
- **Config** (`src/config.ts`) — reads API base URLs from `process.env.REACT_APP_*` (set as Docker build args in `docker-compose.yml`: `SECURITY_API_URL`, `SALES_API_URL`, `PRODUCTION_API_URL`, `PUSH_HUBS_URL`) and derives per-resource endpoint URLs (`ordersUrl`, `jobsUrl`, `usersUrl`, etc.) — baked in at build time, not runtime-configurable.
- **Feature/store pattern** (`src/components/<feature>/`, e.g. `orders`, `jobs`, `users`) — each screen has a `*Container` component that constructs a per-mount tree of Mobx `RootStore`/`Store` objects and injects them via `mobx-react`'s `<Provider>`; child components are `@inject`/`@observer`-connected to read from those stores rather than props drilling. Stores call the matching `*Service` for I/O and expose `@observable` state + `@action` methods (loading flags, models, CRUD operations). This container→root store→feature store layering is repeated per feature (e.g. `OrdersGridContainer` → `OrdersGridRootStore` → `OrdersGridStore`; same shape for `orderCard`, `jobsGrid`).
- **Push notifications** (`src/components/layout/Notifications.tsx`) — connects directly to the Push service's SignalR hub (`APP_CONFIG.pushUrl`) using `@microsoft/signalr`, authenticating via `accessTokenFactory` (the current JWT), and appends incoming `ReceivePushMessageAsync` messages to local component state (not a Mobx store).
- **Layout** (`src/components/layout/`) — `Layout`/`Content`/`TopMenu`/`UserInfo` wrap every authenticated page (applied inside `PrivateRoute`); `Notifications` (the bell icon) lives in the top menu.

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
docker-compose --profile monitoring up    # prometheus + grafana only
```
Available profiles: `security`, `sales`, `production`, `notification`, `client`, `monitoring`, `full`. Docker Compose does **not** auto-activate a dependency's own profile via `depends_on` — every service lists every context profile that can reach it transitively, so e.g. `security-api` carries `security`, `sales`, `production`, `notification`, and `client` (every context that ends up depending on it), not just `security`. Keep this in sync when changing `depends_on` edges or adding services.

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

**Inside the Docker network every API now listens on 8080**, not 80 — that is the default baked into `mcr.microsoft.com/dotnet/aspnet:10.0`. Host-side ports in the table above are unchanged, so the SPA and README need nothing, but any container-to-container URL must carry `:8080` explicitly, and so must every target in `prometheus/prometheus.yml`. Note that `prometheus.yml` is `ADD`ed at image build time: after editing it, `docker-compose build prometheus` is required or the targets keep the old config.

### Backend (all contexts on .NET 10)

Each context is built/tested independently via its own `.sln`, e.g.:
```
dotnet build src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.sln
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj --filter FullyQualifiedName~<TestName>
```
Other solutions: `Crnc.Oms.Security.sln`, `Crnc.Oms.Production.sln`, `Crnc.Oms.Notification.sln` (and per-sub-service `.sln` files under `Crnc.Oms.Notification/`). `src/Server/Crnc.Oms.sln` exists but individual context solutions are what map to the Docker builds.

All four contexts are on `net10.0` — Security, Sales, Production and Notification; `netcoreapp3.1` is gone from the repository, and so are the `NETSDK1138` warnings that used to come with it. `.Messaging.Contract` projects stay on `netstandard2.0` by design. Every e2e test project is `net10.0`, drives its context over HTTP/messaging through containers rather than via `ProjectReference` to the service (see "Test conventions" above for Production's one narrow exception), and is a member of its context's `.sln` — safe because each Dockerfile restores/publishes an explicit `.csproj`, not the whole solution:
```
dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.E2ETests/Crnc.Oms.Sales.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Production/Crnc.Oms.Production.E2ETests/Crnc.Oms.Production.E2ETests.csproj
dotnet test src/Server/src/Crnc.Oms.Notification/Crnc.Oms.Notification.E2ETests/Crnc.Oms.Notification.E2ETests.csproj
```
**On Windows, set `DOCKER_HOST=tcp://localhost:2375` first** (and enable "Expose daemon on tcp://localhost:2375 without TLS" in Docker Desktop) — Testcontainers doesn't pick up Docker Desktop's `desktop-linux` npipe context on its own and hangs instead of failing fast.

### Frontend (`src/Client`)

```
npm install
npm start    # webpack-dev-server, development mode
npm run build   # production bundle (webpack -p)
```
TypeScript config: `tsconfig.json`; linting: `tslint.json` (tslint, not eslint). API base URLs are injected at Docker build time via args (`SECURITY_API_URL`, `SALES_API_URL`, `PRODUCTION_API_URL`, `PUSH_HUBS_URL`) — see `docker-compose.yml`.

**The image build pins `node:16-alpine` deliberately** (`src/Client/Dockerfile`). The floating `node:alpine` tag now resolves to Node 26, which no longer ships yarn at all — `RUN yarn` fails with `yarn: not found` — and whose OpenSSL 3 dropped the `md4` hash that webpack 3 relies on. Node 16 is the last LTS of this frontend's era and carries yarn 1.22, matching the v1 `yarn.lock`. Don't unpin it without upgrading webpack first.

## Commit messages

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
