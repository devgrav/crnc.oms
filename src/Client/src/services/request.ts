import type { AxiosResponse } from "axios";
import { toFailure } from "./errorHandler";
import { ok, type ServiceResult } from "./result";
import type { ItemsResponse } from "@/types/api.types";

// Единственное место, где ошибка HTTP превращается в ServiceResult: отсюда и правило
// "сервисы не бросают наружу".
export async function request<T>(call: () => Promise<AxiosResponse<T>>): Promise<ServiceResult<T>> {
    try {
        const response = await call();
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}

// Sales и Production отдают сетки в обёртке { items: [...] }, причём пустой ответ
// приезжает и вовсе без items.
export async function requestItems<T>(
    call: () => Promise<AxiosResponse<ItemsResponse<T>>>,
): Promise<ServiceResult<T[]>> {
    try {
        const response = await call();
        return ok(response.data.items ?? []);
    } catch (error) {
        return toFailure(error);
    }
}

export async function requestVoid(call: () => Promise<unknown>): Promise<ServiceResult<void>> {
    try {
        await call();
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}
