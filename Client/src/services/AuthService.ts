import CurrentUser from "../auth/CurrentUser";
import CurrentUserContext from "../auth/CurrentUserContext";

export default class AuthService {

    static signIn(login: string, password: string): CurrentUser{        
        let user = new CurrentUser(login, "Some name", "admin");
        CurrentUserContext.init(user);
        return CurrentUserContext.user;
    }

    static signOut(){
        CurrentUserContext.clear();
    }
}