import { MantineProvider } from "@mantine/core";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter } from "react-router";
import AuthProvider from "@/auth/AuthProvider";
import NotificationsProvider from "@/notifications/NotificationsProvider";
import ErrorBoundary from "@/components/ErrorBoundary";
import AppRoutes from "@/routes";
import "@mantine/core/styles.css";

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
