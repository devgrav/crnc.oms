import { observable } from "mobx";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";
import BaseOrderModel from "./BaseOrderModel";

export default class NewOrderModel
    implements BaseOrderModel {
        
    @observable jobType: JobType;
    @observable jobDescription: string;    
    @observable customerTitle: string;
    @observable customerAbbreviation: string;
    @observable customerContactPersonFirstName: string;
    @observable customerContactPersonLastName: string;
    @observable customerContactPersonEmail: string;
    @observable customerContactPersonPhone: string;        
    @observable jobTypes: TextValueDto[];
    [key: string]: any;

    constructor(){
        this.jobType = 0;
        this.jobDescription = "";
        this.customerContactPersonFirstName = "";
        this.customerContactPersonLastName = "";
        this.customerTitle = "";
        this.customerAbbreviation = "";
        this.customerContactPersonEmail = "";
        this.customerContactPersonPhone = "";
        this.jobTypes = [];
    }
}