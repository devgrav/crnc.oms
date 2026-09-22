import { fileURLToPath, URL } from "node:url";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

export default defineConfig({
    plugins: [react()],
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
    },
    build: {
        outDir: "dist",
        sourcemap: true,
    },
});
