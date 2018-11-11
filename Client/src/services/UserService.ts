import axios from "axios";
import APP_CONFIG from "../config";
import { Guid } from "guid-typescript";

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
        return axios.put(`${APP_CONFIG.usersUrl}/${user.id}`, user)
            .then((response) => {
                return;
            });
    }

    public static deleteUser(id: string): Promise<void>{
        return axios.delete(APP_CONFIG.usersUrl, {data: {id}})
            .then((response) => {
                return;
            });
    }
}

export interface UserItemDto{
    id: Guid;
    fullName?: string;
    firstName?: string;
    lastName?: string;
    login?: string;
    password?: string;
    email?: string;
    phone?: string;
    roleId?: number;
    role?: string;
    photoBase64?: string;
    photoMimeType?: string;
    isActive: boolean;
}

export interface UserSearchDto{
    fullName: string;
    login: string;
    role: number;
    isActive: boolean;
}
