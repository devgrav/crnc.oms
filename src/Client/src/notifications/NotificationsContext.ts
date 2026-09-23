import { createContext } from "react";

export interface NotificationsContextValue {
    messages: string[];
    clear: () => void;
}

export const NotificationsContext = createContext<NotificationsContextValue | null>(null);
