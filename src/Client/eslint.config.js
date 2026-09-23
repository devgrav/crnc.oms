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
            ...tseslint.configs.recommendedTypeChecked,
            // Именно configs.flat: eslintrc-вариант этого плагина ESLint 10 не принимает.
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
            "no-console": "error",
            "@typescript-eslint/no-explicit-any": "error",
        },
    },
    {
        files: ["vite.config.ts", "eslint.config.js"],
        languageOptions: { globals: globals.node },
    },
    {
        files: ["**/__tests__/**", "src/test/**"],
        rules: {
            // vi.mocked возвращает ту же функцию, потери this в expect() здесь нет.
            "@typescript-eslint/unbound-method": "off",
        },
    },
);
