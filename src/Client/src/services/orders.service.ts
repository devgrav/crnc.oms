import apiClient from "./apiClient";
import { request, requestItems, requestVoid } from "./request";
import type { ServiceResult } from "./result";
import type { ItemsResponse } from "@/types/api.types";
import type {
    EditOrderResponse,
    NewOrderResponse,
    OrderFormValues,
    OrderRow,
    UpdateOrderPayload,
} from "@/types/orders.types";

export function getOrders(): Promise<ServiceResult<OrderRow[]>> {
    return requestItems(() => apiClient.get<ItemsResponse<OrderRow>>("/sales/orders"));
}

export function getNewOrder(): Promise<ServiceResult<NewOrderResponse>> {
    return request(() => apiClient.get<NewOrderResponse>("/sales/orders/new"));
}

export function getOrder(id: string): Promise<ServiceResult<EditOrderResponse>> {
    return request(() => apiClient.get<EditOrderResponse>(`/sales/orders/${id}`));
}

export function createOrder(order: OrderFormValues): Promise<ServiceResult<void>> {
    return requestVoid(() => apiClient.post("/sales/orders", order));
}

export function updateOrder(order: UpdateOrderPayload): Promise<ServiceResult<void>> {
    return requestVoid(() => apiClient.put("/sales/orders", order));
}
