import CurrentUser from "./CurrentUser";
import React from "react";

export default class CurrentUserContext {

    private static instance: CurrentUser | null;

    static get user(): CurrentUser{
        if(CurrentUserContext.instance){
            return CurrentUserContext.instance;
        }        

        throw Error("Current user is not defined");
    }

    static get isAuthentificated(): boolean{
        if(CurrentUserContext.instance)
            return true;
        
        return false;
    }

    static init(user: CurrentUser){
        (window as any).currentUserContext = CurrentUserContext;
        CurrentUserContext.instance = user;
    }

    static clear(){
        CurrentUserContext.instance = null;
    }
}
