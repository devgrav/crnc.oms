import { JobType } from "../JobType";

import TextValueDto from "../../shared/TextValueDto";

export default interface BaseOrderModel{
    jobType: JobType;
    jobDescription: string;    
    jobId: string;
    jobNumber: string;
    dateSentToCustomer: string;
    customerTitle: string;
    customerAbbreviation: string;
    customerContactPersonFirstName: string;
    isDisabledForEdit: boolean;
    customerContactPersonLastName: string;
    customerContactPersonEmail: string;
    customerContactPersonPhone: string;        
    jobTypes: TextValueDto[];
    [key: string]: any;
} 