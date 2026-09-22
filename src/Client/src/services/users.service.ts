import apiClient from "./apiClient";
import { toFailure } from "./errorHandler";
import { ok, type ServiceResult } from "./result";
import type { RoleOption, UserItem } from "@/types/users.types";

export async function getUsers(): Promise<ServiceResult<UserItem[]>> {
    try {
        const response = await apiClient.get<UserItem[]>("/security/users");
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}

export async function getRoles(): Promise<ServiceResult<RoleOption[]>> {
    try {
        const response = await apiClient.get<RoleOption[]>("/security/roles");
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}

export async function createUser(user: UserItem): Promise<ServiceResult<void>> {
    try {
        await apiClient.post("/security/users", user);
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}

export async function updateUser(user: UserItem): Promise<ServiceResult<void>> {
    try {
        await apiClient.put(`/security/users/${user.id}`, user);
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}

export async function deleteUser(id: string): Promise<ServiceResult<void>> {
    try {
        await apiClient.delete(`/security/users/${id}`);
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}
