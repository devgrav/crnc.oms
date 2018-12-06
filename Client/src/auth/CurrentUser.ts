import {CurrentUserRole} from "./CurrentUserRole";

export default interface CurrentUser{
    login: string;
    fullName: string;
    role: CurrentUserRole;
    jwt: string;
}