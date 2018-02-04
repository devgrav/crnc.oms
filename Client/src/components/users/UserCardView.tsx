import * as React from "react";
import { Link } from "react-router-dom";
import { Button, ButtonProps, Card, Divider, Form, Icon, Image } from "semantic-ui-react";
import * as noAvatar from "../../assets/images/noavatar.png";
import { UserItemDto } from "../../services/UserService";

export default class UserCardView extends React.Component<UserCardViewProps>{

    constructor(props: UserCardViewProps){
        super(props);
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

    public render(){
        return (
            <Card color="blue">
                <Card.Content>
                    <Image
                        floated="left"
                        size="mini"
                        src={this.props.userItem.photoBase64 === null
                            ? noAvatar
                            : `data:${this.props.userItem.photoMimeType};base64,
                            ${this.props.userItem.photoBase64}`}
                    />
                    <div className="ui right floated ">
                        <Button
                            as={Link}
                            to={`/users/${this.props.userItem.id}`}
                            basic
                            icon="pencil"
                            content="Edit"
                            size="mini"
                        />
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
}
