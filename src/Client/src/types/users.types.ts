export interface UserItem {
    id: string;
    fullName?: string;
    firstName?: string;
    lastName?: string;
    login?: string;
    password?: string;
    email?: string;
    phone?: string;
    roleId?: string;
    role?: string;
    photoBase64?: string;
    photoMimeType?: string;
    isActive: boolean;
}

export interface UserFilter {
    fullName: string;
    login: string;
    roleId?: string;
    isActive: boolean;
}

export interface RoleOption {
    value: string;
    text: string;
}

export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";
