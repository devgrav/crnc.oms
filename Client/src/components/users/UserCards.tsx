import * as React from "react";
import { Redirect } from "react-router";
import { Button, Card, Divider, Form, Segment } from "semantic-ui-react";
import { UserItemDto, UserService } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";
import { Link } from "react-router-dom";

export default class UserCards extends React.Component<any, UserCardsState>{

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            isLoading: false,
            isRequiredRedirectToNotFound: false
        };

        this.showLoading = this.showLoading.bind(this);
        this.hideLoading = this.hideLoading.bind(this);
        this.onCancelEdit = this.onCancelEdit.bind(this);
        this.onSaved = this.onSaved.bind(this);
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
            <Segment loading={this.state.isLoading} basic>
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
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    editedUser?: UserItemDto;
    isLoading: boolean;
    isRequiredRedirectToNotFound: boolean;
}
