import { Guid } from "guid-typescript";

export default class OrderGridRowModel{
    id: string;
    number: string;
    createdDate: string;
    customer: string;
    jobType: string;
    jobDescription: string;
    dateSentToCustomer: string;    
    customerSignOffType: string;
    status: string;

    constructor(){
        this.id = Guid.EMPTY;
        this.number = Guid.EMPTY;
        this.createdDate = "";
        this.jobType = "";
        this.jobDescription = "";
        this.dateSentToCustomer = "";
        this.customer = "";
        this.customerSignOffType = "";
        this.status = "";
    }
}