import apiClient from "./apiClient";
import { toFailure } from "./errorHandler";
import { ok, type ServiceResult } from "./result";
import type { ItemsResponse } from "@/types/api.types";
import type {
    EditOrderResponse,
    NewOrderResponse,
    OrderFormValues,
    OrderRow,
    UpdateOrderPayload,
} from "@/types/orders.types";

export async function getOrders(): Promise<ServiceResult<OrderRow[]>> {
    try {
        const response = await apiClient.get<ItemsResponse<OrderRow>>("/sales/orders");
        return ok(response.data.items ?? []);
    } catch (error) {
        return toFailure(error);
    }
}

export async function getNewOrder(): Promise<ServiceResult<NewOrderResponse>> {
    try {
        const response = await apiClient.get<NewOrderResponse>("/sales/orders/new");
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}

export async function getOrder(id: string): Promise<ServiceResult<EditOrderResponse>> {
    try {
        const response = await apiClient.get<EditOrderResponse>(`/sales/orders/${id}`);
        return ok(response.data);
    } catch (error) {
        return toFailure(error);
    }
}

export async function createOrder(order: OrderFormValues): Promise<ServiceResult<void>> {
    try {
        await apiClient.post("/sales/orders", order);
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}

export async function updateOrder(order: UpdateOrderPayload): Promise<ServiceResult<void>> {
    try {
        await apiClient.put("/sales/orders", order);
        return ok(undefined);
    } catch (error) {
        return toFailure(error);
    }
}
