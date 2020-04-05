import * as React from "react";
import { Header, Form } from "semantic-ui-react";
import { nameof } from "ts-simple-nameof";
import BaseOrderModel from "./BaseOrderModel";
import ValidationInfo from "../../shared/ValidationInfo";
import { observer } from "mobx-react";

@observer
export default class ContactPersonInfo extends React.Component<ContactPersonInfoProps>{

    constructor(props: ContactPersonInfoProps){
        super(props);
    }

    render(){
        const {model, validationInfo, onChange} = this.props;        

        return (
            <React.Fragment> 
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerContactPersonFirstName)}  
                    placeholder="John"   
                    className="required"
                    onChange={onChange}                                            
                    label="First name"
                    value={model.customerContactPersonFirstName || ""}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerContactPersonFirstName))}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerContactPersonLastName)}    
                    placeholder="Smith"                        
                    className="required"
                    onChange={onChange}                                                                        
                    label="Last name"
                    value={model.customerContactPersonLastName || ""}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerContactPersonLastName))}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerContactPersonEmail)}
                    placeholder="john_smith@crnc.com"
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerContactPersonEmail))}
                    className="required"
                    onChange={onChange}                                            
                    label="Email"
                    value={model.customerContactPersonEmail || ""}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerContactPersonPhone)}
                    placeholder="89161234567"
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerContactPersonPhone))}
                    className="required"
                    onChange={onChange}                                            
                    label="Phone"
                    value={model.customerContactPersonPhone || ""}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />        
            </React.Fragment> 
        )
    }

}

interface ContactPersonInfoProps{
    model: BaseOrderModel;
    validationInfo: ValidationInfo;
    onChange(event: any, data: any): void;
}