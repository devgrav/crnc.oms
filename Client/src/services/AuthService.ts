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
            CurrentUserContext.init(response.data);
            return;
        })
    }

    static signOut(){
        CurrentUserContext.clear();
    }
}