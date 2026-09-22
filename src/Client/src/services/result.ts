// Сервисы не бросают исключения наружу, поэтому в компонентах нет try/catch.
export interface ServiceResult<T> {
    success: boolean;
    data?: T;
    fieldErrors?: FieldErrors;
    generalError?: string;
}

// Ключи совпадают с именами полей формы - это контракт с бэкендом.
export type FieldErrors = Record<string, string[]>;

export function ok<T>(data: T): ServiceResult<T> {
    return { success: true, data };
}
