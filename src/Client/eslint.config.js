import js from "@eslint/js";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import globals from "globals";
import tseslint from "typescript-eslint";

export default tseslint.config(
    { ignores: ["dist", "e2e"] },
    {
        files: ["**/*.{ts,tsx}"],
        extends: [
            js.configs.recommended,
            // Типизированный линт, а не только синтаксический: правила, которым нужен
            // компилятор, ловят как раз то, ради чего в этом проекте есть strict.
            ...tseslint.configs.recommendedTypeChecked,
            // Именно configs.flat: configs["recommended-latest"] у этого плагина всё
            // ещё в eslintrc-формате (plugins - массив), и ESLint 10 его не принимает.
            reactHooks.configs.flat["recommended-latest"],
            reactRefresh.configs.vite,
        ],
        languageOptions: {
            ecmaVersion: 2022,
            globals: globals.browser,
            parserOptions: {
                project: ["./tsconfig.app.json", "./tsconfig.node.json"],
                tsconfigRootDir: import.meta.dirname,
            },
        },
        rules: {
            // Логирование в продакшн-код не попадает; в старом клиенте console.log
            // висел в config.ts, Notifications и ValidationInfo.
            "no-console": "error",
            "@typescript-eslint/no-explicit-any": "error",
        },
    },
    {
        files: ["vite.config.ts", "eslint.config.js"],
        languageOptions: { globals: globals.node },
    },
);
