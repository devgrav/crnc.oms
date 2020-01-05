import BaseOrderModel from "./BaseOrderModel";
import { observable } from "mobx";
import { JobType } from "../JobType";
import TextValueDto from "../../shared/TextValueDto";
import { MaterialSource } from "../MaterialSource";
import { OrderStatus } from "../OrderStatus";
import { SignoffType } from "../SignoffType";


export default class EditOrderModel
    implements BaseOrderModel {

    @observable jobType: JobType;
    @observable jobDescription: string;    
    @observable materialSource: MaterialSource;
    @observable status: OrderStatus;
    @observable signoffType: SignoffType;
    @observable customerTitle: string;
    @observable customerAbbreviation: string;
    @observable customerContactPersonFirstName: string;
    @observable customerContactPersonMiddleName: string;
    @observable customerContactPersonLastName: string;
    @observable customerContactPersonEmail: string;
    @observable customerContactPersonPhone: string;        
    @observable jobTypes: TextValueDto[];
    @observable statuses: TextValueDto[];
    @observable signoffTypes: TextValueDto[];
    @observable materialSources: TextValueDto[];
    [key: string]: any;

    constructor(){
        this.jobType = 0;
        this.jobDescription = "";
        this.materialSource = 0;
        this.status = 0;
        this.signoffType = 0;
        this.customerContactPersonFirstName = "";
        this.customerContactPersonMiddleName = "";
        this.customerContactPersonLastName = "";
        this.customerTitle = "";
        this.customerAbbreviation = "";
        this.customerContactPersonEmail = "";
        this.customerContactPersonPhone = "";
        this.jobTypes = [];
        this.statuses = [];
        this.signoffTypes = [];
        this.materialSources = [];
    }

}