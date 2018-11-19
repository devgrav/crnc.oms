import * as React from "react";
import { Redirect } from "react-router";
import { Link } from "react-router-dom";
import { Button, Card, Divider, Form, Popup, Segment, Confirm } from "semantic-ui-react";
import { UserItemDto, UserSearchDto, UserService } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";
import UserSearch from "./UserSearch";
import Paging from "../shared/paging/Paging";
import { Guid } from "guid-typescript";

export default class UserCards extends React.Component<any, UserCardsState>{

    private readonly itemsPerPage: number = 8;

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            activePage: 1,
            isLoading: false,
            isRequiredRedirectToNotFound: false,
            isDeleteConfirmOpen: false,
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
        this.handleSave = this.handleSave.bind(this);
        this.handleOpenDeleteConfirm = this.handleOpenDeleteConfirm.bind(this);
        this.handleCloseDeleteConfirm = this.handleCloseDeleteConfirm.bind(this);
        this.handleDeleteConfirmed = this.handleDeleteConfirmed.bind(this);
        this.handleSearch = this.handleSearch.bind(this);
        this.handleClearSearch = this.handleClearSearch.bind(this);
        this.handleSearchChange = this.handleSearchChange.bind(this);
        this.handlePageChange = this.handlePageChange.bind(this);
    }

    private handleSearch(): void{
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

    private async handleClearSearch(): Promise<void>{
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

    private handleSearchChange(event: React.SyntheticEvent<HTMLElement>, data: any){
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
                id: Guid.EMPTY,
                isActive: true
            };
        }

        if (Guid.isGuid(idString)){
            const id = Guid.parse(idString);
            return users.find((u) => u.id === id.toString());
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

    private async DeleteUser(id: string): Promise<void>{
        await UserService.deleteUser(id);            
    }

    private async handleSave(): Promise<void> {
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

    private getUsersPerPage(users: UserItemDto[], pageNumber: number){
          
        let pagedArray = users.slice((pageNumber - 1) * this.itemsPerPage, pageNumber * this.itemsPerPage);     
        
        return pagedArray;
    }

    private getTotalPages(users: UserItemDto[]){
        if(users.length <= this.itemsPerPage)
            return 1;
            
        const totalPages =  Math.ceil(users.length / this.itemsPerPage); 
        
        return totalPages;
    }    

    private handlePageChange(activePage: number){
        this.setState({
            activePage
        });
    }

    private handleOpenDeleteConfirm(deletedUser: UserItemDto){
        this.setState({
            isDeleteConfirmOpen: true,
            deletedUser
        })
    }

    private handleCloseDeleteConfirm(){
        this.setState({
            isDeleteConfirmOpen: false,
            deletedUser: undefined
        })
    }

    private async handleDeleteConfirmed(): Promise<void>{
        this.setState({
            isDeleteConfirmOpen: false
        })

        const {deletedUser} = this.state;
        
        if(deletedUser){
            await this.DeleteUser(deletedUser.id)
            const users = await this.getUsers();
    
            this.setState({
                users
            })

            this.setState({
                deletedUser: undefined
            })
        }
    }

    public render(){
        if (this.state.isRequiredRedirectToNotFound){
            return <Redirect to="/404"/>;
        }

        const deletedUser = this.state.deletedUser ? this.state.deletedUser.fullName : "";

        return (
            <div>
                <Segment loading={this.state.isLoading} basic>
                    <Confirm 
                        open={this.state.isDeleteConfirmOpen} 
                        onCancel={this.handleCloseDeleteConfirm} 
                        onConfirm={this.handleDeleteConfirmed}
                        header={`Delete of user ${deletedUser}`} 
                    />
                    <Paging 
                        primary 
                        floated="right" 
                        vertical 
                        totalPages={this.getTotalPages(this.state.users)} 
                        onPageChange={this.handlePageChange}
                        activePage={this.state.activePage}
                    />
                    <Button.Group size="big" floated="right" vertical>
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
                                    onChange={this.handleSearchChange}
                                    onSearch={this.handleSearch}
                                    onClear={this.handleClearSearch}
                                    search={this.state.search}
                                />}
                            on="click"
                            position="bottom right"
                        />
                    </Button.Group>
                    <Card.Group>
                        {this.getUsersPerPage(this.state.users,this.state.activePage).map((u) => {
                            return <UserCardView key={u.id.toString()} userItem={u} onUserDelete={this.handleOpenDeleteConfirm}/>;
                        })}
                    </Card.Group>
                    {this.state.editedUser &&
                        <UserCardEdit
                            user={this.state.editedUser}
                            onCancelEdit={this.onCancelEdit}
                            onSaved={this.handleSave}
                        />}
                </Segment>
            </div>
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    activePage: number;
    search: UserSearchDto;
    editedUser?: UserItemDto;
    deletedUser?: UserItemDto;
    isLoading: boolean;
    isDeleteConfirmOpen: boolean;
    isRequiredRedirectToNotFound: boolean;
}
