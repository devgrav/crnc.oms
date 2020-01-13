import { JobType } from "../JobType";

import TextValueDto from "../../shared/TextValueDto";

export default interface BaseOrderModel{
    jobType: JobType;
    jobDescription: string;    
    customerTitle: string;
    customerAbbreviation: string;
    customerContactPersonFirstName: string;

    customerContactPersonLastName: string;
    customerContactPersonEmail: string;
    customerContactPersonPhone: string;        
    jobTypes: TextValueDto[];
    [key: string]: any;
} 