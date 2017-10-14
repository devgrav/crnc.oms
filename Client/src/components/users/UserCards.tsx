import * as React from "react";
import { Button, Card, Divider, Form, Segment } from "semantic-ui-react";
import * as avatarJohn from "../../assets/images/man1.jpg";
import * as avatarShon from "../../assets/images/man2.jpg";
import * as avatarHelen from "../../assets/images/woman1.jpg";
import * as avatarAgness from "../../assets/images/woman2.jpg";
import { UserItemDto, UserService } from "../../services/UserService";
import UserCard from "./UserCard";
import UserCardEdit from "./UserCardEdit";

export default class UserCards extends React.Component<any, UserCardsState>{

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            isLoading: false
        };

        this.onEdit = this.onEdit.bind(this);
    }

    private getUsers(): void{
        this.showLoading();

        UserService.getUsersGrid()
        .then((users) => {
            this.setState({
                ...this.state, users
            });
            this.hideLoading();
        });
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

    private onEdit(userId: number){
        console.log(this.state.users);
    }

    public render(){
        return (
            <Segment loading={this.state.isLoading} basic>
                <Card.Group>
                    {this.state.users.map((u) => {
                        return <UserCard key={u.id} userItem={u} onEdit={this.onEdit}/>;
                    })}
                </Card.Group>
            </Segment>
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    isLoading: boolean;
}
