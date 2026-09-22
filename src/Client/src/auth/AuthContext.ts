import { createContext } from "react";
import type { CurrentUser } from "@/types/auth.types";
import type { ServiceResult } from "@/services/result";

export interface AuthContextValue {
    user: CurrentUser | null;
    isAuthenticated: boolean;
    signIn: (login: string, password: string) => Promise<ServiceResult<CurrentUser>>;
    signOut: () => void;
}

// Контекст и провайдер разнесены по файлам: react-refresh требует, чтобы
// модуль с компонентом экспортировал только компоненты.
export const AuthContext = createContext<AuthContextValue | null>(null);
