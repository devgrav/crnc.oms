export type CurrentUserRole = "Admin" | "Manager" | "Main manager";

export const Roles: IRoles = {
    Admin: "Admin",
    MainManager: "Main manager",
    Manager: "Manager"
} 

interface IRoles{
    Admin: CurrentUserRole;
    Manager: CurrentUserRole;
    MainManager: CurrentUserRole;
}


