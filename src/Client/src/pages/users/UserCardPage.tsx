import { useState, type FormEvent } from "react";
import {
    Alert,
    Avatar,
    Button,
    Checkbox,
    FileButton,
    Group,
    List,
    LoadingOverlay,
    Modal,
    PasswordInput,
    SimpleGrid,
    Stack,
    TextInput,
} from "@mantine/core";
import { IconUpload, IconUser } from "@tabler/icons-react";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router";
import RoleSelect from "./RoleSelect";
import { photoSrc, readPhoto } from "./userPhoto";
import { useFormValidation } from "@/hooks/useFormValidation";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { createUser, getUsers, updateUser } from "@/services/users.service";
import { EMPTY_GUID, type UserItem } from "@/types/users.types";

const emptyUser: UserItem = { id: EMPTY_GUID, isActive: true };

export default function UserCardPage() {
    const { id } = useParams<{ id: string }>();
    // У маршрута /users/new параметра :id нет вовсе: undefined здесь значит создание.
    const isNew = id === undefined || id === "new";

    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const validation = useFormValidation();

    const [user, setUser] = useState<UserItem>(emptyUser);
    const [isSaving, setIsSaving] = useState(false);

    const { data: users = [], isLoading } = useServiceQuery(["users"], getUsers);

    // Подстройка под загруженные данные - в рендере, а не эффектом.
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

    async function handlePhotoSelected(file: File | null) {
        if (!file) {
            return;
        }

        const photo = await readPhoto(file);
        setUser((current) => ({ ...current, ...photo }));
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
            <div data-testid="user-card-edit">
            <LoadingOverlay visible={isLoading || isSaving} />
            <form onSubmit={(event) => void handleSubmit(event)}>
                <Stack gap="sm">
                    <Group gap="md">
                        <Avatar src={photoSrc(user)} alt={user.login ?? ""} size={96} radius="sm" data-testid="user-photo">
                            <IconUser size={40} />
                        </Avatar>
                        <FileButton accept="image/*" onChange={(file) => { void handlePhotoSelected(file); }}>
                            {(props) => (
                                <Button
                                    {...props}
                                    variant="default"
                                    leftSection={<IconUpload size={16} />}
                                    data-testid="user-photo-upload"
                                >
                                    Upload photo
                                </Button>
                            )}
                        </FileButton>
                    </Group>
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
