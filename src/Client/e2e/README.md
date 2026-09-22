# E2E-набор SPA (Playwright)

Baseline-тесты фронтенда, снятые **до** миграции на современный стек (см.
`docs/migrations/client-modern-stack-migration-plan.md`, §0). Их задача — зафиксировать
наблюдаемое поведение текущего приложения, чтобы миграция ломала верстку, но не функции.

## Почему отдельный `package.json`

Проект намеренно не входит в `src/Client/package.json`. Сборка образа SPA идёт на
`node:16-alpine` и выполняет `yarn` (то есть ставит и devDependencies), а Playwright
требует Node 18+ и на postinstall тянет браузеры. Попади он в тот же манифест — сломался
бы или раздулся бы билд образа. `e2e/` вдобавок исключён из контекста сборки в
`src/Client/.dockerignore`.

## Запуск

Стенд поднимается снаружи, тесты идут против того же образа, который уезжает в прод:

```
docker-compose --profile client up -d      # из корня репозитория
cd src/Client/e2e
npm install
npx playwright install chromium            # один раз
npm test
```

Отчёт после прогона: `npm run report`.

Базовый URL по умолчанию `http://localhost:8092`, переопределяется через `E2E_BASE_URL`.

## Конвенции

- Тесты называются `Method_Condition_ExpectedResult`, в теле — блоки `//Arrange`,
  `//Act`, `//Assert`, как в бэкендовых наборах репозитория.
- Селекторы — **только `data-testid`**. Тексты и классы Semantic UI при смене кита
  изменятся, а testid'ы переносятся в новый код один в один.
- Стенд один на прогон, база общая: данные, создаваемые тестом, обязаны быть
  уникальными (`unique()` из `support/seed.ts`), и тест не должен зависеть от записей,
  созданных соседним тестом.
- `workers: 1` и `fullyParallel: false` — сетки считают строки, а база общая.
- Где тест прикрывает конкретный риск миграции, в `//Arrange` стоит ссылка на параграф
  плана — та же привычка, что в e2e бэкенда.

## Состав

| Файл | Покрывает |
|---|---|
| `login.spec.ts` | вход, выход, редирект неаутентифицированного на `/login` |
| `orders.spec.ts` | сетка заказов, создание, валидация (ProblemDetails), смена статуса |
| `jobs.spec.ts` | сетка jobs |
| `users.spec.ts` | CRUD пользователей, валидация (плоский SerializableError), клиентский поиск |
| `access.spec.ts` | Forbidden для Manager, обход гарда админом, NotFound |
| `push.spec.ts` | цепочка Sales → шина → Gateway → Push → SignalR → колокольчик |
