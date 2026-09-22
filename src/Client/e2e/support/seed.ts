// Учётки, которыми сидируется security-db при старте стенда. Те же, что в README
// и в AGENTS.md; если сид поменяется — править здесь, а не по тестам.
export const SeedUsers = {
    admin: { login: "admin", password: "111111", fullName: "John Admin", role: "Admin" },
    // shon_bean - именно "Main manager", а не "Manager": уведомления о смене статуса
    // заказа Sales рассылает главным менеджерам (OrderStatusChangedHandler), поэтому
    // на нём же строится push-сценарий.
    mainManager: { login: "shon_bean", password: "111111", role: "Main manager" },
    manager: { login: "agness_stuart", password: "111111", role: "Manager" },
} as const;

export type SeedUser = (typeof SeedUsers)[keyof typeof SeedUsers];

// Уникальный суффикс на тест: фикстура стенда одна на весь прогон, тесты не должны
// зависеть от записей друг друга (та же причина, что в бэкендовых e2e-наборах).
export function unique(prefix: string): string {
    return `${prefix}_${Date.now().toString(36)}${Math.random().toString(36).slice(2, 8)}`;
}
