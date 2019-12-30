import OrdersGridRootStore from "../OrdersRootStore";
import { observable, action, computed } from "mobx";
import NewOrderModel from "./NewOrderModel";
import { OrderService } from "../../../services/OrderService";
import { Guid } from "guid-typescript";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";
import OrderCardRootStore from "./OrderCardRootStore";

export default class NewOrderCardStore{

    @observable
    model: NewOrderModel;

    @observable
    orderCardRootStore: OrderCardRootStore;

    @observable
    isLoading: boolean;

    constructor(rootStore: OrderCardRootStore){
        this.orderCardRootStore = rootStore;
        this.model = new NewOrderModel();
        this.isLoading = false;
    }

    @action
    public showLoader(): void{
        this.isLoading = true;
    }

    @action
    public hideLoader(): void{
        this.isLoading = false;
    }
    
    @action
    public async getNewOrder(): Promise<NewOrderModel>{
        try{
            this.showLoader();
            const newOrder = await OrderService.getNewOrder();            
            this.hideLoader();
            if(newOrder)
                return newOrder;
            return new NewOrderModel();
        }catch (error) {
            this.hideLoader();
            return Promise.reject(error);
        }  
    }

    @action
    public async saveNewOrder(): Promise<void>{
        try{
            this.showLoader();
            await OrderService.saveNewOrder(this.model);            
            this.hideLoader();                    
            this.cancelCreate();
        }catch (error){
             this.hideLoader();
             if (error.response){
                //TODO: Добавить обработку валидации
            }
        }
    }
    

    @action
    public setModel(model: NewOrderModel){
        this.model = model;
    }

    @action
    public setModelValue(name: string, value: any){
        this.model[name] = value;
    }

    
    @action
    public cancelCreate(){
        this.orderCardRootStore.isCreateOrEdit = false;
    }

    @computed
    get jobTypes(): TextValueDto[]{
        let jobTypes: TextValueDto[] = [
            {
                value: Guid.EMPTY,
                text: "Not chosen"
            }
        ];        

        return jobTypes.concat(this.model.jobTypes);
    }
}