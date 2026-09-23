import { AxiosError, AxiosHeaders, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import apiClient from "../apiClient";
import { clearStoredUser, getStoredToken, storeUser } from "@/auth/tokenStorage";
import { makeCurrentUser } from "@/test/factories";

function runRequestInterceptor(): InternalAxiosRequestConfig {
    const handler = apiClient.interceptors.request as unknown as {
        handlers: { fulfilled: (config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig }[];
    };

    const config = { headers: new AxiosHeaders() } as InternalAxiosRequestConfig;

    return handler.handlers[0].fulfilled(config);
}

function runResponseInterceptor(error: unknown): unknown {
    const handler = apiClient.interceptors.response as unknown as {
        handlers: { rejected: (error: unknown) => unknown }[];
    };

    return handler.handlers[0].rejected(error);
}

function unauthorized(): AxiosError {
    const headers = new AxiosHeaders();
    const config = { headers };
    const response = { status: 401, data: "", statusText: "", headers, config } as AxiosResponse;

    return new AxiosError("Request failed", "401", config, null, response);
}

describe("apiClient", () => {
    beforeEach(() => { clearStoredUser(); });
    afterEach(() => { vi.unstubAllGlobals(); });

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

    it("ResponseInterceptor_ExpiredSession_SignsOutAndLeavesToLogin", () => {
        //Arrange - 401 при живом токене значит протухшую сессию: держать пользователя
        //на экране с сообщением нельзя, токен уже не работает
        const assign = vi.fn();
        vi.stubGlobal("location", { assign });
        storeUser(makeCurrentUser());

        //Act, Assert
        expect(() => runResponseInterceptor(unauthorized())).toThrow();
        expect(getStoredToken()).toBeNull();
        expect(assign).toHaveBeenCalledWith("/login");
    });

    it("ResponseInterceptor_NoStoredToken_DoesNotRedirect", () => {
        //Arrange - 401 на неаутентифицированный запрос не должен уводить с /login по кругу
        const assign = vi.fn();
        vi.stubGlobal("location", { assign });

        //Act, Assert
        expect(() => runResponseInterceptor(unauthorized())).toThrow();
        expect(assign).not.toHaveBeenCalled();
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
