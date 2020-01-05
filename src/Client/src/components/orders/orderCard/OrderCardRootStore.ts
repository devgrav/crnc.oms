import { observable, toJS, action, computed } from "mobx";
import { Guid } from "guid-typescript";
import NewOrderCardStore from "./NewOrderCardStore";
import OrdersGridStore from "../ordersGrid/OrdersGridStore";
import TextValueDto from "../../shared/TextValueDto";
import ValidationInfo from "../../shared/ValidationInfo";
import BaseOrderModel from "./BaseOrderModel";
import NewOrderModel from "./NewOrderModel";
import EditOrderCardStore from "./EditOrderCardStore";
import EditOrderModel from "./EditOrderModel";

export default class OrderCardRootStore{

    @observable
    newOrderCardStore: NewOrderCardStore;

    @observable
    editOrderCardStore: EditOrderCardStore;

    @observable
    model: BaseOrderModel;

    @observable
    editedOrderId: Guid;

    @observable
    isCreateOrEdit: boolean;

    @observable
    isLoading: boolean;

    @observable
    validationInfo: ValidationInfo;

    constructor(editedOrderId: Guid){
        this.newOrderCardStore = new NewOrderCardStore(this);
        this.editOrderCardStore = new EditOrderCardStore(this);      
        this.editedOrderId = editedOrderId;
        this.model = this.editedOrderId.isEmpty() ? new NewOrderModel() : new EditOrderModel();  
        this.isCreateOrEdit = true;
        this.isLoading = false;
        this.validationInfo = new ValidationInfo();
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
    public setModelValue(data: any){
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;  

        this.model[name] = value;

        this.setValidationInfoForModelValue(name);
    }

    
    @action
    public cancelCreateOrEdit(){
        this.isCreateOrEdit = false;
    }

    @action
    public setValidationInfoForModelValue(name: string){
        this.validationInfo.tryRemoveFieldValidationInfo(name);
    }

    @action
    public setValidationInfo(validationInfo: any){
        if(validationInfo && validationInfo.errors){
            this.validationInfo.info = validationInfo.errors;
        }        
    }

    @computed
    get jobTypes(): TextValueDto[]{
        let jobTypes: TextValueDto[] = [
            {
                value: 0,
                text: "Not chosen"
            }
        ];        

        jobTypes = jobTypes.concat(this.model.jobTypes);

        return jobTypes;
    }
}