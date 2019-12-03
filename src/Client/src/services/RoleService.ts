import TextValueDto from "../components/shared/TextValueDto";
import APP_CONFIG from "../config";
import AxiosProxy from "./AxiosProxy";

export class RoleService{

    public static GetRoles(): Promise<TextValueDto[]>{
        return AxiosProxy.instance.get(APP_CONFIG.rolesUrl)
            .then((result) => {
                return result.data;
            });
    }
}
