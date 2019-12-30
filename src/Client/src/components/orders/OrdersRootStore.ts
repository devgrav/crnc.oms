import { observable, toJS, action } from "mobx";
import NewOrderCardStore from "./orderCard/NewOrderCardStore";
import OrdersGridStore from "./ordersGrid/OrdersGridStore";
import { Guid } from "guid-typescript";

export default class OrdersGridRootStore{

    @observable
    ordersGridStore: OrdersGridStore;

    @observable
    editedOrderId: string;

    constructor(){    
        this.ordersGridStore = new OrdersGridStore(this);
        this.editedOrderId = Guid.EMPTY;
    }

    @action
    setEdtitedOrderId(id: string){
        this.editedOrderId = id;
    }
}