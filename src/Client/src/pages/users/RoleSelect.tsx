import { Select } from "@mantine/core";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { getRoles } from "@/services/users.service";

interface RoleSelectProps {
    value: string | undefined;
    onChange: (roleId: string | null) => void;
    error?: string;
    withAsterisk?: boolean;
    testId: string;
}

export default function RoleSelect({ value, onChange, error, withAsterisk, testId }: RoleSelectProps) {
    const { data: roles = [], isLoading } = useServiceQuery(["roles"], getRoles);

    return (
        <Select
            label="Role"
            withAsterisk={withAsterisk}
            data={roles.map((role) => ({ value: role.value, label: role.text }))}
            value={value ?? null}
            onChange={onChange}
            error={error}
            disabled={isLoading}
            data-testid={testId}
        />
    );
}
