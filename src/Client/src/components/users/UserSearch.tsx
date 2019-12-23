import * as React from "react";
import { Button, ButtonProps, DropdownProps, Form, FormProps, Icon, InputOnChangeData, Modal, Segment } from "semantic-ui-react";
import {RoleService} from "../../services/RoleService";
import { UserSearchDto } from "../../services/UserService";
import TextValueDto from "../shared/TextValueDto";
import RoleSelect from "./RoleSelect";

export default class UserSearch
    extends React.Component<UserSearchProps>{

    constructor(props: any){
        super(props);

        this.onSearch = this.onSearch.bind(this);
        this.onClear = this.onClear.bind(this);
    }

    private onSearch(event: React.FormEvent<HTMLElement>, data: FormProps){
        this.props.onSearch();
    }

    private onClear(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps){
        this.props.onClear();
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
                <RoleSelect
                    selectedRoleId={this.props.search.role}                                        
                    onChange={this.props.onChange}      
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