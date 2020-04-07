import { observable, toJS, action } from "mobx";
import OrdersGridStore from "./JobsGridStore";
import { Guid } from "guid-typescript";
import JobsGridStore from "./JobsGridStore";

export default class JobsGridRootStore{

    @observable
    jobsGridStore: JobsGridStore;

    constructor(){    
        this.jobsGridStore = new JobsGridStore(this);
    }
}