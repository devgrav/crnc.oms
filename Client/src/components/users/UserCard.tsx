import * as React from "react";
import { Button, ButtonProps, Card, Divider, Form } from "semantic-ui-react";
import * as avatarJack from "../../assets/images/man1.jpg";
import * as avatarShon from "../../assets/images/man2.jpg";
import * as avatarHelen from "../../assets/images/woman1.jpg";
import * as avatarAgness from "../../assets/images/woman2.jpg";
import { UserItemDto } from "../../services/UserService";
import UserCardEdit from "./UserCardEdit";
import UserCardView from "./UserCardView";

const avatarsMap = [
    {
        userId: 1,
        avatar: avatarJack
    },
    {
        userId: 2,
        avatar: avatarShon
    },
    {
        userId: 3,
        avatar: avatarHelen
    },
    {
        userId: 4,
        avatar: avatarAgness
    }
];

export default class UserCard extends React.Component<UserCardProps>{

    constructor(props: UserCardProps){
        super(props);

        this.onEdit = this.onEdit.bind(this);
    }

    private onEdit(){
        this.props.onEdit(this.props.userItem.id);
    }

    public render(){
        if (this.props.userItem.isEdited){
            return (<UserCardEdit userItem={this.props.userItem}/>);
        }

        return (<UserCardView userItem={this.props.userItem} onCardEdit={this.onEdit}/>);
    }
}

interface UserCardProps{
    userItem: UserItemDto;
    onEdit(userId: number);
}
