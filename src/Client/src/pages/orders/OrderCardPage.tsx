import { useState, type FormEvent } from "react";
import { Alert, Button, Group, List, LoadingOverlay, Modal } from "@mantine/core";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router";
import OrderForm from "./OrderForm";
import { useFormValidation } from "@/hooks/useFormValidation";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { createOrder, getNewOrder, getOrder, updateOrder } from "@/services/orders.service";
import type { TextValue } from "@/types/api.types";
import {
    isOrderReadOnly,
    type EditOrderResponse,
    type NewOrderResponse,
    type OrderFormValues,
} from "@/types/orders.types";

const emptyValues: OrderFormValues = {
    jobType: 0,
    jobDescription: "",
    customerTitle: "",
    customerAbbreviation: "",
    customerContactPersonFirstName: "",
    customerContactPersonLastName: "",
    customerContactPersonEmail: "",
    customerContactPersonPhone: "",
};

// Одна карточка на создание и редактирование: отличаются набором видимых полей
// и вызываемым сервисом. См. §6.4 плана миграции.
export default function OrderCardPage() {
    const { id } = useParams<{ id: string }>();
    const isEdit = id !== undefined && id !== "new";

    const navigate = useNavigate();
    const queryClient = useQueryClient();
    const validation = useFormValidation();

    const [values, setValues] = useState<OrderFormValues>(emptyValues);
    const [isSaving, setIsSaving] = useState(false);

    const query = useServiceQuery<NewOrderResponse | EditOrderResponse>(
        ["order", id ?? "new"],
        () => (isEdit && id ? getOrder(id) : getNewOrder()),
    );

    const loaded = query.data;

    // Состояние формы подстраивается под пришедшие данные прямо в рендере, а не
    // эффектом: эффект здесь дал бы лишний каскад рендеров. React Query отдаёт
    // стабильную ссылку на закэшированный объект, поэтому срабатывает один раз.
    const [syncedFrom, setSyncedFrom] = useState<object | null>(null);

    if (loaded && loaded !== syncedFrom) {
        setSyncedFrom(loaded);
        setValues(pickFormValues(loaded));
    }

    function close() {
        void navigate("/orders");
    }

    function handleChange<K extends keyof OrderFormValues>(field: K, value: OrderFormValues[K]) {
        setValues((current) => ({ ...current, [field]: value }));
        validation.clearFieldError(field);
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsSaving(true);
        validation.clearAllErrors();

        const result = isEdit && loaded && "id" in loaded
            ? await updateOrder({ ...values, id: loaded.id })
            : await createOrder(values);

        setIsSaving(false);

        if (result.success) {
            await queryClient.invalidateQueries({ queryKey: ["orders"] });
            close();
            return;
        }

        validation.setFromResult(result);
    }

    const readOnly = isEdit && isOrderReadOnly(values.status ?? undefined);
    const editResponse = loaded && "statuses" in loaded ? loaded : undefined;

    return (
        <Modal
            opened
            onClose={close}
            size="lg"
            title={isEdit ? `Edit order ${id ?? ""}` : "Add new order"}
        >
            {/* testid висит на содержимом, а не на Modal: его корневой элемент -
                обёртка без собственного бокса, и тест не считает её видимой. */}
            <div data-testid="order-card">
            <LoadingOverlay visible={query.isLoading || isSaving} />
            <form onSubmit={(event) => void handleSubmit(event)} id="orderForm">
                {validation.hasErrors && (
                    <Alert color="red" mb="sm" data-testid="order-validation-summary"
                        title="There was some errors with your submission">
                        <List size="sm">
                            {validation.generalError && <List.Item>{validation.generalError}</List.Item>}
                            {Object.values(validation.errors).flat().map((message) => (
                                <List.Item key={message}>{message}</List.Item>
                            ))}
                        </List>
                    </Alert>
                )}
                <OrderForm
                    values={values}
                    onChange={handleChange}
                    validation={validation}
                    jobTypes={loaded?.jobTypes ?? []}
                    statuses={optionsOf(editResponse?.statuses)}
                    materialSources={optionsOf(editResponse?.materialSources)}
                    signoffTypes={optionsOf(editResponse?.signoffTypes)}
                    isEdit={isEdit}
                    disabled={readOnly || isSaving}
                    dateSentToCustomer={editResponse?.dateSentToCustomer}
                    jobNumber={editResponse?.jobNumber}
                />
                <Group justify="flex-end" mt="md">
                    {!readOnly && (
                        <Button type="submit" color="green" loading={isSaving} data-testid="order-save">
                            Save
                        </Button>
                    )}
                    <Button type="button" color="red" variant="outline" onClick={close} data-testid="order-cancel">
                        Cancel
                    </Button>
                </Group>
            </form>
            </div>
        </Modal>
    );
}

function optionsOf(items: TextValue[] | undefined): TextValue[] {
    return items ?? [];
}

function pickFormValues(source: NewOrderResponse | EditOrderResponse): OrderFormValues {
    return {
        jobType: source.jobType,
        jobDescription: source.jobDescription,
        customerTitle: source.customerTitle,
        customerAbbreviation: source.customerAbbreviation,
        customerContactPersonFirstName: source.customerContactPersonFirstName,
        customerContactPersonLastName: source.customerContactPersonLastName,
        customerContactPersonEmail: source.customerContactPersonEmail,
        customerContactPersonPhone: source.customerContactPersonPhone,
        status: source.status,
        materialSource: source.materialSource,
        signoffType: source.signoffType,
    };
}
