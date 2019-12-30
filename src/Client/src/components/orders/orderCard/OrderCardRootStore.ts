import { observable, toJS, action } from "mobx";
import { Guid } from "guid-typescript";
import NewOrderCardStore from "./NewOrderCardStore";
import OrdersGridStore from "../ordersGrid/OrdersGridStore";

export default class OrderCardRootStore{

    @observable
    orderCardStore: NewOrderCardStore;

    @observable
    editedOrderId: string;

    @observable
    isCreateOrEdit: boolean;

    constructor(){
        this.orderCardStore = new NewOrderCardStore(this);        
        this.editedOrderId = Guid.EMPTY;
        this.isCreateOrEdit = true;
    }

    @action
    setEdtitedOrderId(id: string){
        this.editedOrderId = id;
    }
}