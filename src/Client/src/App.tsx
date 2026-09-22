import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router";
import AuthProvider from "@/auth/AuthProvider";
import NotificationsProvider from "@/notifications/NotificationsProvider";
import ErrorBoundary from "@/components/ErrorBoundary";
import AppRoutes from "@/routes";
import "@mantine/core/styles.css";

// Серверные данные живут в кэше React Query, а не в сторах: загрузка, isLoading,
// инвалидация после мутаций - его работа. См. §3.2 плана миграции.
const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
        },
    },
});

export default function App() {
    return (
        <ErrorBoundary>
            <MantineProvider>
                <QueryClientProvider client={queryClient}>
                    <BrowserRouter>
                        <AuthProvider>
                            <NotificationsProvider>
                                <AppRoutes />
                            </NotificationsProvider>
                        </AuthProvider>
                    </BrowserRouter>
                </QueryClientProvider>
            </MantineProvider>
        </ErrorBoundary>
    );
}
