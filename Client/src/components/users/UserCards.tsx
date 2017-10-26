import * as React from "react";
import { Button, Card, Divider, Form, Segment } from "semantic-ui-react";
import { UserItemDto, UserService } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";

export default class UserCards extends React.Component<any, UserCardsState>{

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            isLoading: false
        };

        this.showLoading = this.showLoading.bind(this);
        this.hideLoading = this.hideLoading.bind(this);
        this.onCancelEdit = this.onCancelEdit.bind(this);
        this.onCardEdit = this.onCardEdit.bind(this);
    }

    private async getUsers(): Promise<void>{
        this.showLoading();

        const users = await UserService.getUsersGrid();

        this.setState({
            ...this.state, users
        }, () => this.getEditedUserFromRouter());

        this.hideLoading();
    }

    private getEditedUserFromRouter(): void{
        const id = Number(this.props.match.params.id);
        if (id && !isNaN(id)){
            const user = this.state.users.find((u) => u.id === id);
            if (user){
                this.setState({
                    ...this.state, editedUser: user
                });
            }
        }
    }

    public componentDidMount(): void{
        this.getUsers();
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

    private onCardEdit(user: UserItemDto){
        this.setState({
            ...this.state, editedUser: user
        });
    }

    private onCancelEdit(){
        this.setState({
            ...this.state, editedUser: undefined
        });
    }

    public render(){
        return (
            <Segment loading={this.state.isLoading} basic>
                <Card.Group>
                    {this.state.users.map((u) => {
                        return <UserCardView key={u.id} userItem={u} onCardEdit={this.onCardEdit}/>;
                    })}
                </Card.Group>
                {this.state.editedUser &&
                    <UserCardEdit
                        userItem={this.state.editedUser}
                        onCancelEdit={this.onCancelEdit}
                    />}
            </Segment>
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    editedUser?: UserItemDto;
    isLoading: boolean;
}
