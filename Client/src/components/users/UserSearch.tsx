import * as React from "react";
import { Button, ButtonProps, DropdownProps, Form, FormProps, Icon, InputOnChangeData, Modal, Segment } from "semantic-ui-react";
import {RoleService} from "../../services/RoleService";
import { UserSearchDto } from "../../services/UserService";
import TextValueDto from "../shared/TextValueDto";

export default class UserSearch
    extends React.Component<UserSearchProps, UserSearchState>{

    constructor(props: any){
        super(props);

        this.state = {
            roles: [
                {
                    value: 0,
                    text: "Not chosen"
                }
            ],
            isLoading: false
        };

        this.onSearch = this.onSearch.bind(this);
        this.onClear = this.onClear.bind(this);
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

    private onSearch(event: React.FormEvent<HTMLElement>, data: FormProps){
        this.props.onSearch();
    }

    private onClear(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps){
        this.props.onClear();
    }

    public componentDidMount(){
        this.GetRoles();
    }

    public render(){
        return (
            <Form id="searchForm" onSubmit={this.onSearch}>
                <Form.Input
                    value={this.props.search.fullName}
                    onChange={this.props.onChange}
                    label="Full name"
                    name="fullName"
                    autoComplete="off"
                />
                <Form.Input
                    value={this.props.search.login}
                    onChange={this.props.onChange}
                    label="Login"
                    name="login"
                    autoComplete="off"
                />
                <Form.Select
                    value={this.props.search.role}
                    options={this.state.roles}
                    loading={this.state.isLoading}
                    onChange={this.props.onChange}
                    label="Role"
                    name="role"
                />
                <Form.Checkbox
                    checked={this.props.search.isActive}
                    onChange={this.props.onChange}
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
                    onClick={this.onClear}
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
    search: UserSearchDto;
    onSearch(): void;
    onClear(): void;
    onChange(event: React.SyntheticEvent<HTMLElement>, data: any): void;
}

interface UserSearchState{
    roles: TextValueDto[];
    isLoading: boolean;
}
