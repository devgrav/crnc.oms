import type { CurrentUser } from "@/types/auth.types";

// Ключ и хранилище те же, что в старом клиенте: смена схемы хранения токена -
// отдельный разговор про безопасность, в объём миграции она не входит.
const STORAGE_KEY = "crnc.oms.currentUser";

export function readStoredUser(): CurrentUser | null {
    const raw = sessionStorage.getItem(STORAGE_KEY);

    if (!raw) {
        return null;
    }

    try {
        const parsed: unknown = JSON.parse(raw);
        return isCurrentUser(parsed) ? parsed : null;
    } catch {
        return null;
    }
}

export function storeUser(user: CurrentUser): void {
    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(user));
}

export function clearStoredUser(): void {
    sessionStorage.removeItem(STORAGE_KEY);
}

// Интерцептор apiClient читает токен здесь, а не из React-контекста: он живёт
// вне дерева компонентов и должен видеть актуальное значение на каждом запросе.
export function getStoredToken(): string | null {
    return readStoredUser()?.jwt ?? null;
}

function isCurrentUser(value: unknown): value is CurrentUser {
    if (typeof value !== "object" || value === null) {
        return false;
    }

    const candidate = value as Record<string, unknown>;

    return typeof candidate.login === "string"
        && typeof candidate.jwt === "string"
        && typeof candidate.role === "string";
}
