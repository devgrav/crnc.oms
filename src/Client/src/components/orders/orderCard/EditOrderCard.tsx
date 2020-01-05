import * as React from "react";
import { observer, inject } from "mobx-react";
import NewOrderCardStore from "./NewOrderCardStore";
import { FormProps } from "semantic-ui-react";
import NewOrderModel from "./NewOrderModel";
import OrderCardRootStore from "./OrderCardRootStore";
import OrderCard from "./OrderCard";
import { nameof } from "ts-simple-nameof";
import OrdersRootStores from "../OrdersRootStores";
import EditOrderCardStore from "./EditOrderCardStore";
import EditOrderModel from "./EditOrderModel";

@inject(nameof<OrdersRootStores>(x => x.orderCardRootStore))
@observer
export default class EditOrderCard extends React.Component<EditOrderCardProps>{

    private readonly store: EditOrderCardStore;

    constructor(props: EditOrderCardProps){
        super(props);

        if(!props.orderCardRootStore)
            throw Error("Not found store");

        this.store = props.orderCardRootStore.editOrderCardStore;

        this.onSave = this.onSave.bind(this);
        this.onChange = this.onChange.bind(this);
    }

    async componentDidMount(){
        const {store} = this;
        let order = await store.getOrder(); 
        store.setModel(order);
    }

    onSave(event: React.FormEvent<HTMLFormElement>, data: FormProps){
        const {store} = this;

        store.editOrder();           
    }

    private onChange(event: any, data: any): void{
        const {orderCardRootStore} = this.store;      

        orderCardRootStore.setModelValue(data);        
    }


    public render(){              
        const {onSave, store} = this;

        return (
            <OrderCard
                orderCardRootStore = {store.orderCardRootStore} 
                onSave = {onSave}
                headerText = {`Edit order ${store.orderCardRootStore.editedOrderId}`}
                isEdit = {true}
              />            
        );
    }
}


interface EditOrderCardProps{
    orderCardRootStore?: OrderCardRootStore;
}
