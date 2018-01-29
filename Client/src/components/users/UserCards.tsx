import * as React from "react";
import { Redirect } from "react-router";
import { Link } from "react-router-dom";
import { Button, Card, Divider, Form, Popup, Segment } from "semantic-ui-react";
import { UserItemDto, UserSearchDto, UserService } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";
import UserSearch from "./UserSearch";

export default class UserCards extends React.Component<any, UserCardsState>{

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            isLoading: false,
            isRequiredRedirectToNotFound: false,
            search: {
                fullName: "",
                login: "",
                role: 0,
                isActive: true
            }
        };

        this.showLoading = this.showLoading.bind(this);
        this.hideLoading = this.hideLoading.bind(this);
        this.onCancelEdit = this.onCancelEdit.bind(this);
        this.onSaved = this.onSaved.bind(this);
        this.onSearch = this.onSearch.bind(this);
        this.onClearSearch = this.onClearSearch.bind(this);
        this.onSearchChange = this.onSearchChange.bind(this);
    }

    private onSearch(): void{
        let users = this.state.users.slice();
        if (this.state.search.fullName) {
            users = users.filter((u) => u.fullName ?
                u.fullName.toLowerCase().includes(this.state.search.fullName.toLowerCase())
                : true);
        }
        if (this.state.search.login) {
            users = users.filter((u) => u.login ?
                u.login.toLowerCase().includes(this.state.search.login.toLowerCase())
                : true);
        }
        if (this.state.search.role) {
            users = users.filter((u) =>
                u.roleId === this.state.search.role);
        }

        users = users.filter((u) => u.isActive === this.state.search.isActive);

        this.setState({
            users
        });
    }

    private async onClearSearch(): Promise<void>{
        const users = await this.getUsers();
        this.setState({
            search: {
                fullName: "",
                login: "",
                role: 0,
                isActive: true
            },
            users
        });
    }

    private onSearchChange(event: React.SyntheticEvent<HTMLElement>, data: any){
        const name = data.name;
        const value = data.type === "checkbox" ? data.checked : data.value;
        const search = {...this.state.search, ...{[name]: value}};

        this.setState({
            search
        });
    }

    private async getUsers(): Promise<UserItemDto[]>{
        try{
            this.showLoading();
            const users = await UserService.getUsersGrid();
            this.hideLoading();

            return users;
        }catch (error) {
            this.hideLoading();
            return Promise.reject(error);
        }
    }

    private getEditedUserByRouteId(users: UserItemDto[], idString: string): UserItemDto | undefined{
        if (idString === "new"){
            return {
                id: 0,
                isActive: true
            };
        }

        const id = Number(idString);
        if (id && !isNaN(id)){
            return users.find((u) => u.id === id);
        }
        if (isNaN(id)){
            return undefined;
        }

        return undefined;
    }

    private handleEditedUserByRoute(props: any, users: UserItemDto[]): void{
        if (props.match.params.id){
            const user = this.getEditedUserByRouteId(users, props.match.params.id);
            if (user){
                this.setState({
                    ...this.state, editedUser: user
                });
            }
            else{
                this.setState({
                    ...this.state, isRequiredRedirectToNotFound: true
                });
            }
        }
        else{
            this.setState({
                ...this.state, editedUser: undefined
            });
        }
    }

    private showLoading(): void{
        this.setState({
            ...this.state, isLoading: true
        });
    }

    private hideLoading(): void{
        this.setState({
            ...this.state, isLoading: false
        });
    }

    private onCancelEdit(): void{
        this.setState({
            editedUser: undefined
        });

        this.props.history.push("/users");
    }

    private async onSaved(): Promise<void> {
        const users = await this.getUsers();
        this.setState({
            users
        });
    }

    public async componentDidMount(): Promise<void>{
        const users = await this.getUsers();
        this.setState({
            ...this.state, users
        });
        this.handleEditedUserByRoute(this.props, users);
    }

    public componentWillReceiveProps(nextProps: any): void{
        this.handleEditedUserByRoute(nextProps, this.state.users);
    }

    public render(){
        if (this.state.isRequiredRedirectToNotFound){
            return <Redirect to="/404"/>;
        }

        return (
            <div>
                <Segment loading={this.state.isLoading} basic>
                    <Button.Group floated="right" vertical>
                        <Button
                            as={Link}
                            to="/users/new"
                            icon="plus"
                            title="Add new user"
                            primary
                            attached="left"
                        />

                        <Popup
                            trigger={
                                <Button
                                    icon="search"
                                    title="Search of user"
                                    primary
                                    attached="left"
                                />
                            }
                            content={
                                <UserSearch
                                    onChange={this.onSearchChange}
                                    onSearch={this.onSearch}
                                    onClear={this.onClearSearch}
                                    search={this.state.search}
                                />}
                            on="click"
                            position="bottom right"
                        />
                    </Button.Group>
                    <Card.Group>
                        {this.state.users.map((u) => {
                            return <UserCardView key={u.id} userItem={u}/>;
                        })}
                    </Card.Group>
                    {this.state.editedUser &&
                        <UserCardEdit
                            user={this.state.editedUser}
                            onCancelEdit={this.onCancelEdit}
                            onSaved={this.onSaved}
                        />}
                </Segment>
            </div>
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    search: UserSearchDto;
    editedUser?: UserItemDto;
    isLoading: boolean;
    isRequiredRedirectToNotFound: boolean;
}
