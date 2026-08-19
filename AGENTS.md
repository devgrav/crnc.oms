# AGENTS.md

This file provides guidance to AI coding agents (Claude Code, Codex, etc.) when working with code in this repository.

## Project overview

CRNC OMS is an order-management system for a fictional manufacturing company, built as a set of independent .NET microservices (one per bounded context) plus a React SPA. It tracks an order's lifecycle from creation through conversion into a production job.

Bounded contexts / services:
- **Security** (`src/Server/src/Crnc.Oms.Security`) — auth (JWT issuance), user & role CRUD. ASP.NET Core + MongoDB. Caches users in-memory via `IMemoryCache`.
- **Sales** (`src/Server/src/Crnc.Oms.Sales`) — order & customer management, order → job conversion. ASP.NET Core + PostgreSQL (EF Core), RabbitMQ/MassTransit.
- **Production** (`src/Server/src/Crnc.Oms.Production`) — production job management, created in response to order conversion. ASP.NET Core + PostgreSQL (EF Core), RabbitMQ/MassTransit.
- **Notification** (`src/Server/src/Crnc.Oms.Notification`) — three sub-services: `Notification.Gateway` (routes to channels), `Notification.Email`, `Notification.Push` (SignalR), plus a `Notification.Push.Client` console app used to observe push messages in dev.
- **Client** (`src/Client`) — React + TypeScript + Mobx + Semantic UI SPA.

Each backend service is its own independently buildable/deployable solution and Docker image (own `.sln`, own Dockerfile) — there is no shared solution that builds everything at once.

## Repository layout

- `src/Server/` — all backend microservices (see contexts above), each under `src/Server/src/Crnc.Oms.<Context>/`.
- `src/Client/` — the React SPA (single frontend project, source under `src/Client/src/`).
- `prometheus/`, `grafana/` — Docker build contexts for the monitoring stack.
- `docker-compose.yml` (repo root) — wires every service, its DB, and the monitoring stack together for local runs.

## Architecture (per backend service)

Each backend context follows the same layered/DDD structure (project names prefixed `Crnc.Oms.<Context>.`):

- **`.Domain`** — aggregates, domain events, repository interfaces, `SeedWork` (base types: `DomainEntity`, `IAggregateRoot`, `DomainEvent`, `DomainException`, `Enumeration`, `ICurrentUserContext`, `ICurrentDateTimeProvider`, `IDomainEventDispatcher`). No framework dependencies.
- **`.Application`** — use cases, organized under `Features/<Aggregate>/{CommandHandlers,QueryHandlers,EventHandlers,Dto}`. Commands/queries do **not** use MediatR directly — they go through a custom `ICommandQueryDispatcher` / `IUseCaseCommandHandler<TIn,TOut>` / `IUseCaseQueryHandler<TIn,TOut>` pattern (see `Crnc.Oms.Sales.Application/CommandQueryDispatcher.cs`). MediatR *is* used, but only for dispatching domain events (`IDomainEventNotificationHandler`, wired via `IDomainEventDispatcher`).
- **`.DataAccess`** (Sales/Production, EF Core+Postgres) or **`.Infrastructure.DataAccess`** (Security, Mongo) — repository implementations, EF mappings / Mongo mappings, DB initializer/seeder.
- **`.Integration`** — outbound gateways to other services (HTTP clients to other APIs, MassTransit-based gateways), and typed settings bound from `IntegrationEndpoints` config section.
- **`.Messaging.Contract`** — shared MassTransit command/event contracts published/consumed across services (e.g. `SendNotificationToUserCommand`, order/job conversion events).
- **`.WebApi`** — ASP.NET Core host: `Startup.cs` wires DI, EF `DbContext`, MassTransit bus + consumers, JWT auth, NSwag/Swagger, Prometheus metrics, health checks (`/health`). `Controllers/` call the `ICommandQueryDispatcher`; `Consumers/` handle inbound MassTransit messages; `Authorization/` has role-based policy handlers. **Security is the exception** (post-.NET 10 migration): minimal hosting in `Program.cs`, no `Startup.cs`; `Authorization/` only has role constants (`Roles.cs`) and `AuthSettings`, no policy handlers; no MassTransit (pure inbound HTTP).

