import * as React from "react";
import { observer, inject } from "mobx-react";
import OrdersGridRootStore from "../OrdersRootStore";
import NewOrderCardStore from "./NewOrderCardStore";
import { Modal, Segment, Message, Form, Grid, Button, FormProps, ModalProps, ButtonProps, Select, Header } from "semantic-ui-react";
import { Guid } from "guid-typescript";
import RoleSelect from "../../users/RoleSelect";
import NewOrderModel from "./NewOrderModel";
import {nameof} from "ts-simple-nameof";
import OrderCardRootStore from "./OrderCardRootStore";
import { Redirect } from "react-router-dom";

@inject((rootStore: OrderCardRootStore) => ({
    orderCardStore: rootStore.orderCardStore
}))
@observer
export default class NewOrderCard extends React.Component<NewOrderCardProps>{

    private readonly store: NewOrderCardStore;

    constructor(props: NewOrderCardProps){
        super(props);

        this.store = this.props.orderCardStore || new NewOrderCardStore(new OrderCardRootStore());

        this.onChange = this.onChange.bind(this);
        this.onCancel = this.onCancel.bind(this);
        this.onClose = this.onClose.bind(this);
        this.onSave = this.onSave.bind(this);
    }

    async componentDidMount(){
        const {store} = this;
        let newOrder = await store.getNewOrder(); 
        store.setModel(newOrder);
    }

    onSave(vent: React.FormEvent<HTMLFormElement>, data: FormProps){
        const {store} = this;
        store.saveNewOrder();        
    }

    private onClose(event: React.MouseEvent<HTMLElement>, data: ModalProps): void{
        const {store} = this;

        store.cancelCreate();
    }

    private onCancel(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps): void{
        const {store} = this;

        store.cancelCreate();
    }

    private onChange(event: any, data: any): void{
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;
        
        this.store.setModelValue(name, value);
    }

    public render(){              
        const {model, isLoading, jobTypes} = this.store;
        const {isCreateOrEdit} = this.store.orderCardRootStore;

        if(!isCreateOrEdit)
            return <Redirect to="/orders"/>

        return (
            <Modal open={true} closeIcon onClose={this.onClose}>
                <Modal.Header>{"Add new order"}</Modal.Header>
                <Modal.Content as={Segment} basic clearing loading={isLoading}>
                    <Form id="orderForm" className="ui form" onSubmit={this.onSave}>                                  
                        <Header as="h3" content="Order" dividing/>                                    
                        <Form.Select
                            name={nameof<NewOrderModel>(x => x.jobType)}     
                            className="required"
                            onChange={this.onChange}
                            label = "Job type"
                            value = {model.jobType}
                            options = {jobTypes}
                        />
                        <Form.TextArea
                            name={nameof<NewOrderModel>(x => x.jobDescription)}     
                            className="required"
                            onChange={this.onChange}                                        
                            label="Job Description"                                        
                            value={model.jobDescription}
                            autoComplete="off"
                        />                             
                        <Header as="h3" content="Customer" dividing/>                                    
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerTitle)}
                            className="required"
                            onChange={this.onChange}                                            
                            label="Title"                                            
                            value={model.customerTitle}
                            autoComplete="off"
                        />
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerAbbreviation)}
                            className="required"
                            onChange={this.onChange}                                            
                            label="Abbreviation"                                            
                            value={model.customerAbbreviation}
                            autoComplete="off"
                        />                                                                            
                        <Header as="h3" content="Contact Person" dividing/>                                  
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerContactPersonFirstName)}     
                            className="required"
                            onChange={this.onChange}                                            
                            label="First name"
                            value={model.customerContactPersonFirstName}
                            autoComplete="off"
                        />
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerContactPersonMiddleName)}                                     
                            onChange={this.onChange}                                            
                            label="Middle name"
                            value={model.customerContactPersonMiddleName}
                            autoComplete="off"
                        />
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerContactPersonLastName)}     
                            className="required"
                            onChange={this.onChange}                                            
                            label="Last name"
                            value={model.customerContactPersonLastName}
                            autoComplete="off"
                        />
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerContactPersonEmail)}     
                            className="required"
                            onChange={this.onChange}                                            
                            label="Email"
                            type="email"
                            value={model.customerContactPersonEmail}
                            autoComplete="off"
                        />
                        <Form.Input
                            name={nameof<NewOrderModel>(x => x.customerContactPersonPhone)}
                            className="required"
                            onChange={this.onChange}                                            
                            label="Phone"
                            type="phone"
                            value={model.customerContactPersonPhone}
                            autoComplete="off"
                        />                                    
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button basic color="green" type="submit" content="Save" form="orderForm"/>
                    <Button basic color="red" type="reset" content="Cancel" onClick={this.onCancel}/>
                </Modal.Actions>
            </Modal>
        );
        }
}


interface NewOrderCardProps{
    orderCardStore?: NewOrderCardStore;
}
