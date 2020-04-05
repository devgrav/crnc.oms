import { Guid } from "guid-typescript";
import { OrderStatus } from "../OrderStatus";

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
    statusEnum?: OrderStatus;

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