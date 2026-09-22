import { Button, Checkbox, Group, Stack, TextInput } from "@mantine/core";
import RoleSelect from "./RoleSelect";
import type { UserFilter } from "@/types/users.types";

interface UserFilterFormProps {
    filter: UserFilter;
    onChange: (filter: UserFilter) => void;
    onSearch: () => void;
    onClear: () => void;
}

export default function UserFilterForm({ filter, onChange, onSearch, onClear }: UserFilterFormProps) {
    return (
        <Stack gap="sm" w={260}>
            <TextInput
                label="Full name"
                value={filter.fullName}
                onChange={(event) => { onChange({ ...filter, fullName: event.currentTarget.value }); }}
                data-testid="user-search-fullName"
            />
            <TextInput
                label="Login"
                value={filter.login}
                onChange={(event) => { onChange({ ...filter, login: event.currentTarget.value }); }}
                data-testid="user-search-login"
            />
            <RoleSelect
                value={filter.roleId}
                onChange={(roleId) => { onChange({ ...filter, roleId: roleId ?? undefined }); }}
                testId="user-search-role"
            />
            <Checkbox
                label="Active"
                checked={filter.isActive}
                onChange={(event) => { onChange({ ...filter, isActive: event.currentTarget.checked }); }}
                data-testid="user-search-isActive"
            />
            <Group justify="flex-end">
                <Button variant="default" onClick={onClear} data-testid="user-search-clear">
                    Clear
                </Button>
                <Button onClick={onSearch} data-testid="user-search-submit">
                    Search
                </Button>
            </Group>
        </Stack>
    );
}
