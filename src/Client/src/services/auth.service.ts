import apiClient from "./apiClient";
import { request } from "./request";
import type { ServiceResult } from "./result";
import type { Credentials, CurrentUser } from "@/types/auth.types";

export function signIn(credentials: Credentials): Promise<ServiceResult<CurrentUser>> {
    return request(() => apiClient.post<CurrentUser>("/security/accounts/auth", credentials));
}
