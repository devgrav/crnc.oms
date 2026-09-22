import apiClient from "./apiClient";
import { toFailure } from "./errorHandler";
import { ok, type ServiceResult } from "./result";
import type { Credentials, CurrentUser } from "@/types/auth.types";

export async function signIn(credentials: Credentials): Promise<ServiceResult<CurrentUser>> {
    try {
        const response = await apiClient.post<CurrentUser>("/security/accounts/auth", credentials);
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}
