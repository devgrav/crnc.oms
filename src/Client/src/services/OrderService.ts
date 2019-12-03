import axios from "axios";
import APP_CONFIG from "../config";
import AxiosProxy from "./AxiosProxy";
import { StrictFormGroupProps } from "semantic-ui-react";

export class OrderService{

    public static getOrders(): Promise<OrdersForGridDto>{
        return AxiosProxy.instance.get(APP_CONFIG.ordersUrl)
        .then((response) => {
            return response.data;
        });
    }
}

export interface OrdersForGridDto{
    items: OrdersForGridItemDto[];
}

export interface OrdersForGridItemDto{
    id: number;
    number: string;
    createdDate: string;
    jobType: string;
    jobDescription: string;
    dateSentToCustomer: string;
    customer: string;
    customerSignOffType: string;
    status: string;
}

export enum JobType{
    New = 1,
    Repair = 2,
    Service = 3,
    Other = 4
}

export enum SignoffType{
    Email = 1,
    Verbal = 2
}
