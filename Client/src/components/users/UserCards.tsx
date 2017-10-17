import * as React from "react";
import { Button, Card, Divider, Form, Segment } from "semantic-ui-react";
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
    }

    private async getUsers(): Promise<void>{
        this.showLoading();

        const users = await UserService.getUsersGrid();

        this.setState({
            ...this.state, users
        });

        this.hideLoading();
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

    public render(){
        return (
            <Segment loading={this.state.isLoading} basic>
                <Card.Group>
                    {this.state.users.map((u) => {
                        return <UserCard key={u.id} userItem={u}/>;
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
