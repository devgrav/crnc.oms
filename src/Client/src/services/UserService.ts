import axios from "axios";
import APP_CONFIG from "../config";
import { Guid } from "guid-typescript";
import AxiosProxy from "./AxiosProxy";

export class UserService{

    public static getUsersGrid(): Promise<UserItemDto[]>{
        return AxiosProxy.instance.get(APP_CONFIG.usersUrl)
            .then((response) => {
                return response.data;
            });
    }

    public static postUser(user: UserItemDto): Promise<void>{
        return AxiosProxy.instance.post(APP_CONFIG.usersUrl, user)
            .then((response) => {
                return;
            });
    }

    public static putUser(user: UserItemDto): Promise<void>{
        return AxiosProxy.instance.put(`${APP_CONFIG.usersUrl}/${user.id}`, user)
            .then((response) => {
                return;
            });
    }

    public static deleteUser(id: string): Promise<void>{
        return AxiosProxy.instance.delete(`${APP_CONFIG.usersUrl}/${id}`)
            .then((response) => {
                return;
            });
    }
}

export interface UserItemDto{
    id: string;
    fullName?: string;
    firstName?: string;
    lastName?: string;
    login?: string;
    password?: string;
    email?: string;
    phone?: string;
    roleId?: string;
    role?: string;
    photoBase64?: string;
    photoMimeType?: string;
    isActive: boolean;
}

export interface UserSearchDto{
    fullName: string;
    login: string;
    role: string;
    isActive: boolean;
}
