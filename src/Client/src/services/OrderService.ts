import axios from "axios";
import APP_CONFIG from "../config";
import AxiosProxy from "./AxiosProxy";
import { StrictFormGroupProps } from "semantic-ui-react";
import OrderGridRowModel from "../components/orders/ordersGrid/OrderGridModelRow";
import OrdersGridModel from "../components/orders/ordersGrid/OrderGridModel";
import NewOrderModel from "../components/orders/orderCard/NewOrderModel";

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

    public static saveNewOrder(order :NewOrderModel): Promise<void>{
        return AxiosProxy.instance.post(APP_CONFIG.ordersUrl, order)
            .then((response) => {
                return;
            });
    }
}