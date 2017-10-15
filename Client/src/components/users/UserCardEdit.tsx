import * as React from "react";
import { Button, Card, Divider, Form } from "semantic-ui-react";
import * as avatarJack from "../../assets/images/man1.jpg";
import * as avatarShon from "../../assets/images/man2.jpg";
import * as avatarHelen from "../../assets/images/woman1.jpg";
import * as avatarAgness from "../../assets/images/woman2.jpg";
import { UserItemDto } from "../../services/UserService";

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

export default class UserCardEdit extends React.Component<UserCardEditProps>{

    constructor(props: UserCardEditProps){
        super(props);
    }

    private renderForm(){
        return (
            <Form>
                <Form.Input placeholder="Login" value={this.props.userItem.login}/>
                <Form.Input type="password" placeholder="Password" value={this.props.userItem.password}/>
                <Form.Input placeholder="First name" value={this.props.userItem.firstName}/>
                <Form.Input placeholder="Last name" value={this.props.userItem.lastName}/>
                <Form.Input type="email" placeholder="Email" value={this.props.userItem.email}/>
                <Form.Input placeholder="Phone" value={this.props.userItem.phone || ""}/>
                <Form.Checkbox label="Activity" slider checked={this.props.userItem.isActive}/>
                <Divider/>
                <div className="ui right floated">
                    <Button basic color="green" type="submit" content="Save"/>
                    <Button basic color="red" type="reset" content="Cancel"/>
                </div>
            </Form>
        );
    }

    private getAvatar(userId: number){
        return avatarsMap.filter((p) => p.userId === userId)[0].avatar;
    }

    public render(){
        return (
            <Card
                image={this.getAvatar(this.props.userItem.id)}
                header={this.props.userItem.fullName}
                meta={this.props.userItem.role}
                extra={this.renderForm()}
            />
        );
    }
}

interface UserCardEditProps{
    userItem: UserItemDto;
}
