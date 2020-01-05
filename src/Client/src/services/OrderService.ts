import axios from "axios";
import APP_CONFIG from "../config";
import AxiosProxy from "./AxiosProxy";
import { StrictFormGroupProps } from "semantic-ui-react";
import OrderGridRowModel from "../components/orders/ordersGrid/OrderGridModelRow";
import OrdersGridModel from "../components/orders/ordersGrid/OrderGridModel";
import NewOrderModel from "../components/orders/orderCard/NewOrderModel";
import EditOrderModel from "../components/orders/orderCard/EditOrderModel";
import { Guid } from "guid-typescript";

export class OrderService{

    public static getOrders(): Promise<OrderGridRowModel[]>{
        return AxiosProxy.instance.get(APP_CONFIG.ordersUrl)
        .then((response) => {
            return response.data.items;
        });
    }

    public static getNewOrder(): Promise<NewOrderModel>{
        return AxiosProxy.instance.get(`${APP_CONFIG.ordersUrl}/new`)
        .then((response) => {
            return response.data;
        });
    }

    public static getOrder(id: Guid): Promise<EditOrderModel>{
        return AxiosProxy.instance.get(`${APP_CONFIG.ordersUrl}/${id}`)
        .then((response) => {
            return response.data;
        });
    }

    public static createOrder(order: NewOrderModel): Promise<void>{
        return AxiosProxy.instance.post(APP_CONFIG.ordersUrl, order)
            .then((response) => {
                return;
            });
    }

    public static editOrder(order: EditOrderModel): Promise<void>{
        return AxiosProxy.instance.put(APP_CONFIG.ordersUrl, order)
            .then((response) => {
                return;
            });
    }
}