import { useState, type FormEvent } from "react";
import {
    Alert,
    Button,
    Checkbox,
    Group,
    List,
    LoadingOverlay,
    Modal,
    PasswordInput,
    SimpleGrid,
    Stack,
    TextInput,
} from "@mantine/core";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router";
import RoleSelect from "./RoleSelect";
import { useFormValidation } from "@/hooks/useFormValidation";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { createUser, getUsers, updateUser } from "@/services/users.service";
import { EMPTY_GUID, type UserItem } from "@/types/users.types";

const emptyUser: UserItem = { id: EMPTY_GUID, isActive: true };

export default function UserCardPage() {
    const { id } = useParams<{ id: string }>();
    // У маршрута /users/new параметра :id нет вовсе, поэтому undefined здесь
    // означает создание, а не «id ещё не загрузился».
    const isNew = id === undefined || id === "new";

    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const validation = useFormValidation();

    const [user, setUser] = useState<UserItem>(emptyUser);
    const [isSaving, setIsSaving] = useState(false);

    // Карточка открывается поверх уже загруженного списка, поэтому редактируемый
    // пользователь берётся из того же кэша, а не отдельным запросом.
    const { data: users = [], isLoading } = useServiceQuery(["users"], getUsers);

    // Подстройка состояния под источник делается в рендере, а не эффектом: так
    // не возникает каскада рендеров, а ссылки (emptyUser и объект из кэша
    // React Query) стабильны, поэтому ветка срабатывает один раз на источник.
    const source = isNew ? emptyUser : users.find((candidate) => candidate.id === id);
    const [syncedFrom, setSyncedFrom] = useState<UserItem | null>(null);

    if (source && source !== syncedFrom) {
        setSyncedFrom(source);
        setUser(source);
    }

    function close() {
        void navigate("/users");
    }

    function change<K extends keyof UserItem>(field: K, value: UserItem[K]) {
        setUser((current) => ({ ...current, [field]: value }));
        validation.clearFieldError(field);
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsSaving(true);
        validation.clearAllErrors();

        const result = isNew ? await createUser(user) : await updateUser(user);

        setIsSaving(false);

        if (result.success) {
            await queryClient.invalidateQueries({ queryKey: ["users"] });
            close();
            return;
        }

        validation.setFromResult(result);
    }

    return (
        <Modal
            opened
            onClose={close}
            size="lg"
            title={isNew ? "Add new user" : "Edit user"}
        >
            {/* testid на содержимом: корень Mantine Modal не имеет своего бокса. */}
            <div data-testid="user-card-edit">
            <LoadingOverlay visible={isLoading || isSaving} />
            <form onSubmit={(event) => void handleSubmit(event)}>
                <Stack gap="sm">
                    {validation.hasErrors && (
                        <Alert color="red" data-testid="user-validation-summary"
                            title="There was some errors with your submission">
                            <List size="sm">
                                {validation.generalError && <List.Item>{validation.generalError}</List.Item>}
                                {Object.values(validation.errors).flat().map((message) => (
                                    <List.Item key={message}>{message}</List.Item>
                                ))}
                            </List>
                        </Alert>
                    )}
                    <SimpleGrid cols={{ base: 1, sm: 2 }}>
                        <TextInput
                            label="Login"
                            withAsterisk
                            value={user.login ?? ""}
                            onChange={(event) => { change("login", event.currentTarget.value); }}
                            error={validation.getErrorMessage("login")}
                            data-testid="user-login"
                        />
                        <PasswordInput
                            label="Password"
                            withAsterisk
                            value={user.password ?? ""}
                            onChange={(event) => { change("password", event.currentTarget.value); }}
                            error={validation.getErrorMessage("password")}
                            data-testid="user-password"
                        />
                    </SimpleGrid>
                    <RoleSelect
                        value={user.roleId}
                        onChange={(roleId) => { change("roleId", roleId ?? undefined); }}
                        error={validation.getErrorMessage("roleId")}
                        withAsterisk
                        testId="user-role"
                    />
                    <SimpleGrid cols={{ base: 1, sm: 2 }}>
                        <TextInput
                            label="First name"
                            withAsterisk
                            value={user.firstName ?? ""}
                            onChange={(event) => { change("firstName", event.currentTarget.value); }}
                            error={validation.getErrorMessage("firstName")}
                            data-testid="user-firstName"
                        />
                        <TextInput
                            label="Last name"
                            withAsterisk
                            value={user.lastName ?? ""}
                            onChange={(event) => { change("lastName", event.currentTarget.value); }}
                            error={validation.getErrorMessage("lastName")}
                            data-testid="user-lastName"
                        />
                    </SimpleGrid>
                    <SimpleGrid cols={{ base: 1, sm: 2 }}>
                        <TextInput
                            label="Email"
                            withAsterisk
                            type="email"
                            value={user.email ?? ""}
                            onChange={(event) => { change("email", event.currentTarget.value); }}
                            error={validation.getErrorMessage("email")}
                            data-testid="user-email"
                        />
                        <TextInput
                            label="Phone"
                            value={user.phone ?? ""}
                            onChange={(event) => { change("phone", event.currentTarget.value); }}
                            error={validation.getErrorMessage("phone")}
                            data-testid="user-phone"
                        />
                    </SimpleGrid>
                    <Checkbox
                        label="Active"
                        checked={user.isActive}
                        disabled={isNew}
                        onChange={(event) => { change("isActive", event.currentTarget.checked); }}
                        data-testid="user-isActive"
                    />
                    <Group justify="flex-end">
                        <Button type="submit" color="green" loading={isSaving} data-testid="user-save">
                            Save
                        </Button>
                        <Button type="button" color="red" variant="outline" onClick={close}>
                            Cancel
                        </Button>
                    </Group>
                </Stack>
            </form>
            </div>
        </Modal>
    );
}
