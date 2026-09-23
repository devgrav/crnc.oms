import apiClient from "./apiClient";
import { request, requestVoid } from "./request";
import type { ServiceResult } from "./result";
import type { RoleOption, UserItem } from "@/types/users.types";

export function getUsers(): Promise<ServiceResult<UserItem[]>> {
    return request(() => apiClient.get<UserItem[]>("/security/users"));
}

export function getUser(id: string): Promise<ServiceResult<UserItem>> {
    return request(() => apiClient.get<UserItem>(`/security/users/${id}`));
}

export function getRoles(): Promise<ServiceResult<RoleOption[]>> {
    return request(() => apiClient.get<RoleOption[]>("/security/roles"));
}

export function createUser(user: UserItem): Promise<ServiceResult<void>> {
    return requestVoid(() => apiClient.post("/security/users", user));
}

export function updateUser(user: UserItem): Promise<ServiceResult<void>> {
    return requestVoid(() => apiClient.put(`/security/users/${user.id}`, user));
}

export function deleteUser(id: string): Promise<ServiceResult<void>> {
    return requestVoid(() => apiClient.delete(`/security/users/${id}`));
}
