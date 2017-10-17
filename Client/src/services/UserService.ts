import axios from "axios";
import APP_CONFIG from "../config";

export class UserService{

    public static getUsersGrid(): Promise<UserItemDto[]>{
        return axios.get(APP_CONFIG.usersUrl)
            .then((response) => {
                return response.data;
            });
    }
}

export interface UserItemDto{
    id: number;
    fullName: string;
    firstName: string;
    lastName: string;
    login: string;
    password: string;
    email: string;
    phone: string;
    role: string;
    photoBase64: string;
    photoMimeType: string;
    isActive: boolean;
}
