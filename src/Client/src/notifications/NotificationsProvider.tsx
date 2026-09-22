import { useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { HubConnectionBuilder, type HubConnection } from "@microsoft/signalr";
import { NotificationsContext, type NotificationsContextValue } from "./NotificationsContext";
import { getStoredToken } from "@/auth/tokenStorage";
import { useAuth } from "@/hooks/useAuth";

interface NotificationsProviderProps {
    children: ReactNode;
}

export default function NotificationsProvider({ children }: NotificationsProviderProps) {
    const { isAuthenticated } = useAuth();
    const [messages, setMessages] = useState<string[]>([]);

    // Подстройка состояния под изменившийся вход, а не синхронизация с внешней
    // системой: сама подписка живёт в эффекте ниже.
    const [wasAuthenticated, setWasAuthenticated] = useState(isAuthenticated);

    if (wasAuthenticated !== isAuthenticated) {
        setWasAuthenticated(isAuthenticated);

        if (!isAuthenticated) {
            setMessages([]);
        }
    }

    useEffect(() => {
        if (!isAuthenticated) {
            return;
        }

        // Адрес относительный: хаб проксирует тот же nginx, что и API.
        const connection: HubConnection = new HubConnectionBuilder()
            .withUrl("/hubs/push", { accessTokenFactory: () => getStoredToken() ?? "" })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceivePushMessageAsync", (_userId: string, message: string) => {
            setMessages((current) => [...current, message]);
        });

        void connection.start();

        return () => {
            void connection.stop();
        };
    }, [isAuthenticated]);

    const clear = useCallback(() => { setMessages([]); }, []);

    const value = useMemo<NotificationsContextValue>(
        () => ({ messages, clear }),
        [messages, clear],
    );

    return <NotificationsContext value={value}>{children}</NotificationsContext>;
}
