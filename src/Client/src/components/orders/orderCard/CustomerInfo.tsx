import * as React from "react";
import { Header, Form } from "semantic-ui-react";
import { nameof } from "ts-simple-nameof";
import BaseOrderModel from "./BaseOrderModel";
import ValidationInfo from "../../shared/ValidationInfo";
import { observer } from "mobx-react";

@observer
export default class CustomerInfo extends React.Component<CustomerInfoProps>{

    constructor(props: CustomerInfoProps){
        super(props);
    }

    render(){
        const {model, validationInfo, onChange} = this.props;        

        return (
            <React.Fragment> 
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerTitle)}
                    className="required"
                    onChange={onChange}                                            
                    label="Title"      
                    placeholder="Awesome Company"                                      
                    value={model.customerTitle || ""}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerTitle))}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />
                <Form.Input
                    name={nameof<BaseOrderModel>(x => x.customerAbbreviation)}
                    className="required"
                    onChange={onChange}                                            
                    label="Abbreviation"  
                    placeholder="AC"                                           
                    value={model.customerAbbreviation || ""}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.customerAbbreviation))}
                    autoComplete="off"
                    disabled = {model.isDisabledForEdit}
                />       
            </React.Fragment> 
        )
    }

}

interface CustomerInfoProps{
    model: BaseOrderModel;
    validationInfo: ValidationInfo;
    onChange(event: any, data: any): void;
}