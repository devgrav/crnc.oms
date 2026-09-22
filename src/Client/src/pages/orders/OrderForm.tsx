import { Badge, Divider, Select, Stack, TextInput, Textarea, Title } from "@mantine/core";
import type { TextValue } from "@/types/api.types";
import type { FormValidation } from "@/hooks/useFormValidation";
import type { OrderFormValues } from "@/types/orders.types";

export interface OrderFormProps {
    values: OrderFormValues;
    onChange: <K extends keyof OrderFormValues>(field: K, value: OrderFormValues[K]) => void;
    validation: FormValidation;
    jobTypes: TextValue[];
    statuses: TextValue[];
    materialSources: TextValue[];
    signoffTypes: TextValue[];
    // Статус, источник материала и тип подписи есть только у существующего заказа.
    isEdit: boolean;
    disabled: boolean;
    dateSentToCustomer?: string;
    jobNumber?: string | null;
}

export default function OrderForm(props: OrderFormProps) {
    const { values, onChange, validation, isEdit, disabled } = props;

    function numberField(field: keyof OrderFormValues) {
        return {
            value: values[field] == null ? null : String(values[field]),
            onChange: (value: string | null) => {
                onChange(field, value === null ? null : Number(value));
            },
            error: validation.getErrorMessage(field),
            disabled,
        };
    }

    function textField(field: keyof OrderFormValues) {
        return {
            value: (values[field] as string | undefined) ?? "",
            onChange: (event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                onChange(field, event.currentTarget.value as OrderFormValues[typeof field]);
                validation.clearFieldError(field);
            },
            error: validation.getErrorMessage(field),
            disabled,
        };
    }

    return (
        <Stack gap="sm">
            <Title order={4}>Order</Title>
            <Select
                label="Job type"
                withAsterisk
                data={toOptions(props.jobTypes)}
                data-testid="order-jobType"
                {...numberField("jobType")}
            />
            <Textarea
                label="Job Description"
                withAsterisk
                placeholder="E.g. produce new detail"
                autosize
                minRows={3}
                data-testid="order-jobDescription"
                {...textField("jobDescription")}
            />
            {isEdit && (
                <>
                    <Select
                        label="Status"
                        data={toOptions(props.statuses)}
                        data-testid="order-status-select"
                        {...numberField("status")}
                    />
                    <TextInput
                        label="Date sent to customer"
                        value={props.dateSentToCustomer ?? ""}
                        disabled
                        readOnly
                    />
                    <Select
                        label="Material source"
                        withAsterisk
                        data={toOptions(props.materialSources)}
                        data-testid="order-materialSource"
                        {...numberField("materialSource")}
                    />
                    <Select
                        label="Sign off type"
                        withAsterisk
                        data={toOptions(props.signoffTypes)}
                        data-testid="order-signoffType"
                        {...numberField("signoffType")}
                    />
                </>
            )}

            <Divider />
            <Title order={4}>Customer</Title>
            <TextInput
                label="Title"
                withAsterisk
                placeholder="Awesome Company"
                data-testid="order-customerTitle"
                {...textField("customerTitle")}
            />
            <TextInput
                label="Abbreviation"
                withAsterisk
                placeholder="AC"
                data-testid="order-customerAbbreviation"
                {...textField("customerAbbreviation")}
            />

            <Divider />
            <Title order={4}>Contact Person</Title>
            <TextInput
                label="First name"
                withAsterisk
                placeholder="John"
                data-testid="order-contactFirstName"
                {...textField("customerContactPersonFirstName")}
            />
            <TextInput
                label="Last name"
                withAsterisk
                placeholder="Smith"
                data-testid="order-contactLastName"
                {...textField("customerContactPersonLastName")}
            />
            <TextInput
                label="Email"
                withAsterisk
                placeholder="john_smith@crnc.com"
                data-testid="order-contactEmail"
                {...textField("customerContactPersonEmail")}
            />
            <TextInput
                label="Phone"
                withAsterisk
                placeholder="89161234567"
                data-testid="order-contactPhone"
                {...textField("customerContactPersonPhone")}
            />

            {props.jobNumber && (
                <Badge color="blue">Order converted to job: {props.jobNumber}</Badge>
            )}
        </Stack>
    );
}

function toOptions(items: TextValue[]) {
    return items.map((item) => ({ value: String(item.value), label: item.text }));
}
