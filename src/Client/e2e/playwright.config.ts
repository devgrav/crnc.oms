import { defineConfig, devices } from "@playwright/test";

// Базовый URL SPA. Стенд поднимается снаружи (`docker-compose --profile client up`),
// поэтому `webServer` здесь намеренно не настраивается: тесты гоняются против того же
// образа, который уезжает в прод, а не против dev-сервера.
const baseURL = process.env.E2E_BASE_URL ?? "http://localhost:8092";

export default defineConfig({
    testDir: "./tests",
    // Один воркер и никакого параллелизма: все контексты ходят в одну общую БД стенда,
    // а сетки заказов/пользователей считают строки. Детерминированность baseline важнее
    // скорости — набор из десяти сценариев и так проходит за минуты.
    fullyParallel: false,
    workers: 1,
    forbidOnly: !!process.env.CI,
    retries: process.env.CI ? 1 : 0,
    timeout: 60_000,
    expect: { timeout: 10_000 },
    reporter: process.env.CI
        ? [["list"], ["html", { open: "never" }], ["junit", { outputFile: "results/junit.xml" }]]
        : [["list"], ["html", { open: "never" }]],
    use: {
        baseURL,
        trace: "on-first-retry",
        screenshot: "only-on-failure",
        video: "retain-on-failure",
    },
    projects: [
        {
            name: "chromium",
            use: {
                ...devices["Desktop Chrome"],
                // По умолчанию — браузер, который ставит сам Playwright (так гоняется CI).
                // Если его скачать нельзя (корпоративный прокси, офлайн-машина), можно
                // подсунуть системный Chrome: E2E_BROWSER_CHANNEL=chrome npm test
                channel: process.env.E2E_BROWSER_CHANNEL,
            },
        },
    ],
});
