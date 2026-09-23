import { defineConfig, devices } from "@playwright/test";

// Стенд поднимается снаружи (`docker-compose --profile client up`), поэтому без `webServer`:
// тесты гоняются против образа, который уезжает в прод, а не против dev-сервера.
const baseURL = process.env.E2E_BASE_URL ?? "http://localhost:8092";

export default defineConfig({
    testDir: "./tests",
    // Один воркер: общая БД стенда на все контексты, а сетки считают строки.
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
                // Пусто - браузер самого Playwright; E2E_BROWSER_CHANNEL=chrome берёт системный.
                channel: process.env.E2E_BROWSER_CHANNEL,
            },
        },
    ],
});
