import { observable } from "mobx";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";

export default class NewOrderModel{

    @observable jobType: JobType;
    @observable jobDescription: string;    
    @observable customerTitle: string;
    @observable customerAbbreviation: string;
    @observable customerContactPersonFirstName: string;
    @observable customerContactPersonMiddleName: string;
    @observable customerContactPersonLastName: string;
    @observable customerContactPersonEmail: string;
    @observable customerContactPersonPhone: string;        
    @observable jobTypes: TextValueDto[];
    [key: string]: any;

    constructor(){
        this.jobType = 0;
        this.jobDescription = "";
        this.customerContactPersonFirstName = "";
        this.customerContactPersonMiddleName = "";
        this.customerContactPersonLastName = "";
        this.customerTitle = "";
        this.customerAbbreviation = "";
        this.customerContactPersonEmail = "";
        this.customerContactPersonPhone = "";
        this.jobTypes = [];
    }
}