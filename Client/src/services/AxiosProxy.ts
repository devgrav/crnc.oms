import axios, { AxiosInstance } from "axios";
import APP_CONFIG from "../config";
import CurrentUserContext from "../auth/CurrentUserContext";

export default class AxiosProxy{

    private static _instance: AxiosInstance

    static get instance(){
        if(!AxiosProxy._instance){
            const instance = axios.create()
            instance.defaults.headers.common['Authorization'] = `Bearer ${CurrentUserContext.user.jwt}`;
            AxiosProxy._instance = instance;
        }            

        return AxiosProxy._instance;        
    }

}