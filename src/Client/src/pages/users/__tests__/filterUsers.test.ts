import { describe, expect, it } from "vitest";
import { filterUsers } from "../filterUsers";
import { makeUser } from "@/test/factories";
import { EMPTY_GUID, type UserFilter } from "@/types/users.types";

const managerRoleId = "29679868-fcfe-4350-913d-526a54ea896d";
const mainManagerRoleId = "f1ba72d8-5ebc-4cc4-8b31-eaa0baa87293";

const baseFilter: UserFilter = { fullName: "", login: "", roleId: undefined, isActive: true };

const users = [
    makeUser({ id: "1", login: "shon_bean", fullName: "Shon Bean", roleId: mainManagerRoleId }),
    makeUser({ id: "2", login: "agness_stuart", fullName: "Agness Stuart", roleId: managerRoleId }),
    makeUser({ id: "3", login: "retired_greg", fullName: "Greg Retired", roleId: managerRoleId, isActive: false }),
];

describe("filterUsers", () => {
    it("LoginOnly_NoRoleSelected_StillFilters", () => {
        //Arrange - регрессия на баг старого экрана: он сравнивал roleId с Guid.EMPTY,
        //который истинен, и поиск по одному логину всегда возвращал пусто (§6.3 плана)
        const filter: UserFilter = { ...baseFilter, login: "shon" };

        //Act
        const result = filterUsers(users, filter);

        //Assert
        expect(result.map((user) => user.login)).toEqual(["shon_bean"]);
    });

    it("EmptyGuidAsRole_TreatedAsNoFilter_DoesNotWipeResults", () => {
        //Arrange - даже если пустой guid всё же доедет до фильтра, он не должен
        //совпасть ни с одним пользователем и обнулить выдачу молча
        const filter: UserFilter = { ...baseFilter, roleId: EMPTY_GUID };

        //Act
        const result = filterUsers(users, filter);

        //Assert
        expect(result).toHaveLength(0);
    });

    it("FullName_DifferentCase_MatchesCaseInsensitively", () => {
        //Arrange
        const filter: UserFilter = { ...baseFilter, fullName: "AGNESS" };

        //Act
        const result = filterUsers(users, filter);

        //Assert
        expect(result.map((user) => user.login)).toEqual(["agness_stuart"]);
    });

    it("Role_Selected_KeepsOnlyThatRole", () => {
        //Arrange
        const filter: UserFilter = { ...baseFilter, roleId: managerRoleId };

        //Act
        const result = filterUsers(users, filter);

        //Assert
        expect(result.map((user) => user.login)).toEqual(["agness_stuart"]);
    });

    it("IsActive_False_ShowsOnlyDeactivated", () => {
        //Arrange
        const filter: UserFilter = { ...baseFilter, isActive: false };

        //Act
        const result = filterUsers(users, filter);

        //Assert
        expect(result.map((user) => user.login)).toEqual(["retired_greg"]);
    });

    it("EmptyFilter_DefaultState_ShowsActiveUsers", () => {
        //Arrange, Act
        const result = filterUsers(users, baseFilter);

        //Assert
        expect(result).toHaveLength(2);
    });

    it("MissingFullName_UserWithoutName_DoesNotThrow", () => {
        //Arrange - поля пользователя необязательны в контракте
        const nameless = [makeUser({ id: "4", login: "ghost", fullName: undefined })];

        //Act
        const result = filterUsers(nameless, { ...baseFilter, fullName: "any" });

        //Assert
        expect(result).toHaveLength(0);
    });
});
