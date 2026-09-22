import type { UserFilter, UserItem } from "@/types/users.types";

// Роль участвует в фильтре только когда она выбрана: пустой guid истинен, и
// из-за этого поиск по логину раньше всегда возвращал пусто.
export function filterUsers(users: UserItem[], filter: UserFilter): UserItem[] {
    return users.filter((user) => {
        if (filter.fullName && !includes(user.fullName, filter.fullName)) {
            return false;
        }

        if (filter.login && !includes(user.login, filter.login)) {
            return false;
        }

        if (filter.roleId && user.roleId !== filter.roleId) {
            return false;
        }

        return user.isActive === filter.isActive;
    });
}

function includes(value: string | undefined, search: string): boolean {
    return (value ?? "").toLowerCase().includes(search.toLowerCase());
}
