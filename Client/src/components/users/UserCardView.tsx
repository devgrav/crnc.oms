import * as React from "react";
import { Button, Card, Divider, Form, Image, Icon, ButtonProps } from "semantic-ui-react";
import * as avatarJack from "../../assets/images/man1.jpg";
import * as avatarShon from "../../assets/images/man2.jpg";
import * as avatarHelen from "../../assets/images/woman1.jpg";
import * as avatarAgness from "../../assets/images/woman2.jpg";
import { UserItemDto } from "../../services/UserService";

export default class UserCardView extends React.Component<UserCardViewProps>{

    constructor(props: UserCardViewProps){
        super(props);

        this.onEditClick = this.onEditClick.bind(this);
    }

    private renderCardInfo(){
        return (
            <Form>
                <Form.Field inline>
                    <label>Login:</label>
                    <p>{this.props.userItem.login}</p>
                </Form.Field>
                <Form.Field inline>
                    <label>Email:</label>
                    <p>{this.props.userItem.email}</p>
                </Form.Field>
                <Form.Field inline>
                    <label>Phone:</label>
                    <p>{this.props.userItem.phone || ""}</p>
                </Form.Field>
                <Form.Checkbox label="Active" disabled checked={this.props.userItem.isActive}/>
            </Form>
        );
    }

    private onEditClick(event: React.MouseEvent<HTMLButtonElement>, data: ButtonProps){
        this.props.onCardEdit();
    }

    public render(){
        return (
            <Card>
                <Card.Content>
                    <Image
                        floated="left"
                        size="mini"
                        src={`data:${this.props.userItem.photoMimeType};base64, ${this.props.userItem.photoBase64}`}
                    />
                    <div className="ui right floated ">
                        <Button basic icon="pencil" content="Edit" size="mini" onClick={this.onEditClick}/>
                    </div>
                    <Card.Header>
                        {this.props.userItem.fullName}
                    </Card.Header>
                    <Card.Meta>
                        {this.props.userItem.role}
                    </Card.Meta>
                </Card.Content>
                <Card.Content extra>
                    {this.renderCardInfo()}
                </Card.Content>
            </Card>
        );
    }
}

interface UserCardViewProps{
    userItem: UserItemDto;
    onCardEdit(): void;
}
