import { Navigate, Route, Routes } from "react-router";
import Layout from "@/components/Layout";
import NotFound from "@/components/NotFound";
import ProtectedRoute from "@/components/ProtectedRoute";
import LoginPage from "@/pages/login/LoginPage";
import { Roles } from "@/types/auth.types";

const managerRoles = [Roles.Manager, Roles.MainManager];

// Вся карта маршрутов в одном месте. Экраны заказов, jobs и пользователей
// подключаются следующими шагами миграции (§6).
export default function AppRoutes() {
    return (
        <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<ProtectedRoute roles={managerRoles} />}>
                <Route element={<Layout />}>
                    <Route index element={<Navigate to="/orders" replace />} />
                </Route>
            </Route>
            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}
