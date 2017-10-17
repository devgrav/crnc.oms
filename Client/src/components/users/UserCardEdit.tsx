import * as React from "react";
import { Button, Card, Divider, Form, Grid, Image, Modal, ModalProps } from "semantic-ui-react";
import { UserItemDto } from "../../services/UserService";

export default class UserCardEdit extends React.Component<UserCardEditProps>{

    constructor(props: UserCardEditProps){
        super(props);

        this.onClose = this.onClose.bind(this);
    }

    private onClose(event: React.MouseEvent<HTMLElement>, data: ModalProps){
        this.props.onCancelEdit();
    }

    public render(){
        return (
            <Modal open={true} closeIcon onClose={this.onClose}>
                <Modal.Header>Edit user</Modal.Header>
                <Modal.Content image>
                    <Form className="ui form">
                        <Grid columns={2}>
                            <Grid.Column>
                                <Image
                                    size="medium"
                                    src={`data:${this.props.userItem.photoMimeType};base64,
                                        ${this.props.userItem.photoBase64}`}
                                />
                            </Grid.Column>
                            <Grid.Column>
                                <Form.Group>
                                    <Form.Input label="Login" value={this.props.userItem.login}/>
                                    <Form.Input label="Password" type="password" value={this.props.userItem.password}/>
                                </Form.Group>
                                <Form.Group>
                                    <Form.Input label="First name" value={this.props.userItem.firstName}/>
                                    <Form.Input label="Last name" value={this.props.userItem.lastName}/>
                                </Form.Group>
                                <Form.Group>
                                    <Form.Input label="Email" type="email" value={this.props.userItem.email}/>
                                    <Form.Input label="Phone" value={this.props.userItem.phone || ""}/>
                                </Form.Group>
                                <Form.Checkbox label="Active" checked={this.props.userItem.isActive}/>
                            </Grid.Column>
                        </Grid>
                    </Form>
                </Modal.Content>
                <Modal.Actions>
                    <Button basic color="green" type="submit" content="Save"/>
                    <Button basic color="red" type="reset" content="Cancel"/>
                </Modal.Actions>
            </Modal>
        );
    }
}

interface UserCardEditProps{
    userItem: UserItemDto;
    onCancelEdit(): void;
}
