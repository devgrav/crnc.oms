import { act, renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { useFormValidation } from "../useFormValidation";

describe("useFormValidation", () => {
    it("SetFromResult_FieldErrors_ExposesMessagesPerField", () => {
        //Arrange
        const { result } = renderHook(() => useFormValidation());

        //Act
        act(() => {
            result.current.setFromResult({
                success: false,
                fieldErrors: { login: ["Login is required", "Too short"] },
            });
        });

        //Assert
        expect(result.current.hasErrors).toBe(true);
        expect(result.current.getErrorMessage("login")).toBe("Login is required Too short");
        expect(result.current.getErrorMessage("email")).toBeUndefined();
    });

    it("ClearFieldError_FirstUserInput_DropsOnlyThatField", () => {
        //Arrange
        const { result } = renderHook(() => useFormValidation());
        act(() => {
            result.current.setFromResult({
                success: false,
                fieldErrors: { login: ["Required"], email: ["Required"] },
            });
        });

        //Act
        act(() => { result.current.clearFieldError("login"); });

        //Assert
        expect(result.current.getErrorMessage("login")).toBeUndefined();
        expect(result.current.getErrorMessage("email")).toBe("Required");
        expect(result.current.hasErrors).toBe(true);
    });

    it("ClearFieldError_UnknownField_KeepsStateIdentity", () => {
        //Arrange - лишний рендер на каждый ввод в поле без ошибки не нужен
        const { result } = renderHook(() => useFormValidation());
        act(() => {
            result.current.setFromResult({ success: false, fieldErrors: { login: ["Required"] } });
        });
        const before = result.current.errors;

        //Act
        act(() => { result.current.clearFieldError("phone"); });

        //Assert
        expect(result.current.errors).toBe(before);
    });

    it("SetFromResult_GeneralError_ExposesItSeparately", () => {
        //Arrange
        const { result } = renderHook(() => useFormValidation());

        //Act
        act(() => {
            result.current.setFromResult({ success: false, generalError: "Not valid login or password" });
        });

        //Assert
        expect(result.current.generalError).toBe("Not valid login or password");
        expect(result.current.errors).toEqual({});
        expect(result.current.hasErrors).toBe(true);
    });

    it("ClearAllErrors_BeforeResubmit_ResetsEverything", () => {
        //Arrange
        const { result } = renderHook(() => useFormValidation());
        act(() => {
            result.current.setFromResult({
                success: false,
                fieldErrors: { login: ["Required"] },
                generalError: "Something went wrong.",
            });
        });

        //Act
        act(() => { result.current.clearAllErrors(); });

        //Assert
        expect(result.current.hasErrors).toBe(false);
        expect(result.current.generalError).toBe("");
        expect(result.current.errors).toEqual({});
    });

    it("SetFromResult_SuccessfulRetry_ClearsPreviousErrors", () => {
        //Arrange - сценарий «исправил и сохранил ещё раз»
        const { result } = renderHook(() => useFormValidation());
        act(() => {
            result.current.setFromResult({ success: false, fieldErrors: { login: ["Required"] } });
        });

        //Act
        act(() => { result.current.setFromResult({ success: true }); });

        //Assert
        expect(result.current.hasErrors).toBe(false);
    });
});
