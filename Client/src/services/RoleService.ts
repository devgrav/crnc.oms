import axios from "axios";
import TextValueDto from "../components/shared/TextValueDto";
import APP_CONFIG from "../config";

export class RoleService{

    public static GetRoles(): Promise<TextValueDto[]>{
        return axios.get(APP_CONFIG.rolesUrl)
            .then((result) => {
                return result.data;
            });
    }
}
