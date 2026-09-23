import { useState, type FormEvent } from "react";
import { Alert, Button, Center, Group, List, Loader, LoadingOverlay, Modal } from "@mantine/core";
import { useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router";
import OrderForm from "./OrderForm";
import { useFormValidation } from "@/hooks/useFormValidation";
import { useServiceQuery } from "@/hooks/useServiceQuery";
import { createOrder, getNewOrder, getOrder, updateOrder } from "@/services/orders.service";
import {
    isOrderReadOnly,
    type EditOrderResponse,
    type NewOrderResponse,
    type OrderFormValues,
} from "@/types/orders.types";

type LoadedOrder = NewOrderResponse | EditOrderResponse;

export default function OrderCardPage() {
    // У маршрута /orders/new параметра :id нет вовсе: undefined здесь значит создание.
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const { data, isLoading, error } = useServiceQuery<LoadedOrder>(
        ["order", id ?? "new"],
        () => (id ? getOrder(id) : getNewOrder()),
        { gcTime: 0 },
    );

    function close() {
        void navigate("/orders");
    }

    return (
        <Modal opened onClose={close} size="lg" title={id ? `Edit order ${id}` : "Add new order"}>
            <div data-testid="order-card">
                {error && <Alert color="red">{error.message}</Alert>}
                {isLoading && <Center p="lg"><Loader /></Center>}
                {data && <OrderCardForm loaded={data} orderId={id} onClose={close} />}
            </div>
        </Modal>
    );
}

interface OrderCardFormProps {
    loaded: LoadedOrder;
    orderId: string | undefined;
    onClose: () => void;
}

function OrderCardForm({ loaded, orderId, onClose }: OrderCardFormProps) {
    const queryClient = useQueryClient();
    const validation = useFormValidation();

    const [values, setValues] = useState<OrderFormValues>(() => pickFormValues(loaded));
    const [isSaving, setIsSaving] = useState(false);

    const editResponse = "statuses" in loaded ? loaded : undefined;

    // Считать по values.status нельзя: выбор "Converted to job" погасил бы форму
    // вместе с кнопкой Save, и сохранить перевод стало бы невозможно.
    const readOnly = isOrderReadOnly(editResponse?.status ?? undefined);

    function handleChange<K extends keyof OrderFormValues>(field: K, value: OrderFormValues[K]) {
        setValues((current) => ({ ...current, [field]: value }));
        validation.clearFieldError(field);
    }

    async function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsSaving(true);
        validation.clearAllErrors();

        const result = orderId
            ? await updateOrder({ ...values, id: orderId })
            : await createOrder(values);

        setIsSaving(false);

        if (result.success) {
            await queryClient.invalidateQueries({ queryKey: ["orders"] });
            onClose();
            return;
        }

        validation.setFromResult(result);
    }

    return (
        <>
            <LoadingOverlay visible={isSaving} />
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
                    jobTypes={loaded.jobTypes}
                    statuses={editResponse?.statuses ?? []}
                    materialSources={editResponse?.materialSources ?? []}
                    signoffTypes={editResponse?.signoffTypes ?? []}
                    isEdit={editResponse !== undefined}
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
                    <Button type="button" color="red" variant="outline" onClick={onClose} data-testid="order-cancel">
                        Cancel
                    </Button>
                </Group>
            </form>
        </>
    );
}

function pickFormValues(source: LoadedOrder): OrderFormValues {
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
