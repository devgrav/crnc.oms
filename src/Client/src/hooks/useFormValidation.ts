import { useCallback, useMemo, useState } from "react";
import type { FieldErrors, ServiceResult } from "@/services/result";

export interface FormValidation {
    errors: FieldErrors;
    generalError: string;
    hasErrors: boolean;
    getErrorMessage: (field: string) => string | undefined;
    setFromResult: (result: ServiceResult<unknown>) => void;
    clearFieldError: (field: string) => void;
    clearAllErrors: () => void;
}

// Одна реализация на всё приложение. В старом коде эта логика жила дважды:
// MobX-классом ValidationInfo и руками в UserCardEdit.onChange.
export function useFormValidation(): FormValidation {
    const [errors, setErrors] = useState<FieldErrors>({});
    const [generalError, setGeneralError] = useState("");

    const setFromResult = useCallback((result: ServiceResult<unknown>) => {
        setErrors(result.fieldErrors ?? {});
        setGeneralError(result.generalError ?? "");
    }, []);

    // Ошибка поля гаснет при первом же вводе пользователя - иначе подсветка
    // держится до следующего сабмита и врёт.
    const clearFieldError = useCallback((field: string) => {
        setErrors((current) => {
            if (!(field in current)) {
                return current;
            }

            const next = { ...current };
            delete next[field];
            return next;
        });
    }, []);

    const clearAllErrors = useCallback(() => {
        setErrors({});
        setGeneralError("");
    }, []);

    const getErrorMessage = useCallback(
        (field: string) => errors[field]?.join(" "),
        [errors],
    );

    const hasErrors = useMemo(
        () => Object.keys(errors).length > 0 || generalError.length > 0,
        [errors, generalError],
    );

    return {
        errors,
        generalError,
        hasErrors,
        getErrorMessage,
        setFromResult,
        clearFieldError,
        clearAllErrors,
    };
}
