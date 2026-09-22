export interface CurrentUser {
    id: string;
    login: string;
    fullName: string;
    role: UserRole;
    jwt: string;
}

export type UserRole = "Admin" | "Manager" | "Main manager";

export const Roles = {
    Admin: "Admin",
    Manager: "Manager",
    MainManager: "Main manager",
} as const satisfies Record<string, UserRole>;

export interface Credentials {
    login: string;
    password: string;
}
