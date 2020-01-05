import { observable, toJS, action } from "mobx";
import NewOrderCardStore from "../orderCard/NewOrderCardStore";
import OrdersGridStore from "./OrdersGridStore";
import { Guid } from "guid-typescript";

export default class OrdersGridRootStore{

    @observable
    ordersGridStore: OrdersGridStore;

    constructor(){    
        this.ordersGridStore = new OrdersGridStore(this);
    }
}