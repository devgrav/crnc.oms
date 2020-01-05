import * as React from "react";
import { observer, inject } from "mobx-react";
import { Modal, Segment, Message, Form, Grid, Button, FormProps, ModalProps, ButtonProps, Select, Header } from "semantic-ui-react";
import {nameof} from "ts-simple-nameof";
import OrderCardRootStore from "./OrderCardRootStore";
import { Redirect } from "react-router-dom";
import BaseOrderModel from "./BaseOrderModel";
import ContactPersonInfo from "./ContactPersonInfo";
import ValidationInfo from "../../shared/ValidationInfo";
import CustomerInfo from "./CustomerInfo";
import OrderInfo from "./OrderInfo";

@observer
export default class OrderCard<TModel extends BaseOrderModel> extends React.Component<OrderCardProps>{

    private readonly store: OrderCardRootStore;

    constructor(props: OrderCardProps){
        super(props);

        this.store = this.props.orderCardRootStore;

        this.onChange = this.onChange.bind(this);
        this.onCancel = this.onCancel.bind(this);
        this.onClose = this.onClose.bind(this);
    }

    private onClose(event: React.MouseEvent<HTMLElement>, data: ModalProps): void{
        const {store} = this;

        store.cancelCreateOrEdit();
    }

    private onCancel(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps): void{
        const {store} = this;

        store.cancelCreateOrEdit();
    }

    private onChange(event: any, data: any): void{
        const {store} = this;      

        store.setModelValue(data);        
    }

    public render(){              
        const {model, isLoading, jobTypes, validationInfo, isCreateOrEdit} = this.store;
        const {materialSources, orderStatuses, signoffTypes} = this.store.editOrderCardStore;
        const {onChange} = this;
        const {onSave, headerText, isEdit} = this.props;

        if(!isCreateOrEdit)
            return <Redirect to="/orders"/>

        return (
            <Modal open={true} closeIcon onClose={this.onClose}>
                <Modal.Header>{headerText}</Modal.Header>
                <Modal.Content as={Segment} basic clearing loading={isLoading}>
                    {validationInfo && validationInfo.hasAnyValidationInfo && <Message
                        error
                        header="There was some errors with your submission"
                        list={validationInfo.validationMessages}
                    />}
                    <Form id="orderForm" className="ui form" onSubmit={onSave}>                                  
                        <Header as="h3" content="Order" dividing/>                                    
                        <OrderInfo
                            model ={model} 
                            validationInfo = {validationInfo} 
                            jobTypes = {jobTypes}
                            orderStatuses = {orderStatuses}
                            materialSources = {materialSources}
                            signoffTypes = {signoffTypes}
                            onChange = {onChange}
                            isEdit = {isEdit} 
                        />
                        <Header as="h3" content="Customer" dividing/>                                                                                                           
                        <CustomerInfo
                            model ={model} 
                            validationInfo = {validationInfo} 
                            onChange = {onChange}
                        />
                        <Header as="h3" content="Contact Person" dividing/>          
                        <ContactPersonInfo 
                            model ={model} 
                            validationInfo = {validationInfo} 
                            onChange = {onChange}
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


interface OrderCardProps{
    orderCardRootStore: OrderCardRootStore;
    onSave(event: React.FormEvent<HTMLFormElement>, data: FormProps): void;
    headerText: string;
    isEdit?: boolean;
}