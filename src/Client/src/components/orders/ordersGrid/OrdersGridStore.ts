import { observable, action } from "mobx";
import OrdersGridModel from "./OrderGridModel";
import OrdersGridRootStore from "../OrdersRootStore";
import OrderGridRowModel from "./OrderGridModelRow";
import { OrderService } from "../../../services/OrderService";

export default class OrdersGridStore{

    @observable
    model: OrdersGridModel

    @observable
    isLoading: boolean;

    @observable
    rootStore: OrdersGridRootStore;

    constructor(rootStore: OrdersGridRootStore){
        this.model = new OrdersGridModel();
        this.rootStore = rootStore;
        this.isLoading = false;
    }


    @action
    public showLoading(): void{
        this.isLoading = true;
    }

    @action
    public hideLoading(): void{
        this.isLoading = false;
    }

    @action
    public async getOrders(): Promise<OrderGridRowModel[]>{
        try{
            this.showLoading();
            const orders = await OrderService.getOrders();            
            this.hideLoading();
            if(orders)
                return orders;
            return [];
        }catch (error) {
            this.hideLoading();
            return Promise.reject(error);
        }  
    }

    @action
    public setModel(orders: OrderGridRowModel[]){
        this.model.items = orders;
    }
}