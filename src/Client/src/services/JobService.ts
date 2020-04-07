import axios from "axios";
import APP_CONFIG from "../config";
import AxiosProxy from "./AxiosProxy";
import JobsGridRowModel from "../components/jobs/jobsGrid/JobsGridModelRow";

export class JobService{

    public static getJobs(): Promise<JobsGridRowModel[]>{
        return AxiosProxy.instance.get(APP_CONFIG.jobsUrl)
        .then((response) => {
            return response.data.items;
        });
    }
}