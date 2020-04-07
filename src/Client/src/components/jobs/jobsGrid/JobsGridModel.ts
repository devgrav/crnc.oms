import {observable} from "mobx";
import JobsGridRowModel from "./JobsGridModelRow";

export default class JobsGridModel{
    @observable
    items: JobsGridRowModel[]

    constructor(){
        this.items = [];
    }
}