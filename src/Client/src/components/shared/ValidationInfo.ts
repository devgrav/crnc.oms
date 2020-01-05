import { observable, computed, action } from "mobx";

export default class ValidationInfo{
    
    @observable
    private validationInfo?: any;
        
    public set info(validationInfo: any){
        this.validationInfo = validationInfo;
    }
    
    @computed
    public get info(): any{
        return this.validationInfo;
    }

    @action
    public tryRemoveFieldValidationInfo(name: string): void{        
        if (this.validationInfo && this.validationInfo[name]){            
            delete this.validationInfo[name];
            if (Object.keys(this.validationInfo).length === 0){
                this.validationInfo = {};
            }
        }        
    }
    
    @computed
    public get validationMessages(): string[]{        
        if (this.validationInfo){
            let messages: string[] = [];
            Object.keys(this.validationInfo).forEach((key) => {
                messages = messages.concat(this.validationInfo[key]);
              });

              console.log(messages)

            return messages;
        }
        

        return [];
    }
    

    @computed
    public get hasAnyValidationInfo(){
        if(this.validationInfo){
            let notHaveInfo = Object.keys(this.validationInfo).length === 0 && this.validationInfo.constructor === Object
            return !notHaveInfo;
        }

        return false;
    }

    public hasFieldValidationError(name: string){
        if (this.validationInfo && this.validationInfo[name]){
            return true;
        }

        return false;
    }
}