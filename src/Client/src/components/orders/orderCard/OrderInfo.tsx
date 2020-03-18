import * as React from "react";
import { Header, Form } from "semantic-ui-react";
import { nameof } from "ts-simple-nameof";
import BaseOrderModel from "./BaseOrderModel";
import ValidationInfo from "../../shared/ValidationInfo";
import { observer } from "mobx-react";
import TextValueDto from "../../shared/TextValueDto";
import EditOrderModel from "./EditOrderModel";

@observer
export default class OrderInfo extends React.Component<OrderInfoProps>{

    constructor(props: OrderInfoProps){
        super(props);
    }

    render(){
        const {model, validationInfo, onChange, jobTypes, materialSources, orderStatuses, signoffTypes, isEdit} = this.props;        

        return (
            <React.Fragment> 
                <Form.Select
                    name={nameof<BaseOrderModel>(x => x.jobType)}     
                    className="required"
                    onChange={onChange}
                    label = "Job type"
                    value = {model.jobType || jobTypes[0].value}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.jobType))}
                    options = {jobTypes}
                    disabled = {model.isDisabledForEdit}
                />
                <Form.TextArea
                    name={nameof<BaseOrderModel>(x => x.jobDescription)}     
                    className="required"
                    onChange={onChange}                                        
                    label="Job Description"                                        
                    value={model.jobDescription || ""}
                    error={validationInfo.hasFieldValidationError(nameof<BaseOrderModel>(x => x.jobDescription))}
                    autoComplete="off"       
                    disabled = {model.isDisabledForEdit}                     
                />    
                {isEdit && <React.Fragment>
                    <Form.Select
                        name={nameof<EditOrderModel>(x => x.status)}
                        onChange={onChange}
                        label = "Status"
                        value = {model.status || orderStatuses[0].value}
                        error={validationInfo.hasFieldValidationError(nameof<EditOrderModel>(x => x.status))}
                        options = {orderStatuses}
                        disabled = {model.isDisabledForEdit}
                    />
                    <Form.Select
                        name={nameof<EditOrderModel>(x => x.materialSource)}     
                        className="required"
                        onChange={onChange}
                        label = "Material source"
                        value = {model.materialSource || materialSources[0].value}
                        error={validationInfo.hasFieldValidationError(nameof<EditOrderModel>(x => x.materialSource))}
                        options = {materialSources}
                        disabled = {model.isDisabledForEdit}
                    />
                    <Form.Select
                        name={nameof<EditOrderModel>(x => x.signoffType)}     
                        className="required"
                        onChange={onChange}
                        label = "Sign off type"
                        value = {model.signoffType || signoffTypes[0].value}
                        error={validationInfo.hasFieldValidationError(nameof<EditOrderModel>(x => x.signoffType))}
                        options = {signoffTypes}
                        disabled = {model.isDisabledForEdit}
                    />
                </React.Fragment>}       
            </React.Fragment> 
        )
    }

}

interface OrderInfoProps{
    model: BaseOrderModel;
    validationInfo: ValidationInfo;
    jobTypes: TextValueDto[]
    orderStatuses: TextValueDto[]
    signoffTypes: TextValueDto[]
    materialSources: TextValueDto[]
    onChange(event: any, data: any): void;
    isEdit?: boolean;
}