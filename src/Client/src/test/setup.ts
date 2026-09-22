import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach, vi } from "vitest";

// jsdom не реализует matchMedia, а Mantine опрашивает его при инициализации темы.
Object.defineProperty(window, "matchMedia", {
    writable: true,
    value: (query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        addListener: vi.fn(),
        removeListener: vi.fn(),
        dispatchEvent: vi.fn(),
    }),
});

// Глобальный cleanup: без него компонент из предыдущего теста остаётся в DOM
// и следующий тест ищет элементы в чужом дереве.
afterEach(() => {
    cleanup();
    sessionStorage.clear();
});
