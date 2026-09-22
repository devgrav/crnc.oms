import { AxiosError, AxiosHeaders, type AxiosResponse } from "axios";
import { describe, expect, it } from "vitest";
import { readBlobError, toFailure } from "../errorHandler";

describe("toFailure", () => {
    it("ValidationProblemDetails_WrappedErrors_MapsToFieldErrors", () => {
        //Arrange - формат контроллеров с [ApiController], например Sales/OrdersController
        const error = axiosErrorWith(400, {
            type: "https://tools.ietf.org/html/rfc7231",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: { jobType: ["Job type is required"], jobDescription: ["Too short"] },
        });

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.success).toBe(false);
        expect(result.fieldErrors).toEqual({
            jobType: ["Job type is required"],
            jobDescription: ["Too short"],
        });
        expect(result.generalError).toBeUndefined();
    });

    it("SerializableError_FlatDictionary_MapsToFieldErrors", () => {
        //Arrange - Security/UsersController отдаёт BadRequest(ModelState) без
        //[ApiController], то есть словарь без обёртки errors
        const error = axiosErrorWith(400, { firstName: ["First name is required"] });

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.fieldErrors).toEqual({ firstName: ["First name is required"] });
    });

    it("PlainString_AuthFailure_MapsToGeneralError", () => {
        //Arrange - Security/AccountsController отдаёт голую строку
        const error = axiosErrorWith(400, "Not valid login or password");

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.generalError).toBe("Not valid login or password");
        expect(result.fieldErrors).toBeUndefined();
    });

    it("FieldErrorsKeys_CamelCase_MatchFormFieldNames", () => {
        //Arrange - контракт: ключи совпадают с именами полей формы, потому что
        //оба бэкенда включают DictionaryKeyPolicy = CamelCase
        const error = axiosErrorWith(400, { errors: { customerAbbreviation: ["Not valid"] } });

        //Act
        const result = toFailure(error);

        //Assert
        expect(Object.keys(result.fieldErrors ?? {})).toEqual(["customerAbbreviation"]);
    });

    it("NoResponse_NetworkFailure_ReportsConnectionProblem", () => {
        //Arrange
        const error = new AxiosError("Network Error");

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.success).toBe(false);
        expect(result.generalError).toContain("Cannot reach the server");
    });

    it("ServerError_500_ReportsServerFailure", () => {
        //Arrange
        const error = axiosErrorWith(500, "<html>Internal Server Error</html>");

        //Act
        const result = toFailure(error);

        //Assert - тело 5xx наружу не протекает
        expect(result.generalError).toContain("server failed");
        expect(result.fieldErrors).toBeUndefined();
    });

    it("Unauthorized_401_AsksToSignInAgain", () => {
        //Arrange
        const error = axiosErrorWith(401, "");

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.generalError).toMatch(/sign in again/i);
    });

    it("Forbidden_403_ReportsNoAccess", () => {
        //Arrange
        const error = axiosErrorWith(403, "");

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.generalError).toContain("no access");
    });

    it("NonAxiosError_UnknownThrow_StillReturnsResult", () => {
        //Arrange - сервис не должен бросать наружу даже то, что не от axios
        const error = new Error("boom");

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.success).toBe(false);
        expect(result.generalError).toBe("Something went wrong.");
    });

    it("ProblemDetailsWithoutErrors_OnlyTitle_UsesTitle", () => {
        //Arrange
        const error = axiosErrorWith(400, { title: "Order cannot be edited", status: 400 });

        //Act
        const result = toFailure(error);

        //Assert
        expect(result.generalError).toBe("Order cannot be edited");
    });
});

describe("readBlobError", () => {
    it("JsonInsideBlob_ErrorResponse_ParsesPayload", async () => {
        //Arrange - классическая дыра responseType: "blob": ошибка приезжает
        //JSON'ом внутри бинарного ответа
        const blob = new Blob([JSON.stringify({ title: "Report failed" })]);

        //Act
        const parsed = await readBlobError(blob);

        //Assert
        expect(parsed).toEqual({ title: "Report failed" });
    });

    it("PlainTextBlob_NotJson_ReturnsText", async () => {
        //Arrange
        const blob = new Blob(["upstream timeout"]);

        //Act
        const parsed = await readBlobError(blob);

        //Assert
        expect(parsed).toBe("upstream timeout");
    });
});

function axiosErrorWith(status: number, data: unknown): AxiosError {
    const headers = new AxiosHeaders();
    const config = { headers };
    const response = { status, data, statusText: "", headers, config } as AxiosResponse;

    return new AxiosError("Request failed", String(status), config, null, response);
}
