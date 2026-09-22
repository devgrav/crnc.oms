import { AxiosHeaders, type InternalAxiosRequestConfig } from "axios";
import { beforeEach, describe, expect, it } from "vitest";
import apiClient from "../apiClient";
import { clearStoredUser, storeUser } from "@/auth/tokenStorage";
import { makeCurrentUser } from "@/test/factories";

// Интерцептор вызывается напрямую: важно не то, что axios умеет ходить в сеть,
// а то, что токен читается на каждом запросе.
function runRequestInterceptor(): InternalAxiosRequestConfig {
    const handler = apiClient.interceptors.request as unknown as {
        handlers: { fulfilled: (config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig }[];
    };

    const config = { headers: new AxiosHeaders() } as InternalAxiosRequestConfig;

    return handler.handlers[0].fulfilled(config);
}

describe("apiClient", () => {
    beforeEach(() => { clearStoredUser(); });

    it("BaseUrl_Always_IsRelative", () => {
        //Arrange, Act, Assert - хост бэкенда в бандл не попадает
        expect(apiClient.defaults.baseURL).toBe("/api");
    });

    it("RequestInterceptor_StoredToken_AddsBearerHeader", () => {
        //Arrange
        storeUser(makeCurrentUser({ jwt: "jwt-one" }));

        //Act
        const config = runRequestInterceptor();

        //Assert
        expect(config.headers.Authorization).toBe("Bearer jwt-one");
    });

    it("RequestInterceptor_NoUser_SendsNoAuthorization", () => {
        //Arrange, Act
        const config = runRequestInterceptor();

        //Assert
        expect(config.headers.Authorization).toBeUndefined();
    });

    it("RequestInterceptor_UserSwitched_UsesNewToken", () => {
        //Arrange - старый AxiosProxy запекал токен в инстанс при создании и требовал
        //ручного clear() при смене пользователя; здесь это невозможно по построению
        storeUser(makeCurrentUser({ jwt: "jwt-one" }));
        runRequestInterceptor();

        //Act
        storeUser(makeCurrentUser({ login: "shon_bean", jwt: "jwt-two" }));
        const config = runRequestInterceptor();

        //Assert
        expect(config.headers.Authorization).toBe("Bearer jwt-two");
    });
});
