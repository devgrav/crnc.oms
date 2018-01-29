import * as React from "react";
import { Button, DropdownProps, Form, Icon, InputOnChangeData, Modal, Segment } from "semantic-ui-react";
import {RoleService} from "../../services/RoleService";
import { UserSearchDto } from "../../services/UserService";
import TextValueDto from "../shared/TextValueDto";

export default class UserSearch
    extends React.Component<UserSearchProps, UserSearchState>{

    constructor(props: any){
        super(props);

        this.state = {
            search: {
                fullName: "",
                login: "",
                role: 0,
                isActive: false
            },
            roles: [
                {
                    value: 0,
                    text: "Not chosen"
                }
            ],
            isLoading: false
        };

        this.onChange = this.onChange.bind(this);
    }

    private onChange(event: React.SyntheticEvent<HTMLElement>, data: any){
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;
        const search = {...this.state.search, ...{[name]: value}};

        this.setState({
            search
        });
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

    public render(){
        return (
            <Form id="searchForm">
                <Form.Input
                    value={this.state.search.fullName}
                    onChange={this.onChange}
                    label="Full name"
                    name="fullName"
                />
                <Form.Input
                    value={this.state.search.login}
                    onChange={this.onChange}
                    label="Login"
                    name="login"
                />
                <Form.Select
                    value={this.state.search.role}
                    options={this.state.roles}
                    loading={this.state.isLoading}
                    onChange={this.onChange}
                    label="Role"
                    name="role"
                />
                <Form.Checkbox
                    checked={this.state.search.isActive}
                    onChange={this.onChange}
                    name="isActive"
                    label="Active"
                />
                <Button
                    floated="right"
                    basic
                    color="red"
                    type="reset"
                    content="Clear"
                    icon="cancel"
                />
                <Button
                    floated="right"
                    basic
                    color="green"
                    type="submit"
                    content="Search"
                    icon="search"
                    form="searchForm"
                />
            </Form>
        );
    }
}

interface UserSearchProps{
    onSearch(search: UserSearchDto): void;
    onClear(): void;
}

interface UserSearchState{
    search: UserSearchDto;
    roles: TextValueDto[];
    isLoading: boolean;
}
