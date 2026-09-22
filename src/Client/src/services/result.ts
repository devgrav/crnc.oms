// Сервисы не бросают исключения наружу: любая операция возвращает результат,
// из-за чего в компонентах нет try/catch. См. §4.2-§4.3 плана миграции.
export interface ServiceResult<T> {
    success: boolean;
    data?: T;
    fieldErrors?: FieldErrors;
    generalError?: string;
}

// Ключи совпадают с именами полей формы - это контракт с бэкендом.
// Оба сервиса отдают их в camelCase (DictionaryKeyPolicy в Program.cs).
export type FieldErrors = Record<string, string[]>;

export function ok<T>(data: T): ServiceResult<T> {
    return { success: true, data };
}
