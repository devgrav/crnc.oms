import * as React from 'react';
import { Form } from 'semantic-ui-react';
import TextValueDto from '../shared/TextValueDto';
import { RoleService } from '../../services/RoleService';
import { Guid } from 'guid-typescript';

export default class RoleSelect extends React.Component<RoleSelectProps, RoleSelectState> {
  
    constructor(props: RoleSelectProps) {
        super(props);

        this.state = {
            roles: [
                {
                    value: Guid.EMPTY,
                    text: "Not chosen"
                }
            ],
            isLoading: false
        };
        
    }

    
    private showLoader(){
        this.setState({
            isLoading: true
        });
    }

    private hideLoader(){
        this.setState({
            isLoading: false
        });
    }

    private async GetRoles(){
        this.showLoader();
        const roles = await RoleService.GetRoles();
        this.hideLoader();

        this.setState({
            roles: this.state.roles.concat(roles)
        });
    }

    public componentDidMount(){
        this.GetRoles();
    }

    public render() {
        const {onChange, name, error} = this.props;
        const {roles, isLoading} = this.state;

        let selectedRoleId = this.props.selectedRoleId;

        if(selectedRoleId === null || selectedRoleId === undefined)
            selectedRoleId = Guid.EMPTY

        return (
            <Form.Select
                value={selectedRoleId}
                options={roles}
                loading={isLoading}
                onChange={onChange}
                name={name}
                label="Role"
                error={error}
            />   
        );
    }
}

interface RoleSelectProps{
    selectedRoleId?: string;
    name: string;    
    onChange(event: React.SyntheticEvent<HTMLElement>, data: any): void;
    error?: boolean;
}

interface RoleSelectState{
    roles: TextValueDto[];
    isLoading: boolean;
}


