import { observable, action } from "mobx";
import JobsGridRootStore from "./JobsGridRootStore";
import JobsGridModel from "./JobsGridModel";
import JobsGridRowModel from "./JobsGridModelRow";
import { JobService } from "../../../services/JobService";

export default class JobsGridStore{

    @observable
    model: JobsGridModel

    @observable
    isLoading: boolean;

    @observable
    rootStore: JobsGridRootStore;

    constructor(rootStore: JobsGridRootStore){
        this.model = new JobsGridModel();
        this.rootStore = rootStore;
        this.isLoading = false;
    }


    @action
    public showLoading(): void{
        this.isLoading = true;
    }

    @action
    public hideLoading(): void{
        this.isLoading = false;
    }

    @action
    public async getJobs(): Promise<JobsGridRowModel[]>{
        try{
            this.showLoading();
            const jobs = await JobService.getJobs();            
            this.hideLoading();
            if(jobs)
                return jobs;
            return [];
        }catch (error) {
            this.hideLoading();
            return Promise.reject(error);
        }  
    }

    @action
    public setModel(orders: JobsGridRowModel[]){
        this.model.items = orders;
    }
}