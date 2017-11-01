import axios from "axios";
import APP_CONFIG from "../config";

export class UserService{

    public static getUsersGrid(): Promise<UserItemDto[]>{
        return axios.get(APP_CONFIG.usersUrl)
            .then((response) => {
                return response.data;
            });
    }

    public static postUser(user: UserItemDto): Promise<void>{
        return axios.post(APP_CONFIG.usersUrl, user)
            .then((response) => {
                return;
            });
    }

    public static putUser(user: UserItemDto): Promise<void>{
        return axios.put(`${APP_CONFIG.usersUrl}/${user.id}`, {user})
            .then((response) => {
                return;
            });
    }

    public static deleteUser(id: number): Promise<void>{
        return axios.delete(APP_CONFIG.usersUrl, {data: {id}})
            .then((response) => {
                return;
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
