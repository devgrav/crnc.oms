import { beforeEach, describe, expect, it } from "vitest";
import { clearStoredUser, getStoredToken, readStoredUser, storeUser } from "../tokenStorage";
import { makeCurrentUser } from "@/test/factories";

const STORAGE_KEY = "crnc.oms.currentUser";

describe("tokenStorage", () => {
    beforeEach(() => { sessionStorage.clear(); });

    it("ReadStoredUser_AfterStore_ReturnsSameUser", () => {
        //Arrange
        const user = makeCurrentUser();

        //Act
        storeUser(user);

        //Assert
        expect(readStoredUser()).toEqual(user);
        expect(getStoredToken()).toBe(user.jwt);
    });

    it("ReadStoredUser_NothingStored_ReturnsNull", () => {
        //Arrange, Act, Assert
        expect(readStoredUser()).toBeNull();
        expect(getStoredToken()).toBeNull();
    });

    it("ReadStoredUser_CorruptedJson_ReturnsNullInsteadOfThrowing", () => {
        //Arrange - в sessionStorage может лежать что угодно, в том числе мусор
        sessionStorage.setItem(STORAGE_KEY, "{not json");

        //Act, Assert
        expect(readStoredUser()).toBeNull();
    });

    it("ReadStoredUser_ShapeFromAnotherVersion_ReturnsNull", () => {
        //Arrange - структура из прошлой версии приложения не должна попадать
        //в контекст наполовину разобранной
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify({ login: "admin" }));

        //Act, Assert
        expect(readStoredUser()).toBeNull();
    });

    it("ClearStoredUser_AfterSignOut_RemovesToken", () => {
        //Arrange
        storeUser(makeCurrentUser());

        //Act
        clearStoredUser();

        //Assert
        expect(getStoredToken()).toBeNull();
    });
});
