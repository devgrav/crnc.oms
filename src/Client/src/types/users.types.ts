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
    // undefined, а не пустой guid: старый экран хранил здесь Guid.EMPTY, который
    // истинен, и из-за этого поиск всегда фильтровал по несуществующей роли.
    roleId?: string;
    isActive: boolean;
}

export interface RoleOption {
    value: string;
    text: string;
}

export const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

export function isNewUser(user: UserItem): boolean {
    return user.id === EMPTY_GUID;
}
