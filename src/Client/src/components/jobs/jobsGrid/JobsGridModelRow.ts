import { Guid } from "guid-typescript";
import { Priority } from "../priority";

export default class JobsGridRowModel{
    id: string;
    number: string;
    dateCreated: string;
    manager: string;
    priority: string;
    priorityEnum: Priority;
    jobType: string;
    jobDescription: string;
    materialSource: string;   
    isJobCompeted: boolean;

    constructor(){
        this.id = Guid.EMPTY;
        this.number = Guid.EMPTY;
        this.dateCreated = "";
        this.jobType = "";
        this.jobDescription = "";
        this.materialSource = "";
        this.manager = "";
        this.priority = "";
        this.priorityEnum = Priority.Low;
        this.isJobCompeted = false;
    }
}