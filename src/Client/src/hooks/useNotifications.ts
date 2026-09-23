import { use } from "react";
import { NotificationsContext, type NotificationsContextValue } from "@/notifications/NotificationsContext";

export function useNotifications(): NotificationsContextValue {
    const context = use(NotificationsContext);

    if (!context) {
        throw new Error("useNotifications must be used inside NotificationsProvider");
    }

    return context;
}
