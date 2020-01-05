import OrdersGridRootStore from "../ordersGrid/OrdersGridRootStore";
import { observable, action, computed } from "mobx";
import NewOrderModel from "./NewOrderModel";
import { OrderService } from "../../../services/OrderService";
import { Guid } from "guid-typescript";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";
import OrderCardRootStore from "./OrderCardRootStore";
import ValidationInfo from "../../shared/ValidationInfo";

export default class NewOrderCardStore{

    @observable
    orderCardRootStore: OrderCardRootStore;

    constructor(rootStore: OrderCardRootStore){
        this.orderCardRootStore = rootStore;
        this.orderCardRootStore.model = new NewOrderModel();
    }
    
    @action
    public async getNewOrder(): Promise<NewOrderModel>{
        try{
            this.orderCardRootStore.showLoader();
            const newOrder = await OrderService.getNewOrder();            
            this.orderCardRootStore.hideLoader();
            if(newOrder)
                return newOrder;
            return new NewOrderModel();
        }catch (error) {
            this.orderCardRootStore.hideLoader();
            return Promise.reject(error);
        }  
    }

    @action
    public async createOrder(): Promise<void>{
        try{
            this.orderCardRootStore.showLoader();
            await OrderService.createOrder(this.orderCardRootStore.model);            
            this.orderCardRootStore.hideLoader();                  
            this.orderCardRootStore.cancelCreateOrEdit();
        }catch (error){
            this.orderCardRootStore.hideLoader();
             if (error.response){
                if (error.response.status === 400){
                    this.orderCardRootStore.setValidationInfo(error.response.data);
                }
            }
        }
    }
    

    @action
    public setModel(model: NewOrderModel){
        this.orderCardRootStore.model = model;
    }
}