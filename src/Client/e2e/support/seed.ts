// Учётки сида security-db: если сид поменяется - править здесь, а не по тестам.
export const SeedUsers = {
    admin: { login: "admin", password: "111111", fullName: "John Admin", role: "Admin" },
    // Именно "Main manager": push о смене статуса Sales шлёт главным менеджерам.
    mainManager: { login: "shon_bean", password: "111111", role: "Main manager" },
    manager: { login: "agness_stuart", password: "111111", role: "Manager" },
} as const;

export type SeedUser = (typeof SeedUsers)[keyof typeof SeedUsers];

// Стенд один на весь прогон: тесты не должны зависеть от записей друг друга.
export function unique(prefix: string): string {
    return `${prefix}_${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`;
}
