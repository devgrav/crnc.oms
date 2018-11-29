import CurrentUserRole from "./CurrentUserRole";

export default class CurrentUser{
    login: string;
    fullName: string;
    role: CurrentUserRole;

    constructor(login: string, fullName: string, role: CurrentUserRole){
        this.login = login;
        this.fullName = fullName;
        this.role = role;
    }
}