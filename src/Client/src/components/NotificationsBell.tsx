import { use } from "react";
import { ActionIcon, Badge, Group, List, Popover, Text } from "@mantine/core";
import { NotificationsContext } from "@/notifications/NotificationsContext";

export default function NotificationsBell() {
    const { messages, clear } = use(NotificationsContext);

    return (
        <Popover position="bottom-end" withArrow onClose={clear}>
            <Popover.Target>
                <Group gap={4}>
                    <ActionIcon variant="subtle" aria-label="Notifications" data-testid="notifications-bell">
                        🔔
                    </ActionIcon>
                    {messages.length > 0 && (
                        <Badge color="blue" data-testid="notifications-count">
                            {messages.length}
                        </Badge>
                    )}
                </Group>
            </Popover.Target>
            <Popover.Dropdown>
                {messages.length > 0 ? (
                    <List size="sm" data-testid="notifications-list">
                        {messages.map((message, index) => (
                            <List.Item key={`${index}-${message}`}>{message}</List.Item>
                        ))}
                    </List>
                ) : (
                    <Text size="sm" data-testid="notifications-empty">No notifications!</Text>
                )}
            </Popover.Dropdown>
        </Popover>
    );
}
