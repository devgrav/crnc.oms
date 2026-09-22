import type { CurrentUser } from "@/types/auth.types";
import type { OrderRow } from "@/types/orders.types";
import type { UserItem } from "@/types/users.types";

export function makeCurrentUser(overrides: Partial<CurrentUser> = {}): CurrentUser {
    return {
        id: "2a89985f-f013-4f2a-9545-395efb43a142",
        login: "admin",
        fullName: "John Admin",
        role: "Admin",
        jwt: "test-jwt",
        ...overrides,
    };
}

export function makeUser(overrides: Partial<UserItem> = {}): UserItem {
    return {
        id: "11111111-1111-1111-1111-111111111111",
        login: "shon_bean",
        fullName: "Shon Bean",
        firstName: "Shon",
        lastName: "Bean",
        email: "shon_bean@crnc.com",
        roleId: "f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293",
        role: "Main manager",
        isActive: true,
        ...overrides,
    };
}

export function makeOrderRow(overrides: Partial<OrderRow> = {}): OrderRow {
    return {
        id: "5c5c6017-1b1f-4a46-b423-455ad4f273fe",
        number: "5c5c6017",
        createdDate: "22.09.2026 03:45:10",
        customer: "Some Sales Company",
        jobType: "New",
        jobDescription: "Develop new wall",
        dateSentToCustomer: "",
        customerSignOffType: "",
        status: "Not sent",
        statusEnum: 1,
        ...overrides,
    };
}
