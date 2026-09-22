import { useMemo, useState } from "react";
import {
    Alert,
    Avatar,
    Button,
    Card,
    Group,
    LoadingOverlay,
    Modal,
    Pagination,
    Popover,
    SimpleGrid,
    Stack,
    Text,
} from "@mantine/core";
import { IconPencil, IconSearch, IconUser, IconUserPlus, IconX } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { Link, Outlet } from "react-router";
import UserFilterForm from "./UserFilterForm";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { filterUsers } from "./filterUsers";
import { photoSrc } from "./userPhoto";
import { deleteUser, getUsers } from "@/services/users.service";
import type { UserFilter, UserItem } from "@/types/users.types";

const usersPerPage = 8;

const emptyFilter: UserFilter = {
    fullName: "",
    login: "",
    roleId: undefined,
    isActive: true,
};

export default function UsersPage() {
    const queryClient = useQueryClient();
    const { data: users = [], isLoading, error } = useServiceQuery(["users"], getUsers);

    const [filter, setFilter] = useState<UserFilter>(emptyFilter);
    const [appliedFilter, setAppliedFilter] = useState<UserFilter>(emptyFilter);
    const [page, setPage] = useState(1);
    const [isFilterOpen, setIsFilterOpen] = useState(false);
    const [userToDelete, setUserToDelete] = useState<UserItem | null>(null);

    const visibleUsers = useMemo(() => filterUsers(users, appliedFilter), [users, appliedFilter]);
    const totalPages = Math.max(1, Math.ceil(visibleUsers.length / usersPerPage));
    const pageUsers = visibleUsers.slice((page - 1) * usersPerPage, page * usersPerPage);

    async function handleDeleteConfirmed() {
        if (!userToDelete) {
            return;
        }

        await deleteUser(userToDelete.id);
        setUserToDelete(null);
        await queryClient.invalidateQueries({ queryKey: ["users"] });
    }

    function applySearch() {
        setAppliedFilter(filter);
        setPage(1);
        setIsFilterOpen(false);
    }

    function clearSearch() {
        setFilter(emptyFilter);
        setAppliedFilter(emptyFilter);
        setPage(1);
        setIsFilterOpen(false);
    }

    return (
        <div style={{ position: "relative" }}>
            <LoadingOverlay visible={isLoading} />
            {error && <Alert color="red" mb="sm">{error.message}</Alert>}

            <Group justify="flex-end" mb="sm">
                <Button
                    component={Link}
                    to="/users/new"
                    leftSection={<IconUserPlus size={16} />}
                    data-testid="users-add"
                >
                    Add user
                </Button>
                <Popover opened={isFilterOpen} onChange={setIsFilterOpen} position="bottom-end" withArrow>
                    <Popover.Target>
                        <Button
                            variant="light"
                            leftSection={<IconSearch size={16} />}
                            onClick={() => { setIsFilterOpen((open) => !open); }}
                            data-testid="users-search-open"
                        >
                            Search
                        </Button>
                    </Popover.Target>
                    <Popover.Dropdown>
                        <UserFilterForm
                            filter={filter}
                            onChange={setFilter}
                            onSearch={applySearch}
                            onClear={clearSearch}
                        />
                    </Popover.Dropdown>
                </Popover>
            </Group>

            <SimpleGrid cols={{ base: 1, sm: 2, lg: 4 }} data-testid="users-cards">
                {pageUsers.map((user) => (
                    <UserCard key={user.id} user={user} onDelete={setUserToDelete} />
                ))}
            </SimpleGrid>

            {totalPages > 1 && (
                <Group justify="center" mt="md">
                    <Pagination total={totalPages} value={page} onChange={setPage} />
                </Group>
            )}

            <Modal
                opened={userToDelete !== null}
                onClose={() => { setUserToDelete(null); }}
                title={`Delete of user ${userToDelete?.fullName ?? ""}`}
                data-testid="user-delete-confirm"
            >
                <Group justify="flex-end">
                    <Button variant="default" onClick={() => { setUserToDelete(null); }}>Cancel</Button>
                    <Button color="red" onClick={() => void handleDeleteConfirmed()}>OK</Button>
                </Group>
            </Modal>

            <Outlet />
        </div>
    );
}

function UserCard({ user, onDelete }: { user: UserItem; onDelete: (user: UserItem) => void }) {
    return (
        <Card withBorder padding="sm" data-testid="user-card">
            <Stack gap={4}>
                <Group justify="space-between" wrap="nowrap">
                    <Group gap="xs" wrap="nowrap">
                        <Avatar src={photoSrc(user)} alt={user.fullName} radius="sm" data-testid="user-photo">
                            <IconUser size={18} />
                        </Avatar>
                        <Text fw={600} data-testid="user-fullname">{user.fullName}</Text>
                    </Group>
                    <Group gap={4} wrap="nowrap">
                        <Button
                            component={Link}
                            to={`/users/${user.id}`}
                            size="compact-xs"
                            variant="light"
                            data-testid="user-edit"
                            aria-label={`Edit ${user.login ?? ""}`}
                        >
                            <IconPencil size={14} />
                        </Button>
                        <Button
                            size="compact-xs"
                            variant="light"
                            color="red"
                            onClick={() => { onDelete(user); }}
                            data-testid="user-delete"
                            aria-label={`Delete ${user.login ?? ""}`}
                        >
                            <IconX size={14} />
                        </Button>
                    </Group>
                </Group>
                <Text size="sm" c="dimmed">{user.role}</Text>
                <Text size="sm">Login: {user.login}</Text>
                <Text size="sm">Email: {user.email}</Text>
                <Text size="sm">Phone: {user.phone ?? ""}</Text>
                <Text size="sm">{user.isActive ? "Active" : "Inactive"}</Text>
            </Stack>
        </Card>
    );
}
