import { lazy, Suspense } from "react";
import { Loader, Center } from "@mantine/core";
import { Navigate, Route, Routes } from "react-router";
import Layout from "@/components/Layout";
import NotFound from "@/components/NotFound";
import ProtectedRoute from "@/components/ProtectedRoute";
import LoginPage from "@/pages/login/LoginPage";
import JobsPage from "@/pages/jobs/JobsPage";
import OrdersPage from "@/pages/orders/OrdersPage";
import UsersPage from "@/pages/users/UsersPage";
import UserCardPage from "@/pages/users/UserCardPage";
import { Roles } from "@/types/auth.types";

const OrderCardPage = lazy(() => import("@/pages/orders/OrderCardPage"));

const managerRoles = [Roles.Manager, Roles.MainManager];

export default function AppRoutes() {
    return (
        <Routes>
            <Route path="/login" element={<LoginPage />} />

            <Route element={<ProtectedRoute roles={managerRoles} />}>
                <Route element={<Layout />}>
                    <Route index element={<Navigate to="/orders" replace />} />
                    <Route path="orders" element={<OrdersPage />}>
                        <Route path="new" element={<LazyOrderCard />} />
                        <Route path=":id" element={<LazyOrderCard />} />
                    </Route>
                    <Route path="jobs" element={<JobsPage />} />
                </Route>
            </Route>

            <Route element={<ProtectedRoute roles={[]} />}>
                <Route element={<Layout />}>
                    <Route path="users" element={<UsersPage />}>
                        <Route path="new" element={<UserCardPage />} />
                        <Route path=":id" element={<UserCardPage />} />
                    </Route>
                </Route>
            </Route>

            <Route path="*" element={<NotFound />} />
        </Routes>
    );
}

function LazyOrderCard() {
    return (
        <Suspense fallback={<Center p="lg"><Loader /></Center>}>
            <OrderCardPage />
        </Suspense>
    );
}
