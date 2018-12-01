import CurrentUser from "../auth/CurrentUser";
import CurrentUserContext from "../auth/CurrentUserContext";

export default class AuthService {

    static signIn(): CurrentUser{
        return CurrentUserContext;
    }

    static signOut(){
        
    }

    static isAuthentificated(){

    }
}