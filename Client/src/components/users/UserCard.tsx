import * as React from "react";
import { Button, ButtonProps, Card, Divider, Form, Modal } from "semantic-ui-react";
import { UserItemDto } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";

export default class UserCard extends React.Component<UserCardProps, UserCardState>{

    constructor(props: UserCardProps){
        super(props);

        this.state = {
            isEditing: false
        };

        this.onCardEdit = this.onCardEdit.bind(this);
        this.onCancelEdit = this.onCancelEdit.bind(this);
    }

    private onCardEdit(){
        this.setState({
            isEditing: true
        });
    }

    private onCancelEdit(){
        this.setState({
            isEditing: false
        });
    }

    public render(){
        if (this.state.isEditing){
            return (
                <UserCardEdit
                    userItem={this.props.userItem}
                    onCancelEdit={this.onCancelEdit}
                />
            );
        }

        return (<UserCardView userItem={this.props.userItem} onCardEdit={this.onCardEdit}/>);
    }
}

interface UserCardProps{
    userItem: UserItemDto;
}

interface UserCardState{
    isEditing: boolean;
}
