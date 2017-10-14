import * as React from "react";
import { Button, Card, Divider, Form, Segment } from "semantic-ui-react";
import * as avatarJohn from "../../assets/images/man1.jpg";
import * as avatarShon from "../../assets/images/man2.jpg";
import * as avatarHelen from "../../assets/images/woman1.jpg";
import * as avatarAgness from "../../assets/images/woman2.jpg";
import { UserItemDto, UserService } from "../../services/UserService";
import UserCard from "./UserCard";

export default class UserCards extends React.Component<any, UserCardsState>{

    constructor(props: any){
        super(props);

        this.state = {
            users: [],
            isLoading: false
        };
    }

    private async getUsers(): Promise<void>{
        this.showLoading();

        const users = await UserService.getUsersGrid();

        this.setState({
            users
        });

        this.hideLoading();
    }

    public componentDidMount(): void{
        this.getUsers();
    }

    public showLoading(){
        this.setState({
            ...this.state, isLoading: true
        });
    }

    public hideLoading(){
        this.setState({
            ...this.state, isLoading: false
        });
    }

    public render(){
        return (
            <Segment loading={this.state.isLoading} color="blue">
                <Card.Group>
                    {this.state.users.map((u) =>
                        <UserCard key={u.id} userItem={u}/>)}
                </Card.Group>
            </Segment>
        );
    }
}

interface UserCardsState{
    users: UserItemDto[];
    isLoading: boolean;
}
