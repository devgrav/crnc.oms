import axios from "axios";
import type { FieldErrors, ServiceResult } from "./result";

const NETWORK_ERROR = "Cannot reach the server. Check your connection and try again.";
const SERVER_ERROR = "The server failed to process the request. Try again later.";
const UNKNOWN_ERROR = "Something went wrong.";

// Бэкенды отдают 400 в трёх разных формах, и все три разбираются здесь:
// ValidationProblemDetails с обёрткой errors (Sales), плоский SerializableError
// (Security/UsersController, без [ApiController]) и голая строка (AccountsController).
export function toFailure<T>(error: unknown): ServiceResult<T> {
    if (!axios.isAxiosError(error)) {
        return { success: false, generalError: UNKNOWN_ERROR };
    }

    if (!error.response) {
        return { success: false, generalError: NETWORK_ERROR };
    }

    const status: number = error.response.status;
    const data: unknown = error.response.data;

    if (status >= 500) {
        return { success: false, generalError: SERVER_ERROR };
    }

    if (status === 401) {
        return { success: false, generalError: "Your session has expired. Sign in again." };
    }

    if (status === 403) {
        return { success: false, generalError: "You have no access to this operation." };
    }

    return parseBadRequest<T>(data);
}

function parseBadRequest<T>(data: unknown): ServiceResult<T> {
    if (typeof data === "string" && data.length > 0) {
        return { success: false, generalError: data };
    }

    if (!isRecord(data)) {
        return { success: false, generalError: UNKNOWN_ERROR };
    }

    const problemDetailsErrors = data.errors;

    if (isRecord(problemDetailsErrors)) {
        return { success: false, fieldErrors: toFieldErrors(problemDetailsErrors) };
    }

    const fieldErrors = toFieldErrors(data);

    if (Object.keys(fieldErrors).length > 0) {
        return { success: false, fieldErrors };
    }

    const title = data.title;

    return {
        success: false,
        generalError: typeof title === "string" ? title : UNKNOWN_ERROR,
    };
}

function toFieldErrors(source: Record<string, unknown>): FieldErrors {
    const result: FieldErrors = {};

    // Ошибкой поля считается только массив строк: иначе ProblemDetails без errors
    // подсвечивает несуществующие поля вместо общего сообщения.
    for (const [field, messages] of Object.entries(source)) {
        if (Array.isArray(messages) && messages.every((m) => typeof m === "string")) {
            result[field] = messages;
        }
    }

    return result;
}

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

// Ошибка внутри бинарного ответа - классическая дыра responseType: "blob".
export async function readBlobError(blob: Blob): Promise<unknown> {
    const text = await blob.text();

    try {
        return JSON.parse(text) as unknown;
    } catch {
        return text;
    }
}