Cross-service integration is two-pronged:
1. **Sync HTTP** for reads (e.g. Sales calling Security to resolve an employee/manager).
2. **Async messaging via RabbitMQ/MassTransit** for workflow events — e.g. Sales publishes an order-converted event, Production's consumer creates a job and replies with a job-created event, which Sales's consumer uses to store the job id back on the order. Commands (imperative, e.g. `SendNotificationToUserCommand`) and events (past-tense, e.g. job/order state changes) are modeled as distinct MassTransit contract types.

Test coverage today: `Crnc.Oms.Sales.Tests` (xUnit + FluentAssertions) covers the Sales `Domain` layer with unit tests; `Crnc.Oms.Security.E2ETests` (xUnit + FluentAssertions + Testcontainers) covers the Security service end-to-end over HTTP (auth, users CRUD, roles, role-based authorization) against a container built from its real Dockerfile — see "Backend" below for how to run it. Production and Notification.* currently have no automated tests. Every service is expected to eventually have both unit tests (Domain layer, xUnit + FluentAssertions, Sales-style) and integration/e2e tests (Security-style) — add them opportunistically as services get touched, not just during a dedicated migration.

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
| sales-db | PostgreSQL 9.6.17 | `localhost:5433` | `crnc_oms_sales_db` | `postgres` / `docker` |
| production-db | PostgreSQL 9.6.17 | `localhost:5434` | `crnc_oms_production_db` | `postgres` / `docker` |

These are the ports mapped in `docker-compose.yml`; inside the Docker network services reach each other by container name (`security-db`, `sales-db`, `production-db`) on the default port. Note: `Crnc.Oms.Production.WebApi/appsettings.json`'s local (non-Docker) connection string has `Port=5433` (copy-pasted from Sales) instead of `5434` — only matters if you run the Production API with `dotnet run` directly against the Dockerized `production-db`; fix the port locally or override `ConnectionStrings:OmsProductionDb` when doing so.

### Backend (mixed: Security on .NET 10, others on .NET Core 3.1)

Each context is built/tested independently via its own `.sln`, e.g.:
```
dotnet build src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.sln
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj
dotnet test src/Server/src/Crnc.Oms.Sales/Crnc.Oms.Sales.Tests/Crnc.Oms.Sales.Tests.csproj --filter FullyQualifiedName~<TestName>
```
Other solutions: `Crnc.Oms.Security.sln`, `Crnc.Oms.Production.sln`, `Crnc.Oms.Notification.sln` (and per-sub-service `.sln` files under `Crnc.Oms.Notification/`). `src/Server/Crnc.Oms.sln` exists but individual context solutions are what map to the Docker builds.

`Crnc.Oms.Security` was migrated to .NET 10 (`net10.0` across all 4 app projects) — the rest of the backend is still on `netcoreapp3.1`; building `Crnc.Oms.Security.sln` with the .NET 10 SDK is expected to show `NETSDK1138` warnings for the still-3.1 solutions, not for Security's own projects. Security's e2e test project runs on `net10.0` regardless of the API's own TFM (it drives the service over HTTP through a container, not via `ProjectReference`):
```
dotnet test src/Server/src/Crnc.Oms.Security/Crnc.Oms.Security.E2ETests/Crnc.Oms.Security.E2ETests.csproj
```
**On Windows, set `DOCKER_HOST=tcp://localhost:2375` first** (and enable "Expose daemon on tcp://localhost:2375 without TLS" in Docker Desktop) — Testcontainers doesn't pick up Docker Desktop's `desktop-linux` npipe context on its own and hangs instead of failing fast.

### Frontend (`src/Client`)

```
npm install
npm start    # webpack-dev-server, development mode
npm run build   # production bundle (webpack -p)
```
TypeScript config: `tsconfig.json`; linting: `tslint.json` (tslint, not eslint). API base URLs are injected at Docker build time via args (`SECURITY_API_URL`, `SALES_API_URL`, `PRODUCTION_API_URL`, `PUSH_HUBS_URL`) — see `docker-compose.yml`.
