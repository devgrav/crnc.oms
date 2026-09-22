import { Navigate, Outlet, useLocation } from "react-router";
import Forbidden from "@/components/Forbidden";
import { useAuth } from "@/hooks/useAuth";
import { Roles, type UserRole } from "@/types/auth.types";

interface ProtectedRouteProps {
    roles?: UserRole[];
}

export default function ProtectedRoute({ roles }: ProtectedRouteProps) {
    const { user, isAuthenticated } = useAuth();
    const location = useLocation();

    if (!isAuthenticated || !user) {
        return <Navigate to="/login" replace state={{ from: location.pathname }} />;
    }

    // Админ проходит на любой маршрут независимо от объявленных roles. Поведение
    // намеренное и перенесено из старого PrivateRoute как есть - см. §5.3 плана.
    if (user.role === Roles.Admin) {
        return <Outlet />;
    }

    if (roles && !roles.includes(user.role)) {
        return <Forbidden />;
    }

    return <Outlet />;
}
