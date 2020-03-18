import OrdersGridRootStore from "../ordersGrid/OrdersGridRootStore";
import { observable, action, computed } from "mobx";
import NewOrderModel from "./NewOrderModel";
import { OrderService } from "../../../services/OrderService";
import { Guid } from "guid-typescript";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";
import OrderCardRootStore from "./OrderCardRootStore";
import ValidationInfo from "../../shared/ValidationInfo";
import EditOrderModel from "./EditOrderModel";
import { OrderStatus } from "../OrderStatus";

export default class EditOrderCardStore{

    @observable
    orderCardRootStore: OrderCardRootStore;

    constructor(rootStore: OrderCardRootStore){
        this.orderCardRootStore = rootStore;
        this.orderCardRootStore.model = new EditOrderModel();
    }
    
    @action
    public async getOrder(): Promise<EditOrderModel>{
        try{
            this.orderCardRootStore.showLoader();
            const order = await OrderService.getOrder(this.orderCardRootStore.editedOrderId);            
            this.orderCardRootStore.hideLoader();
            if(order)
                return order;
            return new EditOrderModel();
        }catch (error) {
            this.orderCardRootStore.hideLoader();
            return Promise.reject(error);
        }  
    }

    @action
    public async editOrder(): Promise<void>{
        try{
            this.orderCardRootStore.showLoader();
            await OrderService.editOrder(this.orderCardRootStore.model as EditOrderModel);            
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
    public setModel(model: EditOrderModel){
        if(model.status === OrderStatus.Closed || model.status === OrderStatus.ConvertedToJob)
        {
            model.isDisabledForEdit = true;
        }

        this.orderCardRootStore.model = model;
    }

    @computed
    get orderStatuses(): TextValueDto[]{
        let statuses: TextValueDto[] = [
            {
                value: 0,
                text: "Not chosen"
            }
        ];        

        statuses = statuses.concat((this.orderCardRootStore.model as EditOrderModel).statuses);

        return statuses;
    }

    @computed
    get materialSources(): TextValueDto[]{
        let materialSources: TextValueDto[] = [
            {
                value: 0,
                text: "Not chosen"
            }
        ];        

        materialSources = materialSources.concat((this.orderCardRootStore.model as EditOrderModel).materialSources);

        return materialSources;
    }

    @computed
    get signoffTypes(): TextValueDto[]{
        let signoffTypes: TextValueDto[] = [
            {
                value: 0,
                text: "Not chosen"
            }
        ];        

        signoffTypes = signoffTypes.concat((this.orderCardRootStore.model as EditOrderModel).signoffTypes);

        return signoffTypes;
    }
}