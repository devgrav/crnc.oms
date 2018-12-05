import CurrentUser from "../auth/CurrentUser";
import CurrentUserContext from "../auth/CurrentUserContext";
import APP_CONFIG from "../config";
import axios from "axios";

export default class AuthService {

    static signIn(login: string, password: string): Promise<void>{              
        return axios.post(`${APP_CONFIG.accountsUrl}/auth`, {
            login: login,
            password: password
        })
        .then((response) => {
            const user = response.data as CurrentUser;
            sessionStorage.setItem("crnc.oms.currentUser", JSON.stringify(user))
            CurrentUserContext.init(user);
            return;
        })
    }

    static signOut(){
        sessionStorage.removeItem("crnc.oms.currentUser");
        CurrentUserContext.clear();
    }
}