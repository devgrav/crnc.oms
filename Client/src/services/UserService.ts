import axios from "axios";
import APP_CONFIG from "../config";

export class UserService{

    public static getUsersGrid(): Promise<UserGridItem[]>{
        return axios.get(APP_CONFIG.usersUrl)
            .then((response) => {
                return response.data;
            });
    }
}

export interface UserGridItem{
    id: number;
    fullName: string;
    login: string;
    email: string;
    phone: string;
    isActive: boolean;
}
