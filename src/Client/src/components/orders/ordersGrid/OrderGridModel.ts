import OrderGridRowModel from "./OrderGridModelRow";
import {observable} from "mobx";

export default class OrdersGridModel{
    @observable
    items: OrderGridRowModel[]

    constructor(){
        this.items = [];
    }
}