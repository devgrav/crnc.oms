import { useCallback, useMemo, useState, type ReactNode } from "react";
import { AuthContext, type AuthContextValue } from "./AuthContext";
import { clearStoredUser, readStoredUser, storeUser } from "./tokenStorage";
import * as authService from "@/services/auth.service";
import type { CurrentUser } from "@/types/auth.types";

interface AuthProviderProps {
    children: ReactNode;
}

// Единственное по-настоящему общее состояние приложения. Старый CurrentUserContext
// был статическим синглтоном, который мутировали вне React, из-за чего смена
// пользователя не вызывала перерисовку - см. §5.3 плана миграции.
export default function AuthProvider({ children }: AuthProviderProps) {
    const [user, setUser] = useState<CurrentUser | null>(() => readStoredUser());

    const signIn = useCallback(async (login: string, password: string) => {
        const result = await authService.signIn({ login, password });

        if (result.success && result.data) {
            storeUser(result.data);
            setUser(result.data);
        }

        return result;
    }, []);

    const signOut = useCallback(() => {
        clearStoredUser();
        setUser(null);
    }, []);

    const value = useMemo<AuthContextValue>(
        () => ({ user, isAuthenticated: user !== null, signIn, signOut }),
        [user, signIn, signOut],
    );

    return <AuthContext value={value}>{children}</AuthContext>;
}
