import type { TextValue } from "./api.types";

export interface OrderRow {
    id: string;
    number: string;
    createdDate: string;
    customer: string;
    jobType: string;
    jobDescription: string;
    dateSentToCustomer: string;
    customerSignOffType: string;
    status: string;
    statusEnum: OrderStatus;
}

// Имена полей совпадают с ключами fieldErrors, которые отдаёт Sales.
export interface OrderFormValues {
    jobType: number;
    jobDescription: string;
    customerTitle: string;
    customerAbbreviation: string;
    customerContactPersonFirstName: string;
    customerContactPersonLastName: string;
    customerContactPersonEmail: string;
    customerContactPersonPhone: string;
    status?: number | null;
    materialSource?: number | null;
    signoffType?: number | null;
}

export interface NewOrderResponse extends OrderFormValues {
    jobTypes: TextValue[];
}

export interface EditOrderResponse extends OrderFormValues {
    id: string;
    jobTypes: TextValue[];
    statuses: TextValue[];
    signoffTypes: TextValue[];
    materialSources: TextValue[];
    dateSentToCustomer: string;
    jobId: string | null;
    jobNumber: string | null;
}

export const OrderStatus = {
    NotSent: 1,
    NeedSignoff: 2,
    Signed: 3,
    ConvertedToJob: 4,
    Closed: 5,
} as const;

export type OrderStatus = (typeof OrderStatus)[keyof typeof OrderStatus];

export function isOrderReadOnly(status: number | undefined): boolean {
    return status === OrderStatus.ConvertedToJob || status === OrderStatus.Closed;
}

export interface UpdateOrderPayload extends OrderFormValues {
    id: string;
}
