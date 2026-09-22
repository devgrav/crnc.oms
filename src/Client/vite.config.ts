/// <reference types="vitest/config" />
import { readFileSync } from "node:fs";
import { fileURLToPath, URL } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Версия приложения берётся из одного источника - package.json - и запекается
// в бандл на сборке. Руками её нигде больше не правят; в UI она приходит из
// сборочного артефакта, а не из отдельной константы в коде.
const packageJson = JSON.parse(
    readFileSync(new URL("./package.json", import.meta.url), "utf8"),
) as { version: string };

// Коммит подставляет CI (GITHUB_SHA). Локальная сборка его не знает и не должна:
// "dev" честнее, чем выдуманный хеш.
const commit = (process.env.GITHUB_SHA ?? "").slice(0, 7) || "dev";

export default defineConfig({
    plugins: [react()],
    define: {
        __APP_VERSION__: JSON.stringify(packageJson.version),
        __APP_COMMIT__: JSON.stringify(commit),
    },
    resolve: {
        // Дублирует paths из tsconfig.json: путь @/ должен разрешаться и компилятором,
        // и бандлером. Относительных ../../../ в коде быть не должно.
        alias: {
            "@": fileURLToPath(new URL("./src", import.meta.url)),
        },
    },
    server: {
        port: 8092,
        host: "localhost",
        // Тот же раскладчик путей, что у nginx в контейнере SPA (conf/conf.d/default.conf):
        // в обоих режимах фронт видит один origin и ходит относительными путями.
        proxy: {
            "/api/security": {
                target: "http://localhost:8090",
                changeOrigin: true,
                rewrite: (path) => path.replace(/^\/api\/security/, "/api"),
            },
            "/api/sales": {
                target: "http://localhost:8091",
                changeOrigin: true,
                rewrite: (path) => path.replace(/^\/api\/sales/, "/api"),
            },
            "/api/production": {
                target: "http://localhost:8098",
                changeOrigin: true,
                rewrite: (path) => path.replace(/^\/api\/production/, "/api"),
            },
            "/hubs/push": {
                target: "http://localhost:8107",
                changeOrigin: true,
                ws: true,
            },
        },
    },
    build: {
        outDir: "dist",
        sourcemap: true,
    },
    test: {
        environment: "jsdom",
        globals: true,
        setupFiles: "./src/test/setup.ts",
        // e2e гоняет Playwright, у него свой раннер и свой package.json.
        exclude: ["e2e/**", "node_modules/**", "dist/**"],
        coverage: {
            provider: "v8",
            reportsDirectory: "coverage",
            reporter: ["text-summary", "lcov"],
        },
    },
});
